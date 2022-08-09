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
using Schematica.Common.Systems;
using Schematica.Common.UI;
using Schematica.Core;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace Schematica;

public class Schematica : Mod
{
    public static string SavePath = $@"{Path.Combine(Main.SavePath)}\{nameof(Schematica)}";
    
    //Taking at max 500MB to save a Large world
    //at compression level 9 -> Large world was +- 7000KB and took 33s to save
    //at compression level 1 -> Large world was +- 12000KB and took 4.4s to save
    internal static int BufferSize = 4096 * 37; //.NET's default buffer is 4KB, I'm using around 150KB
    internal static int CompressionLevel = 1; //[0, 9] Bigger level => smaller files. May take longer to load/save schematicas

    internal static bool CanSelectEdges = true;
    internal static bool CanRefreshSchematicasList = true;
    internal static ModKeybind UITestBind;
    internal static ModKeybind TestSetEdges;
    internal static List<SchematicaData> placedSchematicas;
    internal static SchematicaData currentPreview;

    public override void Load() {
        UITestBind = KeybindLoader.RegisterKeybind(this, "Empty", "X");
        TestSetEdges = KeybindLoader.RegisterKeybind(this, "Edges", "R");
        placedSchematicas = new List<SchematicaData>();
        
        bool[] selected = new bool[2];
        string[] textureNames = new[] { "FloppyDisk", "Schematica" };
        
        //TODO: No need to detour this.. Make my own UI and check if cross needs to be shown if Edges aren't pinned
        
        On.Terraria.Graphics.Capture.CaptureInterface.DrawButtons += (orig, self, sb) => {
            for (int i = 0; i < selected.Length; i++) {
                Texture2D background = !selected[i] ? TextureAssets.InventoryBack.Value : TextureAssets.InventoryBack14.Value;
                float scale = 0.8f;
                Vector2 position = new Vector2(24 + 46 * i, 24f + 46f);
                Color color = Main.inventoryBack * 0.8f;
        
                sb.Draw(background, position, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        
                Texture2D icon = ModContent.Request<Texture2D>($"Schematica/Assets/UI/{textureNames[i]}", AssetRequestMode.ImmediateLoad).Value;
                sb.Draw(icon, position + new Vector2(26f) * scale, null, Color.White, 0f, icon.Size() / 2f, 1f, SpriteEffects.None, 0f);
        
                if (i == 0 && (!CaptureInterface.EdgeAPinned || !CaptureInterface.EdgeBPinned))
                    sb.Draw(TextureAssets.Cd.Value, position + new Vector2(26f) * scale, null, Color.White * 0.65f, 0f, TextureAssets.Cd.Value.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }
        
            orig.Invoke(self, sb);
        };
        
        On.Terraria.Graphics.Capture.CaptureInterface.UpdateButtons += (orig, self, mouse) => {
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
                                
                                Task.Factory.StartNew(() => {
                                            string fileName = "BinarySchematica"; //TODO: SchematicaWindowState.Instance.ToggleSaveNamePopup();
                                            
                                            //Removing from Accordion since it'll be unavailable until it finished exporting
                                            SchematicaWindowState.Instance.WindowElement.TryRemoveAccordionItem(fileName);
                                            Schematica.CanRefreshSchematicasList = false;
                                            
                                            SchematicaFileFormat.ExportSchematica(fileName);
                                        }
                                    )
                                        .ContinueWith(
                                            _ => {
                                                Console.WriteLine("Finished Exporting");
                                                
                                                Schematica.CanRefreshSchematicasList = true;
                                                SchematicaWindowState.Instance.WindowElement.TestingRepopulateWindow();
                                            }
                                        );
                            }
                            break;
                        case 1:
                            hoverText = "Load Schematics";
        
                            if (mouseClick) {
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                selected[i] = !selected[i];
                                
                                if (Schematica.CanRefreshSchematicasList)
                                    SchematicaWindowState.Instance.WindowElement.TestingRepopulateWindow();

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

        //Remove "Camera Mode" text
        IL.Terraria.Graphics.Capture.CaptureInterface.Draw += il => {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(i => i.MatchLdcI4(81)))
                return;

            c.Index -= 2;
            c.RemoveRange(18);
        };

        
        //Condition whether edges can be selected or not
        On.Terraria.Graphics.Capture.CaptureInterface.ModeEdgeSelection.Update += (orig, self) => {
            if (CanSelectEdges)
                orig.Invoke(self);
        };
        
        On.Terraria.Graphics.Capture.CaptureInterface.ModeDragBounds.Update += (orig, self) => {
            if (CanSelectEdges)
                orig.Invoke(self);
        };

        On.Terraria.Graphics.Capture.CaptureInterface.Scrolling += (orig, self) => {
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
}

public class KeyBindPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet) {
        if (Schematica.UITestBind.JustPressed) {
            SchematicaWindowState.Instance.WindowElement.TestingRepopulateWindow();
        }

        if (Schematica.TestSetEdges.JustPressed) {
            CaptureInterface.EdgeA = Point.Zero;
            CaptureInterface.EdgeB = new Point(Main.maxTilesX - 1, Main.maxTilesY - 1);

            CaptureInterface.EdgeAPinned = CaptureInterface.EdgeBPinned = true;
        }
    }
}