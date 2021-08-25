using System.Collections.Generic;
using UnityEngine;

namespace VNEI.Logic {
    public class Item {
        public string internalName;
        public string localizedName;
        public string description;
        public Sprite[] icons;
        public GameObject gameObject;

        public List<RecipeInfo> result = new List<RecipeInfo>();
        public List<RecipeInfo> ingredient = new List<RecipeInfo>();
    }
}
