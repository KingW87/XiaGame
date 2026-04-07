using UnityEngine;

namespace ClawSurvivor.Effects
{
    /// <summary>
    /// 特效类型枚举
    /// </summary>
    public enum EffectType
    {
        // 玩家特效
        PlayerAttack,          // 玩家攻击
        PlayerHit,             // 玩家受伤
        PlayerDeath,           // 玩家死亡
        PlayerLevelUp,         // 玩家升级
        PlayerHeal,            // 玩家回血
        PlayerShield,          // 获得护盾
        PlayerSpeedBoost,      // 速度增益
        PlayerExpBoost,        // 经验增益

        // 武器特效
        WeaponSlash,           // 挥砍
        WeaponProjectile,      // 投射物
        WeaponArea,            // 范围攻击

        // 敌人特效
        EnemyHit,              // 敌人受伤
        EnemyDeath,            // 敌人死亡
        EnemySpawn,            // 敌人生成

        // Boss特效
        BossAppear,            // Boss出现
        BossDeath,             // Boss死亡
        BossSkill,             // Boss技能

        // 道具特效
        PickupExp,             // 拾取经验
        PickupHealth,          // 拾取血包
        PickupShield,          // 拾取护盾
        PickupSpeed,           // 拾取速度
        PickupBomb,            // 拾取炸弹

        // 场景特效
        Explosion,             // 爆炸
        LevelUpParticle        // 升级粒子
    }

    /// <summary>
    /// 特效数据配置 - 用于在Inspector中配置特效
    /// </summary>
    [System.Serializable]
    public class EffectData
    {
        [Tooltip("特效类型")]
        public EffectType effectType;

        [Tooltip("特效预制体（不填则使用默认特效）")]
        public GameObject effectPrefab;

        [Tooltip("特效持续时间（秒）")]
        public float duration = 1f;

        [Tooltip("特效缩放")]
        public float scale = 1f;

        [Tooltip("是否随时间消失")]
        public bool fadeOut = true;
    }
}
