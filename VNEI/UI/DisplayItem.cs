using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class DisplayItem : MonoBehaviour, IPointerClickHandler {
        private BaseUI baseUI;

        public Image image;
        public Image background;
        public UITooltip uiTooltip;
        public Text countText;
        public Text qualityText;
        private Item item;

        private void Awake() {
            Transform tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip").transform;
            // fixed InventoryTooltip doesn't clamp inside screen, because the first child knows the size of it, not the base prefab
            uiTooltip.m_tooltipPrefab = tooltipPrefab.childCount == 1 ? tooltipPrefab.GetChild(0).gameObject : tooltipPrefab.gameObject;

            uiTooltip.m_gamepadFocusObject = PrefabManager.Instance.GetPrefab("selected");
            background.sprite = GUIManager.Instance.GetSprite("item_background");
            Styling.ApplyText(countText, GUIManager.Instance.AveriaSerif, Color.white, 12);
            qualityText.font = GUIManager.Instance.AveriaSerifBold;
        }

        public void Update() {
            uiTooltip.m_showTimer = 1;
        }

        public void Init(BaseUI baseUI) {
            this.baseUI = baseUI;
        }

        public void SetItem(Item target, int quality) {
            item = target;
            image.gameObject.SetActive(item != null);

            if (item != null) {
                image.sprite = item.GetIcon();
                uiTooltip.Set(item.GetPrimaryName(), item.GetTooltip(quality));
                qualityText.text = item.maxQuality > 1 || quality > 1 ? $"{quality}" : "";
            } else {
                uiTooltip.Set("", "");
                qualityText.text = "";
            }
        }

        public static bool IsPlayerCheating() {
            return Player.m_localPlayer != null && Terminal.m_cheat;
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (item == null) {
                return;
            }

            if (baseUI.TryGetComponent(out SelectUI selectUI)) {
                selectUI.SelectItem(item);
                return;
            }

            if (IsPlayerCheating() && eventData.button == PointerEventData.InputButton.Right) {
                if (item.gameObject.GetComponent<ItemDrop>()) {
                    ItemDrop itemDrop = item.gameObject.GetComponent<ItemDrop>();
                    bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    int stackSize = isShiftKeyDown ? itemDrop.m_itemData.m_shared.m_maxStackSize : 1;
                    Player.m_localPlayer.PickupPrefab(item.gameObject, stackSize);
                } else {
                    Transform playerTransform = Player.m_localPlayer.transform;
                    Instantiate(item.gameObject, playerTransform.position + playerTransform.forward * 2f, Quaternion.identity);
                }
            } else {
                baseUI.recipeUi.SetItem(item);
                baseUI.ShowRecipe();
            }
        }

        public void SetCount(string count) {
            countText.text = count;
            countText.gameObject.SetActive(true);
        }
    }
}
