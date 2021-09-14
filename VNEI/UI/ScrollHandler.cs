using UnityEngine;
using UnityEngine.EventSystems;

namespace VNEI.UI {
    public class ScrollHandler : MonoBehaviour, IScrollHandler {
        public void OnScroll(PointerEventData eventData) {
            if (!SearchUI.Instance.gameObject.activeSelf) {
                return;
            }

            int scroll = -Mathf.RoundToInt(eventData.scrollDelta.y);
            scroll *= Plugin.invertScroll.Value ? -1 : 1;

            if (scroll != 0) {
                SearchUI.Instance.SwitchPage(scroll);
            }
        }
    }
}
