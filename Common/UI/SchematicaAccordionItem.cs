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
    
    private UIImage previewImage;
    private CancellationTokenSource cancellationTokenSource;

    public SchematicaAccordionItem(SchematicaData schematicaData, int headerHeight) : base(schematicaData.DisplayName, headerHeight) {
        cancellationTokenSource = new CancellationTokenSource();
        
        previewImage = new UIImage(Asset<Texture2D>.Empty) {
            // Width = StyleDimension.Fill,
            // Height = StyleDimension.Fill,
            // MaxHeight = new StyleDimension(200f, 0f),
            ScaleToFit = true,
            AllowResizingDimensions = false,
            HAlign = 0.5f,
        };
        
        Body.Append(previewImage);

        var importButton = new UIButton("Import & Place Schematica") {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(35f, 0f),
            VAlign = 1f
        };

        Body.Append(importButton);

        importButton.OnLeftClick += ImportButtonOnClick;
        SetSchematica(schematicaData);
    }
    private void ImportButtonOnClick(UIMouseEvent evt, UIElement button) {
        Console.WriteLine("Import and Place logic happens here");
        Task.Factory.StartNew(() => {
                return SchematicaFileFormat.ImportSchematica(Title);
            }, cancellationTokenSource.Token)
            .ContinueWith(task => {
                    Console.WriteLine("Imported Schematica successfully");
                    SetSchematica(task.Result);
                }
            );
    }

    public void SetSchematica(SchematicaData schematica) {
        Schematica = schematica;
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (Schematica != null && Schematica.ImagePreview == null && Schematica.ImagePreviewData != null) {
            using var memoryStream = new MemoryStream(Schematica.ImagePreviewData);
            var texture = Texture2D.FromStream(Main.instance.GraphicsDevice, memoryStream);
        
            if (texture != null) {
                Schematica.ImagePreview = texture;
                Schematica.ImagePreviewData = null; // Free up resources
                previewImage.SetImage(Schematica.ImagePreview);

                var aspectRatio = (float) Schematica.ImagePreview.Width / Schematica.ImagePreview.Height;
                var width = Math.Min(Body.GetDimensions().Width, Schematica.ImagePreview.Width);
                previewImage.Width.Set(width, 0f);
                var targetHeight = width / aspectRatio;
                previewImage.Height.Set(targetHeight, 0f);
                
                Body.Height.Pixels += targetHeight + 66f;
            }
        }
    }
}