using System.IO;

namespace VNEI.Logic {
    public static class ExportPaths {
        public static string GetMainExportFolder() {
            return Path.Combine(BepInEx.Paths.BepInExRootPath, "VNEI-Export");
        }
    }
}
