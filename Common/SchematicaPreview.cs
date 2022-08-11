using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Schematica.Common.DataStructures;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace Schematica.Common;

public static class SchematicaPreview
{
    internal static void DrawPreview(SchematicaData schematica, Vector2 startingPostion, float scale = 1f) {
        for (int j = 0; j < schematica.Size.Y; j++) {
            for (int i = 0; i < schematica.Size.X; i++) {
                Vector2 pos = new Vector2(i, j) * 16;
                if (Main.screenPosition.X > pos.X || (Main.screenPosition.X + Main.screenWidth) > pos.X ||
                Main.screenPosition.Y > pos.Y || (Main.screenPosition.Y + Main.screenHeight) > pos.Y) continue;

                int listIndex = i + j * schematica.Size.X;
                TileData tileData = schematica.TileDataList[listIndex];

                //Tiles
                if (tileData.HasTile) {
                    Main.instance.LoadTiles(tileData.TileType);
                    Texture2D texture = TextureAssets.Tile[tileData.TileType].Value;
                    Rectangle? rectangle = new Rectangle(tileData.TileFrameX, tileData.TileFrameY, 16, 16);
                    Main.spriteBatch.Draw(texture, (startingPostion + pos) * scale, rectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }
    }

    // private static void DrawPreview(SchematicaData schematica, Vector2 position, float scale = 1f) {
    //     position -= new Vector2(schematica.Size.X * 16f / 2f, schematica.Size.Y * 16f);
    //     for (int j = 0; j < schematica.Size.Y; j++) {
    //         for (int i = 0; i < schematica.Size.X; i++) {
    //             int listIndex = i + j * schematica.Size.X;
    //             TileData tileData = schematica.data[listIndex];-
    //
    //             //Tiles
    //             if (tileData.TileWallWireStateData.HasTile) {
    //                 Main.instance.LoadTiles(tileData.TileTypeData.Type);
    //                 Texture2D texture = TextureAssets.Tile[tileData.TileTypeData.Type].Value;
    //                 Rectangle rectangle = new Rectangle(tileData.TileWallWireStateData.TileFrameX, tileData.TileWallWireStateData.TileFrameY, 16, 16);
    //                 Vector2 pos = position + new Vector2(i, j) * 16;
    //                 Main.spriteBatch.Draw(texture, pos * scale, rectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    //             }
    //         }
    //     }
    // }
}