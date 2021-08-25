using System.Collections.Generic;

namespace VNEI.Logic {
    public class RecipeInfo {
        public string name;
        public List<Item> ingredient = new List<Item>();
        public List<Item> result = new List<Item>();

        public RecipeInfo(Recipe recipe) {
            name = recipe.name;
            if ((bool)recipe.m_item) {
                result.Add(Indexing.Items[recipe.m_item.name.GetStableHashCode()]);
            } else {
                Log.LogInfo("ItemDrop result is null: recipe " + recipe.name);
            }

            foreach (Piece.Requirement resource in recipe.m_resources) {
                if ((bool)resource.m_resItem) {
                    ingredient.Add(Indexing.Items[resource.m_resItem.name.GetStableHashCode()]);
                } else {
                    Log.LogInfo("ItemDrop ingredient is null: recipe " + recipe.name);
                }
            }
        }

        public RecipeInfo(Smelter.ItemConversion conversion) {
            if ((bool)conversion.m_from) {
                ingredient.Add(Indexing.Items[conversion.m_from.name.GetStableHashCode()]);
            }

            if ((bool)conversion.m_to) {
                result.Add(Indexing.Items[conversion.m_to.name.GetStableHashCode()]);
            }
        }

        public RecipeInfo(Fermenter.ItemConversion conversion) {
            if ((bool)conversion.m_from) {
                ingredient.Add(Indexing.Items[conversion.m_from.name.GetStableHashCode()]);
            }

            if ((bool)conversion.m_to) {
                result.Add(Indexing.Items[conversion.m_to.name.GetStableHashCode()]);
            }
        }

        public RecipeInfo(CookingStation.ItemConversion conversion) {
            if ((bool)conversion.m_from) {
                ingredient.Add(Indexing.Items[conversion.m_from.name.GetStableHashCode()]);
            }

            if ((bool)conversion.m_to) {
                result.Add(Indexing.Items[conversion.m_to.name.GetStableHashCode()]);
            }
        }
    }
}
