using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class SmoothUIGrid : UIGrid
{
    private float oldScrollViewPos;
    private float scrollAmount;

    public override void ScrollWheel(UIScrollWheelEvent evt) {
        if (_scrollbar != null) {
            oldScrollViewPos = _scrollbar.ViewPosition;
            scrollAmount = (float) -evt.ScrollWheelValue / 1f;
            // scrollAmount = MathHelper.Clamp(scrollAmount, -120f, 120f); -> Cap scroll amount?
        }
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (Main.mouseLeft) {
            scrollAmount = (_scrollbar.ViewPosition - oldScrollViewPos) * 3f;
            oldScrollViewPos = _scrollbar.ViewPosition;
        }
        else { _scrollbar.ViewPosition = MathHelper.Lerp(_scrollbar.ViewPosition, oldScrollViewPos + scrollAmount, 0.1f); }
    }
}