using Auga;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace VNEI.UI {
    public class MainVneiHandlerAuga {
        private BaseUI baseUI;

        private static MainVneiHandlerAuga instance;
        private static PlayerPanelTabData panelData;

        public static MainVneiHandlerAuga Instance => instance ?? (instance = new MainVneiHandlerAuga());

        public MainVneiHandlerAuga() {
            Plugin.attachToCrafting.SettingChanged += (sender, e) => CreateWindow(true);
        }

        public void CreateWindow(bool forceRecreate = false) {
            if (!InventoryGui.instance) {
                return;
            }

            if (baseUI && forceRecreate) {
                Object.Destroy(baseUI.gameObject);
                baseUI = null;
            }

            if (!baseUI) {
                baseUI = BaseUI.CreateBaseUI(true, !Plugin.AttachToCrafting(), !Plugin.AttachToCrafting());
            }

            if (!API.PlayerPanel_HasTab("VNEI tab")) {
                panelData = API.PlayerPanel_AddTab("VNEI tab", null, "VNEI", i => { });
            }

            if (Plugin.AttachToCrafting()) {
                RectTransform craftingPanel = (RectTransform)panelData.ContentGO.transform;
                RectTransform baseUIRect = (RectTransform)baseUI.transform;
                Vector2 craftingPanelSize = craftingPanel.sizeDelta;

                baseUIRect.SetParent(craftingPanel);
                const float topSpace = 100f;
                baseUIRect.anchoredPosition = new Vector2(0, -topSpace / 2f + 40f);
                baseUI.SetSize(false, (int)((craftingPanelSize.x - 10f) / 50f), (int)((craftingPanelSize.y - topSpace) / 50f));

                baseUI.dragHandler.GetComponent<Image>().enabled = false;
                baseUI.SetVisibility(true);
            }
        }
    }
}
