using UnityEngine;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.Player
{
    /// <summary>
    /// 玩家控制器 - 负责移动、自动攻击、属性管理
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动设置")]
        [Tooltip("基础移动速度")]
        public float moveSpeed = 5f;
        
        [Header("自动攻击")]
        [Tooltip("自动攻击间隔（秒）")]
        public float attackRate = 0.3f;
        [Tooltip("自动攻击范围")]
        public float attackRange = 10f;
        [Tooltip("基础攻击伤害")]
        public int baseDamage = 50;
        
        [Header("属性")]
        [Tooltip("最大生命值")]
        public int maxHealth = 100;
        [Tooltip("当前生命值")]
        public int currentHealth;
        [Tooltip("当前经验值")]
        public int experience = 0;
        [Tooltip("当前等级")]
        public int level = 1;
        [Tooltip("升级所需经验")]
        public int experienceToNextLevel = 10;
        
        [Header("技能槽")]
        [Tooltip("已装备的技能列表（最多4个）")]
        public List<Skills.SkillCard> equippedSkills = new List<Skills.SkillCard>(4);
        [Tooltip("技能冷却计时器")]
        public List<float> skillCooldownTimers = new List<float>(4);
        
        private Rigidbody2D rb;
        private Transform nearestEnemy;
        private float attackTimer;
        private SpriteRenderer spriteRenderer;
        private bool isInvincible;
        private float invincibleTimer;
        private float invincibleDuration = 1f;
        private int shieldAmount;
        private bool hasLifeSteal;
        private bool isInWater;
        private float waterSlowMultiplier = 1f;
        
        // 属性加成
        private float damageMultiplier = 1f;
        private float attackSpeedMultiplier = 1f;
        private float moveSpeedMultiplier = 1f;
        private float expBoostMultiplier = 1f;

        public float DamageMultiplier => damageMultiplier;
        public float ExpBoostMultiplier => expBoostMultiplier;
        public float SetExpBoostMultiplier { set => expBoostMultiplier = value; }
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int Experience => experience;
        public int Level => level;
        public int ExperienceToNextLevel => experienceToNextLevel;
        public float HealthPercentage => (float)currentHealth / maxHealth;
        public List<Skills.SkillCard> GetEquippedSkills() => new List<Skills.SkillCard>(equippedSkills);
        public List<float> GetCooldownTimers() => new List<float>(skillCooldownTimers);
        
        public event System.Action<int, int> OnHealthChanged;
        public event System.Action<int, int> OnExperienceChanged;
        public event System.Action<int> OnLevelUp;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
        }
        
        private void Update()
        {
            HandleMovement();
            HandleAutoAttack();
            HandleAutoSkills();
        }
        
        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            Vector2 direction = new Vector2(horizontal, vertical).normalized;
            
            if (direction != Vector2.zero)
            {
                // 翻转朝向
                if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
                else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            }
            
            rb.velocity = direction * moveSpeed * moveSpeedMultiplier * waterSlowMultiplier;
        }
        
        private void HandleAutoAttack()
        {
            attackTimer += Time.deltaTime;
            
            // 寻找最近敌人
            FindNearestEnemy();
            
            if (nearestEnemy != null && attackTimer >= attackRate / attackSpeedMultiplier)
            {
                Attack();
                attackTimer = 0f;
            }
        }
        
        private void FindNearestEnemy()
        {
            var enemies = Enemy.EnemyController.AllEnemies;
            float minDistance = attackRange;
            nearestEnemy = null;
            
            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }
        }
        
        private void Attack()
        {
            if (nearestEnemy == null) return;
            
            var enemy = nearestEnemy.GetComponent<Enemy.EnemyController>();
            if (enemy != null)
            {
                int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
                enemy.TakeDamage(finalDamage);
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (isInvincible) return;

            // 护盾先吸收伤害
            if (shieldAmount > 0)
            {
                if (shieldAmount >= damage)
                {
                    shieldAmount -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= shieldAmount;
                    shieldAmount = 0;
                }
                if (Systems.SoundManager.Instance != null)
                    Systems.SoundManager.Instance.PlaySFX(SFXType.ShieldHit);
            }

            if (damage <= 0) return;

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.PlayerHit);

            StartCoroutine(InvincibilityFlash());

            if (currentHealth <= 0)
                Die();
        }

        private System.Collections.IEnumerator InvincibilityFlash()
        {
            isInvincible = true;
            invincibleTimer = invincibleDuration;

            while (invincibleTimer > 0)
            {
                invincibleTimer -= Time.deltaTime;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = Mathf.FloorToInt(invincibleTimer * 10) % 2 == 0;
                yield return null;
            }

            isInvincible = false;
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
        }
        
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void AddExperience(int amount)
        {
            int boosted = Mathf.RoundToInt(amount * expBoostMultiplier);
            experience += boosted;
            OnExperienceChanged?.Invoke(experience, experienceToNextLevel);
            
            if (experience >= experienceToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// 临时经验加倍效果 - 由ItemPickup调用
        /// </summary>
        public void StartExpBoost(float multiplier, float duration)
        {
            expBoostMultiplier = multiplier;
            Debug.Log($"经验加倍 {multiplier}x 持续 {duration}s！");
            StartCoroutine(ExpBoostTimer(duration));
        }

        private System.Collections.IEnumerator ExpBoostTimer(float duration)
        {
            yield return new WaitForSeconds(duration);
            expBoostMultiplier = 1f;
            Debug.Log("经验加倍效果结束");
        }
        
        private void LevelUp()
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.5f);
            
            // 恢复一定血量
            currentHealth = Mathf.Min(currentHealth + maxHealth / 3, maxHealth);

            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.LevelUp);

            OnLevelUp?.Invoke(level);
            OnExperienceChanged?.Invoke(experience, experienceToNextLevel);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        private void Die()
        {
            Debug.Log("玩家死亡 - 游戏结束");
            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.PlayerDeath);
            if (Systems.GameManager.Instance != null)
                Systems.GameManager.Instance.GameOver();
        }
        
        // 属性加成方法
        public void AddDamageBonus(float bonus)
        {
            damageMultiplier += bonus;
        }
        
        public void AddAttackSpeedBonus(float bonus)
        {
            attackSpeedMultiplier += bonus;
        }
        
        public void AddMoveSpeedBonus(float bonus)
        {
            moveSpeedMultiplier += bonus;
        }
        
        public void AddMaxHealthBonus(int bonus)
        {
            maxHealth += bonus;
            currentHealth += bonus;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        // 技能相关
        private void HandleAutoSkills()
        {
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                // 被动技能（冷却为0）不触发
                if (equippedSkills[i].cooldown <= 0) continue;
                
                if (i >= skillCooldownTimers.Count) skillCooldownTimers.Add(0f);
                
                skillCooldownTimers[i] += Time.deltaTime;
                if (skillCooldownTimers[i] >= equippedSkills[i].cooldown)
                {
                    equippedSkills[i].Activate(this);
                    skillCooldownTimers[i] = 0f;
                }
            }
        }
        
        public void EquipSkill(Skills.SkillCard skill)
        {
            if (equippedSkills.Count < 4)
            {
                equippedSkills.Add(skill);
                skillCooldownTimers.Add(0f);
                
                // 被动技能立即生效
                ApplyPassiveSkill(skill);
            }
        }
        
        private void ApplyPassiveSkill(Skills.SkillCard skill)
        {
            switch (skill.skillName)
            {
                case "疾风步":
                    AddMoveSpeedBonus(0.3f);
                    Debug.Log("移动速度 +30%！");
                    break;
                case "力量涌动":
                    AddDamageBonus(0.25f);
                    Debug.Log("攻击力 +25%！");
                    break;
                case "狂热":
                    AddAttackSpeedBonus(0.3f);
                    Debug.Log("攻击速度 +30%！");
                    break;
                case "钢铁之躯":
                    AddMaxHealthBonus(30);
                    Debug.Log("最大生命值 +30！");
                    break;
                case "吸血体质":
                    hasLifeSteal = true;
                    Debug.Log("击杀回复2点生命！");
                    break;
                case "磁力吸引":
                    // 通知所有 ExperiencePickup 翻倍吸附范围
                    var allPickups = FindObjectsOfType<Systems.ExperiencePickup>();
                    foreach (var p in allPickups) p.DoubleMagnetRange();
                    Debug.Log("磁力吸引范围翻倍！");
                    break;
                default:
                    Debug.Log($"装备技能: {skill.skillName}");
                    break;
            }
        }

        public void AddShield(int amount)
        {
            shieldAmount += amount;
            Debug.Log($"获得护盾 {amount} 点！");
        }

        public void OnEnemyKilled()
        {
            if (hasLifeSteal)
            {
                Heal(2);
            }
        }

        /// <summary>
        /// 水域减速 - 由TerrainHazard调用
        /// </summary>
        public void ApplyWaterSlow(float slowMultiplier)
        {
            isInWater = true;
            waterSlowMultiplier = slowMultiplier;
        }

        /// <summary>
        /// 离开水域恢复速度 - 由TerrainHazard调用
        /// </summary>
        public void RemoveWaterSlow()
        {
            isInWater = false;
            waterSlowMultiplier = 1f;
        }
        
        public void UseSkill(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < equippedSkills.Count)
            {
                equippedSkills[slotIndex].Activate(this);
            }
        }
    }
}
