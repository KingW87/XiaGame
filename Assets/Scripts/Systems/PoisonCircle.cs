using UnityEngine;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 毒圈 - 最后一波时缩小，圈外持续掉血
    /// </summary>
    public class PoisonCircle : MonoBehaviour
    {
        [Tooltip("当前半径")]
        public float currentRadius;
        [Tooltip("最小半径")]
        public float minRadius = 5f;
        [Tooltip("收缩速度")]
        public float shrinkSpeed = 1f;

        private SpriteRenderer circleRenderer;
        private Player.PlayerController player;

        public void Initialize(float startRadius, float endRadius)
        {
            currentRadius = startRadius;
            minRadius = endRadius;

            // 创建可视化圆圈
            GameObject circleVisual = new GameObject("CircleVisual");
            circleVisual.transform.SetParent(transform);
            circleRenderer = circleVisual.AddComponent<SpriteRenderer>();
            circleRenderer.sprite = CreateCircleSprite();
            circleRenderer.color = new Color(0.5f, 0f, 0.5f, 0.4f); // 紫色半透明
            circleRenderer.sortingOrder = 100; // 确保在最上层显示

            UpdateCircleSize();

            player = FindObjectOfType<Player.PlayerController>();
        }

        private void Update()
        {
            // 跟随玩家位置
            if (player != null)
            {
                transform.position = player.transform.position;
            }
            
            // 收缩毒圈
            if (currentRadius > minRadius)
            {
                currentRadius -= shrinkSpeed * Time.deltaTime;
                UpdateCircleSize();
            }
        }

        private void UpdateCircleSize()
        {
            if (circleRenderer != null)
            {
                circleRenderer.transform.position = transform.position;
                circleRenderer.transform.localScale = Vector3.one * currentRadius * 2f;
            }
        }

        private Sprite CreateCircleSprite()
        {
            // 创建一个简单的圆形精灵
            int resolution = 64;
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] colors = new Color[resolution * resolution];
            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normalizedDist = dist / (resolution / 2f);

                    if (normalizedDist <= 0.9f)
                    {
                        // 圈内 - 边缘渐变
                        float alpha = Mathf.Clamp01(1f - (normalizedDist - 0.7f) / 0.2f);
                        colors[y * resolution + x] = new Color(0.5f, 0f, 0.5f, alpha * 0.3f);
                    }
                    else if (normalizedDist <= 1f)
                    {
                        // 边缘 - 边框
                        colors[y * resolution + x] = new Color(0.8f, 0f, 0.8f, 0.8f);
                    }
                    else
                    {
                        colors[y * resolution + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 检查玩家是否在毒圈内
        /// </summary>
        public bool IsPlayerInside()
        {
            if (player == null) return true;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            return dist <= currentRadius;
        }
    }
}
