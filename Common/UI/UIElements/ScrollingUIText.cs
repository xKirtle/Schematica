﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace Schematica.Common.UI.UIElements;

public class ScrollingUIText : UIText
{
    public ScrollingUIText(string text, float textScale = 1, bool large = false) : base(text, textScale, large) { }
    public ScrollingUIText(LocalizedText text, float textScale = 1, bool large = false) : base(text, textScale, large) { }

    private bool canScroll;
    private bool scrollingLeft;
    private int cooldownTimer = 0;

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        
        //Parent is the dummy UIElement we use to hide the text behind a certain width

        if (cooldownTimer > 0 && !scrollingLeft) {
            canScroll = false;
        }

        if (IsMouseHovering || Parent.IsMouseHovering) {
            canScroll = true;
        }

        if (cooldownTimer > 0) {
            cooldownTimer--;
            
            return;
        }

        Rectangle cullingArea = GetViewCullingArea();
        Vector2 offset = new(10f);
        
        // Is our text in one of the two ends of the culling area?
        if (!scrollingLeft && Parent.ContainsPoint(new Vector2(cullingArea.Right, cullingArea.Bottom) + offset) ||
            scrollingLeft && Parent.ContainsPoint(new Vector2(cullingArea.Left, cullingArea.Bottom) - offset)) {
            scrollingLeft = !scrollingLeft;
            cooldownTimer = 90;
            
            return;
        }

        if (canScroll) {
            Left.Set(Left.Pixels + 1f * scrollingLeft.ToDirectionInt(), 0f);
            Recalculate();
        }
    }
}