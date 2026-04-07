using UnityEngine;
using UnityEngine.SceneManagement;
using ClawSurvivor.Systems;

namespace ClawSurvivor
{
    /// <summary>
    /// 主菜单按钮功能 - 挂载在按钮预制体上
    /// 拖入不同的预制体即可实现不同功能
    /// </summary>
    public class MenuButton : MonoBehaviour
    {
        public enum ButtonFunction
        {
            开始游戏,
            打开商店,
            打开装备强化,
            打开宠物系统,
            退出游戏
        }

        [Header("按钮功能")]
        [Tooltip("选择这个按钮要执行的功能")]
        public ButtonFunction function = ButtonFunction.开始游戏;

        private void Start()
        {
            // 自动添加点击事件
            var button = GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                Debug.Log($"[MenuButton] 按钮: {gameObject.name}, 功能: {function}, Interactable: {button.interactable}");
                button.onClick.AddListener(OnClick);
            }
            else
            {
                Debug.LogError($"[MenuButton] 按钮: {gameObject.name} 没有Button组件!");
            }
        }

        private void OnClick()
        {
            switch (function)
            {
                case ButtonFunction.开始游戏:
                    OnStartGame();
                    break;
                case ButtonFunction.打开商店:
                    OnOpenShop();
                    break;
                case ButtonFunction.打开装备强化:
                    OnOpenEquipment();
                    break;
                case ButtonFunction.打开宠物系统:
                    OnOpenPet();
                    break;
                case ButtonFunction.退出游戏:
                    OnExitGame();
                    break;
            }
        }

        private void OnStartGame()
        {
            Debug.Log("[MenuButton] 开始游戏按钮被点击");

            var gameInit = FindObjectOfType<GameInit>();
            if (gameInit != null)
            {
                Debug.Log("[MenuButton] 找到GameInit，调用OnStartGame");
                gameInit.OnStartGame();
            }
            else
            {
                Debug.LogWarning("[MenuButton] 未找到GameInit!");
            }

            if (GameStarter.Instance != null)
            {
                Debug.Log("[MenuButton] 找到GameStarter，调用StartGame");
                GameStarter.Instance.StartGame();
            }
            else
            {
                Debug.LogWarning("[MenuButton] 未找到GameStarter!");
            }
        }

        private void OnOpenShop()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OpenGemShop();
            }
        }

        private void OnOpenEquipment()
        {
            // 调用主菜单UI打开装备面板
            var mainMenu = FindObjectOfType<UI.MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.OpenEquipPanel();
            }
        }

        private void OnOpenPet()
        {
            // 调用主菜单UI打开宠物面板
            var mainMenu = FindObjectOfType<UI.MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.OpenPetPanel();
            }
        }

        private void OnExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
