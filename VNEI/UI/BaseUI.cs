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

        public SearchUI searchUi;
        public RecipeUI recipeUi;

        [Header("Prefabs")] public GameObject itemPrefab;
        public GameObject rowPrefab;
        public GameObject recipeDroppedTextPrefab;
        public GameObject arrowPrefab;

        private List<DisplayItem> lastViewedDisplayItems = new List<DisplayItem>();
        private List<Item> lastViewedItems = new List<Item>();

        public const string HistoryQueueKey = "VNEI_History";

        public bool BlockInput { get; private set; }
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
            BlockInput = false;
            ShowSearch(false);
            ShowSearch(true);

            Styling.ApplyAllComponents(root);
            GUIManager.Instance.ApplyWoodpanelStyle(dragHandler);

            recipeUi.OnSetItem += AddItemToLastViewedQueue;
            Plugin.OnOpenHotkey += UpdateVisibility;
            Plugin.transparency.SettingChanged += UpdateTransparency;
            Plugin.showRecentItems.SettingChanged += UpdateLastViewItemsParent;

            if ((bool)InventoryGui.instance) {
                transform.SetParent(InventoryGui.instance.m_player);
                ((RectTransform)transform).anchoredPosition = new Vector2(665, -45);
                UpdateVisibility();
            } else {
                SetVisibility(false);
            }

            UpdateTransparency(null, EventArgs.Empty);
            UpdateLastViewItemsParent(null, EventArgs.Empty);
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
            if (searchUi.searchField.isFocused && !BlockInput) {
                GUIManager.BlockInput(true);
                BlockInput = true;
            } else if (!searchUi.searchField.isFocused && BlockInput) {
                GUIManager.BlockInput(false);
                BlockInput = false;
            }

            if (sizeDirty) {
                sizeDirty = false;
                RebuildSize();
                RebuildDisplayItemRows();
            }

            if (Plugin.goForwardHotkey.Value.IsKeyDown()) {
                NextView();
            } else if (Plugin.goBackHotkey.Value.IsKeyDown()) {
                PreviousView();
            }
        }

        public void PreviousView() {
            UndoManager.Instance.Undo(HistoryQueueKey);
        }

        public void NextView() {
            if (UndoManager.Instance.GetQueue(HistoryQueueKey).GetIndex() == -1) {
                UndoManager.Instance.Redo(HistoryQueueKey);
            }

            UndoManager.Instance.Redo(HistoryQueueKey);
        }

        private void RebuildDisplayItemRows() {
            RebuildDisplayItemRow(lastViewedDisplayItems, lastViewItemsParent);
            UpdateDisplayItemRow(lastViewedDisplayItems, lastViewedItems);
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

        public void ShowSearch(bool trackHistory) {
            SwitchView(() => {
                recipeUi.gameObject.SetActive(false);
                searchUi.gameObject.SetActive(true);
            }, trackHistory);
        }

        public void ShowRecipe(Item item, bool trackHistory) {
            SwitchView(() => {
                recipeUi.SetItem(item);
                recipeUi.gameObject.SetActive(true);
                searchUi.gameObject.SetActive(false);
            }, trackHistory && item != recipeUi.GetItem());
        }

        private void SwitchView(Action changeView, bool trackHistory) {
            if (trackHistory) {
                HistorySnapshot previous = GetCurrentView();

                changeView?.Invoke();

                HistorySnapshot next = GetCurrentView();
                UndoManager.Instance.Add(HistoryQueueKey, new HistoryElement(previous, next));
            } else {
                changeView?.Invoke();
            }
        }

        private HistorySnapshot GetCurrentView() {
            if (recipeUi.gameObject.activeSelf) {
                return new HistorySnapshotRecipe(this, recipeUi.GetItem());
            }

            return new HistorySnapshotSearch(this);
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
            Plugin.showRecentItems.SettingChanged -= UpdateLastViewItemsParent;
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

        private void UpdateLastViewItemsParent(object sender, EventArgs args) {
            lastViewItemsParent.gameObject.SetActive(Plugin.showRecentItems.Value);
        }
    }
}
