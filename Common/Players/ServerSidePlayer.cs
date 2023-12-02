using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Schematica.Common.Players;

public class ServerSidePlayer : ModPlayer
{
    public override void OnEnterWorld() {
        string message = Mod.NetID >= 0 ? "Has Schematica Mod in server" : "Doesn't have Schematica Mod in server";
        Console.WriteLine(message);
    }
}