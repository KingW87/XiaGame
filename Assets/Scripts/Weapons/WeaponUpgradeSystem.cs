using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Weapons
{
    /// <summary>
    /// 武器升级系统 - 管理武器升级和进化
    /// </summary>
    public class WeaponUpgradeSystem : MonoBehaviour
    {
        public static WeaponUpgradeSystem Instance { get; private set; }

        // 玩家当前武器信息
        private Dictionary<int, WeaponInfo> playerWeapons = new Dictionary<int, WeaponInfo>();

        [System.Serializable]
        public class WeaponInfo
        {
            public int weaponId;           // 武器ID（在AllWeapons数组中的索引）
            public int level = 1;          // 当前等级
            public bool isEvolved;         // 是否已进化
            public int evolveMaterials;    // 已拥有的进化材料数量
        }

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

        /// <summary>
        /// 获取玩家武器信息
        /// </summary>
        public WeaponInfo GetWeaponInfo(int weaponId)
        {
            if (!playerWeapons.ContainsKey(weaponId))
            {
                playerWeapons[weaponId] = new WeaponInfo { weaponId = weaponId };
            }
            return playerWeapons[weaponId];
        }

        /// <summary>
        /// 升级武器
        /// </summary>
        public bool UpgradeWeapon(int weaponId)
        {
            var weapon = WeaponData.AllWeapons[weaponId];
            var info = GetWeaponInfo(weaponId);

            if (info.isEvolved)
            {
                Debug.LogWarning("已进化的武器无法升级");
                return false;
            }

            if (info.level >= weapon.maxLevel)
            {
                Debug.LogWarning($"武器已达到最大等级 {weapon.maxLevel}");
                return false;
            }

            // 升级消耗（可以用金币或材料）
            info.level++;
            Debug.Log($"武器升级成功: {weapon.weaponName} LV.{info.level}");
            return true;
        }

        /// <summary>
        /// 获取武器实际伤害（考虑等级加成）
        /// </summary>
        public int GetActualDamage(int weaponId)
        {
            var weapon = WeaponData.AllWeapons[weaponId];
            var info = GetWeaponInfo(weaponId);
            
            if (info.isEvolved && weapon.evolvedWeapon != null)
            {
                return Mathf.RoundToInt(weapon.evolvedWeapon.baseDamage * weapon.evolvedWeapon.GetDamageMultiplier(info.level));
            }
            
            return Mathf.RoundToInt(weapon.baseDamage * weapon.GetDamageMultiplier(info.level));
        }

        /// <summary>
        /// 获取武器实际攻速（考虑等级加成）
        /// </summary>
        public float GetActualAttackSpeed(int weaponId)
        {
            var weapon = WeaponData.AllWeapons[weaponId];
            var info = GetWeaponInfo(weaponId);
            
            float baseSpeed = weapon.attackSpeed;
            float multiplier = weapon.GetSpeedMultiplier(info.level);
            
            if (info.isEvolved && weapon.evolvedWeapon != null)
            {
                baseSpeed = weapon.evolvedWeapon.attackSpeed;
                multiplier = weapon.evolvedWeapon.GetSpeedMultiplier(info.level);
            }
            
            return baseSpeed / multiplier;
        }

        /// <summary>
        /// 收集进化材料
        /// </summary>
        public void CollectEvolveMaterial(int materialId, int count)
        {
            foreach (var kvp in playerWeapons)
            {
                var weapon = WeaponData.AllWeapons[kvp.Key];
                if (weapon.evolveMaterialId == materialId)
                {
                    kvp.Value.evolveMaterials += count;
                    Debug.Log($"收集进化材料: {materialId} x{count} (当前: {kvp.Value.evolveMaterials}/{weapon.evolveMaterialCount})");
                }
            }
        }

        /// <summary>
        /// 进化武器
        /// </summary>
        public bool EvolveWeapon(int weaponId)
        {
            var weapon = WeaponData.AllWeapons[weaponId];
            var info = GetWeaponInfo(weaponId);

            if (!weapon.canEvolve)
            {
                Debug.LogWarning("该武器无法进化");
                return false;
            }

            if (info.isEvolved)
            {
                Debug.LogWarning("该武器已经进化");
                return false;
            }

            if (info.evolveMaterials < weapon.evolveMaterialCount)
            {
                Debug.LogWarning($"进化材料不足: {info.evolveMaterials}/{weapon.evolveMaterialCount}");
                return false;
            }

            // 扣除材料并进化
            info.evolveMaterials -= weapon.evolveMaterialCount;
            info.isEvolved = true;
            Debug.Log($"武器进化成功: {weapon.weaponName} → {weapon.evolvedWeapon.weaponName}!");
            return true;
        }

        /// <summary>
        /// 检查武器是否可以升级
        /// </summary>
        public bool CanUpgrade(int weaponId)
        {
            var weapon = WeaponData.AllWeapons[weaponId];
            var info = GetWeaponInfo(weaponId);
            return !info.isEvolved && info.level < weapon.maxLevel;
        }

        /// <summary>
        /// 检查武器是否可以进化
        /// </summary>
        public bool CanEvolve(int weaponId)
        {
            var weapon = WeaponData.AllWeapons[weaponId];
            var info = GetWeaponInfo(weaponId);
            return weapon.canEvolve && !info.isEvolved && info.evolveMaterials >= weapon.evolveMaterialCount;
        }

        /// <summary>
        /// 获取升级消耗
        /// </summary>
        public int GetUpgradeCost(int weaponId)
        {
            var info = GetWeaponInfo(weaponId);
            return info.level * 100; // 每级100金币
        }

        /// <summary>
        /// 存档数据
        /// </summary>
        public string Serialize()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var kvp in playerWeapons)
            {
                sb.Append($"{kvp.Key},{kvp.Value.level},{kvp.Value.isEvolved},{kvp.Value.evolveMaterials};");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 读档数据
        /// </summary>
        public void Deserialize(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            
            playerWeapons.Clear();
            string[] weapons = data.Split(';');
            foreach (var w in weapons)
            {
                if (string.IsNullOrEmpty(w)) continue;
                string[] parts = w.Split(',');
                if (parts.Length >= 4)
                {
                    int id = int.Parse(parts[0]);
                    var info = new WeaponInfo
                    {
                        weaponId = id,
                        level = int.Parse(parts[1]),
                        isEvolved = bool.Parse(parts[2]),
                        evolveMaterials = int.Parse(parts[3])
                    };
                    playerWeapons[id] = info;
                }
            }
        }
    }
}
