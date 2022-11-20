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

        public static void PrintCSVFile(List<Item> items) {
            string file = BuildFullFilePath("csv");
            Log.LogInfo($"Writing indexed items to file {file}");
            string[] itemsWithoutHeader = items.Select(x => x.PrintItemCSV()).ToArray();
            string[] header = { Item.PrintCSVHeader() };
            string[] itemsWithHeader = new string[itemsWithoutHeader.Length + 1];
            header.CopyTo(itemsWithHeader, 0);
            itemsWithoutHeader.CopyTo(itemsWithHeader, 1);
            File.WriteAllLines(file, itemsWithHeader);
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
        [UsedImplicitly]
        public readonly Dictionary<string, List<string>> Groups;

        public AllGroups(Dictionary<string, List<string>> groups) {
            Groups = groups;
        }
    }

    public class FileWriterController : ConsoleCommand {
        public override void Run(string[] args) {
            List<Item> items = Indexing.GetActiveItems().Select(tuple => tuple.Value).ToList();
            if (args.Length == 0) {
                FileWriter.PrintCSVFile(items);
                return;
            }

            switch (args[0].ToLower()) {
                case "csv":
                    FileWriter.PrintCSVFile(items);
                    break;
                case "yaml":
                    Log.LogInfo($"args '{string.Join(" - ", args)}'");
                    List<string> itemNamesFilterExcluded;
                    if (args.Length >= 2) itemNamesFilterExcluded = args[1].Split(',').ToList();
                    else itemNamesFilterExcluded = new List<string>();
                    List<string> modNamesFilterExcluded;
                    if (args.Length >= 3) modNamesFilterExcluded = args[2].Split(',').ToList();
                    else modNamesFilterExcluded = new List<string>();
                    FileWriter.PrintCLLCItemConfigYaml(items, itemNamesFilterExcluded, modNamesFilterExcluded);
                    break;
                case "text":
                    FileWriter.PrintSimpleTextFile(items);
                    break;
            }
        }

        public override string Name => "vnei_write_items_file";

        public override string Help => "[csv/yaml/text] - default is csv - Writes all indexed items to file";

        public override List<string> CommandOptionList() {
            return new List<string> { "csv", "yaml", "text" };
        }
    }
}
