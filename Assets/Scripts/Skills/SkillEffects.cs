using UnityEngine;
using System.Collections;

namespace ClawSurvivor.Skills
{
    /// <summary>
    /// 冰霜新星技能 - 范围冰冻
    /// </summary>
    [CreateAssetMenu(fileName = "IceNova", menuName = "ClawSurvivor/Skills/Ice Nova")]
    public class IceNovaSkill : SkillCard
    {
        public override void Activate(Player.PlayerController player)
        {
            base.Activate(player);
            
            // 创建冰霜特效
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 查找范围内敌人
            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, range);
            
            foreach (var collider in enemies)
            {
                var enemy = collider.GetComponent<Enemy.EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    // 可以添加冰冻效果
                }
            }
        }
    }
    
    /// <summary>
    /// 治疗波技能 - 恢复生命
    /// </summary>
    [CreateAssetMenu(fileName = "HealingWave", menuName = "ClawSurvivor/Skills/Healing Wave")]
    public class HealingWaveSkill : SkillCard
    {
        public override void Activate(Player.PlayerController player)
        {
            base.Activate(player);
            
            // 创建治疗特效
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // 治疗玩家
            player.Heal(damage);
        }
    }
    
    /// <summary>
    /// 雷电打击技能 - 单体高伤
    /// </summary>
    [CreateAssetMenu(fileName = "ThunderStrike", menuName = "ClawSurvivor/Skills/Thunder Strike")]
    public class ThunderStrikeSkill : SkillCard
    {
        public override void Activate(Player.PlayerController player)
        {
            base.Activate(player);
            
            // 寻找最近敌人
            var enemies = Enemy.EnemyController.AllEnemies;
            Enemy.EnemyController nearest = null;
            float minDist = range;
            
            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
            
            if (nearest != null)
            {
                // 创建雷电特效
                if (effectPrefab != null)
                {
                    GameObject effect = Instantiate(effectPrefab, nearest.transform.position, Quaternion.identity);
                    Destroy(effect, 1f);
                }
                
                nearest.TakeDamage(damage);
            }
        }
    }
}
