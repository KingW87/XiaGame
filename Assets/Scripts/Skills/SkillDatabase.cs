using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Skills
{
    /// <summary>
    /// 技能管理器 - 运行时自动注册技能
    /// </summary>
    public class SkillDatabase : MonoBehaviour
    {
        public static SkillDatabase Instance;
        
        [Header("技能库（自动填充）")]
        [Tooltip("所有可用技能列表")]
        public List<SkillCard> allSkills = new List<SkillCard>();
        
        private void Awake()
        {
            Instance = this;
            CreateDefaultSkills();
        }
        
        private void CreateDefaultSkills()
        {
            // === 主动技能 ===
            allSkills.Add(CreateSkill("冰霜新星", "释放冰霜冲击波，对周围敌人造成伤害并减速", SkillType.Damage, 5f, 30, 5f, 0f));
            allSkills.Add(CreateSkill("治疗波", "恢复自身25点生命值", SkillType.Support, 8f, 25, 0f, 0f));
            allSkills.Add(CreateSkill("雷电打击", "对最近的敌人造成高额伤害", SkillType.Damage, 6f, 80, 12f, 0f));
            allSkills.Add(CreateSkill("火焰旋风", "持续旋转的火焰，伤害周围所有敌人", SkillType.Damage, 10f, 15, 3f, 3f));
            allSkills.Add(CreateSkill("护盾", "获得一个可吸收伤害的护盾", SkillType.Support, 15f, 0, 0f, 5f));
            allSkills.Add(CreateSkill("生命汲取", "对最近敌人造成伤害并回复等量生命", SkillType.Support, 7f, 20, 10f, 0f));

            // === 被动技能 ===
            allSkills.Add(CreateSkill("疾风步", "永久提升移动速度30%", SkillType.Support, 0f, 0, 0f, 0f));
            allSkills.Add(CreateSkill("力量涌动", "永久提升攻击伤害25%", SkillType.Damage, 0f, 0, 0f, 0f));
            allSkills.Add(CreateSkill("狂热", "永久提升攻击速度30%", SkillType.Damage, 0f, 0, 0f, 0f));
            allSkills.Add(CreateSkill("钢铁之躯", "永久提升最大生命值30", SkillType.Support, 0f, 0, 0f, 0f));
            allSkills.Add(CreateSkill("吸血体质", "每次击杀恢复2点生命", SkillType.Support, 0f, 0, 0f, 0f));
            allSkills.Add(CreateSkill("磁力吸引", "经验宝石吸附范围翻倍", SkillType.Support, 0f, 0, 0f, 0f));

            Debug.Log($"技能库已初始化，共 {allSkills.Count} 个技能");
        }
        
        private SkillCard CreateSkill(string name, string desc, SkillType type, float cd, int dmg, float range, float duration)
        {
            SkillCard skill = ScriptableObject.CreateInstance<SkillCard>();
            skill.skillName = name;
            skill.description = desc;
            skill.type = type;
            skill.cooldown = cd;
            skill.damage = dmg;
            skill.range = range;
            skill.duration = duration;
            skill.hideFlags = HideFlags.HideAndDontSave;
            return skill;
        }
        
        /// <summary>
        /// 随机获取N个技能供选择
        /// </summary>
        public SkillCard[] GetRandomSkills(int count)
        {
            List<SkillCard> pool = new List<SkillCard>(allSkills);
            List<SkillCard> selected = new List<SkillCard>();
            
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = Random.Range(0, pool.Count);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }
            
            return selected.ToArray();
        }
    }
}
