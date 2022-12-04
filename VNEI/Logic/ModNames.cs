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
        private static readonly Dictionary<string, BepInPlugin> SourceMod = new Dictionary<string, BepInPlugin>();

        public static void IndexModNames() {
            foreach (IModPrefab modPrefab in ModQuery.GetPrefabs()) {
                SetModOfPrefab(modPrefab.Prefab.name, modPrefab.SourceMod);
            }
        }

        /// <summary>
        ///     Register the mod of an item. Only needed if not using Jotunn.
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="mod"></param>
        public static void SetModOfPrefab(string prefabName, BepInPlugin mod) {
            if (!SourceMod.ContainsKey(prefabName)) {
                SourceMod[prefabName] = mod;
            }
        }

        public static BepInPlugin GetModByPrefabName(string name) {
            if (SourceMod.TryGetValue(name, out BepInPlugin mod)) {
                return mod;
            }

            return null;
        }
    }
}
