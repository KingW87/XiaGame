using UnityEngine;

namespace ClawSurvivor.Skills
{
    public enum SkillType
    {
        Damage,
        Control,
        Support
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "ClawSurvivor/Skill Card")]
    public class SkillCard : ScriptableObject
    {
        [Header("基本信息")]
        public string skillName;
        public string description;
        public Sprite icon;
        public SkillType type;

        [Header("数值")]
        public float cooldown = 10f;
        public int damage;
        public float range;
        public float duration;

        [Header("效果")]
        public GameObject effectPrefab;

        public virtual void Activate(Player.PlayerController player)
        {
            Debug.Log($"使用技能: {skillName}");

            switch (skillName)
            {
                case "冰霜新星":
                    ActivateIceNova(player);
                    break;
                case "治疗波":
                    ActivateHealingWave(player);
                    break;
                case "雷电打击":
                    ActivateThunderStrike(player);
                    break;
                case "火焰旋风":
                    ActivateFireTornado(player);
                    break;
                case "护盾":
                    ActivateShield(player);
                    break;
                case "生命汲取":
                    ActivateLifeDrain(player);
                    break;
            }
        }

        private void ActivateIceNova(Player.PlayerController player)
        {
            CreateEffect(player, 2f);
            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, range);
            foreach (var col in enemies)
            {
                var enemy = col.GetComponent<Enemy.EnemyController>();
                if (enemy != null)
                    enemy.TakeDamage(damage);
            }
        }

        private void ActivateHealingWave(Player.PlayerController player)
        {
            CreateEffect(player, 2f);
            player.Heal(damage);
        }

        private void ActivateThunderStrike(Player.PlayerController player)
        {
            var enemies = Enemy.EnemyController.AllEnemies;
            Enemy.EnemyController nearest = null;
            float minDist = range;

            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (dist < minDist) { minDist = dist; nearest = enemy; }
            }

            if (nearest != null)
                nearest.TakeDamage(damage);
        }

        private void ActivateFireTornado(Player.PlayerController player)
        {
            // 持续范围伤害
            player.StartCoroutine(FireTornadoCoroutine(player));
        }

        private System.Collections.IEnumerator FireTornadoCoroutine(Player.PlayerController player)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, range);
                foreach (var col in enemies)
                {
                    var enemy = col.GetComponent<Enemy.EnemyController>();
                    if (enemy != null)
                        enemy.TakeDamage(damage);
                }
                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void ActivateShield(Player.PlayerController player)
        {
            player.AddShield(30);
        }

        private void ActivateLifeDrain(Player.PlayerController player)
        {
            // 对最近敌人造成伤害并回血
            var enemies = Enemy.EnemyController.AllEnemies;
            Enemy.EnemyController nearest = null;
            float minDist = range;

            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (dist < minDist) { minDist = dist; nearest = enemy; }
            }

            if (nearest != null)
            {
                nearest.TakeDamage(damage);
                player.Heal(damage);
            }
        }

        private void CreateEffect(Player.PlayerController player, float lifetime)
        {
            if (effectPrefab == null) return;
            GameObject effect = Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
            Destroy(effect, lifetime);
        }
    }
}
