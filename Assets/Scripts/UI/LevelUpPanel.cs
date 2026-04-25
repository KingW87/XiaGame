using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawSurvivor.Skills;
using System.Collections;
using System;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 技能选择面板 - 可作为预制体使用
    /// 使用方式：
    /// 1. 将此脚本挂载到Canvas上的空对象，保存为预制体
    /// 2. 调用 LevelUpPanel.Show(skills) 显示面板
    /// 3. 监听 OnSkillSelected 事件获取选择结果
    /// </summary>
    public class LevelUpPanel : MonoBehaviour
    {
        [Header("面板设置")]
        public Color panelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        public Color titleColor = new Color(1f, 0.85f, 0.2f);
        public Color buttonColor = new Color(0.15f, 0.15f, 0.25f, 0.95f);
        public Color buttonHoverColor = new Color(0.3f, 0.3f, 0.5f, 0.95f);
        public Color buttonSelectedColor = new Color(0.4f, 0.4f, 0.6f, 1f);

        [Header("布局设置")]
        public float panelWidth = 900f;
        public float panelHeight = 350f;
        public float cardWidth = 250f;
        public float cardHeight = 300f;
        public float cardSpacing = 30f;

        [Header("技能图标")]
        public Sprite[] skillIcons;

        // 事件：技能被选中时触发 (选中的技能索引, 技能数据)
        public static event Action<int, SkillCard> OnSkillSelected;

        // 内部数据
        private GameObject[] optionCards;
        private Image[] optionIcons;
        private Text[] optionNames;
        private Text[] optionDescs;
        private Text[] optionTypes;
        private SkillCard[] currentChoices;
        private int selectedIndex = -1;

        private void Awake()
        {
            CreatePanelUI();
            gameObject.SetActive(false);

            // 监听技能选择事件，自动装备给玩家
            OnSkillSelected += OnSkillChosen;
        }

        private void OnDestroy()
        {
            OnSkillSelected -= OnSkillChosen;
        }

        private void OnSkillChosen(int index, SkillCard skill)
        {
            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                player.EquipSkill(skill);
            }
        }

        /// <summary>
        /// 显示技能选择面板（静态方法，自动创建或显示现有面板）
        /// </summary>
        /// <param name="skills">可选择的技能数组（通常为3个）</param>
        public static void Show(SkillCard[] skills)
        {
            // 尝试获取现有面板
            LevelUpPanel panel = FindObjectOfType<LevelUpPanel>();
            
            if (panel == null)
            {
                // 如果没有找到，创建一个新的Canvas和面板
                Canvas canvas = CreateUIRoot();
                panel = CreatePanel(canvas).GetComponent<LevelUpPanel>();
            }

            panel.SetSkills(skills);
            panel.gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public static void Hide()
        {
            LevelUpPanel panel = FindObjectOfType<LevelUpPanel>();
            if (panel != null)
            {
                panel.gameObject.SetActive(false);
                Time.timeScale = 1;
            }
        }

        private void SetSkills(SkillCard[] skills)
        {
            currentChoices = skills;
            selectedIndex = -1;

            for (int i = 0; i < 3; i++)
            {
                if (i < skills.Length && skills[i] != null)
                {
                    SkillCard skill = skills[i];
                    optionNames[i].text = skill.skillName;
                    optionDescs[i].text = skill.description;

                    // 设置技能图标
                    int iconIndex = GetSkillIconIndex(skill.skillName);
                    if (skillIcons != null && iconIndex >= 0 && iconIndex < skillIcons.Length && skillIcons[iconIndex] != null)
                    {
                        optionIcons[i].sprite = skillIcons[iconIndex];
                        optionIcons[i].enabled = true;
                    }
                    else
                    {
                        optionIcons[i].enabled = false;
                    }

                    // 设置类型标签
                    switch (skill.type)
                    {
                        case SkillType.Damage:
                            optionTypes[i].text = "伤害";
                            optionTypes[i].color = Color.red;
                            break;
                        case SkillType.Control:
                            optionTypes[i].text = "控制";
                            optionTypes[i].color = Color.cyan;
                            break;
                        case SkillType.Support:
                            optionTypes[i].text = "辅助";
                            optionTypes[i].color = Color.green;
                            break;
                    }

                    optionCards[i].SetActive(true);
                }
                else
                {
                    optionCards[i].SetActive(false);
                }
            }
        }

        private void CreatePanelUI()
        {
            // 主面板
            RectTransform panelRT = gameObject.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = Vector2.zero;

            // 确保有CanvasGroup用于管理可见性
            CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            // 背景遮罩
            GameObject mask = new GameObject("Mask");
            mask.transform.SetParent(transform, false);
            RectTransform maskRT = mask.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.pivot = Vector2.zero;
            maskRT.anchoredPosition = Vector2.zero;
            maskRT.sizeDelta = Vector2.zero;
            Image maskImg = mask.AddComponent<Image>();
            maskImg.color = new Color(0, 0, 0, 0.7f);

            // 技能面板
            GameObject skillPanel = new GameObject("SkillPanel");
            skillPanel.transform.SetParent(transform, false);
            RectTransform skillPanelRT = skillPanel.AddComponent<RectTransform>();
            skillPanelRT.anchorMin = new Vector2(0.5f, 0.5f);
            skillPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
            skillPanelRT.pivot = new Vector2(0.5f, 0.5f);
            skillPanelRT.anchoredPosition = new Vector2(0, 50);
            skillPanelRT.sizeDelta = new Vector2(panelWidth, panelHeight);
            Image panelImg = skillPanel.AddComponent<Image>();
            panelImg.color = panelColor;

            // 标题
            CreateText(skillPanel.transform, "Title", "选择技能", new Vector2(0, panelHeight / 2 - 40), new Vector2(400, 50), 36, titleColor, TextAnchor.MiddleCenter, true);

            // 创建3个技能卡片
            optionCards = new GameObject[3];
            optionIcons = new Image[3];
            optionNames = new Text[3];
            optionDescs = new Text[3];
            optionTypes = new Text[3];

            float startX = -(cardWidth * 1.5f + cardSpacing);

            for (int i = 0; i < 3; i++)
            {
                float x = startX + i * (cardWidth + cardSpacing);
                CreateSkillCard(skillPanel.transform, i, x);
            }
        }

        private void CreateSkillCard(Transform parent, int index, float x)
        {
            // 卡片容器
            GameObject card = new GameObject($"SkillCard_{index}");
            card.transform.SetParent(parent);
            RectTransform cardRT = card.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.anchoredPosition = new Vector2(x, -20);
            cardRT.sizeDelta = new Vector2(cardWidth, cardHeight);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = buttonColor;
            cardBg.type = Image.Type.Sliced;

            // 使用 EventTrigger 处理点击
            EventTrigger trigger = card.AddComponent<EventTrigger>();
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            int idx = index;
            clickEntry.callback.AddListener((data) => OnCardClicked(idx));
            trigger.triggers.Add(clickEntry);

            optionCards[index] = card;

            // 技能图标区域
            GameObject iconArea = new GameObject("IconArea");
            iconArea.transform.SetParent(card.transform);
            RectTransform iconRT = iconArea.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 1f);
            iconRT.anchorMax = new Vector2(0.5f, 1f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(0, -70);
            iconRT.sizeDelta = new Vector2(100, 100);

            Image iconBg = iconArea.AddComponent<Image>();
            iconBg.color = new Color(0.2f, 0.2f, 0.3f, 1f);

            // 技能图标
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(iconArea.transform);
            RectTransform iconImgRT = iconGO.AddComponent<RectTransform>();
            iconImgRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconImgRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconImgRT.pivot = new Vector2(0.5f, 0.5f);
            iconImgRT.anchoredPosition = Vector2.zero;
            iconImgRT.sizeDelta = new Vector2(80, 80);

            optionIcons[index] = iconGO.AddComponent<Image>();
            optionIcons[index].color = Color.white;

            // 技能名称
            optionNames[index] = CreateText(card.transform, $"Name_{index}", "", new Vector2(0, -130), new Vector2(cardWidth - 20, 30), 22, Color.white, TextAnchor.MiddleCenter, true);

            // 技能类型标签
            optionTypes[index] = CreateText(card.transform, $"Type_{index}", "", new Vector2(0, -160), new Vector2(cardWidth - 20, 25), 16, Color.green, TextAnchor.MiddleCenter, false);

            // 技能描述
            optionDescs[index] = CreateText(card.transform, $"Desc_{index}", "", new Vector2(0, -220), new Vector2(cardWidth - 20, 80), 14, new Color(0.8f, 0.8f, 0.8f), TextAnchor.UpperCenter, false);
        }

        private Text CreateText(Transform parent, string name, string text, Vector2 anchorPos, Vector2 size, int fontSize, Color color, TextAnchor align, bool bold)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
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

        private void OnCardClicked(int index)
        {
            if (currentChoices == null || index >= currentChoices.Length || currentChoices[index] == null)
                return;

            selectedIndex = index;

            // 高亮选中卡片
            for (int i = 0; i < 3; i++)
            {
                if (optionCards[i] != null)
                {
                    Image bg = optionCards[i].GetComponent<Image>();
                    if (bg != null)
                    {
                        bg.color = (i == index) ? buttonSelectedColor : buttonColor;
                    }
                }
            }

            // 触发事件
            OnSkillSelected?.Invoke(index, currentChoices[index]);

            // 隐藏面板
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        private int GetSkillIconIndex(string skillName)
        {
            string[] skillOrder = new string[]
            {
                "冰霜新星", "治疗波", "雷电打击", "火焰旋风", "护盾", "生命汲取",
                "疾风步", "力量涌动", "狂热", "钢铁之躯", "吸血体质", "磁力吸引"
            };

            for (int i = 0; i < skillOrder.Length; i++)
            {
                if (skillName.Contains(skillOrder[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 创建UI根节点
        /// </summary>
        private static Canvas CreateUIRoot()
        {
            GameObject canvasGO = new GameObject("LevelUpCanvas");
            canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            return canvas;
        }

        /// <summary>
        /// 创建面板预制体
        /// </summary>
        private static GameObject CreatePanel(Canvas canvas)
        {
            GameObject panel = new GameObject("LevelUpPanel");
            panel.transform.SetParent(canvas.transform, false);
            return panel;
        }
    }
}
