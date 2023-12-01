using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class UIButton : UIPanel
{
    public string Text => uiText.Text;
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

        OnLeftMouseDown += (__, _) => {
            Top.Set(Top.Pixels + 1f, 0f);
        };

        OnLeftMouseUp += (__, _) => {
            Top.Set(Top.Pixels - 1f, 0f);
        };
    }

    public void SetText(string text) => uiText.SetText(text);

    public override void Update(GameTime gameTime) => base.Update(gameTime);
}