using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Schematica.Common.DataStructures;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public abstract class UIAccordionItem : UIElement
{
    public MouseEvent HeaderOnClick;

    public string Title { get; private set; }
    public bool IsOpen { get; private set; }
    protected UIElement Header { get; private set; }
    protected UIElement Body { get; private set; }

    private UIImageFramed arrow;
    private ScrollingUIText title;
    private Asset<Texture2D> arrowAsset;
    private float targetHeight;

    public UIAccordionItem(string title, int headerHeight) {
        Title = title;

        GenerateHeader(headerHeight);
        GenerateBody();

        Width.Set(0f, 1f);
        Height.Set(Header.Height.Pixels + (IsOpen ? Body.Height.Pixels : 0), 0f);
        
        Append(Body);
        Append(Header);

        OverflowHidden = true;

        Header.OnLeftClick += (evt, element) => {
            HeaderOnClick(evt, element);
        };
    }

    private void GenerateHeader(int headerHeight) {
        Header = new UIElement() {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(headerHeight, 0f)
        };

        var panel = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            BackgroundColor = new Color(35, 40, 83),
            BorderColor = new Color(35, 40, 83)
        };
        
        panel.SetPadding(0);

        var uiTextDrawBounds = new UIElement() {
            Width = new StyleDimension(-5f * 2 - 32f - 0f, 1f),
            Height = StyleDimension.Fill,
            OverflowHidden = true
        };
        
        panel.Append(uiTextDrawBounds);

        title = new ScrollingUIText(Title) {
            VAlign = 0.5f,
            HAlign = 0f,
            Left = new StyleDimension(10f, 0f)
        };

        uiTextDrawBounds.Append(title);

        arrowAsset = Main.Assets.Request<Texture2D>("Images/UI/TexturePackButtons", AssetRequestMode.ImmediateLoad);
        arrow = new UIImageFramed(arrowAsset, arrowAsset.Frame(2, 2, (!IsOpen).ToInt(), 0)) {
            Width = new StyleDimension(32f, 0f),
            Height = new StyleDimension(32f, 0f),
            Left = new StyleDimension(-5f - 32f, 1f),
            Top = new StyleDimension((headerHeight - 32f) / 2f, 0f), //VAlign 0.5f was not working?
            IgnoresMouseInteraction = true
        };

        panel.Append(arrow);

        //Bind Events
        Header.OnMouseOver += (__, _) => {
            panel.BackgroundColor = new Color(50, 58, 115);
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        Header.OnMouseOut += (__, _) => {
            panel.BackgroundColor = new Color(35, 40, 83);
        };

        targetHeight = Header.Height.Pixels;
        Header.Append(panel);
    }

    private void GenerateBody() {
        Body = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            // Height = new StyleDimension(240f, 0f),
            PaddingTop = Header.Height.Pixels + 10f,
            BorderColor = new Color(35, 40, 83),
            OverflowHidden = true
        };
    }

    public virtual void ToggleOpen() {
        IsOpen = !IsOpen;
        targetHeight = Header.Height.Pixels + (IsOpen ? Body.Height.Pixels : 0);
        Console.WriteLine($"isOpen: {IsOpen} | targetHeight: {targetHeight}");
        arrow.SetImage(arrowAsset, arrowAsset.Frame(2, 2, (!IsOpen).ToInt(), 0));
        RecalculateChildren();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        Height.Set(MathHelper.Lerp(Height.Pixels, targetHeight, 0.2f), 0f);
        Recalculate();
    }

    public override int CompareTo(object obj) {
        string x = Title;
        string y = (obj as UIAccordionItem)?.Title;

        if (x == null && y == null)
            return 0;
        if (x == null)
            return -1;
        if (y == null)
            return 1;

        int lx = x.Length, ly = y.Length;

        int mx = 0, my = 0;
        for (; mx < lx && my < ly; mx++, my++) {
            if (char.IsDigit(x[mx]) && char.IsDigit(y[my])) {
                long vx = 0, vy = 0;

                for (; mx < lx && char.IsDigit(x[mx]); mx++)
                    vx = vx * 10 + x[mx] - '0';

                for (; my < ly && char.IsDigit(y[my]); my++)
                    vy = vy * 10 + y[my] - '0';

                if (vx != vy)
                    return vx > vy ? 1 : -1;
            }

            if (mx < lx && my < ly && x[mx] != y[my])
                return x[mx] > y[my] ? 1 : -1;
        }

        return lx - mx - (ly - my);
    }
}