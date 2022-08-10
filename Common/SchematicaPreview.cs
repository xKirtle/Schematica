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
    internal static Texture2D GenerateSchematicaThumbnail(SchematicaData schematica, int desiredWidth = 100, int desiredHeight = 100, float scale = 1f) {
        int actualWidth = schematica.Size.X * 16;
        int actualHeight = schematica.Size.Y * 16;
        
        Vector2 offset = new Vector2();
            
        if (actualWidth > desiredWidth || actualHeight > desiredHeight) {
            if (actualHeight > actualWidth) {
                scale = (float)desiredWidth / actualHeight;
                offset.X = (desiredWidth - actualWidth * scale) / 2;
            }
            else {
                scale = (float)desiredWidth / actualWidth;
                offset.Y = (desiredHeight - actualHeight * scale) / 2;
            }
        }
            
        offset = offset / scale;
            
        RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, desiredWidth, desiredHeight);
        Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
        Main.instance.GraphicsDevice.Clear(Color.Transparent);
        Main.spriteBatch.Begin();
        
        DrawPreview(schematica, offset, scale);

        Main.spriteBatch.End();
        Main.instance.GraphicsDevice.SetRenderTarget(null);

        Texture2D mergedTexture = new Texture2D(Main.instance.GraphicsDevice, desiredWidth, desiredHeight);
        Color[] content = new Color[desiredWidth * desiredHeight];
        renderTarget.GetData<Color>(content);
        mergedTexture.SetData<Color>(content);

        return mergedTexture;
    }
    
    private static void DrawPreview(SchematicaData schematica, Vector2 startingPostion, float scale = 1f) {
        for (int j = 0; j < schematica.Size.Y; j++) {
            for (int i = 0; i < schematica.Size.X; i++) {
                int listIndex = i + j * schematica.Size.X;
                TileData tileData = schematica.TileDataList[listIndex];

                //Tiles
                if (tileData.TileWallWireStateData.HasTile) {
                    Main.instance.LoadTiles(tileData.TileTypeData.Type);
                    Texture2D texture = TextureAssets.Tile[tileData.TileTypeData.Type].Value;
                    Rectangle? rectangle = new Rectangle(tileData.TileWallWireStateData.TileFrameX, tileData.TileWallWireStateData.TileFrameY, 16, 16);
                    Vector2 pos = new Vector2(i, j) * 16;
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