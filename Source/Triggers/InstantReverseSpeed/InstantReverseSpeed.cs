using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.TypoHelper.Triggers
{

    [CustomEntity("Typorium_TypoHelper_Trigger_InstantReverseSpeed")]
    public class InstantReverseSpeed : Trigger
    {


        // Constructor
        public InstantReverseSpeed(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            this.AddTag(Tags.FrozenUpdate);
        }


        // When player enters trigger
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            // Reverses the speed of the player
            player.Speed.X = 0 - player.Speed.X;
            player.Facing = (player.Facing == Facings.Left) ? Facings.Right : Facings.Left;
            player.DashDir.X = 0 - player.DashDir.X;
            
        }
    }
}