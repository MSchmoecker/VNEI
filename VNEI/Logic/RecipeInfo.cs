using System.Collections.Generic;
using UnityEngine;

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
                    int key = Indexing.GetRequirementName(resource).GetStableHashCode();
                    if (Indexing.Items.ContainsKey(key)) {
                        ingredient.Add(Indexing.Items[key]);
                    } else {
                        Log.LogInfo($"Recipe Piece.Requirement ingredient not indexed: {resource.m_resItem.name}");
                    }
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

        public RecipeInfo(Character character, List<CharacterDrop.Drop> characterDrops) {
            ingredient.Add(Indexing.Items[character.name.GetStableHashCode()]);

            foreach (CharacterDrop.Drop drop in characterDrops) {
                result.Add(Indexing.Items[drop.m_prefab.name.GetStableHashCode()]);
            }
        }

        public RecipeInfo(GameObject prefab, Piece.Requirement[] requirements) {
            result.Add(Indexing.Items[prefab.name.GetStableHashCode()]);

            foreach (Piece.Requirement requirement in requirements) {
                if ((bool)requirement.m_resItem) {
                    int key = Indexing.GetRequirementName(requirement).GetStableHashCode();
                    if (Indexing.Items.ContainsKey(key)) {
                        ingredient.Add(Indexing.Items[key]);
                    } else {
                        Log.LogInfo($"Piece ingredient not indexed: {requirement.m_resItem.name}");
                    }
                } else {
                    Log.LogInfo($"Piece ingredient is null: {prefab.name}");
                }
            }
        }
    }
}
