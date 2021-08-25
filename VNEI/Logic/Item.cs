using System.Collections.Generic;
using UnityEngine;

namespace VNEI.Logic {
    public class Item {
        public string internalName;
        public string localizedName;
        public string description;
        public Sprite[] icons;
        public GameObject gameObject;

        public List<Recipe> result = new List<Recipe>();
        public List<Recipe> ingredient = new List<Recipe>();
    }
}
