using System.Collections.Generic;
using StardewValley;

namespace Archipelago.Stardew.Networking
{
    public class APData
    {
        public string Uri;
        public int Port;
        public string SlotName;
        public string Password;
        public int Index = 0;
        public string SeedName;
        public Dictionary<string, object> SlotData;
        public readonly List<long> CheckedLocations = new();
        public readonly Dictionary<ISalable, int[]> ReceivedItems = new();
    }
}