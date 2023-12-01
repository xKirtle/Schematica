using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class DraggableUIPanel : UIPanel
{
    private Vector2 offset;
    private bool dragging;
    public bool canDrag;

    public override void LeftMouseDown(UIMouseEvent evt) {
        base.LeftMouseDown(evt);
        dragging = true;
        if (canDrag)
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
    }

    public override void LeftMouseUp(UIMouseEvent evt) {
        base.LeftMouseUp(evt);
        Vector2 end = evt.MousePosition;
        dragging = false;

        if (canDrag) {
            Left.Set(end.X - offset.X, 0f);
            Top.Set(end.Y - offset.Y, 0f);
            canDrag = false;
        }

        Recalculate();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;

        if (dragging && canDrag) {
            Left.Set(Main.mouseX - offset.X, 0f);
            Top.Set(Main.mouseY - offset.Y, 0f);
            Recalculate();
        }

        Rectangle parentSpace = Parent.GetDimensions().ToRectangle();
        if (!GetDimensions().ToRectangle().Intersects(parentSpace)) {
            Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
            Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
            Recalculate();
        }
    }
}