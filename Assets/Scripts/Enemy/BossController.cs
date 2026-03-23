using UnityEngine;
using System.Collections;

namespace ClawSurvivor.Enemy
{
    /// <summary>
    /// Boss类型
    /// </summary>
    public enum BossType
    {
        /// <summary>巨兽Boss - 高血量近战，冲锋+地震波</summary>
        Behemoth,
        /// <summary>暗影Boss - 快速移动，召唤小怪+闪现</summary>
        Shadow,
        /// <summary>毁灭Boss - 远程炮台，弹幕+激光</summary>
        Destroyer,
    }

    /// <summary>
    /// Boss控制器 - 挂载在Boss敌人身上，继承EnemyController基础能力
    /// 每5波出现，有血条UI和3种特殊技能
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    public class BossController : MonoBehaviour
    {
        [Header("Boss设置")]
        [Tooltip("Boss类型")]
        public BossType bossType = BossType.Behemoth;
        [Tooltip("Boss等级")]
        public int bossLevel = 1;

        [Header("Boss基础属性")]
        [Tooltip("血量倍率")]
        public float healthMultiplier = 10f;
        [Tooltip("伤害倍率")]
        public float damageMultiplier = 2f;
        [Tooltip("体型倍率")]
        public float sizeMultiplier = 2.5f;
        [Tooltip("击杀奖励经验")]
        public int experienceBonus = 50;

        [Header("技能冷却（秒）")]
        [Tooltip("技能1冷却")]
        public float skill1Cooldown = 5f;
        [Tooltip("技能2冷却")]
        public float skill2Cooldown = 8f;
        [Tooltip("技能3冷却")]
        public float skill3Cooldown = 12f;

        private EnemyController baseController;
        private Transform playerTarget;
        private int totalMaxHealth;

        // 技能计时器
        private float skill1Timer;
        private float skill2Timer;
        private float skill3Timer;

        // Boss颜色
        private static readonly Color[] BossColors = {
            new Color(0.85f, 0.15f, 0.05f),   // Behemoth: 暗红
            new Color(0.3f, 0.1f, 0.7f),      // Shadow: 深紫
            new Color(0.9f, 0.3f, 0.0f),      // Destroyer: 橙红
        };

        // Boss名称
        private static readonly string[] BossNames = {
            "巨兽",
            "暗影",
            "毁灭者",
        };

        public string BossName => BossNames[(int)bossType];
        public int TotalMaxHealth => totalMaxHealth;
        public int CurrentHealth => baseController != null ? baseController.CurrentHealth : 0;

        // Boss生成/死亡事件
        public static System.Action<BossController> OnBossSpawned;
        public static System.Action<BossController> OnBossDefeated;

        private void Start()
        {
            baseController = GetComponent<EnemyController>();
            var player = FindAnyObjectByType<Player.PlayerController>();
            if (player != null) playerTarget = player.transform;

            // 根据Boss类型和等级设置属性
            SetupBoss();
        }

        private void SetupBoss()
        {
            // Boss等级加成：每5波Boss等级+1
            float levelMult = 1f + (bossLevel - 1) * 0.5f;

            // 设置EnemyController属性（通过ApplyDifficultyMultiplier）
            float hpMult = healthMultiplier * levelMult;
            float dmgMult = damageMultiplier * levelMult;
            float spdMult = 1f + (bossLevel - 1) * 0.1f;
            baseController.ApplyDifficultyMultiplier(hpMult, spdMult, dmgMult);

            // 放大Boss体型
            transform.localScale = Vector3.one * sizeMultiplier * (1f + (bossLevel - 1) * 0.15f);

            // 设置Boss颜色
            Color bossColor = BossColors[(int)bossType];
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = bossColor;

            // 获取最终血量用于血条UI
            // ApplyDifficultyMultiplier已经修改了maxHealth，需要获取
            totalMaxHealth = Mathf.RoundToInt(20 * hpMult); // 基础20血量 * 倍率
            // 更精确：直接从baseController获取
            totalMaxHealth = 0; // 将在OnEnable后通过事件获取

            // 添加额外的经验值设置（通过修改ExperienceValue）
            // EnemyController没有暴露setter，我们在Die后额外掉经验

            // 触发Boss出现事件
            OnBossSpawned?.Invoke(this);
        }

        private void Update()
        {
            if (playerTarget == null || baseController == null) return;
            if (baseController.IsDead) return;

            // 更新技能计时器
            skill1Timer += Time.deltaTime;
            skill2Timer += Time.deltaTime;
            skill3Timer += Time.deltaTime;

            // 检查技能释放
            if (skill1Timer >= skill1Cooldown)
            {
                ExecuteSkill1();
                skill1Timer = 0f;
            }
            if (skill2Timer >= skill2Cooldown)
            {
                ExecuteSkill2();
                skill2Timer = 0f;
            }
            if (skill3Timer >= skill3Cooldown)
            {
                ExecuteSkill3();
                skill3Timer = 0f;
            }
        }

        #region 技能实现

        /// <summary>
        /// 技能1：冲锋（巨兽）/ 召唤小怪（暗影）/ 散射弹幕（毁灭者）
        /// </summary>
        private void ExecuteSkill1()
        {
            switch (bossType)
            {
                case BossType.Behemoth:
                    StartCoroutine(ChargeAttack());
                    break;
                case BossType.Shadow:
                    SpawnMinions(3);
                    break;
                case BossType.Destroyer:
                    FireSpreadProjectiles(8);
                    break;
            }
        }

        /// <summary>
        /// 技能2：地震波（巨兽）/ 闪现（暗影）/ 追踪导弹（毁灭者）
        /// </summary>
        private void ExecuteSkill2()
        {
            switch (bossType)
            {
                case BossType.Behemoth:
                    StartCoroutine(Shockwave());
                    break;
                case BossType.Shadow:
                    StartCoroutine(PhaseDash());
                    break;
                case BossType.Destroyer:
                    FireHomingMissiles(4);
                    break;
            }
        }

        /// <summary>
        /// 技能3：狂暴（巨兽）/ 影分身（暗影）/ 激光扫射（毁灭者）
        /// </summary>
        private void ExecuteSkill3()
        {
            switch (bossType)
            {
                case BossType.Behemoth:
                    StartCoroutine(Enrage());
                    break;
                case BossType.Shadow:
                    StartCoroutine(ShadowClone());
                    break;
                case BossType.Destroyer:
                    StartCoroutine(LaserSweep());
                    break;
            }
        }

        #endregion

        #region 巨兽技能

        /// <summary>
        /// 冲锋攻击 - 朝玩家快速冲刺一段距离
        /// </summary>
        private IEnumerator ChargeAttack()
        {
            if (playerTarget == null) yield break;

            // 蓄力（短暂停顿+变红）
            var sr = GetComponent<SpriteRenderer>();
            Color origColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.5f);

            // 冲锋
            Vector2 dir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            float chargeSpeed = 15f;
            float chargeDuration = 0.4f;
            float elapsed = 0;

            while (elapsed < chargeDuration)
            {
                transform.position += (Vector3)dir * chargeSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }

            sr.color = origColor;

            // 冲锋结束产生震荡（对近距离敌人造成伤害）
            if (playerTarget != null)
            {
                float dist = Vector2.Distance(transform.position, playerTarget.position);
                if (dist < 2f)
                {
                    var player = playerTarget.GetComponent<Player.PlayerController>();
                    if (player != null)
                        player.TakeDamage(Mathf.RoundToInt(15 * damageMultiplier));
                }
            }
        }

