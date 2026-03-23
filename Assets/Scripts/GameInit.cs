using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ClawSurvivor.UI;
using ClawSurvivor.Player;
using ClawSurvivor.Enemy;
using ClawSurvivor.Skills;
using ClawSurvivor.Systems;
using ClawSurvivor.Pickups;

namespace ClawSurvivor
{
    /// <summary>
    /// 游戏初始化器 - 负责主菜单场景
    /// 游戏场景作为预制体，点击开始游戏后动态加载
    /// </summary>
    public class GameInit : MonoBehaviour
    {
        [Header("主菜单预制体")]
        [Tooltip("你自己的主菜单预制体（包含Canvas）")]
        public GameObject mainMenuPrefab;

        [Header("战斗场景预制体")]
        [Tooltip("战斗场景预制体（包含Player、EnemySpawner等）")]
        public GameObject battleScenePrefab;

        [Header("场景设置")]
        [Tooltip("游戏场景父物体（用于挂载预制体）")]
        public Transform battleParent;

        private GameObject battleInstance;

        private void Start()
        {
            // 创建管理器（所有场景都需要）
            CreateManagers();

            // 创建主菜单
            CreateMainMenu();

            // 隐藏所有游戏对象（等待开始游戏）
            HideAllGameObjects();

            // 不自动创建战斗场景，等待点击开始游戏
            Debug.Log("主菜单场景初始化完成");
        }

        /// <summary>
        /// 隐藏所有游戏对象 - 确保初始状态只显示主菜单
        /// </summary>
        private void HideAllGameObjects()
        {
            // 如果有GameStarter，让它隐藏游戏对象
            if (GameStarter.Instance != null)
            {
                GameStarter.Instance.HideGameObjects();
            }
            else
            {
                // 手动隐藏
                HideGameObject("MapGenerator");
                HideGameObject("Player");
                HideGameObject("EnemySpawner");
                HideGameObject("GameHUD");
            }
        }

        private void HideGameObject(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                go.SetActive(false);
            }
        }

        /// <summary>
        /// 开始游戏 - 由主菜单按钮调用
        /// </summary>
        public void OnStartGame()
        {
            if (battleInstance != null)
            {
                // 已经加载过，直接显示
                battleInstance.SetActive(true);
                return;
            }

            if (battleScenePrefab != null)
            {
                // 实例化战斗场景预制体
                if (battleParent != null)
                {
                    battleInstance = Instantiate(battleScenePrefab, battleParent);
                }
                else
                {
                    battleInstance = Instantiate(battleScenePrefab);
                }
                battleInstance.name = "BattleScene";
                Debug.Log("战斗场景已加载");
            }
            else
            {
                Debug.LogError("未设置战斗场景预制体！请在GameInit组件中设置Battle Scene Prefab");
            }
        }

        private GameObject menuInstance;

        private void CreateMainMenu()
        {
            if (mainMenuPrefab != null)
            {
                // 使用用户的主菜单预制体
                menuInstance = Instantiate(mainMenuPrefab, Vector3.zero, Quaternion.identity);
                Debug.Log("使用自定义主菜单预制体");

                // 自动绑定按钮事件
                AutoBindButtons(menuInstance);
            }
            else
            {
                // 创建默认主菜单
                CreateDefaultMainMenu();
            }
        }

        /// <summary>
        /// 自动查找并绑定主菜单按钮事件
        /// </summary>
        private void AutoBindButtons(GameObject menuRoot)
        {
            if (menuRoot == null) return;

            // 确保场景中有GameInitButtons脚本
            GameInitButtons buttons = null;
            var existing = FindObjectsOfType<GameInitButtons>();
            if (existing.Length > 0)
            {
                buttons = existing[0];
            }
            else
            {
                // 创建一个挂载GameInitButtons的物体
                GameObject btnGO = new GameObject("MenuButtons");
                btnGO.transform.SetParent(menuRoot.transform);
                buttons = btnGO.AddComponent<GameInitButtons>();
            }

            // 查找所有按钮
            var buttons_comp = menuRoot.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var btn in buttons_comp)
            {
                string btnName = btn.gameObject.name.ToLower();

                if (btnName.Contains("start") || btnName.Contains("开始"))
                {
                    btn.onClick.AddListener(buttons.OnStartGame);
                }
                else if (btnName.Contains("shop") || btnName.Contains("商店"))
                {
                    btn.onClick.AddListener(buttons.OnOpenShop);
                }
                else if (btnName.Contains("exit") || btnName.Contains("quit") || btnName.Contains("退出"))
                {
                    btn.onClick.AddListener(buttons.OnExitGame);
                }
            }
        }

        private void CreateDefaultMainMenu()
        {
            // 创建Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // 创建主菜单UI
            GameObject menu = new GameObject("MainMenuUI");
            menu.transform.SetParent(canvasGO.transform);
            menu.AddComponent<UI.MainMenuUI>();

            Debug.Log("创建默认主菜单");
        }

        private void CreateManagers()
        {
            // 这些管理器需要跨场景保留
            if (GameObject.Find("GameManager") == null)
                new GameObject("GameManager").AddComponent<Systems.GameManager>();

            if (GameObject.Find("ObjectPool") == null)
                new GameObject("ObjectPool").AddComponent<Systems.ObjectPool>();

            if (GameObject.Find("SkillDatabase") == null)
                new GameObject("SkillDatabase").AddComponent<Skills.SkillDatabase>();

            if (GameObject.Find("PickupSpawner") == null)
                new GameObject("PickupSpawner").AddComponent<Pickups.PickupSpawner>();

            if (GameObject.Find("SoundManager") == null)
                new GameObject("SoundManager").AddComponent<Systems.SoundManager>();

            if (GameObject.Find("SaveSystem") == null)
                new GameObject("SaveSystem").AddComponent<Systems.SaveSystem>();

            if (GameObject.Find("ShopManager") == null)
                new GameObject("ShopManager").AddComponent<Systems.ShopManager>();

            if (GameObject.Find("GameStarter") == null)
                new GameObject("GameStarter").AddComponent<Systems.GameStarter>();

            if (GameObject.Find("EquipmentSystem") == null)
                new GameObject("EquipmentSystem").AddComponent<Systems.EquipmentSystem>();

            if (GameObject.Find("PetSystem") == null)
                new GameObject("PetSystem").AddComponent<Systems.PetSystem>();

            if (GameObject.Find("SceneTransition") == null)
                new GameObject("SceneTransition").AddComponent<Systems.SceneTransition>();

            Debug.Log("管理器创建完成");
        }
    }
}
