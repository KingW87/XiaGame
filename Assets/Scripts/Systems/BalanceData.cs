using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 游戏平衡数值数据表 - 所有可调整的数值配置
    /// 数据可导出为Excel/CSV格式，方便策划调整
    /// </summary>
    [System.Serializable]
    public class BalanceData
    {
        [Header("玩家数值表")]
        public PlayerBalanceTable playerTable;

        [Header("敌人数值表")]
        public EnemyBalanceTable enemyTable;

        [Header("Boss数值表")]
        public BossBalanceTable bossTable;

        [Header("掉落概率表")]
        public DropRateTable dropRateTable;

        [Header("经验曲线表")]
        public ExperienceCurveTable expCurveTable;

        [Header("技能数值表")]
        public SkillBalanceTable skillTable;

        /// <summary>
        /// 默认平衡数据
        /// </summary>
        public static BalanceData GetDefault()
        {
            return new BalanceData
            {
                playerTable = PlayerBalanceTable.GetDefault(),
                enemyTable = EnemyBalanceTable.GetDefault(),
                bossTable = BossBalanceTable.GetDefault(),
                dropRateTable = DropRateTable.GetDefault(),
                expCurveTable = ExperienceCurveTable.GetDefault(),
                skillTable = SkillBalanceTable.GetDefault()
            };
        }
    }

    #region 玩家数值表

    [System.Serializable]
    public class PlayerBalanceTable
    {
        [Tooltip("初始最大生命值")]
        public int initialMaxHealth = 100;

        [Tooltip("初始移动速度")]
        public float initialMoveSpeed = 5f;

        [Tooltip("初始攻击伤害")]
        public int initialBaseDamage = 50;

        [Tooltip("初始攻击间隔（秒）")]
        public float initialAttackRate = 0.3f;

        [Tooltip("初始攻击范围")]
        public float initialAttackRange = 10f;

        [Tooltip("生命成长（每级）")]
        public int healthPerLevel = 10;

        [Tooltip("伤害成长（每级）")]
        public int damagePerLevel = 5;

        [Tooltip("无敌持续时间（秒）")]
        public float invincibilityDuration = 1f;

        [Tooltip("击退力度")]
        public float knockbackForce = 5f;

        public static PlayerBalanceTable GetDefault()
        {
            return new PlayerBalanceTable();
        }
    }

    #endregion

    #region 敌人数值表

    [System.Serializable]
    public class EnemyBalanceTable
    {
        [Tooltip("普通敌人配置")]
        public EnemyTypeBalance normal;

        [Tooltip("快速敌人配置")]
        public EnemyTypeBalance fast;

        [Tooltip("坦克敌人配置")]
        public EnemyTypeBalance tank;

        [Tooltip("远程敌人配置")]
        public EnemyTypeBalance ranged;

        [Tooltip("波次难度系数（每波）")]
        public float waveDifficultyMultiplier = 1.1f;

        public static EnemyBalanceTable GetDefault()
        {
            return new EnemyBalanceTable
            {
                normal = new EnemyTypeBalance
                {
                    typeName = "普通敌人",
                    health = 30,
                    damage = 10,
                    moveSpeed = 2f,
                    attackRate = 1f,
                    experienceValue = 5,
                    spawnWeight = 60
                },
                fast = new EnemyTypeBalance
                {
                    typeName = "快速敌人",
                    health = 15,
                    damage = 8,
                    moveSpeed = 4f,
                    attackRate = 0.5f,
                    experienceValue = 8,
                    spawnWeight = 20
                },
                tank = new EnemyTypeBalance
                {
                    typeName = "坦克敌人",
                    health = 100,
                    damage = 20,
                    moveSpeed = 1f,
                    attackRate = 2f,
                    experienceValue = 15,
                    spawnWeight = 15
                },
                ranged = new EnemyTypeBalance
                {
                    typeName = "远程敌人",
                    health = 25,
                    damage = 15,
                    moveSpeed = 1.5f,
                    attackRate = 1.5f,
                    experienceValue = 10,
                    spawnWeight = 5
                },
                waveDifficultyMultiplier = 1.1f
            };
        }
    }

    [System.Serializable]
    public class EnemyTypeBalance
    {
        [Tooltip("类型名称")]
        public string typeName;

        [Tooltip("生命值")]
        public int health;

        [Tooltip("伤害值")]
        public int damage;

        [Tooltip("移动速度")]
        public float moveSpeed;

        [Tooltip("攻击间隔（秒）")]
        public float attackRate;

        [Tooltip("经验值")]
        public int experienceValue;

        [Tooltip("生成权重（越大越容易生成）")]
        public int spawnWeight;
    }

    #endregion

    #region Boss数值表

    [System.Serializable]
    public class BossBalanceTable
    {
        [Tooltip("Boss基础生命值（每波）")]
        public int baseHealth = 500;

        [Tooltip("Boss基础伤害")]
        public int baseDamage = 30;

        [Tooltip("Boss移动速度")]
        public float moveSpeed = 1.5f;

        [Tooltip("Boss攻击间隔")]
        public float attackRate = 2f;

        [Tooltip("生命成长（每波）")]
        public int healthPerWave = 200;

        [Tooltip("伤害成长（每波）")]
        public int damagePerWave = 5;

        [Tooltip("经验值")]
        public int experienceValue = 500;

        [Tooltip("掉落金币")]
        public int goldDrop = 100;

        [Tooltip("每几波出现Boss")]
        public int wavesPerBoss = 10;

        public static BossBalanceTable GetDefault()
        {
            return new BossBalanceTable();
        }
    }

    #endregion

    #region 掉落概率表

    [System.Serializable]
    public class DropRateTable
    {
        [Tooltip("普通敌人掉落概率")]
        public DropRate normalEnemyDrop;

        [Tooltip("Boss掉落概率")]
        public DropRate bossDrop;

        [Tooltip("道具类型概率")]
        public List<PickupTypeRate> pickupTypeRates = new List<PickupTypeRate>();

        public static DropRateTable GetDefault()
        {
            return new DropRateTable
            {
                normalEnemyDrop = new DropRate
                {
                    dropChance = 0.3f,  // 30%概率掉落
                    minItems = 1,
                    maxItems = 1
                },
                bossDrop = new DropRate
                {
                    dropChance = 1f,    // 100%掉落
                    minItems = 3,
                    maxItems = 5
                },
                pickupTypeRates = new List<PickupTypeRate>
                {
                    new PickupTypeRate { type = "HealthRegen", name = "生命恢复", weight = 30 },
                    new PickupTypeRate { type = "SpeedBoost", name = "速度增益", weight = 20 },
                    new PickupTypeRate { type = "Shield", name = "护盾", weight = 15 },
                    new PickupTypeRate { type = "ExpBoost", name = "经验加倍", weight = 20 },
                    new PickupTypeRate { type = "Magnet", name = "磁力增强", weight = 10 },
                    new PickupTypeRate { type = "Bomb", name = "炸弹", weight = 5 }
                }
            };
        }
    }

    [System.Serializable]
    public class DropRate
    {
        [Tooltip("掉落概率（0-1）")]
        public float dropChance;

        [Tooltip("最少掉落数量")]
        public int minItems;

        [Tooltip("最多掉落数量")]
        public int maxItems;
    }

    [System.Serializable]
    public class PickupTypeRate
    {
        [Tooltip("道具类型")]
        public string type;

        [Tooltip("显示名称")]
        public string name;

        [Tooltip("权重（越大越容易掉落）")]
        public int weight;
    }

    #endregion

    #region 经验曲线表

    [System.Serializable]
    public class ExperienceCurveTable
    {
        [Tooltip("初始升级所需经验")]
        public int baseExpToLevelUp = 10;

        [Tooltip("升级所需经验成长系数")]
        public float expGrowthMultiplier = 1.5f;

        [Tooltip("每级额外经验加成（百分比）")]
        public float expBonusPerLevel = 0.1f;

        [Tooltip("各等级所需经验（可选，优先使用公式）")]
        public List<int> expByLevel = new List<int>();

        public static ExperienceCurveTable GetDefault()
        {
            return new ExperienceCurveTable
            {
                baseExpToLevelUp = 10,
                expGrowthMultiplier = 1.5f,
                expBonusPerLevel = 0.1f
            };
        }

        /// <summary>
        /// 计算指定等级升级所需经验
        /// </summary>
        public int GetExpForLevel(int level)
        {
            if (level <= 1) return baseExpToLevelUp;

            // 使用公式计算
            float exp = baseExpToLevelUp;
            for (int i = 2; i <= level; i++)
            {
                exp *= expGrowthMultiplier;
                exp *= (1 + expBonusPerLevel * (i - 1));
            }
            return Mathf.RoundToInt(exp);
        }
    }

    #endregion

    #region 技能数值表

    [System.Serializable]
    public class SkillBalanceTable
    {
        [Tooltip("攻击类技能配置")]
        public List<SkillBalance> attackSkills;

        [Tooltip("防御类技能配置")]
        public List<SkillBalance> defenseSkills;

        [Tooltip("辅助类技能配置")]
        public List<SkillBalance> utilitySkills;

        public static SkillBalanceTable GetDefault()
        {
            return new SkillBalanceTable
            {
                attackSkills = new List<SkillBalance>
                {
                    new SkillBalance { skillId = "slash", skillName = "挥砍", baseDamage = 50, damagePerLevel = 15, cooldown = 3f, description = "向前方挥砍造成伤害" },
                    new SkillBalance { skillId = "multishot", skillName = "多重箭", baseDamage = 30, damagePerLevel = 10, cooldown = 5f, projectileCount = 3, description = "发射多支箭矢" },
                    new SkillBalance { skillId = "aoe_explosion", skillName = "范围爆炸", baseDamage = 80, damagePerLevel = 20, cooldown = 8f, areaRadius = 5f, description = "在周围造成爆炸伤害" }
                },
                defenseSkills = new List<SkillBalance>
                {
                    new SkillBalance { skillId = "shield", skillName = "护盾", baseValue = 50, valuePerLevel = 20, cooldown = 10f, duration = 5f, description = "获得护盾吸收伤害" },
                    new SkillBalance { skillId = "heal", skillName = "治疗", baseValue = 30, valuePerLevel = 10, cooldown = 15f, description = "恢复生命值" }
                },
                utilitySkills = new List<SkillBalance>
                {
                    new SkillBalance { skillId = "speed_boost", skillName = "加速", baseValue = 1.5f, valuePerLevel = 0.1f, cooldown = 12f, duration = 5f, description = "提升移动速度" },
                    new SkillBalance { skillId = "exp_boost", skillName = "经验加倍", baseValue = 2f, cooldown = 30f, duration = 10f, description = "提升获取经验" },
                    new SkillBalance { skillId = "magnet", skillName = "磁力", baseValue = 3f, cooldown = 20f, duration = 10f, description = "增加道具拾取范围" }
                }
            };
        }
    }

    [System.Serializable]
    public class SkillBalance
    {
        [Tooltip("技能ID")]
        public string skillId;

        [Tooltip("技能名称")]
        public string skillName;

        [Tooltip("技能描述")]
        public string description;

        [Tooltip("基础伤害")]
        public int baseDamage;

        [Tooltip("每级伤害加成")]
        public int damagePerLevel;

        [Tooltip("基础数值（如治疗量、护盾值）")]
        public float baseValue;

        [Tooltip("每级数值加成")]
        public float valuePerLevel;

        [Tooltip("冷却时间（秒）")]
        public float cooldown;

        [Tooltip("持续时间（秒）")]
        public float duration;

        [Tooltip("作用范围")]
        public float areaRadius;

        [Tooltip("投射物数量")]
        public int projectileCount;
    }

    #endregion
}
