namespace Archipelago.Stardew.ItemsAndLocations
{
    public static class Enums
    {
        public enum ItemType
        {
            Basic,
            Minerals,
            Quest,
            Asdf, // ???
            Crafting,
            Arch,
            Fish,
            Cooking,
            Seeds,
            Ring,
            // null // most furniture is in this category
        }
        
        public enum Category
        {
            Basic = 0, // contains basic trash, artifacts, some basic craftables, rings, quest items
            Forageable = -81,
            Vegetable = -75,
            Minerals = -2,
            Fruit = -79,
            TreeSeed = -74,
            Bomb = -8,
            Floor = -24,
            Weapon = -98 // this is the only category i'm sure of
        }
        
        public enum Quality
        {
            Normal,
            Silver,
            Gold,
            Iridium
        }

        public enum Season
        {
            Spring,
            Summer,
            Fall,
            Winter
        }

        public enum ChatKind
        {
            Info, //white but displays ??? for the name
            Error,
            Yellow, //yellow and displays |> as the name
            Private, //dark cyan, displays ??? [Private]
        }

        public enum MessageType
        {
            Hint,
            Item,
            Countdown,
            Default,
            Error,
            Debug,
            Trace
        }

        public enum ShopContexts
        {
            JojaMart,
            Blacksmith, // seems to be both ore store and upgrades
            SeedShop, // Pierre's store
            Hospital,
            ScienceHouse,
            Desert,
            SandyHouse,
            Saloon,
            AdventureGuild
        }
    }
}