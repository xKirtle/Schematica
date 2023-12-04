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
        Width.Set(600f, 0f);
        Height.Set(400f, 0f);
        Left.Set(24f, 0f);
        Top.Set(120f, 0f);
        SetPadding(6f);

        canDrag = false;
        // OnLeftMouseDown += (element, _) => {
        //     Vector2 MenuPosition = new Vector2(Left.Pixels, Top.Pixels);
        //     Vector2 clickPos = Vector2.Subtract(element.MousePosition, MenuPosition);
        //     canDrag = clickPos.Y <= 25;
        // };
        
        // Refresh button
        var refreshButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
            MarginTop = 4f
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
            Height = new StyleDimension(-30f, 1f),
            Top = new StyleDimension(35f, 0f)
        };

        Append(accordion);

        RepopulateSchematicas();
    }
    
    private void AddSearchBar(UIElement parent) {
        //WIP
        searchAreaPanel = new UIPanel() {
            MarginLeft = 26f,
            MarginRight = 26f,
            Width = new StyleDimension(-52f, 1f),
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
        
        for (int i = 0; i < validSchematicas.Count; i++) {
            accordionItems.Add(new SchematicaAccordionItem(validSchematicas[i], accordion.ItemHeight, 308));
        }

        accordion.UpdateItems(accordionItems);
    }

    private void UpdateReactiveComponents() {
        var targetWidth = isExpanded ? Math.Min(Main.screenWidth - 24f * 2f, 1200f) : 600f;
        var targetHeight = isExpanded ? Height.Pixels + (Main.screenHeight - Height.Pixels) / 2f - 72f : 400f;
        
        Width.Set(MathHelper.Lerp(Width.Pixels, targetWidth, 0.25f), 0f);
        Height.Set(MathHelper.Lerp(Height.Pixels, targetHeight, 0.25f), 0f);

        // var searchAreaWidthPercentage = isExpanded ? 0.95f : 0.8f; 
        // searchAreaPanel.Width.Set(0f, MathHelper.Lerp(searchAreaPanel.Width.Percent, searchAreaWidthPercentage, 0.2f));
        
        Recalculate();
    }
}