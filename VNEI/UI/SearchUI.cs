using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class SearchUI : MonoBehaviour {
        public static SearchUI Instance { get; private set; }
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] public InputField searchField;
        [SerializeField] public Toggle cheat;

        [SerializeField] public TypeToggle showUndefined;
        [SerializeField] public TypeToggle showCreatures;
        [SerializeField] public TypeToggle showPieces;
        [SerializeField] public TypeToggle showItems;

        private List<MouseHover> sprites = new List<MouseHover>();
        private bool hasInit;
        private const int ItemsInRow = 11;
        private Vector2 itemSpacing = new Vector2(50f, 50f);
        private Action typeToggleOnChange;

        public void Awake() {
            Instance = this;
            hasInit = false;
            typeToggleOnChange = () => UpdateSearch(true);
            TypeToggle.OnChange += typeToggleOnChange;
        }

        private void Update() {
            if (!hasInit && Player.m_localPlayer != null) {
                Log.LogInfo("Init Search UI");
                Init();
                hasInit = true;
            }

            cheat.gameObject.SetActive(Player.m_localPlayer != null && Console.instance.IsCheatsEnabled());
        }

        public void Init() {
            foreach (KeyValuePair<int, Item> item in Indexing.Items) {
                GameObject sprite = Instantiate(BaseUI.Instance.itemPrefab, scrollRect.content);
                sprite.GetComponentInChildren<MouseHover>().SetItem(item.Value);
                sprites.Add(sprite.GetComponent<MouseHover>());
                sprite.GetComponent<Image>().sprite = item.Value.GetIcon();
            }

            scrollRect.onValueChanged.AddListener((_) => UpdateSearch(false));
            searchField.onValueChanged.AddListener((_) => UpdateSearch(true));

            showUndefined.image.sprite = RecipeUI.Instance.noSprite;
            showCreatures.image.sprite = GUIManager.Instance.GetSprite("texts_button");
            showPieces.image.sprite = GUIManager.Instance.GetSprite("mapicon_bed");
            showItems.image.sprite = GUIManager.Instance.GetSprite("mapicon_trader");

            UpdateSearch(true);
        }

        public void UpdateSearch(bool recalculateLayout) {
            BaseUI.Instance.ShowSearch();
            bool useBlacklist = Plugin.useBlacklist.Value;
            Rect rect = ((RectTransform)scrollRect.transform).rect;
            Vector2 scrollPos = scrollRect.content.anchoredPosition;
            int activeItemCount = 0;

            foreach (MouseHover mouseHover in sprites) {
                RectTransform rectTransform = (RectTransform)mouseHover.transform;

                if (recalculateLayout) {
                    mouseHover.isActive = CalculateActive(mouseHover, useBlacklist);

                    if (mouseHover.isActive) {
                        rectTransform.anchorMin = new Vector2(0f, 1f);
                        rectTransform.anchorMax = new Vector2(0f, 1f);
                        int row = activeItemCount % ItemsInRow;
                        int column = activeItemCount / ItemsInRow;
                        rectTransform.anchoredPosition = new Vector2(row + 0.5f, -column - 0.5f) * itemSpacing;

                        activeItemCount++;
                    }
                }

                float posY = rectTransform.anchoredPosition.y;
                bool invisible = posY > -scrollPos.y + 40 || posY < -scrollPos.y - rect.height - 40;

                mouseHover.gameObject.SetActive(mouseHover.isActive && !invisible);
            }

            if (recalculateLayout) {
                int rowCount = Mathf.CeilToInt( (float) activeItemCount / ItemsInRow);
                scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, rowCount * itemSpacing.y);
            }
        }

        private bool CalculateActive(MouseHover mouseHover, bool useBlacklist) {
            Item item = mouseHover.item;
            bool onBlackList = useBlacklist && item.isOnBlacklist;

            if (onBlackList) {
                return false;
            }

            if (item.itemType == ItemType.Undefined && !showUndefined.isOn) {
                return false;
            }

            if (item.itemType == ItemType.Creature && !showCreatures.isOn) {
                return false;
            }

            if (item.itemType == ItemType.Piece && !showPieces.isOn) {
                return false;
            }

            if (item.itemType == ItemType.Item && !showItems.isOn) {
                return false;
            }

            bool isSearched = item.localizedName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                              item.internalName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isSearched) {
                return false;
            }

            return true;
        }

        public bool IsCheating() {
            return cheat.gameObject.activeSelf && cheat.isOn;
        }

        private void OnDestroy() {
            TypeToggle.OnChange -= typeToggleOnChange;
        }
    }
}
