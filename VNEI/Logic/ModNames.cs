using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Jotunn.Entities;
using Jotunn.Utils;
using UnityEngine;

namespace VNEI.Logic {
    public class ModNames {
        private static Dictionary<string, BepInPlugin> sourceMod = new Dictionary<string, BepInPlugin>();

        public static void IndexModNames() {
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

        private static bool IsBaseUnityPlugin(Type t) {
            return t.IsClass && t.Assembly != typeof(Plugin).Assembly && typeof(BaseUnityPlugin).IsAssignableFrom(t);
        }
    }
}
