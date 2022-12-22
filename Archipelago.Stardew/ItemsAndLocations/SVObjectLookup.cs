using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace Archipelago.Stardew.ItemsAndLocations
{
    public static class SVObjectLookup
    {
        public static Dictionary<string, ISalable> AllItems;
        public static List<string> Seeds;
        public static List<string> Crops;
        public static List<string> Weapons;
        public static List<string> Recipes;

        /* Item attributes
         Category:                      int
         DisplayName:                   string
         Name:                          string
         Type:                          string // can also be null and weapons do not have a type attribute
         Stack:                         int
         Quality:                       int
         CanBeSetDown:                  bool
         CanBeGrabbed:                  bool
         HasBeenPickedUpByFarmer:       bool
         IsHoeDirt:                     bool
         IsOn:                          bool
         IsSpawnedObject:               bool
         IsRecipe:                      bool
         Flipped:                       bool
         Price:                         int
         Edibility:                     int
         Fragility:                     int
         Scale:                         int[2]
         MinutesUntilReady:             int
         */

        public static readonly Dictionary<string, string> SpringSeedToCrops = new()
        {
            { "Jazz Seeds",        "Blue Jazz" },
            { "Cauliflower Seeds", "Cauliflower" },
            { "Coffee Bean",       "Coffee" },
            { "Garlic Seeds",      "Garlic" },
            { "Bean Starter",      "Green Bean" },
            { "Kale Seeds",        "Kale" },
            { "Parsnip Seeds",     "Parsnip" },
            { "Potato Seeds",      "Potato" },
            { "Rhubarb Seeds",     "Rhubarb" },
            { "Strawberry Seeds",  "Strawberry" },
            { "Tulip Bulb" ,       "Tulip" },
            { "Rice Shoot",        "Unmilled Rice" }
        };

        public static readonly Dictionary<string, string> SummerSeedToCrops = new()
        {
            { "Blueberry Seeds",   "Blueberry" },
            { "Corn Seeds",        "Corn" },
            { "Hops Starter",      "Hops" },
            { "Pepper Seeds",      "Hot Pepper" },
            { "Melon Seeds",       "Melon" },
            { "Poppy Seeds",       "Poppy" },
            { "Radish Seeds",      "Radish" },
            { "Red Cabbage Seeds", "Red Cabbage" },
            { "Starfruit Seeds",   "Starfruit" },
            { "Spangle Seeds",     "Summer Spangle" },
            { "Sunflower Seeds",   "Sunflower" },
            { "Tomato Seeds",      "Tomato" },
            { "Wheat Seeds",       "Wheat" }
        };

        public static readonly Dictionary<string, string> FallSeedToCrops = new()
        {
            { "Amaranth Seeds",    "Amaranth" },
            { "Artichoke Seeds",   "Artichoke" },
            { "Beet Seeds",        "Beet" },
            { "Bok Choy Seeds",    "Bok Choy" },
            { "Cranberry Seeds",   "Cranberry" },
            { "Eggplant Seeds",    "Eggplant" },
            { "Fairy Seeds",       "Fairy Rose" },
            { "Grape Starter",     "Grape" },
            { "Pumpkin Seeds",     "Pumpkin" },
            { "Yam Seeds",         "Yam" }
        };

        public static readonly Dictionary<string, string> SpecialSeedToCrops = new()
        {
            { "Ancient Seeds",     "Ancient Fruit" },
            { "Cactus Seeds",      "Cactus Fruit" },
            { "Fiber Seeds",       "Fiber" },
            { "Pineapple Seeds",   "Pineapple" },
            { "Taro Tuber",        "Taro Root" },
            { "Rare Seed",         "Sweet Gem Berry" },
            { "Tea Sapling",       "Tea Leaves" }
        };

        public static void FillContentLookups()
        {
            var gameItems = new List<ISalable> { new Furniture(1226, Vector2.Zero) };
            try
            { 
                gameItems.AddRange(
                Game1.objectInformation.Distinct()
                  .Select(keyValuePair => new Object(keyValuePair.Key, 1)));
                
                gameItems.AddRange(
                Game1.bigCraftablesInformation.Select(keyValuePair
                    => new Object(Vector2.Zero, keyValuePair.Key)));
                
                /*
                itemPriceAndStock.AddRange(
                Game1.content.Load<Dictionary<int, string>>("Data\\weapons")
                  .Select(keyValuePair => new MeleeWeapon(keyValuePair.Key)));

                Weapons = new List<string>(
                    Game1.content.Load<Dictionary<int, string>>("Data\\weapons").Values.ToList()
                    );
                /*
                itemPriceAndStock.AddRange(
                  Game1.content.Load<Dictionary<int, string>>("Data\\furniture")
                    .Select(keyValuePair => new Furniture(keyValuePair.Key, Vector2.Zero)));
  
                itemPriceAndStock.AddRange(
                    Game1.content.Load<Dictionary<int, string>>("Data\\crops")
                        .Select(KeyValuePair => new Crop(KeyValuePair.Key, 0, 0)));
                */
            }
            catch (Exception e)
            {
                Utils.LogMessage($"Exception occurred: {e.Message}, {e.Source}", Enums.MessageType.Error);
            }

            AllItems = gameItems.GroupBy(k => k.DisplayName).Select(o => o.First())
                .ToDictionary(k => k.DisplayName, k => k);

            Seeds = Utils.Merge(new List<Dictionary<string, string>>{
                SpringSeedToCrops,
                SummerSeedToCrops,
                FallSeedToCrops,
                SpecialSeedToCrops
            }).Keys.ToList();

            Crops = Utils.Merge(new List<Dictionary<string, string>>
            {
                SpringSeedToCrops,
                SummerSeedToCrops,
                FallSeedToCrops,
                SpecialSeedToCrops
            }).Values.ToList();
            
            
        }
    }
}