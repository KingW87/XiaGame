using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClawSurvivor.UI;
using ClawSurvivor.Player;
using ClawSurvivor.Enemy;
using ClawSurvivor.Skills;
using ClawSurvivor.Systems;
using ClawSurvivor.Pickups;

namespace ClawSurvivor.Editor
{
    /// <summary>
    /// 一键初始化游戏场景 - 放在空场景中运行即可自动创建所有游戏对象
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        [Header("场景模式")]
        [Tooltip("主菜单模式：只创建Canvas和主菜单UI，不创建游戏对象")]
        public bool mainMenuMode = false;

        [Header("自动创建游戏")]
        [Tooltip("创建玩家")]
        public bool createPlayer = true;
        [Tooltip("创建敌人生成器")]
        public bool createEnemySpawner = true;
        [Tooltip("创建管理器（GameManager/ObjectPool/SkillDB等）")]
        public bool createManagers = true;
        [Tooltip("创建游戏UI（HUD/血条/经验条等）")]
        public bool createUI = true;
        [Tooltip("创建无限地图生成器")]
        public bool createMapGenerator = true;

        // 单例
        public static GameSceneSetup Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            // 延时执行，确保Unity完全初始化
            Invoke(nameof(SetupGameScene), 0.5f);
            // 不销毁自己，保留单例
        }
        
        private void SetupGameScene()
        {
            // 始终创建Canvas
            Canvas canvas = null;

            if (mainMenuMode)
            {
                // 主菜单模式：只创建主菜单
                canvas = CreateCanvas();
                CreateSceneTransition();
                CreateMainMenuUI(canvas);
                CreateManagers(); // 仍需要SoundManager
                CreateGemShopUI(canvas); // 宝石商店UI
                CreateEquipPanelUI(canvas); // 装备界面
                CreatePetPanelUI(canvas); // 宠物界面
                Debug.Log("主菜单场景初始化完成！");
                return;
            }

            // 游戏模式：先显示主菜单，点击开始游戏后再进入游戏
            canvas = CreateCanvas();
            CreateSceneTransition();
            CreateMainMenuUI(canvas);
            CreateManagers();
            CreateGemShopUI(canvas);
            CreateEquipPanelUI(canvas);
            CreatePetPanelUI(canvas);

            // 创建游戏对象（但不立即激活）
            if (createMapGenerator) CreateMapGenerator();
            if (createPlayer) CreatePlayer();
            if (createEnemySpawner) CreateEnemySpawner();
            if (createUI) CreateUI();

            // 隐藏游戏对象，等待开始游戏
            if (GameStarter.Instance != null)
                GameStarter.Instance.HideGameObjects();

            Debug.Log("游戏场景初始化完成（从主菜单开始）！");
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

            return canvas;
        }

        private void CreateSceneTransition()
        {
            // 场景过渡管理器（DontDestroyOnLoad，只创建一次）
            if (Systems.SceneTransition.Instance == null)
            {
                GameObject transitionGO = new GameObject("SceneTransition");
                transitionGO.AddComponent<Systems.SceneTransition>();
            }
        }

        private void CreateMainMenuUI(Canvas parentCanvas)
        {
            if (parentCanvas == null) return;

            GameObject menuGO = new GameObject("MainMenuUI");
            menuGO.transform.SetParent(parentCanvas.transform);
            MainMenuUI menu = menuGO.AddComponent<UI.MainMenuUI>();

            // 将主菜单引用传递给SceneTransition
            if (Systems.SceneTransition.Instance != null)
                Systems.SceneTransition.Instance.SetMainMenuUI(menu);

            Debug.Log("主菜单UI创建完成");
        }

        private void CreateGemShopUI(Canvas parentCanvas)
        {
            if (parentCanvas == null) return;

            GameObject gemShopGO = new GameObject("GemShopUI");
            gemShopGO.transform.SetParent(parentCanvas.transform);
            gemShopGO.AddComponent<UI.GemShopUI>();

            Debug.Log("宝石商店UI创建完成");
        }

        private void CreateEquipPanelUI(Canvas parentCanvas)
        {
            if (parentCanvas == null) return;

            GameObject equipGO = new GameObject("EquipPanelUI");
            equipGO.transform.SetParent(parentCanvas.transform);
            equipGO.AddComponent<UI.EquipPanelUI>();

            Debug.Log("装备界面创建完成");
        }

        private void CreatePetPanelUI(Canvas parentCanvas)
        {
            if (parentCanvas == null) return;

            GameObject petGO = new GameObject("PetPanelUI");
            petGO.transform.SetParent(parentCanvas.transform);
            petGO.AddComponent<UI.PetPanelUI>();

            Debug.Log("宠物界面创建完成");
        }

        private void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            
            // 添加 SpriteRenderer（2D需要这个）
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("Sprites/Default");
            sr.color = Color.blue;
            
            // 添加 BoxCollider2D
            BoxCollider2D col = player.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            
            // 添加 Rigidbody2D
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            
            // 添加脚本
            player.AddComponent<Player.PlayerController>();
            
            // 设置颜色
            player.GetComponent<SpriteRenderer>().color = Color.blue;
            
            Debug.Log("玩家创建完成");
        }
        
        private void CreateManagers()
        {
            // GameManager
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<Systems.GameManager>();

            // ObjectPool
            GameObject objectPool = new GameObject("ObjectPool");
            objectPool.AddComponent<Systems.ObjectPool>();

            // SkillDatabase
            GameObject skillDB = new GameObject("SkillDatabase");
            skillDB.AddComponent<Skills.SkillDatabase>();

            // PickupSpawner
            GameObject pickupSpawner = new GameObject("PickupSpawner");
            pickupSpawner.AddComponent<Pickups.PickupSpawner>();

            // SoundManager
            GameObject soundManager = new GameObject("SoundManager");
            soundManager.AddComponent<Systems.SoundManager>();

            // SaveSystem (DontDestroyOnLoad)
            GameObject saveSystem = new GameObject("SaveSystem");
            saveSystem.AddComponent<Systems.SaveSystem>();

            // ShopManager
            GameObject shopManager = new GameObject("ShopManager");
            shopManager.AddComponent<Systems.ShopManager>();

            // GameStarter
            GameObject gameStarter = new GameObject("GameStarter");
            gameStarter.AddComponent<Systems.GameStarter>();

            // EquipmentSystem
            GameObject equipmentSystem = new GameObject("EquipmentSystem");
            equipmentSystem.AddComponent<Systems.EquipmentSystem>();

            Debug.Log("管理器创建完成");
        }
        
        private void CreateMapGenerator()
        {
            GameObject mapGen = new GameObject("MapGenerator");
            mapGen.AddComponent<Map.MapGenerator>();
            Debug.Log("地图生成器创建完成");
        }
        
        private void CreateEnemySpawner()
        {
            GameObject spawner = new GameObject("EnemySpawner");
            spawner.AddComponent<Enemy.EnemySpawner>();
            Debug.Log("敌人生成器创建完成");
        }

        private void CreateUI()
        {
            // 创建 Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // 设置Canvas材质
            canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            
            // 创建血条
            CreateSlider(canvasGO.transform, "HealthBar", new Vector2(200, 30), new Vector2(20, -30), Color.red);
            
            // 创建经验条
            CreateSlider(canvasGO.transform, "ExperienceBar", new Vector2(200, 20), new Vector2(20, -60), Color.yellow);
            
            // 创建文本
            CreateText(canvasGO.transform, "LevelText", "Lv.1", new Vector2(20, -90), 24);
            CreateText(canvasGO.transform, "TimeText", "00:00", new Vector2(-100, 30), 30);
            CreateText(canvasGO.transform, "KillText", "击杀: 0", new Vector2(-100, 60), 24);

            // 创建HUD脚本
            GameObject hudGO = new GameObject("GameHUD");
            hudGO.transform.SetParent(canvasGO.transform);
            GameHUD hud = hudGO.AddComponent<UI.GameHUD>();

            // 创建Boss血条UI
            GameObject bossBarGO = new GameObject("BossHealthBarUI");
            bossBarGO.transform.SetParent(canvasGO.transform);
            bossBarGO.AddComponent<UI.BossHealthBarUI>();

            // 创建暂停菜单UI
            GameObject pauseGO = new GameObject("PauseMenuUI");
            pauseGO.transform.SetParent(canvasGO.transform);
            pauseGO.AddComponent<UI.PauseMenuUI>();

            // 创建金币商店UI
            GameObject goldShopGO = new GameObject("GoldShopUI");
            goldShopGO.transform.SetParent(canvasGO.transform);
            goldShopGO.AddComponent<UI.GoldShopUI>();

            // 创建宠物系统
            GameObject petSystem = new GameObject("PetSystem");
            petSystem.AddComponent<Systems.PetSystem>();

            Debug.Log("UI创建完成");
        }
        
        private void CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchorPos, Color fillColor)
        {
            GameObject sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent);
            
            RectTransform rt = sliderGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = anchorPos;
            rt.sizeDelta = size;
            
            // 背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGO.transform);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            
            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;
            
            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            
            // Slider组件
            Slider slider = sliderGO.AddComponent<Slider>();
            slider.targetGraphic = bgImg;
            slider.fillRect = fillRT;
            slider.direction = Slider.Direction.LeftToRight;
        }
        
        private void CreateText(Transform parent, string name, string content, Vector2 anchorPos, int fontSize)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent);
            
            RectTransform rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = anchorPos;
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
        }
    }
}
