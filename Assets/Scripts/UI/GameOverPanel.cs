using UnityEngine;
using UnityEngine.UI;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.3f, 0.3f);
        [Tooltip("数据文字颜色")]
        public Color statColor = new Color(0.9f, 0.9f, 0.9f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.2f, 0.6f, 0.3f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.3f, 0.7f, 0.4f);
        [Tooltip("面板大小")]
        public Vector2 panelSize = new Vector2(450, 400);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private Text timeText;
        private Text killText;
        private Text waveText;
        private Text levelText;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            CreatePanelUI();

            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                player.OnHealthChanged += CheckDeath;
        }

        private void CheckDeath(int current, int max)
        {
            if (current <= 0)
                ShowGameOver();
        }

        private void CreatePanelUI()
        {
            uiRoot = new GameObject("GameOverUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRT = uiRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            // 遮罩
            GameObject mask = new GameObject("Mask");
            mask.transform.SetParent(uiRoot.transform, false);
            RectTransform maskRT = mask.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.sizeDelta = Vector2.zero;
            maskRT.offsetMin = Vector2.zero;
            maskRT.offsetMax = Vector2.zero;
            mask.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // 面板
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(uiRoot.transform, false);
            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = panelSize;
            panel.AddComponent<Image>().color = panelColor;

            // 标题
            CreateText(panel.transform, "Title", "游戏结束", new Vector2(0, 140), new Vector2(300, 50), 32, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 分隔线
            GameObject line = new GameObject("Line");
            line.transform.SetParent(panel.transform, false);
            RectTransform lineRT = line.AddComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0.5f, 0.5f);
            lineRT.anchorMax = new Vector2(0.5f, 0.5f);
            lineRT.sizeDelta = new Vector2(350, 3);
            lineRT.anchoredPosition = new Vector2(0, 100);
            line.AddComponent<Image>().color = new Color(1, 1, 1, 0.2f);

            // 数据
            timeText = CreateText(panel.transform, "Time", "", new Vector2(0, 55), new Vector2(300, 35), 22, statColor, TextAnchor.MiddleCenter, FontStyle.Normal);
            killText = CreateText(panel.transform, "Kills", "", new Vector2(0, 15), new Vector2(300, 35), 22, statColor, TextAnchor.MiddleCenter, FontStyle.Normal);
            waveText = CreateText(panel.transform, "Wave", "", new Vector2(0, -25), new Vector2(300, 35), 22, statColor, TextAnchor.MiddleCenter, FontStyle.Normal);
            levelText = CreateText(panel.transform, "Level", "", new Vector2(0, -65), new Vector2(300, 35), 22, statColor, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 重新开始按钮
            CreateButton(panel.transform, "RestartBtn", "重新开始", new Vector2(-110, -130), new Vector2(200, 50), buttonColor, buttonHoverColor, () =>
            {
                GameManager.Instance.RestartGame();
            });

            // 返回主菜单按钮
            CreateButton(panel.transform, "MenuBtn", "返回主菜单", new Vector2(110, -130), new Vector2(200, 50),
                new Color(0.3f, 0.3f, 0.4f), new Color(0.4f, 0.4f, 0.5f), () =>
            {
                if (Systems.SoundManager.Instance != null)
                    Systems.SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
                if (Systems.SceneTransition.Instance != null)
                    Systems.SceneTransition.Instance.TransitionToMainMenu();
                else
                    GameManager.Instance.ExitToMenu();
            });

            uiRoot.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size, int fontSize, Color color, TextAnchor align, FontStyle style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            return txt;
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color color, Color hoverColor, UnityEngine.Events.UnityAction onClick)
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
            button.colors = colors;
            button.onClick.AddListener(onClick);

            CreateText(btn.transform, "Label", label, Vector2.zero, size, 20, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        private void ShowGameOver()
        {
            if (GameManager.Instance != null)
            {
                float time = GameManager.Instance.GameTime;
                int mins = Mathf.FloorToInt(time / 60);
                int secs = Mathf.FloorToInt(time % 60);
                timeText.text = $"存活时间: {mins:00}:{secs:00}";
                killText.text = $"击杀数: {GameManager.Instance.EnemiesKilled}";
            }

            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                levelText.text = $"达到等级: Lv.{player.Level}";

            var spawner = FindObjectOfType<Enemy.EnemySpawner>();
            if (spawner != null)
                waveText.text = $"最高波次: {spawner.CurrentWave}";

            uiRoot.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
