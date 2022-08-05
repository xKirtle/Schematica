using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Schematica.Common.DataStructures;

namespace Schematica.Core.JsonConverters;

//TODO: Should I not use JsonConverters and do the conversion myself?
//https://www.newtonsoft.com/json/help/html/Performance.htm#:~:text=use%20the%20JavaScriptDateConverter.-,Manually%20Serialize,-The%20absolute%20fastest
public class SchematicDataConverter : JsonConverter<SchematicData>
{
    public override void WriteJson(JsonWriter writer, SchematicData schematic, JsonSerializer serializer) {
        
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(schematic.Name));
        writer.WriteValue(schematic.Name);
        
        writer.WritePropertyName(nameof(schematic.Size));
        serializer.Serialize(writer, schematic.Size, typeof(Point));
        
        writer.WritePropertyName(nameof(schematic.data) + "Length");
        writer.WriteValue(schematic.data.Count);
        
        writer.WritePropertyName(nameof(schematic.data));
        writer.WriteStartArray();
        
        for (int i = 0; i < schematic.data.Count; i++)
            serializer.Serialize(writer, schematic.data[i], typeof(TileData));
        
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public override SchematicData ReadJson(JsonReader reader, Type objectType, SchematicData existingValue, bool hasExistingValue, JsonSerializer serializer) {
        SchematicData schematic = new SchematicData();

        reader.Read();
        schematic.Name = reader.ReadAsString();

        reader.Read();
        int[] size = reader.ReadAsString().Split(", ").Select(x => int.Parse(x)).ToArray();
        schematic.Size = new Point(size[0], size[1]);

        reader.Read();
        int length = reader.ReadAsInt32() ?? 0;
        
        reader.Read(); // data
        reader.Read(); // [

        schematic.data = new List<TileData>(length);
        for (int i = 0; i < length; i++) 
            schematic.data.Add(serializer.Deserialize<TileData>(reader));

        while (reader.Read()) ; // ]]}

        return schematic;
    }
}