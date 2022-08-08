using Terraria;

namespace Schematica.Common.DataStructures;

public class TileData
{
    public TileTypeData TileTypeData { get; }
    public WallTypeData WallTypeData { get; }
    public LiquidData LiquidData { get; }
    public TileWallWireStateData TileWallWireStateData { get; }
    
    public TileData(Tile tile) {
        TileTypeData = tile.Get<TileTypeData>();
        WallTypeData = tile.Get<WallTypeData>();
        LiquidData = tile.Get<LiquidData>();
        TileWallWireStateData = tile.Get<TileWallWireStateData>();
    }

    public TileData(TileTypeData tileTypeData, WallTypeData wallTypeData, LiquidData liquidData, TileWallWireStateData tileWallWireStateData) {
        TileTypeData = tileTypeData;
        WallTypeData = wallTypeData;
        LiquidData = liquidData;
        TileWallWireStateData = tileWallWireStateData;
    }
}