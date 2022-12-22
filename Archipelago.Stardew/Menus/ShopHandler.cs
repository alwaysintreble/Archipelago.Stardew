using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Stardew.ItemsAndLocations;
using Archipelago.Stardew.Networking;
using StardewValley;
using StardewValley.Menus;

namespace Archipelago.Stardew.Menus
{
    public static class ShopHandler
    {
        private static readonly Dictionary<string, int[]> JojaPerms = new()
        {
            { "Joja Cola", new[] { 75, int.MaxValue } },
            //{ "Joja Wallpaper", new[] { 20, int.MaxValue } },
            //{ "J. Cola Light",  new[] { 500, int.MaxValue } },
            { "Grass Starter",  new[] { 125, int.MaxValue } },
            { "Sugar",          new[] { 125, int.MaxValue } },
            { "Wheat Flour",    new[] { 125, int.MaxValue } },
            { "Rice",           new[] { 250, int.MaxValue } },
        };
        
        private static readonly Dictionary<string, int[]> SeedShopPerms = new()
        {
            { "Grass Starter", new [] { 100, int.MaxValue } }
        };

        private static readonly Dictionary<string, int[]> SandyPerms = new()
        {
            { "Cactus Seeds", new[] { 150, int.MaxValue } }
        };

        public static void OverrideInventory(ShopMenu menu)
        {
            var context = menu.storeContext;
            var season = Game1.currentSeason;
            Dictionary<ISalable, int[]> permInventory = null;
            switch (context)
            {
                case "JojaMart":
                    permInventory = Utils.LookupToItemDict(JojaPerms);
                    ISalable wallpaper; // TODO need decorations in the AllItems dict
                    ISalable flooring;
                    break;
                case "SeedShop":
                    permInventory = Utils.LookupToItemDict(SeedShopPerms);
                    break;
                case "SandyHouse":
                    permInventory = Utils.LookupToItemDict(SandyPerms);
                    /*var shirt = SVObjectLookup.AllItems[
                        SVObjectLookup.Shirts[Utils.Random.Next(SVObjectLookup.Shirts.Count)]];
                    permInventory.Add(shirt, new[] { shirt.salePrice(), shirt.maximumStackSize() }); TODO */
                    break;
                case "Blacksmith":
                    if (menu.itemPriceAndStock.ContainsKey(SVObjectLookup.AllItems["Copper Ore"])) return;
                    foreach (var num in menu.itemPriceAndStock.Keys.SelectMany(item => menu.itemPriceAndStock[item]))
                        Utils.LogMessage(num.ToString());
                    break;
            }
            
            Dictionary<string, int[]> seasonDict = null;
            if (ArchipelagoClient.ServerData.SlotData == null) return;
            
            if (ArchipelagoClient.ServerData.SlotData.TryGetValue(context, out var data))
            {
                var contextDict = (Dictionary<string, Dictionary<string, int[]>>)data;
                seasonDict = contextDict[season];
            }
            
            if (seasonDict == null) // current context doesn't exist in the slot data so we leave the shop alone
            {
                Utils.LogMessage($"Unable to access {context} in SlotData", Enums.MessageType.Error);
                return;
            }
            try
            {
                var newInventory = Utils.LookupToItemDict(seasonDict);
                
                newInventory = Utils.Merge(new List<Dictionary<ISalable, int[]>> {
                    newInventory,
                    permInventory
                });
                
                menu.setItemPriceAndStock(newInventory);
            }
            catch (Exception e)
            {
                Utils.LogMessage($"{e.Message} at {e.Source}", Enums.MessageType.Error);
            }
        }

        public static bool OnUpgradePurchase(ISalable item, Farmer player, int numberToBuy)
        {
            return true;
        }
    }
}