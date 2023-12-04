using Microsoft.Xna.Framework;
using Schematica.Common.UI;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace Schematica.Common.Players;

public class KeyBindPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet) {
        if (Schematica.UITestBind.JustPressed) {
            // SchematicaWindowState.Instance.WindowElement.RepopulateSchematicas();
            SchematicaUISystem.Instance.Deactivate();
            SchematicaUISystem.Instance.Load();
            SchematicaUISystem.Instance.Activate();
        }

        if (Schematica.TestSetEdges.JustPressed) {
            CaptureInterface.EdgeA = Point.Zero;
            CaptureInterface.EdgeB = new Point(Main.maxTilesX, Main.maxTilesY);

            CaptureInterface.EdgeAPinned = CaptureInterface.EdgeBPinned = true;
        }
    }
}