using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class SearchUI : MonoBehaviour {
        public static SearchUI Instance { get; private set; }

        public void Awake() {
            Instance = this;
            Transform content = GetComponent<ScrollRect>().content;

            foreach (KeyValuePair<int, Item> item in Indexing.Items) {
                GameObject sprite = Instantiate(BaseUI.Instance.itemPrefab, content);
                sprite.GetComponent<Image>().sprite = item.Value.icons.FirstOrDefault();
                sprite.GetComponentInChildren<MouseHover>().SetItem(item.Value);
            }
        }
    }
}
