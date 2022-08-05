using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class UIAccordionItem : UIElement
{
    public string Title { get; private set; }
    
    public UIElement Header { get; private set; }
    
    public UIElement Body { get; private set; }
    
    public Action HeaderClick;
    
    public bool IsOpen { get; private set; }

    private UIImageFramed arrow;
    private Asset<Texture2D> arrowAsset;

    public UIAccordionItem(string title, int headerHeight, int bodyHeight) {
        Title = title;
        
        GenerateHeader(headerHeight);
        GenerateBody(bodyHeight);

        Width.Set(0f, 1f);
        Height.Set(Header.Height.Pixels + (IsOpen ? Body.Height.Pixels : 0), 0f);
        
        Append(Header);
    }

    private void GenerateBody(int bodyHeight) {
        Body = new UIElement() {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(Header.Height.Pixels + bodyHeight, 0f),
        };
        
        UIPanel panel = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            PaddingTop = Header.Height.Pixels + 10f
        };

        UIText text = new UIText("Some Text") {
            VAlign =  0f,
            MaxWidth = new StyleDimension(-5f * 2, 1f), //5 pixel gap for the arrow button and 5 pixel gap from back button
            Left = new StyleDimension(5f, 0f)
        };
        
        panel.Append(text);
        
        Body.Append(panel);
    }
    
    private void GenerateHeader(int headerHeight) {
        Header = new UIElement() {
            Width = StyleDimension.Fill,
            Height = new StyleDimension(headerHeight, 0f)
        };

        UIPanel panel = new UIPanel() {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            BackgroundColor = new Color(35, 40, 83),
            BorderColor = new Color(35, 40, 83)
        };
        panel.SetPadding(0);

        UIText title = new UIText(Title) {
            VAlign = 0.5f,
            Left = new StyleDimension(15f, 0f),
            MaxWidth = new StyleDimension(-5f * 2 - 32f, 1f) //5 pixel gap for the arrow button and 5 pixel gap from back button
        };
        
        panel.Append(title);
        
        arrowAsset = Main.Assets.Request<Texture2D>("Images/UI/TexturePackButtons", AssetRequestMode.ImmediateLoad);
        arrow = new UIImageFramed(arrowAsset, arrowAsset.Frame(2, 2, (!IsOpen).ToInt(), 0)) {
            Left = new StyleDimension(-5f - 32f, 1f),
            Top = new StyleDimension((headerHeight - 32f) / 2f, 0f), //VAlign 0.5f was not working?
            IgnoresMouseInteraction = true
        };

        panel.Append(arrow);
        
        //Bind Events
        Header.OnClick += (__, _) => {
            HeaderClick();
        };

        Header.OnMouseOver += (__, _) => {
            panel.BackgroundColor = new Color(50, 58, 115);
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        Header.OnMouseOut += (__, _) => {
            panel.BackgroundColor = new Color(35, 40, 83);
        };
        
        Header.Append(panel);
    }

    public void ToggleOpen() {
        IsOpen = !IsOpen;

        if (IsOpen) {
            Header.Remove();
            Append(Body);
            Append(Header);
        }
        else
            Body.Remove();
        
        Height.Set(Header.Height.Pixels + (IsOpen ? Body.Height.Pixels - Header.Height.Pixels : 0), 0f);
        arrow.SetImage(arrowAsset, arrowAsset.Frame(2, 2, (!IsOpen).ToInt(), 0));
        RecalculateChildren();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        
        
    }
}