using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schematica.Core.JsonConverters;
using Terraria;

namespace Schematica.Common.DataStructures;

[JsonConverter(typeof(TileDataConverter))]
public class TileData
{
    public ushort TType;
    public ushort WType;
    public byte LAmount;
    public byte LFlags;
    public short TFrameX;
    public short TFrameY;
    public int TBitpack;
    // public int TNonFrameBits => (int) (TBitpack & 0xFF001FFF);

    public TileData() { }
    
    public TileData(Tile tile) {
        TType = tile.Get<TileTypeData>().Type;
        WType = tile.Get<WallTypeData>().Type;
        
        LiquidData lData = tile.Get<LiquidData>();
        LAmount = lData.Amount;
        LFlags = (byte) TileDataPacking.Pack(lData.LiquidType, LFlags, 0, 6);
        LFlags = (byte) TileDataPacking.SetBit(lData.SkipLiquid, LFlags, 6);
        LFlags = (byte) TileDataPacking.SetBit(lData.CheckingLiquid, LFlags, 7);


        TileWallWireStateData data = tile.Get<TileWallWireStateData>();
        TFrameX = data.TileFrameX;
        TFrameY = data.TileFrameY;
        
        TBitpack = TileDataPacking.SetBit(data.HasTile, TBitpack, 0);
        TBitpack = TileDataPacking.SetBit(data.IsActuated, TBitpack, 1);
        TBitpack = TileDataPacking.SetBit(data.HasActuator, TBitpack, 2);

        TBitpack = TileDataPacking.Pack(data.TileColor, TBitpack, 3, 5);
        TBitpack = TileDataPacking.Pack(data.WallColor, TBitpack, 8, 5);
        
        TBitpack = TileDataPacking.Pack(data.TileFrameNumber, TBitpack, 13, 2);
        TBitpack = TileDataPacking.Pack(data.WallFrameNumber, TBitpack, 15, 2);
        
        TBitpack = TileDataPacking.Pack(data.WallFrameX / 36, TBitpack, 17, 4);
        TBitpack = TileDataPacking.Pack(data.WallFrameY / 36, TBitpack, 21, 3);
        
        TBitpack = TileDataPacking.SetBit(data.IsHalfBlock, TBitpack, 24);
        TBitpack = TileDataPacking.Pack((int) data.Slope, TBitpack, 25, 3);

        TBitpack = TileDataPacking.Pack(data.WireData, TBitpack, 28, 4);
        TBitpack = TileDataPacking.SetBit(data.RedWire, TBitpack, 28);
        TBitpack = TileDataPacking.SetBit(data.BlueWire, TBitpack, 29);
        TBitpack = TileDataPacking.SetBit(data.GreenWire, TBitpack, 30);
        TBitpack = TileDataPacking.SetBit(data.YellowWire, TBitpack, 31);
    }
}