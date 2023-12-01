using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
     * (Not implemented)    -> List of mods enabled? (warn when they don't match, schematic might be wrong)
     *
     * Second .dat file in zip (data.dat)
     * Schematica Data      -> Read bytes until end of file
     *
     * Third file in zip (validation.dat)
     * Valid export flag    -> 1 bit (bool) that checks if export code reached the end
     */

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
            for (int j = 0; j < size.Y; j++) {
                for (int i = 0; i < size.X; i++) {
                    //It's actually slower to do that many write calls. Memory reduction gain from this is negligible
                    // if (memoryStream.Length + TileDataByteSize > memoryStream.Capacity) {
                    //     memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
                    //     WriteMemoryToDisk(memoryStream, outputStream);
                    // }

                    Tile tile = Main.tile[minEdge.X + i, minEdge.Y + j];
                    TileData tileData = new TileData(tile);
                    tileData.Serialize(memoryWriter);
                }
            }

            //Saving remaining info in buffer that didn't trigger write above
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

            //This is done when fetching valid schematicas already?
            using (ZipFile zipFile = new ZipFile(File.OpenRead(readPath))) {
                if (zipFile.FindEntry("validation.dat", false) == -1)
                    throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
            }
            
            using ZipInputStream inputStream = new ZipInputStream(File.OpenRead(readPath));
            inputStream.GetNextEntry(); //metadata.dat
            inputStream.CopyTo(memoryStream);

            //Setting back memoryStream to the start (gets left at the end with CopyTo)
            memoryStream.Position = 0;

            BinaryReader reader = new(memoryStream);

            //Check header to determine if it's a valid file (or just renamed to .schematica)
            if (reader.ReadString() != "SCHEMATICA")
                throw new FileLoadException("Cannot import non schematica files");

            //Mod Version -> In case I ever need versioning for backwards compatibility..
            string schematicaModVersion = reader.ReadString();

            //Schematica Size
            schematica.Size = new Point(reader.ReadInt32(), reader.ReadInt32());

            if (!onlyMetadata) {
                schematica.TileDataList = new List<TileData>();
                memoryStream.SetLength(0);
                
                inputStream.GetNextEntry(); //data.dat
                int totalDataBytesRead = CopyStream(inputStream, memoryStream);
                inputStream.CopyTo(memoryStream);
                
                //Checking if total bytes are the expected amount
                int expectedDataBytesRead = schematica.Size.X * schematica.Size.Y * TileDataByteSize;
                if (totalDataBytesRead != expectedDataBytesRead)
                    throw new FileLoadException("Cannot import corrupted or incomplete schematica files");
                
                memoryStream.Position = 0;

                while (memoryStream.Position < memoryStream.Length) {
                    TileData tileData = new TileData();
                    tileData.Deserialize(reader);
                    schematica.TileDataList.Add(tileData);
                }
            }

            Console.WriteLine($"Finished importing {fileName} in {sw.ElapsedMilliseconds}ms");
            
            return schematica;
        }
        catch (Exception e) {
#if !DEBUG
            ModContent.GetInstance<Schematica>().Logger.Warn(e);
#else
            Console.WriteLine(e);
#endif
        }

        return null;
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
                    if (zipFile.Count != 3 ||
                        zipFile.FindEntry("metadata.dat", false) == -1 ||
                        zipFile.FindEntry("data.dat", false) == -1 ||
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
    
    private static int CopyStream(Stream input, Stream output) {
        byte[] buffer = new byte[8192];
        int bytesRead;
        int totalBytesRead = 0;

        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) {
            output.Write(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }
}