using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Map
{
    /// <summary>
    /// 无限地图生成器 - 基于摄像机位置动态加载/卸载地图块
    /// 挂载到场景中的空GameObject上
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

        [Header("地形颜色")]
        public Color grassColor = new Color(0.35f, 0.55f, 0.25f);
        public Color dirtColor = new Color(0.55f, 0.42f, 0.28f);
        public Color stoneColor = new Color(0.5f, 0.5f, 0.5f);
        public Color waterColor = new Color(0.2f, 0.4f, 0.7f);
        public Color lavaColor = new Color(0.8f, 0.3f, 0.1f);

        [Header("障碍物设置")]
        [Tooltip("每个地图块最大障碍物数量")]
        public int maxObstaclesPerChunk = 5;

        [Tooltip("每个地图块最大装饰物数量")]
        public int maxDecorationsPerChunk = 8;

        [Tooltip("障碍物碰撞半径")]
        public float obstacleCollisionRadius = 0.5f;

        [Tooltip("障碍物颜色")]
        public Color rockColor = new Color(0.45f, 0.45f, 0.42f);
        public Color treeTrunkColor = new Color(0.4f, 0.28f, 0.15f);
        public Color treeLeafColor = new Color(0.2f, 0.5f, 0.15f);
        public Color wallColor = new Color(0.4f, 0.38f, 0.35f);
        public Color bushColor = new Color(0.3f, 0.55f, 0.2f);

        [Tooltip("装饰物颜色")]
        public Color flowerColor1 = new Color(0.9f, 0.3f, 0.3f);
        public Color flowerColor2 = new Color(0.9f, 0.9f, 0.2f);
        public Color pebbleColor = new Color(0.6f, 0.58f, 0.55f);
        public Color mushroomColor = new Color(0.85f, 0.3f, 0.2f);

        // 存储已生成的地图块
        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, ChunkData> chunkDataMap = new Dictionary<Vector2Int, ChunkData>();

        // 地图块父级
        private Transform mapRoot;
        private Camera mainCamera;

        // 地形类型权重（用于随机选择）
        private static readonly float[] terrainWeights = { 0.55f, 0.25f, 0.12f, 0.05f, 0.03f };
        private static readonly TerrainType[] terrainTypes = {
            TerrainType.Grass, TerrainType.Dirt, TerrainType.Stone,
            TerrainType.Water, TerrainType.Lava
        };

        // 障碍物类型权重
        private static readonly float[] obstacleWeights = { 0.35f, 0.30f, 0.15f, 0.10f, 0.10f };
        private static readonly ObstacleType[] obstacleTypes = {
            ObstacleType.Rock, ObstacleType.Tree, ObstacleType.Wall,
            ObstacleType.Fence, ObstacleType.Bush
        };

        // 装饰物类型权重
        private static readonly float[] decorationWeights = { 0.30f, 0.25f, 0.20f, 0.15f, 0.10f };
        private static readonly DecorationType[] decorationTypes = {
            DecorationType.GrassPatch, DecorationType.Flower, DecorationType.Pebble,
            DecorationType.Crack, DecorationType.Mushroom
        };

        private System.Random seededRandom;

        private void Awake()
        {
            mainCamera = Camera.main;
            mapRoot = new GameObject("MapRoot").transform;
            mapRoot.SetParent(transform);
            seededRandom = new System.Random(42);
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

            // 计算摄像机所在块坐标
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
            // 获取或创建块数据
            if (!chunkDataMap.ContainsKey(coord))
            {
                chunkDataMap[coord] = CreateChunkData(coord);
            }
            ChunkData data = chunkDataMap[coord];

            // 创建块根对象
            Vector3 worldPos = ChunkToWorld(coord);
            GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
            chunkObj.transform.SetParent(mapRoot);
            chunkObj.transform.position = worldPos;

            // 生成地面
            CreateGround(chunkObj, data);

            // 生成障碍物
            CreateObstacles(chunkObj, data);

            // 生成装饰物
            CreateDecorations(chunkObj, data);

            activeChunks[coord] = chunkObj;
            data.isGenerated = true;
        }

        /// <summary>
        /// 用确定性随机创建块数据
        /// </summary>
        private ChunkData CreateChunkData(Vector2Int coord)
        {
            ChunkData data = new ChunkData(coord);

            // 确定性随机种子（基于坐标）
            int seed = coord.x * 73856093 ^ coord.y * 19349663;
            System.Random rng = new System.Random(seed);

            // 中心安全区
            if (generateSafeCenter && coord.x == 0 && coord.y == 0)
            {
                data.terrain = TerrainType.Grass;
                return data;
            }

            // 随机地形
            data.terrain = GetWeightedRandom(terrainTypes, terrainWeights, rng);

            // 如果是不可通行地形，减少障碍物
            int obstacleCount = data.terrain == TerrainType.Water || data.terrain == TerrainType.Lava
                ? Mathf.RoundToInt(rng.Next(0, 2))
                : rng.Next(1, maxObstaclesPerChunk + 1);

            // 生成障碍物数据
            data.obstacles = new ObstacleType[obstacleCount];
            data.obstaclePositionsX = new float[obstacleCount];
            data.obstaclePositionsY = new float[obstacleCount];

            float halfSize = chunkSize / 2f;
            float padding = 1f;

            for (int i = 0; i < obstacleCount; i++)
            {
                data.obstacles[i] = GetWeightedRandom(obstacleTypes, obstacleWeights, rng);
                data.obstaclePositionsX[i] = (float)(rng.NextDouble() * (chunkSize - padding * 2) - halfSize + padding);
                data.obstaclePositionsY[i] = (float)(rng.NextDouble() * (chunkSize - padding * 2) - halfSize + padding);
            }

            // 生成装饰物数据
            int decoCount = rng.Next(2, maxDecorationsPerChunk + 1);
            data.decorations = new DecorationType[decoCount];

            for (int i = 0; i < decoCount; i++)
            {
                data.decorations[i] = GetWeightedRandom(decorationTypes, decorationWeights, rng);
            }

            return data;
        }

        /// <summary>
        /// 创建地面
        /// </summary>
        private void CreateGround(GameObject chunkObj, ChunkData data)
        {
            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(chunkObj.transform);

            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = GetTerrainColor(data.terrain);
            sr.sortingOrder = -100;

            BoxCollider2D col = ground.AddComponent<BoxCollider2D>();
            col.size = new Vector2(chunkSize, chunkSize);

            // 水和岩浆区域标记为trigger，用于触发效果
            if (data.terrain == TerrainType.Water || data.terrain == TerrainType.Lava)
            {
                col.isTrigger = true;
                TerrainHazard hazard = ground.AddComponent<TerrainHazard>();
                hazard.terrainType = data.terrain;
                if (data.terrain == TerrainType.Lava)
                    hazard.damagePerSecond = 5;
            }
        }

        /// <summary>
        /// 创建障碍物
        /// </summary>
        private void CreateObstacles(GameObject chunkObj, ChunkData data)
        {
            for (int i = 0; i < data.obstacles.Length; i++)
            {
                Vector2 localPos = new Vector2(data.obstaclePositionsX[i], data.obstaclePositionsY[i]);
                CreateObstacle(chunkObj.transform, data.obstacles[i], localPos, i);
            }
        }

        /// <summary>
        /// 创建单个障碍物
        /// </summary>
        private void CreateObstacle(Transform parent, ObstacleType type, Vector2 localPos, int index)
        {
            switch (type)
            {
                case ObstacleType.Rock:
                    CreateRock(parent, localPos, index);
                    break;
                case ObstacleType.Tree:
                    CreateTree(parent, localPos, index);
                    break;
                case ObstacleType.Wall:
                    CreateWall(parent, localPos, index);
                    break;
                case ObstacleType.Fence:
                    CreateFence(parent, localPos, index);
                    break;
                case ObstacleType.Bush:
                    CreateBush(parent, localPos, index);
                    break;
            }
        }

        private void CreateRock(Transform parent, Vector2 pos, int index)
        {
            GameObject rock = new GameObject($"Rock_{index}");
            rock.transform.SetParent(parent);
            rock.transform.localPosition = pos;

            // 岩石主体
            SpriteRenderer sr = rock.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = rockColor;
            sr.sortingOrder = 1;

            // 随机大小
            float scale = 0.6f + Random.value * 0.5f;
            rock.transform.localScale = new Vector3(scale, scale, 1f);

            CircleCollider2D col = rock.AddComponent<CircleCollider2D>();
            col.radius = obstacleCollisionRadius * scale;
        }

        private void CreateTree(Transform parent, Vector2 pos, int index)
        {
            GameObject tree = new GameObject($"Tree_{index}");
            tree.transform.SetParent(parent);
            tree.transform.localPosition = pos;

            // 树干
            GameObject trunk = new GameObject("Trunk");
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0, 0.3f, 0);
            SpriteRenderer trunkSr = trunk.AddComponent<SpriteRenderer>();
            trunkSr.sprite = CreateRectSprite(0.3f, 0.6f);
            trunkSr.color = treeTrunkColor;
            trunkSr.sortingOrder = 0;
            BoxCollider2D trunkCol = trunk.AddComponent<BoxCollider2D>();
            trunkCol.size = new Vector2(0.3f, 0.6f);
            trunkCol.offset = new Vector2(0, 0.3f);

            // 树冠
            GameObject leaves = new GameObject("Leaves");
            leaves.transform.SetParent(tree.transform);
            leaves.transform.localPosition = new Vector3(0, 1.0f, 0);
            SpriteRenderer leafSr = leaves.AddComponent<SpriteRenderer>();
            leafSr.sprite = CreateCircleSprite();
            leafSr.color = treeLeafColor;
            leafSr.sortingOrder = 2;
            leaves.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }

        private void CreateWall(Transform parent, Vector2 pos, int index)
        {
            GameObject wall = new GameObject($"Wall_{index}");
            wall.transform.SetParent(parent);
            wall.transform.localPosition = pos;

            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite(2f, 0.6f);
            sr.color = wallColor;
            sr.sortingOrder = 1;

            BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 0.6f);

            // 随机旋转
            if (Random.value > 0.5f)
                wall.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        private void CreateFence(Transform parent, Vector2 pos, int index)
        {
            GameObject fence = new GameObject($"Fence_{index}");
            fence.transform.SetParent(parent);
            fence.transform.localPosition = pos;

            SpriteRenderer sr = fence.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite(1.5f, 0.3f);
            sr.color = new Color(0.6f, 0.45f, 0.25f);
            sr.sortingOrder = 1;

            BoxCollider2D col = fence.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 0.3f);

            if (Random.value > 0.5f)
                fence.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        private void CreateBush(Transform parent, Vector2 pos, int index)
        {
            GameObject bush = new GameObject($"Bush_{index}");
            bush.transform.SetParent(parent);
            bush.transform.localPosition = pos;

            SpriteRenderer sr = bush.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = bushColor;
            sr.sortingOrder = 1;

            float scale = 0.5f + Random.value * 0.4f;
            bush.transform.localScale = new Vector3(scale, scale, 1f);

            // 灌木不阻挡移动，只有trigger碰撞
            CircleCollider2D col = bush.AddComponent<CircleCollider2D>();
            col.radius = obstacleCollisionRadius * scale;
            col.isTrigger = true;
        }

        /// <summary>
        /// 创建装饰物
        /// </summary>
        private void CreateDecorations(GameObject chunkObj, ChunkData data)
        {
            float halfSize = chunkSize / 2f;
            System.Random rng = new System.Random(data.chunkCoord.x * 73856093 ^ data.chunkCoord.y * 19349663 ^ 999);

            for (int i = 0; i < data.decorations.Length; i++)
            {
                float px = (float)(rng.NextDouble() * (chunkSize - 2f) - halfSize + 1f);
                float py = (float)(rng.NextDouble() * (chunkSize - 2f) - halfSize + 1f);
                CreateDecoration(chunkObj.transform, data.decorations[i], new Vector2(px, py), i);
            }
        }

        private void CreateDecoration(Transform parent, DecorationType type, Vector2 pos, int index)
        {
            switch (type)
            {
                case DecorationType.GrassPatch:
                    CreateGrassPatch(parent, pos, index);
                    break;
                case DecorationType.Flower:
                    CreateFlower(parent, pos, index);
                    break;
                case DecorationType.Pebble:
                    CreatePebble(parent, pos, index);
                    break;
                case DecorationType.Crack:
                    CreateCrack(parent, pos, index);
                    break;
                case DecorationType.Mushroom:
                    CreateMushroom(parent, pos, index);
                    break;
            }
        }

        private void CreateGrassPatch(Transform parent, Vector2 pos, int index)
        {
            GameObject grass = new GameObject($"Grass_{index}");
            grass.transform.SetParent(parent);
            grass.transform.localPosition = pos;

            SpriteRenderer sr = grass.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(0.3f, 0.6f + Random.value * 0.15f, 0.2f);
            sr.sortingOrder = -50;

            float scale = 0.3f + Random.value * 0.4f;
            grass.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void CreateFlower(Transform parent, Vector2 pos, int index)
        {
            GameObject flower = new GameObject($"Flower_{index}");
            flower.transform.SetParent(parent);
            flower.transform.localPosition = pos;

            SpriteRenderer sr = flower.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = Random.value > 0.5f ? flowerColor1 : flowerColor2;
            sr.sortingOrder = -50;

            flower.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        }

        private void CreatePebble(Transform parent, Vector2 pos, int index)
        {
            GameObject pebble = new GameObject($"Pebble_{index}");
            pebble.transform.SetParent(parent);
            pebble.transform.localPosition = pos;

            SpriteRenderer sr = pebble.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = pebbleColor;
            sr.sortingOrder = -50;

            float scale = 0.15f + Random.value * 0.2f;
            pebble.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void CreateCrack(Transform parent, Vector2 pos, int index)
        {
            GameObject crack = new GameObject($"Crack_{index}");
            crack.transform.SetParent(parent);
            crack.transform.localPosition = pos;

            SpriteRenderer sr = crack.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite(1.0f, 0.08f);
            sr.color = new Color(0.25f, 0.22f, 0.18f);
            sr.sortingOrder = -50;

            crack.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 180f));
        }

        private void CreateMushroom(Transform parent, Vector2 pos, int index)
        {
            GameObject mushroom = new GameObject($"Mushroom_{index}");
            mushroom.transform.SetParent(parent);
            mushroom.transform.localPosition = pos;

            // 蘑菇柄
            GameObject stem = new GameObject("Stem");
            stem.transform.SetParent(mushroom.transform);
            stem.transform.localPosition = new Vector3(0, 0.05f, 0);
            SpriteRenderer stemSr = stem.AddComponent<SpriteRenderer>();
            stemSr.sprite = CreateRectSprite(0.08f, 0.15f);
            stemSr.color = new Color(0.9f, 0.88f, 0.8f);
            stemSr.sortingOrder = -50;

            // 蘑菇帽
            GameObject cap = new GameObject("Cap");
            cap.transform.SetParent(mushroom.transform);
            cap.transform.localPosition = new Vector3(0, 0.15f, 0);
            SpriteRenderer capSr = cap.AddComponent<SpriteRenderer>();
            capSr.sprite = CreateCircleSprite();
            capSr.color = mushroomColor;
            capSr.sortingOrder = -50;
            cap.transform.localScale = new Vector3(0.25f, 0.2f, 1f);
        }

        #region 坐标转换

        /// <summary>
        /// 世界坐标 -> 块坐标
        /// </summary>
        public Vector2Int WorldToChunk(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / chunkSize);
            int y = Mathf.FloorToInt(worldPos.y / chunkSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 块坐标 -> 世界坐标（块中心）
        /// </summary>
        public Vector3 ChunkToWorld(Vector2Int chunkCoord)
        {
            return new Vector3(
                chunkCoord.x * chunkSize + chunkSize / 2f,
                chunkCoord.y * chunkSize + chunkSize / 2f,
                0
            );
        }

        #endregion

        #region 辅助方法

        private Color GetTerrainColor(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Grass: return grassColor;
                case TerrainType.Dirt: return dirtColor;
                case TerrainType.Stone: return stoneColor;
                case TerrainType.Water: return waterColor;
                case TerrainType.Lava: return lavaColor;
                default: return grassColor;
            }
        }

        private T GetWeightedRandom<T>(T[] items, float[] weights, System.Random rng)
        {
            float total = 0;
            for (int i = 0; i < weights.Length; i++) total += weights[i];

            float r = (float)rng.NextDouble() * total;
            float cumulative = 0;

            for (int i = 0; i < items.Length; i++)
            {
                cumulative += weights[i];
                if (r <= cumulative) return items[i];
            }
            return items[items.Length - 1];
        }

        /// <summary>
        /// 创建正方形Sprite
        /// </summary>
        private Sprite CreateSquareSprite()
        {
            return CreateRectSprite(1f, 1f);
        }

        /// <summary>
        /// 创建圆形Sprite
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            return CreateRectSprite(1f, 1f);
        }

        /// <summary>
        /// 创建矩形Sprite
        /// </summary>
        private Sprite CreateRectSprite(float width, float height)
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 1f);
        }

        /// <summary>
        /// 获取指定世界位置的地形类型
        /// </summary>
        public TerrainType GetTerrainAt(Vector3 worldPos)
        {
            Vector2Int coord = WorldToChunk(worldPos);
            if (chunkDataMap.TryGetValue(coord, out ChunkData data))
            {
                return data.terrain;
            }
            return TerrainType.Grass;
        }

        /// <summary>
        /// 清除所有地图块（场景切换时调用）
        /// </summary>
        public void ClearAllChunks()
        {
            foreach (var kvp in activeChunks)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            activeChunks.Clear();
            chunkDataMap.Clear();
        }

        #endregion
    }
}