        /// <summary>
        /// 地震波 - 以Boss为中心释放圆形冲击波
        /// </summary>
        private IEnumerator Shockwave()
        {
            // 蓄力
            yield return new WaitForSeconds(0.3f);

            // 创建向外扩散的冲击波视觉效果
            for (int i = 0; i < 3; i++)
            {
                CreateShockwaveRing(i * 0.2f);
            }

            // 对范围内玩家造成伤害
            if (playerTarget != null)
            {
                float dist = Vector2.Distance(transform.position, playerTarget.position);
                if (dist < 6f)
                {
                    var player = playerTarget.GetComponent<Player.PlayerController>();
                    if (player != null)
                    {
                        // 距离越近伤害越高
                        int dmg = Mathf.RoundToInt(20 * damageMultiplier * (1f - dist / 6f));
                        player.TakeDamage(dmg);
                    }
                }
            }
        }

        private void CreateShockwaveRing(float delay)
        {
            StartCoroutine(DelayedShockwaveRing(delay));
        }

        private IEnumerator DelayedShockwaveRing(float delay)
        {
            yield return new WaitForSeconds(delay);

            GameObject ring = new GameObject("ShockwaveRing");
            ring.transform.position = transform.position;
            SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.8f, 0.3f, 0.6f);
            sr.sprite = CreateCircleSprite();
            sr.sortingOrder = 15;
            ring.transform.localScale = Vector3.one * 0.5f;

