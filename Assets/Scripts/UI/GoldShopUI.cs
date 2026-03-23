using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 金币商店UI - 局内使用，用代码动态创建
    /// </summary>
    public class GoldShopUI : MonoBehaviour
    {
        [Header("商店设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.85f, 0.2f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.2f, 0.6f, 0.3f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.3f, 0.7f, 0.4f);
        [Tooltip("面板大小")]
        public Vector2 panelSize = new Vector2(900, 600);

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private Text goldText;
        private List<ShopItemData> currentItems = new List<ShopItemData>();
        private List<GameObject> itemSlots = new List<GameObject>();

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("GoldShopUI 必须放在 Canvas 下面！");
                return;
            }
            CreateGoldShopUI();
        }

        private void CreateGoldShopUI()
        {
            uiRoot = new GameObject("GoldShopUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRT = uiRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

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
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = panelSize;
            panel.AddComponent<Image>().color = panelColor;

            // 标题
            CreateText(panel.transform, "Title", "武器商店", new Vector2(0, panelSize.y / 2 - 40), 
                new Vector2(300, 50), 32, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 金币显示
            goldText = CreateText(panel.transform, "GoldText", "金币: 0", 
                new Vector2(-panelSize.x / 2 + 100, panelSize.y / 2 - 40), 
                new Vector2(200, 35), 24, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleLeft, FontStyle.Bold);

            // 关闭按钮
            CreateButton(panel.transform, "CloseBtn", "X", new Vector2(panelSize.x / 2 - 40, panelSize.y / 2 - 40), 
                new Vector2(40, 40), new Color(0.6f, 0.2f, 0.2f), new Color(0.7f, 0.3f, 0.3f), OnCloseClicked);

            // 物品网格
            CreateItemGrid(panel.transform);

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

        private void CreateItemGrid(Transform parent)
        {
            GameObject grid = new GameObject("ItemGrid");
            grid.transform.SetParent(parent, false);
            RectTransform gridRT = grid.AddComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0.5f, 0.5f);
            gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.pivot = new Vector2(0.5f, 0.5f);
            gridRT.anchoredPosition = new Vector2(0, -20);
            gridRT.sizeDelta = new Vector2(panelSize.x - 60, panelSize.y - 120);

            // 获取商店物品
            if (ShopManager.Instance != null)
            {
                currentItems = ShopManager.Instance.GetWeaponShopItems();
                currentItems.AddRange(ShopManager.Instance.GetPassiveShopItems());
            }

            // 创建物品槽
            int columns = 3;
            float spacing = 20f;
            float slotSize = (panelSize.x - 60 - spacing * (columns - 1)) / columns;

            for (int i = 0; i < currentItems.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                float x = -((columns - 1) * (slotSize + spacing) / 2) + col * (slotSize + spacing);
                float y = ((currentItems.Count / columns) * (slotSize + spacing) / 2) - row * (slotSize + spacing) - slotSize / 2;

                CreateItemSlot(grid.transform, currentItems[i], new Vector2(x, y), slotSize);
            }
        }

        private void CreateItemSlot(Transform parent, ShopItemData item, Vector2 pos, float size)
        {
            GameObject slot = new GameObject($"Slot_{item.itemId}");
            slot.transform.SetParent(parent, false);
            RectTransform slotRT = slot.AddComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.pivot = new Vector2(0.5f, 0.5f);
            slotRT.anchoredPosition = pos;
            slotRT.sizeDelta = new Vector2(size, size);
            slot.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            // 物品名称
            CreateText(slot.transform, "Name", item.itemName, new Vector2(0, size / 2 - 15), 
                new Vector2(size - 10, 25), 16, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 物品等级
            CreateText(slot.transform, "Level", $"Lv.{item.currentLevel}/{item.maxLevel}", 
                new Vector2(0, -size / 2 + 15), new Vector2(size - 10, 20), 12, Color.gray, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 价格
            string priceText = item.goldPrice > 0 ? $"💰 {item.goldPrice}" : "已解锁";
            CreateText(slot.transform, "Price", priceText, new Vector2(0, -size / 2 + 35), 
                new Vector2(size - 10, 18), 14, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleCenter, FontStyle.Normal);

            // 购买按钮
            CreateButton(slot.transform, "BuyBtn", "购买", new Vector2(0, -size / 2 + 55), 
                new Vector2(size - 30, 25), buttonColor, buttonHoverColor, () => OnBuyClicked(item));

            itemSlots.Add(slot);
        }

        private void OnShopOpened()
        {
            if (ShopManager.Instance != null && ShopManager.Instance.IsGoldShopOpen)
            {
                uiRoot.SetActive(true);
                UpdateCurrencyDisplay();
            }
        }

        private void OnShopClosed()
        {
            uiRoot.SetActive(false);
        }

        private void OnItemPurchased(ShopItemData item)
        {
            UpdateCurrencyDisplay();
            RefreshShopItems();
        }

        private void UpdateCurrencyDisplay()
        {
            if (goldText != null && SaveSystem.Instance != null)
            {
                goldText.text = $"金币: {SaveSystem.Instance.GetSessionGold()}";
            }
        }

        private void RefreshShopItems()
        {
            // 刷新物品显示（重新创建或更新）
            // 简化处理：直接刷新整个网格
            foreach (var slot in itemSlots)
            {
                Destroy(slot);
            }
            itemSlots.Clear();

            if (uiRoot.transform.Find("ShopPanel/ItemGrid") != null)
            {
                CreateItemGrid(uiRoot.transform.Find("ShopPanel"));
            }
        }

        private void OnBuyClicked(ShopItemData item)
        {
            if (ShopManager.Instance != null)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(SFXType.ButtonClick);

                bool success = ShopManager.Instance.PurchaseItem(item);
                if (success)
                {
                    Debug.Log($"[GoldShop] 购买成功: {item.itemName}");
                }
            }
        }

        private void OnCloseClicked()
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SFXType.ButtonClick);

            if (ShopManager.Instance != null)
                ShopManager.Instance.CloseGoldShop();
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

            CreateText(btn.transform, "Label", label, Vector2.zero, size, 14, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
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
