using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ClawSurvivor.Enemy;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 小地图控制器 - 在右上角显示实时游戏地图
    /// </summary>
    public class MiniMap : MonoBehaviour
    {
        [Header("小地图设置")]
        [Tooltip("小地图尺寸（像素）")]
        public float mapSize = 150f;

        [Tooltip("小地图世界范围（边长）")]
        public float worldSize = 100f;

        [Header("图标设置")]
        [Tooltip("玩家图标颜色")]
        public Color playerColor = Color.cyan;

        [Tooltip("敌人图标颜色")]
        public Color enemyColor = Color.red;

        [Tooltip("玩家图标大小")]
        public float playerIconSize = 8f;

        [Tooltip("敌人图标大小")]
        public float enemyIconSize = 5f;

        // 组件引用
        private RectTransform containerRT;
        private Image playerIcon;
        private Transform playerTransform;

        // 敌人图标池
        private List<Image> enemyIcons = new List<Image>();

        // 世界坐标到小地图坐标的转换比例
        private float scale => mapSize / worldSize;

        private void Awake()
        {
            Debug.Log("[MiniMap] Awake 被调用");
            // 创建小地图容器
            CreateMapContainer();
        }

        private void OnEnable()
        {
            Debug.Log("[MiniMap] OnEnable 被调用");
        }

        private void CreateMapContainer()
        {
            Debug.Log("[MiniMap] CreateMapContainer 开始");
            
            // 设置自身的RectTransform
            containerRT = GetComponent<RectTransform>();
            if (containerRT == null)
            {
                containerRT = gameObject.AddComponent<RectTransform>();
            }

            Debug.Log("[MiniMap] RectTransform 创建完成");

            // 设置位置到右上角
            containerRT.anchorMin = new Vector2(1, 1);
            containerRT.anchorMax = new Vector2(1, 1);
            containerRT.pivot = new Vector2(1, 1);
            containerRT.anchoredPosition = new Vector2(-80, -80);
            containerRT.sizeDelta = new Vector2(mapSize, mapSize);

            // 设置背景
            Image bg = GetComponent<Image>();
            if (bg == null)
            {
                bg = gameObject.AddComponent<Image>();
            }
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // 创建边框
            CreateBorder();

            // 创建玩家图标
            CreatePlayerIcon();

            Debug.Log("[MiniMap] 小地图容器创建完成");
        }

        private void CreateBorder()
        {
            // 边框颜色
            Color borderColor = Color.white;

            // 上边框
            CreateBorderLine(new Vector2(0, mapSize/2), new Vector2(mapSize, 2), borderColor);
            // 下边框
            CreateBorderLine(new Vector2(0, -mapSize/2), new Vector2(mapSize, 2), borderColor);
            // 左边框
            CreateBorderLine(new Vector2(-mapSize/2, 0), new Vector2(2, mapSize), borderColor);
            // 右边框
            CreateBorderLine(new Vector2(mapSize/2, 0), new Vector2(2, mapSize), borderColor);
        }

        private void CreateBorderLine(Vector2 pos, Vector2 size, Color color)
        {
            GameObject line = new GameObject("Border");
            line.transform.SetParent(transform);
            RectTransform rt = line.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = line.AddComponent<Image>();
            img.color = color;
        }

        private void CreatePlayerIcon()
        {
            GameObject iconGO = new GameObject("PlayerIcon");
            iconGO.transform.SetParent(transform);
            
            RectTransform rt = iconGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(playerIconSize, playerIconSize);
            
            Image img = iconGO.AddComponent<Image>();
            img.color = playerColor;
            
            playerIcon = img;
        }

        private void Start()
        {
            // 获取玩家transform
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // 尝试通过组件查找
                Player.PlayerController pc = FindObjectOfType<Player.PlayerController>();
                if (pc != null)
                {
                    playerTransform = pc.transform;
                }
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("[MiniMap] 未找到玩家！");
            }
            else
            {
                Debug.Log("[MiniMap] 找到玩家: " + playerTransform.name);
            }
        }

        private void LateUpdate()
        {
            UpdatePlayerPosition();
            UpdateEnemyPositions();
        }

        private void UpdatePlayerPosition()
        {
            if (playerIcon == null || playerTransform == null) return;

            // 玩家始终固定在小地图中心
            playerIcon.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void UpdateEnemyPositions()
        {
            // 查找所有敌人
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            
            if (enemies.Length > 0)
            {
                Debug.Log($"[MiniMap] 发现 {enemies.Length} 个敌人");
            }
            
            // 确保图标池足够大
            while (enemyIcons.Count < enemies.Length)
            {
                CreateEnemyIcon();
            }

            // 更新敌人位置
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) continue;

                if (i < enemyIcons.Count)
                {
                    enemyIcons[i].gameObject.SetActive(true);
                    
                    // 计算敌人相对于玩家的位置
                    Vector3 enemyPos = enemies[i].transform.position;
                    Vector3 playerPos = playerTransform.position;
                    Vector2 offset = new Vector2(enemyPos.x - playerPos.x, enemyPos.y - playerPos.y) * scale;
                    
                    // 限制在小地图范围内
                    float halfSize = mapSize / 2f - enemyIconSize;
                    offset.x = Mathf.Clamp(offset.x, -halfSize, halfSize);
                    offset.y = Mathf.Clamp(offset.y, -halfSize, halfSize);
                    
                    enemyIcons[i].rectTransform.anchoredPosition = offset;
                }
            }

            // 隐藏多余的敌人图标
            for (int i = enemies.Length; i < enemyIcons.Count; i++)
            {
                enemyIcons[i].gameObject.SetActive(false);
            }
        }

        private void CreateEnemyIcon()
        {
            GameObject iconGO = new GameObject("EnemyIcon");
            iconGO.transform.SetParent(transform);
            
            RectTransform rt = iconGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(enemyIconSize, enemyIconSize);
            
            Image img = iconGO.AddComponent<Image>();
            img.color = enemyColor;
            
            enemyIcons.Add(img);
        }
    }
}
