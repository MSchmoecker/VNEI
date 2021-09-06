using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jotunn;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VNEI.Logic {
    public class RenderSprites : MonoBehaviour {
        public static RenderSprites instance;
        private static Camera renderer;
        private static int layer = 3;
        private bool hasSetup;

        private bool spawningIsRunning;
        private GameObject currentSpawn;
        private static Vector3 spawnPoint = new Vector3(1000f, 1000f, 1000f);

        private void Awake() {
            instance = this;
        }

        private void Update() {
            if ((bool)Player.m_localPlayer) {
                if (Indexing.ToRenderSprite.Count > 0) {
                    if (!hasSetup) {
                        Log.LogInfo("Render all missing sprites");
                        Setup();
                    }

                    string prefabName = Indexing.ToRenderSprite.Peek();

                    if (!(bool)currentSpawn) {
                        if (!spawningIsRunning) {
                            StartCoroutine(StartSpawnSafe(ZNetScene.instance.GetPrefab(prefabName)));
                        }

                        return;
                    }

                    Indexing.ToRenderSprite.Dequeue();
                    RenderSpriteFromPrefab(prefabName);
                    currentSpawn = null;
                } else {
                    if (hasSetup) {
                        Clear();
                    }
                }
            }
        }

        void Setup() {
            hasSetup = true;

            renderer = new GameObject("Render Camera", typeof(Camera)).GetComponent<Camera>();
            Log.LogInfo("Created renderer");

            renderer.backgroundColor = new Color(0, 0, 0, 0);
            renderer.clearFlags = CameraClearFlags.SolidColor;
            renderer.transform.position = spawnPoint + new Vector3(0, 0, 0);
            renderer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            renderer.cullingMask = 1 << layer;
            Log.LogInfo("Setup camera");
        }

        void Clear() {
            hasSetup = false;

            Destroy(renderer.gameObject);
            Log.LogInfo("Destroyed Camera");
        }

        private void RenderSpriteFromPrefab(string prefabName) {
            RenderTexture oldRenderTexture = RenderTexture.active;
            renderer.targetTexture = RenderTexture.GetTemporary(128, 128, 32);
            RenderTexture.active = renderer.targetTexture;
            Log.LogInfo("Setup Render Texture");

            SetLayerRecursive(currentSpawn.transform, layer);
            currentSpawn.SetActive(true);

            renderer.Render();
            Log.LogInfo($"Rendered {prefabName}");

            currentSpawn.SetActive(false);
            Destroy(currentSpawn);

            RenderTexture targetTexture = renderer.targetTexture;
            Texture2D previewImage = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
            previewImage.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            previewImage.Apply();

            RenderTexture.active = oldRenderTexture;

            Sprite sprite = Sprite.Create(previewImage, new Rect(0, 0, previewImage.width, previewImage.height), new Vector2(0.5f, 0.5f));
            Indexing.Items[Indexing.CleanupName(prefabName).GetStableHashCode()].SetIcon(sprite);

            string dir = BepInEx.Paths.PluginPath + "/VNEI-Out/";
            string path = dir + prefabName + ".png";
            Log.LogInfo(path);

            Directory.CreateDirectory(dir);

            using (FileStream fileStream = File.Create(path)) {
                byte[] bytes = previewImage.EncodeToPNG();
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }

        private static void SetLayerRecursive(Transform transform, int layer) {
            for (int i = 0; i < transform.childCount; i++) {
                SetLayerRecursive(transform.GetChild(i), layer);
            }

            transform.gameObject.layer = layer;
        }

        private IEnumerator StartSpawnSafe(GameObject prefab) {
            spawningIsRunning = true;
            currentSpawn = SpawnSafe(prefab);
            // wait for destroyed components really be destroyed
            yield return null;
            spawningIsRunning = false;
        }

        private static GameObject SpawnSafe(GameObject prefab) {
            bool wasActive = prefab.activeSelf;
            bool wasForceDisableInit = ZNetView.m_forceDisableInit;

            prefab.SetActive(false);
            ZNetView.m_forceDisableInit = true;

            GameObject spawn = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawn.transform.rotation = Quaternion.Euler(0, -30f, 0);

            Vector3 min = new Vector3(100, 100, 100);
            Vector3 max = new Vector3(-100, -100, -100);
            Vector3 size = new Vector3(0, 0, 0);

            foreach (Renderer meshRenderer in spawn.GetComponentsInChildren<Renderer>()) {
                min = Vector3.Min(min, meshRenderer.bounds.min);
                max = Vector3.Max(max, meshRenderer.bounds.max);
                size = Vector3.Max(size, meshRenderer.bounds.size);
            }

            min.y *= -1f;
            max.y *= -1f;

            spawn.transform.position = spawnPoint + (min + max) / 2f + Vector3.back * (1f + size.magnitude * 0.5f);

            // needs to be destroyed first as Character depend on it
            foreach (CharacterDrop characterDrop in spawn.GetComponentsInChildren<CharacterDrop>()) {
                Destroy(characterDrop);
            }

            // needs to be destroyed first as Rigidbody depend on it
            foreach (FixedJoint fixedJoint in spawn.GetComponentsInChildren<FixedJoint>()) {
                Destroy(fixedJoint);
            }

            // needs to be destroyed first as Rigidbody depend on it
            foreach (ConfigurableJoint fixedJoint in spawn.GetComponentsInChildren<ConfigurableJoint>()) {
                Destroy(fixedJoint);
            }

            // destroy all other components
            foreach (Component component in spawn.GetComponentsInChildren<Component>()) {
                if (component is Transform) continue;
                if (component is SkinnedMeshRenderer) continue;
                if (component is MeshRenderer) continue;
                if (component is MeshFilter) continue;

                Destroy(component);
            }

            // // just in case it doesn't gets deleted properly later
            TimedDestruction timedDestruction = spawn.AddComponent<TimedDestruction>();
            timedDestruction.Trigger(1f);

            prefab.SetActive(wasActive);
            ZNetView.m_forceDisableInit = wasForceDisableInit;

            return spawn;
        }
    }
}
