using System;
using System.Collections.Generic;
using System.Linq;
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
                MaterialConversionType type = pair.Key;
                List<MaterialConversion> conversions = pair.Value;

                foreach (var conversion in conversions) {
                    AddMaterialConversion(conversion);
                }
            }
        }

        private static void AddMaterialConversion(MaterialConversion conversion) {
            Indexing.AddRecipeToItems(
                new RecipeInfo(
                    ingredientAmount: Amount.One,
                    ingredients: conversion.Resources.Select(r => (r.Item, r.Amount)),
                    resultAmount: new Amount(conversion.Amount),
                    results: new[] { (conversion.Product, conversion.Amount) },
                    station: "piece_enchantingtable"
                )
            );
        }

        private static void IndexSacrifices() {
            foreach (var disenchantProductsConfig in EnchantCostsHelper.Config.DisenchantProducts) {
                if (disenchantProductsConfig.ItemNames.Count > 0) {
                    foreach (var itemName in disenchantProductsConfig.ItemNames) {
                        AddSacrifice(itemName, disenchantProductsConfig);
                    }
                }
            }
        }

        private static void AddSacrifice(string itemName, DisenchantProductsConfig disenchantProductsConfig) {
            Indexing.AddRecipeToItems(
                new RecipeInfo(
                    ingredientAmount: Amount.One,
                    ingredients: new[] { (itemName, 1) },
                    resultAmount: Amount.One,
                    results: disenchantProductsConfig.Products.Select(p => (p.Item, p.Amount)),
                    station: "piece_enchantingtable"
                )
            );
        }
    }
}
