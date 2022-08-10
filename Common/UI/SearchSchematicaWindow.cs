using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Schematica.Common.DataStructures;
using Schematica.Common.Enums;
using Schematica.Common.Systems;
using Schematica.Common.UI.UIElements;
using Schematica.Core;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Capture;
using Terraria.Localization;
using Terraria.UI;

namespace Schematica.Common.UI;

public class SchematicaUISystem : UISystem<SchematicaWindowState>
{
    public override InterfaceScaleType InterfaceScaleType => InterfaceScaleType.UI;
    public override VanillaInterfaceLayerID VanillaInterfaceLayer => VanillaInterfaceLayerID.Interface_Logic_1;
    //Not displaying above the map

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
    private CancellationTokenSource cancellationTokenSource;
    private Task importSchematica;

    public override void OnInitialize() {
        cancellationTokenSource = new CancellationTokenSource();

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

        RepopulateSchematicas();
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
        UIImageButton searchCancelCross = new(texture) {
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

    public void TryRemoveAccordionItem(string title) => accordion.accordianItems.Find(x => x.Title == title)?.Remove();

    public void RepopulateSchematicas() {
        List<string> validSchematicaNames = SchematicaFileFormat.GetValidSchematicas();
        List<UIAccordionItem> accordionItems = new(validSchematicaNames.Count);

        for (int i = 0; i < validSchematicaNames.Count; i++) { accordionItems.Add(new SchematicaPreviewUIElement(validSchematicaNames[i], accordion.ItemHeight, 308)); }

        accordion.UpdateItems(accordionItems);
    }

    // private void SchematicaHeaderClick(List<string> schematicaNames, int index, UIImage thumbnail) {
    //     string name = schematicaNames[index];
    //     if (Schematica.CurrentPreview?.Name == name || Schematica.PlacedSchematicas.Any(x => x.Name == name))
    //         return;
    //     
    //     if (!importSchematica?.IsCompleted ?? false)
    //         cancellationTokenSource.Cancel();
    //
    //     importSchematica = Task.Factory.StartNew(() => SchematicaFileFormat.ImportSchematica(name), cancellationTokenSource.Token)
    //         .ContinueWith(
    //             task => {
    //                 cancellationTokenSource = new CancellationTokenSource();
    //
    //                 if (task.IsCanceled)
    //                     return;
    //
    //                 Schematica.CurrentPreview = task.Result;
    //                 Schematica.PlacedSchematicas.Add(task.Result);
    //                 Schematica.GeneratePreviewQueue.Enqueue(() => {
    //                         // Texture2D asd = Schematica.CurrentPreview.GeneratePreview();
    //                         // thumbnail.SetImage(asd);
    //                     }
    //                 );
    //
    //                 //End loading animation
    //             });
    //
    //     //Start loading animation
    // }
}