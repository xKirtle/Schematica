using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Schematica.Common.DataStructures;

namespace Schematica.Core.JsonConverters;

//TODO: Should I not use JsonConverters and do the conversion myself?
//https://www.newtonsoft.com/json/help/html/Performance.htm#:~:text=use%20the%20JavaScriptDateConverter.-,Manually%20Serialize,-The%20absolute%20fastest
public class SchematicaDataConverter : JsonConverter<SchematicaData>
{
    public override void WriteJson(JsonWriter writer, SchematicaData schematica, JsonSerializer serializer) {
        
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(schematica.Name));
        writer.WriteValue(schematica.Name);
        
        writer.WritePropertyName(nameof(schematica.Size));
        serializer.Serialize(writer, schematica.Size, typeof(Point));
        
        writer.WritePropertyName(nameof(schematica.data) + "Length");
        writer.WriteValue(schematica.data.Count);
        
        writer.WritePropertyName(nameof(schematica.data));
        writer.WriteStartArray();
        
        for (int i = 0; i < schematica.data.Count; i++)
            serializer.Serialize(writer, schematica.data[i], typeof(TileData));
        
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public override SchematicaData ReadJson(JsonReader reader, Type objectType, SchematicaData existingValue, bool hasExistingValue, JsonSerializer serializer) {
        SchematicaData schematica = new SchematicaData();

        reader.Read();
        schematica.Name = reader.ReadAsString();

        reader.Read();
        int[] size = reader.ReadAsString().Split(", ").Select(x => int.Parse(x)).ToArray();
        schematica.Size = new Point(size[0], size[1]);

        reader.Read();
        int length = reader.ReadAsInt32() ?? 0;
        
        reader.Read(); // data
        reader.Read(); // [

        schematica.data = new List<TileData>(length);
        for (int i = 0; i < length; i++) 
            schematica.data.Add(serializer.Deserialize<TileData>(reader));

        while (reader.Read()) ; // ]]}

        return schematica;
    }
}