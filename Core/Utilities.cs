using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Schematica.Common.DataStructures;
using Terraria;
using Terraria.ModLoader;

namespace Schematica.Core;

public static class Utilities
{
    private static Regex FileNamesRegex = new Regex("(?:(?!\\\\).)+$");
    public static List<string> FileNamesInDirectory(string directoryFullPath) {
        return Directory.GetFiles(directoryFullPath).Select(x => FileNamesRegex.Match(x).Value.Split(".")[^2]).ToList();
    }

    /// <param name="sourcePath">Full path of the compressed source file</param>
    /// <returns>Stream of the contents of the compressed source file</returns>
    public static Stream GetStreamFromCompressedFile(string sourcePath) {
        Stream stream = null;
        ZipFile file = null;
        try {
            using var fileStream = File.OpenRead(sourcePath);
            file = new ZipFile(fileStream);
            stream = file.GetInputStream(file[0]);
        }
        catch (Exception e) {
#if !DEBUG
            ModContent.GetInstance<Schematica>().Logger.Warn(e);
#else       
            Console.WriteLine(e);
#endif
        }
        finally {
            if (file != null) {
                file.IsStreamOwner = true;
                file.Close();
            }
        }

        return stream;
    }
    
    public static string[] GetEnabledModsList() {
        string path = $@"{Path.Combine(Main.SavePath)}\Mods\enabled.json";
        
        using StreamReader sr = new StreamReader(File.ReadAllText(path));
        using JsonTextReader reader = new JsonTextReader(sr);
        JsonSerializer serializer = new JsonSerializer();

        return serializer.Deserialize<string[]>(reader);
    }
}