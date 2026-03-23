using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Pickups
{
    /// <summary>
    /// 道具掉落管理器 - 控制道具的生成和掉落概率
    /// </summary>
    public class PickupSpawner : MonoBehaviour
    {
        public static PickupSpawner Instance;

        [Header("掉落规则")]
        [Tooltip("普通敌人掉落规则列表")]
        public List<PickupDropRule> normalDropRules = new List<PickupDropRule>();

        [Header("Boss掉落")]
        [Tooltip("Boss额外掉落规则")]
        public List<PickupDropRule> bossDropRules = new List<PickupDropRule>();
        [Tooltip("Boss掉落数量")]
        [Range(1, 5)] public int bossDropCount = 2;
        [Tooltip("Boss掉落概率倍率（Boss更容易掉落道具）")]
        [Range(1f, 3f)] public float bossDropChanceMultiplier = 2f;

        [Header("全局设置")]
        [Tooltip("是否启用道具掉落")]
        public bool enableDrops = true;
        [Tooltip("道具存在时间（秒）")]
        public float defaultLifetime = 30f;

        private void Awake()
        {
            Instance = this;

            // 如果没有配置规则，使用默认规则
            if (normalDropRules.Count == 0)
            {
                foreach (var rule in PickupDefaults.DefaultRules)
                    normalDropRules.Add(rule);
            }

            if (bossDropRules.Count == 0)
            {
                // Boss掉落高质量道具（恢复和护盾概率更高）
                bossDropRules.Add(new PickupDropRule
                {
                    pickupType = PickupType.HealthRegen, dropChance = 0.5f, effectValue = 50f,
                    duration = 0f, pickupColor = new Color(0.2f, 1f, 0.3f), pickupSize = 0.9f
                });
                bossDropRules.Add(new PickupDropRule
                {
                    pickupType = PickupType.Shield, dropChance = 0.4f, effectValue = 80f,
                    duration = 0f, pickupColor = new Color(0.5f, 0.5f, 1f), pickupSize = 0.85f
                });
                bossDropRules.Add(new PickupDropRule
                {
                    pickupType = PickupType.SpeedBoost, dropChance = 0.3f, effectValue = 0.8f,
                    duration = 12f, pickupColor = new Color(0.3f, 0.7f, 1f), pickupSize = 0.7f
                });
                bossDropRules.Add(new PickupDropRule
                {
                    pickupType = PickupType.Bomb, dropChance = 0.2f, effectValue = 80f,
                    duration = 0f, pickupColor = new Color(1f, 0.3f, 0.1f), pickupSize = 1f
                });
            }
        }

        /// <summary>
        /// 普通敌人死亡时尝试掉落道具
        /// </summary>
        public void TryDropItem(Vector3 position)
        {
            if (!enableDrops || normalDropRules.Count == 0) return;

            foreach (var rule in normalDropRules)
            {
                if (Random.Range(0f, 1f) <= rule.dropChance)
                {
                    SpawnPickup(position, rule);
                    return; // 最多掉落一个道具
                }
            }
        }

        /// <summary>
        /// Boss死亡时掉落多个高质量道具
        /// </summary>
        public void DropBossItems(Vector3 position)
        {
            if (!enableDrops || bossDropRules.Count == 0) return;

            int dropped = 0;
            foreach (var rule in bossDropRules)
            {
                if (dropped >= bossDropCount) break;

                float chance = Mathf.Clamp01(rule.dropChance * bossDropChanceMultiplier);
                if (Random.Range(0f, 1f) <= chance)
                {
                    // 在Boss周围随机位置掉落
                    Vector3 offset = Random.insideUnitCircle * 2f;
                    SpawnPickup(position + offset, rule);
                    dropped++;
                }
            }
        }

        /// <summary>
        /// 在指定位置生成一个道具
        /// </summary>
        public void SpawnPickup(Vector3 position, PickupDropRule rule)
        {
            GameObject pickup = new GameObject($"Pickup_{rule.pickupType}");
            pickup.transform.position = position;

            SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = rule.pickupColor;
            sr.sortingOrder = 10;
            pickup.transform.localScale = Vector3.one * rule.pickupSize;

            pickup.AddComponent<BoxCollider2D>().isTrigger = true;
            ItemPickup item = pickup.AddComponent<ItemPickup>();
            item.Initialize(rule);
            item.lifetime = defaultLifetime;

            Destroy(pickup, defaultLifetime + 1f);
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
