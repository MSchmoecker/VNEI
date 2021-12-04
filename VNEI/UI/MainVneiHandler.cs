using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VNEI.UI {
    public class MainVneiHandler {
        private static MainVneiHandler instance;
        private BaseUI baseUI;
        private Button vneiTab;
        private bool vneiTabActive;

        public static MainVneiHandler Instance => instance ?? (instance = new MainVneiHandler());

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

            if (vneiTab && forceRecreate) {
                Object.Destroy(vneiTab.gameObject);
                vneiTab = null;
            }

            if (!vneiTab) {
                Button upgradeTab = InventoryGui.instance.m_tabUpgrade;
                vneiTab = Object.Instantiate(upgradeTab, upgradeTab.transform.parent);
                vneiTab.gameObject.SetActive(true);
                vneiTab.transform.SetSiblingIndex(upgradeTab.transform.GetSiblingIndex() + 1);
                vneiTab.name = "VNEI";
                vneiTab.transform.Find("Text").GetComponent<Text>().text = "VNEI";
                vneiTab.onClick.RemoveAllListeners();
                vneiTab.onClick.AddListener(() => {
                    vneiTabActive = true;
                    UpdateInventoryTab();
                });
                UpdateInventoryTab();
            }

            if (!Plugin.Instance.AttachToCrafting()) {
                vneiTab.gameObject.SetActive(false);
            }

            return vneiTab;
        }

        public BaseUI GetOrCreateBaseUI(bool forceRecreate = false) {
            if (!InventoryGui.instance) {
                return null;
            }

            if (baseUI && forceRecreate) {
                Object.Destroy(baseUI.gameObject);
                baseUI = null;
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

        public void UpdateInventoryTab() {
            if (!Plugin.Instance.AttachToCrafting()) {
                if (Plugin.Instance.IsAugaPresent()) {
                    return;
                }

                InventoryGui.instance.m_inventoryRoot.Find("Crafting/RecipeList").gameObject.SetActive(true);
                InventoryGui.instance.m_inventoryRoot.Find("Crafting/Decription").gameObject.SetActive(true);
                return;
            }

            RectTransform vneiTabRect = (RectTransform)GetOrCreateVneiTabButton().transform;
            RectTransform lastActiveTab;

            if (InventoryGui.instance.m_tabUpgrade.gameObject.activeSelf) {
                lastActiveTab = (RectTransform)InventoryGui.instance.m_tabUpgrade.transform;
            } else {
                lastActiveTab = (RectTransform)InventoryGui.instance.m_tabCraft.transform;
            }

            vneiTabRect.anchoredPosition = lastActiveTab.anchoredPosition + new Vector2(107f, 0);

            if (vneiTabActive) {
                InventoryGui.instance.m_tabCraft.interactable = true;
                InventoryGui.instance.m_tabUpgrade.interactable = true;
                GetOrCreateVneiTabButton().interactable = false;
            } else {
                GetOrCreateVneiTabButton().interactable = true;
            }

            InventoryGui.instance.m_inventoryRoot.Find("Crafting/RecipeList").gameObject.SetActive(!vneiTabActive);
            InventoryGui.instance.m_inventoryRoot.Find("Crafting/Decription").gameObject.SetActive(!vneiTabActive);
            GetOrCreateBaseUI().SetVisibility(vneiTabActive);
        }

        public void SetVneiTabNotActive() {
            vneiTabActive = false;
        }
    }
}
