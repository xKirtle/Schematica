using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.ID;

namespace Schematica.Common.DataStructures;

public class CompactTileData
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

    public CompactTileData() { }

    public CompactTileData(int x, int y) => new CompactTileData(Main.tile[x, y]);

    public CompactTileData(Tile tile) => CopyFromTile(tile);

    public CompactTileData(TileData tileData) {
        TType = tileData.TileTypeData.Type;
        WType = tileData.WallTypeData.Type;

        LiquidData lData = tileData.LiquidData;
        LAmount = lData.Amount;
        LFlags = (byte) TileDataPacking.Pack(lData.LiquidType, LFlags, 0, 6);
        LFlags = (byte) TileDataPacking.SetBit(lData.SkipLiquid, LFlags, 6);
        LFlags = (byte) TileDataPacking.SetBit(lData.CheckingLiquid, LFlags, 7);


        TileWallWireStateData data = tileData.TileWallWireStateData;
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
    
    public CompactTileData CopyFromTile(Tile tile) {
        TileData tileData = new TileData(tile);
        return new CompactTileData(tileData);
    }

    public TileData ToTileData() {
        TileTypeData tileTypeData = new TileTypeData() {
            Type = TType
        };

        WallTypeData wallTypeData = new WallTypeData() {
            Type = WType
        };

        LiquidData liquidData = new LiquidData() {
            Amount = LAmount,
            LiquidType = TileDataPacking.Unpack(LFlags, 0, 6),
            SkipLiquid = TileDataPacking.GetBit(LFlags, 6),
            CheckingLiquid = TileDataPacking.GetBit(LFlags, 7)
        };

        TileWallWireStateData tileWallWireStateData = new TileWallWireStateData() {
            TileFrameX = TFrameX,
            TileFrameY = TFrameY,

            HasTile = TileDataPacking.GetBit(TBitpack, 0),
            IsActuated = TileDataPacking.GetBit(TBitpack, 1),
            HasActuator = TileDataPacking.GetBit(TBitpack, 2),

            TileColor = (byte) TileDataPacking.Unpack(TBitpack, 3, 5),
            WallColor = (byte) TileDataPacking.Unpack(TBitpack, 8, 5),

            TileFrameNumber = TileDataPacking.Unpack(TBitpack, 13, 2),
            WallFrameNumber = TileDataPacking.Unpack(TBitpack, 15, 2),

            WallFrameX = TileDataPacking.Unpack(TBitpack, 17, 4) * 36,
            WallFrameY = TileDataPacking.Unpack(TBitpack, 21, 3) * 36,

            IsHalfBlock = TileDataPacking.GetBit(TBitpack, 24),
            Slope = (SlopeType) TileDataPacking.Unpack(TBitpack, 25, 3),
            
            WireData = TileDataPacking.Unpack(TBitpack,  28, 4),
            RedWire = TileDataPacking.GetBit(TBitpack, 28),
            BlueWire = TileDataPacking.GetBit(TBitpack, 29),
            GreenWire = TileDataPacking.GetBit(TBitpack, 30),
            YellowWire = TileDataPacking.GetBit(TBitpack, 31)
        };

        return new TileData(tileTypeData, wallTypeData, liquidData, tileWallWireStateData);
    }

    public void CopyToTile(Tile tile) {
        TileData tileData = ToTileData();

        tile.Get<TileTypeData>() = tileData.TileTypeData;
        tile.Get<WallTypeData>() = tileData.WallTypeData;
        tile.Get<LiquidData>() = tileData.LiquidData;
        tile.Get<TileWallWireStateData>() = tileData.TileWallWireStateData;
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