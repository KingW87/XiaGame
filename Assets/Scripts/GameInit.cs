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
        private bool isInitialized;

        /// <summary>
        /// 自动添加自身到场景（如果还没有GameInit）
        /// 注意：已禁用自动创建，请通过 GameSceneSetup 菜单来初始化场景
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            // 检测是否已通过GameSceneSetup初始化过，如果是则跳过GameInit
            // GameSceneSetup在Editor命名空间，检查其他已初始化的组件
            if (FindObjectOfType<UI.MainMenuUI>() != null ||
                FindObjectOfType<Systems.GameStarter>() != null ||
                FindObjectOfType<Systems.ChapterManager>() != null)
            {
                return;
            }

            var existing = FindObjectOfType<GameInit>();
            if (existing != null) return;

            GameObject go = new GameObject("GameInit");
            go.AddComponent<GameInit>();
        }

        private void Awake()
        {
            // 防止重复初始化
            if (isInitialized) return;

            // 确保DontDestroyOnLoad
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 如果已经有MainMenuUI，说明GameSceneSetup已经初始化过了，跳过GameInit
            if (FindObjectOfType<UI.MainMenuUI>() != null || FindObjectOfType<Systems.GameStarter>() != null)
            {
                Debug.Log("[GameInit] 检测到GameSceneSetup已初始化，跳过GameInit");
                Destroy(gameObject);
                return;
            }

            if (isInitialized) return;
            isInitialized = true;

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
            Debug.Log("[GameInit] OnStartGame 被调用");

            if (battleInstance != null)
            {
                // 已经加载过，直接显示
                battleInstance.SetActive(true);
                Debug.Log("[GameInit] 战斗场景已存在，直接显示");
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
                // 没有设置battleScenePrefab，依赖GameStarter来显示场景中已有的游戏对象
                Debug.Log("[GameInit] 未设置战斗场景预制体，将通过GameStarter显示现有游戏对象");
            }
        }

        private GameObject menuInstance;

        private void CreateMainMenu()
        {
            if (mainMenuPrefab != null)
            {
                // 使用用户的主菜单预制体
                menuInstance = Instantiate(mainMenuPrefab, Vector3.zero, Quaternion.identity);
                Debug.Log("使用自定义主菜单预制体: " + mainMenuPrefab.name);

                // 检查是否有Canvas，如果没有就创建一个
                var canvas = menuInstance.GetComponentInChildren<Canvas>();
                if (canvas == null)
                {
                    Debug.Log("预制体中没有Canvas，创建一个");
                    GameObject canvasGO = new GameObject("Canvas");
                    canvasGO.transform.SetParent(menuInstance.transform, false);
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasGO.AddComponent<GraphicRaycaster>();
                }

                // 自动绑定按钮事件
                AutoBindButtons(menuInstance);
            }
            else
            {
                Debug.LogWarning("未设置主菜单预制体！");
                // 创建默认主菜单
                CreateDefaultMainMenu();
            }
        }

        /// <summary>
        /// 自动查找并绑定主菜单按钮事件
        /// 如果按钮已挂载MenuButton脚本，则自动跳过
        /// </summary>
        private void AutoBindButtons(GameObject menuRoot)
        {
            if (menuRoot == null) return;

            // 查找所有按钮
            var allButtons = menuRoot.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            Debug.Log($"找到 {allButtons.Length} 个按钮");

            if (allButtons.Length == 0)
            {
                Debug.LogWarning("未找到任何按钮！请确保预制体中有Button组件");
                return;
            }

            foreach (var btn in allButtons)
            {
                // 检查是否已经挂载了MenuButton脚本，如果有就不需要自动绑定了
                var menuBtn = btn.GetComponent<MenuButton>();
                if (menuBtn != null)
                {
                    // 已有MenuButton脚本，功能由用户自行配置
                    continue;
                }

                // 没有MenuButton脚本，则自动根据名称绑定
                string btnName = btn.gameObject.name.ToLower();

                if (btnName.Contains("start") || btnName.Contains("开始"))
                {
                    // 添加MenuButton脚本并设置功能
                    var newMenuBtn = btn.gameObject.AddComponent<MenuButton>();
                    newMenuBtn.function = MenuButton.ButtonFunction.开始游戏;
                    Debug.Log($"[AutoBind] 绑定按钮: {btn.gameObject.name} -> 开始游戏");
                }
                else if (btnName.Contains("shop") || btnName.Contains("商店"))
                {
                    var newMenuBtn = btn.gameObject.AddComponent<MenuButton>();
                    newMenuBtn.function = MenuButton.ButtonFunction.打开商店;
                    Debug.Log($"[AutoBind] 绑定按钮: {btn.gameObject.name} -> 打开商店");
                }
                else if (btnName.Contains("exit") || btnName.Contains("quit") || btnName.Contains("退出"))
                {
                    var newMenuBtn = btn.gameObject.AddComponent<MenuButton>();
                    newMenuBtn.function = MenuButton.ButtonFunction.退出游戏;
                    Debug.Log($"[AutoBind] 绑定按钮: {btn.gameObject.name} -> 退出游戏");
                }
                else if (btnName.Contains("equip") || btnName.Contains("装备"))
                {
                    var newMenuBtn = btn.gameObject.AddComponent<MenuButton>();
                    newMenuBtn.function = MenuButton.ButtonFunction.打开装备强化;
                    Debug.Log($"[AutoBind] 绑定按钮: {btn.gameObject.name} -> 打开装备强化");
                }
                else if (btnName.Contains("pet") || btnName.Contains("宠物"))
                {
                    var newMenuBtn = btn.gameObject.AddComponent<MenuButton>();
                    newMenuBtn.function = MenuButton.ButtonFunction.打开宠物系统;
                    Debug.Log($"[AutoBind] 绑定按钮: {btn.gameObject.name} -> 打开宠物系统");
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

            if (GameObject.Find("EffectManager") == null)
                new GameObject("EffectManager").AddComponent<Effects.EffectManager>();

            if (GameObject.Find("BalanceManager") == null)
                new GameObject("BalanceManager").AddComponent<Systems.BalanceManager>();

            Debug.Log("管理器创建完成");
        }
    }
}
