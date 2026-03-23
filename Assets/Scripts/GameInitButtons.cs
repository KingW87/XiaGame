using UnityEngine;
using ClawSurvivor.Systems;

namespace ClawSurvivor
{
    /// <summary>
    /// 主菜单按钮事件处理 - 挂载在主菜单预制体的按钮上
    /// </summary>
    public class GameInitButtons : MonoBehaviour
    {
        /// <summary>
        /// 开始游戏按钮调用此方法
        /// </summary>
        public void OnStartGame()
        {
            // 调用GameInit开始游戏
            var gameInit = FindObjectOfType<GameInit>();
            if (gameInit != null)
            {
                gameInit.OnStartGame();
            }

            // 同时调用GameStarter显示游戏对象
            if (GameStarter.Instance != null)
            {
                GameStarter.Instance.StartGame();
            }
        }

        /// <summary>
        /// 珍宝商店按钮调用此方法
        /// </summary>
        public void OnOpenShop()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OpenGemShop();
            }
        }

        /// <summary>
        /// 退出游戏按钮调用此方法
        /// </summary>
        public void OnExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
