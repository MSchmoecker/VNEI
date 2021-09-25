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

        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform dragHandler;
        [SerializeField] public GameObject itemPrefab;
        [SerializeField] public GameObject rowPrefab;
        [SerializeField] public GameObject recipeDroppedTextPrefab;
        [SerializeField] public GameObject arrowPrefab;
        private bool blockInput;

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
        }

        private void Update() {
            if (SearchUI.Instance.searchField.isFocused && !blockInput) {
                GUIManager.BlockInput(true);
                blockInput = true;
            } else if (!SearchUI.Instance.searchField.isFocused && blockInput) {
                GUIManager.BlockInput(false);
                blockInput = false;
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
    }
}
