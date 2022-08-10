using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Schematica.Common.Enums;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Schematica.Common.Systems;

[Autoload(Side = ModSide.Client)]
public abstract class UISystem<T> : ModSystem where T : UIState, new()
{
    public static UISystem<T> Instance;

    public UserInterface userInterface;
    public T uiState;
    public virtual VanillaInterfaceLayerID VanillaInterfaceLayer { get; protected set; } = VanillaInterfaceLayerID.Ruler;
    public virtual string InterfaceLayerName { get; protected set; } = typeof(T).Name;
    public virtual InterfaceScaleType InterfaceScaleType { get; protected set; } = InterfaceScaleType.None;

    protected GameTime lastUpdateUiGameTime;

    public override void Load() {
        Instance = this;

        userInterface = new UserInterface();
        uiState = new T();
        userInterface.SetState(uiState);
    }

    public override void UpdateUI(GameTime gameTime) {
        lastUpdateUiGameTime = gameTime;
        if (userInterface?.CurrentState != null)
            userInterface.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        //https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values
        int interfaceLayer = layers.FindIndex(layer => layer.Name.Equals(VanillaInterfaceLayer.Stringify()));
        if (interfaceLayer != -1)
            layers.Insert(interfaceLayer, new LegacyGameInterfaceLayer($"Schematica: {InterfaceLayerName}", DrawMethod, InterfaceScaleType));
    }

    protected virtual bool DrawMethod() {
        if (lastUpdateUiGameTime != null && userInterface?.CurrentState != null)
            userInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);

        return true;
    }

    public void Activate() {
        if (userInterface?.CurrentState != uiState)
            userInterface?.SetState(uiState);
    }

    public void Deactivate() {
        if (userInterface?.CurrentState != null)
            userInterface?.SetState(null);
    }
}