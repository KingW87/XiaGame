using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Enemy
{
    [System.Serializable]
    public class EnemySpawnRule
    {
        [Tooltip("敌人类型")]
        public EnemyType enemyType;
        [Tooltip("对应的敌人Prefab（可选，不填则用代码生成）")]
        public GameObject enemyPrefab;
        [Tooltip("从第几波开始出现")]
        public int unlockWave = 1;
        [Tooltip("生成权重，越大出现概率越高")]
        [Range(0f, 1f)] public float spawnWeight = 1f;
    }

    [System.Serializable]
    public class BossSpawnRule
    {
        [Tooltip("Boss类型")]
        public BossType bossType = BossType.Behemoth;
        [Tooltip("Boss预制体（可选，不填则用代码生成）")]
        public GameObject bossPrefab;
        [Tooltip("血量倍率（基础20血量 x 此倍率）")]
        public float healthMultiplier = 10f;
        [Tooltip("伤害倍率")]
        public float damageMultiplier = 2f;
        [Tooltip("体型倍率")]
        public float sizeMultiplier = 2.5f;
        [Tooltip("击杀奖励经验值")]
        public int experienceBonus = 50;
        [Tooltip("技能1冷却（秒）")]
        public float skill1Cooldown = 5f;
        [Tooltip("技能2冷却（秒）")]
        public float skill2Cooldown = 8f;
        [Tooltip("技能3冷却（秒）")]
        public float skill3Cooldown = 12f;
    }

    /// <summary>
    /// 敌人生成器 - 随时间增加难度和数量
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("生成设置")]
        [Tooltip("初始生成间隔（秒）")]
        public float initialSpawnRate = 2f;
        [Tooltip("每波减少的生成间隔")]
        public float spawnRateDecrease = 0.05f;
        [Tooltip("最小生成间隔")]
        public float minSpawnRate = 0.3f;

        [Header("生成范围")]
        [Tooltip("生成半径")]
        public float spawnRadius = 15f;
        [Tooltip("玩家安全区半径，不会在这个范围内生成敌人")]
        public float playerSafeRadius = 5f;

        [Header("敌人出现规则")]
        public List<EnemySpawnRule> spawnRules = new List<EnemySpawnRule>()
        {
            new EnemySpawnRule { enemyType = EnemyType.Normal, unlockWave = 1, spawnWeight = 1f },
            new EnemySpawnRule { enemyType = EnemyType.Fast, unlockWave = 2, spawnWeight = 0.5f },
            new EnemySpawnRule { enemyType = EnemyType.Tank, unlockWave = 3, spawnWeight = 0.3f },
            new EnemySpawnRule { enemyType = EnemyType.Ranged, unlockWave = 4, spawnWeight = 0.3f },
        };

        [Header("难度递增")]
        [Tooltip("每多少秒进入下一波")]
        public float difficultyIncreaseInterval = 30f;
        [Tooltip("每波血量倍率")]
        public float healthMultiplierPerWave = 1.15f;
        [Tooltip("每波速度倍率")]
        public float speedMultiplierPerWave = 1.05f;
        [Tooltip("每波伤害倍率")]
        public float damageMultiplierPerWave = 1.1f;

        [Header("Boss设置")]
        [Tooltip("是否启用Boss")]
        public bool enableBoss = true;
        [Tooltip("每N波出一次Boss")]
        public int bossWaveInterval = 5;
        [Tooltip("Boss波开始后延迟几秒生成")]
        public float bossSpawnDelay = 2f;
        [Tooltip("Boss生成距离玩家的半径")]
        public float bossSpawnRadius = 12f;

        [Header("Boss生成规则（按顺序轮换出现）")]
        public List<BossSpawnRule> bossSpawnRules = new List<BossSpawnRule>()
        {
            new BossSpawnRule { bossType = BossType.Behemoth, healthMultiplier = 10f, damageMultiplier = 2f, sizeMultiplier = 2.5f, experienceBonus = 50 },
            new BossSpawnRule { bossType = BossType.Shadow, healthMultiplier = 8f, damageMultiplier = 2.5f, sizeMultiplier = 2.0f, experienceBonus = 60 },
            new BossSpawnRule { bossType = BossType.Destroyer, healthMultiplier = 12f, damageMultiplier = 3f, sizeMultiplier = 2.8f, experienceBonus = 80 },
        };

        private float currentSpawnRate;
        private float spawnTimer;
        private float difficultyTimer;
        private int currentWave = 1;
        private float enemyHealthMult = 1f;
        private float enemySpeedMult = 1f;
        private float enemyDamageMult = 1f;
        private bool bossSpawnedThisWave;
        private float bossSpawnTimer;
        private bool isBossWave;

        private Transform playerTransform;

        private void Start()
        {
            currentSpawnRate = initialSpawnRate;

            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            spawnTimer += Time.deltaTime;
            difficultyTimer += Time.deltaTime;

            // 难度递增
            if (difficultyTimer >= difficultyIncreaseInterval)
            {
                IncreaseDifficulty();
                difficultyTimer = 0f;
            }

            // Boss波次计时
            if (isBossWave && !bossSpawnedThisWave && enableBoss)
            {
                bossSpawnTimer += Time.deltaTime;
                if (bossSpawnTimer >= bossSpawnDelay)
                {
                    SpawnBoss();
                    bossSpawnedThisWave = true;
                }
            }

            // 生成敌人（Boss波期间也继续生成普通敌人）
            if (spawnTimer >= currentSpawnRate)
            {
                SpawnEnemy();
                spawnTimer = 0f;
            }
        }

        private void SpawnEnemy()
        {
            if (playerTransform == null) return;

            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(playerSafeRadius, spawnRadius);
            Vector2 spawnPos = (Vector2)playerTransform.position + randomCircle;

            EnemySpawnRule rule = GetRandomSpawnRule();
            if (rule == null) return;

            GameObject enemy;
            if (rule.enemyPrefab != null)
            {
                enemy = Instantiate(rule.enemyPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                enemy = CreateEnemyFallback(spawnPos, rule.enemyType);
            }

            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ApplyDifficultyMultiplier(enemyHealthMult, enemySpeedMult, enemyDamageMult);
            }
        }

        private GameObject CreateEnemyFallback(Vector2 pos, EnemyType type)
        {
            GameObject enemy = new GameObject($"Enemy_{type}");
            enemy.transform.position = pos;

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            sr.sprite = CreateSimpleSprite();

            enemy.AddComponent<BoxCollider2D>();
            EnemyController controller = enemy.AddComponent<EnemyController>();
            controller.SetupType(type);

            return enemy;
        }

        private EnemySpawnRule GetRandomSpawnRule()
        {
            List<EnemySpawnRule> available = new List<EnemySpawnRule>();
            List<float> weights = new List<float>();

            foreach (var rule in spawnRules)
            {
                if (currentWave >= rule.unlockWave && rule.spawnWeight > 0)
                {
                    available.Add(rule);
                    weights.Add(rule.spawnWeight);
                }
            }

            if (available.Count == 0) return null;

            float totalWeight = 0;
            foreach (float w in weights) totalWeight += w;

            float random = Random.Range(0, totalWeight);
            float current = 0;
            for (int i = 0; i < available.Count; i++)
            {
                current += weights[i];
                if (random <= current)
                    return available[i];
            }

            return available[available.Count - 1];
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

        private void IncreaseDifficulty()
        {
            currentWave++;
            currentSpawnRate = Mathf.Max(currentSpawnRate - spawnRateDecrease, minSpawnRate);
            enemyHealthMult *= healthMultiplierPerWave;
            enemySpeedMult *= speedMultiplierPerWave;
            enemyDamageMult *= damageMultiplierPerWave;

            // 检测是否是Boss波
            isBossWave = enableBoss && currentWave % bossWaveInterval == 0;
            bossSpawnedThisWave = false;
            bossSpawnTimer = 0f;

            if (isBossWave)
            {
                Debug.Log($"Boss波！第 {currentWave} 波，Boss即将出现！");
            }
            else
            {
                Debug.Log($"波次 {currentWave}, 生成间隔 {currentSpawnRate:F2}秒, 血量x{enemyHealthMult:F2}");
            }
        }

        private void SpawnBoss()
        {
            if (playerTransform == null || bossSpawnRules.Count == 0) return;

            // 按顺序轮换Boss
            int bossIndex = ((currentWave / bossWaveInterval) - 1) % bossSpawnRules.Count;
            BossSpawnRule rule = bossSpawnRules[Mathf.Abs(bossIndex)];
            int bossLevel = currentWave / bossWaveInterval;

            // Boss等级加成
            float levelMult = 1f + (bossLevel - 1) * 0.5f;

            // 在玩家前方生成Boss
            Vector2 spawnDir = Random.insideUnitCircle.normalized;
            Vector2 spawnPos = (Vector2)playerTransform.position + spawnDir * bossSpawnRadius;

            GameObject bossObj;
            if (rule.bossPrefab != null)
            {
                bossObj = Instantiate(rule.bossPrefab, spawnPos, Quaternion.identity);
                if (bossObj.GetComponent<EnemyController>() == null)
                    bossObj.AddComponent<EnemyController>();
                if (bossObj.GetComponent<BossController>() == null)
                    bossObj.AddComponent<BossController>();
            }
            else
            {
                bossObj = new GameObject($"Boss_{rule.bossType}_Lv{bossLevel}");
                bossObj.transform.position = spawnPos;

                SpriteRenderer sr = bossObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
                sr.sprite = CreateSimpleSprite();
                bossObj.transform.localScale = Vector3.one * rule.sizeMultiplier;

                bossObj.AddComponent<BoxCollider2D>();
                bossObj.AddComponent<EnemyController>();
                bossObj.AddComponent<BossController>();
            }

            // 初始化基础敌人类型（Boss基于Tank类型）
            var ec = bossObj.GetComponent<EnemyController>();
            ec.SetupType(EnemyType.Tank);

            // 用规则的倍率 + 等级加成
            ec.ApplyDifficultyMultiplier(rule.healthMultiplier * levelMult, 1f + (bossLevel - 1) * 0.1f, rule.damageMultiplier * levelMult);

            // 初始化Boss控制器（从规则读取所有参数）
            var bc = bossObj.GetComponent<BossController>();
            bc.bossLevel = bossLevel;
            bc.bossType = rule.bossType;
            bc.healthMultiplier = rule.healthMultiplier;
            bc.damageMultiplier = rule.damageMultiplier;
            bc.sizeMultiplier = rule.sizeMultiplier;
            bc.experienceBonus = rule.experienceBonus;
            bc.skill1Cooldown = rule.skill1Cooldown;
            bc.skill2Cooldown = rule.skill2Cooldown;
            bc.skill3Cooldown = rule.skill3Cooldown;

            Debug.Log($"Boss出现！类型: {bc.BossName} Lv.{bossLevel}");
        }

        public int CurrentWave => currentWave;
        public float HealthMultiplier => enemyHealthMult;
        public float SpeedMultiplier => enemySpeedMult;
        public float DamageMultiplier => enemyDamageMult;
    }
}
