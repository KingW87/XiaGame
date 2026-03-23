using UnityEngine;
using System.Collections.Generic;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 音效类型枚举
    /// </summary>
    public enum SFXType
    {
        PlayerAttack,      // 玩家攻击
        PlayerHit,         // 玩家受伤
        PlayerDeath,       // 玩家死亡
        LevelUp,           // 升级
        EnemyHit,          // 敌人受伤
        EnemyDeath,        // 敌人死亡
        BossAppear,        // Boss出现
        BossDeath,         // Boss死亡
        PickupItem,        // 拾取道具
        PickupExp,         // 拾取经验
        ButtonClick,       // 按钮点击
        BombExplosion,     // 炸弹爆炸
        ShieldHit,         // 护盾格挡
        Heal,              // 回复生命
        WaveStart,         // 新波次开始
        SkillActivate      // 技能激活
    }

    /// <summary>
    /// 音效数据配置
    /// </summary>
    [System.Serializable]
    public class SoundData
    {
        [Tooltip("音效类型")]
        public SFXType soundType;
        [Tooltip("音频片段（不填则用Debug.Log标记）")]
        public AudioClip audioClip;
        [Tooltip("音量（0~1）")]
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("音调（0.5~2）")]
        [Range(0.5f, 2f)] public float pitch = 1f;
    }

    /// <summary>
    /// 音效管理器 - 统一管理BGM和SFX播放
    /// 单例模式，挂载到独立GameObject
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [Header("BGM设置")]
        [Tooltip("背景音乐音频片段")]
        public AudioClip bgmClip;
        [Tooltip("BGM音量")]
        [Range(0f, 1f)] public float bgmVolume = 0.5f;
        [Tooltip("BGM是否循环")]
        public bool bgmLoop = true;

        [Header("SFX设置")]
        [Tooltip("音效列表（在Inspector中配置所有音效）")]
        public List<SoundData> soundDataList = new List<SoundData>();
        [Tooltip("SFX音量倍率（全局调节）")]
        [Range(0f, 1f)] public float sfxGlobalVolume = 1f;
        [Tooltip("SFX池大小（同时可播放的音效数量）")]
        [Range(2, 10)] public int sfxPoolSize = 5;

        [Header("运行时设置")]
        [Tooltip("是否启用音效")]
        public bool enableSFX = true;
        [Tooltip("是否启用BGM")]
        public bool enableBGM = true;

        private AudioSource bgmSource;
        private List<AudioSource> sfxPool;
        private Dictionary<SFXType, SoundData> soundMap;
        private int currentSFXIndex;

        private void Awake()
        {
            Instance = this;

            // 创建BGM AudioSource
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = bgmLoop;
            bgmSource.volume = bgmVolume;
            bgmSource.playOnAwake = false;

            // 创建SFX池
            sfxPool = new List<AudioSource>();
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource sfx = gameObject.AddComponent<AudioSource>();
                sfx.playOnAwake = false;
                sfxPool.Add(sfx);
            }

            // 构建音效映射字典
            BuildSoundMap();
        }

        private void Start()
        {
            // 自动播放BGM
            if (enableBGM && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }
        }

        private void BuildSoundMap()
        {
            soundMap = new Dictionary<SFXType, SoundData>();
            foreach (var data in soundDataList)
            {
                if (!soundMap.ContainsKey(data.soundType))
                    soundMap[data.soundType] = data;
            }
        }

        /// <summary>
        /// 播放音效 - 通过枚举类型
        /// </summary>
        public void PlaySFX(SFXType type)
        {
            if (!enableSFX) return;

            if (soundMap.TryGetValue(type, out SoundData data))
            {
                if (data.audioClip != null)
                {
                    PlaySFXClip(data.audioClip, data.volume * sfxGlobalVolume, data.pitch);
                }
                else
                {
                    Debug.Log($"[SFX] {type}");
                }
            }
            else
            {
                Debug.Log($"[SFX] {type} (未配置)");
            }
        }

        /// <summary>
        /// 播放指定音效，支持音量和音调
        /// </summary>
        public void PlaySFX(SFXType type, float volumeScale, float pitchScale)
        {
            if (!enableSFX) return;

            if (soundMap.TryGetValue(type, out SoundData data))
            {
                if (data.audioClip != null)
                {
                    PlaySFXClip(data.audioClip, data.volume * sfxGlobalVolume * volumeScale, data.pitch * pitchScale);
                }
                else
                {
                    Debug.Log($"[SFX] {type} (v:{volumeScale} p:{pitchScale})");
                }
            }
        }

        private void PlaySFXClip(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || sfxPool.Count == 0) return;

            // 从池中找一个空闲的AudioSource
            AudioSource source = sfxPool[currentSFXIndex];
            currentSFXIndex = (currentSFXIndex + 1) % sfxPool.Count;

            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            source.Play();
        }

        /// <summary>
        /// 播放BGM
        /// </summary>
        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null) return;
            bgmClip = clip;
            bgmSource.clip = clip;
            if (enableBGM)
                bgmSource.Play();
        }

        /// <summary>
        /// 停止BGM
        /// </summary>
        public void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource != null)
                bgmSource.volume = bgmVolume;
        }

        /// <summary>
        /// 设置SFX全局音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxGlobalVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 静音所有音效
        /// </summary>
        public void MuteAll()
        {
            enableSFX = false;
            enableBGM = false;
            if (bgmSource != null) bgmSource.Pause();
        }

        /// <summary>
        /// 取消静音
        /// </summary>
        public void UnmuteAll()
        {
            enableSFX = true;
            enableBGM = true;
            if (bgmSource != null && bgmSource.clip != null) bgmSource.UnPause();
        }
    }
}
