using System;
using UnityEngine;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 货币类型枚举
    /// </summary>
    public enum CurrencyType
    {
        Gold,              // 金币 - 局内击杀掉落，用于升级武器/购买局内道具
        Gems,              // 宝石 - Boss掉落/结算奖励，用于局外解锁角色/强力增益
        WeaponFragments,   // 装备碎片 - 击杀掉落，用于装备升级
        PetFragments,      // 宠物碎片 - 击杀/Boss掉落，用于宠物解锁/升级
        SkillBooks         // 技能书 - 结算奖励，用于解锁新技能
    }

    /// <summary>
    /// 存档数据 - 所有持久化数据集中管理
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // === 货币 ===
        [Tooltip("金币（局内+局外累计）")]
        public int gold;
        [Tooltip("宝石（局外累计）")]
        public int gems;
        [Tooltip("装备碎片")]
        public int weaponFragments;
        [Tooltip("宠物碎片")]
        public int petFragments;
        [Tooltip("技能书")]
        public int skillBooks;

        // === 装备升级 ===
        [Tooltip("各武器升级等级（key=武器名, value=等级）")]
        public SerializableDictionary weaponLevels = new SerializableDictionary();

        // === 宠物 ===
        [Tooltip("已解锁宠物ID列表")]
        public SerializableIntArray unlockedPets = new SerializableIntArray();
        [Tooltip("各宠物升级等级（key=宠物ID, value=等级）")]
        public SerializableDictionary petLevels = new SerializableDictionary();

        // === 技能解锁 ===
        [Tooltip("已解锁技能索引列表")]
        public SerializableIntArray unlockedSkills = new SerializableIntArray();

        // === 全局属性（局外永久增益）===
        [Tooltip("永久生命加成")]
        public int bonusMaxHealth;
        [Tooltip("永久伤害加成")]
        public float bonusDamage;
        [Tooltip("永久速度加成")]
        public float bonusMoveSpeed;
        [Tooltip("永久经验加成")]
        public float bonusExpMultiplier = 1f;

        // === 统计 ===
        [Tooltip("总游戏次数")]
        public int totalGamesPlayed;
        [Tooltip("最高等级")]
        public int highestLevel;
        [Tooltip("最长存活时间（秒）")]
        public float longestSurvivalTime;
        [Tooltip("总击杀数")]
        public int totalKills;

        public SaveData()
        {
            gold = 0;
            gems = 0;
            weaponFragments = 0;
            petFragments = 0;
            skillBooks = 0;
            weaponLevels = new SerializableDictionary();
            unlockedPets = new SerializableIntArray();
            petLevels = new SerializableDictionary();
            unlockedSkills = new SerializableIntArray();
            bonusMaxHealth = 0;
            bonusDamage = 0f;
            bonusMoveSpeed = 0f;
            bonusExpMultiplier = 1f;
            totalGamesPlayed = 0;
            highestLevel = 0;
            longestSurvivalTime = 0f;
            totalKills = 0;
        }

        /// <summary>
        /// 获取指定类型的货币数量
        /// </summary>
        public int GetCurrency(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Gold: return gold;
                case CurrencyType.Gems: return gems;
                case CurrencyType.WeaponFragments: return weaponFragments;
                case CurrencyType.PetFragments: return petFragments;
                case CurrencyType.SkillBooks: return skillBooks;
                default: return 0;
            }
        }

        /// <summary>
        /// 增加指定类型的货币
        /// </summary>
        public void AddCurrency(CurrencyType type, int amount)
        {
            switch (type)
            {
                case CurrencyType.Gold: gold += amount; break;
                case CurrencyType.Gems: gems += amount; break;
                case CurrencyType.WeaponFragments: weaponFragments += amount; break;
                case CurrencyType.PetFragments: petFragments += amount; break;
                case CurrencyType.SkillBooks: skillBooks += amount; break;
            }
        }

        /// <summary>
        /// 消耗指定类型的货币，成功返回true
        /// </summary>
        public bool SpendCurrency(CurrencyType type, int amount)
        {
            if (GetCurrency(type) < amount) return false;
            switch (type)
            {
                case CurrencyType.Gold: gold -= amount; break;
                case CurrencyType.Gems: gems -= amount; break;
                case CurrencyType.WeaponFragments: weaponFragments -= amount; break;
                case CurrencyType.PetFragments: petFragments -= amount; break;
                case CurrencyType.SkillBooks: skillBooks -= amount; break;
            }
            return true;
        }
    }

    /// <summary>
    /// 可序列化字典 - 用于存档存储键值对
    /// </summary>
    [Serializable]
    public class SerializableDictionaryItem
    {
        [Tooltip("键")]
        public string key;
        [Tooltip("值")]
        public int value;
    }

    [Serializable]
    public class SerializableDictionary
    {
        [Tooltip("键值对列表")]
        public SerializableDictionaryItem[] items = new SerializableDictionaryItem[0];

        public int GetValue(string key, int defaultValue = 0)
        {
            if (items == null) return defaultValue;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].key == key) return items[i].value;
            }
            return defaultValue;
        }

        public void SetValue(string key, int value)
        {
            if (items == null) items = new SerializableDictionaryItem[0];

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].key == key)
                {
                    items[i].value = value;
                    return;
                }
            }

            ArrayAppend(ref items, new SerializableDictionaryItem { key = key, value = value });
        }

        private static void ArrayAppend(ref SerializableDictionaryItem[] arr, SerializableDictionaryItem item)
        {
            var newArr = new SerializableDictionaryItem[arr.Length + 1];
            arr.CopyTo(newArr, 0);
            newArr[arr.Length] = item;
            arr = newArr;
        }

        public string[] GetAllKeys()
        {
            if (items == null) return new string[0];
            string[] keys = new string[items.Length];
            for (int i = 0; i < items.Length; i++) keys[i] = items[i].key;
            return keys;
        }
    }

    /// <summary>
    /// 可序列化int数组
    /// </summary>
    [Serializable]
    public class SerializableIntArray
    {
        [Tooltip("整数列表")]
        public int[] items = new int[0];

        public bool Contains(int value)
        {
            if (items == null) return false;
            for (int i = 0; i < items.Length; i++)
                if (items[i] == value) return true;
            return false;
        }

        public void Add(int value)
        {
            if (Contains(value)) return;
            if (items == null) items = new int[0];
            var newArr = new int[items.Length + 1];
            items.CopyTo(newArr, 0);
            newArr[items.Length] = value;
            items = newArr;
        }
    }
}
