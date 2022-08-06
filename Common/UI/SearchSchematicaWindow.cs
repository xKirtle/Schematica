using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Schematica.Common.DataStructures;
using Schematica.Common.Enums;
using Schematica.Common.Systems;
using Schematica.Common.UI.UIElements;
using Schematica.Core;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Capture;
using Terraria.Localization;
using Terraria.UI;

namespace Schematica.Common.UI;

public class SchematicaUISystem : UISystem<SchematicaWindowState>
{
    public override InterfaceScaleType InterfaceScaleType => InterfaceScaleType.UI;
    public override VanillaInterfaceLayerID VanillaInterfaceLayer => VanillaInterfaceLayerID.Interface_Logic_1;

    public override void Load() {
        base.Load();
        Deactivate();
    }
}

public class SchematicaWindowState : UIState
{
    public static SchematicaWindowState Instance;
    public SearchSchematicaWindow WindowElement;

    public override void OnInitialize() {
        Instance = this;
        
        WindowElement = new SearchSchematicaWindow();
        Append(WindowElement);
    }

    public override void Draw(SpriteBatch spriteBatch) {
        if (CaptureManager.Instance.Active)
            base.Draw(spriteBatch);
    }
}

public class SearchSchematicaWindow : DraggableUIPanel
{
    private UIAccordion accordion;
    private CancellationTokenSource cancelTokenSource;

    public SearchSchematicaWindow() => cancelTokenSource = new CancellationTokenSource();

    public override void OnInitialize() {
        Width.Set(300f, 0f);
        Height.Set(500f, 0f);
        Left.Set(24f, 0f);
        Top.Set(120f, 0f);
        SetPadding(6f);

        canDrag = false;
        // OnMouseDown += (element, _) => {
        //     Vector2 MenuPosition = new Vector2(Left.Pixels, Top.Pixels);
        //     Vector2 clickPos = Vector2.Subtract(element.MousePosition, MenuPosition);
        //     canDrag = clickPos.Y <= 25;
        // };

        AddSearchBar(this);
        
        //Accordion
        accordion = new UIAccordion(itemHeight: 40) {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(-30f, 1f),
            Top = new StyleDimension(30f, 0f)
        };
        
        Append(accordion);
        
        TestingRepopulateWindow();
    }

    private UIPanel searchAreaPanel;
    private UISearchBar searchBar;
    private void AddSearchBar(UIElement parent) {
        //WIP
        searchAreaPanel = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(25f, 0f),
            BackgroundColor = new Color(35, 40, 83),
            BorderColor = new Color(35, 40, 83)
        };
        searchAreaPanel.SetPadding(0f);

        searchBar = new UISearchBar(LocalizedText.Empty, 1f) {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(24f, 0)
        };

        searchAreaPanel.Append(searchBar);

        Asset<Texture2D> texture = Main.Assets.Request<Texture2D>("Images/UI/SearchCancel");
        UIImageButton searchCancelCross = new UIImageButton(texture) {
            HAlign = 1f,
            VAlign = 0.5f,
            Left = new StyleDimension(-2f, 0f)
        };

        searchAreaPanel.Append(searchCancelCross);
        
        parent.Append(searchAreaPanel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        Schematica.CanSelectEdges = true;
        if (IsMouseHovering) {
            Schematica.CanSelectEdges = false;
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    public void TestingRepopulateWindow() {
        List<string> fileNames = Utilities.FileNamesInDirectory($@"{Path.Combine(Main.SavePath)}\Schematica");
        
        //Test, remove later
        List<UIAccordionItem> testList = new List<UIAccordionItem>(fileNames.Count);
        for (int i = 0; i < fileNames.Count; i++) {
            UIAccordionItem tempItem = new UIAccordionItem(fileNames[i], headerHeight: accordion.ItemHeight, bodyHeight: 308);
            
            UIPanel thumbnailPanel = new UIPanel() {
                Width = StyleDimension.Fill,
                Height = new StyleDimension(240f, 0f), //Can't make square by getting width's pixel value?
                BackgroundColor = new Color(150, 40, 83)
            };

            tempItem.Body.Append(thumbnailPanel);

            UIButton button = new UIButton("Place Schematica") {
                Width = StyleDimension.Fill,
                Height = new StyleDimension(35f, 0f),
                Top = new StyleDimension(-2f, 0f),
                VAlign = 1f
            };
            
            tempItem.Body.Append(button);

            int index = i;
            button.OnClick += (__, _) => {
                Console.WriteLine($"{Schematica.currentPreview.Name} {Schematica.placedSchematics.Count}");
                Schematica.placedSchematics.Add(Schematica.currentPreview);
                Console.WriteLine($"New count: {Schematica.placedSchematics.Count}");
            };
            
            tempItem.HeaderClick += () => {
                Main.NewText(fileNames[index]);
                // cancelTokenSource.Cancel();
                // Task loadingSchematic = new Task(() => {
                //     SchematicaData.LoadSchematic(fileNames[index]);
                //     Main.NewText(Schematica.placedSchematics.Count);
                // }, cancelTokenSource.Token);
                
                SchematicaData.LoadSchematic(fileNames[index]);
                Main.NewText(Schematica.placedSchematics.Count);
            };
            
            testList.Add(tempItem);
        }

        accordion.UpdateItems(testList);
    }

    //MakeThumbnail -> What makes paint tools thumbnails
}