using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Schematica.Common.DataStructures;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class SchematicaPreviewUIElement : UIAccordionItem
{
    public SchematicaData Schematica { get; private set; }
    
    private UIImage thumbnailImage;

    public SchematicaPreviewUIElement(string title, int headerHeight, int bodyHeight) : base(title, headerHeight, bodyHeight) {
        UIPanel thumbnailPanel = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(240f, 0f), //Can't make square by getting width's pixel value?
            BackgroundColor = new Color(35, 40, 83)
        };
        
        Body.Append(thumbnailPanel);
        
        thumbnailImage = new UIImage(Asset<Texture2D>.Empty) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        };
        
        thumbnailPanel.Append(thumbnailImage);
        
        UIButton importButton = new UIButton("Import & Place Schematica") {
            Width  = StyleDimension.Fill,
            Height = new StyleDimension(35f, 0f),
            Top = new StyleDimension(-2f, 0f),
            HAlign = 1f,
            VAlign = 1f
        };
        
        Body.Append(importButton);
        
        importButton.OnClick += ImportButtonOnClick;
    }
    private void ImportButtonOnClick(UIMouseEvent evt, UIElement button) {
        Console.WriteLine("Import and Place logic happens here");
    }

    public void SetSchematica(SchematicaData schematica) {
        Schematica = schematica;
        thumbnailImage.SetImage(Schematica.PreviewThumbnail);
    }
}