using System.Linq;
using Archipelago.Stardew.ItemsAndLocations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace Archipelago.Stardew.Menus
{
    public class ArchipelagoMenu: IClickableMenu
    {
        private const int UiWidth = 800;
        private const int UiHeight = 800;
        private readonly int xPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiWidth / 2);
        private readonly int yPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 -
                                    (UiHeight / 2);

        private readonly ClickableTextureComponent inventoryButton, hintItemButton, hintLocationButton;

        private readonly TextBox hintItemText, hintLocationText;

        public ArchipelagoMenu()
        {
            initialize(xPos, yPos, UiWidth, UiHeight, true);

            inventoryButton = new ClickableTextureComponent("Archipelago Inventory",
                new Rectangle(xPos + UiWidth - Game1.tileSize * 2, yPos + UiHeight - (int)(Game1.tileSize * 1.75f),
                    Game1.tileSize, Game1.tileSize), 
                "", null, Game1.mouseCursors, 
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
            
            hintItemButton = new ClickableTextureComponent("Hint Item",
                new Rectangle(xPos + UiWidth - Game1.tileSize * 2,
                    yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize,
                    Game1.tileSize, Game1.tileSize), 
                "", null, Game1.mouseCursors, 
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);

            hintItemText = new TextBox(null, null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + spaceToClearSideBorder + borderWidth + Game1.tileSize,
                Y = yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize,
                Width = width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 3,
                Height = 180
            };
            
            hintLocationButton = new ClickableTextureComponent("Hint Location",
                new Rectangle(xPos + UiWidth - Game1.tileSize * 2,
                    yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize * 3,
                    Game1.tileSize, Game1.tileSize), 
                "", null, Game1.mouseCursors, 
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
            
            hintLocationText = new TextBox(null, null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + spaceToClearSideBorder + borderWidth + Game1.tileSize,
                Y = yPositionOnScreen + spaceToClearTopBorder + borderWidth + Game1.tileSize * 3,
                Width = width - borderWidth - spaceToClearSideBorder - Game1.tileSize * 3,
                Height = 180
            };
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            hintItemText.Update();
            hintLocationText.Update();
            if (inventoryButton.containsPoint(x, y))
            {
                HandleButtonClick(inventoryButton.name);
            }
            else if (hintItemButton.containsPoint(x, y))
            {
                HandleButtonClick(hintItemButton.name);
            }
            else if (hintLocationButton.containsPoint(x, y))
            {
                HandleButtonClick(hintLocationButton.name);
            }
        }

        private void HandleButtonClick(string name)
        {
            if (name == null) return;

            switch (name)
            {
                case "Archipelago Inventory":
                    Game1.exitActiveMenu();
                    Game1.activeClickableMenu = new ArchipelagoShop();
                    break;
                case "Hint Item":
                    var item = Utils.ToTitle(hintItemText.Text);
                    if (SVObjectLookup.AllItems.Keys.Contains(item))
                        Utils.LogMessage($"Requesting a hint for {item}");
                    else
                        Utils.LogMessage($"{item} is not a valid item name!", Enums.MessageType.Error);
                    break;
                case "Hint Location":
                    var location = Utils.ToTitle(hintLocationText.Text);
                    Utils.LogMessage($"Requesting a hint for location {location}");
                    break;
                default:
                    Utils.LogMessage($"Unexpected button click name in {this}: {name}");
                    break;
            }
        }

        public override void draw(SpriteBatch b)
        {
            Utils.Helper.Input.Suppress(SButton.E);

            Game1.drawDialogueBox(xPos, yPos, UiWidth, UiHeight, false, true);
            
            // draw TitleLabel
            SpriteText.drawStringWithScrollCenteredAt(
                b,
                "Archipelago Commands Menu",
                Game1.options.uiScale <= 1.25f ? xPos + (UiWidth / 2) : xPos - Game1.tileSize * 2 - 32,
                Game1.options.uiScale <= 1.25f ? yPos - (Game1.tileSize / 4) : yPos + Game1.tileSize * 3,
                "Archipelago Commands Menu"
                );

            hintItemText.Draw(b, false);
            hintLocationText.Draw(b, false);

            hintItemButton.draw(b);
            hintLocationButton.draw(b);
            inventoryButton.draw(b);
            
            drawMouse(b);
        }
    }
}