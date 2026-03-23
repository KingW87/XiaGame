using UnityEngine;
using System.Collections.Generic;
using ClawSurvivor.Weapons;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 宠物数据
    /// </summary>
    [System.Serializable]
    public class PetData
    {
        [Tooltip("宠物ID")]
        public int petId;
        [Tooltip("宠物名称")]
        public string petName;
        [Tooltip("宠物描述")]
        public string description;
        [Tooltip("宠物图标")]
        public Sprite icon;
        [Tooltip("是否已解锁")]
        public bool isUnlocked;
        [Tooltip("当前等级")]
        public int level = 1;
        [Tooltip("最大等级")]
        public int maxLevel = 5;

        /// <summary>
        /// 宠物效果类型
        /// </summary>
        public PetEffectType effectType;

        /// <summary>
        /// 每级属性加成
        /// </summary>
        public float effectValuePerLevel;
    }

    /// <summary>
    /// 宠物效果类型
    /// </summary>
    public enum PetEffectType
    {
        Attack,        // 攻击型：自动攻击最近敌人
        Support,       // 辅助型：给玩家加buff
        Heal,          // 治疗型：自动回复生命
        Shield,        // 防御型：提供护盾
        Magnet         // 磁力型：扩大拾取范围
    }

    /// <summary>
    /// 宠物系统 - 管理宠物解锁、升级、战斗行为
    /// </summary>
    public class PetSystem : MonoBehaviour
    {
        public static PetSystem Instance;

        [Header("宠物设置")]
        [Tooltip("是否启用宠物系统")]
        public bool petSystemEnabled = true;
        [Tooltip("宠物存在上限")]
        public int maxActivePets = 3;
        [Tooltip("宠物自动攻击间隔")]
        public float petAttackInterval = 1.5f;

        private List<PetData> allPets = new List<PetData>();
        private List<GameObject> activePetObjects = new List<GameObject>();
        private float attackTimer;
        private Player.PlayerController player;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePets();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            player = FindObjectOfType<Player.PlayerController>();
            LoadUnlockedPets();
        }

        private void Update()
        {
            if (!petSystemEnabled || player == null || activePetObjects.Count == 0) return;

            attackTimer += Time.deltaTime;
            if (attackTimer >= petAttackInterval)
            {
                attackTimer = 0f;
                PerformPetAttacks();
            }
        }

        private void InitializePets()
        {
            // 创建默认宠物
            allPets.Add(new PetData
            {
                petId = 0,
                petName = "小精灵",
                description = "可爱的小精灵，自动攻击最近敌人",
                effectType = PetEffectType.Attack,
                effectValuePerLevel = 5f, // 每级+5伤害
                isUnlocked = true,
                level = 1,
                maxLevel = 5
            });

            allPets.Add(new PetData
            {
                petId = 1,
                petName = "火焰猫",
                description = "炽热的火焰猫，持续造成范围伤害",
                effectType = PetEffectType.Attack,
                effectValuePerLevel = 8f,
                isUnlocked = false,
                level = 0,
                maxLevel = 5
            });

            allPets.Add(new PetData
            {
                petId = 2,
                petName = "冰霜狼",
                description = "冰霜之狼，攻击附带减速效果",
                effectType = PetEffectType.Support,
                effectValuePerLevel = 0.1f,
                isUnlocked = false,
                level = 0,
                maxLevel = 5
            });

            allPets.Add(new PetData
            {
                petId = 3,
                petName = "雷电鹰",
                description = "雷电之鹰，高速攻击多个目标",
                effectType = PetEffectType.Attack,
                effectValuePerLevel = 12f,
                isUnlocked = false,
                level = 0,
                maxLevel = 5
            });

            allPets.Add(new PetData
            {
                petId = 4,
                petName = "暗影豹",
                description = "神秘暗影豹，周期性隐身并爆发伤害",
                effectType = PetEffectType.Attack,
                effectValuePerLevel = 20f,
                isUnlocked = false,
                level = 0,
                maxLevel = 5
            });

            Debug.Log($"[PetSystem] 已初始化 {allPets.Count} 个宠物");
        }

        private void LoadUnlockedPets()
        {
            if (SaveSystem.Instance == null) return;

            // 从存档加载宠物解锁状态
            foreach (var pet in allPets)
            {
                pet.isUnlocked = SaveSystem.Instance.IsPetUnlocked(pet.petId);
                if (pet.isUnlocked)
                {
                    pet.level = SaveSystem.Instance.GetPetLevel(pet.petId);
                }
            }

            // 激活已解锁的宠物
            SpawnActivePets();
        }

        private void SpawnActivePets()
        {
            // 清理旧宠物
            foreach (var petObj in activePetObjects)
            {
                if (petObj != null) Destroy(petObj);
            }
            activePetObjects.Clear();

            // 生成新宠物
            int count = 0;
            foreach (var pet in allPets)
            {
                if (pet.isUnlocked && count < maxActivePets)
                {
                    SpawnPet(pet);
                    count++;
                }
            }

            Debug.Log($"[PetSystem] 已激活 {activePetObjects.Count} 个宠物");
        }

        private void SpawnPet(PetData petData)
        {
            if (player == null) return;

            GameObject petObj = new GameObject($"Pet_{petData.petName}");
            petObj.transform.SetParent(player.transform);
            petObj.transform.localPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);

            // 添加SpriteRenderer
            SpriteRenderer sr = petObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.color = GetPetColor(petData.effectType);
            sr.sortingOrder = 10;
            petObj.transform.localScale = Vector3.one * 0.4f;

            // 添加宠物行为脚本
            PetBehavior behavior = petObj.AddComponent<PetBehavior>();
            behavior.Initialize(petData, player.transform);

            activePetObjects.Add(petObj);
        }

        private Color GetPetColor(PetEffectType type)
        {
            switch (type)
            {
                case PetEffectType.Attack: return new Color(1f, 0.4f, 0.4f);
                case PetEffectType.Support: return new Color(0.4f, 0.8f, 1f);
                case PetEffectType.Heal: return new Color(0.4f, 1f, 0.5f);
                case PetEffectType.Shield: return new Color(0.5f, 0.5f, 1f);
                case PetEffectType.Magnet: return new Color(1f, 0.6f, 1f);
                default: return Color.white;
            }
        }

        private void PerformPetAttacks()
        {
            foreach (var petObj in activePetObjects)
            {
                var behavior = petObj.GetComponent<PetBehavior>();
                if (behavior != null)
                {
                    behavior.PerformAction();
                }
            }
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

        // === 公开API ===

        /// <summary>
        /// 获取所有宠物数据
        /// </summary>
        public List<PetData> GetAllPets() => allPets;

        /// <summary>
        /// 获取宠物数据
        /// </summary>
        public PetData GetPet(int petId)
        {
            foreach (var pet in allPets)
                if (pet.petId == petId) return pet;
            return null;
        }

        /// <summary>
        /// 宠物是否已解锁
        /// </summary>
        public bool IsPetUnlocked(int petId)
        {
            var pet = GetPet(petId);
            return pet != null && pet.isUnlocked;
        }

        /// <summary>
        /// 刷新宠物（重新生成）
        /// </summary>
        public void RefreshPets()
        {
            LoadUnlockedPets();
        }
    }

    /// <summary>
    /// 宠物行为 - 挂载在每个宠物对象上
    /// </summary>
    public class PetBehavior : MonoBehaviour
    {
        private PetData petData;
        private Transform playerTransform;
        private float orbitAngle;
        private float orbitSpeed = 60f;
        private float orbitRadius = 1.5f;
        private bool isAttacking;

        public void Initialize(PetData data, Transform player)
        {
            petData = data;
            playerTransform = player;
            orbitRadius = 1.2f + petData.level * 0.3f;
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // 环绕玩家移动
            orbitAngle += orbitSpeed * Time.deltaTime;
            float rad = orbitAngle * Mathf.Deg2Rad;
            Vector3 targetPos = playerTransform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * orbitRadius;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
        }

        public void PerformAction()
        {
            if (petData == null || Enemy.EnemyController.AllEnemies.Count == 0) return;

            float damage = petData.effectValuePerLevel * petData.level;
            float attackRange = 8f;

            switch (petData.effectType)
            {
                case PetEffectType.Attack:
                    // 攻击最近敌人
                    var nearest = FindNearestEnemy(attackRange);
                    if (nearest != null)
                    {
                        nearest.TakeDamage(Mathf.RoundToInt(damage));
                        CreateAttackEffect(nearest.transform.position);
                    }
                    break;

                case PetEffectType.Support:
                    // 辅助效果（减速敌人）
                    var nearbyEnemies = Physics2D.OverlapCircleAll(playerTransform.position, attackRange);
                    foreach (var col in nearbyEnemies)
                    {
                        // 暂时预留减速效果
                    }
                    break;

                case PetEffectType.Heal:
                    // 回复生命
                    var player = playerTransform.GetComponent<Player.PlayerController>();
                    if (player != null)
                    {
                        player.Heal(Mathf.RoundToInt(damage));
                    }
                    break;

                case PetEffectType.Shield:
                    // 给予护盾
                    var playerWithShield = playerTransform.GetComponent<Player.PlayerController>();
                    if (playerWithShield != null)
                    {
                        playerWithShield.AddShield(Mathf.RoundToInt(damage));
                    }
                    break;

                case PetEffectType.Magnet:
                    // 扩大拾取范围（通过经验宝石的磁力实现）
                    var expPickups = FindObjectsOfType<ExperiencePickup>();
                    foreach (var pickup in expPickups)
                    {
                        pickup.DoubleMagnetRange();
                    }
                    break;
            }
        }

        private Enemy.EnemyController FindNearestEnemy(float maxRange)
        {
            Enemy.EnemyController nearest = null;
            float minDist = maxRange;

            foreach (var enemy in Enemy.EnemyController.AllEnemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        private void CreateAttackEffect(Vector3 position)
        {
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * 0.3f;
            Destroy(effect, 0.2f);
        }
    }
}
