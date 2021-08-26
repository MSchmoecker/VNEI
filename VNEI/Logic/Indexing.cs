﻿using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace VNEI.Logic {
    public static class Indexing {
        public static Dictionary<int, Item> Items { get; private set; } = new Dictionary<int, Item>();
        public static event Action IndexFinished;

        public static void IndexAll() {
            PrefabManager.OnPrefabsRegistered -= IndexAll;

            Log.LogInfo("Index prefabs");
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
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
            }

            Log.LogInfo("Index Recipes: " + ObjectDB.instance.m_recipes.Count);
            foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
                if ((bool)recipe.m_item) {
                    AddResultToItem(recipe.m_item.name, new RecipeInfo(recipe));
                }

                foreach (Piece.Requirement resource in recipe.m_resources) {
                    if ((bool)resource.m_resItem) {
                        AddIngredientToItem(resource.m_resItem.name, new RecipeInfo(recipe));
                    }
                }
            }

            Log.LogInfo("Index Smelter, Fermenter, CookingStation, CharacterDrop");
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
                        AddConversionRecipe(conversion.m_from, conversion.m_to, new RecipeInfo(conversion, cookingStation.name), cookingStation.name);
                    }
                }

                if (prefab.TryGetComponent(out CharacterDrop characterDrop) && prefab.TryGetComponent(out Character character)) {
                    RecipeInfo recipeInfo = new RecipeInfo(character, characterDrop.m_drops);
                    AddIngredientToItem(prefab.name, recipeInfo);

                    foreach (CharacterDrop.Drop drop in characterDrop.m_drops) {
                        AddResultToItem(drop.m_prefab.name, recipeInfo);
                    }
                }

                // TODO Source Station listing
            }

            Log.LogInfo("Index prefabs Recipes");
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Piece piece)) {
                    RecipeInfo recipeInfo = new RecipeInfo(prefab, piece.m_resources);
                    AddResultToItem(prefab.name, recipeInfo);

                    foreach (Piece.Requirement resource in piece.m_resources) {
                        AddIngredientToItem(resource.m_resItem.name, recipeInfo);
                    }
                }
            }

            IndexFinished?.Invoke();
        }

        private static void AddItem(string name, string localizeName, string description, Sprite[] icons, GameObject prefab) {
            int key = CleanupName(name).GetStableHashCode();
            Item item;

            if (Items.ContainsKey(key)) {
                item = Items[key];
            } else {
                item = new Item();
                Items.Add(key, item);
            }

            item.internalName = prefab.name;
            item.localizedName = localizeName.Length > 0 ? Localization.instance.Localize(localizeName) : item.localizedName;
            item.description = description.Length > 0 ? description : item.description;
            item.icons = icons != null && icons.Length > 0 ? icons : item.icons;
            item.gameObject = prefab;
        }

        public static void AddResultToItem(string name, RecipeInfo recipeInfo) {
            int key = CleanupName(name).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                Items[key].result.Add(recipeInfo);
            } else {
                Log.LogInfo($"cannot add recipeInfo {recipeInfo.name} to result, {name} is not indexed");
            }
        }

        public static void AddIngredientToItem(string name, RecipeInfo recipeInfo) {
            int key = CleanupName(name).GetStableHashCode();

            if (Items.ContainsKey(key)) {
                Items[key].ingredient.Add(recipeInfo);
            } else {
                Log.LogInfo($"cannot add recipeInfo {recipeInfo.name} to ingredient, {name} is not indexed");
            }
        }

        public static void AddConversionRecipe(ItemDrop from, ItemDrop to, RecipeInfo recipeInfo, string name) {
            if ((bool)from) {
                AddIngredientToItem(from.name, recipeInfo);
            } else {
                Log.LogInfo($"conversion from is null: {name}");
            }

            if ((bool)to) {
                AddResultToItem(to.name, recipeInfo);
            } else {
                Log.LogInfo($"conversion to is null: {name}");
            }
        }

        public static string CleanupName(string name) {
            return name.Replace("JVLmock_", "");
        }
    }
}
