using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VNEI.UI {
    public class MainVneiHandler {
        private static MainVneiHandler instance;
        private BaseUI baseUI;
        private Button vneiTab;
        private bool vneiTabActive;
        private readonly List<Button> otherButtons = new List<Button>();

        public static MainVneiHandler Instance => instance ?? (instance = new MainVneiHandler());
        public bool VneiTabActive => vneiTabActive;

        private MainVneiHandler() {
            Plugin.attachToCrafting.SettingChanged += (sender, e) => {
                GetOrCreateVneiTabButton(true);
                GetOrCreateBaseUI(true);
            };
        }

        public Button GetOrCreateVneiTabButton(bool forceRecreate = false) {
            if (!InventoryGui.instance) {
                return null;
            }

            if (Plugin.Instance.IsAugaPresent()) {
                return null;
            }

            if (vneiTab) {
                if (forceRecreate) {
                    Object.Destroy(vneiTab.gameObject);
                    vneiTab = null;
                } else {
                    return vneiTab;
                }
            }

            if (!vneiTab) {
                Button upgradeTab = InventoryGui.instance.m_tabUpgrade;
                vneiTab = Object.Instantiate(upgradeTab, upgradeTab.transform.parent);
                vneiTab.gameObject.SetActive(true);
                vneiTab.transform.SetSiblingIndex(vneiTab.transform.parent.childCount - 2);
                vneiTab.name = "VNEI";
                vneiTab.transform.Find("Text").GetComponent<Text>().text = "VNEI";
                vneiTab.onClick.RemoveAllListeners();
                vneiTab.onClick.AddListener(SetVneiTabActive);
                SetVneiTabNotActive();
            }

            if (!Plugin.Instance.AttachToCrafting()) {
                vneiTab.gameObject.SetActive(false);
                SetVneiTabNotActive();
            }

            return vneiTab;
        }

        public BaseUI GetOrCreateBaseUI(bool forceRecreate = false) {
            if (!InventoryGui.instance) {
                return null;
            }

            if (baseUI) {
                if (forceRecreate) {
                    Object.Destroy(baseUI.gameObject);
                    baseUI = null;
                } else {
                    return baseUI;
                }
            }

            if (!baseUI) {
                baseUI = BaseUI.CreateBaseUI(true, !Plugin.Instance.AttachToCrafting(), !Plugin.Instance.AttachToCrafting());

                if (Plugin.Instance.AttachToCrafting()) {
                    RectTransform craftingPanel = (RectTransform)InventoryGui.instance.m_inventoryRoot.Find("Crafting");
                    RectTransform baseUIRect = (RectTransform)baseUI.transform;
                    Vector2 craftingPanelSize = craftingPanel.sizeDelta;

                    baseUIRect.SetParent(craftingPanel);
                    const float topSpace = 160f;
                    baseUIRect.anchoredPosition = new Vector2(0, -topSpace / 2f + 30f);
                    baseUI.SetSize(false, (int)((craftingPanelSize.x - 10f) / 50f), (int)((craftingPanelSize.y - topSpace) / 50f));

                    baseUI.dragHandler.GetComponent<Image>().enabled = false;
                } else {
                    vneiTabActive = false;
                }
            }

            return baseUI;
        }

        public void UpdateOtherTabs() {
            otherButtons.RemoveAll(i => i == null);

            foreach (Button tab in InventoryGui.instance.m_tabCraft.transform.parent.GetComponentsInChildren<Button>()) {
                if (tab == GetOrCreateVneiTabButton()) {
                    continue;
                }

                if (otherButtons.Contains(tab)) {
                    continue;
                }

                tab.onClick.AddListener(SetVneiTabNotActive);
                otherButtons.Add(tab);
            }
        }

        public void UpdateTabPosition() {
            RectTransform tab = (RectTransform)GetOrCreateVneiTabButton().transform;

            for (int i = tab.GetSiblingIndex() - 1; i >= 0; i--) {
                if (!tab.parent.GetChild(i).gameObject.activeSelf) {
                    continue;
                }

                Vector2 leftPos = ((RectTransform)tab.parent.GetChild(i)).anchoredPosition;
                tab.anchoredPosition = leftPos + new Vector2(107f, 0);
                break;
            }
        }

        public void SetVneiTabActive() {
            vneiTabActive = true;

            GetOrCreateVneiTabButton().interactable = false;
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/RecipeList").gameObject.SetActive(false);
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/Decription").gameObject.SetActive(false);
            GetOrCreateBaseUI().SetVisibility(true);

            foreach (Button button in otherButtons) {
                button.interactable = true;
            }

            UpdateTabPosition();
        }

        public void SetVneiTabNotActive() {
            vneiTabActive = false;

            GetOrCreateVneiTabButton().interactable = true;
            GetOrCreateBaseUI().SetVisibility(false);
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/RecipeList").gameObject.SetActive(true);
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/Decription").gameObject.SetActive(true);
        }
    }
}
