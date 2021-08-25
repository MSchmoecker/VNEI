using System;
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

                if (prefab.TryGetComponent(out Character character)) {
                    Item item = new Item() {
                        internalName = prefab.name,
                        localizedName = Localization.instance.Localize(character.m_name),
                        description = "",
                        icons = Array.Empty<Sprite>(),
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
                                Items[conversion.m_from.name.GetStableHashCode()].ingredient.Add(new RecipeInfo(conversion));
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
                                Items[conversion.m_from.name.GetStableHashCode()].ingredient.Add(new RecipeInfo(conversion));
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
                                Items[conversion.m_from.name.GetStableHashCode()].ingredient.Add(new RecipeInfo(conversion));
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

                if (prefab.TryGetComponent(out CharacterDrop characterDrop) && prefab.TryGetComponent(out Character character)) {
                    Items[prefab.name.GetStableHashCode()].ingredient.Add(new RecipeInfo(character, characterDrop.m_drops));

                    foreach (CharacterDrop.Drop drop in characterDrop.m_drops) {
                        Items[drop.m_prefab.name.GetStableHashCode()].result.Add(new RecipeInfo(character, characterDrop.m_drops));
                    }
                }

                // TODO Source Station listing
                // TODO Search
            }

            foreach (GameObject prefab in ZNetScene.instance.m_prefabs) {
                if (prefab.TryGetComponent(out Piece piece)) {
                    Item item = Items[prefab.name.GetStableHashCode()];
                    RecipeInfo recipeInfo = new RecipeInfo(prefab, piece.m_resources);
                    item.result.Add(recipeInfo);

                    foreach (Piece.Requirement resource in piece.m_resources) {
                        int key = GetRequirementName(resource).GetStableHashCode();
                        if (Items.ContainsKey(key)) {
                            Items[key].ingredient.Add(recipeInfo);
                        } else {
                            Log.LogInfo($"Piece ingredient not indexed: {resource.m_resItem.name}");
                        }
                    }
                }
            }

            IndexFinished?.Invoke();
        }

        public static string GetRequirementName(Piece.Requirement requirement) {
            return requirement.m_resItem.name.Replace("JVLmock_", "");
        }
    }
}
