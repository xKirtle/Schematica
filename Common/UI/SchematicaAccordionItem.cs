using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Schematica.Common.DataStructures;
using Schematica.Core;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class SchematicaAccordionItem : UIAccordionItem
{
    public SchematicaData Schematica { get; private set; }

    private UIPanel panel;
    private UISchematicaThumbnail thumbnail;
    private UIImage previewImage;
    private UIText width;
    private UIText height;
    private CancellationTokenSource cancellationTokenSource;

    public SchematicaAccordionItem(SchematicaData schematicaData, int headerHeight, int bodyHeight) : base(schematicaData.DisplayName, headerHeight, bodyHeight) {
        cancellationTokenSource = new CancellationTokenSource();
        
        panel = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(240f, 0f), //Can't make square by getting width's pixel value?
            BackgroundColor = new Color(35, 40, 83),
            OverflowHidden = true
        };

        Body.Append(panel);

        width = new UIText("Width: ??") {
            HAlign = 0f
        };
        
        panel.Append(width);
        
        height = new UIText("Height: ??") {
            HAlign = 1f
        };
        
        panel.Append(height);

        // thumbnail = new UISchematicaThumbnail() {
        //     Width = StyleDimension.Fill,
        //     Height = new StyleDimension(-35f, 1f),
        //     Top = new StyleDimension(35f, 0)
        // };

        // panel.Append(thumbnail);
        
        previewImage = new UIImage(Asset<Texture2D>.Empty) {
            HAlign = 0.5f,
            Width = StyleDimension.Fill,
            Height = new StyleDimension(200f, 1f),
            Top = new StyleDimension(35f, 0)
        };
        
        panel.Append(previewImage);

        UIButton importButton = new("Import & Place Schematica") {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(35f, 0f),
            Top = new StyleDimension(-2f, 0f),
            HAlign = 1f,
            VAlign = 1f
        };

        Body.Append(importButton);

        importButton.OnLeftClick += ImportButtonOnClick;
        SetSchematica(schematicaData);
    }
    private void ImportButtonOnClick(UIMouseEvent evt, UIElement button) {
        Console.WriteLine("Import and Place logic happens here");
        Task.Factory.StartNew(() => SchematicaFileFormat.ImportSchematica(Title), cancellationTokenSource.Token)
            .ContinueWith(task => SetSchematica(task.Result));
    }

    public void SetSchematica(SchematicaData schematica) {
        Schematica = schematica;
        
        width.SetText($"Width: {Schematica.Size.X}");
        height.SetText($"Height: {Schematica.Size.Y}");
        // SetThumbnailScale();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (Schematica != null && Schematica.ImagePreview == null &&Schematica.ImagePreviewData != null) {
            using var memoryStream = new MemoryStream(Schematica.ImagePreviewData);
            var texture = Texture2D.FromStream(Main.instance.GraphicsDevice, memoryStream);

            if (texture != null) {
                Schematica.ImagePreview = texture;
                Schematica.ImagePreviewData = null; // Free up resources
                previewImage.SetImage(Schematica.ImagePreview);
                Console.WriteLine("ImagePreview set");
            }
        }
    }

    private void SetThumbnailScale() {
        int desiredWidth = 100 * 16;
        int desiredHeight = 100 * 16;

        int actualWidth = Schematica.Size.X * 16;
        int actualHeight = Schematica.Size.Y * 16;

        float scale = 1;
        Vector2 offset = new Vector2();
        
        if (actualWidth > desiredWidth || actualHeight > desiredHeight) {
            if (actualHeight > actualWidth) {
                scale = (float) desiredWidth / actualHeight;
                offset.X = (desiredWidth - actualWidth * scale) / 2;
            }
            else {
                scale = (float) desiredWidth / actualWidth;
                offset.Y = (desiredHeight - actualHeight * scale) / 2;
            }
        }

        // offset = Vector2.Zero;
        // offset /= scale;
        //76 by 34
        // offset = offset / scale + new Vector2(325, 1200);
        //
        // offset = new Vector2(1764, 8341);

        Console.WriteLine($"desired height: {desiredHeight}");
        Console.WriteLine($"offset: {offset}");
        Console.WriteLine($"scale: {scale}");
        thumbnail.SetSchematica(Schematica, offset, scale);
    }
}