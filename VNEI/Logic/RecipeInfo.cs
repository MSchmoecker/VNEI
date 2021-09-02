using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VNEI.Logic {
    public class RecipeInfo {
        public List<Tuple<Item, Amount>> ingredient = new List<Tuple<Item, Amount>>();
        public List<Tuple<Item, Amount>> result = new List<Tuple<Item, Amount>>();
        public Item station;
        public Amount droppedCount = new Amount(1);
        public bool isOnBlacklist;

        public void AddIngredient<T>(T item, Amount count, Func<T, string> getName, string context) {
            if (item != null) {
                int key = Indexing.CleanupName(getName(item)).GetStableHashCode();
                if (Indexing.Items.ContainsKey(key)) {
                    ingredient.Add(new Tuple<Item, Amount>(Indexing.Items[key], count));
                } else {
                    Log.LogInfo($"cannot add item '{getName(item)}' to ingredient as is not indexed");
                }
            } else {
                Log.LogInfo($"cannot add ingredient to '{context}', item is null (uses amount {count})");
            }
        }

        public void AddResult<T>(T item, Amount count, Func<T, string> getName, string context) {
            if (item != null) {
                int key = Indexing.CleanupName(getName(item)).GetStableHashCode();
                if (Indexing.Items.ContainsKey(key)) {
                    result.Add(new Tuple<Item, Amount>(Indexing.Items[key], count));
                } else {
                    Log.LogInfo($"cannot add item '{getName(item)}' to result as is not indexed");
                }
            } else {
                Log.LogInfo($"cannot add result to '{context}', item is null (uses amount {count})");
            }
        }

        public void SetStation<T>(T item, Func<T, string> getName) {
            if (item != null) {
                int key = Indexing.CleanupName(getName(item)).GetStableHashCode();
                if (Indexing.Items.ContainsKey(key)) {
                    station = Indexing.Items[key];
                } else {
                    Log.LogInfo($"cannot set station '{getName(item)}' as is not indexed");
                }
            } else {
                Log.LogInfo($"cannot set station: is null");
            }
        }

        private void CalculateIsOnBlacklist() {
            if (ingredient.Any(i => Plugin.ItemBlacklist.Contains(i.Item1.internalName))) {
                isOnBlacklist = true;
                return;
            }

            if (result.Any(i => Plugin.ItemBlacklist.Contains(i.Item1.internalName))) {
                isOnBlacklist = true;
            }
        }

        public RecipeInfo(Recipe recipe) {
            if (recipe.m_craftingStation != null) {
                SetStation(recipe.m_craftingStation, i => i.name);
            }

            AddResult(recipe.m_item, new Amount(recipe.m_amount), i => i.name, recipe.name);

            foreach (Piece.Requirement resource in recipe.m_resources) {
                AddIngredient(resource.m_resItem, new Amount(resource.m_amount), i => i.name, recipe.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Smelter.ItemConversion conversion, Smelter smelter) {
            SetStation(smelter, i => i.name);
            AddIngredient(conversion.m_from, new Amount(1), i => i.name, smelter.name);
            AddResult(conversion.m_to, new Amount(1), i => i.name, smelter.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Fermenter.ItemConversion conversion, Fermenter fermenter) {
            SetStation(fermenter, i => i.name);
            AddIngredient(conversion.m_from, new Amount(1), i => i.name, fermenter.name);
            AddResult(conversion.m_to, new Amount(conversion.m_producedItems), i => i.name, fermenter.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(CookingStation.ItemConversion conversion, CookingStation cookingStation) {
            SetStation(cookingStation, i => i.name);
            AddIngredient(conversion.m_from, new Amount(1), i => i.name, cookingStation.name);
            AddResult(conversion.m_to, new Amount(1), i => i.name, cookingStation.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Character character, List<CharacterDrop.Drop> characterDrops) {
            AddIngredient(character, new Amount(1), i => i.name, character.name);

            foreach (CharacterDrop.Drop drop in characterDrops) {
                AddResult(drop.m_prefab, new Amount(drop.m_amountMin, drop.m_amountMax, drop.m_chance), i => i.name, character.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject prefab, Piece.Requirement[] requirements) {
            AddResult(prefab, new Amount(1), i => i.name, prefab.name);

            foreach (Piece.Requirement requirement in requirements) {
                AddIngredient(requirement.m_resItem, new Amount(requirement.m_amount), i => i.name, prefab.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject piece, Pickable pickable) {
            AddIngredient(piece, new Amount(1), i => i.name, piece.name);
            AddResult(pickable.m_itemPrefab, new Amount(pickable.m_amount), i => i.name, pickable.name);

            if (pickable.m_extraDrops != null && pickable.m_extraDrops.m_drops.Count > 0) {
                RecipeInfo fromDropTable = new RecipeInfo(piece, pickable.m_extraDrops);

                // TODO extra drops could potentially be wrong in future, extra drops min/max is ignored as not displayable at the moment

                foreach (Tuple<Item, Amount> tuple in fromDropTable.ingredient) {
                    Amount amount = tuple.Item2;
                    amount.chance = fromDropTable.droppedCount.chance;
                    ingredient.Add(new Tuple<Item, Amount>(tuple.Item1, amount));
                }

                foreach (Tuple<Item, Amount> tuple in fromDropTable.result) {
                    Amount amount = tuple.Item2;
                    amount.chance = fromDropTable.droppedCount.chance;
                    ingredient.Add(new Tuple<Item, Amount>(tuple.Item1, amount));
                }
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject from, DropTable dropTable) {
            droppedCount = new Amount(dropTable.m_dropMin, dropTable.m_dropMax, dropTable.m_dropChance);
            AddIngredient(from, new Amount(1), i => i.name, from.name);

            float totalWeight = dropTable.m_drops.Sum(i => i.m_weight);

            foreach (DropTable.DropData drop in dropTable.m_drops) {
                float chance = totalWeight == 0 ? 1 : drop.m_weight / totalWeight;
                AddResult(drop.m_item, new Amount(drop.m_stackMin, drop.m_stackMax, chance), i => i.name, from.name);
            }

            CalculateIsOnBlacklist();
        }

        public bool IngredientsAndResultSame() {
            return ingredient.SequenceEqual(result);
        }
    }
}
