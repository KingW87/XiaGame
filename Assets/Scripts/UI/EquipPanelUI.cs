using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 装备界面 - 显示已解锁武器、被动道具等信息
    /// </summary>
    public class EquipPanelUI : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.1f, 0.12f, 0.18f, 0.95f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(0.3f, 0.7f, 0.9f);
        [Tooltip("内容区域颜色")]
        public Color contentColor = new Color(0.15f, 0.18f, 0.25f, 0.9f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.2f, 0.5f, 0.4f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.3f, 0.65f, 0.5f);
        [Tooltip("返回按钮颜色")]
        public Color backButtonColor = new Color(0.6f, 0.25f, 0.25f);
        [Tooltip("返回按钮悬停颜色")]
        public Color backButtonHoverColor = new Color(0.75f, 0.35f, 0.35f);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private GameObject contentArea;
        private List<GameObject> weaponItemList = new List<GameObject>();
        private List<GameObject> passiveItemList = new List<GameObject>();

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("EquipPanelUI 必须放在 Canvas 下面！");
                return;
            }

            CreateEquipPanel();
            Hide();
        }

        private void CreateEquipPanel()
        {
            uiRoot = new GameObject("EquipPanelUI");
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
            CreateText(panel.transform, "Title", "装备界面", new Vector2(0, 220), 
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
                    string currencyText = $"金币: {data.gold} | 宝石: {data.gems} | 碎片: {data.weaponFragments}";
                    CreateText(panel.transform, "Currency", currencyText, new Vector2(0, 170),
                        new Vector2(600, 35), 20, new Color(0.9f, 0.85f, 0.5f), TextAnchor.MiddleCenter, FontStyle.Normal);
                }
            }

            // 标签页：武器 / 被动道具
            CreateTabButtons(panel.transform);

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

            // 初始显示武器列表
            ShowWeaponList();
        }

        private void CreateTabButtons(Transform parent)
        {
            // 武器标签
            GameObject weaponTab = new GameObject("WeaponTab");
            weaponTab.transform.SetParent(parent, false);
            RectTransform wtRT = weaponTab.AddComponent<RectTransform>();
            wtRT.anchorMin = new Vector2(0.5f, 0.5f);
            wtRT.anchorMax = new Vector2(0.5f, 0.5f);
            wtRT.pivot = new Vector2(0.5f, 0.5f);
            wtRT.anchoredPosition = new Vector2(-120, 120);
            wtRT.sizeDelta = new Vector2(150, 40);
            Image wtImg = weaponTab.AddComponent<Image>();
            wtImg.color = buttonColor;
            Button wtBtn = weaponTab.AddComponent<Button>();
            ColorBlock wtColors = wtBtn.colors;
            wtColors.highlightedColor = buttonHoverColor;
            wtBtn.colors = wtColors;
            wtBtn.onClick.AddListener(ShowWeaponList);
            CreateText(weaponTab.transform, "Label", "武器", Vector2.zero, 
                new Vector2(150, 40), 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 被动道具标签
            GameObject passiveTab = new GameObject("PassiveTab");
            passiveTab.transform.SetParent(parent, false);
            RectTransform ptRT = passiveTab.AddComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0.5f, 0.5f);
            ptRT.anchorMax = new Vector2(0.5f, 0.5f);
            ptRT.pivot = new Vector2(0.5f, 0.5f);
            ptRT.anchoredPosition = new Vector2(80, 120);
            ptRT.sizeDelta = new Vector2(150, 40);
            Image ptImg = passiveTab.AddComponent<Image>();
            ptImg.color = new Color(0.4f, 0.4f, 0.5f);
            Button ptBtn = passiveTab.AddComponent<Button>();
            ColorBlock ptColors = ptBtn.colors;
            ptColors.highlightedColor = new Color(0.5f, 0.5f, 0.6f);
            ptBtn.colors = ptColors;
            ptBtn.onClick.AddListener(ShowPassiveList);
            CreateText(passiveTab.transform, "Label", "被动道具", Vector2.zero,
                new Vector2(150, 40), 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        private void ShowWeaponList()
        {
            ClearContent();

            if (SaveSystem.Instance == null || SaveSystem.Instance.CurrentData == null) return;
            var data = SaveSystem.Instance.CurrentData;
            var keys = data.weaponLevels.GetAllKeys();

            // 示例武器列表（根据实际游戏数据调整）
            string[] sampleWeapons = { "利爪", "冲刺", "旋风", "护盾", "雷击" };
            int[] sampleLevels = { 1, 0, 0, 0, 0 }; // 默认等级

            for (int i = 0; i < sampleWeapons.Length; i++)
            {
                int level = data.weaponLevels.GetValue(sampleWeapons[i], sampleLevels[i]);
                CreateWeaponItem(i, sampleWeapons[i], level);
            }

            // 显示提示
            CreateText(contentArea.transform, "Tip", "点击武器可升级（消耗碎片）", new Vector2(0, -150),
                new Vector2(400, 30), 18, new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, FontStyle.Normal);
        }

        private void ShowPassiveList()
        {
            ClearContent();

            if (SaveSystem.Instance == null || SaveSystem.Instance.CurrentData == null) return;
            var data = SaveSystem.Instance.CurrentData;

            // 示例被动道具
            string[] passives = { "力量戒指", "敏捷之靴", "经验宝珠", "生命护符", "幸运护符" };
            bool[] unlocked = { true, false, false, false, false };

            for (int i = 0; i < passives.Length; i++)
            {
                CreatePassiveItem(i, passives[i], unlocked[i]);
            }

            CreateText(contentArea.transform, "Tip", "未解锁道具可通过商店购买", new Vector2(0, -150),
                new Vector2(400, 30), 18, new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, FontStyle.Normal);
        }

        private void CreateWeaponItem(int index, string name, int level)
        {
            GameObject item = new GameObject($"Weapon_{name}");
            item.transform.SetParent(contentArea.transform, false);
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 100 - index * 60);
            rt.sizeDelta = new Vector2(580, 50);
            item.AddComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 0.8f);

            // 武器名称
            CreateText(item.transform, "Name", name, new Vector2(-200, 0),
                new Vector2(180, 50), 22, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);

            // 等级显示
            string levelText = level > 0 ? $"Lv.{level}" : "未解锁";
            Color levelColor = level > 0 ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.5f, 0.5f, 0.5f);
            CreateText(item.transform, "Level", levelText, new Vector2(0, 0),
                new Vector2(100, 50), 20, levelColor, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 升级按钮
            if (level > 0)
            {
                int cost = level * 50; // 升级费用
                CreateButton(item.transform, "UpgradeBtn", $"升级({cost}碎片)", new Vector2(180, 0),
                    new Vector2(140, 38), buttonColor, buttonHoverColor, () => OnUpgradeWeapon(name, level, cost));
            }
            else
            {
                int cost = 100; // 解锁费用
                CreateButton(item.transform, "UnlockBtn", $"解锁({cost}碎片)", new Vector2(180, 0),
                    new Vector2(140, 38), new Color(0.5f, 0.3f, 0.6f), new Color(0.6f, 0.4f, 0.7f), 
                    () => OnUnlockWeapon(name, cost));
            }

            weaponItemList.Add(item);
        }

        private void CreatePassiveItem(int index, string name, bool unlocked)
        {
            GameObject item = new GameObject($"Passive_{name}");
            item.transform.SetParent(contentArea.transform, false);
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 100 - index * 60);
            rt.sizeDelta = new Vector2(580, 50);
            item.AddComponent<Image>().color = unlocked ? new Color(0.2f, 0.25f, 0.35f, 0.8f) : new Color(0.15f, 0.15f, 0.2f, 0.6f);

            // 名称
            Color textColor = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            CreateText(item.transform, "Name", name, new Vector2(-200, 0),
                new Vector2(250, 50), 22, textColor, TextAnchor.MiddleLeft, FontStyle.Bold);

            // 状态
            string statusText = unlocked ? "已装备" : "未解锁";
            CreateText(item.transform, "Status", statusText, new Vector2(100, 0),
                new Vector2(120, 50), 18, unlocked ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.6f, 0.4f, 0.3f),
                TextAnchor.MiddleCenter, FontStyle.Normal);

            passiveItemList.Add(item);
        }

        private void ClearContent()
        {
            foreach (var item in weaponItemList) Destroy(item);
            foreach (var item in passiveItemList) Destroy(item);
            weaponItemList.Clear();
            passiveItemList.Clear();
        }

        private void OnUpgradeWeapon(string name, int currentLevel, int cost)
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentData;

            if (data.weaponFragments >= cost)
            {
                data.SpendCurrency(CurrencyType.WeaponFragments, cost);
                data.weaponLevels.SetValue(name, currentLevel + 1);
                SaveSystem.Instance.SaveGame();
                Debug.Log($"[Equip] 升级 {name} 到 {currentLevel + 1} 级");
                ShowWeaponList(); // 刷新显示
            }
            else
            {
                Debug.Log("[Equip] 碎片不足！");
            }
        }

        private void OnUnlockWeapon(string name, int cost)
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentData;

            if (data.weaponFragments >= cost)
            {
                data.SpendCurrency(CurrencyType.WeaponFragments, cost);
                data.weaponLevels.SetValue(name, 1);
                SaveSystem.Instance.SaveGame();
                Debug.Log($"[Equip] 解锁 {name}");
                ShowWeaponList();
            }
            else
            {
                Debug.Log("[Equip] 碎片不足！");
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
            labelTxt.fontSize = 18;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.fontStyle = FontStyle.Bold;
        }

        public void Show()
        {
            if (uiRoot != null)
            {
                uiRoot.SetActive(true);
                ShowWeaponList();
            }
        }

        public void Hide()
        {
            if (uiRoot != null)
                uiRoot.SetActive(false);
        }
    }
}
