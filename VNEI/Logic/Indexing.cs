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

            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Piece piece)) {
                    Item item = new Item() {
                        internalName = prefab.name,
                        localizedName = Localization.instance.Localize(piece.m_name),
                        description = piece.m_description,
                        icons = new[] { piece.m_icon },
                        gameObject = prefab
                    };

                    Items.Add(item.internalName.GetStableHashCode(), item);
                }

                if (prefab.TryGetComponent(out ItemDrop itemDrop)) {
                    Item item = new Item() {
                        internalName = prefab.name,
                        localizedName = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name),
                        description = itemDrop.m_itemData.m_shared.m_description,
                        icons = itemDrop.m_itemData.m_shared.m_icons,
                        gameObject = prefab
                    };

                    Items.Add(item.internalName.GetStableHashCode(), item);
                }
            }

            Log.LogInfo("Index Recipes: " + ObjectDB.instance.m_recipes.Count);
            foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
                if (recipe.m_item && Items.ContainsKey(recipe.m_item.name.GetStableHashCode())) {
                    Items[recipe.m_item.name.GetStableHashCode()].result.Add(new RecipeInfo(recipe));
                }

                foreach (Piece.Requirement resource in recipe.m_resources) {
                    if (resource.m_resItem && Items.ContainsKey(resource.m_resItem.name.GetStableHashCode())) {
                        Items[resource.m_resItem.name.GetStableHashCode()].ingredient.Add(new RecipeInfo(recipe));
                    }
                }
            }

            Log.LogInfo("Index Smelter, Fermenter, CookingStation");
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Smelter smelter)) {
                    foreach (Smelter.ItemConversion conversion in smelter.m_conversion) {
                        if ((bool)conversion.m_from) {
                            if (Items.ContainsKey(conversion.m_from.name.GetStableHashCode())) {
                                Items[conversion.m_from.name.GetStableHashCode()].result.Add(new RecipeInfo(conversion));
                            } else {
                                Log.LogInfo($"item not in index! {conversion.m_from.name}");
                            }
                        } else {
                            Log.LogInfo($"conversion from is null: {smelter.name}");
                        }

                        if ((bool)conversion.m_to) {
                            if (Items.ContainsKey(conversion.m_to.name.GetStableHashCode())) {
                                Items[conversion.m_to.name.GetStableHashCode()].result.Add(new RecipeInfo(conversion));
                            } else {
                                Log.LogInfo($"item not in index! {conversion.m_to.name}");
                            }
                        } else {
                            Log.LogInfo($"conversion to is null: {smelter.name}");
                        }
                    }
                }

                if (prefab.TryGetComponent(out Fermenter fermenter)) {
                    foreach (Fermenter.ItemConversion conversion in fermenter.m_conversion) {
                        if ((bool)conversion.m_from) {
                            if (Items.ContainsKey(conversion.m_from.name.GetStableHashCode())) {
                                Items[conversion.m_from.name.GetStableHashCode()].result.Add(new RecipeInfo(conversion));
                            } else {
                                Log.LogInfo($"item not in index! {conversion.m_from.name}");
                            }
                        } else {
                            Log.LogInfo($"conversion from is null: {fermenter.name}");
                        }

                        if ((bool)conversion.m_to) {
                            if (Items.ContainsKey(conversion.m_to.name.GetStableHashCode())) {
                                Items[conversion.m_to.name.GetStableHashCode()].result.Add(new RecipeInfo(conversion));
                            } else {
                                Log.LogInfo($"item not in index! {conversion.m_to.name}");
                            }
                        } else {
                            Log.LogInfo($"conversion to is null: {fermenter.name}");
                        }
                    }
                }

                if (prefab.TryGetComponent(out CookingStation cookingStation)) {
                    foreach (CookingStation.ItemConversion conversion in cookingStation.m_conversion) {
                        if ((bool)conversion.m_from) {
                            if (Items.ContainsKey(conversion.m_from.name.GetStableHashCode())) {
                                Items[conversion.m_from.name.GetStableHashCode()].result.Add(new RecipeInfo(conversion));
                            } else {
                                Log.LogInfo($"item not in index! {conversion.m_from.name}");
                            }
                        } else {
                            Log.LogInfo($"conversion from is null: {cookingStation.name}");
                        }

                        if ((bool)conversion.m_to) {
                            if (Items.ContainsKey(conversion.m_to.name.GetStableHashCode())) {
                                Items[conversion.m_to.name.GetStableHashCode()].result.Add(new RecipeInfo(conversion));
                            } else {
                                Log.LogInfo($"item not in index! {conversion.m_to.name}");
                            }
                        } else {
                            Log.LogInfo($"conversion to is null: {cookingStation.name}");
                        }
                    }
                }

                // TODO Monster Drops
                // TODO Source Station listing
            }

            IndexFinished?.Invoke();
        }
    }
}