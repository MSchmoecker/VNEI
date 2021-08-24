using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VNEI.UI {
    public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private Transform textBase;
        [SerializeField] private Text text;

        public void SetText(string value) {
            text.text = value;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            textBase.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData) {
            textBase.gameObject.SetActive(false);
        }
    }
}
