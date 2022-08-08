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
    public static List<string> GetEnabledModsList() {
        string enabledModsPath = $@"{Path.Combine(Main.SavePath)}\Mods\enabled.json";
        string[] modNames = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(enabledModsPath));

        string lastLaunchedModsPath = $@"{Path.Combine(Main.SavePath)}\LastLaunchedMods.txt";
        string[] lastLaunchedModsList = File.ReadAllLines(lastLaunchedModsPath);

        List<string> curatedModsList = new List<string>();
        for (int i = 0; i < lastLaunchedModsList.Length; i++) {
            if (modNames.Contains(lastLaunchedModsList[i].Split(" ")[0])) {
                curatedModsList.Add(lastLaunchedModsList[i]);
            }
        }

        return curatedModsList;
    }
}