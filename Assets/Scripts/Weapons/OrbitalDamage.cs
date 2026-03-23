using UnityEngine;

namespace ClawSurvivor.Weapons
{
    public class OrbitalDamage : MonoBehaviour
    {
        private int damage;
        private Transform owner;
        private float hitCooldown;
        private float hitTimer;

        public void Initialize(int damage, Transform owner)
        {
            this.damage = damage;
            this.owner = owner;
            hitCooldown = 0.5f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<Enemy.EnemyController>();
            if (enemy != null)
            {
                hitTimer += Time.deltaTime;
                if (hitTimer >= hitCooldown)
                {
                    enemy.TakeDamage(damage);
                    hitTimer = 0f;
                }
            }
        }
    }
}
