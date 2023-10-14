using System;
using TMPro;
using UnityEngine;

namespace VNEI.UI {
    public class TabButton : MonoBehaviour {
        private TextMeshProUGUI nameText;

        public void Awake() {
            nameText = transform.Find("Text").GetComponent<TextMeshProUGUI>();

            Plugin.tabName.SettingChanged += UpdateTabName;
            UpdateTabName(null, EventArgs.Empty);
        }

        private void OnDestroy() {
            Plugin.tabName.SettingChanged -= UpdateTabName;
        }

        private void UpdateTabName(object sender, EventArgs e) {
            nameText.text = Plugin.tabName.Value;
        }
    }
}
