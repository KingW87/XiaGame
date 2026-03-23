using UnityEngine;
using UnityEngine.UI;
using ClawSurvivor.Enemy;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// Boss血条UI - 屏幕顶部显示当前Boss名称和血条
    /// 挂载到Canvas下
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("颜色设置")]
        [Tooltip("血条颜色")]
        public Color healthColor = new Color(0.9f, 0.15f, 0.1f);
        [Tooltip("血条背景颜色")]
        public Color bgColor = new Color(0.2f, 0.2f, 0.2f);
        [Tooltip("Boss名字颜色")]
        public Color nameColor = new Color(1f, 0.85f, 0.2f);
        [Tooltip("Boss等级颜色")]
        public Color levelColor = new Color(0.8f, 0.8f, 0.8f);

        [Header("布局")]
        [Tooltip("血条宽度")]
        public float barWidth = 600f;
        [Tooltip("血条高度")]
        public float barHeight = 24f;
        [Tooltip("距顶部偏移")]
        public float topOffset = 60f;

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private Text bossNameText;
        private Text bossLevelText;
        private Image healthFill;
        private BossController currentBoss;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            CreateUI();
            uiRoot.SetActive(false);

            // 监听Boss出现和死亡事件
            BossController.OnBossSpawned += OnBossSpawned;
            BossController.OnBossDefeated += OnBossDefeated;
        }

        private void OnDestroy()
        {
            BossController.OnBossSpawned -= OnBossSpawned;
            BossController.OnBossDefeated -= OnBossDefeated;
        }

        private void CreateUI()
        {
            // 根节点 - 全屏锚定
            uiRoot = new GameObject("BossHealthBarUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRT = uiRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.5f, 1f);
            rootRT.anchorMax = new Vector2(0.5f, 1f);
            rootRT.pivot = new Vector2(0.5f, 1f);
            rootRT.anchoredPosition = new Vector2(0, -topOffset);
            rootRT.sizeDelta = new Vector2(barWidth + 40, 70);

            // Boss名称
            GameObject nameObj = new GameObject("BossName");
            nameObj.transform.SetParent(uiRoot.transform, false);
            bossNameText = nameObj.AddComponent<Text>();
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.5f, 1f);
            nameRT.anchorMax = new Vector2(0.5f, 1f);
            nameRT.pivot = new Vector2(0.5f, 1f);
            nameRT.anchoredPosition = new Vector2(0, 0);
            nameRT.sizeDelta = new Vector2(barWidth, 30);
            bossNameText.fontSize = 26;
            bossNameText.fontStyle = FontStyle.Bold;
            bossNameText.color = nameColor;
            bossNameText.alignment = TextAnchor.MiddleCenter;

            // Boss等级
            GameObject levelObj = new GameObject("BossLevel");
            levelObj.transform.SetParent(uiRoot.transform, false);
            bossLevelText = levelObj.AddComponent<Text>();
            RectTransform levelRT = levelObj.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0.5f, 1f);
            levelRT.anchorMax = new Vector2(0.5f, 1f);
            levelRT.pivot = new Vector2(0.5f, 1f);
            levelRT.anchoredPosition = new Vector2(0, -25);
            levelRT.sizeDelta = new Vector2(barWidth, 20);
            bossLevelText.fontSize = 16;
            bossLevelText.color = levelColor;
            bossLevelText.alignment = TextAnchor.MiddleCenter;

            // 血条背景
            GameObject barBg = new GameObject("HealthBarBg");
            barBg.transform.SetParent(uiRoot.transform, false);
            RectTransform bgRT = barBg.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.5f, 1f);
            bgRT.anchorMax = new Vector2(0.5f, 1f);
            bgRT.pivot = new Vector2(0.5f, 1f);
            bgRT.anchoredPosition = new Vector2(0, -45);
            bgRT.sizeDelta = new Vector2(barWidth, barHeight);
            Image bgImg = barBg.AddComponent<Image>();
            bgImg.color = bgColor;

            // 血条填充
            GameObject barFill = new GameObject("HealthBarFill");
            barFill.transform.SetParent(barBg.transform, false);
            RectTransform fillRT = barFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.pivot = new Vector2(0, 0.5f);
            fillRT.anchoredPosition = Vector2.zero;
            fillRT.sizeDelta = Vector2.zero;
            healthFill = barFill.AddComponent<Image>();
            healthFill.color = healthColor;
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillAmount = 1f;
        }

        private void Update()
        {
            if (currentBoss == null || uiRoot == null || !uiRoot.activeSelf) return;

            // 更新血条
            float hp = currentBoss.HealthPercentage;
            healthFill.fillAmount = Mathf.Max(0, hp);

            // 血量低时变红闪烁
            if (hp < 0.3f)
            {
                healthFill.color = Color.Lerp(healthColor, Color.yellow, Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f);
            }
            else
            {
                healthFill.color = healthColor;
            }
        }

        private void OnBossSpawned(BossController boss)
        {
            currentBoss = boss;
            if (uiRoot != null)
            {
                bossNameText.text = boss.BossName;
                bossLevelText.text = $"Lv.{boss.bossLevel} Boss";
                uiRoot.SetActive(true);
                healthFill.fillAmount = 1f;
            }
        }

        private void OnBossDefeated(BossController boss)
        {
            currentBoss = null;
            if (uiRoot != null)
            {
                uiRoot.SetActive(false);
            }
        }
    }
}
