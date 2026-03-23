using UnityEngine;
using UnityEngine.UI;

namespace ClawSurvivor.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("跟随目标")]
        [Tooltip("需要跟随的目标Transform")]
        public Transform playerTransform;

        [Header("跟随偏移")]
        [Tooltip("血条在玩家上方的偏移")]
        public Vector2 healthBarOffset = new Vector2(0, 0.8f);
        [Tooltip("经验条在玩家上方的偏移")]
        public Vector2 expBarOffset = new Vector2(0, 0.6f);
        [Tooltip("血量文字偏移")]
        public Vector2 healthTextOffset = new Vector2(0.5f, 0.8f);
        [Tooltip("等级文字偏移")]
        public Vector2 levelTextOffset = new Vector2(0.5f, 0.6f);

        [Header("血条")]
        [Tooltip("血条Slider组件")]
        public Slider healthBar;
        [Tooltip("血量数字文字")]
        public Text healthText;

        [Header("经验条")]
        [Tooltip("经验条Slider组件")]
        public Slider experienceBar;
        [Tooltip("等级文字")]
        public Text levelText;

        [Header("信息（固定屏幕位置）")]
        [Tooltip("存活时间文字")]
        public Text timeText;
        [Tooltip("击杀数文字")]
        public Text killText;

        [Header("技能槽")]
        [Tooltip("技能图标数组")]
        public Image[] skillIcons;
        [Tooltip("技能冷却时间数组")]
        public float[] skillCooldowns;

        private Player.PlayerController player;
        private Canvas parentCanvas;

        private void Start()
        {
            player = FindObjectOfType<Player.PlayerController>();
            if (playerTransform == null && player != null)
                playerTransform = player.transform;

            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("GameHUD 必须放在 Canvas 下面！");
                return;
            }

            player.OnHealthChanged += UpdateHealthBar;
            player.OnExperienceChanged += UpdateExperienceBar;
            player.OnLevelUp += OnPlayerLevelUp;

            UpdateHealthBar(player.CurrentHealth, player.MaxHealth);
            UpdateExperienceBar(player.Experience, player.ExperienceToNextLevel);
            UpdateLevelText(player.Level);
        }

        private void Update()
        {
            UpdateTimeAndKills();
            FollowPlayer();
        }

        private void FollowPlayer()
        {
            if (playerTransform == null || parentCanvas == null) return;

            if (healthBar != null)
                WorldToUI(playerTransform.position + (Vector3)healthBarOffset, healthBar.GetComponent<RectTransform>());

            if (healthText != null)
                WorldToUI(playerTransform.position + (Vector3)healthTextOffset, healthText.GetComponent<RectTransform>());

            if (experienceBar != null)
                WorldToUI(playerTransform.position + (Vector3)expBarOffset, experienceBar.GetComponent<RectTransform>());

            if (levelText != null)
                WorldToUI(playerTransform.position + (Vector3)levelTextOffset, levelText.GetComponent<RectTransform>());
        }

        private void WorldToUI(Vector3 worldPos, RectTransform uiRect)
        {
            Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            Vector2 screenPos = cam != null
                ? RectTransformUtility.WorldToScreenPoint(cam, worldPos)
                : (Vector2)Camera.main.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(), screenPos, cam, out Vector2 localPos);
            uiRect.anchoredPosition = localPos;
        }
        
        private void UpdateHealthBar(int current, int max)
        {
            if (healthBar != null)
                healthBar.value = (float)current / max;
            
            if (healthText != null)
                healthText.text = $"{current}/{max}";
        }
        
        private void UpdateExperienceBar(int current, int needed)
        {
            if (experienceBar != null)
                experienceBar.value = (float)current / needed;
        }
        
        private void OnPlayerLevelUp(int newLevel)
        {
            UpdateLevelText(newLevel);
            // 可以添加升级特效
        }
        
        private void UpdateLevelText(int level)
        {
            if (levelText != null)
                levelText.text = $"Lv.{level}";
        }
        
        private void UpdateTimeAndKills()
        {
            if (Systems.GameManager.Instance != null)
            {
                if (timeText != null)
                    timeText.text = FormatTime(Systems.GameManager.Instance.GameTime);
                
                if (killText != null)
                    killText.text = $"击杀: {Systems.GameManager.Instance.EnemiesKilled}";
            }
        }
        
        private string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return $"{mins:00}:{secs:00}";
        }
        
        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnHealthChanged -= UpdateHealthBar;
                player.OnExperienceChanged -= UpdateExperienceBar;
                player.OnLevelUp -= OnPlayerLevelUp;
            }
        }
    }
}
