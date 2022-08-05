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

public class UIAccordion : UIElement
{
    private List<UIAccordionItem> accordianItems;
    public UIGrid Items;
    private UIScrollbar scrollbar;
    
    public UIAccordionItem SelectedItem { get; private set; }
    public int ItemHeight { get; private set; }
    
    //Accordion will handle the unopened items height
    //Each item can then specify how much space its body will take when selected

    public UIAccordion(int itemHeight) {
        ItemHeight = itemHeight;
        accordianItems = new List<UIAccordionItem>();
        
        Items = new UIGrid() {
            Width = new StyleDimension(-20f - 5f, 1f),
            Height = StyleDimension.Fill,
        };

        scrollbar = new UIScrollbar() {
            Width = new StyleDimension(20f, 0f),
            Height = new StyleDimension(-10f, 1f),
            Left = new StyleDimension(-20f, 1f),
            Top = new StyleDimension(5f, 0f)
        };

        Items.SetScrollbar(scrollbar);

        Append(Items);
        Append(scrollbar);
    }

    public void UpdateItems(List<UIAccordionItem> list) {
        accordianItems.Clear();
        Items.Clear();

        accordianItems.AddRange(list);
        accordianItems.Sort(new NaturalComparer());

        accordianItems.ForEach(item => Items.Add(item));
        
        //Bind events

        foreach (UIAccordionItem accordianItem in accordianItems) {
            accordianItem.HeaderClick += () => {
                //Change arrow
                //Smoothly increase height
                
                accordianItem.ToggleOpen();
                // Recalculate();
            };
        }
        
    }
}