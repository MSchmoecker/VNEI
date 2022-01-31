using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using VNEIPatcher;

namespace VNEI.Logic {
    public static class Indexing {
        /// <summary>
        ///     Called when trying to index new prefab names.
        ///     Usually use <see cref="AddItem"/> to register a new item.
        ///     If two items have the same name only the first one is added.
        /// </summary>
        public static event Action<GameObject> OnIndexingItems;

        /// <summary>
        ///     Called after added all items. Disabled items are not shown inside any UI.
        ///     Usually use <see cref="DisableItem"/> to disable a given item.
        /// </summary>
        public static event Action<GameObject> OnDisableItems;

        /// <summary>
        ///     Called after adding a Valheim <see cref="Recipe"/>.
        /// </summary>
        public static event Action<Recipe> OnIndexingRecipes;

        /// <summary>
        ///     Called when adding specific item recipes (like conversions or drops).
        ///     Usually use <see cref="AddRecipeToItems"/> with a new <see cref="RecipeInfo"/>, see a full example here
        ///     <code>
        ///     Indexing.OnIndexingItemRecipes += (prefab) => {
        ///         if (prefab.TryGetComponent(out CookingStation cookingStation)) {
        ///             foreach (CookingStation.ItemConversion conversion in cookingStation.m_conversion) {
        ///                 RecipeInfo recipeInfo = new RecipeInfo();
        ///                 recipeInfo.SetStation(cookingStation, 1);
        ///                 recipeInfo.AddIngredient(conversion.m_from, Amount.One, Amount.One, 1, prefab.name);
        ///                 recipeInfo.AddResult(conversion.m_to, Amount.One, Amount.One, 1, prefab.name);
        ///                 Indexing.AddRecipeToItems(recipeInfo);
        ///             }
        ///         }
        ///     };
        ///     </code>
        /// </summary>
        public static event Action<GameObject> OnIndexingItemRecipes;

        public static event Action IndexFinished;

        private static Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
        private static Dictionary<string, BepInPlugin> sourceMod = new Dictionary<string, BepInPlugin>();

        public static void IndexAll() {
            if (HasIndexed()) {
                return;
            }

            foreach (CustomItem customItem in ModRegistry.GetItems()) {
                SetModOfPrefab(customItem.ItemPrefab.name, customItem.SourceMod);
            }

            foreach (CustomPiece customPiece in ModRegistry.GetPieces()) {
                SetModOfPrefab(customPiece.PiecePrefab.name, customPiece.SourceMod);
            }

            foreach (CustomPrefab customPrefab in ModRegistry.GetPrefabs()) {
                SetModOfPrefab(customPrefab.Prefab.name, customPrefab.SourceMod);
            }

            foreach (KeyValuePair<string, Type> pair in VNEIPatcher.VNEIPatcher.sourceMod) {
                Type pluginType = pair.Value.Assembly.DefinedTypes.FirstOrDefault(IsBaseUnityPlugin);

                if (pluginType != null && Chainloader.ManagerObject.TryGetComponent(pluginType, out Component mod)) {
                    SetModOfPrefab(pair.Key, ((BaseUnityPlugin)mod).Info.Metadata);
                }
            }

            Dictionary<string, PieceTable> pieceTables = new Dictionary<string, PieceTable>();

            Log.LogInfo("Index prefabs");

            AddItem(new Item("vnei_any_item", "$vnei_any_item", string.Empty, null, ItemType.Undefined, null));

            // m_prefabs first iteration: base indexing
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (!(bool)prefab) {
                    Log.LogDebug("prefab is null!");
                    continue;
                }

                string fallbackLocalizedName = string.Empty;

                if (prefab.TryGetComponent(out HoverText hoverText)) {
                    fallbackLocalizedName = hoverText.m_text;
                }

                // Treasure Chests are the only none-buildable prefabs that needs to be indexed
                if (prefab.name.StartsWith("TreasureChest")) {
                    TryAddItem<Piece>(prefab, i => i.m_name, ItemType.Piece, i => i.m_description, i => i.m_icon);
                }

                if (prefab.TryGetComponent(out ItemDrop itemDrop)) {
                    ItemDrop.ItemData itemData = itemDrop.m_itemData;
                    Sprite icon = null;

                    if ((bool)itemDrop && itemData.m_shared.m_icons.Length > 0) {
                        icon = itemData.GetIcon();
                    }

                    if (itemData.m_shared.m_damageModifiers == null) {
                        itemData.m_shared.m_damageModifiers = new List<HitData.DamageModPair>();
                        Log.LogWarning($"fixed m_damageModifiers is null for '{prefab.name}'");
                    }

                    ItemType type = ItemTypeHelper.GetItemType(itemData);

                    ItemDrop.ItemData.SharedData shared = itemData.m_shared;
                    AddItem(new Item(prefab.name, shared.m_name, shared.m_description, icon, type, prefab, shared.m_maxQuality));

                    // add pieces here as it is guaranteed they are buildable
                    if ((bool)itemData.m_shared.m_buildPieces) {
                        pieceTables.Add(CleanupName(prefab.name), itemData.m_shared.m_buildPieces);

                        foreach (GameObject buildPiece in itemData.m_shared.m_buildPieces.m_pieces) {
                            TryAddItem<Piece>(buildPiece, i => i.m_name, ItemType.Piece, i => i.m_description, i => i.m_icon);
                        }
                    }
                }

                TryAddItem<Character>(prefab, i => i.m_name, ItemType.Creature);
                TryAddItem<MineRock>(prefab, i => i.m_name, ItemType.Undefined);
                TryAddItem<MineRock5>(prefab, i => i.m_name, ItemType.Undefined);
                TryAddItem<DropOnDestroyed>(prefab, i => fallbackLocalizedName, ItemType.Undefined);
                TryAddItem<Pickable>(prefab, i => i.m_overrideName, ItemType.Undefined);
                TryAddItem<SpawnArea>(prefab, i => fallbackLocalizedName, ItemType.Creature);
                TryAddItem<Destructible>(prefab, i => fallbackLocalizedName, ItemType.Undefined);
                TryAddItem<TreeLog>(prefab, i => fallbackLocalizedName, ItemType.Undefined);
                TryAddItem<TreeBase>(prefab, i => fallbackLocalizedName, ItemType.Undefined);
                TryAddItem<Trader>(prefab, i => i.m_name, ItemType.Undefined);

                try {
                    OnIndexingItems?.Invoke(prefab);
                } catch (Exception e) {
                    Log.LogError(e);
                }
            }

