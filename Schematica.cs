using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Schematica.Common;
using Schematica.Common.DataStructures;
using Schematica.Common.Players;
using Schematica.Common.Systems;
using Schematica.Common.UI;
using Schematica.Core;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace Schematica;

public class Schematica : Mod
{
    public static string SavePath = Path.Combine(Main.SavePath, nameof(Schematica));

    //With an i7-10700k and an ssd m.2. samsung 970 evo:
    
    //at compression level 9 -> Large world was +- 7000KB
    //at compression level 1 -> Large world was +- 12000KB
    //Large world takes 3.3s and 500MB to export
    //Large world takes 1.7s and 500MB to import

    internal static int BufferSize = 4096 * 37; //.NET's default buffer is 4KB, I'm using around 150KB
    internal static int CompressionLevel = 1; //[0, 9] Bigger level => smaller files. May take longer to load/save schematicas

    internal static bool CanSelectEdges = true;
    internal static bool CanRefreshSchematicasList = true;

    internal static ModKeybind UITestBind;
    internal static ModKeybind TestSetEdges;
    internal static List<SchematicaData> LoadedSchematicas = new List<SchematicaData>();
    internal static int SelectedSchematicaIndex = -1;

    public override void Load() {
        UITestBind = KeybindLoader.RegisterKeybind(this, "Empty", "X");
        TestSetEdges = KeybindLoader.RegisterKeybind(this, "Edges", "R");

        bool[] selected = new bool[2];
        string[] textureNames = new[] { "FloppyDisk", "Schematica" };

        //TODO: No need to detour this.. Make my own UI and check if cross needs to be shown if Edges aren't pinned

        Terraria.Graphics.Capture.On_CaptureInterface.DrawButtons += (orig, self, sb) => {
            for (int i = 0; i < selected.Length; i++) {
                Texture2D background = !selected[i] ? TextureAssets.InventoryBack.Value : TextureAssets.InventoryBack14.Value;
                float scale = 0.8f;
                Vector2 position = new(24 + 46 * i, 24f + 46f);
                Color color = Main.inventoryBack * 0.8f;

                sb.Draw(background, position, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                Texture2D icon = ModContent.Request<Texture2D>($"Schematica/Assets/UI/{textureNames[i]}", AssetRequestMode.ImmediateLoad).Value;
                sb.Draw(icon, position + new Vector2(26f) * scale, null, Color.White, 0f, icon.Size() / 2f, 1f, SpriteEffects.None, 0f);

                if (i == 0 && (!CaptureInterface.EdgeAPinned || !CaptureInterface.EdgeBPinned))
                    sb.Draw(TextureAssets.Cd.Value, position + new Vector2(26f) * scale, null, Color.White * 0.65f, 0f, TextureAssets.Cd.Value.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }

            orig.Invoke(self, sb);
        };
        
        Terraria.Graphics.Capture.On_CaptureInterface.UpdateButtons += (orig, self, mouse) => {
            bool baseReturn = orig.Invoke(self, mouse);
            if (baseReturn)
                return true;

            for (int i = 0; i < selected.Length; i++) {
                if (new Rectangle(24 + 46 * i, 24 + 46, 42, 42).Contains(mouse.ToPoint())) {
                    Main.LocalPlayer.mouseInterface = true;

                    bool mouseClick = Main.mouseLeft && Main.mouseLeftRelease;
                    string hoverText = "";
                    switch (i) {
                        case 0:
                            hoverText = "Save Current Selection";

                            if (mouseClick && CaptureInterface.EdgeAPinned && CaptureInterface.EdgeBPinned) {
                                SoundEngine.PlaySound(SoundID.MenuTick);

                                Task.Factory.StartNew(
                                        () => {
                                            string schematicaDisplayName = "Binary Schematica"; //TODO: SchematicaWindowState.Instance.ToggleSaveNamePopup();

                                            //Removing from Accordion since it'll be unavailable until it finished exporting
                                            SchematicaWindowState.Instance.WindowElement.TryRemoveAccordionItem(schematicaDisplayName);
                                            CanRefreshSchematicasList = false;
                                            
                                            TakeSchematicaSnapshot(schematicaDisplayName);
                                            while (CaptureInterface.CameraLock) Task.Delay(50).Wait();
                                            SchematicaFileFormat.ExportSchematica(schematicaDisplayName, CaptureInterface.EdgeA, CaptureInterface.EdgeB);
                                        }
                                    )
                                    .ContinueWith(
                                        _ => {
                                            Console.WriteLine("Finished Exporting");

                                            CanRefreshSchematicasList = true;
                                            SchematicaWindowState.Instance.WindowElement.RepopulateSchematicas();
                                        }
                                    );
                            }
                            break;
                        case 1:
                            hoverText = "Load Schematics";

                            if (mouseClick) {
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                selected[i] = !selected[i];

                                if (CanRefreshSchematicasList)
                                    SchematicaWindowState.Instance.WindowElement.RepopulateSchematicas();

                                if (selected[i])
                                    SchematicaUISystem.Instance.Activate();
                                else
                                    SchematicaUISystem.Instance.Deactivate();
                            }
                            break;
                        case 2:
                            hoverText = "Schematic Preview: " + (selected[i] ? "Enabled" : "Disabled");

                            if (mouseClick) {
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                selected[i] = !selected[i];
                            }
                            break;
                    }

                    Main.instance.MouseText(hoverText, 0, 0);

                    return true;
                }
            }

            return false;
        };

        // Remove "Camera Mode" text
         Terraria.Graphics.Capture.IL_CaptureInterface.Draw += il => {
             ILCursor c = new ILCursor(il);
        
             if (!c.TryGotoNext(i => i.MatchLdcI4(81)))
                 return;
        
             c.Index -= 2;
             c.RemoveRange(18);
         };


        //Condition whether edges can be selected or not
        Terraria.Graphics.Capture.On_CaptureInterface.ModeEdgeSelection.Update += (orig, self) => {
            if (CanSelectEdges)
                orig.Invoke(self);
        };

        Terraria.Graphics.Capture.On_CaptureInterface.ModeDragBounds.Update += (orig, self) => {
            if (CanSelectEdges)
                orig.Invoke(self);
        };

        Terraria.Graphics.Capture.On_CaptureInterface.Scrolling += (orig, self) => {
            if (CanSelectEdges)
                orig.Invoke(self);
        };

        //One single IL Edit to prevent the 3 detours above?
        // IL.Terraria.Graphics.Capture.CaptureInterface.Update += il => {
        //     var c = new ILCursor(il);
        //     
        //     if (!c.TryGotoNext(i  => i.MatchRet()))
        //         return;
        //     
        //     if (!c.TryGotoNext(i  => i.MatchRet()))
        //         return;
        //     
        //     if (!c.TryGotoNext(i  => i.MatchRet()))
        //         return;
        //
        //     c.Index++;
        //     ILLabel modesUpdateLoopLabel = c.MarkLabel();
        //     c.Index -= 2;
        //
        //     c.Emit(OpCodes.Ldarg_0);
        //
        //     c.EmitDelegate<Func<bool, bool, bool>>(
        //         (updateButtons, mouseLeft) => {
        //             return updateButtons && mouseLeft && Schematica.CanSelectEdges;
        //         });
        //
        //     // c.Index++;
        //     // ILLabel modesUpdateLoopLabel = c.MarkLabel();
        //     // c.Index -= 2; //-=2?
        //     //
        //     // c.Emit(OpCodes.Ldloc_2);
        //     // //Emit delegate of Schematic.CanSelectEdges -> Change to IgnoreEdgeSelection
        //     // c.EmitDelegate(() => !Schematica.CanSelectEdges);
        //     //
        //     // //Emit brfalse to check if delegate is false, and if it is, skip to label which is the next instruction after return
        //     // c.Emit(OpCodes.Brfalse_S, modesUpdateLoopLabel);
        // };
    }

    public static void TakeSchematicaSnapshot(string displayName) {
        Rectangle GetArea() {
            int x = Math.Min(CaptureInterface.EdgeA.X, CaptureInterface.EdgeB.X);
            int y = Math.Min(CaptureInterface.EdgeA.Y, CaptureInterface.EdgeB.Y);
            int num = Math.Abs(CaptureInterface.EdgeA.X - CaptureInterface.EdgeB.X);
            int num2 = Math.Abs(CaptureInterface.EdgeA.Y - CaptureInterface.EdgeB.Y);
            return new Rectangle(x, y, num + 1, num2 + 1);
        }
        
        var captureSettings = new CaptureSettings {
            Area = GetArea(),
            Biome = CaptureBiome.GetCaptureBiome(CaptureInterface.Settings.BiomeChoiceIndex),
            CaptureBackground = !CaptureInterface.Settings.TransparentBackground,
            CaptureEntities = false,
            UseScaling = CaptureInterface.Settings.PackImage,
            CaptureMech = false,
            OutputName = displayName
        };
                                            
        if (captureSettings.Biome.WaterStyle != 13)
            Main.liquidAlpha[13] = 0f;
                                            
        CaptureInterface.StartCamera(captureSettings);
    }
}