using System;
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
using Terraria.ModLoader;
using Terraria.UI;

namespace Schematica.Common.UI;

public class SchematicaUISystem : UISystem<SchematicaWindowState>
{
    public override InterfaceScaleType InterfaceScaleType => InterfaceScaleType.UI;
    public override VanillaInterfaceLayerID VanillaInterfaceLayer => VanillaInterfaceLayerID.Capture_Manager_Check;
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
    private Task importSchematica;

    private UIPanel searchAreaPanel;
    private UISearchBar searchBar;
    private bool isExpanded;

    public override void OnInitialize() {
        Width.Set(500f, 0f);
        Height.Set(500f, 0f);
        MarginLeft = 24f;
        MarginTop = 120f;
        // Left.Set(24f, 0f);
        // Top.Set(120f, 0f);
        SetPadding(6f);

        canDrag = false;
        // OnLeftMouseDown += (element, _) => {
        //     Vector2 MenuPosition = new Vector2(Left.Pixels, Top.Pixels);
        //     Vector2 clickPos = Vector2.Subtract(element.MousePosition, MenuPosition);
        //     canDrag = clickPos.Y <= 25;
        // };

        // Refresh button
        var refreshButton = new UIColoredImageButton(ModContent.Request<Texture2D>("Schematica/Assets/UI/Refresh", AssetRequestMode.ImmediateLoad), true) {
            MarginLeft = 4f,
            MarginTop = 2f,
            Width = new StyleDimension(26f, 0f),
            Height = new StyleDimension(26f, 0f)
        };

        refreshButton.OnLeftClick += (evt, element) => {
            RepopulateSchematicas();
        };

        Append(refreshButton);

        // Expand menu arrow
        var arrowAsset = Main.Assets.Request<Texture2D>("Images/UI/TexturePackButtons", AssetRequestMode.ImmediateLoad);
        var expandArrow = new UIImageFramed(arrowAsset, arrowAsset.Frame(2, 2, 1, 1)) {
            HAlign = 1f,
            VAlign = 0f,
            Width = new StyleDimension(32f, 0f),
            Height = new StyleDimension(32f, 0f)
        };

        expandArrow.OnLeftClick += (evt, element) => {
            isExpanded = !isExpanded;

            expandArrow.SetFrame(arrowAsset.Frame(2, 2, (!isExpanded).ToInt(), 1));
            expandArrow.Left.Set(isExpanded ? 10f : 0f, 0f);
        };

        Append(expandArrow);

        AddSearchBar(this);

        //Accordion
        accordion = new UIAccordion(itemHeight: 40) {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(-35f, 1f),
            Top = new StyleDimension(35f, 0f)
        };

        Append(accordion);

        RepopulateSchematicas();
    }

    private void AddSearchBar(UIElement parent) {
        //WIP
        searchAreaPanel = new UIPanel() {
            MarginLeft = 36f,
            Width = new StyleDimension(-24f - 36f, 1f),
            Height = new StyleDimension(32f, 0f),
            BackgroundColor = new Color(35, 40, 83),
            BorderColor = new Color(35, 40, 83)
        };

        searchAreaPanel.SetPadding(0f);

        searchBar = new UISearchBar(LocalizedText.Empty, 1f) {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(24f, 0)
        };

        searchAreaPanel.Append(searchBar);

        var searchCancelCross = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
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

        UpdateReactiveComponents();

        accordion.RecalculateChildren();
    }

    public void TryRemoveAccordionItem(string title) => accordion.accordionItems.Find(x => x.Title == title)?.Remove();

    public void RepopulateSchematicas() {
        var validSchematicas = SchematicaFileFormat.GetValidAndUnloadedSchematicasMetadata();
        var accordionItems = new List<UIAccordionItem>(validSchematicas.Count);

        for (int i = 0; i < validSchematicas.Count; i++) { accordionItems.Add(new SchematicaAccordionItem(validSchematicas[i], accordion.ItemHeight)); }

        accordion.UpdateItems(accordionItems);
    }

    private void UpdateReactiveComponents() {
        var targetWidth = isExpanded ? Math.Min(Main.screenWidth - 24f * 2f, 800f) : 500f;
        // var targetHeight = isExpanded ? Height.Pixels + (Main.screenHeight - Height.Pixels) / 2f - 72f : 450f;
        var targetHeight = Height.Pixels + (Main.screenHeight - Height.Pixels) / 2f - 72f;

        Width.Set(MathHelper.Lerp(Width.Pixels, targetWidth, 0.25f), 0f);
        Height.Set(MathHelper.Lerp(Height.Pixels, targetHeight, 0.25f), 0f);

        Recalculate();
    }
}