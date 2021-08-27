﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VNEI.Logic {
    public class RecipeInfo {
        public Dictionary<Item, Amount> ingredient = new Dictionary<Item, Amount>();
        public Dictionary<Item, Amount> result = new Dictionary<Item, Amount>();

        public struct Amount {
            public int min;
            public int max;
            public bool fixedCount;
            public float chance;

            public Amount(int min, int max, float chance = 1f) {
                this.min = min;
                this.max = max;
                fixedCount = min == max;
                this.chance = chance;
            }

            public Amount(int amount, float chance = 1f) {
                min = amount;
                max = amount;
                fixedCount = true;
                this.chance = chance;
            }

            public override string ToString() {
                int percent = Mathf.RoundToInt(chance * 100f);
                string value = "";

                if (percent != 100) {
                    value += $"{percent}% ";
                }

                if (fixedCount) {
                    value += $"{max}x";
                } else {
                    value += $"{min}-{max}x";
                }

                return value;
            }
        }

        public void AddIngredient<T>(T item, Amount count, Func<T, string> getName, string context) {
            if (item != null) {
                int key = Indexing.CleanupName(getName(item)).GetStableHashCode();
                if (Indexing.Items.ContainsKey(key)) {
                    ingredient.Add(Indexing.Items[key], count);
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
                    result.Add(Indexing.Items[key], count);
                } else {
                    Log.LogInfo($"cannot add item '{getName(item)}' to result as is not indexed");
                }
            } else {
                Log.LogInfo($"cannot add result to '{context}', item is null (uses amount {count})");
            }
        }

        public RecipeInfo(Recipe recipe) {
            AddResult(recipe.m_item, new Amount(recipe.m_amount), i => i.name, recipe.name);

            foreach (Piece.Requirement resource in recipe.m_resources) {
                AddIngredient(resource.m_resItem, new Amount(resource.m_amount), i => i.name, recipe.name);
            }
        }

        public RecipeInfo(Smelter.ItemConversion conversion, string context) {
            AddIngredient(conversion.m_from, new Amount(1), i => i.name, context);
            AddResult(conversion.m_to, new Amount(1), i => i.name, context);
        }

        public RecipeInfo(Fermenter.ItemConversion conversion, string context) {
            AddIngredient(conversion.m_from, new Amount(1), i => i.name, context);
            AddResult(conversion.m_to, new Amount(conversion.m_producedItems), i => i.name, context);
        }

        public RecipeInfo(CookingStation.ItemConversion conversion, string context) {
            AddIngredient(conversion.m_from, new Amount(1), i => i.name, context);
            AddResult(conversion.m_to, new Amount(1), i => i.name, context);
        }

        public RecipeInfo(Character character, List<CharacterDrop.Drop> characterDrops) {
            AddIngredient(character, new Amount(1), i => i.name, character.name);

            foreach (CharacterDrop.Drop drop in characterDrops) {
                AddResult(drop.m_prefab, new Amount(drop.m_amountMin, drop.m_amountMax, drop.m_chance), i => i.name, character.name);
            }
        }

        public RecipeInfo(GameObject prefab, Piece.Requirement[] requirements) {
            AddResult(prefab, new Amount(1), i => i.name, prefab.name);

            foreach (Piece.Requirement requirement in requirements) {
                AddIngredient(requirement.m_resItem, new Amount(requirement.m_amount), i => i.name, prefab.name);
            }
        }

        public RecipeInfo(Piece piece, Pickable pickable) {
            AddIngredient(piece, new Amount(1), i => i.name, piece.name);
            AddResult(pickable.m_itemPrefab, new Amount(pickable.m_amount), i => i.name, pickable.name);
        }

        public RecipeInfo(MineRock mineRock) {
            Amount amount = new Amount(mineRock.m_dropItems.m_dropMin, mineRock.m_dropItems.m_dropMax, mineRock.m_dropItems.m_dropChance);
            AddIngredient(mineRock, new Amount(1), i => i.name, mineRock.name);

            foreach (DropTable.DropData drop in mineRock.m_dropItems.m_drops) {
                AddResult(drop.m_item, new Amount(amount.min * drop.m_stackMin, amount.max * drop.m_stackMax), i => i.name, mineRock.name);
            }
        }

        public RecipeInfo(DropOnDestroyed dropOnDestroyed) {
            Amount amount = new Amount(dropOnDestroyed.m_dropWhenDestroyed.m_dropMin, dropOnDestroyed.m_dropWhenDestroyed.m_dropMax,
                                       dropOnDestroyed.m_dropWhenDestroyed.m_dropChance);
            AddIngredient(dropOnDestroyed, new Amount(1), i => i.name, dropOnDestroyed.name);

            foreach (DropTable.DropData drop in dropOnDestroyed.m_dropWhenDestroyed.m_drops) {
                AddResult(drop.m_item, new Amount(amount.min * drop.m_stackMin, amount.max * drop.m_stackMax, amount.chance), i => i.name,
                          dropOnDestroyed.name);
            }
        }

        public bool IngredientsAndResultSame() {
            return ingredient.SequenceEqual(result);
        }
    }
}
