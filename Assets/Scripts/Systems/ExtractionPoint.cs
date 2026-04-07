using UnityEngine;
using System;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 撤离点 - Boss死亡后出现，玩家到达后完成撤离
    /// </summary>
    public class ExtractionPoint : MonoBehaviour
    {
        [Tooltip("撤离点半径")]
        public float radius = 3f;

        [Tooltip("撤离持续时间")]
        public float duration = 30f;

        [Tooltip("剩余时间")]
        public float remainingTime;

        private SpriteRenderer sr;
        private Player.PlayerController player;
        private bool isActive;
        private float timer;

        public event Action OnPlayerExtracted;

        private void Awake()
        {
            // 创建可视化
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(transform);
            sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -1;
            UpdateVisual();

            player = FindObjectOfType<Player.PlayerController>();
            gameObject.SetActive(false);
        }

        private void UpdateVisual()
        {
            if (sr != null)
            {
                Texture2D texture = new Texture2D(64, 64);
                Color[] colors = new Color[64 * 64];
                Vector2 center = new Vector2(32, 32);

                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center) / 32f;
                        if (dist < 0.8f)
                        {
                            colors[y * 64 + x] = new Color(0f, 1f, 0f, 0.3f);
                        }
                        else if (dist < 0.9f)
                        {
                            colors[y * 64 + x] = new Color(0f, 1f, 0f, 0.8f);
                        }
                        else
                        {
                            colors[y * 64 + x] = Color.clear;
                        }
                    }
                }

                texture.SetPixels(colors);
                texture.Apply();
                sr.sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
                sr.transform.localScale = Vector3.one * radius * 2f;
            }
        }

        /// <summary>
        /// 激活撤离点
        /// </summary>
        public void Activate(float durationSeconds)
        {
            duration = durationSeconds;
            remainingTime = duration;
            timer = 0;
            isActive = true;
            gameObject.SetActive(true);
            transform.position = GetExtractionPosition();

            Debug.Log($"[Extraction] 撤离点已激活！位置: {transform.position}, 持续时间: {duration}秒");
        }

        /// <summary>
        /// 获取撤离点位置（玩家附近的随机位置）
        /// </summary>
        private Vector2 GetExtractionPosition()
        {
            if (player == null) return Vector2.zero;

            // 在玩家前方一定距离生成
            Vector2 forward = player.transform.right;
            if (forward == Vector2.zero) forward = Vector2.up;

            return (Vector2)player.transform.position + forward * 10f;
        }

        private void Update()
        {
            if (!isActive) return;

            timer += Time.deltaTime;
            remainingTime = duration - timer;

            // 旋转动画
            if (sr != null)
            {
                sr.transform.Rotate(Vector3.forward, -60f * Time.deltaTime);
            }

            // 检查玩家是否到达
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= radius)
                {
                    CompleteExtraction();
                }
            }

            // 时间到
            if (remainingTime <= 0)
            {
                EndExtraction();
            }
        }

        private void CompleteExtraction()
        {
            Debug.Log("[Extraction] 玩家已撤离！");
            OnPlayerExtracted?.Invoke();
            isActive = false;
            gameObject.SetActive(false);
        }

        private void EndExtraction()
        {
            Debug.Log("[Extraction] 撤离时间结束");
            isActive = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 是否正在撤离中
        /// </summary>
        public bool IsActive() => isActive;
    }
}
