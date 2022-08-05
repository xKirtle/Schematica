using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schematica.Core.JsonConverters;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace Schematica.Common.DataStructures;

[JsonConverter(typeof(SchematicaDataConverter))]
public class SchematicaData
{
    public string Name { get; internal set; }
    public Point Size { get; internal set; }

    public List<TileData> data;
    
    private static Point EdgeA => CaptureInterface.EdgeA;
    private static Point EdgeB => CaptureInterface.EdgeB;
    private static string SavePath = $@"{Path.Combine(Main.SavePath)}\Schematica";
    
    //Bring up UI popup to ask for a name before saving
    public static void SaveSchematic(string fileName = null) {
        SchematicaData schematica = new SchematicaData();
        try {
            schematica.Name = fileName ?? GetNextDefaultSchematicName();
            schematica.Size = new Point(Math.Abs(EdgeA.X - EdgeB.X) + 1, Math.Abs(EdgeA.Y - EdgeB.Y) + 1);

            int numElements = schematica.Size.X * schematica.Size.Y;
            Point minEdge = new Point(Math.Min(EdgeA.X, EdgeB.X), Math.Min(EdgeA.Y, EdgeB.Y));
            schematica.data = new List<TileData>();
            
            for (int j = 0; j < schematica.Size.Y; j++)
                for (int i = 0; i < schematica.Size.X; i++) {
                    Tile tile = Main.tile[minEdge.X + i, minEdge.Y + j];
                    schematica.data.Add(new TileData(tile));
                }

            Directory.CreateDirectory(SavePath);
            
            string path = $@"{SavePath}\{schematica.Name}.json";
            string json = JsonConvert.SerializeObject(schematica);
            string schematicPath = $@"{SavePath}\{schematica.Name}.schematica";

            if (File.Exists(schematicPath))
                File.Delete(schematicPath);

            File.WriteAllText(path, json);
            using var zipArchive = ZipFile.Open(schematicPath, ZipArchiveMode.Create);
            zipArchive.CreateEntryFromFile(path, schematica.Name + ".json");
            zipArchive.Dispose();

            File.Delete(path);
            
            schematica = null;
            GC.Collect();
            GC.Collect();
        }
        catch (Exception e) { }
    }
    
    public static void LoadSchematic(string filename) {
        string path = $@"{SavePath}\{filename}";

        using var zipArchive = new ZipArchive(new MemoryStream(File.ReadAllBytes($"{path}.schematica")));
        Stream stream = zipArchive.Entries[0].Open();

        using StreamReader sr = new StreamReader(stream);
        using JsonTextReader reader = new JsonTextReader(sr);
        JsonSerializer serializer = new JsonSerializer();
        SchematicaData schematica =  serializer.Deserialize<SchematicaData>(reader);

        Console.WriteLine($"{schematica.Name} {schematica.Size}");
    }

    //Method to read used folder for saves and detect what's the next integer for naming it can use (for the default naming scheme)
    public static string GetNextDefaultSchematicName() {

        return "DefaultName";
    }
}