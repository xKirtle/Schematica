using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schematica.Core;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;
using ZipFile = System.IO.Compression.ZipFile;

namespace Schematica.Common.DataStructures;

[Serializable]
public class SchematicaData
{
    public string Name { get; internal set; }
    public Point Size { get; internal set; }
    
    public List<TileData> data;

    public static void LoadSchematic(string filename) {
        if (Schematica.currentPreview?.Name == filename)
            return;
        
        foreach (SchematicaData schematica in Schematica.placedSchematicas) {
            if (schematica.Name == filename) {
                Console.WriteLine("found");
                Schematica.currentPreview = schematica;
                return;
            }
        }

        string path = $@"{Schematica.SavePath}\{filename}.schematica";
        //ZipFile.OpenRead?
        using var zipArchive = new ZipArchive(new MemoryStream(File.ReadAllBytes(path)));
        var stream = zipArchive.Entries[0].Open();
        
        using StreamReader sr = new StreamReader(stream);
        using JsonTextReader reader = new JsonTextReader(sr);
        JsonSerializer serializer = new JsonSerializer();
        
        Schematica.currentPreview = serializer.Deserialize<SchematicaData>(reader);
    }

    private static SchematicaData GenerateSchematicaFromEdges(string name) {
        //Building our schematica data object
        SchematicaData schematica = new SchematicaData();
        schematica.Name = name;
        schematica.Size = new Point(Math.Abs(CaptureInterface.EdgeA.X - CaptureInterface.EdgeB.X) + 1, Math.Abs(CaptureInterface.EdgeA.Y - CaptureInterface.EdgeB.Y) + 1);

        Point minEdge = new Point(Math.Min(CaptureInterface.EdgeA.X, CaptureInterface.EdgeB.X), Math.Min(CaptureInterface.EdgeA.Y, CaptureInterface.EdgeB.Y));
        schematica.data = new List<TileData>();

        for (int j = 0; j < schematica.Size.Y; j++) {
            for (int i = 0; i < schematica.Size.X; i++) {
                Tile tile = Main.tile[minEdge.X + i, minEdge.Y + j];
                schematica.data.Add(new TileData(tile));
            }
        }

        return schematica;
    }
}