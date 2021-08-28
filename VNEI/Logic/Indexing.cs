using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace VNEI.Logic {
    public static class Indexing {
        public static Dictionary<int, Item> Items { get; private set; } = new Dictionary<int, Item>();
        public static event Action IndexFinished;

        public static void IndexAll() {
            if (Items.Count > 0) {
                return;
            }

            Log.LogInfo("Index prefabs");
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (!(bool)prefab) {
                    Log.LogInfo("prefab is null!");
                    continue;
                }

                string fallbackLocalizedName = string.Empty;
                if (prefab.TryGetComponent(out HoverText hoverText)) {
                    fallbackLocalizedName = hoverText.m_text;
                }

                if (prefab.TryGetComponent(out Piece piece)) {
                    AddItem(prefab.name, piece.m_name, piece.m_description, new[] { piece.m_icon }, prefab);
                }

                if (prefab.TryGetComponent(out ItemDrop itemDrop)) {
                    AddItem(prefab.name, itemDrop.m_itemData.m_shared.m_name, itemDrop.m_itemData.m_shared.m_description,
                            itemDrop.m_itemData.m_shared.m_icons, prefab);
                }

                if (prefab.TryGetComponent(out Character character)) {
                    AddItem(prefab.name, character.m_name, string.Empty, Array.Empty<Sprite>(), prefab);
                }

                if (prefab.TryGetComponent(out MineRock mineRock)) {
                    AddItem(prefab.name, mineRock.m_name, string.Empty, Array.Empty<Sprite>(), prefab);
                }

                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                    AddItem(prefab.name, fallbackLocalizedName, string.Empty, Array.Empty<Sprite>(), prefab);
                }
            }

            Log.LogInfo("Index Recipes: " + ObjectDB.instance.m_recipes.Count);
            foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
                if ((bool)recipe.m_item) {
                    ItemObtainedInRecipe(recipe.m_item.name, new RecipeInfo(recipe));
                }

                foreach (Piece.Requirement resource in recipe.m_resources) {
                    if ((bool)resource.m_resItem) {
                        ItemUsedInRecipe(resource.m_resItem.name, new RecipeInfo(recipe));
                    }
                }
            }

            Log.LogInfo("Index prefabs Recipes");
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Smelter smelter)) {
                    foreach (Smelter.ItemConversion conversion in smelter.m_conversion) {
                        AddConversionRecipe(conversion.m_from, conversion.m_to, new RecipeInfo(conversion, smelter.name), smelter.name);
                    }
                }

                if (prefab.TryGetComponent(out Fermenter fermenter)) {
                    foreach (Fermenter.ItemConversion conversion in fermenter.m_conversion) {
                        AddConversionRecipe(conversion.m_from, conversion.m_to, new RecipeInfo(conversion, fermenter.name), fermenter.name);
                    }
                }

                if (prefab.TryGetComponent(out CookingStation cookingStation)) {
                    foreach (CookingStation.ItemConversion conversion in cookingStation.m_conversion) {
                        AddConversionRecipe(conversion.m_from, conversion.m_to, new RecipeInfo(conversion, cookingStation.name),
                                            cookingStation.name);
                    }
                }

                if (prefab.TryGetComponent(out CharacterDrop characterDrop) && prefab.TryGetComponent(out Character character)) {
                    RecipeInfo recipeInfo = new RecipeInfo(character, characterDrop.m_drops);
                    ItemUsedInRecipe(prefab.name, recipeInfo);

                    foreach (CharacterDrop.Drop drop in characterDrop.m_drops) {
                        ItemObtainedInRecipe(drop.m_prefab.name, recipeInfo);
                    }
                }

                if (prefab.TryGetComponent(out MineRock mineRock)) {
                    RecipeInfo recipeInfo = new RecipeInfo(mineRock, mineRock.m_dropItems);

                    ItemUsedInRecipe(prefab.name, recipeInfo);
                    foreach (DropTable.DropData drop in mineRock.m_dropItems.m_drops) {
                        ItemObtainedInRecipe(drop.m_item.name, recipeInfo);
                    }
                }

                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) {
                    RecipeInfo recipeInfo = new RecipeInfo(dropOnDestroyed, dropOnDestroyed.m_dropWhenDestroyed);
                    ItemUsedInRecipe(prefab.name, recipeInfo);

                    foreach (DropTable.DropData drop in dropOnDestroyed.m_dropWhenDestroyed.m_drops) {
                        ItemObtainedInRecipe(drop.m_item.name, recipeInfo);
                    }
                }

                // TODO Source Station listing

                if (prefab.TryGetComponent(out Piece piece)) {
                    RecipeInfo recipeInfo = new RecipeInfo(prefab, piece.m_resources);
                    ItemObtainedInRecipe(prefab.name, recipeInfo);

                    foreach (Piece.Requirement resource in piece.m_resources) {
                        ItemUsedInRecipe(resource.m_resItem.name, recipeInfo);
                    }
                }

                if ((bool)piece && prefab.TryGetComponent(out Plant plant)) {
                    foreach (GameObject grownPrefab in plant.m_grownPrefabs) {
                        if (grownPrefab.TryGetComponent(out Pickable pickable)) {
                            RecipeInfo recipeInfo = new RecipeInfo(piece, pickable);
                            if (!recipeInfo.IngredientsAndResultSame()) {
                                ItemUsedInRecipe(prefab.name, recipeInfo);
                                ItemObtainedInRecipe(pickable.m_itemPrefab.name, recipeInfo);
                            }
                        }
                    }
                }

                if ((bool)piece && prefab.TryGetComponent(out Container container)) {
                    if (container.m_defaultItems.m_drops.Count > 0) {
                        RecipeInfo recipeInfo = new RecipeInfo(container, container.m_defaultItems);

                        ItemUsedInRecipe(prefab.name, recipeInfo);
                        foreach (DropTable.DropData drop in container.m_defaultItems.m_drops) {
                            ItemObtainedInRecipe(drop.m_item.name, recipeInfo);
                        }
                    }
                }
            }

            Log.LogInfo($"Loaded {Items.Count} items");

            IndexFinished?.Invoke();
        }

        private static void AddItem(string name, string localizeName, string description, Sprite[] icons, GameObject prefab) {
            if (Plugin.fixPlants.Value) {
                if (name.ToLower().Contains("sapling_") && !name.ToLower().Contains("seed")) {
                    return;
                }
            }

            int key = CleanupName(name).GetStableHashCode();
            Item item;

            if (Items.ContainsKey(key)) {
                item = Items[key];
                Log.LogInfo($"Items contains key already: {CleanupName(name)}");
            } else {
                item = new Item();
                Items.Add(key, item);

                item.isOnBlacklist = Plugin.ItemBlacklist.Contains(name);
            }

            item.internalName = prefab.name;
            item.localizedName = localizeName.Length > 0 ? Localization.instance.Localize(localizeName) : item.localizedName;
            item.description = description.Length > 0 ? description : item.description;
            item.icons = icons != null && icons.Length > 0 ? icons : item.icons;
            item.gameObject = prefab;
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
    }
}
