using Archipelago.Stardew.Networking;
using StardewValley.Menus;

namespace Archipelago.Stardew.Menus
{
    public class ArchipelagoShop: ShopMenu
    {
        public ArchipelagoShop() : base(ArchipelagoClient.ServerData.ReceivedItems)
        {
            Utils.LogMessage(
                $"Setting up Archipelago inventory shop with: {ArchipelagoClient.ServerData.ReceivedItems}");
            foreach (var item in ArchipelagoClient.ServerData.ReceivedItems)
            {
                Utils.LogMessage($"{item.Key.DisplayName}: ${item.Value[0]}, {item.Value[1]}X");
            }
        }
    }
}