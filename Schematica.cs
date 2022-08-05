using System;
using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Schematica.Common;
using Schematica.Common.DataStructures;
using Schematica.Common.UI;
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
    internal static bool CanSelectEdges;
    internal static ModKeybind UITestBind;
    public override void Load() {
        UITestBind = KeybindLoader.RegisterKeybind(this, "Empty", "X");
        
        bool[] selected = new bool[3];
        string[] textureNames = new[] { "FloppyDisk", "Magnifier", "Schematica" };
        //Import, Export, Toggle on/off

        //TODO: No need to detour this.. Make my own UI and check if cross needs to be shown if Edges aren't pinned
        //Make UIElement with custom layout and simply call Draw and Update on it from somewhere..
        
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

                                // ThreadPool.QueueUserWorkItem(state => SchematicData.SaveSchematic());
                                SchematicData.SaveSchematic();
                            }
                            break;
                        case 1:
                            hoverText = "Load Schematics";

                            if (mouseClick) {
                                SoundEngine.PlaySound(SoundID.MenuTick);
                                
                                // ThreadPool.QueueUserWorkItem(state => SchematicData.LoadSchematic("DefaultName"));
                                SchematicData.LoadSchematic("DefaultName");
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

                    //Not in a Draw method but should be fine..?
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
            SchematicaUIState.Instance.SchematicaWindowElement.TestingRepopulateWindow();
        }
    }
}