using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 平衡数据管理器 - 加载和使用游戏平衡数值
    /// 支持从JSON文件加载，也可导出为CSV格式
    /// </summary>
    public class BalanceManager : MonoBehaviour
    {
        public static BalanceManager Instance;

        [Header("数据配置")]
        [Tooltip("平衡数据（可在Inspector中配置）")]
        public BalanceData balanceData;

        [Header("数据文件")]
        [Tooltip("数据文件路径（相对于Resources）")]
        public string dataFilePath = "BalanceData/GameBalance";

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
                return;
            }

            LoadBalanceData();
        }

        /// <summary>
        /// 加载平衡数据
        /// </summary>
        private void LoadBalanceData()
        {
            // 尝试从Resources加载
            TextAsset jsonFile = Resources.Load<TextAsset>(dataFilePath);
            if (jsonFile != null)
            {
                try
                {
                    balanceData = JsonUtility.FromJson<BalanceData>(jsonFile.text);
                    Debug.Log("[BalanceManager] 从文件加载平衡数据成功");
                    return;
                }
                catch
                {
                    Debug.LogWarning("[BalanceManager] 解析JSON失败，使用默认数据");
                }
            }

            // 使用默认数据
            balanceData = BalanceData.GetDefault();
            Debug.Log("[BalanceManager] 使用默认平衡数据");
        }

        /// <summary>
        /// 获取玩家数值表
        /// </summary>
        public PlayerBalanceTable GetPlayerTable()
        {
            return balanceData?.playerTable ?? PlayerBalanceTable.GetDefault();
        }

        /// <summary>
        /// 获取敌人数值表
        /// </summary>
        public EnemyBalanceTable GetEnemyTable()
        {
            return balanceData?.enemyTable ?? EnemyBalanceTable.GetDefault();
        }

        /// <summary>
        /// 获取Boss数值表
        /// </summary>
        public BossBalanceTable GetBossTable()
        {
            return balanceData?.bossTable ?? BossBalanceTable.GetDefault();
        }

        /// <summary>
        /// 获取掉落概率表
        /// </summary>
        public DropRateTable GetDropRateTable()
        {
            return balanceData?.dropRateTable ?? DropRateTable.GetDefault();
        }

        /// <summary>
        /// 获取经验曲线表
        /// </summary>
        public ExperienceCurveTable GetExpCurveTable()
        {
            return balanceData?.expCurveTable ?? ExperienceCurveTable.GetDefault();
        }

        /// <summary>
        /// 获取技能数值表
        /// </summary>
        public SkillBalanceTable GetSkillTable()
        {
            return balanceData?.skillTable ?? SkillBalanceTable.GetDefault();
        }

        /// <summary>
        /// 导出为CSV格式（用于Excel）
        /// </summary>
        public void ExportToCSV(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            ExportPlayerTable(folderPath);
            ExportEnemyTable(folderPath);
            ExportBossTable(folderPath);
            ExportDropRateTable(folderPath);
            ExportExpCurveTable(folderPath);
            ExportSkillTable(folderPath);

            Debug.Log($"[BalanceManager] 已导出CSV到: {folderPath}");
        }

        private void ExportPlayerTable(string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("参数,数值,说明");
            sb.AppendLine($"初始最大生命值,{balanceData.playerTable.initialMaxHealth},玩家初始生命值");
            sb.AppendLine($"初始移动速度,{balanceData.playerTable.initialMoveSpeed},基础移动速度");
            sb.AppendLine($"初始攻击伤害,{balanceData.playerTable.initialBaseDamage},基础攻击力");
            sb.AppendLine($"初始攻击间隔,{balanceData.playerTable.initialAttackRate},秒");
            sb.AppendLine($"初始攻击范围,{balanceData.playerTable.initialAttackRange},单位");
            sb.AppendLine($"生命成长,{balanceData.playerTable.healthPerLevel},每级增加");
            sb.AppendLine($"伤害成长,{balanceData.playerTable.damagePerLevel},每级增加");
            sb.AppendLine($"无敌持续时间,{balanceData.playerTable.invincibilityDuration},秒");

            File.WriteAllText(Path.Combine(folderPath, "玩家数值表.csv"), sb.ToString(), Encoding.UTF8);
        }

        private void ExportEnemyTable(string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("类型,生命值,伤害,移动速度,攻击间隔,经验值,生成权重");
            sb.AppendLine($"普通敌人,{balanceData.enemyTable.normal.health},{balanceData.enemyTable.normal.damage},{balanceData.enemyTable.normal.moveSpeed},{balanceData.enemyTable.normal.attackRate},{balanceData.enemyTable.normal.experienceValue},{balanceData.enemyTable.normal.spawnWeight}");
            sb.AppendLine($"快速敌人,{balanceData.enemyTable.fast.health},{balanceData.enemyTable.fast.damage},{balanceData.enemyTable.fast.moveSpeed},{balanceData.enemyTable.fast.attackRate},{balanceData.enemyTable.fast.experienceValue},{balanceData.enemyTable.fast.spawnWeight}");
            sb.AppendLine($"坦克敌人,{balanceData.enemyTable.tank.health},{balanceData.enemyTable.tank.damage},{balanceData.enemyTable.tank.moveSpeed},{balanceData.enemyTable.tank.attackRate},{balanceData.enemyTable.tank.experienceValue},{balanceData.enemyTable.tank.spawnWeight}");
            sb.AppendLine($"远程敌人,{balanceData.enemyTable.ranged.health},{balanceData.enemyTable.ranged.damage},{balanceData.enemyTable.ranged.moveSpeed},{balanceData.enemyTable.ranged.attackRate},{balanceData.enemyTable.ranged.experienceValue},{balanceData.enemyTable.ranged.spawnWeight}");
            sb.AppendLine($"波次难度系数,{balanceData.enemyTable.waveDifficultyMultiplier},每波倍率");

            File.WriteAllText(Path.Combine(folderPath, "敌人数值表.csv"), sb.ToString(), Encoding.UTF8);
        }

        private void ExportBossTable(string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("参数,数值,说明");
            sb.AppendLine($"基础生命值,{balanceData.bossTable.baseHealth},首Boss生命值");
            sb.AppendLine($"基础伤害,{balanceData.bossTable.baseDamage},Boss攻击力");
            sb.AppendLine($"移动速度,{balanceData.bossTable.moveSpeed},Boss移动速度");
            sb.AppendLine($"攻击间隔,{balanceData.bossTable.attackRate},秒");
            sb.AppendLine($"生命成长,{balanceData.bossTable.healthPerWave},每波增加");
            sb.AppendLine($"伤害成长,{balanceData.bossTable.damagePerWave},每波增加");
            sb.AppendLine($"经验值,{balanceData.bossTable.experienceValue},击杀奖励");
            sb.AppendLine($"金币掉落,{balanceData.bossTable.goldDrop},击杀奖励");
            sb.AppendLine($"出现频率,{balanceData.bossTable.wavesPerBoss},每N波出现");

            File.WriteAllText(Path.Combine(folderPath, "Boss数值表.csv"), sb.ToString(), Encoding.UTF8);
        }

        private void ExportDropRateTable(string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("来源,掉落概率,最少数量,最多数量");
            sb.AppendLine($"普通敌人,{balanceData.dropRateTable.normalEnemyDrop.dropChance},{balanceData.dropRateTable.normalEnemyDrop.minItems},{balanceData.dropRateTable.normalEnemyDrop.maxItems}");
            sb.AppendLine($"Boss,{balanceData.dropRateTable.bossDrop.dropChance},{balanceData.dropRateTable.bossDrop.minItems},{balanceData.dropRateTable.bossDrop.maxItems}");
            sb.AppendLine();
            sb.AppendLine("道具类型,显示名称,权重");
            foreach (var rate in balanceData.dropRateTable.pickupTypeRates)
            {
                sb.AppendLine($"{rate.type},{rate.name},{rate.weight}");
            }

            File.WriteAllText(Path.Combine(folderPath, "掉落概率表.csv"), sb.ToString(), Encoding.UTF8);
        }

        private void ExportExpCurveTable(string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("参数,数值,说明");
            sb.AppendLine($"初始经验,{balanceData.expCurveTable.baseExpToLevelUp},1级升2级所需");
            sb.AppendLine($"成长系数,{balanceData.expCurveTable.expGrowthMultiplier},经验倍率");
            sb.AppendLine($"等级加成,{balanceData.expCurveTable.expBonusPerLevel},每级额外加成");
            sb.AppendLine();
            sb.AppendLine("等级,所需经验");
            for (int i = 1; i <= 50; i++)
            {
                sb.AppendLine($"{i},{balanceData.expCurveTable.GetExpForLevel(i)}");
            }

            File.WriteAllText(Path.Combine(folderPath, "经验曲线表.csv"), sb.ToString(), Encoding.UTF8);
        }

        private void ExportSkillTable(string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("类型,技能ID,技能名称,基础伤害,每级伤害,冷却,描述");
            foreach (var skill in balanceData.skillTable.attackSkills)
            {
                sb.AppendLine($"攻击,{skill.skillId},{skill.skillName},{skill.baseDamage},{skill.damagePerLevel},{skill.cooldown},{skill.description}");
            }
            foreach (var skill in balanceData.skillTable.defenseSkills)
            {
                sb.AppendLine($"防御,{skill.skillId},{skill.skillName},{skill.baseValue},{skill.valuePerLevel},{skill.cooldown},{skill.description}");
            }
            foreach (var skill in balanceData.skillTable.utilitySkills)
            {
                sb.AppendLine($"辅助,{skill.skillId},{skill.skillName},{skill.baseValue},{skill.valuePerLevel},{skill.cooldown},{skill.description}");
            }

            File.WriteAllText(Path.Combine(folderPath, "技能数值表.csv"), sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 从CSV导入数据（用于外部编辑后再导入游戏）
        /// </summary>
        public void ImportFromCSV(string folderPath)
        {
            // TODO: 实现CSV导入逻辑
            Debug.Log("[BalanceManager] CSV导入功能待实现");
        }
    }
}
