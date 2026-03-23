using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using ClawSurvivor.UI;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 场景过渡管理器 - 渐黑/渐白过渡效果，控制游戏流程
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance;

        [Header("过渡设置")]
        [Tooltip("过渡颜色")]
        public Color fadeColor = Color.black;
        [Tooltip("渐变速度")]
        public float fadeSpeed = 1.5f;
        [Tooltip("游戏场景名称")]
        public string gameSceneName = "SampleScene";

        [Header("状态")]
        [Tooltip("当前是否处于主菜单模式")]
        public bool isMainMenuMode = true;

        private GameObject fadeCanvas;
        private Image fadeImage;
        private bool isTransitioning;
        private MainMenuUI mainMenuUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            CreateFadeOverlay();
        }

        private void CreateFadeOverlay()
        {
            fadeCanvas = new GameObject("FadeCanvas");
            fadeCanvas.transform.SetParent(transform);
            Canvas canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 最高层级
            fadeCanvas.AddComponent<CanvasScaler>();
            fadeCanvas.AddComponent<GraphicRaycaster>();

            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(fadeCanvas.transform, false);
            RectTransform rt = imageObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }

        /// <summary>
        /// 从主菜单进入游戏
        /// </summary>
        public void TransitionToGame()
        {
            if (isTransitioning) return;
            StartCoroutine(FadeAndStartGame());
        }

        private IEnumerator FadeAndStartGame()
        {
            isTransitioning = true;
            isMainMenuMode = false;

            // 渐黑
            yield return StartCoroutine(FadeTo(1f));

            // 隐藏主菜单
            if (mainMenuUI != null)
                mainMenuUI.Hide();

            // 显示游戏对象
            if (GameStarter.Instance != null)
                GameStarter.Instance.StartGame();

            // 确保时间恢复
            Time.timeScale = 1;

            // 渐白
            yield return StartCoroutine(FadeTo(0f));

            isTransitioning = false;
        }

        /// <summary>
        /// 返回主菜单（从游戏结算界面）
        /// </summary>
        public void TransitionToMainMenu()
        {
            if (isTransitioning) return;
            StartCoroutine(FadeAndShowMenu());
        }

        private IEnumerator FadeAndShowMenu()
        {
            isTransitioning = true;

            // 渐黑
            yield return StartCoroutine(FadeTo(1f));

            // 确保时间恢复
            Time.timeScale = 1;

            // 重新加载场景
            SceneManager.LoadScene(gameSceneName);

            // 等待场景加载
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == gameSceneName);

            // 标记为主菜单模式
            isMainMenuMode = true;

            // 显示主菜单
            if (mainMenuUI != null)
                mainMenuUI.Show();

            // 渐白
            yield return StartCoroutine(FadeTo(0f));

            isTransitioning = false;
        }

        /// <summary>
        /// 渐变到目标透明度
        /// </summary>
        private IEnumerator FadeTo(float targetAlpha)
        {
            if (fadeImage == null) yield break;

            Color currentColor = fadeImage.color;
            float startAlpha = currentColor.a;

            float elapsed = 0f;
            float duration = Mathf.Abs(targetAlpha - startAlpha) / fadeSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }

            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);
        }

        /// <summary>
        /// 设置主菜单UI引用
        /// </summary>
        public void SetMainMenuUI(MainMenuUI ui)
        {
            mainMenuUI = ui;
        }

        private void OnDestroy()
        {
            if (fadeCanvas != null)
                Destroy(fadeCanvas);
        }
    }
}
