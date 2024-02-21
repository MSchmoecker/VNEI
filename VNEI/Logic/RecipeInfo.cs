using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VNEI.Logic {
    public class RecipeInfo {
        public Dictionary<Amount, List<Part>> Ingredients { get; private set; } = new Dictionary<Amount, List<Part>>();
        public Dictionary<Amount, List<Part>> Results { get; private set; } = new Dictionary<Amount, List<Part>>();
        public List<Part> Stations { get; private set; } = new List<Part>();
        public bool IsOnBlacklist { get; private set; }

        public static List<RecipeInfo> Recipes { get; private set; } = new List<RecipeInfo>();

        public bool IsSelfKnown { get; private set; }
        public float Width { get; private set; }

        public IEnumerable<Item> GetStationItems() {
            if (Stations.Count >= 1) {
                return new[] { Stations[0].item ?? Plugin.Instance.noStation };
            }

            return new[] { Plugin.Instance.noStation };
        }

        public void UpdateKnown() {
            IsSelfKnown = CalcSelfKnown();
        }

        private static bool AnySelfKnown(Dictionary<Amount, List<Part>> list) {
            return list.Where(pair => pair.Key.max != 0 && pair.Value != null).Any(pair => pair.Value.Any(part => part.item.IsSelfKnown));
        }

        private static bool AllSelfKnown(Dictionary<Amount, List<Part>> list) {
            return list.Where(pair => pair.Key.max != 0 && pair.Value != null).All(pair => pair.Value.All(part => part.item.IsSelfKnown));
        }

        private bool CalcSelfKnown() {
            if (!Plugin.ShowOnlyKnown) {
                return true;
            }

            if (Stations.Any(s => !s.item.IsSelfKnown)) {
                return false;
            }

            if (AllSelfKnown(Ingredients)) {
                return true;
            }

            if (AllSelfKnown(Results)) {
                return true;
            }

            return false;
        }

        public void AddIngredient(string name, Amount groupAmount, Amount count, int quality) {
            Item item = Indexing.GetItem(name);

            if (item != null) {
                if (!Ingredients.ContainsKey(groupAmount)) {
                    Ingredients.Add(groupAmount, new List<Part>());
                }

                Ingredients[groupAmount].Add(new Part(item, count, quality));
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
                if (!Results.ContainsKey(groupAmount)) {
                    Results.Add(groupAmount, new List<Part>());
                }

                Results[groupAmount].Add(new Part(item, count, quality));
            }
        }

        public void AddResult<T>(T target, Amount groupAmount, Amount count, int quality, string context) where T : Object {
            if (target) {
                AddResult(target.name, groupAmount, count, quality);
            } else {
                Log.LogDebug($"cannot add result to '{context}', item is null (uses amount {count})");
            }
        }

        public void AddStation(Item item, int level) {
            if (item != null) {
                Stations.Add(new Part(item, new Amount(1), level));
            }
        }

        public void AddStation(string name, int level) {
            AddStation(Indexing.GetItem(name), level);
        }

        public void AddStation<T>(T target, int level) where T : Object {
            if (target != null) {
                AddStation(target.name, level);
            } else {
                Log.LogDebug($"cannot set station: is null");
            }
        }

        public void CalculateIsOnBlacklist() {
            if (Ingredients.SelectMany(i => i.Value).Any(i => i.item.isOnBlacklist)) {
                IsOnBlacklist = true;
                return;
            }

            if (Results.SelectMany(i => i.Value).Any(i => i.item.isOnBlacklist)) {
                IsOnBlacklist = true;
            }
        }

        public RecipeInfo() {
            Recipes.Add(this);
        }

        public RecipeInfo(Recipe recipe, int quality) : this() {
            if (recipe.GetRequiredStation(quality) != null) {
                AddStation(recipe.GetRequiredStation(quality), recipe.GetRequiredStationLevel(quality));
            } else {
                AddStation(Plugin.Instance.handStation, 1);
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

                AddIngredient(resource.m_resItem, Amount.One, amount, 1, recipe.name);
            }
        }

        public RecipeInfo(Smelter.ItemConversion conversion, Smelter smelter) : this() {
            AddStation(smelter, 1);
            AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, smelter.name);

            if ((bool)smelter.m_fuelItem) {
                AddIngredient(smelter.m_fuelItem, Amount.One, new Amount(smelter.m_fuelPerProduct), 1, smelter.name);
            }

            AddResult(conversion.m_to, Amount.One, Amount.One, 1, smelter.name);
        }

        public RecipeInfo(Fermenter.ItemConversion conversion, Fermenter fermenter) : this() {
            AddStation(fermenter, 1);
            AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, fermenter.name);
            AddResult(conversion.m_to, Amount.One, new Amount(conversion.m_producedItems), 1, fermenter.name);
        }

        public RecipeInfo(CookingStation.ItemConversion conversion, CookingStation cookingStation) : this() {
            AddStation(cookingStation, 1);
            AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, cookingStation.name);
            AddResult(conversion.m_to, Amount.One, Amount.One, 1, cookingStation.name);
        }

        public RecipeInfo(Incinerator incinerator) : this() {
            AddStation(incinerator, 1);
            AddResult(incinerator.m_defaultResult, Amount.One, Amount.One, 1, incinerator.name);
            AddIngredient("vnei_any_item", Amount.One, new Amount(incinerator.m_defaultCost), 1);
        }

        public RecipeInfo(Incinerator.IncineratorConversion conversion, Incinerator incinerator) : this() {
            AddStation(incinerator, 1);
            AddResult(conversion.m_result, Amount.One, new Amount(conversion.m_resultAmount), 1, incinerator.name);
            foreach (Incinerator.Requirement requirement in conversion.m_requirements) {
                AddIngredient(requirement.m_resItem, Amount.One, new Amount(requirement.m_amount), 1, incinerator.name);
            }
        }

        public RecipeInfo(Incinerator.IncineratorConversion conversion, Incinerator.Requirement requirement, Incinerator incinerator) : this() {
            AddStation(incinerator, 1);
            AddResult(conversion.m_result, Amount.One, new Amount(conversion.m_resultAmount), 1, incinerator.name);
            AddIngredient(requirement.m_resItem, Amount.One, new Amount(requirement.m_amount), 1, incinerator.name);
        }

        public RecipeInfo(CharacterDrop characterDrop) : this() {
            Character character = characterDrop.GetComponent<Character>();
            AddIngredient(character, Amount.One, Amount.One, 1, character.name);

            foreach (CharacterDrop.Drop drop in characterDrop.m_drops) {
                Amount amount = new Amount(drop.m_amountMin, drop.m_amountMax, drop.m_chance);
                AddResult(drop.m_prefab, Amount.One, amount, 1, character.name);
            }
        }

        public RecipeInfo(GameObject prefab, Piece piece, Item crafter) : this() {
            Stations.Add(new Part(crafter, new Amount(1), 1));

            if (piece.m_craftingStation) {
                AddStation(piece.m_craftingStation, 1);
            }

            foreach (Piece.Requirement requirement in piece.m_resources) {
                AddIngredient(requirement.m_resItem, Amount.One, new Amount(requirement.m_amount), 1, prefab.name);
            }

            if (piece.TryGetComponent(out Plant plant) && plant.m_grownPrefabs?.Length >= 1) {
                foreach (GameObject grownPrefab in plant.m_grownPrefabs) {
                    AddResult(grownPrefab, Amount.One, new Amount(1, 1f / plant.m_grownPrefabs.Length), 1, prefab.name);
                }

                Indexing.DisableItem(prefab.name, "Plant has grownPrefabs");
            } else {
                AddResult(prefab, Amount.One, Amount.One, 1, prefab.name);
            }
        }

        public RecipeInfo(GameObject prefab, Pickable pickable) : this() {
            AddIngredient(prefab, Amount.One, Amount.One, 1, prefab.name);
            AddResult(pickable.m_itemPrefab, Amount.One, new Amount(pickable.m_amount), 1, pickable.name);

            if (pickable.m_extraDrops != null && pickable.m_extraDrops.m_drops.Count > 0) {
                AddDropTable(prefab, pickable.m_extraDrops);
            }

            CombineGroupAmounts(Results);
        }

        public RecipeInfo(GameObject from, DropTable dropTable) : this() {
            AddIngredient(from, Amount.One, Amount.One, 1, from.name);
            AddDropTable(from, dropTable);

            if (dropTable.m_drops != null && dropTable.m_drops.Count == 0) {
                Indexing.DisableItem(from.name, "No drops in DropTable");
            }
        }

        public RecipeInfo(SpawnArea spawnArea) : this() {
            AddIngredient(spawnArea, Amount.One, Amount.One, 1, spawnArea.name);
            foreach (SpawnArea.SpawnData spawnData in spawnArea.m_prefabs) {
                AddResult(spawnData.m_prefab, Amount.One, new Amount(1, 1f / spawnArea.m_prefabs.Count), 1, spawnArea.name);
            }
        }

        public RecipeInfo(Destructible spawnArea) : this() {
            AddIngredient(spawnArea, Amount.One, Amount.One, 1, spawnArea.name);
            AddResult(spawnArea.m_spawnWhenDestroyed, Amount.One, Amount.One, 1, spawnArea.name);
        }

        private void AddTreeLog(GameObject logPrefab, int depth, string name, int treeCount) {
            if (depth > 50) {
                Log.LogWarning($"TreeLog is recursively {name}");
                return;
            }

            if (logPrefab && logPrefab.TryGetComponent(out TreeLog treeLog)) {
                if (treeLog.m_subLogPoints != null) {
                    AddTreeLog(treeLog.m_subLogPrefab, depth + 1, name, treeLog.m_subLogPoints.Length);
                }

                AddDropTable(treeLog.gameObject, treeLog.m_dropWhenDestroyed, treeCount);
                Indexing.DisableItem(treeLog.gameObject.name, name);
            }
        }

        public RecipeInfo(TreeBase treeBase) : this() {
            AddIngredient(treeBase, Amount.One, Amount.One, 1, treeBase.name);
            AddTreeLog(treeBase.m_logPrefab, 0, treeBase.name, 1);
            AddDropTable(treeBase.gameObject, treeBase.m_dropWhenDestroyed);

            if (treeBase.m_stubPrefab && treeBase.m_stubPrefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                AddDropTable(treeBase.gameObject, dropOnDestroyed.m_dropWhenDestroyed);
                Indexing.DisableItem(treeBase.m_stubPrefab.name, treeBase.gameObject.name);
            }
        }

        public RecipeInfo(Trader trader, Trader.TradeItem tradeItem) : this() {
            AddStation(trader, 1);
            AddIngredient(StoreGui.instance.m_coinPrefab, Amount.One, new Amount(tradeItem.m_price), 1, trader.name);
            AddResult(tradeItem.m_prefab, Amount.One, new Amount(tradeItem.m_stack), 1, trader.name);
        }

        public RecipeInfo(Growup growup) : this() {
            AddIngredient(growup.gameObject, Amount.One, Amount.One, 1, "growup base ");
            AddResult(growup.m_grownPrefab, Amount.One, Amount.One, 1, "growup result");
        }

        private void AddDropTable(GameObject from, DropTable dropTable, int groupMultiplier = 1) {
            Amount tableCount = new Amount(dropTable.m_dropMin * groupMultiplier, dropTable.m_dropMax * groupMultiplier, dropTable.m_dropChance);

            float totalWeight = dropTable.m_drops.Sum(i => i.m_weight);

            foreach (DropTable.DropData drop in dropTable.m_drops) {
                float chance = totalWeight == 0 ? 1 : drop.m_weight / totalWeight;
                AddResult(drop.m_item, tableCount, new Amount(drop.m_stackMin, drop.m_stackMax, chance), 1, from.name);
            }
        }

        public bool IngredientsAndResultSame() {
            return Ingredients.SequenceEqual(Results);
        }

        public void CalculateWidth() {
            const float elementWidth = 50f;
            float width = 0;
            width += Mathf.Max(1, Stations.Count) * elementWidth;

            foreach (KeyValuePair<Amount, List<Part>> pair in Ingredients) {
                if (Ingredients.Count > 1 || pair.Key.min != 1 || pair.Key.max != 1 || Math.Abs(pair.Key.chance - 1f) > 0.01f) {
                    width += elementWidth;
                }

                width += pair.Value.Count * elementWidth;
            }

            foreach (KeyValuePair<Amount, List<Part>> pair in Results) {
                if (Results.Count > 1 || pair.Key.min != 1 || pair.Key.max != 1 || Math.Abs(pair.Key.chance - 1f) > 0.01f) {
                    width += elementWidth;
                }

                width += pair.Value.Count * elementWidth;
            }

            Width = width;
        }

        private void CombineGroupAmounts(Dictionary<Amount, List<Part>> groups) {
            if (groups.Keys.Count <= 1 || groups.Keys.Any(i => Math.Abs(i.chance - groups.Keys.First().chance) > 0.001f)) {
                return;
            }

            if (groups.Values.Count <= 1 || groups.Values.Any(i => !i.SequenceEqual(groups.Values.First()))) {
                return;
            }

            Amount amount = new Amount(groups.Keys.Sum(i => i.min), groups.Keys.Sum(i => i.max), groups.Keys.First().chance);
            List<Part> parts = groups.Values.First();

            groups.Clear();
            groups.Add(amount, parts);
        }

        public bool IsUpgrade(out Item item) {
            List<Part> upgradableIngredients = Ingredients.Values.SelectMany(i => i).Where(i => i.item.maxQuality > 1).ToList();
            List<Part> upgradableResults = Results.Values.SelectMany(i => i).Where(i => i.item.maxQuality > 1).ToList();

            if (!(upgradableIngredients.Count == 1 && upgradableResults.Count == 1)) {
                item = null;
                return false;
            }

            Part ingredient = upgradableIngredients[0];
            Part result = upgradableResults[0];
            item = ingredient.item;
            return ingredient.item == result.item && ingredient.quality < result.quality;
        }
    }
}
