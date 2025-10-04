using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.TypoHelper.Entities
{


    [CustomEntity("Typorium_TypoHelper_Entity_SubpixelPanel")]
    public class SubpixelPanel : Entity
    {

        // SubHUD things
        public static float SIZING_RATIO = 6;

        // Size
        float panel_width;
        float panel_height;

        // Appearance
        Color color;
        float opacity;


        // Constructor
        public SubpixelPanel(EntityData data, Vector2 offset, EntityID id) : base( data.Position + offset)
        {

            // Adds tag subhud
            this.AddTag(TagsExt.SubHUD);

            // Size
            this.panel_width = data.Width;
            this.panel_height = data.Height;

            // Appearance
            this.color = data.HexColor("color");
            this.opacity = data.Float("opacity");
        }


        // Render
        public override void Render()
        {

            // Original Rendering
            base.Render();

            // Gets camera
            Camera camera = this.SceneAs<Level>().Camera;
            if (camera == null)
            {
                return;
            }

            // Calculates and draws at position
            Vector2 square_position = new Vector2(
                this.Position.X - camera.X,
                this.Position.Y - camera.Y
            ) * SIZING_RATIO;

            // Draws thick line
            Draw.HollowRect(
                square_position.X,
                square_position.Y,
                this.panel_width * SIZING_RATIO,
                this.panel_height * SIZING_RATIO,
                this.color * this.opacity
            );

            // Gets player coordonates
            Player player = this.SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }

            // Calculates subpixels point position
            Vector2 subpixels = player.PositionRemainder + new Vector2(0.5f, 0.5f);
            Vector2 subpixels_position = square_position + new Vector2(
                subpixels.X * this.panel_width,
                subpixels.Y * this.panel_height
            ) * SIZING_RATIO;

            // Draws point
            for (int i = -2; i < 1; i++)
            {
                Draw.HollowRect(subpixels_position.X + i, subpixels_position.Y + i, -2*i, -2*i, this.color);
            }

            
        }
    }
}