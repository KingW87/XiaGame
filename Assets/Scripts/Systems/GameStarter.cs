using UnityEngine;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 游戏启动器 - 管理从主菜单到游戏的过渡
    /// 单例模式，挂载在一个DontDestroy的GameObject上
    /// </summary>
    public class GameStarter : MonoBehaviour
    {
        public static GameStarter Instance;

        // 需要隐藏的游戏对象名称
        private string[] gameObjectNames = { "MapGenerator", "Player", "EnemySpawner", "GameHUD" };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 开始游戏 - 显示游戏对象并启用组件
        /// </summary>
        public void StartGame()
        {
            // 显示并启用游戏对象
            var mapGen = GameObject.Find("MapGenerator");
            if (mapGen != null)
            {
                mapGen.SetActive(true);
                var mg = mapGen.GetComponent<Map.MapGenerator>();
                if (mg != null) mg.enabled = true;
            }

            var player = GameObject.Find("Player");
            if (player != null)
            {
                player.SetActive(true);
                var pc = player.GetComponent<Player.PlayerController>();
                if (pc != null) pc.enabled = true;
            }

            var spawner = GameObject.Find("EnemySpawner");
            if (spawner != null)
            {
                spawner.SetActive(true);
                var es = spawner.GetComponent<Enemy.EnemySpawner>();
                if (es != null) es.enabled = true;
            }

            var hud = GameObject.Find("GameHUD");
            if (hud != null)
            {
                hud.SetActive(true);
                var hudComp = hud.GetComponent<UI.GameHUD>();
                if (hudComp != null) hudComp.enabled = true;
            }

            // 恢复游戏时间
            Time.timeScale = 1;

            Debug.Log("[GameStarter] 游戏开始");
        }

        /// <summary>
        /// 隐藏游戏对象并禁用组件 - 用于从主菜单启动时
        /// </summary>
        public void HideGameObjects()
        {
            // 禁用并隐藏游戏对象（确保 Update 不再运行）
            var mapGen = GameObject.Find("MapGenerator");
            if (mapGen != null)
            {
                var mg = mapGen.GetComponent<Map.MapGenerator>();
                if (mg != null) mg.enabled = false;
                mapGen.SetActive(false);
            }

            var player = GameObject.Find("Player");
            if (player != null)
            {
                var pc = player.GetComponent<Player.PlayerController>();
                if (pc != null) pc.enabled = false;
                player.SetActive(false);
            }

            var spawner = GameObject.Find("EnemySpawner");
            if (spawner != null)
            {
                var es = spawner.GetComponent<Enemy.EnemySpawner>();
                if (es != null) es.enabled = false;
                spawner.SetActive(false);
            }

            var hud = GameObject.Find("GameHUD");
            if (hud != null)
            {
                var hudComp = hud.GetComponent<UI.GameHUD>();
                if (hudComp != null) hudComp.enabled = false;
                hud.SetActive(false);
            }

            Debug.Log("[GameStarter] 游戏对象已隐藏");
        }
    }
}
