using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 自动创建游戏UI - 挂载到任意对象上即可自动创建
    /// </summary>
    public class AutoCreateUI : MonoBehaviour
    {
        [Header("血条设置")]
        public Vector2 healthBarSize = new Vector2(300, 25);
        public Vector2 healthBarPos = new Vector2(30, -40);
        public Color healthBarColor = Color.red;

        [Header("经验条设置")]
        public Vector2 expBarSize = new Vector2(300, 20);
        public Vector2 expBarPos = new Vector2(30, -75);
        public Color expBarColor = Color.yellow;

        [Header("文字设置")]
        public Vector2 healthTextPos = new Vector2(340, -40);
        public int healthTextFontSize = 18;
        public Vector2 levelTextPos = new Vector2(30, -100);
        public int levelTextFontSize = 24;
        public Vector2 timeTextPos = new Vector2(-250, 40);
        public int timeTextFontSize = 28;
        public Vector2 killTextPos = new Vector2(-250, 10);
        public int killTextFontSize = 22;

        [Header("UI设置")]
        [Tooltip("是否创建血条")]
        public bool createHealthBar = true;
        [Tooltip("是否创建经验条")]
        public bool createExperienceBar = true;
        [Tooltip("是否创建信息文字（时间/击杀等）")]
        public bool createInfoTexts = true;

        private void Start()
        {
            CreateGameUI();
            Destroy(this);
        }
        
        private void CreateGameUI()
        {
            // 创建 Canvas
            GameObject canvasGO = new GameObject("GameCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // 查找或创建 GameHUD
            GameHUD hud = FindObjectOfType<GameHUD>();
            if (hud == null)
            {
                GameObject hudGO = new GameObject("GameHUD");
                hud = hudGO.AddComponent<GameHUD>();
            }
            
            // 绑定 Canvas 到 GameHUD
            SetPrivateField(hud, "healthBar", CreateSlider(canvasGO.transform, "HealthBar", healthBarSize, healthBarPos, healthBarColor));
            SetPrivateField(hud, "experienceBar", CreateSlider(canvasGO.transform, "ExperienceBar", expBarSize, expBarPos, expBarColor));
            SetPrivateField(hud, "healthText", CreateText(canvasGO.transform, "HealthText", "100/100", healthTextPos, healthTextFontSize));
            SetPrivateField(hud, "levelText", CreateText(canvasGO.transform, "LevelText", "Lv.1", levelTextPos, levelTextFontSize, Color.yellow));
            SetPrivateField(hud, "timeText", CreateText(canvasGO.transform, "TimeText", "00:00", timeTextPos, timeTextFontSize));
            SetPrivateField(hud, "killText", CreateText(canvasGO.transform, "KillText", "击杀: 0", killTextPos, killTextFontSize));
            
            Debug.Log("UI创建完成！");
        }
        
        private Slider CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchorPos, Color fillColor)
        {
            GameObject sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent);
            
            RectTransform rt = sliderGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = anchorPos;
            rt.sizeDelta = size;
            
            // 背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGO.transform);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.6f);
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            
            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;
            
            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            
            // Slider组件
            Slider slider = sliderGO.AddComponent<Slider>();
            slider.targetGraphic = bgImg;
            slider.fillRect = fillRT;
            slider.direction = Slider.Direction.LeftToRight;
            
            return slider;
        }
        
        private TextMeshProUGUI CreateText(Transform parent, string name, string content, Vector2 anchorPos, int fontSize, Color? color = null)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent);
            
            RectTransform rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = anchorPos;
            rt.sizeDelta = new Vector2(200, 40);
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color ?? Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            
            return tmp;
        }
        
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
    }
}
