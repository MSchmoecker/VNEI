using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VNEI.Logic {
    public class RenderSprites : MonoBehaviour {
        public static RenderSprites instance;
        private Camera renderer;
        private Light light;
        private const int Layer = 3;

        private static Vector3 spawnPoint = new Vector3(1000f, 1000f, 1000f);

        private void Awake() {
            instance = this;
        }

        public void StartRender() {
            StartCoroutine(RenderAll());
        }

        private void SetupRendering() {
            Log.LogInfo("Setup renderer camera");
            renderer = new GameObject("Render Camera", typeof(Camera)).GetComponent<Camera>();
            renderer.backgroundColor = new Color(0, 0, 0, 0);
            renderer.clearFlags = CameraClearFlags.SolidColor;
            renderer.transform.position = spawnPoint + new Vector3(0, 0, 0);
            renderer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            renderer.cullingMask = 1 << Layer;

            light = new GameObject("Render Light", typeof(Light)).GetComponent<Light>();
            light.transform.position = spawnPoint;
            light.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            light.type = LightType.Directional;
            light.cullingMask = 1 << Layer;
        }

        private void ClearRendering() {
            Log.LogInfo("Destroy renderer camera");
            Destroy(renderer.gameObject);
            Destroy(light.gameObject);
        }

        IEnumerator RenderAll() {
            Queue<GameObject> queue = new Queue<GameObject>();

            while (Indexing.ToRenderSprite.Count > 0) {
                string prefabName = Indexing.ToRenderSprite.Dequeue();
                GameObject spawn = SpawnSafe(ZNetScene.instance.GetPrefab(prefabName));
                queue.Enqueue(spawn);
            }

            // wait for destroyed components really be destroyed
            yield return null;

            SetupRendering();

            while (queue.Count > 0) {
                GameObject currentSpawn = queue.Dequeue();
                RenderSpriteFromPrefab(currentSpawn);
            }

            ClearRendering();
        }

        private void RenderSpriteFromPrefab(GameObject spawn) {
            RenderTexture oldRenderTexture = RenderTexture.active;
            renderer.targetTexture = RenderTexture.GetTemporary(128, 128, 32);
            RenderTexture.active = renderer.targetTexture;

            SetLayerRecursive(spawn.transform, Layer);
            spawn.SetActive(true);

            renderer.Render();
            Log.LogInfo($"Rendered {spawn.name}");

            spawn.SetActive(false);
            Destroy(spawn);

            RenderTexture targetTexture = renderer.targetTexture;
            Texture2D previewImage = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
            previewImage.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            previewImage.Apply();

            RenderTexture.active = oldRenderTexture;

            Sprite sprite = Sprite.Create(previewImage, new Rect(0, 0, previewImage.width, previewImage.height), new Vector2(0.5f, 0.5f));
            Indexing.Items[Indexing.CleanupName(spawn.name).GetStableHashCode()].SetIcon(sprite);

            string dir = $"{BepInEx.Paths.PluginPath}/VNEI-Out";
            string path = $"{dir}/{spawn.name}.png";

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

        private static GameObject SpawnSafe(GameObject prefab) {
            bool wasActive = prefab.activeSelf;
            prefab.SetActive(false);

            GameObject spawn = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            spawn.name = prefab.name;
            spawn.transform.rotation = Quaternion.Euler(0, -30f, 0);

            prefab.SetActive(wasActive);

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
            foreach (Joint joint in spawn.GetComponentsInChildren<Joint>()) {
                Destroy(joint);
            }

            // destroy all other components
            foreach (Component component in spawn.GetComponentsInChildren<Component>()) {
                if (component is Transform) continue;
                if (component is SkinnedMeshRenderer) continue;
                if (component is MeshRenderer) continue;
                if (component is MeshFilter) continue;

                Destroy(component);
            }

            // just in case it doesn't gets deleted properly later
            TimedDestruction timedDestruction = spawn.AddComponent<TimedDestruction>();
            timedDestruction.Trigger(1f);

            return spawn;
        }
    }
}
