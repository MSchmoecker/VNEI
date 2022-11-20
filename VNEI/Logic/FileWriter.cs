using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Jotunn.Entities;

namespace VNEI.Logic {
    public static class FileWriter {
        private static readonly string FileOutputPath = BepInEx.Paths.BepInExRootPath;
        private const string FileName = "VNEI.indexed.items";
        private const string MissingType = "missing-type";

        private static string BuildFullFilePath(string fileSuffix) {
            return $"{FileOutputPath}{Path.DirectorySeparatorChar}{FileName}.{fileSuffix}";
        }

        public static void PrintSimpleTextFile(List<Item> items) {
            string file = BuildFullFilePath("txt");
            Log.LogInfo($"Writing indexed items to file {file}");
            File.WriteAllLines(file, items.Select(item => item.PrintItem()).ToArray());
        }

        public static void PrintCSVFile(List<Item> items, string separator) {
            string file = BuildFullFilePath("csv");
            Log.LogInfo($"Writing indexed items to file {file}");
            string header = Item.PrintCSVHeader(separator);
            string[] lines = items.Select(x => x.PrintItemCSV(separator)).ToArray();
            File.WriteAllLines(file, new[] { header }.Concat(lines).ToArray());
        }

        public static void PrintCLLCItemConfigYaml(
            List<Item> items,
            List<string> itemNamesFilterExcluded,
            List<string> modNamesFilterExcluded
        ) {
            string file = BuildFullFilePath("yml");
            Log.LogInfo($"Writing indexed items to file {file}");
            Log.LogInfo($"Excluding items via match terms '{string.Join(",", itemNamesFilterExcluded)}' and mods with names matching '{string.Join(",", modNamesFilterExcluded)}'");

            ISerializer serializer = new SerializerBuilder()
                                     .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                     .Build();

            List<(string itemType, string internalName, string modName)> validItems =
                FilterInvalidItems(items, itemNamesFilterExcluded, modNamesFilterExcluded)
                    .Select(item => (
                                item.gameObject.TryGetComponent(out ItemDrop itemDrop) ? itemDrop.m_itemData.m_shared.m_itemType.ToString() : item.itemType.ToString(),
                                item.internalName,
                                item.GetModName()))
                    .ToList();
            Dictionary<string, List<string>> groupedItems;
            groupedItems = validItems
                           .GroupBy(item => item.modName != string.Empty ? $"{item.itemType}_{CleanModName(item.modName)}" : item.itemType)
                           .Where(group => group.Key != MissingType)
                           .ToDictionary(group => group.Key,
                                         group => group.Select(groupedItem => groupedItem.internalName).ToList());

            string yamlContent = serializer.Serialize(new AllGroups(groupedItems));
            File.WriteAllText(file, yamlContent);
        }

        private static List<Item> FilterInvalidItems(List<Item> items, List<string> itemNamesFilterExcluded, List<string> modNamesFilterExcluded) {
            return items
                   .Where(item => item.gameObject != null)
                   .Where(item => itemNamesFilterExcluded.All(filterExclude => !item.internalName.Contains(filterExclude)))
                   .Where(item => modNamesFilterExcluded.All(filterExclude => !item.GetModName().Contains(filterExclude))).ToList();
        }

        private static string CleanModName(string modNameOriginal) {
            return Regex.Replace(modNameOriginal, "[^\\w]+", "_");
        }
    }

    public class AllGroups {
        [UsedImplicitly] public readonly Dictionary<string, List<string>> Groups;

        public AllGroups(Dictionary<string, List<string>> groups) {
            Groups = groups;
        }
    }

    public abstract class FileWriterController : ConsoleCommand {
        public override string Help => "Writes all indexed items to a file";

        protected static List<Item> GetItems() {
            return Indexing.GetActiveItems().Select(tuple => tuple.Value).ToList();
        }
    }

    public class FileWriterControllerCSV : FileWriterController {
        public override string Name => "vnei_export_csv";

        public override void Run(string[] args) {
            string separatorArg = args.Length > 0 ? args[0] : string.Empty;
            string separator;

            switch (separatorArg) {
                case "comma":
                    separator = ",";
                    break;
                case "tab":
                    separator = "\t";
                    break;
                case "semicolon":
                    separator = ";";
                    break;
                default:
                    separator = ",";
                    break;
            }

            FileWriter.PrintCSVFile(GetItems(), separator);
        }

        public override List<string> CommandOptionList() {
            return new List<string> { "comma", "semicolon", "tab" };
        }
    }

    public class FileWriterControllerYAML : FileWriterController {
        public override string Name => "vnei_export_yaml";

        public override void Run(string[] args) {
            Log.LogInfo($"args '{string.Join(" - ", args)}'");
            List<string> itemNamesFilterExcluded = args.Length > 0 ? args[0].Split(',').ToList() : new List<string>();
            List<string> modNamesFilterExcluded = args.Length > 1 ? args[1].Split(',').ToList() : new List<string>();
            FileWriter.PrintCLLCItemConfigYaml(GetItems(), itemNamesFilterExcluded, modNamesFilterExcluded);
        }
    }

    public class FileWriterControllerText : FileWriterController {
        public override string Name => "vnei_export_text";

        public override void Run(string[] args) {
            FileWriter.PrintSimpleTextFile(GetItems());
        }
    }
}
