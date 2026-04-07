using UnityEngine;

namespace ClawSurvivor.Weapons
{
    /// <summary>
    /// 武器类型
    /// </summary>
    public enum WeaponType
    {
        Melee,    // 近战：近距离扇形攻击
        Projectile, // 投射物：发射子弹
        Orbital   // 环绕：围绕玩家旋转
    }

    /// <summary>
    /// 武器稀有度
    /// </summary>
    public enum WeaponRarity
    {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,      // 稀有
        Epic,      // 史诗
        Legendary  // 传说
    }

    /// <summary>
    /// 武器数据
    /// </summary>
    [System.Serializable]
    public class WeaponData
    {
        public string weaponName;
        public WeaponType type;
        public WeaponRarity rarity;
        public int baseDamage;
        public float attackSpeed;
        public float range;
        public float projectileSpeed;
        public int projectileCount;
        public Color color;
        
        // 升级相关
        public int maxLevel = 5;           // 最大等级
        public float damagePerLevel = 0.2f; // 每级伤害提升百分比
        public float speedPerLevel = 0.1f;  // 每级攻速提升百分比
        
        // 进化相关
        public bool canEvolve;              // 是否可以进化
        public int evolveMaterialId;       // 进化材料ID
        public int evolveMaterialCount;     // 进化所需材料数量
        public WeaponData evolvedWeapon;    // 进化后的武器

        /// <summary>
        /// 获取当前等级的伤害乘数
        /// </summary>
        public float GetDamageMultiplier(int level)
        {
            return 1f + damagePerLevel * (level - 1);
        }

        /// <summary>
        /// 获取当前等级的攻速乘数
        /// </summary>
        public float GetSpeedMultiplier(int level)
        {
            return 1f + speedPerLevel * (level - 1);
        }

        /// <summary>
        /// 克隆武器数据
        /// </summary>
        public WeaponData Clone()
        {
            return new WeaponData
            {
                weaponName = weaponName,
                type = type,
                rarity = rarity,
                baseDamage = baseDamage,
                attackSpeed = attackSpeed,
                range = range,
                projectileSpeed = projectileSpeed,
                projectileCount = projectileCount,
                color = color,
                maxLevel = maxLevel,
                damagePerLevel = damagePerLevel,
                speedPerLevel = speedPerLevel,
                canEvolve = canEvolve,
                evolveMaterialId = evolveMaterialId,
                evolveMaterialCount = evolveMaterialCount,
                evolvedWeapon = evolvedWeapon
            };
        }

