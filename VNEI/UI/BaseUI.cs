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
        public Transform lastViewItemsParent;

        public SearchUI searchUi;
        public RecipeUI recipeUi;

        [Header("Prefabs")] public GameObject itemPrefab;
        public GameObject rowPrefab;
        public GameObject recipeDroppedTextPrefab;
        public GameObject arrowPrefab;

        private List<DisplayItem> lastViewedDisplayItems = new List<DisplayItem>();
        private List<Item> lastViewedItems = new List<Item>();
        private bool blockInput;
        private bool sizeDirty;
        [HideInInspector] public List<TypeToggle> typeToggles = new List<TypeToggle>();
        private bool canBeHidden;

        public int ItemSizeX { get; private set; }
        public int ItemSizeY { get; private set; }
        public event Action RebuildedSize;
        private bool usePluginSize = true;

        public static BaseUI CreateBaseUI(bool canBeHidden = false, bool scaleWithPluginSetting = false, bool draggable = true) {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("VNEI");
            GameObject spawn = Instantiate(prefab, GUIManager.CustomGUIFront.transform);
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
            GUIManager.Instance.ApplyWoodpanelStyle(dragHandler);
            UpdateTransparency(null, EventArgs.Empty);
            Plugin.transparency.SettingChanged += UpdateTransparency;

            if ((bool)InventoryGui.instance) {
                transform.SetParent(InventoryGui.instance.m_player);
                ((RectTransform)transform).anchoredPosition = new Vector2(665, -45);
                UpdateVisibility();
            } else {
                root.gameObject.SetActive(false);
                dragHandler.gameObject.SetActive(false);
            }

            recipeUi.OnSetItem += AddItemToLastViewedQueue;
            Plugin.OnOpenHotkey += UpdateVisibility;

            RebuildSize();
            RebuildLastViewedDisplayItems();
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
                RebuildLastViewedDisplayItems();
            }
        }

        private void RebuildLastViewedDisplayItems() {
            foreach (DisplayItem displayItem in lastViewedDisplayItems) {
                Destroy(displayItem.gameObject);
            }

            lastViewedDisplayItems.Clear();

            if (GetComponent<SelectUI>()) {
                // TODO responsibility in SelectUI?
                return;
            }

            int lastViewCount = Mathf.Max(ItemSizeX - 5, 0);

            RectTransform parentRectTransform = ((RectTransform)lastViewItemsParent.transform);
            parentRectTransform.anchoredPosition = new Vector2((lastViewCount * 50f) / 2f + 5f, -25f);
            parentRectTransform.sizeDelta = new Vector2(lastViewCount * 50f, 50f);

            for (int i = 0; i < lastViewCount; i++) {
                GameObject sprite = Instantiate(itemPrefab, lastViewItemsParent);
                ((RectTransform)sprite.transform).anchoredPosition = new Vector2(25f + i * 50, -25f);
                DisplayItem displayItem = sprite.GetComponent<DisplayItem>();
                displayItem.Init(this);
                lastViewedDisplayItems.Add(displayItem);
            }

            UpdateLastViewDisplayItems();
        }

        private void LateUpdate() {
            root.anchoredPosition = dragHandler.anchoredPosition;
        }

        private void HideAll() {
            searchUi.gameObject.SetActive(false);
            recipeUi.gameObject.SetActive(false);
        }

        public void ShowSearch() {
            HideAll();
            searchUi.gameObject.SetActive(true);
        }

        public void ShowRecipe() {
            HideAll();
            recipeUi.gameObject.SetActive(true);
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
            if (lastViewedItems.Count > 25) {
                lastViewedItems.RemoveAt(lastViewedItems.Count - 1);
            }

            // display items at corresponding slots
            UpdateLastViewDisplayItems();
        }

        private void UpdateLastViewDisplayItems() {
            for (int i = 0; i < lastViewedDisplayItems.Count; i++) {
                if (i >= lastViewedItems.Count) {
                    lastViewedDisplayItems[i].SetItem(null, 1);
                    continue;
                }

                lastViewedDisplayItems[i].SetItem(lastViewedItems[i], 1);
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
            RebuildLastViewedDisplayItems();
        }

        private void UpdateVisibility() {
            if (canBeHidden) {
                root.gameObject.SetActive(Plugin.isUiOpen);
                dragHandler.gameObject.SetActive(Plugin.isUiOpen);
            } else {
                root.gameObject.SetActive(true);
                dragHandler.gameObject.SetActive(true);
            }
        }

        private void UpdateTransparency(object sender, EventArgs args) {
            dragHandler.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f - Plugin.transparency.Value / 100f);
        }
    }
}
