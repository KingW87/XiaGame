using UnityEngine;
using System.Collections.Generic;
using ClawSurvivor.Player;
using ClawSurvivor.Weapons;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 商店物品类型
    /// </summary>
    public enum ShopItemType
    {
        Weapon,           // 武器升级
        Pet,              // 宠物
        PassiveItem,      // 被动道具
        GemUpgrade        // 宝石购买永久属性
    }

    /// <summary>
    /// 商店物品数据
    /// </summary>
    [System.Serializable]
    public class ShopItemData
    {
        [Tooltip("物品ID")]
        public string itemId;
        [Tooltip("物品名称")]
        public string itemName;
        [Tooltip("物品类型")]
        public ShopItemType itemType;
        [Tooltip("描述")]
        public string description;
        [Tooltip("价格（金币）")]
        public int goldPrice;
        [Tooltip("价格（宝石）")]
        public int gemPrice;
        [Tooltip("图标")]
        public Sprite icon;
        [Tooltip("是否已解锁")]
        public bool isUnlocked;
        [Tooltip("当前等级")]
        public int currentLevel;
        [Tooltip("最大等级")]
        public int maxLevel = 10;
    }

    /// <summary>
    /// 商店管理器 - 管理局内金币商店和局外宝石商店
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        [Header("商店设置")]
        [Tooltip("是否启用商店")]
        public bool shopEnabled = true;

        [Header("商店UI引用")]
        [Tooltip("金币商店UI")]
        public GameObject goldShopUI;
        [Tooltip("宝石商店UI")]
        public GameObject gemShopUI;

        private List<ShopItemData> weaponShopItems = new List<ShopItemData>();
        private List<ShopItemData> petShopItems = new List<ShopItemData>();
        private List<ShopItemData> passiveShopItems = new List<ShopItemData>();
        private List<ShopItemData> gemUpgradeItems = new List<ShopItemData>();

        private bool isGoldShopOpen;
        private bool isGemShopOpen;

        public bool IsGoldShopOpen => isGoldShopOpen;
        public bool IsGemShopOpen => isGemShopOpen;

        public event System.Action OnShopOpened;
        public event System.Action OnShopClosed;
        public event System.Action<ShopItemData> OnItemPurchased;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeShopItems();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeShopItems()
        {
            // 初始化武器商店
            WeaponData[] weapons = WeaponData.AllWeapons;
            for (int i = 0; i < weapons.Length; i++)
            {
                var weapon = weapons[i];
                weaponShopItems.Add(new ShopItemData
                {
                    itemId = $"weapon_{i}",
                    itemName = weapon.weaponName,
                    itemType = ShopItemType.Weapon,
                    description = $"升级{weapon.weaponName}的伤害和攻速",
                    goldPrice = (i + 1) * 10,
                    gemPrice = 0,
                    isUnlocked = i == 0, // 第一把武器默认解锁
                    currentLevel = 0,
                    maxLevel = 10
                });
            }

            // 初始化宠物商店
            string[] petNames = { "小精灵", "火焰猫", "冰霜狼", "雷电鹰", "暗影豹" };
            int[] petCosts = { 0, 50, 100, 200, 500 };
            for (int i = 0; i < petNames.Length; i++)
            {
                petShopItems.Add(new ShopItemData
                {
                    itemId = $"pet_{i}",
                    itemName = petNames[i],
                    itemType = ShopItemType.Pet,
                    description = $"解锁{petNames[i]}宠物，战斗时自动攻击",
                    goldPrice = 0,
                    gemPrice = petCosts[i],
                    isUnlocked = i == 0,
                    currentLevel = i == 0 ? 1 : 0,
                    maxLevel = 5
                });
            }

            // 初始化被动道具商店
            string[] passiveNames = { "生命之心", "力量戒指", "敏捷之靴", "经验宝珠", "幸运护符" };
            int[] passiveCosts = { 30, 40, 40, 50, 60 };
            for (int i = 0; i < passiveNames.Length; i++)
            {
                passiveShopItems.Add(new ShopItemData
                {
                    itemId = $"passive_{i}",
                    itemName = passiveNames[i],
                    itemType = ShopItemType.PassiveItem,
                    description = $"永久+{10 * (i + 1)}{GetPassiveStatType(i)}",
                    goldPrice = passiveCosts[i],
                    gemPrice = 0,
                    isUnlocked = false,
                    currentLevel = 0,
                    maxLevel = 5
                });
            }

            // 初始化宝石升级商店
            gemUpgradeItems.Add(new ShopItemData
            {
                itemId = "gem_health",
                itemName = "生命强化",
                itemType = ShopItemType.GemUpgrade,
                description = "永久+20最大生命值",
                goldPrice = 0,
                gemPrice = 10,
                isUnlocked = true,
                currentLevel = 0,
                maxLevel = 20
            });
            gemUpgradeItems.Add(new ShopItemData
            {
                itemId = "gem_damage",
                itemName = "伤害强化",
                itemType = ShopItemType.GemUpgrade,
                description = "永久+5%伤害",
                goldPrice = 0,
                gemPrice = 15,
                isUnlocked = true,
                currentLevel = 0,
                maxLevel = 20
            });
            gemUpgradeItems.Add(new ShopItemData
            {
                itemId = "gem_speed",
                itemName = "速度强化",
                itemType = ShopItemType.GemUpgrade,
                description = "永久+3%移动速度",
                goldPrice = 0,
                gemPrice = 12,
                isUnlocked = true,
                currentLevel = 0,
                maxLevel = 20
            });
            gemUpgradeItems.Add(new ShopItemData
            {
                itemId = "gem_exp",
                itemName = "经验强化",
                itemType = ShopItemType.GemUpgrade,
                description = "永久+5%经验获取",
                goldPrice = 0,
                gemPrice = 20,
                isUnlocked = true,
                currentLevel = 0,
                maxLevel = 10
            });

            Debug.Log($"[ShopManager] 商店初始化完成 | 武器:{weaponShopItems.Count} 宠物:{petShopItems.Count} 被动:{passiveShopItems.Count} 宝石升级:{gemUpgradeItems.Count}");
        }

        private string GetPassiveStatType(int index)
        {
            switch (index)
            {
                case 0: return "最大生命";
                case 1: return "攻击力";
                case 2: return "移速";
                case 3: return "经验";
                case 4: return "幸运";
                default: return "";
            }
        }

        // === 商店开关 ===

        /// <summary>
        /// 打开金币商店（局内）
        /// </summary>
        public void OpenGoldShop()
        {
            if (!shopEnabled) return;
            isGoldShopOpen = true;
            isGemShopOpen = false;
            Time.timeScale = 0; // 暂停游戏
            OnShopOpened?.Invoke();
            Debug.Log("[ShopManager] 金币商店已打开");
        }

        /// <summary>
        /// 关闭金币商店
        /// </summary>
        public void CloseGoldShop()
        {
            isGoldShopOpen = false;
            Time.timeScale = 1; // 恢复游戏
            OnShopClosed?.Invoke();
            Debug.Log("[ShopManager] 金币商店已关闭");
        }

        /// <summary>
        /// 打开宝石商店（局外主菜单）
        /// </summary>
        public void OpenGemShop()
        {
            if (!shopEnabled) return;
            isGemShopOpen = true;
            isGoldShopOpen = false;
            OnShopOpened?.Invoke();
            Debug.Log("[ShopManager] 宝石商店已打开");
        }

        /// <summary>
        /// 关闭宝石商店
        /// </summary>
        public void CloseGemShop()
        {
            isGemShopOpen = false;
            OnShopClosed?.Invoke();
            Debug.Log("[ShopManager] 宝石商店已关闭");
        }

        // === 购买逻辑 ===

        /// <summary>
        /// 购买物品
        /// </summary>
        public bool PurchaseItem(ShopItemData item)
        {
            if (item == null || item.currentLevel >= item.maxLevel)
            {
                Debug.Log("[ShopManager] 购买失败：物品不存在或已满级");
                return false;
            }

            // 根据商店类型判断使用哪种货币
            if (isGoldShopOpen)
            {
                // 局内金币商店
                if (SaveSystem.Instance == null || !SaveSystem.Instance.SpendSessionGold(item.goldPrice))
                {
                    Debug.Log($"[ShopManager] 金币不足！需要{item.goldPrice}，已有{SaveSystem.Instance?.GetSessionGold() ?? 0}");
                    return false;
                }
            }
            else if (isGemShopOpen)
            {
                // 局外宝石商店
                if (SaveSystem.Instance == null || !SaveSystem.Instance.SpendCurrency(CurrencyType.Gems, item.gemPrice))
                {
                    Debug.Log($"[ShopManager] 宝石不足！需要{item.gemPrice}，已有{SaveSystem.Instance?.GetCurrency(CurrencyType.Gems) ?? 0}");
                    return false;
                }
            }
            else
            {
                Debug.Log("[ShopManager] 购买失败：未打开任何商店");
                return false;
            }

            // 升级物品
            item.currentLevel++;

            // 应用效果
            ApplyPurchaseEffect(item);

            OnItemPurchased?.Invoke(item);
            Debug.Log($"[ShopManager] 购买成功：{item.itemName} Lv.{item.currentLevel}");
            return true;
        }

        private void ApplyPurchaseEffect(ShopItemData item)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null) return;

            switch (item.itemType)
            {
                case ShopItemType.Weapon:
                    // 武器升级 - 通过SaveSystem
                    Debug.Log($"[ShopManager] 武器升级：{item.itemName} Lv.{item.currentLevel}");
                    break;

                case ShopItemType.Pet:
                    // 宠物解锁/升级 - 通过SaveSystem
                    Debug.Log($"[ShopManager] 宠物：{item.itemName} Lv.{item.currentLevel}");
                    break;

                case ShopItemType.PassiveItem:
                    // 被动道具 - 立即应用属性加成
                    int statBonus = 10 * item.currentLevel;
                    switch (item.itemId)
                    {
                        case "passive_0": // 生命之心
                            player.AddMaxHealthBonus(statBonus);
                            break;
                        case "passive_1": // 力量戒指
                            player.AddDamageBonus(statBonus * 0.01f);
                            break;
                        case "passive_2": // 敏捷之靴
                            player.AddMoveSpeedBonus(statBonus * 0.01f);
                            break;
                        case "passive_3": // 经验宝珠
                            player.SetExpBoostMultiplier = player.ExpBoostMultiplier + statBonus * 0.01f;
                            break;
                        case "passive_4": // 幸运护符
                            // 暂时预留
                            break;
                    }
                    Debug.Log($"[ShopManager] 被动道具生效：{item.itemName} +{statBonus}%");
                    break;

                case ShopItemType.GemUpgrade:
                    // 宝石升级 - 立即应用永久属性
                    if (SaveSystem.Instance != null)
                    {
                        switch (item.itemId)
                        {
                            case "gem_health":
                                SaveSystem.Instance.BuyBonusHealth(item.gemPrice, 20);
                                player.AddMaxHealthBonus(20);
                                break;
                            case "gem_damage":
                                SaveSystem.Instance.BuyBonusDamage(item.gemPrice, 0.05f);
                                player.AddDamageBonus(0.05f);
                                break;
                            case "gem_speed":
                                SaveSystem.Instance.BuyBonusSpeed(item.gemPrice, 0.03f);
                                player.AddMoveSpeedBonus(0.03f);
                                break;
                            case "gem_exp":
                                SaveSystem.Instance.BuyBonusExp(item.gemPrice, 0.05f);
                                player.SetExpBoostMultiplier = player.ExpBoostMultiplier + 0.05f;
                                break;
                        }
                    }
                    break;
            }
        }

        // === 获取商店数据 ===

        /// <summary>
        /// 获取武器商店列表
        /// </summary>
        public List<ShopItemData> GetWeaponShopItems() => weaponShopItems;

        /// <summary>
        /// 获取宠物商店列表
        /// </summary>
        public List<ShopItemData> GetPetShopItems() => petShopItems;

        /// <summary>
        /// 获取被动道具商店列表
        /// </summary>
        public List<ShopItemData> GetPassiveShopItems() => passiveShopItems;

        /// <summary>
        /// 获取宝石升级商店列表
        /// </summary>
        public List<ShopItemData> GetGemUpgradeItems() => gemUpgradeItems;

        /// <summary>
        /// 根据ID获取物品
        /// </summary>
        public ShopItemData GetItemById(string itemId)
        {
            foreach (var item in weaponShopItems)
                if (item.itemId == itemId) return item;
            foreach (var item in petShopItems)
                if (item.itemId == itemId) return item;
            foreach (var item in passiveShopItems)
                if (item.itemId == itemId) return item;
            foreach (var item in gemUpgradeItems)
                if (item.itemId == itemId) return item;
            return null;
        }

        /// <summary>
        /// 获取物品购买价格
        /// </summary>
        public int GetItemPrice(ShopItemData item)
        {
            if (isGoldShopOpen) return item.goldPrice;
            if (isGemShopOpen) return item.gemPrice;
            return 0;
        }

        /// <summary>
        /// 获取物品当前货币数量
        /// </summary>
        public int GetCurrentCurrency()
        {
            if (isGoldShopOpen)
                return SaveSystem.Instance?.GetSessionGold() ?? 0;
            if (isGemShopOpen)
                return SaveSystem.Instance?.GetCurrency(CurrencyType.Gems) ?? 0;
            return 0;
        }
    }
}
