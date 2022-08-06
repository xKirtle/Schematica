using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Schematica.Common.DataStructures;
using Schematica.Common.UI.UIElements;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI;

public class SchematicaSaveNameWindow : UIPanel
{
    private bool isOpen;
    private UITextBox textBox;
    
    public override void OnInitialize() {
        Width.Set(300f, 0f);
        Height.Set(150f, 0);
        VAlign = 0.5f;
        HAlign = 0.5f;

        textBox = new UITextBox("Insert name here") {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(24f, 0f),
            HAlign = 0.5f,
            VAlign = 0.3f
        };

        Append(textBox);

        UIText saveNameTooltip = new UIText("Choose a name for your schematica:") {
            HAlign = 0.5f
        };
        
        Append(saveNameTooltip);

        UIButton confirmButton = new UIButton("Confirm") {
            Width = new StyleDimension(0f, 0.4f),
            Height = new StyleDimension(30f, 0f),
            HAlign = 0.1f,
            VAlign = 0.9f
        };
    
        confirmButton.OnClick += (__, _) => {
            ThreadPool.QueueUserWorkItem(state => SchematicaData.SaveSchematic(textBox.Text));
        };
        
        Append(confirmButton);

        UIButton cancelButton = new UIButton("Cancel") {
            Width = new StyleDimension(0f, 0.4f),
            Height = new StyleDimension(30f, 0f),
            HAlign = 0.9f,
            VAlign = 0.9f
        };

        cancelButton.OnClick += (__, _) => {
            textBox.SetText("");
            ToggleVisibility();
        };
        
        Append(cancelButton);
    }

    public void ToggleVisibility() => isOpen = !isOpen;

    public override void Draw(SpriteBatch spriteBatch) {
        Schematica.CanSelectEdges = true;
        
        if (isOpen) {
            base.Draw(spriteBatch);
            
            if (IsMouseHovering)
                Schematica.CanSelectEdges = false;
        }
    }
}