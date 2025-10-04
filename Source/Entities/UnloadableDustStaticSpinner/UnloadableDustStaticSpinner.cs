using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaskaMod.Entities
{

	[CustomEntity("Typorium_TypoHelper_Entity_UnloadableDustStaticSpinner")]
	[TrackedAs(typeof(DustStaticSpinner))]

	class UnloadableDustStaticSpinner : DustStaticSpinner
	{


        // Constructor
		public UnloadableDustStaticSpinner(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset, data.Bool("attachToSolid", false), false)
		{
		}


        // Updater
		public override void Update()
		{

            // Original Updater
			base.Update();
            
            // Gets camera and unloads dust if out of camera
			Camera camera = this.SceneAs<Level>().Camera;
            if (camera == null)
            {
                return;
            }

			this.Collidable = base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f && base.Y < camera.Y + 180f + 16f;
		}
	}
}