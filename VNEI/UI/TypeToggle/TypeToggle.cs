using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public abstract class TypeToggle : MonoBehaviour {
        public Image image;
        public Image background;
        protected BaseUI baseUI;
        protected UITooltip tooltip;
        [SerializeField] private bool isOn = true;
        private bool isEnabled = true;

        protected virtual void Awake() {
            baseUI = GetComponentInParent<BaseUI>();
            tooltip = GetComponent<UITooltip>();
            background = transform.Find("Background").GetComponent<Image>();

            UpdateToggle();

            if (Indexing.HasIndexed()) {
                UpdateItemCount();
            } else {
                Indexing.IndexFinished += UpdateItemCount;
            }
        }

        protected virtual void OnDestroy() {
            Indexing.IndexFinished -= UpdateItemCount;
        }

        public bool IsOn {
            get => isOn;
            set {
                isOn = value;
                UpdateToggle();
            }
        }

        public bool IsEnabled {
            get => isEnabled;
            set {
                isEnabled = value;
                UpdateToggle();
            }
        }

        public void UpdateToggle() {
            image.color = IsOn && IsEnabled ? Color.white : new Color(0, 0, 0, 0.5f);
            background.color = IsEnabled ? new Color(0.61f, 0.61f, 0.61f) : new Color(0.32f, 0.32f, 0.32f);
        }

        protected abstract void UpdateItemCount();
    }
}