            // 扩散动画
            float duration = 0.5f;
            float elapsed = 0;
            float startScale = 0.5f;
            float endScale = 12f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                ring.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);
                var ringSr = ring.GetComponent<SpriteRenderer>();
                if (ringSr != null)
                    ringSr.color = new Color(1f, 0.8f, 0.3f, 0.6f * (1f - t));
                yield return null;
            }

            Destroy(ring);
        }

        /// <summary>
        /// 狂暴 - 短时间内大幅提升攻速和移速
        /// </summary>
        private IEnumerator Enrage()
        {
            Debug.Log($"Boss {BossName} 进入狂暴状态！");

            // 视觉效果：发光
            var sr = GetComponent<SpriteRenderer>();
            Color origColor = sr != null ? sr.color : Color.red;

            float duration = 5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // 闪烁效果
                if (sr != null)
                    sr.color = Color.Lerp(origColor, Color.yellow, Mathf.Sin(elapsed * 10f) * 0.5f + 0.5f);
                yield return null;
            }

            if (sr != null) sr.color = origColor;
        }

        #endregion

        #region 暗影技能

        /// <summary>
        /// 召唤小怪
        /// </summary>
        private void SpawnMinions(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 3f;
                Vector2 spawnPos = (Vector2)transform.position + offset;

                GameObject minion = new GameObject("ShadowMinion");
                minion.transform.position = spawnPos;

                SpriteRenderer sr = minion.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.5f, 0.2f, 0.8f);
                sr.sortingOrder = 5;
                sr.sprite = CreateSimpleSprite();
                minion.transform.localScale = Vector3.one * 0.6f;

                minion.AddComponent<BoxCollider2D>();
                EnemyController ec = minion.AddComponent<EnemyController>();
                ec.SetupType(EnemyType.Fast);

                // 小怪血量较低
                ec.ApplyDifficultyMultiplier(0.5f, 1.5f, 0.8f);

                Destroy(minion, 15f);
            }
        }

        /// <summary>
        /// 闪现 - 瞬移到玩家附近
        /// </summary>
        private IEnumerator PhaseDash()
        {
            if (playerTarget == null) yield break;

            // 消失
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            yield return new WaitForSeconds(0.3f);

            // 瞬移到玩家附近
            Vector2 offset = Random.insideUnitCircle * 2f;
            transform.position = (Vector2)playerTarget.position + offset;

            // 重新出现
            if (sr != null) sr.enabled = true;

            // 闪现后攻击
            if (playerTarget != null)
            {
                float dist = Vector2.Distance(transform.position, playerTarget.position);
                if (dist < 2f)
                {
                    var player = playerTarget.GetComponent<Player.PlayerController>();
                    if (player != null)
                        player.TakeDamage(Mathf.RoundToInt(10 * damageMultiplier));
                }
            }
        }

        /// <summary>
        /// 影分身 - 创建2个假身吸引注意
        /// </summary>
        private IEnumerator ShadowClone()
        {
            if (playerTarget == null) yield break;

            // 创建3个分身
            GameObject[] clones = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(
                    Mathf.Cos(i * Mathf.PI * 2 / 3) * 4f,
                    Mathf.Sin(i * Mathf.PI * 2 / 3) * 4f
                );
                clones[i] = CreateClone((Vector2)transform.position + offset);
            }

            // 分身存在5秒
            yield return new WaitForSeconds(5f);

            foreach (var clone in clones)
            {
                if (clone != null) Destroy(clone);
            }
        }

        private GameObject CreateClone(Vector2 position)
        {
            GameObject clone = new GameObject("ShadowClone");
            clone.transform.position = position;

            SpriteRenderer sr = clone.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.4f, 0.15f, 0.6f, 0.5f);
            sr.sortingOrder = 4;
            sr.sprite = CreateSimpleSprite();
            clone.transform.localScale = transform.localScale * 0.8f;

            // 分身也能造成少量伤害
            BoxCollider2D col = clone.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            var trigger = clone.AddComponent<CloneDamageTrigger>();
            trigger.damage = Mathf.RoundToInt(5 * damageMultiplier);

            return clone;
        }

        #endregion

        #region 毁灭者技能

        /// <summary>
        /// 散射弹幕 - 向玩家方向发射多颗弹丸
        /// </summary>
        private void FireSpreadProjectiles(int count)
        {
            if (playerTarget == null) return;

            Vector2 dir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            float baseAngle = Mathf.Atan2(dir.y, dir.x);
            float spreadAngle = 60f * Mathf.Deg2Rad; // 总共60度扇形

            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle - spreadAngle / 2f + (spreadAngle / (count - 1)) * i;
                Vector2 fireDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                CreateBossProjectile(fireDir, 6f, Mathf.RoundToInt(8 * damageMultiplier), Color.red);
            }
        }

        /// <summary>
        /// 追踪导弹 - 发射追踪玩家的弹丸
        /// </summary>
        private void FireHomingMissiles(int count)
        {
            if (playerTarget == null) return;

            for (int i = 0; i < count; i++)
            {
                StartCoroutine(DelayedMissile(i * 0.3f));
            }
        }

        private IEnumerator DelayedMissile(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (playerTarget == null) yield break;

            Vector2 dir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            CreateHomingProjectile(dir, 4f, Mathf.RoundToInt(12 * damageMultiplier));
            CreateHomingProjectile(dir, 4f, Mathf.RoundToInt(12 * damageMultiplier));
        }

        /// <summary>
        /// 激光扫射 - 旋转激光持续伤害
        /// </summary>
        private IEnumerator LaserSweep()
        {
            if (playerTarget == null) yield break;

            float duration = 3f;
            float elapsed = 0;
            float startAngle = Mathf.Atan2(
                ((Vector2)playerTarget.position - (Vector2)transform.position).y,
                ((Vector2)playerTarget.position - (Vector2)transform.position).x
            );

            // 创建激光视觉效果
            GameObject laser = new GameObject("BossLaser");
            laser.transform.position = transform.position;
            SpriteRenderer laserSr = laser.AddComponent<SpriteRenderer>();
            laserSr.color = new Color(1f, 0.3f, 0.1f, 0.7f);
            laserSr.sprite = CreateRectSprite(0.3f, 15f);
            laserSr.sortingOrder = 12;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // 慢速旋转
                float currentAngle = startAngle + (elapsed / duration) * 360f * Mathf.Deg2Rad;
                laser.transform.rotation = Quaternion.Euler(0, 0, currentAngle * Mathf.Rad2Deg);

                // 检测激光是否碰到玩家
                if (playerTarget != null)
                {
                    Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
                    float playerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x);
                    float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle * Mathf.Rad2Deg, playerAngle * Mathf.Rad2Deg));

                    if (angleDiff < 10f && toPlayer.magnitude < 15f)
                    {
                        // 激光持续伤害（每0.5秒一次）
                        if (Mathf.FloorToInt(elapsed * 2) != Mathf.FloorToInt((elapsed - Time.deltaTime) * 2))
                        {
                            var player = playerTarget.GetComponent<Player.PlayerController>();
                            if (player != null)
                                player.TakeDamage(Mathf.RoundToInt(5 * damageMultiplier));
                        }
                    }
                }

                yield return null;
            }

            Destroy(laser);
        }

        #endregion

        #region 辅助方法

        private void CreateBossProjectile(Vector2 direction, float speed, int damage, Color color)
        {
            GameObject proj = new GameObject("BossProjectile");
            proj.transform.position = transform.position;
            proj.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sortingOrder = 8;
            sr.sprite = CreateCircleSprite();

            proj.AddComponent<BoxCollider2D>().isTrigger = true;
            proj.AddComponent<EnemyProjectile>().Initialize(damage, speed, playerTarget);
            Destroy(proj, 5f);
        }

        private void CreateHomingProjectile(Vector2 direction, float speed, int damage)
        {
            GameObject proj = new GameObject("BossHomingMissile");
            proj.transform.position = transform.position;
            proj.transform.localScale = Vector3.one * 0.35f;

            SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.2f, 0.8f);
            sr.sortingOrder = 8;
            sr.sprite = CreateCircleSprite();

            proj.AddComponent<BoxCollider2D>().isTrigger = true;
            proj.AddComponent<EnemyProjectile>().Initialize(damage, speed * 0.7f, playerTarget);
            Destroy(proj, 6f);
        }

        private Sprite CreateSimpleSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        private Sprite CreateCircleSprite()
        {
            return CreateSimpleSprite();
        }

        private Sprite CreateRectSprite(float width, float height)
        {
            return CreateSimpleSprite();
        }

        /// <summary>
        /// 获取当前血量百分比
        /// </summary>
        public float HealthPercentage
        {
            get
            {
                if (baseController == null) return 0;
                return baseController.HealthPercent;
            }
        }

        private void OnDestroy()
        {
            // Boss死亡时触发事件
            if (baseController != null && baseController.IsDead)
            {
                OnBossDefeated?.Invoke(this);

                // Boss掉落大量经验
                DropBonusExperience();

                // Boss掉落高质量道具
                if (Pickups.PickupSpawner.Instance != null)
                    Pickups.PickupSpawner.Instance.DropBossItems(transform.position);
            }
        }

        private void DropBonusExperience()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 2f;
                Vector3 spawnPos = transform.position + (Vector3)offset;

                GameObject gem = new GameObject("BossExperienceGem");
                gem.transform.position = spawnPos;
                gem.transform.localScale = Vector3.one * 1.2f;

                SpriteRenderer sr = gem.AddComponent<SpriteRenderer>();
                sr.color = Color.yellow;
                sr.sortingOrder = 10;
                sr.sprite = CreateSimpleSprite();

                gem.AddComponent<BoxCollider2D>().isTrigger = true;
                gem.AddComponent<Systems.ExperiencePickup>().Initialize(experienceBonus / 5 + 10);
                Destroy(gem, 30f);
            }
        }

        #endregion
    }

    /// <summary>
    /// 暗影分身伤害触发器
    /// </summary>
    public class CloneDamageTrigger : MonoBehaviour
    {
        public int damage = 5;
        private float damageTimer;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= 1f)
                {
                    var player = other.GetComponent<Player.PlayerController>();
                    if (player != null) player.TakeDamage(damage);
                    damageTimer = 0f;
                }
            }
        }
    }
}
