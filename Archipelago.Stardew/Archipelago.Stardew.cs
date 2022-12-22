using System.Linq;
using Archipelago.Stardew.ItemsAndLocations;
using Archipelago.Stardew.Networking;
using Archipelago.Stardew.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Archipelago.Stardew
{
    public class ModEntry : Mod
    {
        /*
         * public methods
         */
        
        /// <summary>
        /// Mod entry point, called when mod is loaded.
        /// </summary>
        /// <param name="helper">Modding API helper</param>
        public override void Entry(IModHelper helper)
        {
            // setup basic info from SMAPI
            Utils.Initialize(Monitor, Helper);
            
            // base data we need for using AP
            helper.Events.GameLoop.SaveCreated += (_, _) => SaveCreated();
            helper.Events.GameLoop.SaveLoaded += (_, _) => SaveLoaded();
            Utils.SetConsoleCommands();

            // modify game content
            helper.Events.Input.ButtonPressed += ButtonPressed;
            helper.Events.Display.MenuChanged += MenuChanged;
            helper.Events.GameLoop.OneSecondUpdateTicking += (_, _) => Update();
            
            /*// patches
            var harmony = new Harmony(ModManifest.UniqueID);
            try
            {
                // bundle completions
                harmony.Patch(
                    original: AccessTools.Method(typeof(Bundle), nameof(Bundle.completionAnimation),
                        new[] { typeof(JunimoNoteMenu), typeof(bool), typeof(int) }),
                    postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.BundleCompletion))
                );
            }
            catch (Exception e)
            {
                Monitor.Log($"Failed to apply Harmony patch: {e.Source}\n{e.Message}", LogLevel.Error);
            }*/
        }
        
        /*
         * Private methods
         */
        
        /// <summary>
        /// Raised when a new farmer is created. Not sure what I need to do for multiplayer support yet, but checks
        /// for connection, and saves it to file.
        /// </summary>
        private static void SaveCreated()
        {
            Utils.Setup();
            if (!ArchipelagoClient.Authenticated) return;
            Utils.SaveConnectionInfo();
            SVObjectLookup.FillContentLookups();
        }

        /// <summary>
        /// Raised when an existing save is loaded. If we're already connected to an AP server, verify it's the same
        /// save for the same multiworld. If not related to the currently multiworld, disconnect and quit to main menu.
        /// </summary>
        private void SaveLoaded()
        {
            Utils.Setup();
            if (ArchipelagoClient.Authenticated)
            {
                Utils.ReadConnectionInfo();
                if (Utils.SavedServerData != null &&
                    Utils.SavedServerData.SeedName == ArchipelagoClient.ServerData.SeedName) return;
                Utils.LogMessage("Attempted to load save for different game. Returning to Title...",
                    Enums.MessageType.Error);
                Game1.exitToTitle = true;
                ArchipelagoClient.Disconnect();
            }
            else
            {
                Utils.ReadConnectionInfo();
                if (Utils.SavedServerData != null) ArchipelagoClient.ServerData = Utils.SavedServerData;
            }
        }
        
        /// <summary>
        /// Binds a key to open the Archipelago Connection Menu. Can be opened anywhere technically, but disabling it
        /// while in game seems safest.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button != Utils.MenuButton) return;
            if (Context.IsWorldReady) 
                switch (Game1.activeClickableMenu)
                {
                    case ArchipelagoMenu:
                        Game1.exitActiveMenu();
                        break;
                    case null:
                        Game1.activeClickableMenu = new ArchipelagoMenu();
                        break;
                    default:
                        return;
                }
            else if (Game1.activeClickableMenu is ConnectMenu)
                Game1.exitActiveMenu();
            else
                Game1.activeClickableMenu = new ConnectMenu();
        }
        
        /// <summary>
        /// Raised when player opens or closes a menu.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The Event data</param>
        private void MenuChanged(object sender, MenuChangedEventArgs e)
        {
            Utils.LogMessage($"Old Menu: {e.OldMenu}. New Menu: {e.NewMenu}");
            
            if (!Context.IsWorldReady /*|| !ArchipelagoClient.Authenticated TODO*/) return;
            
            if (e.OldMenu is JunimoNoteMenu menu)
                ItemsAndLocationsHandler.CheckBundles(menu);
            
            if (e.NewMenu == null) return;
            switch (e.NewMenu)
            {
                case ShopMenu shopMenu:
                    switch (shopMenu.storeContext)
                    {
                        case "Hospital" or "ScienceHouse" or "Saloon" or "Desert": //we don't randomize any of these yet
                            return;
                        default:
                            ShopHandler.OverrideInventory(shopMenu);
                            break;
                    }
                    break;
                case MuseumMenu museumMenu:
                    break;
            }
        }

        /// <summary>
        /// Called once every second while the game is ready. If we aren't in game or not connected to the server, does
        /// nothing, otherwise processes Archipelago messages and item receipts.
        /// </summary>
        private static void Update()
        {
            #if DEBUG
            Utils.PrintMessages();
            #endif
            if (!Context.IsWorldReady || !ArchipelagoClient.Authenticated || Utils.SavedServerData == null) return;
            ArchipelagoClient.APUpdate();
        }
    }
}