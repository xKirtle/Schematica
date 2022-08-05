using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Schematica.Core;

public static class Utilities
{
    private static Regex FileNamesRegex = new Regex("(?:(?!\\\\).)+$");
    public static List<string> FileNamesInDirectory(string directoryFullPath) {
        return Directory.GetFiles(directoryFullPath).Select(x => FileNamesRegex.Match(x).Value.Replace(".schematica", "")).ToList();
    }
}