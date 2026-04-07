using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using ClawSurvivor.UI;
using ClawSurvivor.Player;
using ClawSurvivor.Enemy;
using ClawSurvivor.Skills;
using ClawSurvivor.Systems;
using ClawSurvivor.Pickups;

namespace ClawSurvivor.Editor
{
    /// <summary>
    /// 一键初始化游戏场景 - 通过菜单 "ClawSurvivor/初始化游戏场景" 运行
    /// </summary>
    public class GameSceneSetup : MonoBehaviour
    {
        // 统一字体
        private static TMP_FontAsset _electronicFont;
        private static TMP_FontAsset ElectronicFont
        {
            get
            {
                if (_electronicFont == null)
                {
                    _electronicFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Electronic Highway Sign SDF");
                    if (_electronicFont == null)
                    {
                        _electronicFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Electronic Highway Sign SDF.asset");
                    }
                }
                return _electronicFont;
            }
        }

        [MenuItem("ClawSurvivor/初始化游戏场景")]
        public static void RunSetup()
        {
            try
            {
                // 创建MainCamera（游戏必须有摄像机才能看到画面）
                CreateMainCamera();

                // 先创建Canvas（用于游戏UI）
                GameObject canvasGO = new GameObject("Canvas");
                Canvas canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();

                // 创建Managers
                CreateManager("GameManager", typeof(Systems.GameManager));
                CreateManager("ObjectPool", typeof(Systems.ObjectPool));
                CreateManager("SkillDatabase", typeof(Skills.SkillDatabase));
                CreateManager("PickupSpawner", typeof(Pickups.PickupSpawner));
                CreateManager("SoundManager", typeof(Systems.SoundManager));
                CreateManager("EffectManager", typeof(Effects.EffectManager));
                CreateManager("BalanceManager", typeof(Systems.BalanceManager));
                CreateManager("SaveSystem", typeof(Systems.SaveSystem));
                CreateManager("ShopManager", typeof(Systems.ShopManager));
                CreateManager("EquipmentSystem", typeof(Systems.EquipmentSystem));
                CreateManager("ChapterManager", typeof(Systems.ChapterManager));
                CreateManager("CollectibleSystem", typeof(Systems.CollectibleSystem));
                CreateManager("ExtractionPoint", typeof(Systems.ExtractionPoint));

                // 创建字体管理器并设置字体
                var fontManagerGO = new GameObject("UIFontManager");
                var fontManager = fontManagerGO.AddComponent<Systems.UIFontManager>();
                fontManager.electronicFont = ElectronicFont;

                // 创建战斗场景对象（直接显示）
                CreateBattleSceneObjects(canvas);

                // 创建UI（包括小地图）
                CreateUI(canvas.transform);

                // 创建GameStarter用于管理
                GameObject starterGO = new GameObject("GameStarter");
                starterGO.AddComponent<Systems.GameStarter>();

                Debug.Log("场景初始化完成！直接进入战斗模式！");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameSceneSetup] 初始化失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void CreateBattleSceneObjects(Canvas canvas)
        {
            // 创建玩家
            GameObject playerGO = new GameObject("Player");
            var playerController = playerGO.AddComponent<Player.PlayerController>();
            playerGO.AddComponent<UnityEngine.Rigidbody2D>();
            playerGO.AddComponent<CircleCollider2D>();
            playerGO.AddComponent<SpriteRenderer>();
            playerGO.tag = "Player";

            // 创建地图生成器
            GameObject mapGenGO = new GameObject("MapGenerator");
            var mapGen = mapGenGO.AddComponent<Map.MapGenerator>();

            // 创建敌人生成器
            GameObject spawnerGO = new GameObject("EnemySpawner");
            spawnerGO.AddComponent<Enemy.EnemySpawner>();

            // 创建游戏HUD（必须放在Canvas下）
            GameObject hudGO = new GameObject("GameHUD");
            hudGO.transform.SetParent(canvas.transform);
            var hud = hudGO.AddComponent<UI.GameHUD>();

            // 确保这些对象默认激活
            playerGO.SetActive(true);
            mapGenGO.SetActive(true);
            spawnerGO.SetActive(true);
            hudGO.SetActive(true);

            Debug.Log("战斗场景对象创建完成");
        }

        private static void CreateManager(string name, System.Type type)
        {
            GameObject go = new GameObject(name);
            go.AddComponent(type);
        }

        private static void CreateMainCamera()
        {
            // 检查是否已存在MainCamera
            GameObject existingCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (existingCamera != null)
            {
                Debug.Log("MainCamera已存在，跳过创建");
                return;
            }

            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            
            // 添加Camera组件
            Camera cam = cameraGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.fieldOfView = 60;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000;
            cam.orthographic = true; // 2D游戏使用正交投影
            cam.orthographicSize = 5;
            
            // 添加Audio Listener
            cameraGO.AddComponent<AudioListener>();

            // 添加摄像机跟随脚本
            var cameraFollow = cameraGO.AddComponent<Systems.CameraFollow>();
            cameraFollow.smoothSpeed = 5f;
            cameraFollow.offset = new Vector3(0, 0, -10f);
            
            // 设置位置
            cameraGO.transform.position = new Vector3(0, 10, -10);
            cameraGO.transform.rotation = Quaternion.Euler(45, 0, 0);
            
            Debug.Log("MainCamera创建完成");
        }

        [Header("场景模式")]
        [Tooltip("主菜单模式（已废弃，请保持为false以显示章节选择UI）")]
        public bool mainMenuMode = false; // 默认为false，确保创建ChapterSelectUI

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
            CreateChapterSelectUI(canvas); // 章节选择UI
            CreateManagers();
            CreateGemShopUI(canvas);
            CreateEquipPanelUI(canvas);
            CreatePetPanelUI(canvas);

            // 创建游戏对象（但不立即激活）
            if (createMapGenerator) CreateMapGenerator();
            if (createPlayer) CreatePlayer();
            if (createEnemySpawner) CreateEnemySpawner();
            if (createUI) CreateUI(null);

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

            // 创建 EventSystem（UI交互必须）
            CreateEventSystem();

            return canvas;
        }

