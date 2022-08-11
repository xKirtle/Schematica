using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Schematica.Common.DataStructures;
using Terraria.UI;

namespace Schematica.Common.UI.UIElements;

public class UISchematicaThumbnail : UIElement
{
    public SchematicaData Schematica { get; private set; }
    private float scale;
    private Vector2 offset;
    public void SetSchematica(SchematicaData schematica, Vector2 offset, float scale) {
        Schematica = schematica;
        this.offset = offset;
        this.scale = scale;
    }

    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);

        if (Schematica != null) {
            if (Schematica.Size.X > 500 || Schematica.Size.Y > 500)
                return;
            
            Vector2 position = new Vector2(GetOuterDimensions().X, GetOuterDimensions().Y);
            SchematicaPreview.DrawPreview(Schematica, position + offset, scale);
        }
    }
}