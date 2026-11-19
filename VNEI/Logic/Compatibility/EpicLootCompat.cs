using System.Collections.Generic;
using BepInEx.Bootstrap;
using EpicLoot.Crafting;
using EpicLoot.CraftingV2;

namespace VNEI.Logic.Compatibility {
    public static class EpicLootCompat {
        public static void Init() {
            if (!Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot")) {
                return;
            }

            Indexing.AfterIndexingRecipes += IndexMaterialConversions;
            Indexing.AfterIndexingRecipes += IndexSacrifices;
        }

        private static void IndexMaterialConversions() {
            foreach (var pair in MaterialConversions.Conversions) {
                foreach (var conversion in pair.Value) {
                    List<(string, int)> ingredients = new List<(string, int)>();

                    foreach (var resource in conversion.Resources) {
                        ingredients.Add((resource.Item, resource.Amount));
                    }

                    Indexing.AddRecipeToItems(
                        new RecipeInfo(
                            ingredientAmount: Amount.One,
                            ingredients: ingredients,
                            resultAmount: Amount.One,
                            results: new[] { (conversion.Product, conversion.Amount) },
                            station: "piece_enchantingtable"
                        )
                    );
                }
            }
        }

        private static void IndexSacrifices() {
            foreach (var disenchantProductsConfig in EnchantCostsHelper.Config.DisenchantProducts) {
                if (disenchantProductsConfig.ItemNames.Count > 0) {
                    List<(string, int)> products = new List<(string, int)>();

                    foreach (var product in disenchantProductsConfig.Products) {
                        products.Add((product.Item, product.Amount));
                    }

                    foreach (var itemName in disenchantProductsConfig.ItemNames) {
                        Indexing.AddRecipeToItems(
                            new RecipeInfo(
                                ingredientAmount: Amount.One,
                                ingredients: new[] { (itemName, 1) },
                                resultAmount: Amount.One,
                                results: products,
                                station: "piece_enchantingtable"
                            )
                        );
                    }
                }
            }
        }
    }
}
