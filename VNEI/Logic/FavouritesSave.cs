using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;

namespace VNEI.Logic {
    public class FavouritesSave {
        public const string favouritesFileName = Plugin.ModGuid + ".favourites.txt";

        public static string GetFavouritesFilePath() {
            return Path.Combine(Paths.ConfigPath, favouritesFileName);
        }

        public static void Load() {
            string filePath = GetFavouritesFilePath();

            if (!File.Exists(filePath)) {
                return;
            }

            using (StreamReader file = File.OpenText(filePath)) {
                while (!file.EndOfStream) {
                    string prefabName = file.ReadLine();

                    if (string.IsNullOrEmpty(prefabName)) {
                        continue;
                    }

                    Item item = Indexing.GetItem(prefabName);

                    if (item != null) {
                        item.isFavorite = true;
                    }
                }
            }
        }

        public static void Save() {
            using (StreamWriter file = File.CreateText(GetFavouritesFilePath())) {
                foreach (KeyValuePair<int, Item> pair in Indexing.GetActiveItems()) {
                    if (pair.Value.isFavorite) {
                        file.WriteLine(pair.Value.internalName);
                    }
                }
            }
        }
    }
}
