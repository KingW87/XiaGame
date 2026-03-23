using UnityEngine;

namespace ClawSurvivor.Weapons
{
    public class Bullet : MonoBehaviour
    {
        private int damage;

        public void Initialize(int damage)
        {
            this.damage = damage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy.EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
