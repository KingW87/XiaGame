using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 宠物界面 - 显示已解锁宠物、宠物状态、升级等功能
    /// </summary>
    public class PetPanelUI : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.1f, 0.12f, 0.18f, 0.95f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.6f, 0.2f);
        [Tooltip("内容区域颜色")]
        public Color contentColor = new Color(0.15f, 0.18f, 0.25f, 0.9f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.25f, 0.45f, 0.6f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.35f, 0.55f, 0.7f);
        [Tooltip("返回按钮颜色")]
        public Color backButtonColor = new Color(0.6f, 0.25f, 0.25f);
        [Tooltip("返回按钮悬停颜色")]
        public Color backButtonHoverColor = new Color(0.75f, 0.35f, 0.35f);
        [Tooltip("已解锁宠物颜色")]
        public Color unlockedColor = new Color(0.2f, 0.35f, 0.3f);
        [Tooltip("未解锁宠物颜色")]
        public Color lockedColor = new Color(0.15f, 0.15f, 0.2f);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private GameObject contentArea;
        private List<GameObject> petItemList = new List<GameObject>();

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("PetPanelUI 必须放在 Canvas 下面！");
                return;
            }

            CreatePetPanel();
            Hide();
        }

        private void CreatePetPanel()
        {
            uiRoot = new GameObject("PetPanelUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform uiRootRT = uiRoot.AddComponent<RectTransform>();
            uiRootRT.anchorMin = Vector2.zero;
            uiRootRT.anchorMax = Vector2.one;
            uiRootRT.pivot = new Vector2(0.5f, 0.5f);
            uiRootRT.anchoredPosition = Vector2.zero;
            uiRootRT.sizeDelta = Vector2.zero;

            // 背景遮罩
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(uiRoot.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // 面板容器
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(uiRoot.transform, false);
            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = new Vector2(700, 500);
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = panelColor;

            // 标题
            CreateText(panel.transform, "Title", "宠物界面", new Vector2(0, 220),
                new Vector2(400, 50), 36, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 返回按钮
            CreateButton(panel.transform, "BackBtn", "← 返回", new Vector2(-300, 220),
                new Vector2(120, 45), backButtonColor, backButtonHoverColor, OnBackClick);

            // 货币显示
            if (SaveSystem.Instance != null)
            {
                var data = SaveSystem.Instance.CurrentData;
                if (data != null)
                {
                    string currencyText = $"宠物碎片: {data.petFragments} | 宝石: {data.gems}";
                    CreateText(panel.transform, "Currency", currencyText, new Vector2(0, 170),
                        new Vector2(600, 35), 20, new Color(0.9f, 0.7f, 0.4f), TextAnchor.MiddleCenter, FontStyle.Normal);
                }
            }

            // 内容区域
            contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(panel.transform, false);
            RectTransform contentRT = contentArea.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.anchoredPosition = new Vector2(0, -30);
            contentRT.sizeDelta = new Vector2(640, 340);
            contentArea.AddComponent<Image>().color = contentColor;

            RefreshPetList();
        }

        private void RefreshPetList()
        {
            ClearContent();

            if (SaveSystem.Instance == null || SaveSystem.Instance.CurrentData == null) return;
            var data = SaveSystem.Instance.CurrentData;

            // 示例宠物数据
            string[] petNames = { "小精灵", "火焰猫", "冰霜狼", "雷电鹰", "暗影豹" };
            string[] petEffects = { "自动攻击", "火焰伤害", "冰霜减速", "雷电链锁", "暗影护盾" };
            int[] petLevels = { 1, 0, 0, 0, 0 };
            int[] maxLevels = { 5, 5, 5, 5, 5 };
            int[] unlockCosts = { 0, 50, 80, 100, 120 };
            int[] upgradeCosts = { 30, 40, 50, 60, 70 };

            for (int i = 0; i < petNames.Length; i++)
            {
                int savedLevel = data.petLevels.GetValue(i.ToString(), petLevels[i]);
                bool isUnlocked = savedLevel > 0 || (i == 0); // 第一个宠物默认解锁
                CreatePetItem(i, petNames[i], petEffects[i], savedLevel, maxLevels[i], 
                    isUnlocked ? upgradeCosts[savedLevel] * savedLevel : unlockCosts[i]);
            }

            CreateText(contentArea.transform, "Tip", "宠物自动战斗，升级提升属性", new Vector2(0, -150),
                new Vector2(400, 30), 18, new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, FontStyle.Normal);
        }

        private void CreatePetItem(int index, string name, string effect, int level, int maxLevel, int cost)
        {
            bool isUnlocked = level > 0 || index == 0;

            GameObject item = new GameObject($"Pet_{name}");
            item.transform.SetParent(contentArea.transform, false);
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 110 - index * 65);
            rt.sizeDelta = new Vector2(580, 55);
            item.AddComponent<Image>().color = isUnlocked ? unlockedColor : lockedColor;

            // 宠物图标占位（圆形）
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(item.transform, false);
            RectTransform iconRT = icon.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(-220, 0);
            iconRT.sizeDelta = new Vector2(45, 45);
            Image iconImg = icon.AddComponent<Image>();
            iconImg.color = isUnlocked ? GetPetColor(index) : new Color(0.3f, 0.3f, 0.3f);

            // 宠物名称
            Color textColor = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            CreateText(item.transform, "Name", name, new Vector2(-150, 8),
                new Vector2(150, 30), 20, textColor, TextAnchor.MiddleLeft, FontStyle.Bold);

            // 效果描述
            CreateText(item.transform, "Effect", effect, new Vector2(-150, -12),
                new Vector2(180, 25), 16, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft, FontStyle.Normal);

            // 等级显示
            string levelText = isUnlocked ? $"Lv.{level}/{maxLevel}" : "未解锁";
            Color levelColor = isUnlocked ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.6f, 0.4f, 0.3f);
            CreateText(item.transform, "Level", levelText, new Vector2(50, 0),
                new Vector2(100, 50), 18, levelColor, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 操作按钮
            if (isUnlocked && level < maxLevel)
            {
                CreateButton(item.transform, "UpgradeBtn", $"升级({cost})", new Vector2(190, 0),
                    new Vector2(120, 38), buttonColor, buttonHoverColor, () => OnUpgradePet(index, level, cost));
            }
            else if (!isUnlocked)
            {
                CreateButton(item.transform, "UnlockBtn", $"解锁({cost})", new Vector2(190, 0),
                    new Vector2(120, 38), new Color(0.5f, 0.3f, 0.6f), new Color(0.6f, 0.4f, 0.7f),
                    () => OnUnlockPet(index, cost));
            }
            else if (level >= maxLevel)
            {
                CreateText(item.transform, "Max", "满级", new Vector2(190, 0),
                    new Vector2(120, 38), 18, new Color(0.9f, 0.8f, 0.3f), TextAnchor.MiddleCenter, FontStyle.Bold);
            }

            petItemList.Add(item);
        }

        private Color GetPetColor(int index)
        {
            switch (index)
            {
                case 0: return new Color(0.3f, 0.8f, 0.5f); // 小精灵 - 绿色
                case 1: return new Color(1f, 0.4f, 0.2f);   // 火焰猫 - 橙红色
                case 2: return new Color(0.4f, 0.7f, 1f);   // 冰霜狼 - 蓝色
                case 3: return new Color(0.9f, 0.8f, 0.2f); // 雷电鹰 - 黄色
                case 4: return new Color(0.5f, 0.3f, 0.7f); // 暗影豹 - 紫色
                default: return Color.gray;
            }
        }

        private void ClearContent()
        {
            foreach (var item in petItemList) Destroy(item);
            petItemList.Clear();
        }

        private void OnUpgradePet(int petId, int currentLevel, int cost)
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentData;

            if (data.petFragments >= cost)
            {
                data.SpendCurrency(CurrencyType.PetFragments, cost);
                data.petLevels.SetValue(petId.ToString(), currentLevel + 1);
                SaveSystem.Instance.SaveGame();
                Debug.Log($"[Pet] 升级宠物 {petId} 到 {currentLevel + 1} 级");
                RefreshPetList();
            }
            else
            {
                Debug.Log("[Pet] 宠物碎片不足！");
            }
        }

        private void OnUnlockPet(int petId, int cost)
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentData;

            if (data.petFragments >= cost)
            {
                data.SpendCurrency(CurrencyType.PetFragments, cost);
                data.petLevels.SetValue(petId.ToString(), 1);
                SaveSystem.Instance.SaveGame();
                Debug.Log($"[Pet] 解锁宠物 {petId}");
                RefreshPetList();
            }
            else
            {
                Debug.Log("[Pet] 宠物碎片不足！");
            }
        }

        private void OnBackClick()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
            Hide();
            // 显示主菜单
            var mainMenu = GetComponentInParent<Canvas>().GetComponentInChildren<MainMenuUI>();
            if (mainMenu != null) mainMenu.Show();
        }

        private Text CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size,
            int fontSize, Color color, TextAnchor align, FontStyle style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            return txt;
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size,
            Color color, Color hoverColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent, false);
            RectTransform rt = btn.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btn.AddComponent<Image>().color = color;
            Button button = btn.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f);
            button.colors = colors;
            button.onClick.AddListener(onClick);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btn.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.pivot = new Vector2(0.5f, 0.5f);
            labelRT.anchoredPosition = Vector2.zero;
            labelRT.sizeDelta = Vector2.zero;
            Text labelTxt = labelObj.AddComponent<Text>();
            labelTxt.text = label;
            labelTxt.fontSize = 16;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.fontStyle = FontStyle.Bold;
        }

        public void Show()
        {
            if (uiRoot != null)
            {
                uiRoot.SetActive(true);
                RefreshPetList();
            }
        }

        public void Hide()
        {
            if (uiRoot != null)
                uiRoot.SetActive(false);
        }
    }
}
