using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VNEI.Logic {
    public class RecipeInfo {
        public List<Part> ingredient = new List<Part>();
        public List<Part> result = new List<Part>();
        public Part station;
        public Amount droppedCount = new Amount(1);
        public bool isOnBlacklist;

        public class Part {
            public readonly Item item;
            public readonly Amount amount;
            public readonly int quality;

            public Part(Item item, Amount amount, int quality) {
                this.item = item;
                this.amount = amount;
                this.quality = quality;
            }

            protected bool Equals(Part other) {
                return Equals(item, other.item) && amount.Equals(other.amount) && quality == other.quality;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((Part)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = (item != null ? item.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ amount.GetHashCode();
                    hashCode = (hashCode * 397) ^ quality;
                    return hashCode;
                }
            }
        }

        public void AddIngredient<T>(T target, Amount count, int quality, Func<T, string> getName, string context) {
            if (target != null) {
                Item item = Indexing.GetItem(getName(target));

                if (item != null) {
                    ingredient.Add(new Part(item, count, quality));
                }
            } else {
                Log.LogInfo($"cannot add ingredient to '{context}', item is null (uses amount {count})");
            }
        }

        public void AddResult<T>(T target, Amount count, int quality, Func<T, string> getName, string context) {
            if (target != null) {
                Item item = Indexing.GetItem(getName(target));

                if (item != null) {
                    result.Add(new Part(item, count, quality));
                }
            } else {
                Log.LogInfo($"cannot add result to '{context}', item is null (uses amount {count})");
            }
        }

        public void SetStation<T>(T target, int level, Func<T, string> getName) {
            if (target != null) {
                Item item = Indexing.GetItem(getName(target));

                if (item != null) {
                    station = new Part(item, new Amount(1), level);
                }
            } else {
                Log.LogInfo($"cannot set station: is null");
            }
        }

        private void CalculateIsOnBlacklist() {
            if (ingredient.Any(i => Plugin.ItemBlacklist.Contains(i.item.internalName))) {
                isOnBlacklist = true;
                return;
            }

            if (result.Any(i => Plugin.ItemBlacklist.Contains(i.item.internalName))) {
                isOnBlacklist = true;
            }
        }

        public RecipeInfo(Recipe recipe, int quality) {
            if (recipe.m_craftingStation != null) {
                SetStation(recipe.m_craftingStation, quality, i => i.name);
            }

            AddResult(recipe.m_item, new Amount(recipe.m_amount), quality, i => i.name, recipe.name);

            if (quality > 1) {
                AddIngredient(recipe.m_item, new Amount(1), quality - 1, i => i.name, recipe.name);
            }

            foreach (Piece.Requirement resource in recipe.m_resources) {
                Amount amount = new Amount(resource.GetAmount(quality));
                if (Amount.IsSameMinMax(amount, Amount.Zero)) {
                    continue;
                }

                AddIngredient(resource.m_resItem, amount, quality, i => i.name, recipe.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Smelter.ItemConversion conversion, Smelter smelter) {
            SetStation(smelter, 1, i => i.name);
            AddIngredient(conversion.m_from, new Amount(1), 1, i => i.name, smelter.name);
            AddResult(conversion.m_to, new Amount(1), 1, i => i.name, smelter.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Fermenter.ItemConversion conversion, Fermenter fermenter) {
            SetStation(fermenter, 1, i => i.name);
            AddIngredient(conversion.m_from, new Amount(1), 1, i => i.name, fermenter.name);
            AddResult(conversion.m_to, new Amount(conversion.m_producedItems), 1, i => i.name, fermenter.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(CookingStation.ItemConversion conversion, CookingStation cookingStation) {
            SetStation(cookingStation, 1, i => i.name);
            AddIngredient(conversion.m_from, new Amount(1), 1, i => i.name, cookingStation.name);
            AddResult(conversion.m_to, new Amount(1), 1, i => i.name, cookingStation.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Character character, List<CharacterDrop.Drop> characterDrops) {
            AddIngredient(character, new Amount(1), 1, i => i.name, character.name);

            foreach (CharacterDrop.Drop drop in characterDrops) {
                AddResult(drop.m_prefab, new Amount(drop.m_amountMin, drop.m_amountMax, drop.m_chance), 1, i => i.name, character.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject prefab, Piece piece, Item crafter) {
            station = new Part(crafter, new Amount(1), 1);
            AddResult(prefab, new Amount(1), 1, i => i.name, prefab.name);

            foreach (Piece.Requirement requirement in piece.m_resources) {
                AddIngredient(requirement.m_resItem, new Amount(requirement.m_amount), 1, i => i.name, prefab.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject piece, Pickable pickable) {
            AddIngredient(piece, new Amount(1), 1, i => i.name, piece.name);
            AddResult(pickable.m_itemPrefab, new Amount(pickable.m_amount), 1, i => i.name, pickable.name);

            if (pickable.m_extraDrops != null && pickable.m_extraDrops.m_drops.Count > 0) {
                RecipeInfo fromDropTable = new RecipeInfo(piece, pickable.m_extraDrops);

                // TODO extra drops could potentially be wrong in future, extra drops min/max is ignored as not displayable at the moment

                foreach (Part part in fromDropTable.ingredient) {
                    Amount amount = part.amount;
                    amount.chance = fromDropTable.droppedCount.chance;
                    ingredient.Add(new Part(part.item, amount, 1));
                }

                foreach (Part part in fromDropTable.result) {
                    Amount amount = part.amount;
                    amount.chance = fromDropTable.droppedCount.chance;
                    ingredient.Add(new Part(part.item, amount, 1));
                }
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject from, DropTable dropTable) {
            droppedCount = new Amount(dropTable.m_dropMin, dropTable.m_dropMax, dropTable.m_dropChance);
            AddIngredient(from, new Amount(1), 1, i => i.name, from.name);

            float totalWeight = dropTable.m_drops.Sum(i => i.m_weight);

            foreach (DropTable.DropData drop in dropTable.m_drops) {
                float chance = totalWeight == 0 ? 1 : drop.m_weight / totalWeight;
                AddResult(drop.m_item, new Amount(drop.m_stackMin, drop.m_stackMax, chance), 1, i => i.name, from.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(SpawnArea spawnArea) {
            AddIngredient(spawnArea, new Amount(1), 1, i => i.name, spawnArea.name);
            foreach (SpawnArea.SpawnData spawnData in spawnArea.m_prefabs) {
                AddResult(spawnData.m_prefab, new Amount(1, 1f / spawnArea.m_prefabs.Count), 1, i => i.name, spawnArea.name);
            }

            CalculateIsOnBlacklist();
        }

        public bool IngredientsAndResultSame() {
            return ingredient.SequenceEqual(result);
        }
    }
}
