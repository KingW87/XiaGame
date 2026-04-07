using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ClawSurvivor.Systems;

namespace ClawSurvivor.UI
{
    /// <summary>
    /// 章节选择UI - 选择章节、装备、宠物后进入游戏
    /// </summary>
    public class ChapterSelectUI : MonoBehaviour
    {
        [Header("面板设置")]
        [Tooltip("面板背景颜色")]
        public Color panelColor = new Color(0.05f, 0.08f, 0.12f, 0.98f);
        [Tooltip("标题颜色")]
        public Color titleColor = new Color(1f, 0.85f, 0.2f);
        [Tooltip("按钮颜色")]
        public Color buttonColor = new Color(0.15f, 0.5f, 0.25f);
        [Tooltip("按钮悬停颜色")]
        public Color buttonHoverColor = new Color(0.2f, 0.6f, 0.35f);
        [Tooltip("返回按钮颜色")]
        public Color backButtonColor = new Color(0.4f, 0.4f, 0.5f);

        private Canvas parentCanvas;
        private GameObject uiRoot;

        // 章节选择
        private int selectedChapter = 1;
        private TextMeshProUGUI chapterText;

        // 装备选择
        private List<int> selectedEquipSlots = new List<int>();
        private const int MaxEquipSlots = 6; // 头盔、衣服、裤子、鞋子、武器、副武器

        // 宠物选择
        private int selectedPetId = -1;
        private TextMeshProUGUI petText;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("ChapterSelectUI 必须放在 Canvas 下面！");
                return;
            }

