using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ClawSurvivor.Player;

namespace ClawSurvivor.UI
{
    public class SkillBarUI : MonoBehaviour
    {
        [Header("技能栏设置")]
        [Tooltip("技能槽背景颜色")]
        public Color slotColor = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        [Tooltip("冷却遮罩颜色")]
        public Color cooldownColor = new Color(0, 0, 0, 0.6f);
        [Tooltip("技能槽大小")]
        public float slotSize = 70f;
        [Tooltip("技能槽间距")]
        public float slotSpacing = 10f;
        [Tooltip("技能栏距底部偏移")]
        public float barOffsetY = 50f;

        private Canvas parentCanvas;
        private GameObject uiRoot;
        private List<Image> cooldownOverlays = new List<Image>();
        private List<Text> cooldownTexts = new List<Text>();
        private List<Text> skillNameTexts = new List<Text>();
        private PlayerController player;
        private RectTransform barRect;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            player = FindObjectOfType<PlayerController>();
            CreateBarUI();
        }

        private void Update()
        {
            if (player == null) return;
            UpdateCooldowns();
        }

        private void CreateBarUI()
        {
            uiRoot = new GameObject("SkillBarUI");
            uiRoot.transform.SetParent(parentCanvas.transform, false);
            RectTransform rootRT = uiRoot.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;

            // 背景条
            GameObject bar = new GameObject("Bar");
            bar.transform.SetParent(uiRoot.transform, false);
            barRect = bar.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0f);
            barRect.anchorMax = new Vector2(0.5f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = new Vector2(0, barOffsetY);
            float totalWidth = 4 * slotSize + 3 * slotSpacing + 40;
            barRect.sizeDelta = new Vector2(totalWidth, slotSize + 30);
            bar.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.7f);

            for (int i = 0; i < 4; i++)
            {
                float x = -((4 - 1) * (slotSize + slotSpacing)) / 2f + i * (slotSize + slotSpacing);
                CreateSlot(bar.transform, i, x);
            }
        }

        private void CreateSlot(Transform parent, int index, float x)
        {
            // 槽位
            GameObject slot = new GameObject($"Slot_{index}");
            slot.transform.SetParent(parent, false);
            RectTransform slotRT = slot.AddComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.pivot = new Vector2(0.5f, 0.5f);
            slotRT.anchoredPosition = new Vector2(x, 0);
            slotRT.sizeDelta = new Vector2(slotSize, slotSize);
            Image slotImg = slot.AddComponent<Image>();
            slotImg.color = slotColor;

            // 技能名称
            GameObject nameObj = new GameObject("SkillName");
            nameObj.transform.SetParent(slot.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.5f, 1f);
            nameRT.anchorMax = new Vector2(0.5f, 1f);
            nameRT.pivot = new Vector2(0.5f, 1f);
            nameRT.anchoredPosition = new Vector2(0, 2);
            nameRT.sizeDelta = new Vector2(slotSize + 20, 22);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = "空";
            nameText.fontSize = 11;
            nameText.color = new Color(0.6f, 0.6f, 0.6f);
            nameText.alignment = TextAnchor.MiddleCenter;
            skillNameTexts.Add(nameText);

            // 冷却遮罩
            GameObject cdOverlay = new GameObject("CooldownOverlay");
            cdOverlay.transform.SetParent(slot.transform, false);
            RectTransform cdRT = cdOverlay.AddComponent<RectTransform>();
            cdRT.anchorMin = new Vector2(0, 0);
            cdRT.anchorMax = new Vector2(1, 1);
            cdRT.pivot = new Vector2(0.5f, 0.5f);
            cdRT.sizeDelta = Vector2.zero;
            Image cdImg = cdOverlay.AddComponent<Image>();
            cdImg.color = cooldownColor;
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Vertical;
            cdImg.fillOrigin = 2;
            cdOverlay.SetActive(false);
            cooldownOverlays.Add(cdImg);

            // 冷却时间文字
            GameObject cdText = new GameObject("CDText");
            cdText.transform.SetParent(slot.transform, false);
            RectTransform cdTextRT = cdText.AddComponent<RectTransform>();
            cdTextRT.anchorMin = Vector2.zero;
            cdTextRT.anchorMax = Vector2.one;
            cdTextRT.sizeDelta = Vector2.zero;
            Text cdTmp = cdText.AddComponent<Text>();
            cdTmp.fontSize = 18;
            cdTmp.color = Color.white;
            cdTmp.alignment = TextAnchor.MiddleCenter;
            cdTmp.fontStyle = FontStyle.Bold;
            cdText.SetActive(false);
            cooldownTexts.Add(cdTmp);
        }

        private void UpdateCooldowns()
        {
            var skills = player.GetEquippedSkills();
            var timers = player.GetCooldownTimers();

            for (int i = 0; i < 4; i++)
            {
                if (i < skills.Count)
                {
                    skillNameTexts[i].text = skills[i].skillName;
                    skillNameTexts[i].color = skills[i].cooldown <= 0 ? Color.green : Color.white;

                    if (skills[i].cooldown > 0)
                    {
                        float cd = skills[i].cooldown;
                        float timer = i < timers.Count ? timers[i] : 0f;
                        float remaining = cd - timer;

                        if (remaining > 0.1f)
                        {
                            cooldownOverlays[i].gameObject.SetActive(true);
                            cooldownOverlays[i].fillAmount = remaining / cd;
                            cooldownTexts[i].gameObject.SetActive(true);
                            cooldownTexts[i].text = remaining.ToString("F1");
                        }
                        else
                        {
                            cooldownOverlays[i].gameObject.SetActive(false);
                            cooldownTexts[i].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        cooldownOverlays[i].gameObject.SetActive(false);
                        cooldownTexts[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    skillNameTexts[i].text = "空";
                    skillNameTexts[i].color = new Color(0.6f, 0.6f, 0.6f);
                    cooldownOverlays[i].gameObject.SetActive(false);
                    cooldownTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
