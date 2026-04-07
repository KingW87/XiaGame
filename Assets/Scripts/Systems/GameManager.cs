using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 游戏主管理器 - 控制游戏流程
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        [Header("游戏状态")]
        [Tooltip("是否暂停")]
        public bool isPaused;
        [Tooltip("游戏已进行时间（秒）")]
        public float gameTime;
        [Tooltip("击杀敌人总数")]
        public int enemiesKilled;

        [Header("地图引用")]
        [Tooltip("地图生成器引用")]
        public Map.MapGenerator mapGenerator;
        
        // 游戏状态事件
        public System.Action<float> OnGameTimeChanged;
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;
        public System.Action OnGameOver;
        
        private void Awake()
        {
            Instance = this;
            // 自动查找MapGenerator
            if (mapGenerator == null)
                mapGenerator = FindObjectOfType<Map.MapGenerator>();
        }
        
        private void Update()
        {
            if (!isPaused)
            {
                gameTime += Time.deltaTime;
                OnGameTimeChanged?.Invoke(gameTime);
            }
            
            // 暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        
        public void TogglePause()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                Time.timeScale = 0;
                OnGamePaused?.Invoke();
            }
            else
            {
                Time.timeScale = 1;
                OnGameResumed?.Invoke();
            }
        }
        
        public void AddKill()
        {
            enemiesKilled++;
        }
        
        public void GameOver()
        {
            Time.timeScale = 0;
            OnGameOver?.Invoke();
            Debug.Log($"游戏结束！存活时间: {gameTime:F1}秒, 击杀: {enemiesKilled}");
        }
        
        public void RestartGame()
        {
            // 必须在恢复时间尺度后才能正确加载场景
            Time.timeScale = 1;
            
            // 异步加载当前场景
            StartCoroutine(RestartGameCoroutine());
        }

        private System.Collections.IEnumerator RestartGameCoroutine()
        {
            // 等待一帧确保时间尺度生效
            yield return null;
            
            // 清理地图块
            if (mapGenerator != null)
                mapGenerator.ClearAllChunks();
            
            // 加载场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public void ExitToMenu()
        {
            Time.timeScale = 1;
            if (mapGenerator != null)
                mapGenerator.ClearAllChunks();
            SceneManager.LoadScene(0);
        }
        
        public bool IsPaused => isPaused;
        public float GameTime => gameTime;
        public int EnemiesKilled => enemiesKilled;
    }
}
