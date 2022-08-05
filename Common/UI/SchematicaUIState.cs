using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Schematica.Common.Systems;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;
using Terraria.UI;

namespace Schematica.Common.UI;

public class SchematicaUISystem : UISystem<SchematicaUIState>
{
    public override InterfaceScaleType InterfaceScaleType => InterfaceScaleType.UI;

    public override void UpdateUI(GameTime gameTime) {
        if (CaptureManager.Instance.Active)
            base.UpdateUI(gameTime);
    }

    protected override bool DrawMethod() {
        if (CaptureManager.Instance.Active)
            return base.DrawMethod();

        return true;
    }
}

public class SchematicaUIState : UIState
{
    public static SchematicaUIState Instance;
    public SchematicaWindow SchematicaWindowElement;
    public override void OnInitialize() {
        Instance = this;
        
        SchematicaWindowElement = new SchematicaWindow();
        Append(SchematicaWindowElement);
    }

    public override void Update(GameTime gameTime) => base.Update(gameTime);
}