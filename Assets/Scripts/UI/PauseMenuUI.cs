using UnityEngine;
using UnityEngine.UI;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 暂停菜单UI - 按ESC暂停时自动弹出，支持继续/重新开始/退出
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.85f, 0.2f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.3f, 0.35f, 0.5f, 0.95f);
        [Tooltip("面板大小")]
        public Vector2 panelSize = new Vector2(350, 320);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private bool isShowing;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("PauseMenuUI 必须放在 Canvas 下面！");
                return;
            }

            CreatePanelUI();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused += Show;
                GameManager.Instance.OnGameResumed += Hide;
            }

            uiRoot.SetActive(false);
        }

        private void CreatePanelUI()
        {
            uiRoot = new GameObject("PauseMenuUI");
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
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = panelSize;
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = panelColor;

            // 标题
            CreateText(panel.transform, "Title", "游戏暂停", new Vector2(0, 100), new Vector2(300, 50), 30, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 分隔线
            CreateLine(panel.transform, "Line", new Vector2(0, 65), new Vector2(280, 3));

            // 继续游戏按钮
            CreateButton(panel.transform, "ContinueBtn", "继续游戏", new Vector2(0, 20), new Vector2(220, 50),
                new Color(0.2f, 0.5f, 0.3f), new Color(0.3f, 0.6f, 0.4f), () =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.TogglePause();
                });

            // 重新开始按钮
            CreateButton(panel.transform, "RestartBtn", "重新开始", new Vector2(0, -45), new Vector2(220, 50),
                buttonColor, buttonHoverColor, () =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.RestartGame();
                });

            // 退出到主菜单按钮
            CreateButton(panel.transform, "ExitBtn", "退出到主菜单", new Vector2(0, -110), new Vector2(220, 50),
                new Color(0.5f, 0.2f, 0.2f), new Color(0.6f, 0.3f, 0.3f), () =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.ExitToMenu();
                });
        }

        private void CreateText(Transform parent, string name, string text, Vector2 anchorPos, Vector2 size,
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
            line.AddComponent<Image>().color = new Color(1, 1, 1, 0.2f);
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
            labelTxt.fontSize = 20;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.fontStyle = FontStyle.Bold;
        }

        private void Show()
        {
            if (isShowing) return;
            uiRoot.SetActive(true);
            isShowing = true;
        }

        private void Hide()
        {
            if (!isShowing) return;
            uiRoot.SetActive(false);
            isShowing = false;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused -= Show;
                GameManager.Instance.OnGameResumed -= Hide;
            }
        }
    }
}
