using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework;
using Schematica.Common.DataStructures;
using Schematica.Common.UI;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace Schematica.Core;

public static class SchematicaFileFormat
{
    internal static int TileDataByteSize => 14;

    /*
     * First .dat file in zip (metadata.dat)
     * Header               -> 10 bytes that spell out 'SCHEMATICA' in ASCII characters
     * Version              -> String that holds mod version it was made with
     * Schematica Size      -> 2 ushort values (x and y)
     *
     * Second .dat file in zip (data.dat)
     * Schematica Data      -> Read bytes until end of file
     *
     * Third file in zip (dependencies.dat)
     * Mod dependency count -> int with number of mods used in the schematic
     * Mod dependencies     -> List of mod names and versions (internalName@version)
     * 
     * Fourth file in zip (validation.dat)
     * Valid export flag    -> 1 bit (bool) that checks if export code reached the end
     */

    // TODO: Schematic Preview image can be captured using Terraria's screenshot tool! (and maybe compressed or scaled down)
    // and saved as a png file in the zip file (or as a byte array in another .dat file)

    public static void ExportSchematica(string fileName, Point edgeA, Point edgeB) {
        var sw = Stopwatch.StartNew();

        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException("Cannot save schematica with an invalid name");

        Point size = new(Math.Abs(edgeA.X - edgeB.X) + 1, Math.Abs(edgeA.Y - edgeB.Y) + 1);

        if (size == Point.Zero) // Save Button does not allow 
            throw new ArgumentException("Size of schematica cannot be zero");

        Point minEdge = new(Math.Min(edgeA.X, edgeB.X), Math.Min(edgeA.Y, edgeB.Y));

        try {
            // Making sure Schematica's path exists
            Directory.CreateDirectory(Schematica.SavePath);
            
            string writePath = Path.Combine(Schematica.SavePath, $"{fileName}.schematica");
            using var fileStream = new FileStream(writePath, FileMode.Create);
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);
            using var memoryStream = new MemoryStream(Schematica.BufferSize);
            var memoryWriter = new BinaryWriter(memoryStream);
            
            // Metadata
            WriteEntryToArchive(zipArchive, "metadata.dat", memoryStream, memoryWriter, () => {
                // Header
                memoryWriter.Write("SCHEMATICA");

                // Mod Version
                memoryWriter.Write(ModContent.GetInstance<Schematica>().Version.ToString());

                // Schematica Size
                memoryWriter.Write(size.X);
                memoryWriter.Write(size.Y);
            });
            
            // Data entry
            var modDependencies = new HashSet<string>();
            WriteEntryToArchive(zipArchive, "data.dat", memoryStream, memoryWriter, () => {
                // TileData
                for (int j = 0; j < size.Y; j++) {
                    for (int i = 0; i < size.X; i++) {
                        Tile tile = Main.tile[minEdge.X + i, minEdge.Y + j];
                        var modTile = TileLoader.GetTile(tile.TileType);
                        
                        if (modTile != null) {
                            string schematicDependency = $"{modTile.Mod.Name}@{modTile.Mod.Version}";
                            modDependencies.Add(schematicDependency);
                        }

                        TileData tileData = new TileData(tile);
                        tileData.Serialize(memoryWriter);
                    }
                }
            });
            
            // Dependencies entry
            WriteEntryToArchive(zipArchive, "dependencies.dat", memoryStream, memoryWriter, () => {
                memoryWriter.Write(modDependencies.Count);
                foreach (string modDependency in modDependencies) {
                    memoryWriter.Write(modDependency);
                }
            });
            
            // Validation entry
            WriteEntryToArchive(zipArchive, "validation.dat", memoryStream, memoryWriter, () => {
                memoryWriter.Write(true);
            });
            
            Console.WriteLine($"Finished exporting {fileName} in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    public static bool CanExportSchematica() {
        bool ValidCoordinates(Point point) => point.X >= 0 && point.X < Main.maxTilesX && point.Y >= 0 && point.Y < Main.maxTilesY;

        if (!CaptureInterface.EdgeAPinned ||
            !CaptureInterface.EdgeBPinned ||
            !ValidCoordinates(CaptureInterface.EdgeA) ||
            !ValidCoordinates(CaptureInterface.EdgeB))
            return false;

        return true;
    }

    public static SchematicaData ImportSchematica(string fileName, bool onlyMetadata = false) {
        var sw = Stopwatch.StartNew();
        
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException(nameof(fileName), "Cannot import schematica with an invalid name");

        var schematica = new SchematicaData();
        schematica.Name = fileName;

        try {
            string readPath = Path.Combine(Schematica.SavePath, $"{fileName}.schematica");
            using var fileStream = new FileStream(readPath, FileMode.Open);
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);

            ValidateAndParseMetadata(zipArchive, schematica);
            ValidateSchematicaIntegrity(zipArchive);
            ValidateSchematicaDependencies(zipArchive);
            
            if (!onlyMetadata) {
                ValidateAndParseSchematicaData(zipArchive, schematica);
            }
            
            Console.WriteLine($"Finished importing {fileName} in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return null;
        }

        return schematica;
    }

    public static List<string> GetValidSchematicas() {
        List<string> list = new();

        // Create directory at mod startup if it doesn't exist?
        if (!Directory.Exists(Schematica.SavePath))
            return list;

        foreach (string file in Directory.GetFiles(Schematica.SavePath)) {
            string fileName = Path.GetFileNameWithoutExtension(file);

            if (list.Contains(fileName))
                continue;

            try {
                using var fileStream = File.OpenRead(file);
                using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read);
                
                // If file is not valid, skip
                if (zipArchive.Entries.Count != 4 ||
                    zipArchive.GetEntry("metadata.dat") == null ||
                    zipArchive.GetEntry("data.dat") == null ||
                    zipArchive.GetEntry("dependencies.dat") == null ||
                    zipArchive.GetEntry("validation.dat") == null)
                    continue;

                var metadataEntry = zipArchive.GetEntry("metadata.dat");
                if (metadataEntry == null)
                    continue;

                using var metadataStream = metadataEntry.Open();
                using var memoryStream = new MemoryStream();
                metadataStream.CopyTo(memoryStream);
                
                //Setting back memoryStream to the start (gets left at the end with CopyTo)
                memoryStream.Position = 0;
                using var reader = new BinaryReader(memoryStream);
                
                if (reader.ReadString() != "SCHEMATICA")
                    continue;

                list.Add(fileName);
            }
            catch (Exception e) {
                // File could not be read because it was open elsewhere
                Console.WriteLine(e);
            }
        }

        return list;
    }
    
