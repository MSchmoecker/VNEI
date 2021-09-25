using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace VNEI.Logic {
    public static class Indexing {
        public static event Action IndexFinished;
        public static readonly Queue<string> ToRenderSprite = new Queue<string>();

        private static Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
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

            Dictionary<string, PieceTable> pieceTables = new Dictionary<string, PieceTable>();

            Log.LogInfo("Index prefabs");

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
                if (prefab.TryGetComponent(out Piece piece) && prefab.name.StartsWith("TreasureChest")) {
                    AddItem(new Item(prefab.name, piece.m_name, piece.m_description, piece.m_icon, ItemType.Piece, prefab));
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

                    AddItem(new Item(prefab.name, itemData.m_shared.m_name, itemData.m_shared.m_description, icon, ItemType.Item, prefab));

                    // add pieces here as it is guaranteed they are buildable
                    if ((bool)itemData.m_shared.m_buildPieces) {
                        pieceTables.Add(CleanupName(prefab.name), itemData.m_shared.m_buildPieces);

                        foreach (GameObject buildPiece in itemData.m_shared.m_buildPieces.m_pieces) {
                            if (!buildPiece.TryGetComponent(out Piece p)) {
                                continue;
                            }

                            AddItem(new Item(buildPiece.name, p.m_name, p.m_description, p.m_icon, ItemType.Piece, buildPiece));
                        }
                    }
                }

                if (prefab.TryGetComponent(out Character character)) {
                    AddItem(new Item(prefab.name, character.m_name, string.Empty, null, ItemType.Creature, prefab));
                }

                if (prefab.TryGetComponent(out MineRock mineRock)) {
                    AddItem(new Item(prefab.name, mineRock.m_name, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out MineRock5 mineRock5)) {
                    AddItem(new Item(prefab.name, mineRock5.m_name, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                    AddItem(new Item(prefab.name, fallbackLocalizedName, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out Pickable pickable)) {
                    AddItem(new Item(prefab.name, pickable.m_overrideName, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out SpawnArea spawnArea)) {
                    AddItem(new Item(prefab.name, fallbackLocalizedName, string.Empty, null, ItemType.Creature, prefab));
                }

                if (prefab.TryGetComponent(out Destructible destructible)) {
                    AddItem(new Item(prefab.name, fallbackLocalizedName, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out TreeLog treeLog)) {
                    AddItem(new Item(prefab.name, fallbackLocalizedName, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out TreeBase treeBase)) {
                    AddItem(new Item(prefab.name, fallbackLocalizedName, string.Empty, null, ItemType.Undefined, prefab));
                }

                if (prefab.TryGetComponent(out Trader trader)) {
                    AddItem(new Item(prefab.name, trader.m_name, string.Empty, null, ItemType.Undefined, prefab));
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

                if (prefab.TryGetComponent(out CharacterDrop characterDrop) && prefab.TryGetComponent(out Character character)) {
                    AddRecipeToItems(new RecipeInfo(character, characterDrop.m_drops));
                }

                if (prefab.TryGetComponent(out MineRock mineRock)) {
                    AddRecipeToItems(new RecipeInfo(prefab, mineRock.m_dropItems));
                }

                if (prefab.TryGetComponent(out MineRock5 mineRock5)) {
                    AddRecipeToItems(new RecipeInfo(prefab, mineRock5.m_dropItems));
                }

                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                    AddRecipeToItems(new RecipeInfo(prefab, dropOnDestroyed.m_dropWhenDestroyed));
                }

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

                if (prefab.TryGetComponent(out Pickable pickable)) {
                    AddRecipeToItems(new RecipeInfo(prefab, pickable));
                }

                if (prefab.TryGetComponent(out SpawnArea spawnArea)) {
                    AddRecipeToItems(new RecipeInfo(spawnArea));
                }

                if (prefab.TryGetComponent(out Destructible destructible) && destructible.m_spawnWhenDestroyed != null) {
                    AddRecipeToItems(new RecipeInfo(destructible));
                }

                if (prefab.TryGetComponent(out TreeLog treeLog)) {
                    AddRecipeToItems(new RecipeInfo(treeLog));
                }

                if (prefab.TryGetComponent(out TreeBase treeBase)) {
                    AddRecipeToItems(new RecipeInfo(treeBase));
                }

                if (prefab.TryGetComponent(out Trader trader)) {
                    foreach (Trader.TradeItem tradeItem in trader.m_items) {
                        AddRecipeToItems(new RecipeInfo(trader, tradeItem));
                    }
                }
            }

            foreach (KeyValuePair<string, PieceTable> pair in pieceTables) {
                foreach (GameObject prefab in pair.Value.m_pieces) {
                    if (prefab.TryGetComponent(out Piece piece)) {
                        AddRecipeToItems(new RecipeInfo(prefab, piece, GetItem(pair.Key)));
                    }
                }
            }

            Log.LogInfo($"Loaded {GetActiveItems().Count()} items");

            if (!(bool)RenderSprites.instance) {
                new GameObject("RenderSprites", typeof(RenderSprites));
            }

            RenderSprites.instance.StartRender();

            IndexFinished?.Invoke();
        }

        private static void DisableItem(string name, string context) {
            Item item = GetItem(name);

            if (item != null) {
                item.isActive = false;
                Log.LogDebug($"disabling {name}: {context}");
            }
        }

        private static void AddItem(Item item) {
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

        public static void AddConversionRecipe(ItemDrop from, ItemDrop to, RecipeInfo recipeInfo, string name) {
            if ((bool)from) {
                ItemUsedInRecipe(from.name, recipeInfo);
            } else {
                Log.LogDebug($"conversion from is null: {name}");
            }

            if ((bool)to) {
                ItemObtainedInRecipe(to.name, recipeInfo);
            } else {
                Log.LogDebug($"conversion to is null: {name}");
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
    }
}
