using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Xna.Framework;
using Schematica.Common.DataStructures;
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
        Stopwatch sw = Stopwatch.StartNew();

        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException("Cannot save schematica with an invalid name");

        Point size = new(Math.Abs(edgeA.X - edgeB.X) + 1, Math.Abs(edgeA.Y - edgeB.Y) + 1);

        if (size == Point.Zero) //Save Button does not allow 
            throw new ArgumentException("Size of schematica cannot be zero");

        Point minEdge = new(Math.Min(edgeA.X, edgeB.X), Math.Min(edgeA.Y, edgeB.Y));

        try {
            //Making sure Schematica's path exists
            Directory.CreateDirectory(Schematica.SavePath);

            string writePath = Path.Combine(Schematica.SavePath, $"{fileName}.schematica");
            using ZipOutputStream outputStream = new ZipOutputStream(File.Create(writePath), Schematica.BufferSize);
            outputStream.SetLevel(Schematica.CompressionLevel);

            ZipEntry schematicaMetadataZipEntry = new("metadata.dat");
            schematicaMetadataZipEntry.DateTime = DateTime.Now;
            outputStream.PutNextEntry(schematicaMetadataZipEntry);

            BinaryWriter zipWriter = new(outputStream);
            using MemoryStream memoryStream = new MemoryStream(Schematica.BufferSize);
            BinaryWriter memoryWriter = new(memoryStream);

            //Header
            memoryWriter.Write("SCHEMATICA");

            //Mod Version
            memoryWriter.Write(ModContent.GetInstance<Schematica>().Version.ToString());

            //Schematica Size
            memoryWriter.Write(size.X);
            memoryWriter.Write(size.Y);

            //Writing initial info to disk
            memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
            WriteMemoryToDisk(memoryStream, outputStream);

            ZipEntry schematicaDataZipEntry = new("data.dat");
            schematicaDataZipEntry.DateTime = DateTime.Now;
            outputStream.PutNextEntry(schematicaDataZipEntry);
            
            //TileData
            var modDependencies = new HashSet<string>();
            
            for (int j = 0; j < size.Y; j++) {
                for (int i = 0; i < size.X; i++) {
                    //It's actually slower to do that many write calls. Memory reduction gain from this is negligible
                    // if (memoryStream.Length + TileDataByteSize > memoryStream.Capacity) {
                    //     memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
                    //     WriteMemoryToDisk(memoryStream, outputStream);
                    // }
                    
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

            //Saving remaining info in buffer that didn't trigger write above
            memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
            WriteMemoryToDisk(memoryStream, outputStream);
            
            //Mod Dependencies
            ZipEntry schematicaDependenciesZipEntry = new("dependencies.dat");
            schematicaDependenciesZipEntry.DateTime = DateTime.Now;
            outputStream.PutNextEntry(schematicaDependenciesZipEntry);
            
            memoryWriter.Write(modDependencies.Count);
            foreach (string modDependency in modDependencies) {
                memoryWriter.Write(modDependency);
            }
            
            memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
            WriteMemoryToDisk(memoryStream, outputStream);

            ZipEntry finalizingEntry = new("validation.dat");
            finalizingEntry.DateTime = DateTime.Now;
            outputStream.PutNextEntry(finalizingEntry);

            memoryWriter.Write(true);
            memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
            WriteMemoryToDisk(memoryStream, outputStream);

            Console.WriteLine($"Finished exporting {fileName} in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception e) {
#if !DEBUG
            ModContent.GetInstance<Schematica>().Logger.Warn(e);
#else
            Console.WriteLine(e);
#endif
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
        Stopwatch sw = Stopwatch.StartNew();
        
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentNullException("Cannot import schematica with an invalid name");

        SchematicaData schematica = new();
        schematica.Name = fileName;

        try {
            Stream memoryStream = new MemoryStream(Schematica.BufferSize);
            string readPath = Path.Combine(Schematica.SavePath, $"{fileName}.schematica");
            using var zipFile = new ZipFile(File.OpenRead(readPath));
            
            ValidateAndParseMetadata(zipFile, schematica);
            ValidateSchematicaIntegrity(zipFile);
            ValidateSchematicaDependencies(zipFile);
            
            if (!onlyMetadata) {
                ValidateAndParseSchematicaData(zipFile, schematica);
            }

            Console.WriteLine($"Finished importing {fileName} in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception e) {
#if !DEBUG
            ModContent.GetInstance<Schematica>().Logger.Warn(e);
#else
            Console.WriteLine(e);
#endif

            return null;
        }

        return schematica;
    }

    private static void WriteMemoryToDisk(MemoryStream memoryStream, Stream diskOutputStream) {
        //Writing memoryStream to ouput zip stream and flushing any pendent operations
        memoryStream.WriteTo(diskOutputStream);
        //Reseting memoryStream without abusing memory
        memoryStream.SetLength(0);

        //flush diskOutput?
    }

    public static List<string> GetValidSchematicas() {
        List<string> list = new();

        //Create directory at mod startup if it doesn't exist?
        if (!Directory.Exists(Schematica.SavePath))
            return list;

        foreach (string file in Directory.GetFiles(Schematica.SavePath)) {
            string fileName = Path.GetFileNameWithoutExtension(file);

            if (list.Contains(fileName))
                continue;

            try {
                //If file is not valid, skip
                using (ZipFile zipFile = new ZipFile(File.OpenRead(file))) {
                    if (zipFile.Count != 4 ||
                        zipFile.FindEntry("metadata.dat", false) == -1 ||
                        zipFile.FindEntry("data.dat", false) == -1 ||
                        zipFile.FindEntry("dependencies.dat", false) == -1 ||
                        zipFile.FindEntry("validation.dat", false) == -1)
                        continue;
                }

                using (ZipInputStream inputStream = new ZipInputStream(File.OpenRead(file))) {
                    using MemoryStream memoryStream = new MemoryStream();
                    inputStream.GetNextEntry();
                    inputStream.CopyTo(memoryStream);

                    //Setting back memoryStream to the start (gets left at the end with CopyTo)
                    memoryStream.Position = 0;
                    BinaryReader reader = new(memoryStream);

                    if (reader.ReadString() != "SCHEMATICA")
                        continue;

                    list.Add(fileName);
                }
            }
            catch (Exception e) {
                //File could not be read because it was open elsewhere
#if !DEBUG
            ModContent.GetInstance<Schematica>().Logger.Warn(e);
#else
                Console.WriteLine(e);
#endif
            }
        }

        return list;
    }

    private static void ValidateAndParseMetadata(ZipFile zipFile, SchematicaData schematica) {
        var metadataEntry = zipFile.GetEntry("metadata.dat");
        if (metadataEntry == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
            
        using var metadataStream = zipFile.GetInputStream(metadataEntry);
        using var reader = new BinaryReader(metadataStream);
            
        // Check header to determine if it's a valid file (or just renamed to .schematica)
        if (reader.ReadString() != "SCHEMATICA")
            throw new FileLoadException("Invalid header in metadata file");

        // Mod Version -> In case I ever need versioning for backwards compatibility..
        string schematicaModVersion = reader.ReadString();
                        
        //Schematica Size
        schematica.Size = new Point(reader.ReadInt32(), reader.ReadInt32());
    }

    private static void ValidateSchematicaIntegrity(ZipFile zipFile) {
        if (zipFile.FindEntry("validation.dat", false) == -1)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
    }
    
    private static void ValidateSchematicaDependencies(ZipFile zipFile) {
        var dependenciesEntry = zipFile.GetEntry("dependencies.dat");
        if (dependenciesEntry == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
        
        var enabledMods = ModLoader.Mods.Select(mod => $"{mod.Name}@{mod.Version}").ToList();
        
        using var metadataStream = zipFile.GetInputStream(dependenciesEntry);
        using var reader = new BinaryReader(metadataStream);
        
        int modDependencyCount = reader.ReadInt32();

        for (int i = 0; i < modDependencyCount; i++) {
            var modDependency = reader.ReadString();
            if (!enabledMods.Contains(modDependency))
                throw new FileLoadException($"Cannot import schematica because it requires {modDependency}");
        }
    }

    private static void ValidateAndParseSchematicaData(ZipFile zipFile, SchematicaData schematicaData) {
        var dataEntry = zipFile.GetEntry("data.dat");
        if (dataEntry == null)
            throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
        
        schematicaData.TileDataList = new List<TileData>();
        
        using var dataStream = zipFile.GetInputStream(dataEntry);
        using var reader = new BinaryReader(dataStream);
        
        int expectedDataBytesRead = schematicaData.Size.X * schematicaData.Size.Y * TileDataByteSize;
        
        if (dataEntry.Size != expectedDataBytesRead)
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