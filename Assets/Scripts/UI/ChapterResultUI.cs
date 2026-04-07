using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 章节结算UI - 显示章节完成/失败的结算界面
    /// </summary>
    public class ChapterResultUI : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板颜色")]
        public Color panelColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        [Tooltip("成功颜色")]
        public Color successColor = new Color(0.2f, 0.8f, 0.3f);
        [Tooltip("失败颜色")]
        public Color failColor = new Color(0.8f, 0.2f, 0.2f);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI statsText;
        private TextMeshProUGUI rewardsText;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("ChapterResultUI 必须放在 Canvas 下面！");
                return;
            }

            CreateUI();

            // 监听章节事件
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.OnChapterComplete += ShowSuccess;
                ChapterManager.Instance.OnChapterFailed += ShowFail;
            }

            uiRoot.SetActive(false);
        }

        private void CreateUI()
        {
            uiRoot = new GameObject("ChapterResultUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rt = uiRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // 遮罩
            GameObject mask = new GameObject("Mask");
            mask.transform.SetParent(uiRoot.transform, false);
            RectTransform maskRT = mask.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.sizeDelta = Vector2.zero;
            Image maskImg = mask.AddComponent<Image>();
            maskImg.color = new Color(0, 0, 0, 0.7f);

            // 面板
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(uiRoot.transform, false);
            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(500, 450);
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = panelColor;

            // 标题
            titleText = CreateTMPText(panel.transform, "Title", "章节完成", new Vector2(0, 160), new Vector2(400, 50), 40, successColor);

            // 分隔线
            CreateLine(panel.transform, "Line1", new Vector2(0, 120), new Vector2(400, 2));

            // 统计数据
            statsText = CreateTMPText(panel.transform, "Stats", "", new Vector2(0, 50), new Vector2(400, 120), 20, Color.white);

            // 分隔线2
            CreateLine(panel.transform, "Line2", new Vector2(0, -30), new Vector2(400, 2));

            // 奖励
            rewardsText = CreateTMPText(panel.transform, "Rewards", "", new Vector2(0, -80), new Vector2(400, 100), 18, Color.yellow);

            // 按钮区域
            CreateButton(panel.transform, "NextChapterBtn", "下一章", new Vector2(-120, -180), new Vector2(180, 50),
                successColor, () =>
                {
                    NextChapter();
                });

            CreateButton(panel.transform, "MenuBtn", "返回主菜单", new Vector2(120, -180), new Vector2(180, 50),
                new Color(0.4f, 0.4f, 0.5f), () =>
                {
                    ReturnToMenu();
                });
        }

        private TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
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

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
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
            colors.highlightedColor = color * 1.2f;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btn.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 22;
            labelTmp.color = Color.white;
            labelTmp.alignment = TextAlignmentOptions.Center;
        }

        private void ShowSuccess()
        {
            titleText.text = "章节完成";
            titleText.color = successColor;

            // 显示统计
            int wave = ChapterManager.Instance?.currentWave ?? 0;
            float time = ChapterManager.Instance?.ChapterTime ?? 0;
            int mins = Mathf.FloorToInt(time / 60);
            int secs = Mathf.FloorToInt(time % 60);

            statsText.text = $"存活时间: {mins:00}:{secs}\n达成波次: {wave}\n击杀数: {GameManager.Instance?.EnemiesKilled ?? 0}";

            // 显示奖励
            var (junk, frag, artifact) = CollectibleSystem.Instance?.GetChapterCollectibles() ?? (0, 0, 0);
            rewardsText.text = $"获得奖励:\n金币 +{junk}\n藏品碎片 +{frag}\n神装碎片 +{artifact}";

            uiRoot.SetActive(true);
            Time.timeScale = 0;
        }

        private void ShowFail()
        {
            titleText.text = "章节失败";
            titleText.color = failColor;

            int wave = ChapterManager.Instance?.currentWave ?? 0;
            statsText.text = $"坚持波次: {wave}\n击杀数: {GameManager.Instance?.EnemiesKilled ?? 0}";
            rewardsText.text = "再接再厉！";

            uiRoot.SetActive(true);
            Time.timeScale = 0;
        }

        private void NextChapter()
        {
            Time.timeScale = 1;
            uiRoot.SetActive(false);

            // 保存本章收藏品
            CollectibleSystem.Instance?.SaveChapterCollectibles();

            // 开始下一章
            int nextChapter = (ChapterManager.Instance?.currentChapter ?? 1) + 1;
            ChapterManager.Instance?.StartChapter(nextChapter);
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1;
            uiRoot.SetActive(false);

            // 保存本章收藏品
            CollectibleSystem.Instance?.SaveChapterCollectibles();

            // 返回主菜单
            SceneTransition.Instance?.TransitionToMainMenu();
        }

        private void OnDestroy()
        {
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.OnChapterComplete -= ShowSuccess;
                ChapterManager.Instance.OnChapterFailed -= ShowFail;
            }
        }
    }
}
