using System.IO;

namespace VNEI.Logic {
    public static class ExportPaths {
        private static string GetMainExportFolder() {
            return Path.Combine(BepInEx.Paths.BepInExRootPath, "VNEI-Export");
        }

        public static string GetExportLocation(string fileName, params string[] path) {
            string fullPath = Path.Combine(GetMainExportFolder(), Path.Combine(path), fileName);
            string directory = Path.GetDirectoryName(fullPath);

            if (directory == null || string.IsNullOrEmpty(directory)) {
                throw new DirectoryNotFoundException($"Invalid directory for {fileName}: {directory}");
            }

            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            return fullPath;
        }
    }
}
