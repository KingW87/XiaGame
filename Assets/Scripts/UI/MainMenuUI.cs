using UnityEngine;
using UnityEngine.UI;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 主菜单界面 - 包含游戏标题、开始游戏、退出按钮
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("标题文字")]
        public string titleText = "ClawSurvivor";
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.85f, 0.2f);
        [Tooltip("副标题文字")]
        public string subtitleText = "生存 · 战斗 · 进化";
        [Tooltip("副标题颜色")]
        public Color subtitleColor = new Color(0.7f, 0.7f, 0.8f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.15f, 0.5f, 0.25f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.2f, 0.6f, 0.35f);
        [Tooltip("商店按钮颜色")]
        public Color shopButtonColor = new Color(0.4f, 0.2f, 0.6f);
        [Tooltip("商店按钮悬停颜色")]
        public Color shopButtonHoverColor = new Color(0.5f, 0.3f, 0.7f);
        [Tooltip("装备按钮颜色")]
        public Color equipButtonColor = new Color(0.2f, 0.4f, 0.6f);
        [Tooltip("装备按钮悬停颜色")]
        public Color equipButtonHoverColor = new Color(0.3f, 0.5f, 0.7f);
        [Tooltip("宠物按钮颜色")]
        public Color petButtonColor = new Color(0.6f, 0.4f, 0.2f);
        [Tooltip("宠物按钮悬停颜色")]
        public Color petButtonHoverColor = new Color(0.7f, 0.5f, 0.3f);
        [Tooltip("退出按钮颜色")]
        public Color exitButtonColor = new Color(0.5f, 0.15f, 0.15f);
        [Tooltip("退出按钮悬停颜色")]
        public Color exitButtonHoverColor = new Color(0.6f, 0.25f, 0.25f);
        [Tooltip("背景颜色")]
        public Color backgroundColor = new Color(0.05f, 0.05f, 0.1f);

        private Canvas parentCanvas;
        private GameObject uiRoot;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("MainMenuUI 必须放在 Canvas 下面！");
                return;
            }

            CreateMainMenuUI();
        }

        private void CreateMainMenuUI()
        {
            uiRoot = new GameObject("MainMenuUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform uiRootRT = uiRoot.AddComponent<RectTransform>();
            uiRootRT.anchorMin = Vector2.zero;
            uiRootRT.anchorMax = Vector2.one;
            uiRootRT.pivot = new Vector2(0.5f, 0.5f);
            uiRootRT.anchoredPosition = Vector2.zero;
            uiRootRT.sizeDelta = Vector2.zero;

            // 背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(uiRoot.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = backgroundColor;

            // 标题
            CreateText(uiRoot.transform, "Title", titleText, new Vector2(0, 150),
                new Vector2(600, 80), 56, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 副标题
            CreateText(uiRoot.transform, "Subtitle", subtitleText, new Vector2(0, 80),
                new Vector2(400, 40), 24, subtitleColor, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 分隔线
            CreateLine(uiRoot.transform, "Line", new Vector2(0, 40), new Vector2(300, 3));

            // 开始游戏按钮
            CreateButton(uiRoot.transform, "StartBtn", "开始游戏", new Vector2(0, -30),
                new Vector2(260, 60), buttonColor, buttonHoverColor, () =>
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
                    StartGame();
                });

            // 商店按钮
            CreateButton(uiRoot.transform, "ShopBtn", "珍宝商店", new Vector2(0, -110),
                new Vector2(260, 60), shopButtonColor, shopButtonHoverColor, () =>
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
                    OpenShop();
                });

            // 装备按钮
            CreateButton(uiRoot.transform, "EquipBtn", "装备强化", new Vector2(0, -190),
                new Vector2(260, 60), equipButtonColor, equipButtonHoverColor, () =>
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
                    OpenEquipPanel();
                });

            // 宠物按钮
            CreateButton(uiRoot.transform, "PetBtn", "宠物系统", new Vector2(0, -270),
                new Vector2(260, 60), petButtonColor, petButtonHoverColor, () =>
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
                    OpenPetPanel();
                });

            // 退出游戏按钮
            CreateButton(uiRoot.transform, "ExitBtn", "退出游戏", new Vector2(0, -350),
                new Vector2(260, 60), exitButtonColor, exitButtonHoverColor, () =>
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                });

            // 版本信息
            CreateText(uiRoot.transform, "Version", "v0.1 Alpha", new Vector2(180, -280),
                new Vector2(200, 30), 16, new Color(0.4f, 0.4f, 0.5f), TextAnchor.MiddleRight, FontStyle.Normal);
        }

        private void StartGame()
        {
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.TransitionToGame();
            }
            else
            {
                // 没有SceneTransition时直接重启场景
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OpenShop()
        {
            // 打开宝石商店
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OpenGemShop();
            }
        }

        private void OpenEquipPanel()
        {
            Hide();
            // 查找并显示装备面板
            var equipPanel = GetComponentInParent<Canvas>().GetComponentInChildren<EquipPanelUI>();
            if (equipPanel != null)
            {
                equipPanel.Show();
            }
        }

        private void OpenPetPanel()
        {
            Hide();
            // 查找并显示宠物面板
            var petPanel = GetComponentInParent<Canvas>().GetComponentInChildren<PetPanelUI>();
            if (petPanel != null)
            {
                petPanel.Show();
            }
        }

        private Text CreateText(Transform parent, string name, string text, Vector2 anchorPos, Vector2 size,
            int fontSize, Color color, TextAnchor align, FontStyle style)
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
            txt.fontStyle = style;
            return txt;
        }

        private void CreateLine(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject line = new GameObject(name);
            line.transform.SetParent(parent, false);
            RectTransform rt = line.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            line.AddComponent<Image>().color = new Color(1, 1, 1, 0.15f);
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size,
            Color color, Color hoverColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);
            RectTransform rt = btn.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btn.AddComponent<Image>().color = color;
            Button button = btn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f);
            button.colors = colors;
            button.onClick.AddListener(onClick);

            // 按钮文字
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btn.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.pivot = new Vector2(0.5f, 0.5f);
            labelRT.anchoredPosition = Vector2.zero;
            labelRT.sizeDelta = Vector2.zero;
            Text labelTxt = labelObj.AddComponent<Text>();
            labelTxt.text = label;
            labelTxt.fontSize = 24;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.fontStyle = FontStyle.Bold;
        }

        /// <summary>
        /// 隐藏主菜单（进入游戏后调用）
        /// </summary>
        public void Hide()
        {
            if (uiRoot != null)
                uiRoot.SetActive(false);
        }

        /// <summary>
        /// 显示主菜单（返回主菜单时调用）
        /// </summary>
        public void Show()
        {
            if (uiRoot != null)
                uiRoot.SetActive(true);
        }
    }
}
