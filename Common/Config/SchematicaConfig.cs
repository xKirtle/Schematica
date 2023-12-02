using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Schematica.Common.Config;

public class SchematicaConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool AllowOlderSchematicas;
}