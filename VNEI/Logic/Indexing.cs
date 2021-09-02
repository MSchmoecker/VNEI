using System;
using System.Collections.Generic;
using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace VNEI.Logic {
    public static class Indexing {
        public static Dictionary<int, Item> Items { get; private set; } = new Dictionary<int, Item>();
        public static event Action IndexFinished;

        private static Dictionary<string, BepInPlugin> sourceMod = new Dictionary<string, BepInPlugin>();

        public static void IndexAll() {
            if (Items.Count > 0) {
                return;
            }

            foreach (CustomItem customItem in ModRegistry.GetItems()) {
                sourceMod[customItem.ItemPrefab.name] = customItem.SourceMod;
            }

            foreach (CustomPiece customPiece in ModRegistry.GetPieces()) {
                sourceMod[customPiece.PiecePrefab.name] = customPiece.SourceMod;
            }

            Log.LogInfo("Index prefabs");
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (!(bool)prefab) {
                    Log.LogInfo("prefab is null!");
                    continue;
                }

                string fallbackLocalizedName = string.Empty;
                Sprite icon = null;

                if (prefab.TryGetComponent(out HoverText hoverText)) {
                    fallbackLocalizedName = hoverText.m_text;
                }

                if (prefab.TryGetComponent(out Piece piece)) {
                    AddItem(new Item(prefab.name, piece.m_name, piece.m_description, piece.m_icon, ItemType.Piece, prefab));
                }

                if (prefab.TryGetComponent(out ItemDrop itemDrop)) {
                    ItemDrop.ItemData itemData = itemDrop.m_itemData;

                    if ((bool)itemDrop && itemData.m_shared.m_icons.Length > 0) {
                        icon = itemData.GetIcon();
                    }

                    if (itemData.m_shared.m_damageModifiers == null) {
                        itemData.m_shared.m_damageModifiers = new List<HitData.DamageModPair>();
                        Log.LogWarning($"fixed m_damageModifiers is null for '{prefab.name}'");
                    }

                    AddItem(new Item(prefab.name, itemData.m_shared.m_name, itemData.m_shared.m_description, icon, ItemType.Item, prefab));
                }

                if (prefab.TryGetComponent(out Character character)) {
                    AddItem(new Item(prefab.name, character.m_name, string.Empty, null, ItemType.Creature, prefab));
                }

                if (prefab.TryGetComponent(out MineRock mineRock)) {
                    AddItem(new Item(prefab.name, mineRock.m_name, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                    AddItem(new Item(prefab.name, fallbackLocalizedName, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out Pickable pickable)) {
                    AddItem(new Item(prefab.name, pickable.m_overrideName, string.Empty, icon, ItemType.Undefined, prefab));
                }
            }

            Log.LogInfo("Index Recipes: " + ObjectDB.instance.m_recipes.Count);
            foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
                if (!recipe.m_enabled) {
                    Log.LogInfo($"skipping {recipe.name}: not enabled");
                    continue;
                }

                AddRecipeToItems(new RecipeInfo(recipe));
            }

            Log.LogInfo("Index prefabs Recipes");
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

                if (prefab.TryGetComponent(out CharacterDrop characterDrop) && prefab.TryGetComponent(out Character character)) {
                    AddRecipeToItems(new RecipeInfo(character, characterDrop.m_drops));
                }

                if (prefab.TryGetComponent(out MineRock mineRock)) {
                    AddRecipeToItems(new RecipeInfo(prefab, mineRock.m_dropItems));
                }

                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                    AddRecipeToItems(new RecipeInfo(prefab, dropOnDestroyed.m_dropWhenDestroyed));
                }

                // TODO Source Station listing

                if (prefab.TryGetComponent(out Piece piece)) {
                    AddRecipeToItems(new RecipeInfo(prefab, piece.m_resources));
                }

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

                if (prefab.TryGetComponent(out Pickable pickable)) {
                    AddRecipeToItems(new RecipeInfo(prefab, pickable));
                }
            }

            Log.LogInfo($"Loaded {Items.Count} items");

            IndexFinished?.Invoke();
        }

        private static void AddItem(Item item) {
            if (Plugin.fixPlants.Value) {
                if (item.internalName.ToLower().Contains("sapling_") && !item.internalName.ToLower().Contains("seed")) {
                    return;
                }
            }

            int key = CleanupName(item.internalName).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                Log.LogInfo($"Items contains key already: {CleanupName(item.internalName)}");
            } else {
                Items.Add(key, item);
            }
        }

        public static void ItemObtainedInRecipe(string name, RecipeInfo recipeInfo) {
            int key = CleanupName(name).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                Items[key].result.Add(recipeInfo);
            } else {
                Log.LogInfo($"cannot add recipe to obtaining, '{CleanupName(name)}' is not indexed");
            }
        }

        public static void ItemUsedInRecipe(string name, RecipeInfo recipeInfo) {
            int key = CleanupName(name).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                Items[key].ingredient.Add(recipeInfo);
            } else {
                Log.LogInfo($"cannot add recipe to using, '{CleanupName(name)}' is not indexed");
            }
        }

        public static void AddRecipeToItems(RecipeInfo recipeInfo) {
            foreach (Tuple<Item, Amount> tuple in recipeInfo.ingredient) {
                ItemUsedInRecipe(tuple.Item1.internalName, recipeInfo);
            }

            foreach (Tuple<Item, Amount> tuple in recipeInfo.result) {
                ItemObtainedInRecipe(tuple.Item1.internalName, recipeInfo);
            }

            if (recipeInfo.station != null) {
                ItemUsedInRecipe(recipeInfo.station.internalName, recipeInfo);
            }
        }

        public static void AddConversionRecipe(ItemDrop from, ItemDrop to, RecipeInfo recipeInfo, string name) {
            if ((bool)from) {
                ItemUsedInRecipe(from.name, recipeInfo);
            } else {
                Log.LogInfo($"conversion from is null: {name}");
            }

            if ((bool)to) {
                ItemObtainedInRecipe(to.name, recipeInfo);
            } else {
                Log.LogInfo($"conversion to is null: {name}");
            }
        }

        public static string CleanupName(string name) {
            name = name.Replace("JVLmock_", "");

            if (Plugin.fixPlants.Value) {
                name = name.Replace("sapling_", "");
            }

            return name.ToLower();
        }

        public static BepInPlugin GetModByPrefabName(string name) {
            if (sourceMod.ContainsKey(name)) {
                return sourceMod[name];
            }

            return null;
        }
    }
}
