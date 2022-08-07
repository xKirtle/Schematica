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

    public static string[] GetEnabledModsList() {
        string path = $@"{Path.Combine(Main.SavePath)}\Mods\enabled.json";
        
        using StreamReader sr = new StreamReader(File.ReadAllText(path));
        using JsonTextReader reader = new JsonTextReader(sr);
        JsonSerializer serializer = new JsonSerializer();

        return serializer.Deserialize<string[]>(reader);
    }
}