using UnityEngine;
using TMPro;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// UI字体管理器 - 统一管理项目中的字体
    /// </summary>
    public class UIFontManager : MonoBehaviour
    {
        private static UIFontManager _instance;
        public static UIFontManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIFontManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIFontManager");
                        _instance = go.AddComponent<UIFontManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("字体资源")]
        [Tooltip("Electronic Highway Sign 字体")]
        public TMP_FontAsset electronicFont;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 加载字体
            if (electronicFont == null)
            {
                LoadFont();
            }
        }

        private void LoadFont()
        {
            // 尝试从Resources加载
            electronicFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
#if UNITY_EDITOR
            if (electronicFont == null)
            {
                // 尝试从Editor资源路径加载
                string path = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Electronic Highway Sign SDF.asset";
                electronicFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
#endif
        }

        /// <summary>
        /// 获取统一字体
        /// </summary>
        public TMP_FontAsset GetFont()
        {
            if (electronicFont == null)
            {
                LoadFont();
            }
            return electronicFont;
        }

        /// <summary>
        /// 为TextMeshProUGUI设置字体
        /// </summary>
        public void SetFont(TextMeshProUGUI tmp)
        {
            if (tmp != null && electronicFont != null)
            {
                tmp.font = electronicFont;
            }
        }
    }
}
