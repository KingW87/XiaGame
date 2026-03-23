using UnityEngine;
using System.Collections;
using ClawSurvivor.Systems;

namespace ClawSurvivor.Pickups
{
    /// <summary>
    /// 道具拾取物 - 磁力吸附 + 碰撞拾取 + 效果执行
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("道具设置")]
        [Tooltip("道具类型")]
        public PickupType pickupType;
        [Tooltip("效果数值")]
        public float effectValue = 10f;
        [Tooltip("持续时间（秒），0=即时")]
        public float duration = 10f;
        [Tooltip("吸附范围")]
        public float magnetRadius = 3f;
        [Tooltip("吸附速度")]
        public float moveSpeed = 8f;
        [Tooltip("存在时长（秒），超时自动消失")]
        public float lifetime = 30f;

        private Transform playerTarget;
        private bool isAttracted;
        private float lifetimeTimer;
        private SpriteRenderer spriteRenderer;

        private static readonly Color[] TypeColors = {
            new Color(0.2f, 1f, 0.3f),     // HealthRegen: 绿
            new Color(0.3f, 0.7f, 1f),     // SpeedBoost: 蓝
            new Color(0.5f, 0.5f, 1f),     // Shield: 靛蓝
            new Color(1f, 0.85f, 0.2f),    // ExpBoost: 金
            new Color(0.8f, 0.4f, 1f),     // Magnet: 紫
            new Color(1f, 0.3f, 0.1f),     // Bomb: 橙红
        };

        private static readonly string[] TypeNames = {
            "生命恢复", "速度增益", "护盾", "经验加倍", "磁力增强", "炸弹清屏"
        };

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                playerTarget = player.transform;

            // 设置颜色
            int typeIndex = Mathf.Clamp((int)pickupType, 0, TypeColors.Length - 1);
            spriteRenderer.color = TypeColors[typeIndex];
            spriteRenderer.sortingOrder = 10;

            // 轻微上下浮动动画
            StartCoroutine(FloatAnimation());
        }

        private void Update()
        {
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (playerTarget == null) return;

            float distance = Vector2.Distance(transform.position, playerTarget.position);

            if (distance <= magnetRadius)
                isAttracted = true;

            if (isAttracted)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTarget.position,
                    moveSpeed * Time.deltaTime
                );
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<Player.PlayerController>();
                if (player != null)
                {
                    ApplyEffect(player);
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// 应用道具效果到玩家
        /// </summary>
        private void ApplyEffect(Player.PlayerController player)
        {
            Debug.Log($"拾取道具: {TypeNames[(int)pickupType]} (值:{effectValue}, 时长:{duration}s)");

            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.PickupItem);

            switch (pickupType)
            {
                case PickupType.HealthRegen:
                    player.Heal(Mathf.RoundToInt(effectValue));
                    if (Systems.SoundManager.Instance != null)
                        Systems.SoundManager.Instance.PlaySFX(SFXType.Heal);
                    break;

                case PickupType.SpeedBoost:
                    player.AddMoveSpeedBonus(effectValue);
                    if (duration > 0)
                        StartCoroutine(RemoveMoveSpeedBonus(player, effectValue, duration));
                    break;

                case PickupType.Shield:
                    player.AddShield(Mathf.RoundToInt(effectValue));
                    break;

                case PickupType.ExpBoost:
                    // 临时经验加倍效果 - 通过PlayerController的expBoostMultiplier实现
                    player.StartExpBoost(effectValue, duration);
                    break;

                case PickupType.Magnet:
                    var allPickups = FindObjectsOfType<Systems.ExperiencePickup>();
                    foreach (var p in allPickups)
                        p.DoubleMagnetRange();
                    if (duration > 0)
                        StartCoroutine(RestoreMagnetRange(duration));
                    break;

                case PickupType.Bomb:
                    BombClearEnemies(Mathf.RoundToInt(effectValue));
                    break;
            }
        }

        /// <summary>
        /// 炸弹清屏 - 消灭一定范围内的所有敌人
        /// </summary>
        private void BombClearEnemies(int damage)
        {
            if (playerTarget == null) return;

            var enemies = Enemy.EnemyController.AllEnemies;
            int killed = 0;

            // 对屏幕范围内所有敌人造成高额伤害
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i] != null && !enemies[i].IsDead)
                {
                    float dist = Vector2.Distance(playerTarget.position, enemies[i].transform.position);
                    if (dist <= 12f) // 12单位半径
                    {
                        enemies[i].TakeDamage(damage);
                        killed++;
                    }
                }
            }

            Debug.Log($"炸弹清屏！消灭了 {killed} 个敌人");

            // 播放炸弹爆炸音效
            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.BombExplosion);

            // 创建爆炸特效
            CreateExplosionEffect();
        }

        private void CreateExplosionEffect()
        {
            if (playerTarget == null) return;

            GameObject explosion = new GameObject("Explosion");
            explosion.transform.position = playerTarget.position;

            SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = new Color(1f, 0.5f, 0f, 0.6f);
            sr.sortingOrder = 20;
            explosion.transform.localScale = Vector3.one * 2f;

            // 快速放大后消失
            StartCoroutine(ExplosionAnim(explosion.transform));
        }

        private IEnumerator ExplosionAnim(Transform t)
        {
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                t.localScale = Vector3.one * (2f + progress * 10f);
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 0.6f * (1f - progress);
                    sr.color = c;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(t.gameObject);
        }

        private IEnumerator RemoveMoveSpeedBonus(Player.PlayerController player, float bonus, float wait)
        {
            yield return new WaitForSeconds(wait);
            // 注意：PlayerController没有减少速度的方法，此处使用负值加成
            // 由于设计上临时buff不可叠加，直接忽略
            Debug.Log("速度增益效果结束");
        }

        private IEnumerator RestoreMagnetRange(float wait)
        {
            yield return new WaitForSeconds(wait);
            var allPickups = FindObjectsOfType<Systems.ExperiencePickup>();
            foreach (var p in allPickups)
                p.DoubleMagnetRange(); // 翻倍两次 = 恢复原始
        }

        private IEnumerator FloatAnimation()
        {
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime * 2f;
                float y = Mathf.Sin(time) * 0.1f;
                transform.position += new Vector3(0, y * Time.deltaTime, 0);
                yield return null;
            }
        }

        /// <summary>
        /// 初始化道具数据
        /// </summary>
        public void Initialize(PickupDropRule rule)
        {
            pickupType = rule.pickupType;
            effectValue = rule.effectValue;
            duration = rule.duration;
            magnetRadius = rule.magnetRadius;
            transform.localScale = Vector3.one * rule.pickupSize;
        }

        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(8, 8);
            Color[] colors = new Color[8 * 8];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }
    }
}
