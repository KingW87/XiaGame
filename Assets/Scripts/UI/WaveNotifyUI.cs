using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ClawSurvivor.Enemy;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    public class WaveNotifyUI : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("波次通知颜色")]
        public Color waveColor = new Color(1f, 0.4f, 0.1f);
        [Tooltip("副标题颜色")]
        public Color subColor = new Color(0.9f, 0.9f, 0.9f);
        [Tooltip("Boss波次通知颜色")]
        public Color bossWaveColor = new Color(1f, 0.1f, 0.1f);
        [Tooltip("普通波次显示时长（秒）")]
        public float displayDuration = 3f;
        [Tooltip("Boss波次显示时长（秒）")]
        public float bossDisplayDuration = 4f;
        [Tooltip("毒圈通知颜色")]
        public Color poisonCircleColor = new Color(0.6f, 0f, 0.6f);
        [Tooltip("毒圈显示时长（秒）")]
        public float poisonDisplayDuration = 4f;

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private Text waveText;
        private Text subText;
        private int lastWave = 0;
        private Coroutine fadeCoroutine;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            CreateUI();
            uiRoot.SetActive(false);

            // 订阅毒圈事件
            var chapterManager = FindObjectOfType<ChapterManager>();
            if (chapterManager != null)
            {
                chapterManager.OnPoisonCircleStart += ShowPoisonCircleNotify;
            }
        }

        private void OnDestroy()
        {
            // 取消订阅
            var chapterManager = FindObjectOfType<ChapterManager>();
            if (chapterManager != null)
            {
                chapterManager.OnPoisonCircleStart -= ShowPoisonCircleNotify;
            }
        }

        private void Update()
        {
            var spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null && spawner.CurrentWave != lastWave)
            {
                lastWave = spawner.CurrentWave;
                if (lastWave > 1)
                    ShowWaveNotify(lastWave);
            }
        }

        private void CreateUI()
        {
            uiRoot = new GameObject("WaveNotifyUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRT = uiRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0.5f, 0.5f);
            rootRT.anchorMax = new Vector2(0.5f, 0.5f);
            rootRT.pivot = new Vector2(0.5f, 0.5f);
            rootRT.sizeDelta = new Vector2(600, 120);

            waveText = new GameObject("WaveText").AddComponent<Text>();
            waveText.transform.SetParent(uiRoot.transform, false);
            RectTransform waveRT = waveText.GetComponent<RectTransform>();
            waveRT.anchorMin = new Vector2(0.5f, 0.5f);
            waveRT.anchorMax = new Vector2(0.5f, 0.5f);
            waveRT.pivot = new Vector2(0.5f, 0.5f);
            waveRT.anchoredPosition = new Vector2(0, 20);
            waveRT.sizeDelta = new Vector2(500, 50);
            waveText.fontSize = 36;
            waveText.color = waveColor;
            waveText.fontStyle = FontStyle.Bold;
            waveText.alignment = TextAnchor.MiddleCenter;

            subText = new GameObject("SubText").AddComponent<Text>();
            subText.transform.SetParent(uiRoot.transform, false);
            RectTransform subRT = subText.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 0.5f);
            subRT.anchorMax = new Vector2(0.5f, 0.5f);
            subRT.pivot = new Vector2(0.5f, 0.5f);
            subRT.anchoredPosition = new Vector2(0, -20);
            subRT.sizeDelta = new Vector2(400, 30);
            subText.fontSize = 20;
            subText.color = subColor;
            subText.alignment = TextAnchor.MiddleCenter;
        }

        private void ShowWaveNotify(int wave)
        {
            bool isBossWave = wave % 5 == 0;

            if (isBossWave)
            {
                waveText.text = $"第 {wave} 波 - BOSS来袭！";
                waveText.color = bossWaveColor;
                waveText.fontSize = 42;
                subText.text = "强大的Boss出现了！小心！";
            }
            else
            {
                waveText.text = $"第 {wave} 波";
                waveText.color = waveColor;
                waveText.fontSize = 36;
                subText.text = "敌人变强了！";
            }

            uiRoot.SetActive(true);

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeAndHide(isBossWave ? bossDisplayDuration : displayDuration));
        }

        private void ShowPoisonCircleNotify()
        {
            waveText.text = "毒圈来袭！";
            waveText.color = poisonCircleColor;
            waveText.fontSize = 40;
            subText.text = "毒圈正在缩小，圈外会持续掉血！";

            uiRoot.SetActive(true);

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeAndHide(poisonDisplayDuration));
        }

        private IEnumerator FadeAndHide(float duration)
        {
            CanvasGroup group = uiRoot.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            yield return new WaitForSeconds(duration - 1f);

            float fadeTime = 1f;
            while (fadeTime > 0)
            {
                fadeTime -= Time.deltaTime;
                group.alpha = Mathf.Max(0, fadeTime);
                yield return null;
            }

            Destroy(group);
            uiRoot.SetActive(false);
        }
    }
}
