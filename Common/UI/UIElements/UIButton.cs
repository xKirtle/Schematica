using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class UIButton : UIPanel
{
    private string text;
    private UIText uiText;
    
    public UIButton(string text) {
        uiText = new UIText(text) {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        
        Append(uiText);

        OnMouseOver += (__, _) => {
            BackgroundColor = new Color(80, 102, 181) * 0.7f;
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        OnMouseOut += (__, _) => {
            BackgroundColor = new Color(63, 82, 151) * 0.7f;
        };

        OnMouseDown += (__, _) => {
            Top.Set(Top.Pixels + 1f, 0f);
        };

        OnMouseUp += (__, _) => {
            Top.Set(Top.Pixels - 1f, 0f);
        };
    }

    public override void Update(GameTime gameTime) => base.Update(gameTime);
}