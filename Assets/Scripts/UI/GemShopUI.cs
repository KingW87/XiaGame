using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 宝石商店UI - 局外主菜单使用
    /// </summary>
    public class GemShopUI : MonoBehaviour
    {
        [Header("商店设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(0.6f, 0.3f, 1f);
        [Tooltip("宠物标签颜色")]
        public Color petTabColor = new Color(1f, 0.5f, 0.2f);
        [Tooltip("升级标签颜色")]
        public Color upgradeTabColor = new Color(0.2f, 0.8f, 1f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.5f, 0.2f, 0.8f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.6f, 0.3f, 0.9f);
        [Tooltip("面板大小")]
        public Vector2 panelSize = new Vector2(800, 550);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private Text gemText;
        private Text goldText;
        private int currentTab; // 0=宠物, 1=升级

        private List<ShopItemData> petItems = new List<ShopItemData>();
        private List<ShopItemData> upgradeItems = new List<ShopItemData>();
        private List<GameObject> itemSlots = new List<GameObject>();

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("GemShopUI 必须放在 Canvas 下面！");
                return;
            }
            CreateGemShopUI();
        }

        private void CreateGemShopUI()
        {
            uiRoot = new GameObject("GemShopUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRT = uiRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            // 遮罩背景
            GameObject mask = new GameObject("Mask");
            mask.transform.SetParent(uiRoot.transform, false);
            RectTransform maskRT = mask.AddComponent<RectTransform>();
            maskRT.anchorMin = Vector2.zero;
            maskRT.anchorMax = Vector2.one;
            maskRT.sizeDelta = Vector2.zero;
            mask.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // 商店面板
            GameObject panel = new GameObject("ShopPanel");
            panel.transform.SetParent(uiRoot.transform, false);
            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = panelSize;
            panel.AddComponent<Image>().color = panelColor;

            // 标题
            CreateText(panel.transform, "Title", "珍宝商店", new Vector2(0, panelSize.y / 2 - 40),
                new Vector2(300, 50), 36, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 货币显示
            gemText = CreateText(panel.transform, "GemText", "💎 0",
                new Vector2(-panelSize.x / 2 + 100, panelSize.y / 2 - 40),
                new Vector2(150, 35), 22, new Color(0.6f, 0.8f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);

            goldText = CreateText(panel.transform, "GoldText", "🏆 0",
                new Vector2(-panelSize.x / 2 + 250, panelSize.y / 2 - 40),
                new Vector2(150, 35), 22, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleLeft, FontStyle.Bold);

            // 关闭按钮
            CreateButton(panel.transform, "CloseBtn", "X", new Vector2(panelSize.x / 2 - 40, panelSize.y / 2 - 40),
                new Vector2(40, 40), new Color(0.6f, 0.2f, 0.2f), new Color(0.7f, 0.3f, 0.3f), OnCloseClicked);

            // 标签页按钮
            CreateButton(panel.transform, "PetTab", "宠物", new Vector2(-120, panelSize.y / 2 - 90),
                new Vector2(120, 35), petTabColor, new Color(1f, 0.6f, 0.3f), () => SwitchTab(0));

            CreateButton(panel.transform, "UpgradeTab", "属性升级", new Vector2(40, panelSize.y / 2 - 90),
                new Vector2(120, 35), upgradeTabColor, new Color(0.3f, 0.9f, 1f), () => SwitchTab(1));

            // 物品网格区域
            GameObject gridArea = new GameObject("ItemGridArea");
            gridArea.transform.SetParent(panel.transform, false);
            RectTransform gridAreaRT = gridArea.AddComponent<RectTransform>();
            gridAreaRT.anchorMin = new Vector2(0.5f, 0.5f);
            gridAreaRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridAreaRT.pivot = new Vector2(0.5f, 0.5f);
            gridAreaRT.anchoredPosition = new Vector2(0, -30);
            gridAreaRT.sizeDelta = new Vector2(panelSize.x - 60, panelSize.y - 180);

            // 初始显示宠物页
            SwitchTab(0);

            // 初始隐藏
            uiRoot.SetActive(false);

            // 监听商店事件
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened += OnShopOpened;
                ShopManager.Instance.OnShopClosed += OnShopClosed;
                ShopManager.Instance.OnItemPurchased += OnItemPurchased;
            }
        }

        private void SwitchTab(int tab)
        {
            currentTab = tab;

            // 清除旧物品槽
            foreach (var slot in itemSlots)
            {
                Destroy(slot);
            }
            itemSlots.Clear();

            // 获取当前页物品
            if (ShopManager.Instance != null)
            {
                if (tab == 0)
                    petItems = ShopManager.Instance.GetPetShopItems();
                else
                    upgradeItems = ShopManager.Instance.GetGemUpgradeItems();
            }

            // 创建物品网格
            var gridArea = uiRoot.transform.Find("ShopPanel/ItemGridArea");
            if (gridArea != null)
            {
                CreateItemGrid(gridArea);
            }
        }

        private void CreateItemGrid(Transform parent)
        {
            List<ShopItemData> items = currentTab == 0 ? petItems : upgradeItems;
            if (items.Count == 0) return;

            int columns = 3;
            float spacing = 15f;
            float slotWidth = (panelSize.x - 60 - spacing * (columns - 1)) / columns;
            float slotHeight = 120f;

            for (int i = 0; i < items.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                float x = -((columns - 1) * (slotWidth + spacing) / 2) + col * (slotWidth + spacing);
                float y = ((items.Count / columns + 1) * (slotHeight + spacing) / 2) - row * (slotHeight + spacing) - slotHeight / 2;

                CreateItemSlot(parent, items[i], new Vector2(x, y), slotWidth, slotHeight);
            }
        }

        private void CreateItemSlot(Transform parent, ShopItemData item, Vector2 pos, float width, float height)
        {
            GameObject slot = new GameObject($"Slot_{item.itemId}");
            slot.transform.SetParent(parent, false);
            RectTransform slotRT = slot.AddComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.pivot = new Vector2(0.5f, 0.5f);
            slotRT.anchoredPosition = pos;
            slotRT.sizeDelta = new Vector2(width, height);
            slot.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 0.9f);

            // 物品名称
            CreateText(slot.transform, "Name", item.itemName, new Vector2(0, height / 2 - 18),
                new Vector2(width - 20, 28), 18, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 物品描述
            CreateText(slot.transform, "Desc", item.description, new Vector2(0, 10),
                new Vector2(width - 30, 35), 11, Color.gray, TextAnchor.UpperCenter, FontStyle.Normal);

            // 等级/状态
            string levelText = item.isUnlocked ? $"Lv.{item.currentLevel}/{item.maxLevel}" : "未解锁";
            Color levelColor = item.isUnlocked ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
            CreateText(slot.transform, "Level", levelText, new Vector2(0, -15),
                new Vector2(width - 20, 20), 12, levelColor, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 价格（宝石）
            string priceText = item.gemPrice > 0 ? $"💎 {item.gemPrice}" : "免费";
            CreateText(slot.transform, "Price", priceText, new Vector2(0, -35),
                new Vector2(width - 20, 20), 16, new Color(0.6f, 0.8f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);

            // 购买/升级按钮
            string btnText = item.isUnlocked ? (item.currentLevel >= item.maxLevel ? "满级" : "升级") : "解锁";
            CreateButton(slot.transform, "ActionBtn", btnText, new Vector2(0, -height / 2 + 18),
                new Vector2(width - 40, 28), buttonColor, buttonHoverColor, () => OnActionClicked(item));

            itemSlots.Add(slot);
        }

        private void OnShopOpened()
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsGemShopOpen)
            {
                uiRoot.SetActive(true);
                UpdateCurrencyDisplay();
                SwitchTab(currentTab);
            }
        }

        private void OnShopClosed()
        {
            uiRoot.SetActive(false);
        }

        private void OnItemPurchased(ShopItemData item)
        {
            UpdateCurrencyDisplay();
            SwitchTab(currentTab);
        }

        private void UpdateCurrencyDisplay()
        {
            if (SaveSystem.Instance != null)
            {
                if (gemText != null)
                    gemText.text = $"💎 {SaveSystem.Instance.GetCurrency(CurrencyType.Gems)}";
                if (goldText != null)
                    goldText.text = $"🏆 {SaveSystem.Instance.GetCurrency(CurrencyType.Gold)}";
            }
        }

        private void OnActionClicked(ShopItemData item)
        {
            if (ShopManager.Instance != null)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(SFXType.ButtonClick);

                bool success = ShopManager.Instance.PurchaseItem(item);
                if (success)
                {
                    Debug.Log($"[GemShop] 操作成功: {item.itemName}");
                }
            }
        }

        private void OnCloseClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SFXType.ButtonClick);

            if (ShopManager.Instance != null)
                ShopManager.Instance.CloseGemShop();
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
            button.colors = colors;
            button.onClick.AddListener(onClick);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btn.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            Text labelTxt = labelObj.AddComponent<Text>();
            labelTxt.text = label;
            labelTxt.fontSize = size.y > 30 ? 16 : 12;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.fontStyle = FontStyle.Bold;
        }

        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened -= OnShopOpened;
                ShopManager.Instance.OnShopClosed -= OnShopClosed;
                ShopManager.Instance.OnItemPurchased -= OnItemPurchased;
            }
        }
    }
}
