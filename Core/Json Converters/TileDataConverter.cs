using System;
using Newtonsoft.Json;
using Schematica.Common.DataStructures;

namespace Schematica.Core.JsonConverters;

public class TileDataConverter : JsonConverter<TileData>
{
    public override void WriteJson(JsonWriter writer, TileData tileData, JsonSerializer serializer) {
        writer.WriteStartArray();
        
        writer.WriteValue(tileData.TType);
        writer.WriteValue(tileData.WType);
        writer.WriteValue(tileData.LAmount);
        writer.WriteValue(tileData.LFlags);
        writer.WriteValue(tileData.TFrameX);
        writer.WriteValue(tileData.TFrameY);
        writer.WriteValue(tileData.TBitpack);
        
        writer.WriteEndArray();
    }

    public override TileData ReadJson(JsonReader reader, Type objectType, TileData existingValue, bool hasExistingValue, JsonSerializer serializer) {
        TileData tileData = new TileData();
        
        reader.Read(); // [
        
        tileData.TType = ushort.Parse(reader.ReadAsString());
        tileData.WType = ushort.Parse(reader.ReadAsString());
        tileData.LAmount = byte.Parse(reader.ReadAsString());
        tileData.LFlags = byte.Parse(reader.ReadAsString());
        tileData.TFrameX = short.Parse(reader.ReadAsString());
        tileData.TFrameY = short.Parse(reader.ReadAsString());
        tileData.TBitpack = int.Parse(reader.ReadAsString());
        
        reader.Read(); // ]
        
        return tileData;
    }
}