        public static WeaponData[] AllWeapons => new WeaponData[]
        {
            // 普通武器
            new WeaponData { 
                weaponName = "短剑", 
                type = WeaponType.Melee, 
                rarity = WeaponRarity.Common,
                baseDamage = 15, 
                attackSpeed = 0.4f, 
                range = 2f, 
                color = Color.white,
                maxLevel = 5,
                damagePerLevel = 0.2f,
                speedPerLevel = 0.05f,
                canEvolve = true,
                evolveMaterialId = 1,
                evolveMaterialCount = 3
            },
            new WeaponData { 
                weaponName = "手枪", 
                type = WeaponType.Projectile, 
                rarity = WeaponRarity.Common,
                baseDamage = 10, 
                attackSpeed = 0.6f, 
                range = 15f, 
                projectileSpeed = 12f, 
                projectileCount = 1, 
                color = new Color(0.8f, 0.8f, 0.2f),
                maxLevel = 5,
                damagePerLevel = 0.2f,
                speedPerLevel = 0.08f,
                canEvolve = true,
                evolveMaterialId = 1,
                evolveMaterialCount = 3
            },
            new WeaponData { 
                weaponName = "散弹", 
                type = WeaponType.Projectile, 
                rarity = WeaponRarity.Uncommon,
                baseDamage = 8, 
                attackSpeed = 1.2f, 
                range = 8f, 
                projectileSpeed = 10f, 
                projectileCount = 3, 
                color = new Color(0.8f, 0.5f, 0.2f),
                maxLevel = 5,
                damagePerLevel = 0.25f,
                speedPerLevel = 0.1f,
                canEvolve = true,
                evolveMaterialId = 2,
                evolveMaterialCount = 3
            },
            new WeaponData { 
                weaponName = "回旋镖", 
                type = WeaponType.Orbital, 
                rarity = WeaponRarity.Uncommon,
                baseDamage = 20, 
                attackSpeed = 0f, 
                range = 3f, 
                color = new Color(0.2f, 0.8f, 0.8f),
                maxLevel = 5,
                damagePerLevel = 0.3f,
                speedPerLevel = 0f,
                canEvolve = true,
                evolveMaterialId = 3,
                evolveMaterialCount = 3
            },
            new WeaponData { 
                weaponName = "长矛", 
                type = WeaponType.Melee, 
                rarity = WeaponRarity.Rare,
                baseDamage = 25, 
                attackSpeed = 0.8f, 
                range = 3.5f, 
                color = new Color(0.6f, 0.6f, 0.7f),
                maxLevel = 6,
                damagePerLevel = 0.25f,
                speedPerLevel = 0.08f,
                canEvolve = true,
                evolveMaterialId = 4,
                evolveMaterialCount = 5
            },
        };

        /// <summary>
        /// 进化后的武器数据
        /// </summary>
        public static WeaponData[] EvolvedWeapons => new WeaponData[]
        {
            new WeaponData { 
                weaponName = "利刃风暴", 
                type = WeaponType.Melee, 
                rarity = WeaponRarity.Epic,
                baseDamage = 50, 
                attackSpeed = 0.25f, 
                range = 3.5f, 
                color = new Color(1f, 0.9f, 0.3f),
                maxLevel = 8,
                damagePerLevel = 0.3f,
                speedPerLevel = 0.1f,
                canEvolve = false
            },
            new WeaponData { 
                weaponName = "激光手枪", 
                type = WeaponType.Projectile, 
                rarity = WeaponRarity.Epic,
                baseDamage = 35, 
                attackSpeed = 0.3f, 
                range = 20f, 
                projectileSpeed = 20f, 
                projectileCount = 1, 
                color = new Color(1f, 0.2f, 0.2f),
                maxLevel = 8,
                damagePerLevel = 0.3f,
                speedPerLevel = 0.12f,
                canEvolve = false
            },
            new WeaponData { 
                weaponName = "榴弹炮", 
                type = WeaponType.Projectile, 
                rarity = WeaponRarity.Epic,
                baseDamage = 25, 
                attackSpeed = 1.0f, 
                range = 12f, 
                projectileSpeed = 8f, 
                projectileCount = 5, 
                color = new Color(1f, 0.5f, 0f),
                maxLevel = 8,
                damagePerLevel = 0.35f,
                speedPerLevel = 0.1f,
                canEvolve = false
            },
            new WeaponData { 
                weaponName = "能量光环", 
                type = WeaponType.Orbital, 
                rarity = WeaponRarity.Epic,
                baseDamage = 60, 
                attackSpeed = 0f, 
                range = 5f, 
                color = new Color(0.3f, 1f, 1f),
                maxLevel = 8,
                damagePerLevel = 0.4f,
                speedPerLevel = 0f,
                canEvolve = false
            },
            new WeaponData { 
                weaponName = "雷神之矛", 
                type = WeaponType.Melee, 
                rarity = WeaponRarity.Legendary,
                baseDamage = 80, 
                attackSpeed = 0.4f, 
                range = 5f, 
                color = new Color(1f, 0.8f, 0.2f),
                maxLevel = 10,
                damagePerLevel = 0.35f,
                speedPerLevel = 0.12f,
                canEvolve = false
            },
        };
    }
}
