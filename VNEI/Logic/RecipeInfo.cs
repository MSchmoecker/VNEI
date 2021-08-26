using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VNEI.Logic {
    public class RecipeInfo {
        public string name;
        public List<Item> ingredient = new List<Item>();
        public List<Item> result = new List<Item>();

        public void AddIngredient<T>(T item, Func<T, string> getName, string context) {
            if (item != null) {
                int key = Indexing.CleanupName(getName(item)).GetStableHashCode();
                if (Indexing.Items.ContainsKey(key)) {
                    ingredient.Add(Indexing.Items[key]);
                } else {
                    Log.LogInfo($"cannot add item {getName(item)} to ingredient, {name} is not indexed");
                }
            } else {
                Log.LogInfo("cannot add ingredient, item is null " + context);
            }
        }

        public void AddResult<T>(T item, Func<T, string> getName, string context) {
            if (item != null) {
                int key = Indexing.CleanupName(getName(item)).GetStableHashCode();
                if (Indexing.Items.ContainsKey(key)) {
                    result.Add(Indexing.Items[key]);
                } else {
                    Log.LogInfo($"cannot add item {getName(item)} to result, {name} is not indexed");
                }
            } else {
                Log.LogInfo("cannot add result, item is null " + context);
            }
        }

        public RecipeInfo(Recipe recipe) {
            AddResult(recipe.m_item, i => i.name, recipe.name);

            foreach (Piece.Requirement resource in recipe.m_resources) {
                AddIngredient(resource.m_resItem, i => i.name, recipe.name);
            }
        }

        public RecipeInfo(Smelter.ItemConversion conversion, string context) {
            AddIngredient(conversion.m_from, i => i.name, context);
            AddResult(conversion.m_to, i => i.name, context);
        }

        public RecipeInfo(Fermenter.ItemConversion conversion, string context) {
            AddIngredient(conversion.m_from, i => i.name, context);
            AddResult(conversion.m_to, i => i.name, context);
        }

        public RecipeInfo(CookingStation.ItemConversion conversion, string context) {
            AddIngredient(conversion.m_from, i => i.name, context);
            AddResult(conversion.m_to, i => i.name, context);
        }

        public RecipeInfo(Character character, List<CharacterDrop.Drop> characterDrops) {
            AddIngredient(character, i => i.name, character.name);

            foreach (CharacterDrop.Drop drop in characterDrops) {
                AddResult(drop.m_prefab, i => i.name, character.name);
            }
        }

        public RecipeInfo(GameObject prefab, Piece.Requirement[] requirements) {
            AddResult(prefab, i => i.name, prefab.name);

            foreach (Piece.Requirement requirement in requirements) {
                AddIngredient(requirement.m_resItem, i => i.name, prefab.name);
            }
        }

        public RecipeInfo(Piece piece, Pickable pickable) {
            AddIngredient(piece, i => i.name, piece.name);
            AddResult(pickable.m_itemPrefab, i => i.name, pickable.name);
        }

        public bool IngredientsAndResultSame() {
            return ingredient.SequenceEqual(result);
        }
    }
}
