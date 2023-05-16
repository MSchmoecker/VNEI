using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Entities;
using UnityEngine;

namespace VNEI.Logic {
    public class IconExport : ConsoleCommand {
        public override string Name => "vnei_export_icons";
        public override string Help => "Exports icons";

        public override void Run(string[] args) {
            List<Item> items = Indexing.GetActiveItems().Select(tuple => tuple.Value).ToList();

            Log.LogInfo($"Exporting icons into {ExportPaths.GetExportLocation("", "icons")}");

            foreach (IGrouping<string, Item> group in items.GroupBy(item => item.GetModName())) {
                if (args.Length >= 1 && !args[0].Split(',').Contains(group.Key)) {
                    continue;
                }

                Log.LogInfo($"Exporting {group.Count()} icons for {group.Key}");

                foreach (Item item in group) {
                    SaveIcon(item);
                }
            }
        }

        private static void SaveIcon(Item item) {
            if (!item.prefab) {
                return;
            }

            Sprite icon = item.GetIcon();

            if (!icon) {
                Log.LogWarning($"No icon for {item.internalName}");
                return;
            }

            string fileName = Path.GetInvalidFileNameChars().Aggregate($"{item.internalName}.png", (current, c) => current.Replace(c, '_'));
            string modName = Path.GetInvalidPathChars().Aggregate(item.GetModName(), (current, c) => current.Replace(c, '_'));
            string path = ExportPaths.GetExportLocation(fileName, "icons", modName);

            int width = icon.texture.width;
            int height = icon.texture.height;

            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(icon.texture, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            int iconWidth = (int)icon.rect.width;
            int iconHeight = (int)icon.rect.height;

            int iconPosX = (int)(icon.textureRect.x - icon.textureRectOffset.x);
            int iconPosY = (int)(height - (icon.textureRect.y - icon.textureRectOffset.y + iconHeight));

            Texture2D readableTexture = new Texture2D(iconWidth, iconHeight);
            readableTexture.ReadPixels(new Rect(iconPosX, iconPosY, iconWidth, iconHeight), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);

            File.WriteAllBytes(path, readableTexture.EncodeToPNG());
        }
    }
}