            CreateUI();
            uiRoot.SetActive(false);
        }

        private void CreateUI()
        {
            uiRoot = new GameObject("ChapterSelectUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rt = uiRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // 遮罩背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(uiRoot.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = panelColor;
            bgImg.raycastTarget = true;

            // 标题
            CreateText(uiRoot.transform, "Title", "Chapter Select", new Vector2(0, 280),
                new Vector2(400, 60), 36, titleColor, TextAnchor.MiddleCenter, FontStyle.Bold);

            // ==================== 章节选择 ====================
            CreateText(uiRoot.transform, "ChapterLabel", "Chapter", new Vector2(-250, 200),
                new Vector2(150, 30), 22, Color.white, TextAnchor.MiddleLeft, FontStyle.Normal);

            // 章节减少按钮
            CreateButton(uiRoot.transform, "ChapterPrevBtn", "<", new Vector2(-300, 200),
                new Vector2(40, 40), buttonColor, () => { ChangeChapter(-1); });

            // 章节显示
            chapterText = CreateText(uiRoot.transform, "ChapterNum", "Chapter 1", new Vector2(-230, 200),
                new Vector2(120, 40), 28, Color.yellow, TextAnchor.MiddleCenter, FontStyle.Bold);

            // 章节增加按钮
            CreateButton(uiRoot.transform, "ChapterNextBtn", ">", new Vector2(-150, 200),
                new Vector2(40, 40), buttonColor, () => { ChangeChapter(1); });

            // ==================== 装备选择 ====================
            CreateText(uiRoot.transform, "EquipLabel", "Equipment", new Vector2(0, 140),
                new Vector2(200, 30), 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 装备槽位（6个）
            string[] equipNames = { "Head", "Body", "Legs", "Feet", "Weapon", "SubWpn" };
            float[] equipX = { -220, -110, 0, 110, 220, 330 };
            for (int i = 0; i < MaxEquipSlots; i++)
            {
                int slotIndex = i;
                CreateEquipSlot(equipX[i], 80, equipNames[i], slotIndex);
            }

            // ==================== 宠物选择 ====================
            CreateText(uiRoot.transform, "PetLabel", "Pet", new Vector2(0, -20),
                new Vector2(200, 30), 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 宠物显示
            petText = CreateText(uiRoot.transform, "PetName", "None", new Vector2(0, -60),
                new Vector2(200, 30), 20, Color.gray, TextAnchor.MiddleCenter, FontStyle.Normal);

            // 选择宠物按钮
            CreateButton(uiRoot.transform, "SelectPetBtn", "Select Pet", new Vector2(0, -100),
                new Vector2(180, 45), new Color(0.3f, 0.5f, 0.6f), new Color(0.4f, 0.6f, 0.7f), () => { SelectPet(); });

            // ==================== 按钮区域 ====================
            // 开始章节按钮
            CreateButton(uiRoot.transform, "StartChapterBtn", "Start", new Vector2(100, -220),
                new Vector2(200, 60), buttonColor, buttonHoverColor, () => {
                    Debug.Log("[ChapterSelect] 开始章节按钮被点击!");
                    StartChapter();
                });

            // 返回按钮
            CreateButton(uiRoot.transform, "BackBtn", "Back", new Vector2(-100, -220),
                new Vector2(150, 60), backButtonColor, new Color(0.5f, 0.5f, 0.6f), () => { BackToMenu(); });
        }

        private void CreateEquipSlot(float x, float y, string name, int slotIndex)
        {
            GameObject slot = new GameObject($"EquipSlot_{slotIndex}");
            slot.transform.SetParent(uiRoot.transform, false);
            RectTransform rt = slot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(80, 80);

            // 背景
            Image bg = slot.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            bg.raycastTarget = true;

            // 边框
            GameObject border = new GameObject("Border");
            border.transform.SetParent(slot.transform, false);
            RectTransform borderRT = border.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(-2, -2);
            borderRT.offsetMax = new Vector2(2, 2);
            Image borderImg = border.AddComponent<Image>();
            borderImg.color = selectedEquipSlots.Contains(slotIndex) ? Color.green : new Color(0.5f, 0.5f, 0.5f);
            borderImg.raycastTarget = true;

            // 文字
            GameObject label = new GameObject("Label");
            label.transform.SetParent(slot.transform, false);
            RectTransform labelRT = label.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = name;
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;

            // 统一设置字体
            if (UIFontManager.Instance != null)
            {
                UIFontManager.Instance.SetFont(txt);
            }

            // 按钮
            Button btn = slot.AddComponent<Button>();
            btn.onClick.AddListener(() => ToggleEquipSlot(slotIndex, borderImg));

            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.4f);
            btn.colors = colors;
        }

        private void ToggleEquipSlot(int slotIndex, Image border)
        {
            if (selectedEquipSlots.Contains(slotIndex))
            {
                selectedEquipSlots.Remove(slotIndex);
                border.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                selectedEquipSlots.Add(slotIndex);
                border.color = Color.green;
            }
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size, int fontSize, Color color, TextAnchor align, FontStyle style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchorToTMP(align);
            txt.fontStyle = style == FontStyle.Bold ? FontStyles.Bold : FontStyles.Normal;

            // 统一设置字体
            if (UIFontManager.Instance != null)
            {
                UIFontManager.Instance.SetFont(txt);
            }

            return txt;
        }

        private TextAlignmentOptions TextAnchorToTMP(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
        {
            CreateButton(parent, name, label, pos, size, color, color * 1.2f, onClick);
        }

        private void CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color color, Color hoverColor, UnityEngine.Events.UnityAction onClick)
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

            // 确保 Image 的 RaycastTarget 为 true（UI交互必须）
            var img = btn.GetComponent<Image>();
            img.raycastTarget = true;

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btn.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
            labelTxt.text = label;
            labelTxt.fontSize = size.y > 50 ? 22 : 18;
            labelTxt.color = Color.white;
            labelTxt.alignment = TextAlignmentOptions.Center;
            labelTxt.fontStyle = FontStyles.Bold;

            // 统一设置字体
            if (UIFontManager.Instance != null)
            {
                UIFontManager.Instance.SetFont(labelTxt);
            }
        }

        private void ChangeChapter(int delta)
        {
            selectedChapter = Mathf.Clamp(selectedChapter + delta, 1, 10);
            chapterText.text = $"Chapter {selectedChapter}";

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
        }

        private void SelectPet()
        {
            // 简单的宠物选择逻辑：遍历已解锁的宠物
            if (PetSystem.Instance != null)
            {
                var pets = PetSystem.Instance.GetAllPets();
                int currentIndex = selectedPetId == -1 ? -1 : pets.FindIndex(p => p.petId == selectedPetId);

                // 找下一个已解锁的宠物
                for (int i = 1; i <= pets.Count; i++)
                {
                    int nextIndex = (currentIndex + i) % pets.Count;
                    if (pets[nextIndex].isUnlocked)
                    {
                        selectedPetId = pets[nextIndex].petId;
                        petText.text = pets[nextIndex].petName;
                        petText.color = Color.yellow;
                        break;
                    }
                }
            }

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
        }

        private void StartChapter()
        {
            Debug.Log($"[ChapterSelect] StartChapter 被调用! chapter={selectedChapter}, equip={selectedEquipSlots.Count}, pet={selectedPetId}");

            try
            {
                // 保存选择
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.CurrentData.selectedChapter = selectedChapter;
                    SaveSystem.Instance.CurrentData.selectedPetId = selectedPetId;
                    SaveSystem.Instance.CurrentData.selectedEquipSlots.items = selectedEquipSlots.ToArray();
                }

                // 隐藏当前UI
                Hide();

                // 直接启用游戏对象（简化方案）
                var mapGen = GameObject.Find("MapGenerator");
                if (mapGen != null)
                {
                    mapGen.SetActive(true);
                    var mg = mapGen.GetComponent<ClawSurvivor.Map.MapGenerator>();
                    if (mg != null) mg.enabled = true;
                }

                var player = GameObject.Find("Player");
                if (player != null)
                {
                    player.SetActive(true);
                    var pc = player.GetComponent<ClawSurvivor.Player.PlayerController>();
                    if (pc != null) pc.enabled = true;
                }

                var spawner = GameObject.Find("EnemySpawner");
                if (spawner != null)
                {
                    spawner.SetActive(true);
                    var es = spawner.GetComponent<ClawSurvivor.Enemy.EnemySpawner>();
                    if (es != null) es.enabled = true;
                }

                var hud = GameObject.Find("GameHUD");
                if (hud != null)
                {
                    hud.SetActive(true);
                    var hudComp = hud.GetComponent<ClawSurvivor.UI.GameHUD>();
                    if (hudComp != null) hudComp.enabled = true;
                }

                // 如果有章节管理器，启动章节模式
                if (ChapterManager.Instance != null)
                {
                    ChapterManager.Instance.StartChapter(selectedChapter);
                }

                // 恢复游戏时间
                Time.timeScale = 1;

                Debug.Log("[ChapterSelect] 章节已开始");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ChapterSelect] StartChapter异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void BackToMenu()
        {
            Hide();
            var mainMenu = GetComponentInParent<Canvas>().GetComponentInChildren<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.Show();
            }

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
        }

        /// <summary>
        /// 显示章节选择UI
        /// </summary>
        public void Show()
        {
            // 重置选择
            selectedChapter = 1;
            selectedEquipSlots.Clear();
            selectedPetId = -1;

            // 默认选择第一个宠物
            if (PetSystem.Instance != null)
            {
                var pets = PetSystem.Instance.GetAllPets();
                foreach (var pet in pets)
                {
                    if (pet.isUnlocked)
                    {
                        selectedPetId = pet.petId;
                        petText.text = pet.petName;
                        petText.color = Color.yellow;
                        break;
                    }
                }
            }

            chapterText.text = $"Chapter {selectedChapter}";
            if (petText != null && selectedPetId == -1)
            {
                petText.text = "None";
                petText.color = Color.gray;
            }

            uiRoot.SetActive(true);
        }

        /// <summary>
        /// 隐藏章节选择UI
        /// </summary>
        public void Hide()
        {
            uiRoot.SetActive(false);
        }
    }
}
