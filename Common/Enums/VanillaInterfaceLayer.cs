namespace Schematica.Common.Enums;

public enum VanillaInterfaceLayer
{
    Interface_Logic_1,
    MP_Player_Names,
    Emote_Bubbles,
    Entity_Markers,
    Smart_Cursor_Targets,
    Laser_Ruler,
    Ruler,
    Gamepad_Lock_On,
    Tile_Grid_Option,
    Town_NPC_House_Banners,
    Hide_UI_Toggle,
    Wire_Selection,
    Capture_Manager_Check,
    Ingame_Options,
    Fancy_UI,
    Achievement_Complete_Popups,
    Entity_Health_Bars,
    Invasion_Progress_Bars,
    Map_And_Minimap,
    Diagnose_Net,
    Diagnose_Video,
    Sign_Tile_Bubble,
    Hair_Window,
    Dresser_Window,
    NPC_And_Sign_Dialog,
    Interface_Logic_2,
    Resource_Bars,
    Interface_Logic_3,
    Inventory,
    Info_Accessories_Bar,
    Radial_Hotbars,
    Mouse_Text,
    Player_Chat,
    Death_Text,
    Cursor,
    Debug_Stuff,
    Mouse_Item_And_NPC_Head,
    Mouse_Over,
    Interact_Item_Icon,
    Interface_Logic_4
}

public static class _
{
    public static string Stringify(this VanillaInterfaceLayer layer) {
        string layerName = "";
        switch (layer) {
            case VanillaInterfaceLayer.Map_And_Minimap:
                layerName = "Map / Minimap";
                break;
            case VanillaInterfaceLayer.NPC_And_Sign_Dialog:
                layerName = "NPC / Sign Dialog";
                break;
            case VanillaInterfaceLayer.Mouse_Item_And_NPC_Head:
                layerName = "Mouse Item / NPC Head";
                break;
            default:
                layerName = layer.ToString().Replace("_", "");
                break;
        }

        return $"Vanilla: {layerName}";
    }
}