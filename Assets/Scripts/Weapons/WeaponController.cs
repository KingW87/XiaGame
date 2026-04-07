using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Weapons
{
    /// <summary>
    /// 武器控制器 - 管理玩家武器和攻击行为
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("当前武器")]
        [Tooltip("当前装备的武器数据")]
        public WeaponData currentWeapon;

        [Header("武器ID")]
        public int currentWeaponId = 0; // 在AllWeapons数组中的索引

        private Player.PlayerController player;
        private float attackTimer;
        private List<GameObject> orbitalObjects = new List<GameObject>();

        public WeaponData CurrentWeapon => currentWeapon;
        public int CurrentWeaponId => currentWeaponId;
        public System.Action<WeaponData> OnWeaponChanged;

        private void Start()
        {
            player = GetComponent<Player.PlayerController>();
            
            // 确保升级系统存在
            if (WeaponUpgradeSystem.Instance == null)
            {
                GameObject go = new GameObject("WeaponUpgradeSystem");
                go.AddComponent<WeaponUpgradeSystem>();
            }
            
            // 默认武器：短剑
            currentWeaponId = 0;
            currentWeapon = WeaponData.AllWeapons[0];
            
            // 应用升级属性
            ApplyWeaponStats();
        }

        /// <summary>
        /// 应用武器升级/进化属性
        /// </summary>
        public void ApplyWeaponStats()
        {
            var info = WeaponUpgradeSystem.Instance.GetWeaponInfo(currentWeaponId);
            
            // 如果已进化，使用进化后的数据
            if (info.isEvolved && currentWeapon.evolvedWeapon != null)
            {
                currentWeapon = currentWeapon.evolvedWeapon;
            }
            
            // 应用攻速
            attackTimer = 0f;
        }

        private void Update()
        {
            // 计算实际攻速（考虑升级加成）
            float actualAttackSpeed = GetActualAttackSpeed();
            
            attackTimer += Time.deltaTime;

            switch (currentWeapon.type)
            {
                case WeaponType.Melee:
                    if (attackTimer >= actualAttackSpeed)
                    {
                        MeleeAttack();
                        attackTimer = 0f;
                    }
                    break;
                case WeaponType.Projectile:
                    if (attackTimer >= actualAttackSpeed)
                    {
                        ProjectileAttack();
                        attackTimer = 0f;
                    }
                    break;
                case WeaponType.Orbital:
                    UpdateOrbital();
                    break;
            }
        }

        /// <summary>
        /// 获取实际攻击速度（考虑升级）
        /// </summary>
        private float GetActualAttackSpeed()
        {
            if (WeaponUpgradeSystem.Instance != null)
            {
                return WeaponUpgradeSystem.Instance.GetActualAttackSpeed(currentWeaponId);
            }
            return currentWeapon.attackSpeed;
        }

        private void MeleeAttack()
        {
            // 找最近敌人
            var enemies = Enemy.EnemyController.AllEnemies;
            Enemy.EnemyController nearest = null;
            float minDist = currentWeapon.range + 0.5f;

            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDist) { minDist = dist; nearest = enemy; }
            }

            if (nearest != null)
            {
                // 扇形范围判定 - 使用升级系统的伤害计算
                Vector2 dir = (nearest.transform.position - transform.position).normalized;
                int finalDamage = GetActualDamage();

                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentWeapon.range);
                foreach (var col in hits)
                {
                    var enemy = col.GetComponent<Enemy.EnemyController>();
                    if (enemy != null)
                    {
                        Vector2 enemyDir = (enemy.transform.position - transform.position).normalized;
                        if (Vector2.Dot(dir, enemyDir) > 0.3f)
                            enemy.TakeDamage(finalDamage);
                    }
                }

                // 显示挥砍特效
                CreateSlashEffect(dir);
            }
        }

        /// <summary>
        /// 获取实际伤害（考虑升级）
        /// </summary>
        private int GetActualDamage()
        {
            float damage = currentWeapon.baseDamage * player.DamageMultiplier;
            if (WeaponUpgradeSystem.Instance != null)
            {
                damage = WeaponUpgradeSystem.Instance.GetActualDamage(currentWeaponId) * player.DamageMultiplier;
            }
            return Mathf.RoundToInt(damage);
        }

        private void CreateSlashEffect(Vector2 direction)
        {
            GameObject slash = new GameObject("SlashEffect");
            slash.transform.position = transform.position;
            slash.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            SpriteRenderer sr = slash.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = new Color(1, 1, 1, 0.5f);
            sr.sortingOrder = 15;
            slash.transform.localScale = new Vector3(currentWeapon.range * 2, 1f, 1);

            Destroy(slash, 0.15f);
        }

        private void ProjectileAttack()
        {
            var enemies = Enemy.EnemyController.AllEnemies;
            if (enemies.Count == 0) return;

            int actualDamage = GetActualDamage();

            if (currentWeapon.projectileCount == 1)
            {
                // 单发：瞄准最近敌人
                Enemy.EnemyController nearest = FindNearest(currentWeapon.range);
                if (nearest == null) return;

                FireBullet(nearest.transform.position, actualDamage);
            }
            else
            {
                // 多发：扇形散射
                Enemy.EnemyController nearest = FindNearest(currentWeapon.range);
                if (nearest == null) return;

                Vector2 baseDir = (nearest.transform.position - transform.position).normalized;
                float spreadAngle = 20f;
                for (int i = 0; i < currentWeapon.projectileCount; i++)
                {
                    float angle = -spreadAngle + (spreadAngle * 2f / (currentWeapon.projectileCount - 1)) * i;
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
                    Vector2 target = (Vector2)transform.position + dir * currentWeapon.range;
                    FireBullet(target, actualDamage);
                }
            }
        }

        private void FireBullet(Vector3 targetPos, int damage)
        {
            GameObject bullet = new GameObject("Bullet");
            bullet.transform.position = transform.position;

            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = currentWeapon.color;
            sr.sortingOrder = 12;
            bullet.transform.localScale = Vector3.one * 0.2f;

            bullet.AddComponent<BoxCollider2D>().isTrigger = true;
            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
            rb.velocity = dir * currentWeapon.projectileSpeed;

            bullet.AddComponent<Bullet>().Initialize(damage);
            Destroy(bullet, 3f);
        }

        private void UpdateOrbital()
        {
            // 确保环绕物数量
            while (orbitalObjects.Count < 2)
            {
                GameObject orb = new GameObject("Orbital");
                orb.transform.SetParent(transform);

                SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
                sr.sprite = CreateSimpleSprite();
                sr.color = currentWeapon.color;
                sr.sortingOrder = 11;
                orb.transform.localScale = Vector3.one * 0.3f;

                orb.AddComponent<BoxCollider2D>().isTrigger = true;
                int actualDamage = GetActualDamage();
                orb.AddComponent<OrbitalDamage>().Initialize(actualDamage, transform);
                orbitalObjects.Add(orb);
            }

            // 旋转
            float angle = Time.time * 180f; // 每秒转180度
            for (int i = 0; i < orbitalObjects.Count; i++)
            {
                float a = angle + (360f / orbitalObjects.Count) * i;
                float rad = a * Mathf.Deg2Rad;
                orbitalObjects[i].transform.position = transform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * currentWeapon.range;
            }
        }

        private Enemy.EnemyController FindNearest(float maxRange)
        {
            var enemies = Enemy.EnemyController.AllEnemies;
            Enemy.EnemyController nearest = null;
            float minDist = maxRange;
            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDist) { minDist = dist; nearest = enemy; }
            }
            return nearest;
        }

        public void EquipWeapon(WeaponData weapon)
        {
            // 清理旧环绕物
            foreach (var obj in orbitalObjects)
                Destroy(obj);
            orbitalObjects.Clear();

            currentWeapon = weapon;
            attackTimer = 0f;
            OnWeaponChanged?.Invoke(weapon);
            Debug.Log($"装备武器: {weapon.weaponName}");
        }

        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(8, 8);
            Color[] colors = new Color[8 * 8];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }
    }
}