            // m_prefabs second iteration: disable prefabs
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Piece piece)) {
                    if (GetItem(prefab.name) == null) {
                        Log.LogDebug($"not indexed piece {piece.name}: not buildable");
                    }
                }

                if (prefab.TryGetComponent(out Humanoid humanoid) && !(humanoid is Player)) {
                    foreach (GameObject defaultItem in humanoid.m_defaultItems) {
                        if (!(bool)defaultItem) continue;
                        DisableItem(defaultItem.name, $"is defaultItem from {prefab.name}");
                    }

                    foreach (GameObject weapon in humanoid.m_randomWeapon) {
                        if (!(bool)weapon) continue;
                        DisableItem(weapon.name, $"is weapon from {prefab.name}");
                    }

                    foreach (GameObject shield in humanoid.m_randomShield) {
                        if (!(bool)shield) continue;
                        DisableItem(shield.name, $"is shield from {prefab.name}");
                    }

                    foreach (GameObject armour in humanoid.m_randomArmor) {
                        if (!(bool)armour) continue;
                        DisableItem(armour.name, $"is armour from {prefab.name}");
                    }

                    foreach (Humanoid.ItemSet set in humanoid.m_randomSets) {
                        if (set?.m_items == null) continue;

                        foreach (GameObject item in set.m_items) {
                            if (!(bool)item) continue;
                            DisableItem(item.name, $"is randomSet item from {prefab.name}");
                        }
                    }
                }

                try {
                    OnDisableItems?.Invoke(prefab);
                } catch (Exception e) {
                    Log.LogError(e);
                }
            }

            Log.LogInfo($"Index recipes");
            foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
                if (!recipe.m_enabled) {
                    Log.LogDebug($"skipping {recipe.name}: not enabled");
                    continue;
                }

                if (!(bool)recipe.m_item) {
                    Log.LogDebug($"skipping {recipe.name}: item is null");
                    continue;
                }

                for (int quality = 1; quality <= recipe.m_item.m_itemData.m_shared.m_maxQuality; quality++) {
                    AddRecipeToItems(new RecipeInfo(recipe, quality));
                }

                try {
                    OnIndexingRecipes?.Invoke(recipe);
                } catch (Exception e) {
                    Log.LogError(e);
                }
            }

            Log.LogInfo("Index prefabs recipes");
            // m_prefabs third iteration: recipes
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Smelter smelter)) {
                    foreach (Smelter.ItemConversion conversion in smelter.m_conversion) {
                        AddRecipeToItems(new RecipeInfo(conversion, smelter));
                    }
                }

                if (prefab.TryGetComponent(out Fermenter fermenter)) {
                    foreach (Fermenter.ItemConversion conversion in fermenter.m_conversion) {
                        AddRecipeToItems(new RecipeInfo(conversion, fermenter));
                    }
                }

                if (prefab.TryGetComponent(out CookingStation cookingStation)) {
                    foreach (CookingStation.ItemConversion conversion in cookingStation.m_conversion) {
                        AddRecipeToItems(new RecipeInfo(conversion, cookingStation));
                    }
                }

                if (prefab.TryGetComponent(out Incinerator incinerator)) {
                    AddRecipeToItems(new RecipeInfo(incinerator));

                    foreach (Incinerator.IncineratorConversion conversion in incinerator.m_conversions) {
                        if (conversion.m_requireOnlyOneIngredient) {
                            foreach (Incinerator.Requirement requirement in conversion.m_requirements) {
                                AddRecipeToItems(new RecipeInfo(conversion, requirement, incinerator));
                            }
                        } else {
                            AddRecipeToItems(new RecipeInfo(conversion, incinerator));
                        }
                    }
                }

                TryAddRecipeToItems<CharacterDrop>(prefab, i => new RecipeInfo(i));
                TryAddRecipeToItems<MineRock>(prefab, i => new RecipeInfo(prefab, i.m_dropItems));
                TryAddRecipeToItems<MineRock5>(prefab, i => new RecipeInfo(prefab, i.m_dropItems));
                TryAddRecipeToItems<DropOnDestroyed>(prefab, i => new RecipeInfo(prefab, i.m_dropWhenDestroyed));

                prefab.TryGetComponent(out Piece piece);

                if ((bool)piece && prefab.TryGetComponent(out Plant plant)) {
                    foreach (GameObject grownPrefab in plant.m_grownPrefabs) {
                        if (grownPrefab.TryGetComponent(out Pickable pickablePlant)) {
                            RecipeInfo recipeInfo = new RecipeInfo(prefab, pickablePlant);
                            if (!recipeInfo.IngredientsAndResultSame()) {
                                ItemUsedInRecipe(prefab.name, recipeInfo);
                                ItemObtainedInRecipe(pickablePlant.m_itemPrefab.name, recipeInfo);
                            }
                        }
                    }
                }

                if ((bool)piece && prefab.TryGetComponent(out Container container)) {
                    if (container.m_defaultItems.m_drops.Count > 0) {
                        AddRecipeToItems(new RecipeInfo(container.gameObject, container.m_defaultItems));
                    }
                }

                TryAddRecipeToItems<Pickable>(prefab, i => new RecipeInfo(prefab, i));
                TryAddRecipeToItems<SpawnArea>(prefab, i => new RecipeInfo(i));

                if (prefab.TryGetComponent(out Destructible destructible) && destructible.m_spawnWhenDestroyed != null) {
                    AddRecipeToItems(new RecipeInfo(destructible));
                }

                TryAddRecipeToItems<TreeLog>(prefab, i => new RecipeInfo(i));
                TryAddRecipeToItems<TreeBase>(prefab, i => new RecipeInfo(i));

                if (prefab.TryGetComponent(out Trader trader)) {
                    foreach (Trader.TradeItem tradeItem in trader.m_items) {
                        AddRecipeToItems(new RecipeInfo(trader, tradeItem));
                    }
                }

                try {
                    OnIndexingItemRecipes?.Invoke(prefab);
                } catch (Exception e) {
                    Log.LogError(e);
                }
            }

            foreach (KeyValuePair<string, PieceTable> pair in pieceTables) {
                foreach (GameObject prefab in pair.Value.m_pieces) {
                    if (prefab.TryGetComponent(out Piece piece)) {
                        AddRecipeToItems(new RecipeInfo(prefab, piece, GetItem(pair.Key)));
                    }
                }
            }

            RecipeInfo.OnCalculateIsOnBlacklist?.Invoke();

            Log.LogInfo($"Loaded {GetActiveItems().Count()} items");

            try {
                IndexFinished?.Invoke();
            } catch (Exception e) {
                Log.LogError(e);
            }
        }

        private static void TryAddItem<T>(GameObject target, Func<T, string> getName, ItemType itemType, Func<T, string> getDescription = null, Func<T, Sprite> getIcon = null) where T : Component {
            if (!target.TryGetComponent(out T component)) {
                return;
            }

            try {
                string description = getDescription?.Invoke(component) ?? string.Empty;
                Sprite icon = getIcon?.Invoke(component);
                Item item = new Item(target.name, getName(component), description, icon, itemType, target);
                AddItem(item);
            } catch (Exception e) {
                Log.LogError(target.name + Environment.NewLine + e);
            }
        }

        private static void TryAddRecipeToItems<T>(GameObject target, Func<T, RecipeInfo> getRecipe) where T : Component {
            if (!target.TryGetComponent(out T component)) {
                return;
            }

            try {
                AddRecipeToItems(getRecipe(component));
            } catch (Exception e) {
                Log.LogError(target.name + Environment.NewLine + e);
            }
        }

        public static bool HasIndexed() => Items.Count > 0;

        public static void DisableItem(string name, string context) {
            Item item = GetItem(name);

            if (item != null) {
                item.isActive = false;
                Log.LogDebug($"disabling {name}: {context}");
            }
        }

        public static void AddItem(Item item) {
            if (Plugin.fixPlants.Value) {
                if (item.internalName.ToLower().Contains("sapling_") && !item.internalName.ToLower().Contains("seed")) {
                    return;
                }
            }

            int key = CleanupName(item.internalName).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                Log.LogDebug($"Items contains key already: {CleanupName(item.internalName)}");
            } else {
                Items.Add(key, item);
            }
        }

        public static void ItemObtainedInRecipe(string name, RecipeInfo recipeInfo) {
            Item item = GetItem(name);

            if (item != null) {
                item.result.Add(recipeInfo);
            } else {
                Log.LogDebug($"cannot add recipe to obtaining, '{CleanupName(name)}' is not indexed");
            }
        }

        public static void ItemUsedInRecipe(string name, RecipeInfo recipeInfo) {
            Item item = GetItem(name);

            if (item != null) {
                item.ingredient.Add(recipeInfo);
            } else {
                Log.LogDebug($"cannot add recipe to using, '{CleanupName(name)}' is not indexed");
            }
        }

        public static void AddRecipeToItems(RecipeInfo recipeInfo) {
            foreach (Part part in recipeInfo.ingredient.SelectMany(i => i.Value)) {
                ItemUsedInRecipe(part.item.internalName, recipeInfo);
            }

            foreach (Part part in recipeInfo.result.SelectMany(i => i.Value)) {
                ItemObtainedInRecipe(part.item.internalName, recipeInfo);
            }

            if (recipeInfo.station != null) {
                ItemUsedInRecipe(recipeInfo.station.item.internalName, recipeInfo);
            }
        }

        public static string CleanupName(string name) {
            name = name.Replace("JVLmock_", "");

            if (Plugin.fixPlants.Value) {
                name = name.Replace("sapling_", "");
            }

            return name.ToLower();
        }

        /// <summary>
        ///     Register the mod of an item. Only needed if not using Jotunn.
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="mod"></param>
        public static void SetModOfPrefab(string prefabName, BepInPlugin mod) {
            if (!sourceMod.ContainsKey(prefabName)) {
                sourceMod[prefabName] = mod;
            }
        }

        public static BepInPlugin GetModByPrefabName(string name) {
            if (sourceMod.ContainsKey(name)) {
                return sourceMod[name];
            }

            return null;
        }

        public static IEnumerable<KeyValuePair<int, Item>> GetActiveItems() {
            return Items.Where(i => i.Value.isActive);
        }

        public static Item GetItem(string name) {
            int key = CleanupName(name).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                return Items[key];
            }

            Log.LogDebug($"cannot get item: '{CleanupName(name)}' is not indexed");
            return null;
        }

        private static bool IsBaseUnityPlugin(Type t) {
            return t.IsClass && t.Assembly != typeof(Plugin).Assembly && typeof(BaseUnityPlugin).IsAssignableFrom(t);
        }
    }
}
