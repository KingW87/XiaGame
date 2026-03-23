using UnityEngine;

namespace ClawSurvivor.Pickups
{
    /// <summary>
    /// 道具类型枚举
    /// </summary>
    public enum PickupType
    {
        HealthRegen,   // 生命恢复：立刻恢复一定血量
        SpeedBoost,    // 速度增益：临时提升移动速度
        Shield,        // 护盾：获得可吸收伤害的护盾
        ExpBoost,      // 经验加倍：临时提升经验获取
        Magnet,        // 磁力增强：临时扩大吸附范围
        Bomb           // 炸弹清屏：消灭屏幕范围内所有普通敌人
    }

    /// <summary>
    /// 道具掉落规则 - 可在Inspector中配置
    /// </summary>
    [System.Serializable]
    public class PickupDropRule
    {
        [Tooltip("道具类型")]
        public PickupType pickupType;
        [Tooltip("掉落概率（0~1）")]
        [Range(0f, 1f)] public float dropChance = 0.1f;
        [Tooltip("效果数值（不同类型含义不同）")]
        public float effectValue = 10f;
        [Tooltip("持续时间（秒），0表示即时效果")]
        public float duration = 10f;
        [Tooltip("道具颜色")]
        public Color pickupColor = Color.white;
        [Tooltip("道具大小")]
        public float pickupSize = 0.6f;
        [Tooltip("吸附范围")]
        public float magnetRadius = 3f;
    }

    /// <summary>
    /// 每种道具的默认配置
    /// </summary>
    public static class PickupDefaults
    {
        public static readonly PickupDropRule[] DefaultRules = new PickupDropRule[]
        {
            new PickupDropRule { pickupType = PickupType.HealthRegen, dropChance = 0.15f, effectValue = 20f, duration = 0f, pickupColor = new Color(0.2f, 1f, 0.3f), pickupSize = 0.7f },
            new PickupDropRule { pickupType = PickupType.SpeedBoost, dropChance = 0.08f, effectValue = 0.5f, duration = 8f, pickupColor = new Color(0.3f, 0.7f, 1f), pickupSize = 0.5f },
            new PickupDropRule { pickupType = PickupType.Shield, dropChance = 0.06f, effectValue = 30f, duration = 0f, pickupColor = new Color(0.5f, 0.5f, 1f), pickupSize = 0.65f },
            new PickupDropRule { pickupType = PickupType.ExpBoost, dropChance = 0.05f, effectValue = 2f, duration = 15f, pickupColor = new Color(1f, 0.85f, 0.2f), pickupSize = 0.55f },
            new PickupDropRule { pickupType = PickupType.Magnet, dropChance = 0.07f, effectValue = 2f, duration = 12f, pickupColor = new Color(0.8f, 0.4f, 1f), pickupSize = 0.5f },
            new PickupDropRule { pickupType = PickupType.Bomb, dropChance = 0.03f, effectValue = 50f, duration = 0f, pickupColor = new Color(1f, 0.3f, 0.1f), pickupSize = 0.8f },
        };
    }
}
