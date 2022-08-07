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
     * First .dat file in zip
     * Header               -> 10 bytes that spell out 'SCHEMATICA' in ASCII characters
     * Version              -> String that holds mod version it was made with
     * Schematica Size      -> 2 ushort values (x and y)
     *
     * Second .dat file in zip
     * Schematica Data      -> Read bytes until end of file
     * (Not implemented)    -> List of mods enabled? (warn when they don't match, schematic might be wrong)
     */

    public static void ExportSchematica(string fileName) {
        if (String.IsNullOrEmpty(fileName))
            throw new ArgumentNullException("Cannot save schematica with an invalid name");

        Point size = new Point(Math.Abs(CaptureInterface.EdgeA.X - CaptureInterface.EdgeB.X) + 1, Math.Abs(CaptureInterface.EdgeA.Y - CaptureInterface.EdgeB.Y) + 1);

        if (size == Point.Zero) //Save Button does not allow 
            throw new ArgumentException("Size of schematica cannot be zero");

        Point minEdge = new Point(Math.Min(CaptureInterface.EdgeA.X, CaptureInterface.EdgeB.X), Math.Min(CaptureInterface.EdgeA.Y, CaptureInterface.EdgeB.Y));

        try {
            //Making sure Schematica's path exists
            Directory.CreateDirectory(Schematica.SavePath);

            string writePath = $@"{Schematica.SavePath}\{fileName}.schematica";
            using var outputStream = new ZipOutputStream(File.Create(writePath)); //, Schematica.BufferSize);
            outputStream.SetLevel(Schematica.CompressionLevel);

            ZipEntry schematicaMetadataZipEntry = new ZipEntry("metadata.dat");
            schematicaMetadataZipEntry.DateTime = DateTime.Now;
            outputStream.PutNextEntry(schematicaMetadataZipEntry);

            BinaryWriter zipWriter = new BinaryWriter(outputStream);

            using var memoryStream = new MemoryStream(Schematica.BufferSize);
            BinaryWriter memoryWriter = new BinaryWriter(memoryStream);

            //Header
            memoryWriter.Write("SCHEMATICA");

            //Mod Version
            memoryWriter.Write(ModContent.GetInstance<Schematica>().Version.ToString());

            //Schematica Size
            memoryWriter.Write((ushort) size.X);
            memoryWriter.Write((ushort) size.Y);

            //Writing initial info to disk
            memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
            WriteMemoryToDisk(memoryStream, outputStream);

            ZipEntry schematicaDataZipEntry = new ZipEntry("data.dat");
            schematicaDataZipEntry.DateTime = DateTime.Now;
            outputStream.PutNextEntry(schematicaDataZipEntry);

            //TileData
            for (int j = 0; j < size.Y; j++) {
                for (int i = 0; i < size.X; i++) {
                    if (memoryStream.Length + TileDataByteSize > memoryStream.Capacity) {
                        memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
                        WriteMemoryToDisk(memoryStream, outputStream);
                    }

                    Tile tile = Main.tile[minEdge.X + i, minEdge.Y + j];
                    TileData tileData = new TileData(tile);
                    tileData.Serialize(memoryWriter);
                }
            }

            //Saving remaining info stuck in buffer
            memoryWriter.Flush(); //Ensures writer's data is flushed to its underlying stream (memoryStream)
            WriteMemoryToDisk(memoryStream, outputStream);

            //TODO: Add one bit to signal whether file has finished saving? Since saving is async, the UI Window will display
            //the file (which was created but not finalized) and user could try to load it when it's not ready yet
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
        if (String.IsNullOrEmpty(fileName))
            throw new ArgumentNullException("Cannot import schematica with an invalid name");

        SchematicaData schematica = new SchematicaData();
        schematica.Name = fileName;

        try {
            Stream memoryStream = new MemoryStream(Schematica.BufferSize);
            string readPath = $@"{Schematica.SavePath}\{fileName}.schematica";
            using var inputStream = new ZipInputStream(File.OpenRead(readPath)); //Still 
            inputStream.GetNextEntry();
            inputStream.CopyTo(memoryStream);

            //Setting back memoryStream to the start (gets left at the end with CopyTo)
            memoryStream.Position = 0;

            BinaryReader reader = new BinaryReader(memoryStream);

            //Check header to determine if it's a valid file (or just renamed to .schematica)
            if (reader.ReadString() != "SCHEMATICA")
                throw new FileLoadException("Cannot import non schematica files");

            //Mod Version -> In case I ever need versioning for backwards compatibility..
            string schematicaModVersion = reader.ReadString();

            //Schematica Size
            schematica.Size = new Point(reader.ReadUInt16(), reader.ReadUInt16());

            if (!onlyMetadata) {
                schematica.data = new List<TileData>();
                memoryStream.SetLength(0);
                inputStream.GetNextEntry();
                
                inputStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                while (memoryStream.Position < memoryStream.Length) {
                    TileData tileData = new TileData();
                    tileData.Deserialize(reader);
                    schematica.data.Add(tileData);
                }
            }

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
}