using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 收藏品类型
    /// </summary>
    public enum CollectibleType
    {
        Junk,           // 垃圾：卖钱
        Fragment,       // 藏品碎片：解锁图鉴+属性
        ArtifactShard   // 神装碎片：合成神装
    }

    /// <summary>
    /// 收藏品数据
    /// </summary>
    [System.Serializable]
    public class CollectibleData
    {
        [Tooltip("收藏品ID")]
        public int id;
        [Tooltip("收藏品名称")]
        public string name;
        [Tooltip("描述")]
        public string description;
        [Tooltip("类型")]
        public CollectibleType type;
        [Tooltip("价值（金钱或碎片数量）")]
        public int value;
        [Tooltip("是否已解锁")]
        public bool isUnlocked;
    }

    /// <summary>
    /// 收藏品系统 - 管理废弃物品、藏品碎片、神装碎片
    /// </summary>
    public class CollectibleSystem : MonoBehaviour
    {
        public static CollectibleSystem Instance;

        [Header("设置")]
        [Tooltip("是否启用收藏品系统")]
        public bool enabled = true;

        [Header("掉落概率")]
        [Tooltip("垃圾掉落概率（0-1）")]
        public float junkDropChance = 0.5f;
        [Tooltip("藏品碎片掉落概率（0-1）")]
        public float fragmentDropChance = 0.2f;
        [Tooltip("神装碎片掉落概率（0-1）")]
        public float artifactDropChance = 0.05f;
        [Tooltip("Boss额外掉落碎片数量")]
        public int bossFragmentBonus = 3;

        [Header("当前章节获得")]
        [Tooltip("本章获得的垃圾数量")]
        public int junkCollected;
        [Tooltip("本章获得的碎片数量")]
        public int fragmentsCollected;
        [Tooltip("本章获得的神装碎片数量")]
        public int artifactShardsCollected;

        private List<CollectibleData> allCollectibles = new List<CollectibleData>();
        private List<CollectibleItem> activeItems = new List<CollectibleItem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCollectibles();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeCollectibles()
        {
            // 初始化收藏品图鉴
            // 垃圾类
            allCollectibles.Add(new CollectibleData { id = 0, name = "破旧的瓶子", type = CollectibleType.Junk, value = 10 });
            allCollectibles.Add(new CollectibleData { id = 1, name = "生锈的齿轮", type = CollectibleType.Junk, value = 15 });
            allCollectibles.Add(new CollectibleData { id = 2, name = "废弃的电池", type = CollectibleType.Junk, value = 20 });

            // 藏品碎片类
            allCollectibles.Add(new CollectibleData { id = 10, name = "古代硬币", type = CollectibleType.Fragment, value = 1, description = "古老的流通货币，收集一定数量可解锁属性" });
            allCollectibles.Add(new CollectibleData { id = 11, name = "神秘宝石", type = CollectibleType.Fragment, value = 1, description = "蕴含神秘力量的宝石" });
            allCollectibles.Add(new CollectibleData { id = 12, name = "龙之鳞片", type = CollectibleType.Fragment, value = 1, description = "巨龙的鳞片，拥有强大的力量" });

            // 神装碎片类
            allCollectibles.Add(new CollectibleData { id = 20, name = "神剑碎片", type = CollectibleType.ArtifactShard, value = 1, description = "传说神剑的碎片，收集10个可合成" });
            allCollectibles.Add(new CollectibleData { id = 21, name = "神器碎片", type = CollectibleType.ArtifactShard, value = 1, description = "神秘神器的碎片" });

            Debug.Log($"[Collectible] 已初始化 {allCollectibles.Count} 种收藏品");
        }

        /// <summary>
        /// 尝试掉落收藏品
        /// </summary>
        public void TryDropCollectible(Vector3 position)
        {
            if (!enabled) return;

            float rand = Random.value;

            // 优先判断高价值掉落
            if (rand < artifactDropChance)
            {
                SpawnCollectible(position, CollectibleType.ArtifactShard);
            }
            else if (rand < artifactDropChance + fragmentDropChance)
            {
                SpawnCollectible(position, CollectibleType.Fragment);
            }
            else if (rand < artifactDropChance + fragmentDropChance + junkDropChance)
            {
                SpawnCollectible(position, CollectibleType.Junk);
            }
        }

        /// <summary>
        /// Boss死亡额外掉落
        /// </summary>
        public void BossDropBonus(Vector3 position)
        {
            if (!enabled) return;

            // 必定掉落碎片
            for (int i = 0; i < bossFragmentBonus; i++)
            {
                SpawnCollectible(position, CollectibleType.Fragment);
            }

            // 概率掉落神装碎片
            if (Random.value < 0.3f)
            {
                SpawnCollectible(position, CollectibleType.ArtifactShard);
            }
        }

        private void SpawnCollectible(Vector3 position, CollectibleType type)
        {
            GameObject itemGO = new GameObject($"Collectible_{type}");
            itemGO.transform.position = position;

            CollectibleItem item = itemGO.AddComponent<CollectibleItem>();
            item.Initialize(type);
            activeItems.Add(item);
        }

        /// <summary>
        /// 收集物品
        /// </summary>
        public void CollectItem(CollectibleType type)
        {
            switch (type)
            {
                case CollectibleType.Junk:
                    junkCollected += Random.Range(5, 15);
                    break;
                case CollectibleType.Fragment:
                    fragmentsCollected += 1;
                    break;
                case CollectibleType.ArtifactShard:
                    artifactShardsCollected += 1;
                    break;
            }

            Debug.Log($"[Collectible] 收集 {type}, 本章: 垃圾={junkCollected} 碎片={fragmentsCollected} 神装={artifactShardsCollected}");
        }

        /// <summary>
        /// 获取本章收集的资源
        /// </summary>
        public (int junk, int fragment, int artifact) GetChapterCollectibles()
        {
            return (junkCollected, fragmentsCollected, artifactShardsCollected);
        }

        /// <summary>
        /// 章节结算时保存收藏品
        /// </summary>
        public void SaveChapterCollectibles()
        {
            if (SaveSystem.Instance == null) return;

            // 保存到存档
            SaveSystem.Instance.AddCurrency(CurrencyType.Gold, junkCollected);

            Debug.Log($"[Collectible] 本章获得: {junkCollected}金币, {fragmentsCollected}碎片, {artifactShardsCollected}神装碎片");
        }

        /// <summary>
        /// 重置本章收集（开始新章节时）
        /// </summary>
        public void ResetChapterCollectibles()
        {
            junkCollected = 0;
            fragmentsCollected = 0;
            artifactShardsCollected = 0;
        }

        /// <summary>
        /// 清理场景中的收藏品
        /// </summary>
        public void ClearActiveItems()
        {
            foreach (var item in activeItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            activeItems.Clear();
        }
    }

    /// <summary>
    /// 收藏品实体 - 挂在场景中的收藏品物体上
    /// </summary>
    public class CollectibleItem : MonoBehaviour
    {
        [Tooltip("收藏品类型")]
        public CollectibleType type;

        private SpriteRenderer sr;
        private Player.PlayerController player;
        private float magnetRange = 5f;
        private float moveSpeed = 8f;
        private bool isAttracted;

        public void Initialize(CollectibleType collectibleType)
        {
            type = collectibleType;
            player = FindObjectOfType<Player.PlayerController>();

            // 创建可视化
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(transform);
            sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            UpdateVisual();
            SetPosition();
        }

        private void UpdateVisual()
        {
            switch (type)
            {
                case CollectibleType.Junk:
                    sr.color = Color.gray;
                    transform.localScale = Vector3.one * 0.3f;
                    break;
                case CollectibleType.Fragment:
                    sr.color = Color.cyan;
                    transform.localScale = Vector3.one * 0.4f;
                    break;
                case CollectibleType.ArtifactShard:
                    sr.color = new Color(1f, 0.5f, 0f); // 金色
                    transform.localScale = Vector3.one * 0.5f;
                    break;
            }

            // 临时用方形代替
            sr.sprite = CreateSimpleSprite();
        }

        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(8, 8);
            Color[] colors = new Color[64];
            for (int i = 0; i < 64; i++) colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }

        private void SetPosition()
        {
            // 随机偏移
            transform.position += (Vector3)Random.insideUnitCircle * 0.5f;
        }

        private void Update()
        {
            if (player == null) return;

            float dist = Vector2.Distance(transform.position, player.transform.position);

            // 磁力吸附
            if (dist < magnetRange)
            {
                isAttracted = true;
            }

            if (isAttracted)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);

                // 被玩家收集
                if (dist < 1f)
                {
                    Collect();
                }
            }
            else
            {
                // 缓慢旋转效果
                transform.Rotate(Vector3.forward, 90f * Time.deltaTime);
            }
        }

        private void Collect()
        {
            if (CollectibleSystem.Instance != null)
            {
                CollectibleSystem.Instance.CollectItem(type);
            }

            Destroy(gameObject);
        }
    }
}
