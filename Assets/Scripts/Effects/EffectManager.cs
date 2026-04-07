using UnityEngine;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.Effects
{
    /// <summary>
    /// 特效管理器 - 统一管理所有游戏特效
    /// 单例模式，挂载到独立GameObject
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance;

        [Header("特效配置")]
        [Tooltip("特效列表（在Inspector中配置）")]
        public List<EffectData> effectConfigs = new List<EffectData>();

        [Header("默认特效设置")]
        [Tooltip("默认粒子特效颜色")]
        public Color defaultParticleColor = Color.white;
        [Tooltip("默认爆炸颜色")]
        public Color explosionColor = new Color(1f, 0.5f, 0f);

        [Header("特效池设置")]
        [Tooltip("特效池大小")]
        public int poolSize = 20;

        private Dictionary<EffectType, GameObject> effectPrefabs;
        private List<GameObject> effectPool;
        private int currentPoolIndex;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeEffectMap();
            InitializeEffectPool();
        }

        private void InitializeEffectMap()
        {
            effectPrefabs = new Dictionary<EffectType, GameObject>();
            foreach (var config in effectConfigs)
            {
                if (!effectPrefabs.ContainsKey(config.effectType))
                {
                    effectPrefabs[config.effectType] = config.effectPrefab;
                }
            }
        }

        private void InitializeEffectPool()
        {
            effectPool = new List<GameObject>();
            // 预创建一些默认特效
            for (int i = 0; i < poolSize; i++)
            {
                CreateDefaultEffect();
            }
        }

        private GameObject CreateDefaultEffect()
        {
            // 创建一个默认的粒子特效
            GameObject effect = new GameObject("DefaultEffect");
            effect.transform.SetParent(transform);
            effect.SetActive(false);

            // 添加粒子系统
            ParticleSystem ps = effect.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.startColor = defaultParticleColor;
            main.loop = false;

            var emission = ps.emission;
            emission.rateOverTime = 20;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;

            var renderer = effect.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            effectPool.Add(effect);
            return effect;
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        public void PlayEffect(EffectType type, Vector3 position, float scale = 1f)
        {
            GameObject effect = GetEffectObject(type);
            if (effect == null)
            {
                Debug.LogWarning($"[EffectManager] 未找到特效: {type}");
                return;
            }

            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * scale;
            effect.SetActive(true);

            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                ps.Play();
            }

            // 自动回收
            var config = GetEffectConfig(type);
            float duration = config != null ? config.duration : 1f;
            StartCoroutine(RecycleEffect(effect, duration));
        }

        /// <summary>
        /// 播放特效（带父对象）
        /// </summary>
        public void PlayEffect(EffectType type, Transform parent, Vector3 localPosition, float scale = 1f)
        {
            GameObject effect = GetEffectObject(type);
            if (effect == null)
            {
                Debug.LogWarning($"[EffectManager] 未找到特效: {type}");
                return;
            }

            effect.transform.SetParent(parent);
            effect.transform.localPosition = localPosition;
            effect.transform.localScale = Vector3.one * scale;
            effect.SetActive(true);

            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                ps.Play();
            }

            var config = GetEffectConfig(type);
            float duration = config != null ? config.duration : 1f;
            StartCoroutine(RecycleEffect(effect, duration));
        }

        private System.Collections.IEnumerator RecycleEffect(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            effect.SetActive(false);
            effect.transform.SetParent(transform);
        }

        private GameObject GetEffectObject(EffectType type)
        {
            // 优先使用配置的预制体
            if (effectPrefabs.TryGetValue(type, out GameObject prefab) && prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                return instance;
            }

            // 使用池中的对象
            foreach (var effect in effectPool)
            {
                if (!effect.activeInHierarchy)
                {
                    // 根据类型调整颜色
                    AdjustEffectColor(effect, type);
                    return effect;
                }
            }

            // 池满了，创建新的
            return CreateDefaultEffect();
        }

        private void AdjustEffectColor(GameObject effect, EffectType type)
        {
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps == null) return;

            var main = ps.main;
            switch (type)
            {
                case EffectType.PlayerAttack:
                case EffectType.WeaponSlash:
                    main.startColor = Color.cyan;
                    break;
                case EffectType.EnemyDeath:
                case EffectType.Explosion:
                    main.startColor = explosionColor;
                    break;
                case EffectType.PlayerLevelUp:
                case EffectType.LevelUpParticle:
                    main.startColor = new Color(1f, 0.85f, 0.2f); // 金色
                    break;
                case EffectType.PickupHealth:
                    main.startColor = Color.green;
                    break;
                case EffectType.PickupShield:
                    main.startColor = Color.blue;
                    break;
                case EffectType.PickupExp:
                    main.startColor = Color.yellow;
                    break;
                default:
                    main.startColor = defaultParticleColor;
                    break;
            }
        }

        private EffectData GetEffectConfig(EffectType type)
        {
            foreach (var config in effectConfigs)
            {
                if (config.effectType == type)
                    return config;
            }
            return null;
        }

        /// <summary>
        /// 播放玩家攻击特效
        /// </summary>
        public static void PlayPlayerAttack(Vector3 position)
        {
            if (Instance != null)
                Instance.PlayEffect(EffectType.PlayerAttack, position);
        }

        /// <summary>
        /// 播放玩家受伤特效
        /// </summary>
        public static void PlayPlayerHit(Vector3 position)
        {
            if (Instance != null)
                Instance.PlayEffect(EffectType.PlayerHit, position);
        }

        /// <summary>
        /// 播放敌人死亡特效
        /// </summary>
        public static void PlayEnemyDeath(Vector3 position)
        {
            if (Instance != null)
                Instance.PlayEffect(EffectType.EnemyDeath, position);
        }

        /// <summary>
        /// 播放升级特效
        /// </summary>
        public static void PlayLevelUp(Vector3 position)
        {
            if (Instance != null)
                Instance.PlayEffect(EffectType.PlayerLevelUp, position);
        }

        /// <summary>
        /// 播放爆炸特效
        /// </summary>
        public static void PlayExplosion(Vector3 position)
        {
            if (Instance != null)
                Instance.PlayEffect(EffectType.Explosion, position);
        }
    }
}
