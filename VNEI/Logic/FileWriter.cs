using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Jotunn.Entities;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VNEI.Logic
{
    public static class FileWriter
    {
        private static string fileOutputPath = BepInEx.Paths.BepInExRootPath;
        private static string fileName = "VNEI.indexed.items";
        private const string missingType = "missing-type";
        private static List<ItemType> badItemTypes = new List<ItemType> {
                ItemType.Undefined,
                ItemType.Creature,
                ItemType.Piece
        };

        private static string buildFullFilePath(string fileSuffix)
        {
            return $"{fileOutputPath}{Path.DirectorySeparatorChar}{fileName}.{fileSuffix}";
        }

        public static void PrintSimpleTextFile(List<Item> Items)
        {
            string file = buildFullFilePath("txt");
            Log.LogInfo($"Writing indexed items to file {file}");
            File.WriteAllLines(file,
                Items.Select(item => item.PrintItem()).ToArray());
        }

        public static void PrintCSVFile(List<Item> Items)
        {
            string file = buildFullFilePath("csv");
            Log.LogInfo($"Writing indexed items to file {file}");
            string[] itemsWithoutHeader = Items.Select(x => x.PrintItemCSV()).ToArray();
            string[] header = new string[] { Item.PrintCSVHeader() };
            string[] itemsWithHEader = new string[itemsWithoutHeader.Length + 1];
            header.CopyTo(itemsWithHEader, 0);
            itemsWithoutHeader.CopyTo(itemsWithHEader, 1);
            File.WriteAllLines(file, itemsWithHEader);
        }

        public static void PrintCLLCItemConfigYaml(
            List<Item> Items,
            List<string> itemNamesFilterExcluded,
            List<string> modNamesFilterExcluded
            )
        {
            string file = buildFullFilePath("yml");
            Log.LogInfo($"Writing indexed items to file {file}");
            Log.LogInfo($"Excluding items via match terms " +
                $"'{string.Join(",", itemNamesFilterExcluded)}' " +
                $"and mods with names matching " +
                $"'{string.Join(",", modNamesFilterExcluded)}'");
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            List<(string itemType, string internalName, string modName)> validItems =
                filterInvalidItems(Items, itemNamesFilterExcluded, modNamesFilterExcluded)
                .Select(item => (
                    item.gameObject.TryGetComponent(out ItemDrop itemDrop) ?
                    itemDrop.m_itemData.m_shared.m_itemType.ToString() : missingType,
                    item.internalName,
                    (item.mod != null ? item.mod.Name : string.Empty)))
                .ToList();
            Dictionary<string, List<string>> groupedItems = validItems
                .GroupBy(item => item.modName != string.Empty ?
                       $"{item.itemType}_{cleanModName(item.modName)}" : item.itemType)
                .Where(group => group.Key != missingType)
                .ToDictionary(group => group.Key,
                group => group.Select(groupedItem => groupedItem.internalName).ToList());

            string yamlContent = serializer.Serialize(new AllGroups(groupedItems));
            File.WriteAllText(file, yamlContent);
        }

        private static List<Item> filterInvalidItems(
            List<Item> Items,
            List<string> itemNamesFilterExcluded,
            List<string> modNamesFilterExcluded)
        {
            return Items
                .Where(item => item.gameObject != null)
                .Where(item => !badItemTypes.Contains(item.itemType))
                .Where(item => itemNamesFilterExcluded.All(
                    filterExclude => !item.internalName.Contains(filterExclude)))
                .Where(item => modNamesFilterExcluded.All(
                    filterExclude => !(item.mod != null ? item.mod.Name : string.Empty)
                    .Contains(filterExclude))).ToList();
        }

        private static string cleanModName(string modNameOriginal)
        {
            return Regex.Replace(modNameOriginal, "[^\\w]+", "_");
        }
    }

    public class AllGroups
    {
        public readonly Dictionary<string, List<string>> Groups;

        public AllGroups(Dictionary<string, List<string>> groups)
        {
            this.Groups = groups;
        }
    }

    public class FileWriterController : ConsoleCommand
    {
        public override void Run(string[] args)
        {
            List<Item> items = Indexing.GetActiveItems().Select(tuple => tuple.Value).ToList();
            if (args.Length == 0) FileWriter.PrintCSVFile(items);
            else
            {
                switch (args[0].ToLower())
                {
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
        }

        public override string Name => "vnei_write_items_file";
        public override string Help => "Writes (all) indexed items to file. Options:\n" +
            "   1. csv (default) -> csv can be used with excel or other tools\n" +
            "   2. yaml -> yaml creates a CLLC ItemConfig.yml like file\n" +
            "      yaml optional extra args (both provided as list, separated by ','): \n" +
            "      a. strings to scan item prefab names for and exclude them from results e.g. RRR,someotherfilter\n" +
            "      b. strings to scan mod names for and exclude them from results e.g. MonsterMobs,someothermod\n" +
            "   3. text -> simple text\n";

    }
}
