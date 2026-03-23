using UnityEngine;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 存档管理器 - 负责数据持久化（JSON本地存储）
    /// 单例模式
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance;

        [Header("存档设置")]
        [Tooltip("存档文件名")]
        public string saveFileName = "ClawSurvivor_save.json";
        [Tooltip("是否自动保存")]
        public bool autoSave = true;
        [Tooltip("自动保存间隔（秒）")]
        public float autoSaveInterval = 60f;

        [Header("运行时数据")]
        [Tooltip("当前局内金币（每局独立）")]
        public int sessionGold;

        private string SavePath => System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
        private float autoSaveTimer;
        private SaveData currentData;

        public SaveData CurrentData => currentData;
        public event System.Action<SaveData> OnDataLoaded;
        public event System.Action OnDataSaved;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!autoSave) return;
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGame();
            }
        }

        /// <summary>
        /// 加载存档
        /// </summary>
        public void LoadGame()
        {
            currentData = new SaveData();

            if (System.IO.File.Exists(SavePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(SavePath);
                    currentData = JsonUtility.FromJson<SaveData>(json);
                    if (currentData == null) currentData = new SaveData();
                    Debug.Log($"[SaveSystem] 存档加载成功 | 金币:{currentData.gold} 宝石:{currentData.gems}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] 存档加载失败，使用默认数据: {e.Message}");
                    currentData = new SaveData();
                }
            }
            else
            {
                Debug.Log("[SaveSystem] 未找到存档，使用默认数据");
            }

            sessionGold = 0;
            OnDataLoaded?.Invoke(currentData);
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        public void SaveGame()
        {
            if (currentData == null) return;

            try
            {
                string json = JsonUtility.ToJson(currentData, true);
                System.IO.File.WriteAllText(SavePath, json);
                OnDataSaved?.Invoke();
                Debug.Log($"[SaveSystem] 存档已保存");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] 存档保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        public void DeleteSave()
        {
            if (System.IO.File.Exists(SavePath))
            {
                System.IO.File.Delete(SavePath);
                currentData = new SaveData();
                sessionGold = 0;
                Debug.Log("[SaveSystem] 存档已删除");
            }
        }

        // === 局内货币（每局独立） ===

        /// <summary>
        /// 获取局内金币
        /// </summary>
        public int GetSessionGold() => sessionGold;

        /// <summary>
        /// 增加局内金币（击杀掉落）
        /// </summary>
        public void AddSessionGold(int amount)
        {
            sessionGold += amount;
        }

        /// <summary>
        /// 消耗局内金币
        /// </summary>
        public bool SpendSessionGold(int amount)
        {
            if (sessionGold < amount) return false;
            sessionGold -= amount;
            return true;
        }

        // === 局外货币（持久化） ===

        /// <summary>
        /// 增加局外货币
        /// </summary>
        public void AddCurrency(CurrencyType type, int amount)
        {
            if (currentData == null) return;
            currentData.AddCurrency(type, amount);
        }

        /// <summary>
        /// 消耗局外货币
        /// </summary>
        public bool SpendCurrency(CurrencyType type, int amount)
        {
            if (currentData == null) return false;
            return currentData.SpendCurrency(type, amount);
        }

        /// <summary>
        /// 获取局外货币数量
        /// </summary>
        public int GetCurrency(CurrencyType type)
        {
            return currentData?.GetCurrency(type) ?? 0;
        }

        // === 局外持久化数据操作 ===

        /// <summary>
        /// 升级武器，返回升级后等级（-1表示碎片不足）
        /// </summary>
        public int UpgradeWeapon(string weaponName, int cost)
        {
            if (currentData == null) return -1;
            if (!currentData.SpendCurrency(CurrencyType.WeaponFragments, cost)) return -1;

            int currentLevel = currentData.weaponLevels.GetValue(weaponName, 0);
            int newLevel = currentLevel + 1;
            currentData.weaponLevels.SetValue(weaponName, newLevel);
            SaveGame();
            return newLevel;
        }

        /// <summary>
        /// 获取武器等级
        /// </summary>
        public int GetWeaponLevel(string weaponName)
        {
            return currentData?.weaponLevels.GetValue(weaponName, 0) ?? 0;
        }

        /// <summary>
        /// 获取武器升级费用
        /// </summary>
        public int GetWeaponUpgradeCost(string weaponName)
        {
            int level = GetWeaponLevel(weaponName);
            return (level + 1) * 5;
        }

        /// <summary>
        /// 解锁宠物
        /// </summary>
        public bool UnlockPet(int petId, int cost)
        {
            if (currentData == null) return false;
            if (currentData.unlockedPets.Contains(petId)) return false;
            if (!currentData.SpendCurrency(CurrencyType.PetFragments, cost)) return false;
            currentData.unlockedPets.Add(petId);
            currentData.petLevels.SetValue(petId.ToString(), 1);
            SaveGame();
            return true;
        }

        /// <summary>
        /// 升级宠物
        /// </summary>
        public int UpgradePet(int petId, int cost)
        {
            if (currentData == null) return -1;
            if (!currentData.unlockedPets.Contains(petId)) return -1;
            if (!currentData.SpendCurrency(CurrencyType.PetFragments, cost)) return -1;

            int currentLevel = currentData.petLevels.GetValue(petId.ToString(), 1);
            int newLevel = currentLevel + 1;
            currentData.petLevels.SetValue(petId.ToString(), newLevel);
            SaveGame();
            return newLevel;
        }

        /// <summary>
        /// 获取宠物等级
        /// </summary>
        public int GetPetLevel(int petId)
        {
            return currentData?.petLevels.GetValue(petId.ToString(), 0) ?? 0;
        }

        /// <summary>
        /// 检查宠物是否已解锁
        /// </summary>
        public bool IsPetUnlocked(int petId)
        {
            return currentData?.unlockedPets.Contains(petId) ?? false;
        }

        /// <summary>
        /// 获取宠物解锁费用
        /// </summary>
        public int GetPetUnlockCost(int petId)
        {
            return (petId + 1) * 10;
        }

        /// <summary>
        /// 获取宠物升级费用
        /// </summary>
        public int GetPetUpgradeCost(int petId)
        {
            int level = GetPetLevel(petId);
            return (level + 1) * 8;
        }

        /// <summary>
        /// 解锁技能
        /// </summary>
        public bool UnlockSkill(int skillIndex, int cost)
        {
            if (currentData == null) return false;
            if (currentData.unlockedSkills.Contains(skillIndex)) return false;
            if (!currentData.SpendCurrency(CurrencyType.SkillBooks, cost)) return false;
            currentData.unlockedSkills.Add(skillIndex);
            SaveGame();
            return true;
        }

        /// <summary>
        /// 检查技能是否已解锁
        /// </summary>
        public bool IsSkillUnlocked(int skillIndex)
        {
            return currentData?.unlockedSkills.Contains(skillIndex) ?? false;
        }

        // === 局外永久属性购买 ===

        /// <summary>
        /// 购买永久生命加成
        /// </summary>
        public bool BuyBonusHealth(int cost, int amount)
        {
            if (currentData == null) return false;
            if (!currentData.SpendCurrency(CurrencyType.Gems, cost)) return false;
            currentData.bonusMaxHealth += amount;
            SaveGame();
            return true;
        }

        /// <summary>
        /// 购买永久伤害加成
        /// </summary>
        public bool BuyBonusDamage(int cost, float amount)
        {
            if (currentData == null) return false;
            if (!currentData.SpendCurrency(CurrencyType.Gems, cost)) return false;
            currentData.bonusDamage += amount;
            SaveGame();
            return true;
        }

        /// <summary>
        /// 购买永久速度加成
        /// </summary>
        public bool BuyBonusSpeed(int cost, float amount)
        {
            if (currentData == null) return false;
            if (!currentData.SpendCurrency(CurrencyType.Gems, cost)) return false;
            currentData.bonusMoveSpeed += amount;
            SaveGame();
            return true;
        }

        /// <summary>
        /// 购买永久经验加成
        /// </summary>
        public bool BuyBonusExp(int cost, float amount)
        {
            if (currentData == null) return false;
            if (!currentData.SpendCurrency(CurrencyType.Gems, cost)) return false;
            currentData.bonusExpMultiplier += amount;
            SaveGame();
            return true;
        }

        // === 结算统计更新 ===

        /// <summary>
        /// 游戏结束时更新统计数据并结算奖励
        /// </summary>
        public GameRewardData CalculateGameRewards(int level, float survivalTime, int kills)
        {
            if (currentData == null) return null;

            GameRewardData rewards = new GameRewardData();

            // 结算金币 = 局内金币 + 击杀奖励
            rewards.goldReward = sessionGold + kills * 2;

            // 宝石奖励 = 等级 + 时间奖励
            rewards.gemReward = level * 3 + Mathf.FloorToInt(survivalTime / 60) * 5;

            // 装备碎片奖励
            rewards.weaponFragmentReward = kills / 10;

            // 宠物碎片奖励
            rewards.petFragmentReward = kills / 20 + (level >= 10 ? 3 : 0);

            // 技能书奖励（高等级才有）
            rewards.skillBookReward = level >= 15 ? 1 : (level >= 8 ? Random.Range(0, 2) : 0);

            // 更新统计
            currentData.totalGamesPlayed++;
            if (level > currentData.highestLevel) currentData.highestLevel = level;
            if (survivalTime > currentData.longestSurvivalTime) currentData.longestSurvivalTime = survivalTime;
            currentData.totalKills += kills;

            return rewards;
        }

        /// <summary>
        /// 确认领取结算奖励
        /// </summary>
        public void ClaimGameRewards(GameRewardData rewards)
        {
            if (currentData == null || rewards == null) return;
            currentData.gold += rewards.goldReward;
            currentData.gems += rewards.gemReward;
            currentData.weaponFragments += rewards.weaponFragmentReward;
            currentData.petFragments += rewards.petFragmentReward;
            currentData.skillBooks += rewards.skillBookReward;
            sessionGold = 0;
            SaveGame();
            Debug.Log($"[SaveSystem] 领取奖励 | 金币+{rewards.goldReward} 宝石+{rewards.gemReward} 装备碎片+{rewards.weaponFragmentReward}");
        }

        /// <summary>
        /// 新一局开始时重置局内数据
        /// </summary>
        public void ResetSessionData()
        {
            sessionGold = 0;
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }

    /// <summary>
    /// 结算奖励数据
    /// </summary>
    public class GameRewardData
    {
        [Tooltip("金币奖励")]
        public int goldReward;
        [Tooltip("宝石奖励")]
        public int gemReward;
        [Tooltip("装备碎片奖励")]
        public int weaponFragmentReward;
        [Tooltip("宠物碎片奖励")]
        public int petFragmentReward;
        [Tooltip("技能书奖励")]
        public int skillBookReward;
    }
}
