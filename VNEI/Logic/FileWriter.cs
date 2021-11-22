using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
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

        public static void PrintToFile(Dictionary<int, Item> Items)
        {
            if (Plugin.printItemListToFile.Value)
            {
                PrintSimpleTextFile(Items);
                PrintCSVFile(Items);
                PrintCLLCItemConfigYaml(Items,
                    new List<string> { "RRR" },
                    new List<string> { "MonsterMobs" });
            }
        }

        private static void PrintSimpleTextFile(Dictionary<int, Item> Items)
        {
            string file = buildFullFilePath("txt");
            Log.LogInfo($"Writing indexed items to file {file}");
            File.WriteAllLines(file,
                Items.Select(x => x.Value.PrintItem()).ToArray());
        }

        private static void PrintCSVFile(Dictionary<int, Item> Items)
        {
            string file = buildFullFilePath("csv");
            Log.LogInfo($"Writing indexed items to file {file}");
            string[] itemsWithoutHeader = Items.Select(x => x.Value.PrintItemCSV()).ToArray();
            string[] header = new string[] { Item.PrintCSVHeader() };
            string[] itemsWithHEader = new string[itemsWithoutHeader.Length + 1];
            header.CopyTo(itemsWithHEader, 0);
            itemsWithoutHeader.CopyTo(itemsWithHEader, 1);
            File.WriteAllLines(file, itemsWithHEader);
        }

        private static void PrintCLLCItemConfigYaml(
            Dictionary<int, Item> Items,
            List<string> itemNamesFilterExcluded,
            List<string> modNamesFilterExcluded
            )
        {
            string file = buildFullFilePath("yml");
            Log.LogInfo($"Writing indexed items to file {file}");
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
            Dictionary<int, Item> Items,
            List<string> itemNamesFilterExcluded,
            List<string> modNamesFilterExcluded)
        {
            return Items.Values
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

}