    private static void WriteEntryToArchive(ZipArchive archive, string entryName, MemoryStream memoryStream, BinaryWriter memoryWriter, Action writeAction) {
        var zipEntry = archive.CreateEntry(entryName);
        using var entryStream = zipEntry.Open();
        writeAction();
        memoryWriter.Flush();

        // Writing memoryStream to ouput zip stream and flushing any pendant operations
        memoryStream.WriteTo(memoryStream);
        // Reseting memoryStream without abusing memory
        memoryStream.SetLength(0);
    }

    private static void ValidateAndParseMetadata(ZipArchive zipArchive, SchematicaData schematica) {
        var metadataEntry = zipArchive.GetEntry("metadata.dat");
        if (metadataEntry == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");

        using var metadataStream = metadataEntry.Open();
        using var reader = new BinaryReader(metadataStream);
            
        // Check header to determine if it's a valid file (or just renamed to .schematica)
        if (reader.ReadString() != "SCHEMATICA")
            throw new FileLoadException("Invalid header in metadata file");

        // Mod Version -> In case I ever need versioning for backwards compatibility..
        string schematicaModVersion = reader.ReadString();
                        
        //Schematica Size
        schematica.Size = new Point(reader.ReadInt32(), reader.ReadInt32());
    }

    private static void ValidateSchematicaIntegrity(ZipArchive zipArchive) {
        if (zipArchive.GetEntry("validation.dat") == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
    }
    
    private static void ValidateSchematicaDependencies(ZipArchive zipArchive) {
        var dependenciesEntry = zipArchive.GetEntry("dependencies.dat");
        if (dependenciesEntry == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
        
        var enabledMods = ModLoader.Mods.Select(mod => $"{mod.Name}@{mod.Version}").ToList();
        
        using var metadataStream = dependenciesEntry.Open();
        using var reader = new BinaryReader(metadataStream);
        
        int modDependencyCount = reader.ReadInt32();
        List<string> missingDependencies = new List<string>();

        for (int i = 0; i < modDependencyCount; i++) {
            var modDependency = reader.ReadString();
            if (!enabledMods.Contains(modDependency)) {
                missingDependencies.Add(modDependency);
            }
        }

        if (missingDependencies.Any())
            throw new FileLoadException($"Cannot import schematica because it requires the following dependencies: {string.Join(", ", missingDependencies)}");
    }

    private static void ValidateAndParseSchematicaData(ZipArchive zipArchive, SchematicaData schematicaData) {
        var dataEntry = zipArchive.GetEntry("data.dat");
        if (dataEntry == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
        
        schematicaData.TileDataList = new List<TileData>();
        
        using var dataStream = dataEntry.Open();
        using var reader = new BinaryReader(dataStream);
        
        int expectedDataBytesRead = schematicaData.Size.X * schematicaData.Size.Y * TileDataByteSize;
        
        if (dataEntry.Length != expectedDataBytesRead)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
        
        try {
            int actualDataBytesRead = 0;
            while (actualDataBytesRead < expectedDataBytesRead) {
                TileData tileData = new TileData();
                tileData.Deserialize(reader);
                schematicaData.TileDataList.Add(tileData);
                actualDataBytesRead += TileDataByteSize;
            }
        } catch (EndOfStreamException) {
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
        }
    }
}