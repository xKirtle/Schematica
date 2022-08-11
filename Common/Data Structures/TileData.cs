using System.IO;
using Terraria;
using Terraria.ID;

namespace Schematica.Common.DataStructures;

public struct TileData //Takes up 14 bytes
{
    public ushort TileType = 0;
    public ushort WallType = 0;
    public byte LiquidAmount = 0;
    public byte LiquidFlags = 0; 
    /* byte -> [8 bits]
     [0,5]      -> LiquidType
     [6]        -> SkipLiquid
     [7]        -> CheckingLiquid
     */
    
    public short TileFrameX = 0;
    public short TileFrameY = 0;
    public int PersistentTileData = 0; //Non frame related bits
    /* int -> [32 bits]
     [0]        -> HasTile
     [1]        -> IsActuacted
     [2]        -> HasActuator
     [3,7]      -> TileColor
     [8,12]     -> WallColor
     [13,14]    -> TileFrameNumber
     [15,16]    -> WallFrameNumber
     [17,20]    -> WallFrameX (not in pixels)
     [21,23]    -> WallFrameY (not in pixels)
     [24]       -> IsHalfBlock
     [25,27]    -> Slope
     [28,31]    -> WireData (contains bits from all colors)
     
     ** Extra Info **
     [28]       -> RedWire
     [29]       -> BlueWire
     [30]       -> GreenWire
     [31]       -> YellowWire
     */

    public int LiquidType => TileDataPacking.Unpack((int) LiquidFlags, 0, 6);
    public bool SkipLiquid => TileDataPacking.GetBit((int) LiquidFlags, 6);
    public bool CheckingLiquid => TileDataPacking.GetBit((int) LiquidFlags, 7);
    public bool HasTile => TileDataPacking.GetBit(PersistentTileData, 0);
    public bool IsActuated => TileDataPacking.GetBit(PersistentTileData, 1);
    public bool HasActuator => TileDataPacking.GetBit(PersistentTileData, 2);
    public byte TileColor => (byte) TileDataPacking.Unpack(PersistentTileData, 3, 5);
    public byte WallColor => (byte) TileDataPacking.Unpack(PersistentTileData, 8, 5);
    public int TileFrameNumber => TileDataPacking.Unpack(PersistentTileData, 13, 2); 
    public int WallFrameNumber => TileDataPacking.Unpack(PersistentTileData, 15, 2); 
    public int WallFrameX => TileDataPacking.Unpack(PersistentTileData, 17, 4) * 36; 
    public int WallFrameY => TileDataPacking.Unpack(PersistentTileData, 21, 3) * 36; 
    public bool IsHalfBlock => TileDataPacking.GetBit(PersistentTileData, 24);
    public SlopeType Slope => (SlopeType) TileDataPacking.Unpack(PersistentTileData, 25, 3);
    public int WireData => TileDataPacking.Unpack(PersistentTileData, 28, 4);
    public bool RedWire => TileDataPacking.GetBit(PersistentTileData, 28);
    public bool BlueWire => TileDataPacking.GetBit(PersistentTileData, 29);
    public bool GreenWire => TileDataPacking.GetBit(PersistentTileData, 30);
    public bool YellowWire => TileDataPacking.GetBit(PersistentTileData, 31);
    

    public TileData(Tile tile) {
        TileType = tile.Get<TileTypeData>().Type;
        WallType = tile.Get<WallTypeData>().Type;

        LiquidData liquidData = tile.Get<LiquidData>();
        LiquidAmount = liquidData.Amount;
        LiquidFlags = (byte) TileDataPacking.Pack(liquidData.LiquidType, (int) LiquidFlags, 0, 6);
        LiquidFlags = (byte) TileDataPacking.SetBit(liquidData.SkipLiquid, (int) LiquidFlags, 6);
        LiquidFlags = (byte) TileDataPacking.SetBit(liquidData.CheckingLiquid, (int) LiquidFlags, 7);

        TileWallWireStateData tileWallWireStateData = tile.Get<TileWallWireStateData>();
        TileFrameX = tileWallWireStateData.TileFrameX;
        TileFrameY = tileWallWireStateData.TileFrameY;
        PersistentTileData = tileWallWireStateData.NonFrameBits;
    }

    public void CopyTo(Tile tile) {
        TileTypeData tileTypeData = new TileTypeData();
        tileTypeData.Type = TileType;
        
        WallTypeData wallTypeData = new WallTypeData();
        wallTypeData.Type = WallType;

        LiquidData liquidData = new LiquidData();
        liquidData.Amount = LiquidAmount;
        liquidData.LiquidType = TileDataPacking.Unpack((int) LiquidFlags, 0, 6);
        liquidData.SkipLiquid = TileDataPacking.GetBit((int) LiquidFlags, 6);
        liquidData.CheckingLiquid = TileDataPacking.GetBit((int) LiquidFlags, 7);

        TileWallWireStateData tileWallWireStateData = new TileWallWireStateData();
        tileWallWireStateData.TileFrameX = TileFrameX;
        tileWallWireStateData.TileFrameY = TileFrameY;
        tileWallWireStateData.SetAllBitsClearFrame(PersistentTileData);

        tile.Get<TileTypeData>() = tileTypeData;
        tile.Get<WallTypeData>() = wallTypeData;
        tile.Get<LiquidData>() = liquidData;
        tile.Get<TileWallWireStateData>() = tileWallWireStateData;
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write(TileType);
        writer.Write(WallType);
        writer.Write(LiquidAmount);
        writer.Write(LiquidFlags);
        writer.Write(TileFrameX);
        writer.Write(TileFrameY);
        writer.Write(PersistentTileData);
    }

    public void Deserialize(BinaryReader reader) {
        TileType = reader.ReadUInt16();
        WallType = reader.ReadUInt16();
        LiquidAmount = reader.ReadByte();
        LiquidFlags = reader.ReadByte();
        TileFrameX = reader.ReadInt16();
        TileFrameY = reader.ReadInt16();
        PersistentTileData = reader.ReadInt32();
    }
}