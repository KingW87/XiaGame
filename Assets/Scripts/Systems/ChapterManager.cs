using UnityEngine;
using System;
using ClawSurvivor.Enemy;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 章节模式管理器 - 控制章节流程：波次→毒圈→Boss→撤离
    /// </summary>
    public class ChapterManager : MonoBehaviour
    {
        public static ChapterManager Instance;

        [Header("章节设置")]
        [Tooltip("当前章节编号（1开始）")]
        public int currentChapter = 1;
        [Tooltip("章节总波次数")]
        public int totalWaves = 10;
        [Tooltip("最后一波开始前几秒显示警告")]
        public float finalWaveWarningTime = 5f;

        [Header("毒圈设置")]
        [Tooltip("是否启用毒圈")]
        public bool poisonCircleEnabled = true;
        [Tooltip("毒圈开始时间（秒，最后一here之前）")]
        public float poisonCircleStartTime = 30f;
        [Tooltip("毒圈伤害间隔（秒）")]
        public float poisonDamageInterval = 0.5f;
        [Tooltip("毒圈每次伤害值")]
        public int poisonDamagePerTick = 1;

        [Header("撤离设置")]
        [Tooltip("撤离持续时间（秒）")]
        public float extractionDuration = 30f;
        [Tooltip("撤离期间刷怪间隔")]
        public float extractionSpawnInterval = 0.5f;
        [Tooltip("撤离期间每次刷怪数量")]
        public int extractionSpawnCount = 3;

        [Header("状态")]
        [Tooltip("当前是否在章节模式中")]
        public bool isInChapterMode;
        [Tooltip("当前是否在撤离阶段")]
        public bool isExtractionPhase;
        [Tooltip("当前波次")]
        public int currentWave;
        [Tooltip("章节是否完成")]
        public bool chapterCompleted;

        // 事件
        public Action<int> OnWaveChanged;
        public Action OnFinalWaveStart;
        public Action OnPoisonCircleStart;
        public Action<float> OnPoisonCircleShrink;
        public Action OnBossSpawned;
        public Action OnBossDefeated;
        public Action OnExtractionStart;
        public Action<float> OnExtractionTimerUpdate;
        public Action OnChapterComplete;
        public Action OnChapterFailed;

        [Tooltip("章节已进行时间")]
        public float chapterTimer;

        /// <summary>
        /// 获取章节已进行时间
        /// </summary>
        public float ChapterTime => chapterTimer;
        private float poisonDamageTimer;
        private float extractionTimer;
        private float extractionSpawnTimer;
        private Player.PlayerController player;
        private EnemySpawner enemySpawner;
        private bool finalWaveTriggered;
        private bool bossDefeated;
        private PoisonCircle poisonCircle;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            player = FindObjectOfType<Player.PlayerController>();
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        private void Update()
        {
            if (!isInChapterMode || chapterCompleted) return;

            chapterTimer += Time.deltaTime;

            // 更新波次
            UpdateWaves();

            // 毒圈逻辑
            if (poisonCircleEnabled && !isExtractionPhase)
            {
                UpdatePoisonCircle();
            }

            // 撤离阶段逻辑
            if (isExtractionPhase)
            {
                UpdateExtraction();
            }
        }

        private void UpdateWaves()
        {
            if (enemySpawner == null) return;

            int newWave = enemySpawner.CurrentWave;
            if (newWave != currentWave && newWave > 0)
            {
                currentWave = newWave;
                OnWaveChanged?.Invoke(currentWave);
                Debug.Log($"[Chapter] 波次变化: {currentWave}/{totalWaves}");

                // 检查是否是最后一波
                if (currentWave >= totalWaves && !finalWaveTriggered)
                {
                    finalWaveTriggered = true;
                    OnFinalWaveStart?.Invoke();
                    Debug.Log("[Chapter] 最后一波即将开始！");
                }
            }
        }

        private void UpdatePoisonCircle()
        {
            // 计算到最后一波开始的时间
            if (finalWaveTriggered && poisonCircle != null)
            {
                // 毒圈持续缩小
                float shrinkProgress = (chapterTimer - poisonCircleStartTime) / 10f;
                OnPoisonCircleShrink?.Invoke(Mathf.Clamp01(shrinkProgress));
            }
            else if (!finalWaveTriggered && chapterTimer >= poisonCircleStartTime - finalWaveWarningTime)
            {
                // 即将进入最后一波，显示警告
                if (poisonCircle == null)
                {
                    CreatePoisonCircle();
                }
            }

            // 毒圈伤害
            if (poisonCircle != null && !player.IsInsidePoisonCircle())
            {
                poisonDamageTimer += Time.deltaTime;
                if (poisonDamageTimer >= poisonDamageInterval)
                {
                    poisonDamageTimer = 0;
                    player.TakeDamage(poisonDamagePerTick);
                }
            }
        }

        private void UpdateExtraction()
        {
            extractionTimer -= Time.deltaTime;
            OnExtractionTimerUpdate?.Invoke(extractionTimer);

            // 撤离期间刷怪
            extractionSpawnTimer += Time.deltaTime;
            if (extractionSpawnTimer >= extractionSpawnInterval)
            {
                extractionSpawnTimer = 0;
                SpawnExtractionEnemies();
            }

            // 撤离结束
            if (extractionTimer <= 0)
            {
                CompleteChapter();
            }
        }

        private void CreatePoisonCircle()
        {
            if (poisonCircle == null)
            {
                GameObject circleGO = new GameObject("PoisonCircle");
                circleGO.transform.position = player.transform.position; // 在玩家位置创建
                poisonCircle = circleGO.AddComponent<PoisonCircle>();
                poisonCircle.Initialize(20f, 5f); // 初始半径20，随时间缩小到5
            }
            OnPoisonCircleStart?.Invoke();
        }

        private void SpawnExtractionEnemies()
        {
            if (enemySpawner == null) return;

            // 在玩家周围生成敌人
            for (int i = 0; i < extractionSpawnCount; i++)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 15f;
                Vector2 spawnPos = (Vector2)player.transform.position + randomOffset;
                enemySpawner.SpawnEnemyAt(spawnPos);
            }
        }

        /// <summary>
        /// 开始章节模式
        /// </summary>
        public void StartChapter(int chapterNumber)
        {
            currentChapter = chapterNumber;
            isInChapterMode = true;
            chapterCompleted = false;
            currentWave = 0;
            chapterTimer = 0;
            finalWaveTriggered = false;
            bossDefeated = false;
            isExtractionPhase = false;
            extractionTimer = extractionDuration;

            Debug.Log($"[Chapter] 章节 {chapterNumber} 开始！共 {totalWaves} 波");
        }

        /// <summary>
        /// 结束章节（胜利）
        /// </summary>
        public void CompleteChapter()
        {
            if (chapterCompleted) return;
            chapterCompleted = true;
            isInChapterMode = false;

            OnChapterComplete?.Invoke();
            Debug.Log($"[Chapter] 章节 {currentChapter} 通关完成！");
        }

        /// <summary>
        /// 章节失败（玩家死亡）
        /// </summary>
        public void FailChapter()
        {
            if (chapterCompleted) return;
            chapterCompleted = true;
            isInChapterMode = false;

            OnChapterFailed?.Invoke();
            Debug.Log($"[Chapter] 章节 {currentChapter} 失败");
        }

        /// <summary>
        /// Boss被击败
        /// </summary>
        public void OnBossDefeatedEvent()
        {
            if (bossDefeated) return;
            bossDefeated = true;

            OnBossDefeated?.Invoke();
            Debug.Log("[Chapter] Boss已被击败！");

            // 开始撤离阶段
            StartExtraction();
        }

        /// <summary>
        /// 开始撤离阶段
        /// </summary>
        private void StartExtraction()
        {
            isExtractionPhase = true;
            extractionTimer = extractionDuration;
            extractionSpawnTimer = 0;

            OnExtractionStart?.Invoke();
            Debug.Log($"[Chapter] 撤离阶段开始！持续 {extractionDuration} 秒");
        }

        /// <summary>
        /// 获取章节进度（0-1）
        /// </summary>
        public float GetChapterProgress()
        {
            if (totalWaves <= 0) return 0;
            return (float)currentWave / totalWaves;
        }

        /// <summary>
        /// 获取撤离进度（0-1）
        /// </summary>
        public float GetExtractionProgress()
        {
            if (extractionDuration <= 0) return 1;
            return 1 - (extractionTimer / extractionDuration);
        }

        /// <summary>
        /// 停止章节模式
        /// </summary>
        public void StopChapter()
        {
            isInChapterMode = false;
            chapterCompleted = false;
            isExtractionPhase = false;

            if (poisonCircle != null)
            {
                Destroy(poisonCircle.gameObject);
                poisonCircle = null;
            }
        }
    }
}
