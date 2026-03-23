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
    /// 武器数据
    /// </summary>
    public class WeaponData
    {
        public string weaponName;
        public WeaponType type;
        public int baseDamage;
        public float attackSpeed;
        public float range;
        public float projectileSpeed;
        public int projectileCount;
        public Color color;

        public static WeaponData[] AllWeapons => new WeaponData[]
        {
            new WeaponData { weaponName = "短剑", type = WeaponType.Melee, baseDamage = 15, attackSpeed = 0.4f, range = 2f, color = Color.white },
            new WeaponData { weaponName = "手枪", type = WeaponType.Projectile, baseDamage = 10, attackSpeed = 0.6f, range = 15f, projectileSpeed = 12f, projectileCount = 1, color = new Color(0.8f, 0.8f, 0.2f) },
            new WeaponData { weaponName = "散弹", type = WeaponType.Projectile, baseDamage = 8, attackSpeed = 1.2f, range = 8f, projectileSpeed = 10f, projectileCount = 3, color = new Color(0.8f, 0.5f, 0.2f) },
            new WeaponData { weaponName = "回旋镖", type = WeaponType.Orbital, baseDamage = 20, attackSpeed = 0f, range = 3f, color = new Color(0.2f, 0.8f, 0.8f) },
            new WeaponData { weaponName = "长矛", type = WeaponType.Melee, baseDamage = 25, attackSpeed = 0.8f, range = 3.5f, color = new Color(0.6f, 0.6f, 0.7f) },
        };
    }
}
