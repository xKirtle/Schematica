﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Schematica.Common.DataStructures;
using Schematica.Common.UI.UIElements;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI;

public class SchematicsWindow : DraggableUIPanel
{
    private UIAccordion accordion;
    public override void OnInitialize() {
        Width.Set(300f, 0f);
        Height.Set(500f, 0f);
        Left.Set(24f, 0f);
        Top.Set(120f, 0f);
        SetPadding(6f);

        OnMouseDown += (element, _) => {
            // Vector2 MenuPosition = new Vector2(Left.Pixels, Top.Pixels);
            // Vector2 clickPos = Vector2.Subtract(element.MousePosition, MenuPosition);
            // canDrag = clickPos.Y <= 25;
            canDrag = false;
        };

        AddSearchBar(this);
        
        //Accordion
        accordion = new UIAccordion(itemHeight: 40) {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(-30f, 1f),
            Top = new StyleDimension(30f, 0f)
        };
        
        Append(accordion);

        //Test, remove later
        List<UIAccordionItem> testList = new List<UIAccordionItem>();
        for (int i = 0; i < 105; i++) {
            testList.Add(new UIAccordionItem($"Item {i + 1 + Main.rand.Next(0, 20)}", headerHeight: accordion.ItemHeight, bodyHeight: 250));
        }

        accordion.UpdateItems(testList);
    }

    private UIPanel searchAreaPanel;
    private UISearchBar searchBar;
    private void AddSearchBar(UIElement parent) {
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
    
    //Only have one loaded schematic at a time (the one currently selected by the accordion)

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        
        Schematica.CanSelectEdges = true;
        if (IsMouseHovering) {
            Schematica.CanSelectEdges = false;
            Main.LocalPlayer.mouseInterface = true;
        }
        
        //Smooth Scroll
        // accordion.Items.OnScrollWheel += (evt, element) => {
        //     Console.WriteLine(evt.ScrollWheelValue);
        // };

        // Console.WriteLine(accordion.scrollbar.ViewPosition);
    }
    
    //MakeThumbnail -> What makes paint tools thumbnails
}