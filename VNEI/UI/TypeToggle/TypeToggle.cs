using UnityEngine;
using UnityEngine.UI;

namespace VNEI.UI {
    public class TypeToggle : MonoBehaviour {
        public Image image;
        protected BaseUI baseUI;
        [SerializeField] private bool isOn = true;

        public bool IsOn {
            get => isOn;
            set {
                isOn = value;
                UpdateToggle();
            }
        }

        public void UpdateToggle() {
            image.color = isOn ? Color.white : new Color(0, 0, 0, 0.5f);
        }
    }
}
