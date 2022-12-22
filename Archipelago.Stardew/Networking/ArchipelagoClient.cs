using System;
using System.Linq;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.Stardew.ItemsAndLocations;
using Archipelago.Stardew.Menus;
using StardewModdingAPI;
using StardewValley;

namespace Archipelago.Stardew.Networking
{
    public static class ArchipelagoClient
    {
        private const string ApVersion = "0.3.5";
        public static APData ServerData = new();

        private static int lastTime;
        private static float disconnectTimeout = 5.0f;
        public static bool Authenticated { get; private set; }

        public static ArchipelagoSession Session;
        public static DeathLinkService DeathLinkService;

        /// <summary>
        /// Creates a thread worker and uses it to call Connect with the params.
        /// </summary>
        /// <param name="uri">url that we're attempting to connect to.</param>
        /// <param name="port">port for the url we need to connect to.</param>
        /// <param name="slotName">username</param>
        /// <param name="password">server password</param>
        public static void ConnectAsync(string uri, int port, string slotName, string password)
        {
            ThreadPool.QueueUserWorkItem((_) => Connect(uri, port, slotName, password));
        }

        /// <summary>
        /// Attempts to connect the Archipelago server and throws (hopefully detailed) errors otherwise. Assigns all
        /// params to Archipelago.ServerData for later access and saving.
        /// </summary>
        /// <param name="uri">url we're attempting to connect to.</param>
        /// <param name="port">port for the url to connect to.</param>
        /// <param name="slotName">username</param>
        /// <param name="password">server password</param>
        private static void Connect(string uri, int port, string slotName, string password)
        {   
            // hopefully this will only happen if the user manually attempts to connect to a different server while
            // already connected. If not add some sort of check here.
            if (Authenticated)
            {
                Game1.exitActiveMenu();
                Game1.activeClickableMenu = new AlreadyConnectedMenu();
                return;
            }

            LoginResult result;
            
            // assign args to ServerData
            ServerData.Uri = uri;
            ServerData.Port = port;
            ServerData.SlotName = slotName;
            ServerData.Password = password;
            
            // start up a session
            Session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri, ServerData.Port);
            Session.MessageLog.OnMessageReceived += OnMessageReceived;
            Session.Socket.ErrorReceived += SessionErrorReceived;
            Session.Socket.SocketClosed += SessionSocketClosed;

            try
            {
                Utils.LogMessage("Attempting Connection...");
                result = Session.TryConnectAndLogin(
                    "BatBoy",
                    ServerData.SlotName,
                    ItemsHandlingFlags.AllItems,
                    new Version(ApVersion),
                    null,
                    "",
                    ServerData.Password == "" ? null : ServerData.Password
                );
            }
            catch (Exception e)
            {
                Utils.LogMessage(e.GetBaseException().Message, Enums.MessageType.Error);
                result = new LoginFailure(e.GetBaseException().Message);
            }
            
            Utils.LogMessage($"Connection Result: {result.Successful}");

            if (!result.Successful)
            {
                var failure = (LoginFailure)result;
                var errorMessage = $"Failed to Connect to {ServerData.Uri} as {ServerData.SlotName}:";
                errorMessage +=
                    failure.Errors.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");
                errorMessage +=
                    failure.ErrorCodes.Aggregate(errorMessage, (current, error) => current + $"\n   {error}");

                Utils.LogMessage($"Failed to connect: {errorMessage}", Enums.MessageType.Error);

                Authenticated = false;
                Disconnect();
                Game1.exitActiveMenu();
                Game1.activeClickableMenu = new FailureMenu();
            }
            else
            {
                var success = (LoginSuccessful)result;
                ServerData.SlotData ??= success.SlotData;
                Authenticated = true;
                Game1.exitActiveMenu();
                Game1.activeClickableMenu = new SuccessMenu();
            }
        }

        private static void OnMessageReceived(LogMessage message)
        {
            var messageType = message switch
            {
                HintItemSendLogMessage => Enums.MessageType.Hint,
                ItemSendLogMessage => Enums.MessageType.Item,
                CountdownLogMessage => Enums.MessageType.Countdown,
                _ => Enums.MessageType.Default
            };
            Utils.LogMessage(message.ToString(), messageType);
        }

        private static void SessionErrorReceived(Exception e, string message)
        {
            Utils.LogMessage(message, Enums.MessageType.Error);
            Disconnect();
        }

        private static void SessionSocketClosed(string reason)
        {
            Utils.LogMessage($"Connection to Archipelago lost: {reason}", Enums.MessageType.Error);
            Disconnect();
        }

        public static void Disconnect()
        {
            if (Session is { Socket: { } })
                Session.Socket.DisconnectAsync();
            Session = null;
            Authenticated = false;
        }

        public static void APUpdate()
        {
            if (!Authenticated)
            {
                var now = DateTime.Now.Second;
                var dT = now - lastTime;
                lastTime = now;
                disconnectTimeout -= dT;
                if (!(disconnectTimeout <= 0.0f)) return;
                
                ConnectAsync(ServerData.Uri, ServerData.Port, ServerData.SlotName, ServerData.Password);
                disconnectTimeout = 5.0f;
                return;
            }
            Utils.PrintMessages();
            if (ServerData.Index >= Session.Items.AllItemsReceived.Count) return;
            var currentItemId = Session.Items.AllItemsReceived[Convert.ToInt32(ServerData.Index)].Item;
            ++ServerData.Index;
            ItemsAndLocationsHandler.Unlock(currentItemId);
        }
    }
}