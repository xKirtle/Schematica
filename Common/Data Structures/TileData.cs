using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria;

namespace Schematica.Common.DataStructures;

public class TileData
{
    //Max bytes per TileData: 14 bytes
    public ushort TType;    //2 bytes
    public ushort WType;    //2 bytes
    public byte LAmount;    //1 byte
    public byte LFlags;     //1 byte
    public short TFrameX;   //2 bytes
    public short TFrameY;   //2 bytes
    public int TBitpack;    //4 bytes
    
    // public int TNonFrameBits => (int) (TBitpack & 0xFF001FFF);

    public TileData() { }

    public TileData(int x, int y) => new TileData(Main.tile[x, y]);

    public TileData(Tile tile) => GenerateInfo(tile);

    public TileData GenerateInfo(Tile tile) {
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

        return this;
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write(TType);
        writer.Write(WType);
        writer.Write(LAmount);
        writer.Write(LFlags);
        writer.Write(TFrameX);
        writer.Write(TFrameY);
        writer.Write(TBitpack);
    }

    public void Deserialize(BinaryReader reader) {
        TType = reader.ReadUInt16();
        WType = reader.ReadUInt16();
        LAmount = reader.ReadByte();
        LFlags = reader.ReadByte();
        TFrameX = reader.ReadInt16();
        TFrameY = reader.ReadInt16();
        TBitpack = reader.ReadInt32();
    }
}