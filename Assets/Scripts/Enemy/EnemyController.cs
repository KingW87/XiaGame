using UnityEngine;
using System.Collections.Generic;
using ClawSurvivor.Systems;
using ClawSurvivor.Effects;

namespace ClawSurvivor.Enemy
{
    public enum EnemyType
    {
        Normal,    // 普通型：直线追踪
        Fast,      // 快速型：高速低血
        Tank,      // 坦克型：低速高血
        Ranged,    // 远程型：保持距离发射弹丸
        Boss       // Boss型：由BossController控制行为
    }

    public class EnemyController : MonoBehaviour
    {
        public static List<EnemyController> AllEnemies = new List<EnemyController>();

        [Header("类型")]
        [Tooltip("敌人类型")]
        public EnemyType enemyType = EnemyType.Normal;

        [Header("特殊类型")]
        [Tooltip("是否为Boss")]
        public bool isBoss;
        [Tooltip("是否为精英怪")]
        public bool isElite;

        [Header("属性")]
        [Tooltip("最大生命值")]
        public int maxHealth = 20;
        [Tooltip("攻击伤害")]
        public int damage = 10;
        [Tooltip("移动速度")]
        public float moveSpeed = 2f;
        [Tooltip("死亡掉落经验值")]
        public int experienceValue = 5;

        [Header("攻击")]
        [Tooltip("攻击冷却时间（秒）")]
        public float attackCooldown = 1f;
        [Tooltip("攻击范围")]
        public float attackRange = 1f;

        [Header("碰撞伤害")]
        [Tooltip("碰撞伤害（普通小怪攻击的一半）")]
        public int collisionDamage = 5;
        [Tooltip("碰撞伤害冷却时间（秒）")]
        public float collisionDamageCooldown = 0.5f;

        [Header("远程设置（仅远程型）")]
        [Tooltip("偏好距离（远程型保持的距离）")]
        public float preferredDistance = 6f;
        [Tooltip("弹射物飞行速度")]
        public float projectileSpeed = 5f;
        [Tooltip("弹射物预制体（可选）")]
        public GameObject projectilePrefab;

        private int currentHealth;
        private Transform playerTarget;
        private float attackTimer;
        private float collisionDamageTimer; // 碰撞伤害冷却计时器
        private bool isDead;

        // 敌人基础颜色
        private static readonly Color[] TypeColors = {
            new Color(0.85f, 0.15f, 0.15f),  // Normal: 红
            new Color(0.1f, 0.9f, 0.2f),     // Fast: 绿
            new Color(0.6f, 0.2f, 0.9f),     // Tank: 紫
            new Color(1f, 0.65f, 0f)         // Ranged: 橙
        };

        private Color originalColor;

        public EnemyType Type => enemyType;
        public bool IsDead => isDead;
        public int CurrentHealth => currentHealth;
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        private void OnEnable()
        {
            AllEnemies.Add(this);
            currentHealth = maxHealth;
            isDead = false;
        }

