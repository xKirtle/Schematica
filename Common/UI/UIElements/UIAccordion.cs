using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

//WARNING: The UIGrid has no limit of items.. could affect performance if thousands of entries?
public class UIAccordion : UIElement
{
    public List<UIAccordionItem> accordionItems;
    public SmoothUIGrid Items;
    public UIScrollbar scrollbar;

    public UIAccordionItem SelectedItem { get; private set; }
    public int ItemHeight { get; private set; }

    public UIAccordion(int itemHeight) {
        ItemHeight = itemHeight;
        accordionItems = new List<UIAccordionItem>();

        Items = new SmoothUIGrid() {
            Width = new StyleDimension(-20f - 3f, 1f),
            Height = StyleDimension.Fill,
            ListPadding = 5f
        };

        scrollbar = new UIScrollbar() {
            Width = new StyleDimension(20f, 0f),
            Height = new StyleDimension(-16f, 1f),
            Left = new StyleDimension(-20f, 1f),
            // Top = new StyleDimension(5f, 0f),
            MarginTop = 5f,
            MarginBottom = 5f
        };

        Items.SetScrollbar(scrollbar);

        Append(Items);
        Append(scrollbar);
    }

    public void UpdateItems(List<UIAccordionItem> list) {
        Clear();

        accordionItems.AddRange(list);
        Items.AddRange(list);
        Items.UpdateOrder();

        //Bind events
        foreach (UIAccordionItem accordianItem in accordionItems) {
            accordianItem.HeaderOnClick += (__, _) => {
                if (SelectedItem != accordianItem)
                    SelectedItem?.ToggleOpen();

                accordianItem.ToggleOpen();
                SelectedItem = accordianItem.IsOpen ? accordianItem : null;
            };
        }
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        foreach (var accordionItem in accordionItems) {
            accordionItem.Update(gameTime);
        }
    }

    public void Clear() {
        accordionItems.Clear();
        Items.Clear();
    }
}