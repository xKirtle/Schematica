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

    public CompactTileData(Tile tile) {
        CompactTileData compactTileData = GenerateCompactTileData(this, tile.Get<TileTypeData>(), tile.Get<WallTypeData>(), tile.Get<LiquidData>(), tile.Get<TileWallWireStateData>());
        // compactTileData.CopyTo(this);
    }

    public CompactTileData(TileData tileData) {
        CompactTileData compactTileData = GenerateCompactTileData(this, tileData.TileTypeData, tileData.WallTypeData, tileData.LiquidData, tileData.TileWallWireStateData);
        // compactTileData.CopyTo(this);
    }
    
    public static CompactTileData GenerateCompactTileData(CompactTileData compactTileData, TileTypeData tileTypeData, WallTypeData wallTypeData, LiquidData liquidData, TileWallWireStateData tileWallWireStateData) {
        // CompactTileData compactTileData = new CompactTileData();
        
        compactTileData.TType = tileTypeData.Type;
        compactTileData.WType = wallTypeData.Type;
        
        compactTileData.LAmount = liquidData.Amount;
        compactTileData.LFlags = (byte) TileDataPacking.Pack(liquidData.LiquidType, compactTileData.LFlags, 0, 6);
        compactTileData.LFlags = (byte) TileDataPacking.SetBit(liquidData.SkipLiquid, compactTileData.LFlags, 6);
        compactTileData.LFlags = (byte) TileDataPacking.SetBit(liquidData.CheckingLiquid, compactTileData.LFlags, 7);
        
        compactTileData.TFrameX = tileWallWireStateData.TileFrameX;
        compactTileData.TFrameY = tileWallWireStateData.TileFrameY;
        
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.HasTile, compactTileData.TBitpack, 0);
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.IsActuated, compactTileData.TBitpack, 1);
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.HasActuator, compactTileData.TBitpack, 2);

        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.TileColor, compactTileData.TBitpack, 3, 5);
        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.WallColor, compactTileData.TBitpack, 8, 5);
        
        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.TileFrameNumber, compactTileData.TBitpack, 13, 2);
        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.WallFrameNumber, compactTileData.TBitpack, 15, 2);
        
        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.WallFrameX / 36, compactTileData.TBitpack, 17, 4);
        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.WallFrameY / 36, compactTileData.TBitpack, 21, 3);
        
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.IsHalfBlock, compactTileData.TBitpack, 24);
        compactTileData.TBitpack = TileDataPacking.Pack((int) tileWallWireStateData.Slope, compactTileData.TBitpack, 25, 3);

        compactTileData.TBitpack = TileDataPacking.Pack(tileWallWireStateData.WireData, compactTileData.TBitpack, 28, 4);
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.RedWire, compactTileData.TBitpack, 28);
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.BlueWire, compactTileData.TBitpack, 29);
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.GreenWire, compactTileData.TBitpack, 30);
        compactTileData.TBitpack = TileDataPacking.SetBit(tileWallWireStateData.YellowWire, compactTileData.TBitpack, 31);

        return compactTileData;
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

    public void CopyTo(CompactTileData compactTileData) {
        compactTileData.TType = TType;
        compactTileData.WType = WType;
        compactTileData.LAmount = LAmount;
        compactTileData.LFlags = LFlags;
        compactTileData.TFrameX = TFrameX;
        compactTileData.TFrameY = TFrameY;
        compactTileData.TBitpack = TBitpack;
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