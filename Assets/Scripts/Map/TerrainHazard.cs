using UnityEngine;

namespace ClawSurvivor.Map
{
    /// <summary>
    /// 地形危险区域 - 水域减速 / 岩浆伤害
    /// 挂载到水域和岩浆地面的BoxCollider2D（trigger）上
    /// </summary>
    public class TerrainHazard : MonoBehaviour
    {
        public TerrainType terrainType;
        public float damagePerSecond = 5f;
        public float slowMultiplier = 0.5f;

        private float damageInterval = 1f;
        private float damageTimer;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                ApplyEffect(other);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                switch (terrainType)
                {
                    case TerrainType.Water:
                        // 水域：持续减速（在PlayerController中处理）
                        // 这里通过发送消息通知PlayerController
                        other.SendMessage("ApplyWaterSlow", slowMultiplier, SendMessageOptions.DontRequireReceiver);
                        break;

                    case TerrainType.Lava:
                        // 岩浆：持续伤害
                        damageTimer += Time.deltaTime;
                        if (damageTimer >= damageInterval)
                        {
                            damageTimer = 0f;
                            other.SendMessage("TakeDamage", Mathf.RoundToInt(damagePerSecond), SendMessageOptions.DontRequireReceiver);
                        }
                        break;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 离开危险区域时恢复速度
                if (terrainType == TerrainType.Water)
                {
                    other.SendMessage("RemoveWaterSlow", SendMessageOptions.DontRequireReceiver);
                }
                damageTimer = 0f;
            }
        }

        private void ApplyEffect(Collider2D target)
        {
            damageTimer = 0f;
        }
    }
}
