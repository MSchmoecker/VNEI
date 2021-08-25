using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] public GameObject itemPrefab;
        [SerializeField] public GameObject rowPrefab;
        [SerializeField] public GameObject arrowPrefab;
        private bool blockInput;

        public static void Create() {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("VNEI");
            Instantiate(prefab, GUIManager.CustomGUIFront.transform);
        }

        private void Awake() {
            Instance = this;
            ShowSearch();
            root.gameObject.SetActive(false);

            Styling.ApplyAllComponents(root);
            Styling.ApplyWoodpanel(root.GetComponent<Image>());
            Styling.ApplyLocalization(root);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F3)) {
                root.gameObject.SetActive(!root.gameObject.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.F4)) {
                ShowSearch();
            }

            if (SearchUI.Instance.searchField.isFocused && !blockInput) {
                GUIManager.BlockInput(true);
                blockInput = true;
            } else if (!SearchUI.Instance.searchField.isFocused && blockInput) {
                GUIManager.BlockInput(false);
                blockInput = false;
            }
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
