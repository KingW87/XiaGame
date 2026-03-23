using UnityEngine;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 经验宝石 - 玩家触碰后获得经验
    /// </summary>
    public class ExperiencePickup : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("磁力吸附半径")]
        public float magnetRadius = 3f;
        [Tooltip("吸附后飞向玩家的速度")]
        public float moveSpeed = 8f;
        
        private int experienceValue;
        private Transform playerTarget;
        private bool isAttracted;
        
        private void Start()
        {
            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        public void DoubleMagnetRange()
        {
            magnetRadius *= 2f;
        }
        
        private void Update()
        {
            if (playerTarget == null) return;
            
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            
            // 吸附范围
            if (distance <= magnetRadius)
            {
                isAttracted = true;
            }
            
            // 被吸附后飞向玩家
            if (isAttracted)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, 
                    playerTarget.position, 
                    moveSpeed * Time.deltaTime
                );
            }
        }
        
        public void Initialize(int value)
        {
            experienceValue = value;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<Player.PlayerController>();
                if (player != null)
                {
                    player.AddExperience(experienceValue);
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(SFXType.PickupExp);
                }
                Destroy(gameObject);
            }
        }
    }
}
