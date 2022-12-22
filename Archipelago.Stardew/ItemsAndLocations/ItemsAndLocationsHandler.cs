using System.Collections.Generic;
using System.Linq;
using Archipelago.Stardew.Networking;
using StardewValley.Menus;

namespace Archipelago.Stardew.ItemsAndLocations
{
    public static class ItemsAndLocationsHandler
    {
        private static readonly Dictionary<long, string> ItemsLookup = new();
        private static readonly Dictionary<string, long> BundleLookup = new();

        /// <summary>
        /// Sets up lookup dictionaries for items and locations.
        /// Must be called after loading into the game, but before checking locations or unlocking items.
        /// </summary>
        public static void Initialize()
        {
            const long baseOffset = 0xD0000;
            for (var index = 0; index < SVObjectLookup.AllItems.ToList().Count; index++)
                ItemsLookup[baseOffset + index] = SVObjectLookup.AllItems.ToList()[index].Key;

            Utils.LogMessage($"Number of items: {ItemsLookup.Count}");
            
        }
        
        public static void Unlock(long itemToUnlock)
        {
            if (!ItemsLookup.TryGetValue(itemToUnlock, out var itemName))
            {
                Utils.LogMessage($"Couldn't find {itemToUnlock} in lookup table to unlock it.", Enums.MessageType.Error);
                return;
            }
            var count = Utils.DefaultStackCount;
            if (SVObjectLookup.Seeds.Contains(itemName))
                count = Utils.SeedStackCount;
            var itemData = SVObjectLookup.AllItems[itemName];
            ArchipelagoClient.ServerData.ReceivedItems.Add(itemData, new[] { 0, count });
        }
        
        private static void CheckLocation(long locationIndex)
        {
            if (!ArchipelagoClient.Authenticated) return;
            Utils.LogMessage($"Sending completion check for {locationIndex}");
            ArchipelagoClient.Session.Locations.CompleteLocationChecksAsync(locationIndex);
            if (!ArchipelagoClient.ServerData.CheckedLocations.Contains(locationIndex))
                ArchipelagoClient.ServerData.CheckedLocations.Add(locationIndex);
        }

        public static void BuildBundles(JunimoNoteMenu menu)
        {
            foreach (var b in menu.bundles)
            {
                Utils.LogMessage($"Replacing bundle: {b}, {b.name}, {b.label}, {b.bundleIndex}");
            }
        }

        public static void CheckBundles(JunimoNoteMenu menu)
        {
            foreach (var b in menu.bundles)
            {
                Utils.LogMessage($"Checking bundle: {b}, {b.name}, {b.label}, {b.bundleIndex}");
                Utils.LogMessage($"Bundle reward: {b.rewardDescription}");
                Utils.LogMessage($"Bundle ingredients: ");
                foreach (var ingredient in b.ingredients)
                {
                    Utils.LogMessage($"{ingredient.index}, {ingredient.quality}, {ingredient.stack}, {ingredient.completed}");
                }
                if (!b.complete) continue;
                
                Utils.LogMessage($"Sending completion check for {b.name} with index {b.bundleIndex}");
                if (BundleLookup.TryGetValue(b.name, out var locationIndex))
                    CheckLocation(locationIndex);
                else
                    Utils.LogMessage($"Unable to find {b.name} in bundle lookup", Enums.MessageType.Error);
            }
        }
    }
}