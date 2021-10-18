using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VNEI.Logic {
    public class RecipeInfo {
        public Dictionary<Amount, List<Part>> ingredient = new Dictionary<Amount, List<Part>>();
        public Dictionary<Amount, List<Part>> result = new Dictionary<Amount, List<Part>>();
        public Part station;
        public bool isOnBlacklist;

        public void AddIngredient(string name, Amount groupAmount, Amount count, int quality) {
            Item item = Indexing.GetItem(name);

            if (item != null) {
                if (!ingredient.ContainsKey(groupAmount)) {
                    ingredient.Add(groupAmount, new List<Part>());
                }

                ingredient[groupAmount].Add(new Part(item, count, quality));
            }
        }

        public void AddIngredient<T>(T target, Amount groupAmount, Amount count, int quality, string context) where T : Object {
            if (target) {
                AddIngredient(target.name, groupAmount, count, quality);
            } else {
                Log.LogDebug($"cannot add ingredient to '{context}', item is null (uses amount {count})");
            }
        }

        public void AddResult(string name, Amount groupAmount, Amount count, int quality) {
            Item item = Indexing.GetItem(name);

            if (item != null) {
                if (!result.ContainsKey(groupAmount)) {
                    result.Add(groupAmount, new List<Part>());
                }

                result[groupAmount].Add(new Part(item, count, quality));
            }
        }

        public void AddResult<T>(T target, Amount groupAmount, Amount count, int quality, string context) where T : Object {
            if (target) {
                AddResult(target.name, groupAmount, count, quality);
            } else {
                Log.LogDebug($"cannot add result to '{context}', item is null (uses amount {count})");
            }
        }

        public void SetStation(string name, int level) {
            Item item = Indexing.GetItem(name);

            if (item != null) {
                station = new Part(item, new Amount(1), level);
            }
        }

        public void SetStation<T>(T target, int level) where T : Object {
            if (target != null) {
                SetStation(target.name, level);
            } else {
                Log.LogDebug($"cannot set station: is null");
            }
        }

        private void CalculateIsOnBlacklist() {
            if (ingredient.SelectMany(i => i.Value).Any(i => Plugin.ItemBlacklist.Contains(i.item.internalName))) {
                isOnBlacklist = true;
                return;
            }

            if (result.SelectMany(i => i.Value).Any(i => Plugin.ItemBlacklist.Contains(i.item.internalName))) {
                isOnBlacklist = true;
            }
        }

        public RecipeInfo() {
        }

        public RecipeInfo(Recipe recipe, int quality) {
            if (recipe.m_craftingStation != null) {
                SetStation(recipe.m_craftingStation, quality);
            }

            AddResult(recipe.m_item, Amount.One, new Amount(recipe.m_amount), quality, recipe.name);

            if (quality > 1) {
                AddIngredient(recipe.m_item, Amount.One, Amount.One, quality - 1, recipe.name);
            }

            foreach (Piece.Requirement resource in recipe.m_resources) {
                Amount amount = new Amount(resource.GetAmount(quality));
                if (Amount.IsSameMinMax(amount, Amount.Zero)) {
                    continue;
                }

                AddIngredient(resource.m_resItem, Amount.One, amount, quality, recipe.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Smelter.ItemConversion conversion, Smelter smelter) {
            SetStation(smelter, 1);
            AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, smelter.name);

            if ((bool)smelter.m_fuelItem) {
                AddIngredient(smelter.m_fuelItem, Amount.One, new Amount(smelter.m_fuelPerProduct), 1, smelter.name);
            }

            AddResult(conversion.m_to, Amount.One, Amount.One, 1, smelter.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Fermenter.ItemConversion conversion, Fermenter fermenter) {
            SetStation(fermenter, 1);
            AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, fermenter.name);
            AddResult(conversion.m_to, Amount.One, new Amount(conversion.m_producedItems), 1, fermenter.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(CookingStation.ItemConversion conversion, CookingStation cookingStation) {
            SetStation(cookingStation, 1);
            AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, cookingStation.name);
            AddResult(conversion.m_to, Amount.One, Amount.One, 1, cookingStation.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Incinerator incinerator) {
            SetStation(incinerator, 1);
            AddResult(incinerator.m_defaultResult, Amount.One, Amount.One, 1, incinerator.name);
            AddIngredient("vnei_any_item", Amount.One, new Amount(incinerator.m_defaultCost), 1);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Incinerator.IncineratorConversion conversion, Incinerator incinerator) {
            SetStation(incinerator, 1);
            AddResult(conversion.m_result, Amount.One, new Amount(conversion.m_resultAmount), 1, incinerator.name);
            foreach (Incinerator.Requirement requirement in conversion.m_requirements) {
                AddIngredient(requirement.m_resItem, Amount.One, new Amount(requirement.m_amount), 1, incinerator.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Incinerator.IncineratorConversion conversion, Incinerator.Requirement requirement, Incinerator incinerator) {
            SetStation(incinerator, 1);
            AddResult(conversion.m_result, Amount.One, new Amount(conversion.m_resultAmount), 1, incinerator.name);
            AddIngredient(requirement.m_resItem, Amount.One, new Amount(requirement.m_amount), 1, incinerator.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Character character, List<CharacterDrop.Drop> characterDrops) {
            AddIngredient(character, Amount.One, Amount.One, 1, character.name);

            foreach (CharacterDrop.Drop drop in characterDrops) {
                Amount amount = new Amount(drop.m_amountMin, drop.m_amountMax, drop.m_chance);
                AddResult(drop.m_prefab, Amount.One, amount, 1, character.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject prefab, Piece piece, Item crafter) {
            station = new Part(crafter, new Amount(1), 1);
            AddResult(prefab, Amount.One, Amount.One, 1, prefab.name);

            foreach (Piece.Requirement requirement in piece.m_resources) {
                AddIngredient(requirement.m_resItem, Amount.One, new Amount(requirement.m_amount), 1, prefab.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject piece, Pickable pickable) {
            AddIngredient(piece, Amount.One, Amount.One, 1, piece.name);
            AddResult(pickable.m_itemPrefab, Amount.One, new Amount(pickable.m_amount), 1, pickable.name);

            if (pickable.m_extraDrops != null && pickable.m_extraDrops.m_drops.Count > 0) {
                AddDropTable(piece, pickable.m_extraDrops);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(GameObject from, DropTable dropTable) {
            AddDropTable(from, dropTable);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(SpawnArea spawnArea) {
            AddIngredient(spawnArea, Amount.One, Amount.One, 1, spawnArea.name);
            foreach (SpawnArea.SpawnData spawnData in spawnArea.m_prefabs) {
                AddResult(spawnData.m_prefab, Amount.One, new Amount(1, 1f / spawnArea.m_prefabs.Count), 1, spawnArea.name);
            }

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Destructible spawnArea) {
            AddIngredient(spawnArea, Amount.One, Amount.One, 1, spawnArea.name);
            AddResult(spawnArea.m_spawnWhenDestroyed, Amount.One, Amount.One, 1, spawnArea.name);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(TreeLog treeLog) {
            for (int i = 0; i < treeLog.m_subLogPoints.Length; i++) {
                AddResult(treeLog.m_subLogPrefab, Amount.One, Amount.One, 1, treeLog.name);
            }

            AddDropTable(treeLog.gameObject, treeLog.m_dropWhenDestroyed);

            CalculateIsOnBlacklist();
        }

        public RecipeInfo(TreeBase treeBase) {
            AddResult(treeBase.m_stubPrefab, Amount.One, Amount.One, 1, treeBase.name);
            AddResult(treeBase.m_logPrefab, Amount.One, Amount.One, 1, treeBase.name);
            AddDropTable(treeBase.gameObject, treeBase.m_dropWhenDestroyed);
            CalculateIsOnBlacklist();
        }

        public RecipeInfo(Trader trader, Trader.TradeItem tradeItem) {
            SetStation(trader, 1);
            AddIngredient(StoreGui.instance.m_coinPrefab, Amount.One, new Amount(tradeItem.m_price), 1, trader.name);
            AddResult(tradeItem.m_prefab, Amount.One, new Amount(tradeItem.m_stack), 1, trader.name);
            CalculateIsOnBlacklist();
        }

        private void AddDropTable(GameObject from, DropTable dropTable) {
            Amount tableCount = new Amount(dropTable.m_dropMin, dropTable.m_dropMax, dropTable.m_dropChance);
            AddIngredient(from, Amount.One, Amount.One, 1, from.name);

            float totalWeight = dropTable.m_drops.Sum(i => i.m_weight);

            foreach (DropTable.DropData drop in dropTable.m_drops) {
                float chance = totalWeight == 0 ? 1 : drop.m_weight / totalWeight;
                AddResult(drop.m_item, tableCount, new Amount(drop.m_stackMin, drop.m_stackMax, chance), 1, from.name);
            }

            CalculateIsOnBlacklist();
        }

        public bool IngredientsAndResultSame() {
            return ingredient.SequenceEqual(result);
        }
    }
}
