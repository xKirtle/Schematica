using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Schematica.Common.DataStructures;
using Terraria;

namespace Schematica.Common;

public class SchematicaPreview
{
    public static void DrawPreview(SchematicaData schematica, Vector2 position, float scale = 1f) {
        for (int j = 0; j < schematica.Size.Y; j++) {
            for (int i = 0; i < schematica.Size.X; i++) {
                int listIndex = i + j * schematica.Size.X;
                TileData tileData = schematica.data[listIndex];
                
                
            }
        }
    }
}