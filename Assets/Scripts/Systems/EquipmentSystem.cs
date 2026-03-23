using UnityEngine;
using ClawSurvivor.Weapons;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 装备升级系统 - 管理武器升级和被动装备
    /// </summary>
    public class EquipmentSystem : MonoBehaviour
    {
        public static EquipmentSystem Instance;

        [Header("装备设置")]
        [Tooltip("是否启用装备系统")]
        public bool equipmentEnabled = true;
        [Tooltip("最大同时装备武器数")]
        public int maxWeapons = 3;

        private Player.PlayerController player;
        private WeaponController weaponController;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            player = FindObjectOfType<Player.PlayerController>();
            weaponController = player?.GetComponent<WeaponController>();

            if (player != null && SaveSystem.Instance != null)
            {
                ApplyPermanentBonuses();
            }
        }

        /// <summary>
        /// 应用存档中的永久属性加成
        /// </summary>
        private void ApplyPermanentBonuses()
        {
            if (SaveSystem.Instance == null) return;

            var data = SaveSystem.Instance.CurrentData;
            if (data == null) return;

            // 永久生命加成
            if (data.bonusMaxHealth > 0)
            {
                player.AddMaxHealthBonus(data.bonusMaxHealth);
                Debug.Log($"[Equipment] 永久生命 +{data.bonusMaxHealth}");
            }

            // 永久伤害加成
            if (data.bonusDamage > 0)
            {
                player.AddDamageBonus(data.bonusDamage);
                Debug.Log($"[Equipment] 永久伤害 +{data.bonusDamage * 100}%");
            }

            // 永久速度加成
            if (data.bonusMoveSpeed > 0)
            {
                player.AddMoveSpeedBonus(data.bonusMoveSpeed);
                Debug.Log($"[Equipment] 永久移速 +{data.bonusMoveSpeed * 100}%");
            }

            // 永久经验加成
            if (data.bonusExpMultiplier > 1f)
            {
                player.SetExpBoostMultiplier = player.ExpBoostMultiplier * data.bonusExpMultiplier;
                Debug.Log($"[Equipment] 永久经验 x{data.bonusExpMultiplier}");
            }
        }

        /// <summary>
        /// 升级指定武器
        /// </summary>
        public bool UpgradeWeapon(string weaponName)
        {
            if (SaveSystem.Instance == null || weaponController == null) return false;

            int cost = SaveSystem.Instance.GetWeaponUpgradeCost(weaponName);
            if (SaveSystem.Instance.SpendCurrency(CurrencyType.WeaponFragments, cost))
            {
                int newLevel = SaveSystem.Instance.UpgradeWeapon(weaponName, 0);
                ApplyWeaponUpgrade(weaponName, newLevel);
                Debug.Log($"[Equipment] 武器升级: {weaponName} -> Lv.{newLevel}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 应用武器升级效果
        /// </summary>
        private void ApplyWeaponUpgrade(string weaponName, int level)
        {
            if (player == null) return;

            // 武器等级影响伤害
            float damageBonus = level * 0.1f; // 每级+10%
            player.AddDamageBonus(damageBonus);

            // 攻速略微提升
            float attackSpeedBonus = level * 0.02f;
            player.AddAttackSpeedBonus(attackSpeedBonus);
        }

        /// <summary>
        /// 获取武器等级
        /// </summary>
        public int GetWeaponLevel(string weaponName)
        {
            return SaveSystem.Instance?.GetWeaponLevel(weaponName) ?? 0;
        }

        /// <summary>
        /// 获取武器升级费用
        /// </summary>
        public int GetWeaponUpgradeCost(string weaponName)
        {
            return SaveSystem.Instance?.GetWeaponUpgradeCost(weaponName) ?? 0;
        }

        /// <summary>
        /// 切换武器
        /// </summary>
        public void SwitchWeapon(int weaponIndex)
        {
            if (weaponController == null) return;

            WeaponData[] allWeapons = WeaponData.AllWeapons;
            if (weaponIndex >= 0 && weaponIndex < allWeapons.Length)
            {
                weaponController.EquipWeapon(allWeapons[weaponIndex]);
            }
        }

        /// <summary>
        /// 获取所有武器数据
        /// </summary>
        public WeaponData[] GetAllWeapons()
        {
            return WeaponData.AllWeapons;
        }

        /// <summary>
        /// 检查是否可以装备更多武器
        /// </summary>
        public bool CanEquipMoreWeapons()
        {
            return true; // 简化逻辑，实际可根据maxWeapons限制
        }

        /// <summary>
        /// 刷新装备（游戏重新开始时调用）
        /// </summary>
        public void RefreshEquipment()
        {
            ApplyPermanentBonuses();
        }
    }
}
