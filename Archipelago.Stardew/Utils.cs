using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Archipelago.Stardew.ItemsAndLocations;
using Archipelago.Stardew.Networking;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace Archipelago.Stardew
{
    public static class Utils
    {
        public static IModHelper Helper;
        private static IMonitor Monitor;

        private static string directoryPath;
        private static string filePath;
        
        public const SButton MenuButton = SButton.OemTilde;
        public static APData SavedServerData;

        public const int DefaultStackCount = 10;
        public const int SeedStackCount = 25;

        private const int DequeueCount = 2;
        private static int dequeueTimeout = 3;
        private static readonly List<ChatMessage> ChatMessages = new();

        public static Random Random = new();

        private struct ChatMessage
        {
            public ChatMessage(string message, Color color)
            {
                Message = message;
                Color = color;
            }
                
            public string Message { get; }
            public Color Color { get; }
                
        }

        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            LogMessage("Monitor and helper initialized.");
        }
        
        public static void Setup()
        {
            LogMessage("Setting up utils data...");
            directoryPath = Path.Combine(Helper.DirectoryPath, "data");
            filePath = Path.Combine(directoryPath, $"{Game1.GetSaveGameName()}_{Game1.getFarm().Name}");
            SVObjectLookup.FillContentLookups();
        }
        /// <summary>
        /// We're connected to AP and creating a new save file, which are valid conditions. Save the connection info
        /// to a file, and delete the old file, if one exists.
        /// </summary>
        public static void SaveConnectionInfo()
        {
            var serializedData = JsonConvert.SerializeObject(ArchipelagoClient.ServerData);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, serializedData);
            Random = new Random((int)ArchipelagoClient.ServerData.SlotData["seed"]);
        }

        public static void ReadConnectionInfo()
        {
            if (!File.Exists(filePath)) return;
            var serializedData = File.ReadAllText(filePath);
            SavedServerData = JsonConvert.DeserializeObject<APData>(serializedData);
            Random = new Random((int)ArchipelagoClient.ServerData.SlotData["seed"]);
        }

        public static void LogMessage(string message, Enums.MessageType type = Enums.MessageType.Trace)
        {
            Color color;
            switch (type)
            {
                case Enums.MessageType.Hint:
                    color = Color.Plum;
                    Monitor.Log(message);
                    break;
                case Enums.MessageType.Item:
                    color = Color.Aqua;
                    Monitor.Log(message);
                    break;
                case Enums.MessageType.Countdown:
                    if (Context.IsWorldReady) Game1.chatBox.addInfoMessage(message);
                    Monitor.Log(message);
                    return;
                case Enums.MessageType.Default:
                    color = Color.Salmon;
                    Monitor.Log(message);
                    break;
                case Enums.MessageType.Error:
                    if (Context.IsWorldReady) Game1.chatBox.addErrorMessage(message);
                    Monitor.Log(message, LogLevel.Error);
                    return;
                case Enums.MessageType.Debug:
                    Monitor.Log(message, LogLevel.Debug);
                    return;
                case Enums.MessageType.Trace:
                    Monitor.Log(message);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, message);
            }
            
            ChatMessages.Add(new ChatMessage(message, color));
        }

        public static void PrintMessages()
        {
            dequeueTimeout -= 1; // this function gets called once every second
            if (!(dequeueTimeout <= 0)) return;
            var toProcess = new List<ChatMessage>();
            while (toProcess.Count < DequeueCount && ChatMessages.Count > 0)
            {
                toProcess.Add(ChatMessages[0]);
                ChatMessages.RemoveAt(0);
            }
            
            foreach (var message in toProcess)
                Game1.chatBox.addMessage(message.Message, message.Color);
            dequeueTimeout = 3;
        }

        public static string ToTitle(string[] name)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(string.Join(" ", name).ToLower());
        }

        public static string ToTitle(string name)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLower());
        }

        public static Dictionary<ISalable, int[]> LookupToItemDict(Dictionary<string, int[]> lookup)
        {
            return lookup.Keys.ToDictionary(item => SVObjectLookup.AllItems[item], item => lookup[item]);
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> enumerable)
        {
            return enumerable.SelectMany(
                x => x
                ).ToDictionary(
                x => x.Key, y => y.Value
                );
        }

        public static void SetConsoleCommands()
        {
            var commands = new List<Tuple<string, string, Action<string, string[]>>>
            {
                new("ap_shop", "Opens the item shop for received AP items", ApShop)
            };

#if DEBUG
            
            commands.Add(new Tuple<string, string, Action<string, string[]>>(
                "fill_lookups", "Debug command to fill the content lookup tables.", FillLookups));
            
            commands.Add(new Tuple<string, string, Action<string, string[]>>(
                "everything_shop", "Debug command to open a shop menu with every item for free.", EverythingShop));
            
            commands.Add(new Tuple<string, string, Action<string, string[]>>(
                "output_items", "Debug command to output all items information to a json file.", OutputItems));
            
            commands.Add(new Tuple<string, string, Action<string, string[]>>(
                "add_item", "Debug command to add item to the received item shop. Name must match exactly." +
                            " Supports custom amount", DebugReceiveItem));
            
            commands.Add(new Tuple<string, string, Action<string, string[]>>(
                "free_item", "Debug command to grant a free item.", FreeItem));
            
            #endif
            
            foreach (var command in commands)
                Helper.ConsoleCommands.Add(command.Item1, command.Item2, command.Item3);
        }

        private static void ApShop(string command, string[] args)
        {
            Game1.activeClickableMenu = new ShopMenu(ArchipelagoClient.ServerData.ReceivedItems);
        }

        private static void FillLookups(string command, string[] args)
        {
            SVObjectLookup.FillContentLookups();
        }

        private static void EverythingShop(string command, string[] args)
        {
            var everythingShop =
                SVObjectLookup.AllItems.Values.ToDictionary(item => item, _ => new[] { 0, 100 });
            Game1.activeClickableMenu = new ShopMenu(everythingShop);
        }

        private static void DebugReceiveItem(string command, string[] args)
        {
            string itemName;
            if (int.TryParse(args[-1], out var count)) 
                itemName = ToTitle(args.SkipLast(1).ToArray());
            else
            {
                count = DefaultStackCount;
                itemName = ToTitle(args);
            }

            if (!SVObjectLookup.AllItems.TryGetValue(itemName, out var itemData))
            {
                LogMessage($"Attempted to lookup {itemName} but couldn't find it", Enums.MessageType.Error);
                return;
            }

            if (SVObjectLookup.Seeds.Contains(itemName)) count = SeedStackCount;
            if (ArchipelagoClient.ServerData.ReceivedItems.ContainsKey(itemData))
                ArchipelagoClient.ServerData.ReceivedItems[itemData][1] += count;
            ArchipelagoClient.ServerData.ReceivedItems.Add(itemData, new[] { 0, count });
        }

        private static void FreeItem(string command, string[] args)
        {
            var itemName = string.Join(" ", args);
            itemName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(itemName.ToLower());
            int count = 1;

            if (!SVObjectLookup.AllItems.TryGetValue(itemName, out var itemData)) return;
            var seedDictionaries =
                Merge(new List<Dictionary<string, string>>{
                    SVObjectLookup.SpringSeedToCrops,
                    SVObjectLookup.SummerSeedToCrops,
                    SVObjectLookup.FallSeedToCrops,
                    SVObjectLookup.SpecialSeedToCrops
                });
            if (seedDictionaries.ContainsKey(itemName)) count = 25;
            var itemDict = new Dictionary<ISalable, int[]> { { itemData, new[] { 0, count } } };
            
            Game1.activeClickableMenu = new ShopMenu(itemDict);
        }

        private static void OutputItems(string command, string[] args)
        {
            if (SVObjectLookup.AllItems == null)
            {
                Monitor.Log("Items lookup is empty. Please run /fill_lookups first", LogLevel.Debug);
                return;
            }
            var itemsOutput = Path.Combine(directoryPath, "sv_items_output.json");
            try
            {
                var serializedData = JsonConvert.SerializeObject(SVObjectLookup.AllItems.Values, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                File.WriteAllText(itemsOutput, serializedData);
                LogMessage($"Items output to {itemsOutput}");
            }
            catch (Exception e)
            {
                LogMessage($"Exception occured: {e.Message}, {e.Source}", Enums.MessageType.Error);
            }
        }
    }
}