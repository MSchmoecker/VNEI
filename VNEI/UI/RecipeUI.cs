using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VNEI.Logic;

namespace VNEI.UI {
    public class RecipeUI : MonoBehaviour {
        public static RecipeUI Instance { get; private set; }

        [SerializeField] public RectTransform results;
        [SerializeField] public RectTransform ingredients;

        private Item currentItem;

        private void Awake() {
            Instance = this;
        }

        public void SetItem(Item item) {
            currentItem = item;

            for (int i = 0; i < results.childCount; i++) {
                Destroy(results.GetChild(i).gameObject);
            }

            for (int i = 0; i < ingredients.childCount; i++) {
                Destroy(ingredients.GetChild(i).gameObject);
            }

            foreach (Recipe recipe in item.result) {
                SpawnRecipe(recipe, results);
            }

            foreach (Recipe recipe in item.ingredient) {
                SpawnRecipe(recipe, ingredients);
            }
        }

        public void SpawnRecipe(Recipe recipe, RectTransform root) {
            GameObject row = Instantiate(BaseUI.Instance.rowPrefab, root);
            SpawnItemDrop(recipe.m_item, recipe, row.GetComponent<RectTransform>());

            foreach (Piece.Requirement resource in recipe.m_resources) {
                SpawnItemDrop(resource.m_resItem, recipe, row.GetComponent<RectTransform>());
            }
        }

        void SpawnItemDrop(ItemDrop itemDrop, Recipe recipe, RectTransform root) {
            GameObject item = Instantiate(BaseUI.Instance.itemPrefab, root);

            if (itemDrop == null) {
                Log.LogInfo("ItemDrop is null: recipe " + recipe.name);
                return;
            }

            int key = itemDrop.name.GetStableHashCode();
            if (Indexing.Items.ContainsKey(key)) {
                item.GetComponent<MouseHover>().SetItem(Indexing.Items[key]);
                item.GetComponent<Image>().sprite = Indexing.Items[key].icons.FirstOrDefault();
            } else {
                Log.LogInfo("Itemdrop not indexed: " + itemDrop.name + "/" + itemDrop.m_itemData?.m_shared?.m_name);
            }
        }
    }
}
