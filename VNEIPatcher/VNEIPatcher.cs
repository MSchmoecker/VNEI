using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Mono.Cecil;
using UnityEngine;
using System.Linq;

namespace VNEIPatcher {
    public class VNEIPatcher {
        public static IEnumerable<string> TargetDLLs { get; } = Array.Empty<string>();

        public const string Version = "0.1.0";

        public static readonly Dictionary<string, Type> sourceMod = new Dictionary<string, Type>();

        private static Harmony harmony;

        public static void Patch(AssemblyDefinition assembly) {
        }

        public static void Finish() {
            harmony = new Harmony("com.maxsch.valheim.vnei.patcher");
            // cannot patch AssetBundleHook directly because AssetBundle is not available yet
            harmony.PatchAll(typeof(ChainHook));
        }

        [HarmonyPatch]
        public class ChainHook {
            [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Initialize)), HarmonyPostfix]
            public static void AfterChainloaderInitialize() {
                harmony.PatchAll(typeof(AssetBundleHook));
            }
        }

        [HarmonyPatch]
        public class AssetBundleHook {
            [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadAsset), typeof(string), typeof(Type)), HarmonyPostfix]
            public static void LoadAssetPatch(UnityEngine.Object __result) {
                if (__result == null) {
                    return;
                }

                StackFrame[] stackFrames = new StackTrace().GetFrames() ?? Array.Empty<StackFrame>();
                Type callingType = stackFrames.FirstOrDefault(IsProbabilityModAssembly)?.GetMethod()?.ReflectedType;
                AddMod(__result.name, callingType);
            }

            [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadAllAssets), typeof(Type)), HarmonyPostfix]
            public static void LoadAllAssetsPatch(UnityEngine.Object[] __result) {
                if (__result == null) {
                    return;
                }

                StackFrame[] stackFrames = new StackTrace().GetFrames() ?? Array.Empty<StackFrame>();
                Type callingType = stackFrames.FirstOrDefault(IsProbabilityModAssembly)?.GetMethod()?.ReflectedType;

                foreach (UnityEngine.Object result in __result) {
                    AddMod(result.name, callingType);
                }
            }
        }

        private static bool IsProbabilityModAssembly(StackFrame frame) {
            return frame.GetMethod().ReflectedType?.Assembly != typeof(VNEIPatcher).Assembly &&
                   frame.GetMethod().ReflectedType?.Assembly != typeof(AssetBundle).Assembly;
        }

        private static void AddMod(string prefab, Type type) {
            if (!sourceMod.ContainsKey(prefab)) {
                sourceMod.Add(prefab, type);
            }
        }
    }
}
