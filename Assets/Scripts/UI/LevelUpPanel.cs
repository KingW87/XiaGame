using UnityEngine;
using UnityEngine.UI;
using ClawSurvivor.Skills;

namespace ClawSurvivor.UI
{
    public class LevelUpPanel : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.1f, 0.1f, 0.15f, 0.92f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.85f, 0.2f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.3f, 0.35f, 0.5f, 0.95f);
        [Tooltip("面板宽度")]
        public float panelWidth = 700f;
        [Tooltip("面板高度")]
        public float panelHeight = 400f;
        [Tooltip("选项按钮高度")]
        public float optionHeight = 80f;
        [Tooltip("选项间距")]
        public float optionSpacing = 20f;

        [Header("面板位置偏移")]
        [Tooltip("面板中心偏移")]
        public Vector2 panelOffset = Vector2.zero;

        private Canvas parentCanvas;
        private RectTransform canvasRect;
        private GameObject uiRoot;
        private RectTransform panelRect;
        private Text[] choiceNames;
        private Text[] choiceDescriptions;
        private Text[] choiceTypes;
        private SkillCard[] currentChoices;
        private bool isShowing;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            canvasRect = parentCanvas.GetComponent<RectTransform>();

            CreatePanelUI();

            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                player.OnLevelUp += OnLevelUp;

            uiRoot.SetActive(false);
        }

        private void CreatePanelUI()
        {
            uiRoot = new GameObject("LevelUpUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform uiRootRT = uiRoot.AddComponent<RectTransform>();
            uiRootRT.anchorMin = Vector2.zero;
            uiRootRT.anchorMax = Vector2.one;
            uiRootRT.pivot = new Vector2(0.5f, 0.5f);
            uiRootRT.anchoredPosition = Vector2.zero;
            uiRootRT.sizeDelta = Vector2.zero;

            // 背景遮罩
            GameObject mask = new GameObject("Mask");
            mask.transform.SetParent(uiRoot.transform, false);
            RectTransform maskRT = mask.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.pivot = Vector2.zero;
            maskRT.anchoredPosition = Vector2.zero;
            maskRT.sizeDelta = Vector2.zero;
            maskRT.offsetMin = Vector2.zero;
            maskRT.offsetMax = Vector2.zero;
            Image maskImg = mask.AddComponent<Image>();
            maskImg.color = new Color(0, 0, 0, 0.6f);

            // 面板
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(uiRoot.transform, false);
            panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = panelOffset;
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = panelColor;

            // 标题
            CreateText(panel.transform, "Title", "升级！选择一个技能", new Vector2(0, -30), new Vector2(400, 50), 28, titleColor, TextAnchor.MiddleCenter, true);

            // 3个选项
            choiceNames = new Text[3];
            choiceDescriptions = new Text[3];
            choiceTypes = new Text[3];

            float startY = -80;
            for (int i = 0; i < 3; i++)
            {
                float y = startY - i * (optionHeight + optionSpacing);
                CreateOptionButton(panel.transform, i, y);
            }
        }

        private Text CreateText(Transform parent, string name, string text, Vector2 anchorPos, Vector2 size, int fontSize, Color color, TextAnchor align, bool bold)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchorPos;
            rt.sizeDelta = size;
            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            return txt;
        }

        private void CreateOptionButton(Transform parent, int index, float y)
        {
            GameObject btn = new GameObject($"Option_{index}");
            btn.transform.SetParent(parent);

            RectTransform btnRT = btn.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 1f);
            btnRT.anchorMax = new Vector2(0.5f, 1f);
            btnRT.pivot = new Vector2(0.5f, 1f);
            btnRT.anchoredPosition = new Vector2(0, y);
            btnRT.sizeDelta = new Vector2(panelWidth - 80, optionHeight);

            Image btnImg = btn.AddComponent<Image>();
            btnImg.color = buttonColor;
            btnImg.type = Image.Type.Sliced;

            Button button = btn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = buttonHoverColor;
            button.colors = colors;

            int idx = index;
            button.onClick.AddListener(() => SelectSkill(idx));

            // 技能名称
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(btn.transform);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0, 0.5f);
            nameRT.pivot = new Vector2(0, 0.5f);
            nameRT.anchoredPosition = new Vector2(30, 10);
            nameRT.sizeDelta = new Vector2(300, 35);
            choiceNames[index] = nameObj.AddComponent<Text>();
            choiceNames[index].fontSize = 20;
            choiceNames[index].color = Color.white;
            choiceNames[index].fontStyle = FontStyle.Bold;
            choiceNames[index].alignment = TextAnchor.MiddleLeft;

            // 技能类型标签
            GameObject typeObj = new GameObject("Type");
            typeObj.transform.SetParent(btn.transform);
            RectTransform typeRT = typeObj.AddComponent<RectTransform>();
            typeRT.anchorMin = new Vector2(1, 0.5f);
            typeRT.anchorMax = new Vector2(1, 0.5f);
            typeRT.pivot = new Vector2(1, 0.5f);
            typeRT.anchoredPosition = new Vector2(-30, 10);
            typeRT.sizeDelta = new Vector2(100, 30);
            choiceTypes[index] = typeObj.AddComponent<Text>();
            choiceTypes[index].fontSize = 16;
            choiceTypes[index].alignment = TextAnchor.MiddleRight;

            // 技能描述
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(btn.transform);
            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.5f);
            descRT.anchorMax = new Vector2(0, 0.5f);
            descRT.pivot = new Vector2(0, 0.5f);
            descRT.anchoredPosition = new Vector2(30, -18);
            descRT.sizeDelta = new Vector2(panelWidth - 160, 25);
            choiceDescriptions[index] = descObj.AddComponent<Text>();
            choiceDescriptions[index].fontSize = 14;
            choiceDescriptions[index].color = new Color(0.8f, 0.8f, 0.8f);
            choiceDescriptions[index].alignment = TextAnchor.MiddleLeft;
        }

        private void OnLevelUp(int newLevel)
        {
            ShowLevelUpChoices();
        }

        private void ShowLevelUpChoices()
        {
            if (isShowing) return;

            currentChoices = SkillDatabase.Instance.GetRandomSkills(3);

            for (int i = 0; i < 3; i++)
            {
                if (i < currentChoices.Length && currentChoices[i] != null)
                {
                    choiceNames[i].text = currentChoices[i].skillName;
                    choiceDescriptions[i].text = currentChoices[i].description;

                    switch (currentChoices[i].type)
                    {
                        case SkillType.Damage:
                            choiceTypes[i].text = "[伤害]";
                            choiceTypes[i].color = Color.red;
                            break;
                        case SkillType.Control:
                            choiceTypes[i].text = "[控制]";
                            choiceTypes[i].color = Color.cyan;
                            break;
                        case SkillType.Support:
                            choiceTypes[i].text = "[辅助]";
                            choiceTypes[i].color = Color.green;
                            break;
                    }
                }
            }

            uiRoot.SetActive(true);
            Time.timeScale = 0;
            isShowing = true;
        }

        private void SelectSkill(int index)
        {
            if (index >= currentChoices.Length || currentChoices[index] == null) return;

            var player = FindObjectOfType<Player.PlayerController>();
            player.EquipSkill(currentChoices[index]);

            ClosePanel();
        }

        private void ClosePanel()
        {
            uiRoot.SetActive(false);
            Time.timeScale = 1;
            isShowing = false;
        }
    }
}
