using System;
using Archipelago.Stardew.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace Archipelago.Stardew.Menus
{
    public class ConnectMenu: IClickableMenu
    {
        // Constants
        private const int UiWidth = 800;
        private const int UiHeight = 800;

        private readonly int xPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiWidth / 2);
        private readonly int yPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiHeight / 2);

        // UI Elements
        private readonly ClickableTextureComponent okButton;

        // connection info
        private readonly TextBox uriTextBox, slotNameTextBox, passwordTextBox;

        public ConnectMenu()
        {
            initialize(xPos, yPos, UiWidth, UiHeight);
            
            // Confirmation Button
            okButton = new ClickableTextureComponent("OK",
                new Rectangle(xPos + UiWidth - Game1.tileSize * 2, yPos + UiHeight - (int)(Game1.tileSize * 1.75f),
                    Game1.tileSize, Game1.tileSize), "", null, Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
            
            
            // Text Boxes
            uriTextBox = new TextBox(null, null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + spaceToClearSideBorder + borderWidth + Game1.tileSize,
                Y = yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize,
                Width = width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 3,
                Height = 180
            };
            Game1.keyboardDispatcher.Subscriber = uriTextBox;
            
            slotNameTextBox = new TextBox(null, null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + spaceToClearSideBorder + borderWidth + Game1.tileSize,
                Y = yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize * 5,
                Width = width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 3,
                Height = 180
            };
            
            passwordTextBox = new TextBox(null, null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + spaceToClearSideBorder + borderWidth + Game1.tileSize,
                Y = yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize * 7,
                Width = width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 3,
                Height = 180
            };
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            uriTextBox.Update();
            slotNameTextBox.Update();
            passwordTextBox.Update();

            if (okButton.containsPoint(x, y) && IsOkButtonReady())
            {
                HandleButtonClick(okButton.name);
                okButton.scale -= 0.25f;
                okButton.scale = Math.Max(0.75f, okButton.scale);
            }
        }

        private void HandleButtonClick(string name)
        {
            if (name == null) return;
            switch (name)
            {
                case "OK":
                    Game1.playSound("coin");
                    if (IsOkButtonReady())
                    {
                        var uri = uriTextBox.Text;
                        if (uri == "") uri = "archipelago.gg:38281";
                        var port = 38281;
                        var slotName = slotNameTextBox.Text;
                        var password = passwordTextBox.Text ?? "";

                        if (uri.Contains(":"))
                        {
                            var splits = uri.Split(":");
                            uri = splits[0];
                            int.TryParse(splits[1], out port);
                        }

                        Utils.LogMessage($"Attempting Connection with: {uri}, {port}, {slotName}, {password}");
                        ArchipelagoClient.ConnectAsync(uri, port, slotName, password);
                    }
                    break;
            }
            
        }

        private bool IsOkButtonReady()
        {
            return !string.IsNullOrEmpty(slotNameTextBox.Text);
        }

        public override void draw(SpriteBatch b)
        {
            Utils.Helper.Input.Suppress(SButton.E);
            
            // draw screen fade
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            
            // draw menu dialogue box
            Game1.drawDialogueBox(xPos, yPos, UiWidth, UiHeight, false, true);
            
            // draw TitleLabel
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                "Connect to Archipelago",
                Game1.options.uiScale <= 1.25f ? xPos + (UiWidth / 2) : xPos - Game1.tileSize * 2 - 32,
                Game1.options.uiScale <= 1.25f ? yPos - (Game1.tileSize / 4) : yPos + Game1.tileSize * 3,
                "Connect to Archipelago"
            );
            
            // draw connection text headers
            SpriteText.drawStringWithScrollBackground(
                b,
                "Host",
                xPositionOnScreen - spaceToClearSideBorder - borderWidth - Game1.tileSize,
                yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize,
                "Host"
                );
            SpriteText.drawStringWithScrollBackground(
                b,
                "Player Name",
                xPositionOnScreen - borderWidth - (int)(Game1.tileSize * 3.75f),
                yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize * 5,
                "Player Name"
            );
            SpriteText.drawStringWithScrollBackground(
                b,
                "Password",
                xPositionOnScreen - (int)(Game1.tileSize * 3.4f),
                yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize * 7,
                "Password"
            );
            
            // draw uri Input
            uriTextBox.Draw(b, false);
            slotNameTextBox.Draw(b, false);
            passwordTextBox.Draw(b, false);
            
            // draw connect button
            okButton.draw(b);

            // draw cursor last
            drawMouse(b);
        }
    }

    public class SuccessMenu : IClickableMenu
    {
        // Constants
        private const int UiWidth = 800;
        private const int UiHeight = 800;

        private readonly int xPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiWidth / 2);
        private readonly int yPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiHeight / 2);

        public SuccessMenu()
        {
            initialize(xPos, yPos, UiWidth, UiHeight);
        }

        public override void draw(SpriteBatch b)
        {
            
            // draw screen fade
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            
            // draw TitleLabel
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                $"Successfully Connected to {ArchipelagoClient.ServerData.Uri}:{ArchipelagoClient.ServerData.Port}",
                Game1.options.uiScale <= 1.25f ? xPos + (UiWidth / 2) : xPos - Game1.tileSize * 2 - 32,
                Game1.options.uiScale <= 1.25f ? yPos - (Game1.tileSize / 4) : yPos + Game1.tileSize * 3,
                $"Successfully Connected to {ArchipelagoClient.ServerData.Uri}:{ArchipelagoClient.ServerData.Port}"
            );
            
            // draw cursor last
            drawMouse(b);
        }
    }
    
    public class FailureMenu : IClickableMenu
    {
        // Constants
        private const int UiWidth = 800;
        private const int UiHeight = 800;

        private readonly int xPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiWidth / 2);
        private readonly int yPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiHeight / 2);

        public FailureMenu()
        {
            initialize(xPos, yPos, UiWidth, UiHeight);
        }

        public override void draw(SpriteBatch b)
        {
            
            // draw screen fade
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            
            // draw TitleLabel
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                $"Failed to connect to {ArchipelagoClient.ServerData.Uri}:{ArchipelagoClient.ServerData.Port}." +
                $" Check entry was correct.",
                Game1.options.uiScale <= 1.25f ? xPos + (UiWidth / 2) : xPos - Game1.tileSize * 2 - 32,
                Game1.options.uiScale <= 1.25f ? yPos - (Game1.tileSize / 4) : yPos + Game1.tileSize * 3,
                $"Failed to connect to" +
                $" {ArchipelagoClient.ServerData.Uri}:{ArchipelagoClient.ServerData.Port}. Check entry was correct."
            );
            
            // draw cursor last
            drawMouse(b);
        }
    }    
    public class AlreadyConnectedMenu : IClickableMenu
    {
        // Constants
        private const int UiWidth = 800;
        private const int UiHeight = 800;

        private readonly int xPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiWidth / 2);
        private readonly int yPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiHeight / 2);

        public AlreadyConnectedMenu()
        {
            initialize(xPos, yPos, UiWidth, UiHeight);
        }

        public override void draw(SpriteBatch b)
        {
            
            // draw screen fade
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            
            // draw TitleLabel
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                $"Already connected to {ArchipelagoClient.ServerData.Uri}:{ArchipelagoClient.ServerData.Port}.",
                Game1.options.uiScale <= 1.25f ? xPos + (UiWidth / 2) : xPos - Game1.tileSize * 2 - 32,
                Game1.options.uiScale <= 1.25f ? yPos - (Game1.tileSize / 4) : yPos + Game1.tileSize * 3,
                $"Failed to connect to" +
                $"Already connected to {ArchipelagoClient.ServerData.Uri}:{ArchipelagoClient.ServerData.Port}."
            );
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                "If you would like to connect to a different server, please relaunch the game.",
                Game1.options.uiScale <= 1.25f ? xPos + (UiWidth / 2) : xPos - Game1.tileSize * 2 - 32,
                yPos * 2,
                $"Failed to connect to" +
                "If you would like to connect to a different server, please relaunch the game."
            );
            
            // draw cursor last
            drawMouse(b);
        }
    }
}