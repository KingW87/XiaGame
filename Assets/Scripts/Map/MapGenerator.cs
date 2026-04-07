using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Map
{

    /// <summary>
    /// 无限地图生成器 - 基于摄像机位置动态加载/卸载地图块
    /// 使用预制体插槽配置
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("地图块设置")]
        [Tooltip("每个地图块的世界大小（正方形边长）")]
        public float chunkSize = 20f;

        [Tooltip("摄像机周围可视范围（块数），设为2表示加载3x3的块")]
        public int viewDistance = 2;

        [Tooltip("是否在中心点生成无障碍安全区")]
        public bool generateSafeCenter = true;

        [Header("地面预制体")]
        [Tooltip("草地预制体")]
        public GameObject grassPrefab;
        [Tooltip("泥土预制体")]
        public GameObject dirtPrefab;
        [Tooltip("石头地面预制体")]
        public GameObject stonePrefab;
        [Tooltip("水预制体")]
        public GameObject waterPrefab;
        [Tooltip("岩浆预制体")]
        public GameObject lavaPrefab;

        [Header("障碍物预制体")]
        [Tooltip("岩石预制体")]
        public GameObject rockPrefab;
        [Tooltip("树木预制体")]
        public GameObject treePrefab;
        [Tooltip("墙壁预制体")]
        public GameObject wallPrefab;
        [Tooltip("栅栏预制体")]
        public GameObject fencePrefab;
        [Tooltip("灌木预制体")]
        public GameObject bushPrefab;

        [Header("装饰物预制体")]
        [Tooltip("草丛预制体")]
        public GameObject grassPatchPrefab;
        [Tooltip("花朵预制体")]
        public GameObject flowerPrefab;
        [Tooltip("鹅卵石预制体")]
        public GameObject pebblePrefab;
        [Tooltip("裂缝预制体")]
        public GameObject crackPrefab;
        [Tooltip("蘑菇预制体")]
        public GameObject mushroomPrefab;

        [Header("生成设置")]
        [Tooltip("每个地图块最大障碍物数量")]
        public int maxObstaclesPerChunk = 5;

        [Tooltip("每个地图块最大装饰物数量")]
        public int maxDecorationsPerChunk = 8;

        [Header("权重设置")]
        [Tooltip("草地权重")]
        public float grassWeight = 0.4f;
        [Tooltip("泥土权重")]
        public float dirtWeight = 0.25f;
        [Tooltip("石头权重")]
        public float stoneWeight = 0.2f;
        [Tooltip("水权重")]
        public float waterWeight = 0.1f;
        [Tooltip("岩浆权重")]
        public float lavaWeight = 0.05f;

        [Tooltip("岩石权重")]
        public float rockWeight = 0.35f;
        [Tooltip("树木权重")]
        public float treeWeight = 0.3f;
        [Tooltip("墙壁权重")]
        public float wallWeight = 0.15f;
        [Tooltip("栅栏权重")]
        public float fenceWeight = 0.1f;
        [Tooltip("灌木权重")]
        public float bushWeight = 0.1f;

        // 存储已生成的地图块
        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, ChunkData> chunkDataMap = new Dictionary<Vector2Int, ChunkData>();

        // 地图块父级
        private Transform mapRoot;
        private Camera mainCamera;

        // 预制体数组
        private GameObject[] terrainPrefabs;
        private float[] terrainWeights;
        private GameObject[] obstaclePrefabs;
        private float[] obstacleWeights;
        private GameObject[] decorationPrefabs;
        private float[] decorationWeights;

        private void Awake()
        {
            mainCamera = Camera.main;
            mapRoot = new GameObject("MapRoot").transform;
            mapRoot.SetParent(transform);

            // 初始化预制体数组
            InitializePrefabs();
        }

        private void InitializePrefabs()
        {
            // 地形
            terrainPrefabs = new GameObject[] { 
                grassPrefab, dirtPrefab, stonePrefab, waterPrefab, lavaPrefab 
            };
            terrainWeights = new float[] { 
                grassWeight, dirtWeight, stoneWeight, waterWeight, lavaWeight 
            };

            // 障碍物
            obstaclePrefabs = new GameObject[] { 
                rockPrefab, treePrefab, wallPrefab, fencePrefab, bushPrefab 
            };
            obstacleWeights = new float[] { 
                rockWeight, treeWeight, wallWeight, fenceWeight, bushWeight 
            };

            // 装饰物
            decorationPrefabs = new GameObject[] { 
                grassPatchPrefab, flowerPrefab, pebblePrefab, crackPrefab, mushroomPrefab 
            };
            decorationWeights = new float[] { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f };
        }

        private void Start()
        {
            UpdateChunks();
        }

        private void Update()
        {
            UpdateChunks();
        }

        /// <summary>
        /// 根据摄像机位置更新可见地图块
        /// </summary>
        private void UpdateChunks()
        {
            if (mainCamera == null) return;

            Vector3 camPos = mainCamera.transform.position;
            Vector2Int camChunk = WorldToChunk(camPos);

            // 收集需要的块坐标
            HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int y = -viewDistance; y <= viewDistance; y++)
                {
                    neededChunks.Add(new Vector2Int(camChunk.x + x, camChunk.y + y));
                }
            }

            // 卸载不需要的块
            List<Vector2Int> toRemove = new List<Vector2Int>();
            foreach (var kvp in activeChunks)
            {
                if (!neededChunks.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var coord in toRemove)
            {
                Destroy(activeChunks[coord]);
                activeChunks.Remove(coord);
            }

            // 生成新块
            foreach (var coord in neededChunks)
            {
                if (!activeChunks.ContainsKey(coord))
                {
                    GenerateChunk(coord);
                }
            }
        }

        /// <summary>
        /// 生成单个地图块
        /// </summary>
        private void GenerateChunk(Vector2Int coord)
        {
            if (!chunkDataMap.ContainsKey(coord))
            {
                chunkDataMap[coord] = CreateChunkData(coord);
            }
            ChunkData data = chunkDataMap[coord];

            Vector3 worldPos = ChunkToWorld(coord);
            GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
            chunkObj.transform.SetParent(mapRoot);
            chunkObj.transform.position = worldPos;

            // 生成地面
            CreateTerrain(chunkObj, data);

            // 生成障碍物
            CreateObstacles(chunkObj, data);

            // 生成装饰物
            CreateDecorations(chunkObj, data);

            activeChunks[coord] = chunkObj;
            data.isGenerated = true;
        }

        private ChunkData CreateChunkData(Vector2Int coord)
        {
            ChunkData data = new ChunkData(coord);
            System.Random rng = new System.Random(coord.x * 73856093 ^ coord.y * 19349663);

            // 中心安全区
            if (generateSafeCenter && coord.x == 0 && coord.y == 0)
            {
                data.terrain = TerrainType.Grass;
                return data;
            }

            // 随机地形
            data.terrain = GetWeightedRandomTerrain(rng);

            // 障碍物数量（水和岩浆不生成障碍物）
            int obstacleCount = (data.terrain == TerrainType.Water || data.terrain == TerrainType.Lava) 
                ? 0 : rng.Next(0, maxObstaclesPerChunk + 1);
            data.obstacleCount = obstacleCount;

            // 装饰物数量
            data.decorationCount = rng.Next(0, maxDecorationsPerChunk + 1);

            return data;
        }

        private void CreateTerrain(GameObject chunkObj, ChunkData data)
        {
            int terrainIndex = (int)data.terrain;
            GameObject terrainPrefab = terrainIndex < terrainPrefabs.Length ? terrainPrefabs[terrainIndex] : null;
            
            if (terrainPrefab != null)
            {
                GameObject terrain = Instantiate(terrainPrefab, chunkObj.transform);
                // 设置地面渲染层级为最下层
                SetSortingLayerRecursively(terrain, -200, "Ground");
            }
            else
            {
                // 如果没有预制体，创建一个简单的方块作为占位
                GameObject ground = new GameObject("Ground");
                ground.transform.SetParent(chunkObj.transform);
                SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
                sr.color = GetTerrainColor(data.terrain);
                sr.sortingOrder = -200; // 最下层
                sr.sortingLayerName = "Ground";
                ground.transform.localScale = new Vector3(chunkSize, chunkSize, 1);
            }
        }

        private void SetSortingLayerRecursively(GameObject obj, int order, string layerName)
        {
            if (obj == null) return;
            
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = order;
                sr.sortingLayerName = layerName;
            }
            
            // 递归处理子对象
            foreach (Transform child in obj.transform)
            {
                SetSortingLayerRecursively(child.gameObject, order, layerName);
            }
        }

        private void CreateObstacles(GameObject chunkObj, ChunkData data)
        {
            if (obstaclePrefabs == null || data.obstacleCount == 0) return;

            System.Random rng = new System.Random(data.chunkCoord.x * 73856093 ^ data.chunkCoord.y * 19349663 ^ 111);
            float halfSize = chunkSize / 2f - 1f;

            for (int i = 0; i < data.obstacleCount; i++)
            {
                int obstacleIndex = GetWeightedRandomIndex(obstacleWeights, rng);
                GameObject obstaclePrefab = obstaclePrefabs[obstacleIndex];

                if (obstaclePrefab != null)
                {
                    Vector2 pos = new Vector2(
                        (float)(rng.NextDouble() * chunkSize - halfSize),
                        (float)(rng.NextDouble() * chunkSize - halfSize)
                    );
                    Instantiate(obstaclePrefab, chunkObj.transform).transform.localPosition = pos;
                }
            }
        }

        private void CreateDecorations(GameObject chunkObj, ChunkData data)
        {
            if (decorationPrefabs == null || data.decorationCount == 0) return;

            System.Random rng = new System.Random(data.chunkCoord.x * 73856093 ^ data.chunkCoord.y * 19349663 ^ 222);
            float halfSize = chunkSize / 2f - 0.5f;

            for (int i = 0; i < data.decorationCount; i++)
            {
                int decoIndex = GetWeightedRandomIndex(decorationWeights, rng);
                GameObject decoPrefab = decorationPrefabs[decoIndex];

                if (decoPrefab != null)
                {
                    Vector2 pos = new Vector2(
                        (float)(rng.NextDouble() * chunkSize - halfSize),
                        (float)(rng.NextDouble() * chunkSize - halfSize)
                    );
                    Instantiate(decoPrefab, chunkObj.transform).transform.localPosition = pos;
                }
            }
        }

        private int GetWeightedRandomIndex(float[] weights, System.Random rng)
        {
            float total = 0;
            foreach (float w in weights) total += w;

            float r = (float)rng.NextDouble() * total;
            float cumulative = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (r <= cumulative) return i;
            }
            return weights.Length - 1;
        }

        private TerrainType GetWeightedRandomTerrain(System.Random rng)
        {
            float total = 0;
            foreach (float w in terrainWeights) total += w;

            float r = (float)rng.NextDouble() * total;
            float cumulative = 0;

            for (int i = 0; i < terrainWeights.Length; i++)
            {
                cumulative += terrainWeights[i];
                if (r <= cumulative) return (TerrainType)i;
            }
            return TerrainType.Grass;
        }

        private Color GetTerrainColor(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Grass: return new Color(0.35f, 0.55f, 0.25f);
                case TerrainType.Dirt: return new Color(0.55f, 0.42f, 0.28f);
                case TerrainType.Stone: return new Color(0.5f, 0.5f, 0.5f);
                case TerrainType.Water: return new Color(0.2f, 0.4f, 0.7f);
                case TerrainType.Lava: return new Color(0.8f, 0.3f, 0.1f);
                default: return Color.green;
            }
        }

        #region 坐标转换

        public Vector2Int WorldToChunk(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / chunkSize);
            int y = Mathf.FloorToInt(worldPos.y / chunkSize);
            return new Vector2Int(x, y);
        }

        public Vector3 ChunkToWorld(Vector2Int chunkCoord)
        {
            return new Vector3(
                chunkCoord.x * chunkSize + chunkSize / 2f,
                chunkCoord.y * chunkSize + chunkSize / 2f,
                0
            );
        }

        #endregion

        public void ClearAllChunks()
        {
            foreach (var kvp in activeChunks)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            activeChunks.Clear();
            chunkDataMap.Clear();
        }
    }
}
