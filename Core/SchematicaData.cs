﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace Schematica.Common.DataStructures;

public class SchematicaData
{
    public string DisplayName { get; internal set; }
    public Point Size { get; internal set; }
    public List<TileData> TileDataList { get; internal set; }
    public Texture2D ImagePreview { get; internal set; }
    internal byte[] ImagePreviewData { get; set; }
}