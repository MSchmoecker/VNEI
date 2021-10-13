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
        public static BaseUI Instance { get; private set; }

        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform dragHandler;
        [SerializeField] public GameObject itemPrefab;
        [SerializeField] public GameObject rowPrefab;
        [SerializeField] public GameObject recipeDroppedTextPrefab;
        [SerializeField] public GameObject arrowPrefab;
        [SerializeField] public DisplayItem[] lastViewedDisplayItems;

        private List<Item> lastViewedItems = new List<Item>();
        private bool blockInput;
        private bool sizeDirty;

        public static void Create() {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("VNEI");
            Instantiate(prefab, GUIManager.CustomGUIFront.transform);
        }

        private void Awake() {
            Instance = this;
            dragHandler.gameObject.AddComponent<DragWindowCntrl>();
            gameObject.AddComponent<ScrollHandler>();
            ShowSearch();

            Styling.ApplyAllComponents(root);
            GUIManager.Instance.ApplyWoodpanelStyle(dragHandler);

            if ((bool)InventoryGui.instance) {
                transform.SetParent(InventoryGui.instance.m_player);
                ((RectTransform)transform).anchoredPosition = new Vector2(665, -45);
            } else {
                root.gameObject.SetActive(false);
                dragHandler.gameObject.SetActive(false);
            }

            RecipeUI.OnSetItem += AddItemToLastViewedQueue;
            Plugin.columnCount.SettingChanged += RebuildSizeEvent;
            Plugin.rowCount.SettingChanged += RebuildSizeEvent;

            RebuildSize();
        }

        private void RebuildSizeEvent(object sender, EventArgs e) => sizeDirty = true;

        private void RebuildSize() {
            root.sizeDelta = new Vector2(Plugin.columnCount.Value * 50f + 20f,
                Plugin.rowCount.Value * 50f + 110f);
            dragHandler.sizeDelta = root.sizeDelta + new Vector2(10f, 10f);
        }

        private void Update() {
            if (SearchUI.Instance.searchField.isFocused && !blockInput) {
                GUIManager.BlockInput(true);
                blockInput = true;
            } else if (!SearchUI.Instance.searchField.isFocused && blockInput) {
                GUIManager.BlockInput(false);
                blockInput = false;
            }

            if (sizeDirty) {
                sizeDirty = false;
                RebuildSize();
            }
        }

        private void LateUpdate() {
            root.anchoredPosition = dragHandler.anchoredPosition;
        }

        private void HideAll() {
            SearchUI.Instance.gameObject.SetActive(false);
            RecipeUI.Instance.gameObject.SetActive(false);
        }

        public void ShowSearch() {
            HideAll();
            SearchUI.Instance.gameObject.SetActive(true);
        }

        public void ShowRecipe() {
            HideAll();
            RecipeUI.Instance.gameObject.SetActive(true);
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
            if (lastViewedItems.Count > lastViewedDisplayItems.Length) {
                lastViewedItems.RemoveAt(lastViewedItems.Count - 1);
            }

            // display items at corresponding slots
            for (int i = 0; i < lastViewedDisplayItems.Length; i++) {
                if (i >= lastViewedItems.Count) {
                    lastViewedDisplayItems[i].SetItem(null, 1);
                    continue;
                }

                lastViewedDisplayItems[i].SetItem(lastViewedItems[i], 1);
            }
        }

        private void OnDestroy() {
            RecipeUI.OnSetItem -= AddItemToLastViewedQueue;
        }
    }
}
