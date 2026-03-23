using UnityEngine;

namespace ClawSurvivor.Enemy
{
    /// <summary>
    /// 敌人弹丸 - 飞向玩家造成伤害
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private int damage;
        private float speed;
        private Transform target;

        public void Initialize(int damage, float speed, Transform target)
        {
            this.damage = damage;
            this.speed = speed;
            this.target = target;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;

            // 超出范围销毁
            if (Vector2.Distance(transform.position, target.position) > 20f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<Player.PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
