using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VNEI.UI {
    public class MainVneiHandler : VneiHandler {
        private static MainVneiHandler instance;
        private BaseUI baseUI;
        private Button vneiTab;
        private readonly List<Button> otherButtons = new List<Button>();

        public static MainVneiHandler Instance => instance ?? (instance = new MainVneiHandler());
        public bool VneiTabActive { get; private set; }

        private MainVneiHandler() {
            Plugin.attachToCrafting.SettingChanged += (sender, e) => {
                GetTabButton(true);
                CreateBaseUI(true);
            };
        }

        public BaseUI GetBaseUI() {
            return CreateBaseUI();
        }

        public Button GetTabButton(bool forceRecreate = false) {
            if (!InventoryGui.instance) {
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
                vneiTab.gameObject.AddComponent<TabButton>();
                vneiTab.transform.SetSiblingIndex(vneiTab.transform.parent.childCount - 2);
                vneiTab.name = "VNEI";
                vneiTab.onClick.RemoveAllListeners();
                vneiTab.onClick.AddListener(SetTabActive);
                SetTabInactive();
            }

            if (!Plugin.AttachToCrafting()) {
                vneiTab.gameObject.SetActive(false);
                SetTabInactive();
            }

            return vneiTab;
        }

        public BaseUI CreateBaseUI(bool forceRecreate = false) {
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
                baseUI = BaseUI.CreateBaseUI(true, !Plugin.AttachToCrafting(), !Plugin.AttachToCrafting());

                if (Plugin.AttachToCrafting()) {
                    RectTransform craftingPanel = (RectTransform)InventoryGui.instance.m_inventoryRoot.Find("Crafting");
                    RectTransform baseUIRect = (RectTransform)baseUI.transform;
                    Vector2 craftingPanelSize = craftingPanel.sizeDelta;

                    baseUIRect.SetParent(craftingPanel);
                    const float topSpace = 160f;
                    baseUIRect.anchoredPosition = new Vector2(0, -topSpace / 2f + 30f);
                    baseUI.SetSize(false, (int)((craftingPanelSize.x - 10f) / 50f), (int)((craftingPanelSize.y - topSpace) / 50f));

                    baseUI.dragHandler.GetComponent<Image>().enabled = false;
                    baseUI.SetVisibility(false);
                } else {
                    VneiTabActive = false;
                }
            }

            return baseUI;
        }

        public void UpdateOtherTabs() {
            otherButtons.RemoveAll(i => !i);

            foreach (Button tab in InventoryGui.instance.m_tabCraft.transform.parent.GetComponentsInChildren<Button>()) {
                if (tab == GetTabButton()) {
                    continue;
                }

                if (otherButtons.Contains(tab)) {
                    continue;
                }

                tab.onClick.AddListener(SetTabInactive);
                otherButtons.Add(tab);
            }
        }

        public void UpdateTabPosition() {
            RectTransform tab = (RectTransform)GetTabButton().transform;

            for (int i = tab.GetSiblingIndex() - 1; i >= 0; i--) {
                if (!tab.parent.GetChild(i).gameObject.activeSelf) {
                    continue;
                }

                Vector2 leftPos = ((RectTransform)tab.parent.GetChild(i)).anchoredPosition;
                tab.anchoredPosition = leftPos + new Vector2(107f, 0);
                break;
            }
        }

        public void SetTabActive() {
            VneiTabActive = true;

            GetTabButton().interactable = false;
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/RecipeList").gameObject.SetActive(false);
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/Decription").gameObject.SetActive(false);
            GetBaseUI().SetVisibility(true);

            InventoryGui.instance.m_tabCraft.interactable = true;
            InventoryGui.instance.m_tabUpgrade.interactable = true;

            UpdateTabPosition();
        }

        public void SetTabInactive() {
            VneiTabActive = false;

            GetTabButton().interactable = true;
            if (Plugin.AttachToCrafting()) {
                GetBaseUI().SetVisibility(false);
            }

            InventoryGui.instance.m_inventoryRoot.Find("Crafting/RecipeList").gameObject.SetActive(true);
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/Decription").gameObject.SetActive(true);
        }
    }
}
