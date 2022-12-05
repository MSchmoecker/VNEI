using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.GUI;
using UnityEngine;
using Jotunn.Managers;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    [DefaultExecutionOrder(5)]
    public class BaseUI : MonoBehaviour {
        [Header("Local References")] public RectTransform root;
        public RectTransform dragHandler;
        public RectTransform lastViewItemsParent;
        public RectTransform favoriteItemsParent;

        public SearchUI searchUi;
        public RecipeUI recipeUi;

        [Header("Prefabs")] public GameObject itemPrefab;
        public GameObject rowPrefab;
        public GameObject recipeDroppedTextPrefab;
        public GameObject arrowPrefab;

        private List<DisplayItem> lastViewedDisplayItems = new List<DisplayItem>();
        private List<DisplayItem> favoriteItemsDisplayItems = new List<DisplayItem>();

        private List<Item> lastViewedItems = new List<Item>();
        // private List<Item> favoriteItems = new List<Item>();

        private bool blockInput;
        private bool sizeDirty;
        [HideInInspector] public List<ItemTypeToggle> typeToggles = new List<ItemTypeToggle>();
        [HideInInspector] public FavoriteTypeToggle favoriteToggle;
        public Action typeToggleChange;
        private bool canBeHidden;

        public int ItemSizeX { get; private set; }
        public int ItemSizeY { get; private set; }
        public event Action RebuildedSize;
        private bool usePluginSize = true;

        public static BaseUI CreateBaseUI(bool canBeHidden = false, bool scaleWithPluginSetting = false, bool draggable = true) {
            GameObject spawn = Instantiate(Plugin.Instance.vneiUI, GUIManager.CustomGUIFront ? GUIManager.CustomGUIFront.transform : null);
            BaseUI baseUI = spawn.GetComponent<BaseUI>();
            baseUI.canBeHidden = canBeHidden;
            baseUI.usePluginSize = scaleWithPluginSetting;

            if (baseUI.usePluginSize) {
                Plugin.columnCount.SettingChanged += baseUI.RebuildSizeEvent;
                Plugin.rowCount.SettingChanged += baseUI.RebuildSizeEvent;
            }

            if (draggable) {
                baseUI.dragHandler.gameObject.AddComponent<DragWindowCntrl>();
            }

            return baseUI;
        }

        private void Awake() {
            ShowSearch();

            Styling.ApplyAllComponents(root);

            foreach (Text text in root.GetComponentsInChildren<Text>(true)) {
                text.text = Localization.instance.Localize(text.text);
            }

            GUIManager.Instance.ApplyWoodpanelStyle(dragHandler);
            UpdateTransparency(null, EventArgs.Empty);
            Plugin.transparency.SettingChanged += UpdateTransparency;

            if ((bool)InventoryGui.instance) {
                transform.SetParent(InventoryGui.instance.m_player);
                ((RectTransform)transform).anchoredPosition = new Vector2(665, -45);
                UpdateVisibility();
            } else {
                SetVisibility(false);
            }

            recipeUi.OnSetItem += AddItemToLastViewedQueue;
            Plugin.OnOpenHotkey += UpdateVisibility;

            RebuildSize();
            RebuildDisplayItemRows();
        }

        private void RebuildSizeEvent(object sender, EventArgs e) => sizeDirty = true;

        private void RebuildSize() {
            if (usePluginSize) {
                ItemSizeX = Plugin.columnCount.Value;
                ItemSizeY = Plugin.rowCount.Value;
            }

            root.sizeDelta = new Vector2(ItemSizeX * 50f + 10f, ItemSizeY * 50f + 100f);
            dragHandler.sizeDelta = root.sizeDelta + new Vector2(10f, 10f);

            RebuildedSize?.Invoke();
        }

        private void Update() {
            if (searchUi.searchField.isFocused && !blockInput) {
                GUIManager.BlockInput(true);
                blockInput = true;
            } else if (!searchUi.searchField.isFocused && blockInput) {
                GUIManager.BlockInput(false);
                blockInput = false;
            }

            if (sizeDirty) {
                sizeDirty = false;
                RebuildSize();
                RebuildDisplayItemRows();
            }
        }

        private void RebuildDisplayItemRows() {
            RebuildDisplayItemRow(lastViewedDisplayItems, lastViewItemsParent);
            UpdateDisplayItemRow(lastViewedDisplayItems, lastViewedItems);

            // RebuildDisplayItemRow(favoriteItemsDisplayItems, favoriteItemsParent);
            // UpdateDisplayItemRow(favoriteItemsDisplayItems, favoriteItems);
        }

        private void RebuildDisplayItemRow(List<DisplayItem> displayItems, RectTransform parent) {
            foreach (DisplayItem displayItem in displayItems) {
                Destroy(displayItem.gameObject);
            }

            displayItems.Clear();

            if (GetComponent<SelectUI>()) {
                // TODO responsibility in SelectUI?
                return;
            }

            int displayItemCount = Mathf.Max(ItemSizeX - 5, 0);

            parent.anchoredPosition = new Vector2((displayItemCount * 50f) / 2f + 5f, parent.anchoredPosition.y);
            parent.sizeDelta = new Vector2(displayItemCount * 50f, 50f);

            for (int i = 0; i < displayItemCount; i++) {
                DisplayItem displayItem = SpawnDisplayItem(new Vector2(25f + i * 50, -25f), parent);
                displayItems.Add(displayItem);
            }
        }

        private DisplayItem SpawnDisplayItem(Vector2 anchoredPosition, Transform parent) {
            GameObject sprite = Instantiate(itemPrefab, parent);
            ((RectTransform)sprite.transform).anchoredPosition = anchoredPosition;
            DisplayItem displayItem = sprite.GetComponent<DisplayItem>();
            displayItem.Init(this);
            return displayItem;
        }

        private void LateUpdate() {
            root.anchoredPosition = dragHandler.anchoredPosition;
        }

        public void ShowSearch() {
            recipeUi.gameObject.SetActive(false);
            searchUi.gameObject.SetActive(true);
        }

        public void ShowRecipe() {
            recipeUi.gameObject.SetActive(true);
            searchUi.gameObject.SetActive(false);
        }

        private void AddItemToLastViewedQueue(Item item) {
            // add new item at start
            lastViewedItems.Insert(0, item);

            // remove duplicate items
            for (int i = 1; i < lastViewedItems.Count; i++) {
                if (lastViewedItems[i] == item) {
                    lastViewedItems.RemoveAt(i);
                }
            }

            // remove overflowing items
            if (lastViewedItems.Count > 50) {
                lastViewedItems.RemoveAt(lastViewedItems.Count - 1);
            }

            // display items at corresponding slots
            UpdateDisplayItemRow(lastViewedDisplayItems, lastViewedItems);
        }

        public void RemoveItemFromLastViewedQueue(Item item) {
            lastViewedItems.Remove(item);
            UpdateDisplayItemRow(lastViewedDisplayItems, lastViewedItems);
        }

        private static void UpdateDisplayItemRow(List<DisplayItem> displayItems, List<Item> items) {
            for (int i = 0; i < displayItems.Count; i++) {
                if (i >= items.Count) {
                    displayItems[i].SetItem(null, 1);
                    continue;
                }

                displayItems[i].SetItem(items[i], 1);
            }
        }

        private void OnDestroy() {
            recipeUi.OnSetItem -= AddItemToLastViewedQueue;
            Plugin.OnOpenHotkey -= UpdateVisibility;
            Plugin.transparency.SettingChanged -= UpdateTransparency;
        }

        public void SetSize(bool usePluginSize, int itemsX, int itemsY) {
            this.usePluginSize = usePluginSize;
            ItemSizeX = itemsX;
            ItemSizeY = itemsY;

            // don't use `sizeDirty = true` as it needs one frame to execute
            RebuildSize();
            RebuildDisplayItemRows();
        }

        public void SetVisibility(bool visible) {
            root.gameObject.SetActive(visible);
            dragHandler.gameObject.SetActive(visible);
        }

        private void UpdateVisibility() {
            SetVisibility(!canBeHidden || Plugin.isUiOpen);
        }

        private void UpdateTransparency(object sender, EventArgs args) {
            dragHandler.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f - Plugin.transparency.Value / 100f);
        }
    }
}