        private void CreateEventSystem()
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("EventSystem创建完成");
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

        private void CreateChapterSelectUI(Canvas parentCanvas)
        {
            if (parentCanvas == null) return;

            GameObject chapterSelectGO = new GameObject("ChapterSelectUI");
            chapterSelectGO.transform.SetParent(parentCanvas.transform);
            chapterSelectGO.AddComponent<UI.ChapterSelectUI>();

            Debug.Log("章节选择UI创建完成");
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

            // EffectManager
            GameObject effectManager = new GameObject("EffectManager");
            effectManager.AddComponent<Effects.EffectManager>();

            // BalanceManager
            GameObject balanceManager = new GameObject("BalanceManager");
            balanceManager.AddComponent<Systems.BalanceManager>();

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

            // ChapterManager (章节模式)
            GameObject chapterManager = new GameObject("ChapterManager");
            chapterManager.AddComponent<Systems.ChapterManager>();

            // CollectibleSystem (收藏品系统)
            GameObject collectibleSystem = new GameObject("CollectibleSystem");
            collectibleSystem.AddComponent<Systems.CollectibleSystem>();

            // ExtractionPoint (撤离点)
            GameObject extractionPoint = new GameObject("ExtractionPoint");
            extractionPoint.AddComponent<Systems.ExtractionPoint>();

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

        private static void CreateUI(Transform canvasTransform)
        {
            // 如果没有传入canvasTransform，则查找已有的
            if (canvasTransform == null || canvasTransform.Equals(null))
            {
                // 查找已有的Canvas（由CreateCanvas创建的）
                Canvas existingCanvas = FindObjectOfType<Canvas>();
                if (existingCanvas == null)
                {
                    Debug.LogError("未找到Canvas，请确保先调用CreateCanvas()");
                    return;
                }
                canvasTransform = existingCanvas.transform;
            }

            // 创建血条
            CreateSlider(canvasTransform, "HealthBar", new Vector2(200, 30), new Vector2(20, -30), Color.red);

            // 创建经验条
            CreateSlider(canvasTransform, "ExperienceBar", new Vector2(200, 20), new Vector2(20, -60), Color.yellow);

            // 创建文本
            CreateText(canvasTransform, "LevelText", "Lv.1", new Vector2(20, -90), 24);
            CreateText(canvasTransform, "TimeText", "00:00", new Vector2(-100, 30), 30);
            CreateText(canvasTransform, "KillText", "击杀: 0", new Vector2(-100, 60), 24);

            // 创建HUD脚本
            GameObject hudGO = new GameObject("GameHUD");
            hudGO.transform.SetParent(canvasTransform);
            GameHUD hud = hudGO.AddComponent<UI.GameHUD>();

            // 创建小地图UI
            CreateMiniMap(canvasTransform);

            // 创建Boss血条UI
            GameObject bossBarGO = new GameObject("BossHealthBarUI");
            bossBarGO.transform.SetParent(canvasTransform);
            bossBarGO.AddComponent<UI.BossHealthBarUI>();

            // 创建暂停菜单UI
            GameObject pauseGO = new GameObject("PauseMenuUI");
            pauseGO.transform.SetParent(canvasTransform);
            pauseGO.AddComponent<UI.PauseMenuUI>();

            // 创建金币商店UI
            GameObject goldShopGO = new GameObject("GoldShopUI");
            goldShopGO.transform.SetParent(canvasTransform);
            goldShopGO.AddComponent<UI.GoldShopUI>();

            // 创建游戏结算面板
            GameObject gameOverGO = new GameObject("GameOverPanel");
            gameOverGO.transform.SetParent(canvasTransform);
            gameOverGO.AddComponent<UI.GameOverPanel>();

            // 创建章节结算UI
            GameObject chapterResultGO = new GameObject("ChapterResultUI");
            chapterResultGO.transform.SetParent(canvasTransform);
            chapterResultGO.AddComponent<UI.ChapterResultUI>();

            // 创建升级技能选择面板
            GameObject levelUpGO = new GameObject("LevelUpPanel");
            levelUpGO.transform.SetParent(canvasTransform);
            levelUpGO.AddComponent<UI.LevelUpPanel>();

            // 创建宠物系统
            GameObject petSystem = new GameObject("PetSystem");
            petSystem.AddComponent<Systems.PetSystem>();

            Debug.Log("UI创建完成");
        }
        
        private static void CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchorPos, Color fillColor)
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
        
        private static void CreateText(Transform parent, string name, string content, Vector2 anchorPos, int fontSize)
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
            tmp.font = ElectronicFont;
        }

        private static void CreateMiniMap(Transform parent)
        {
            Debug.Log("[GameSceneSetup] CreateMiniMap 被调用, parent: " + (parent != null ? parent.name : "null"));
            
            // 创建小地图容器（右上角）
            GameObject miniMapGO = new GameObject("MiniMap");
            miniMapGO.transform.SetParent(parent);
            
            Debug.Log("[GameSceneSetup] MiniMap GameObject 创建完成");

            // 添加小地图脚本（脚本内会创建UI）
            var miniMap = miniMapGO.AddComponent<UI.MiniMap>();
            miniMap.mapSize = 150f;
            miniMap.worldSize = 100f;
            miniMap.playerColor = Color.cyan;
            miniMap.enemyColor = Color.red;
            miniMap.playerIconSize = 8f;
            miniMap.enemyIconSize = 5f;

            Debug.Log("[GameSceneSetup] MiniMap组件添加完成");
        }
    }
}