        public void ApplyDifficultyMultiplier(float healthMult, float speedMult, float damageMult)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * healthMult);
            currentHealth = maxHealth;
            moveSpeed *= speedMult;
            damage = Mathf.RoundToInt(damage * damageMult);
        }

        /// <summary>
        /// 初始化敌人类型（代码创建敌人时调用）
        /// </summary>
        public void SetupType(EnemyType type)
        {
            enemyType = type;
            originalColor = TypeColors[(int)type];
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = originalColor;

            switch (type)
            {
                case EnemyType.Normal:
                    maxHealth = 20; damage = 10; moveSpeed = 2f; experienceValue = 5;
                    transform.localScale = Vector3.one * 0.8f;
                    break;
                case EnemyType.Fast:
                    maxHealth = 10; damage = 5; moveSpeed = 4f; experienceValue = 4;
                    attackRange = 0.8f;
                    transform.localScale = Vector3.one * 0.5f;
                    break;
                case EnemyType.Tank:
                    maxHealth = 60; damage = 15; moveSpeed = 1f; experienceValue = 12;
                    attackRange = 1.5f;
                    transform.localScale = Vector3.one * 1.2f;
                    break;
                case EnemyType.Ranged:
                    maxHealth = 15; damage = 8; moveSpeed = 1.5f; experienceValue = 8;
                    attackRange = 8f; attackCooldown = 2f;
                    preferredDistance = 6f;
                    transform.localScale = Vector3.one * 0.7f;
                    break;
            }
            currentHealth = maxHealth;
        }

        private void OnDisable()
        {
            AllEnemies.Remove(this);
        }

        private void Start()
        {
            // 初始化碰撞伤害为普通攻击的一半
            if (collisionDamage == 0)
                collisionDamage = Mathf.Max(1, damage / 2);

            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                playerTarget = player.transform;
        }

        private void Update()
        {
            if (isDead || playerTarget == null) return;

            switch (enemyType)
            {
                case EnemyType.Normal:
                case EnemyType.Fast:
                case EnemyType.Tank:
                    MoveTowardsPlayer();
                    TryAttackPlayer();
                    break;
                case EnemyType.Ranged:
                    RangedBehavior();
                    break;
            }
        }

        private void MoveTowardsPlayer()
        {
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            if (direction.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            else if (direction.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }

        private void RangedBehavior()
        {
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            Vector2 direction = (playerTarget.position - transform.position).normalized;

            // 保持距离
            if (distance < preferredDistance - 1f)
            {
                transform.position -= (Vector3)direction * moveSpeed * Time.deltaTime;
            }
            else if (distance > preferredDistance + 1f)
            {
                transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            }

            if (direction.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            else if (direction.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);

            // 在攻击范围内发射弹丸
            if (distance <= attackRange)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    FireProjectile();
                    attackTimer = 0f;
                }
            }
        }

        private void FireProjectile()
        {
            GameObject proj = new GameObject("EnemyProjectile");
            proj.transform.position = transform.position;

            SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
            sr.color = Color.yellow;
            sr.sortingOrder = 8;
            sr.sprite = CreateSimpleSprite();
            proj.transform.localScale = Vector3.one * 0.3f;

            proj.AddComponent<BoxCollider2D>().isTrigger = true;
            proj.AddComponent<EnemyProjectile>().Initialize(damage, projectileSpeed, playerTarget);
            Destroy(proj, 5f);
        }

        private void TryAttackPlayer()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            if (distanceToPlayer <= attackRange)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    AttackPlayer();
                    attackTimer = 0f;
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (isDead) return;
            var player = collision.collider.GetComponent<Player.PlayerController>();
            if (player != null)
            {
                // 普通攻击冷却
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    AttackPlayer();
                    attackTimer = 0f;
                }

                // 碰撞伤害（独立冷却，普通小怪攻击的一半伤害）
                collisionDamageTimer += Time.deltaTime;
                if (collisionDamageTimer >= collisionDamageCooldown)
                {
                    player.TakeDamage(collisionDamage);
                    collisionDamageTimer = 0f;
                }
            }
        }

        private void AttackPlayer()
        {
            var player = playerTarget.GetComponent<Player.PlayerController>();
            if (player != null)
                player.TakeDamage(damage);
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;

            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.EnemyHit);

            StartCoroutine(FlashRed());

            if (currentHealth <= 0)
                Die();
        }

        private System.Collections.IEnumerator FlashRed()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            if (Systems.GameManager.Instance != null)
                Systems.GameManager.Instance.AddKill();

            // 通知玩家击杀（用于吸血体质等被动）
            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                player.OnEnemyKilled();

            // 掉落经验宝石
            if (Systems.ObjectPool.Instance != null)
            {
                var pickup = Systems.ObjectPool.Instance.Spawn<ExperiencePickup>("ExperienceGem", transform.position);
                if (pickup != null) { pickup.Initialize(experienceValue); }
                else { CreateExperienceGemDirectly(); }
            }
            else
            {
                CreateExperienceGemDirectly();
            }

            // 播放敌人死亡特效
            if (EffectManager.Instance != null)
                EffectManager.PlayEnemyDeath(transform.position);

            // 播放敌人死亡音效
            if (Systems.SoundManager.Instance != null)
                Systems.SoundManager.Instance.PlaySFX(SFXType.EnemyDeath);

            // 尝试掉落道具
            if (Pickups.PickupSpawner.Instance != null)
                Pickups.PickupSpawner.Instance.TryDropItem(transform.position);

            // 尝试掉落武器进化材料
            TryDropEvolveMaterial();

            Destroy(gameObject);
        }

        /// <summary>
        /// 尝试掉落武器进化材料
        /// </summary>
        private void TryDropEvolveMaterial()
        {
            // 根据敌人难度决定是否掉落进化材料
            float dropChance = 0.1f; // 10%基础掉落率
            
            // Boss必定掉落
            if (isBoss)
            {
                dropChance = 1f;
            }
            // 精英怪高概率掉落
            else if (isElite)
            {
                dropChance = 0.5f;
            }
            
            if (Random.value > dropChance) return;

            // 根据波次数决定材料稀有度
            int materialId = 1; // 默认普通
            float waveProgress = 0f;
            
            var chapterMgr = Systems.ChapterManager.Instance;
            if (chapterMgr != null)
            {
                waveProgress = (float)chapterMgr.currentWave / chapterMgr.totalWaves;
            }
            
            // 波次越高，材料越稀有
            float rand = Random.value;
            if (waveProgress > 0.8f && rand > 0.7f)
                materialId = 4; // 史诗
            else if (waveProgress > 0.5f && rand > 0.5f)
                materialId = 3; // 稀有
            else if (waveProgress > 0.3f && rand > 0.3f)
                materialId = 2; // 优秀
            
            // 直接给予材料（不通过物品掉落）
            if (Weapons.WeaponUpgradeSystem.Instance != null)
            {
                Weapons.WeaponUpgradeSystem.Instance.CollectEvolveMaterial(materialId, isBoss ? 3 : 1);
                Debug.Log($"掉落进化材料: {Pickups.EvolveMaterialIds.GetMaterialName(materialId)} x{(isBoss ? 3 : 1)}");
            }
        }

        private void CreateExperienceGemDirectly()
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0);

            GameObject gem = new GameObject("ExperienceGem");
            gem.transform.position = spawnPos;
            gem.transform.localScale = Vector3.one * 0.8f;

            SpriteRenderer sr = gem.AddComponent<SpriteRenderer>();
            sr.color = Color.green;
            sr.sortingOrder = 10;
            sr.sprite = CreateSimpleSprite();

            gem.AddComponent<BoxCollider2D>().isTrigger = true;
            gem.AddComponent<Systems.ExperiencePickup>().Initialize(experienceValue);
            Destroy(gem, 30f);
        }

        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
    }
}
