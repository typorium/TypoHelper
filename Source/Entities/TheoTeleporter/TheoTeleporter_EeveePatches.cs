using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.EeveeHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.TypoHelper.Entities.ForTheoTeleporter
{


    public static class EeveeHelperPatches
    {


        // Moves the entity if it needs special attention (such as holdable containers)
        public static bool MoveIfCustomEntity(Entity entity, Vector2 moveto)
        {

            bool moved = false;

            // If entity is holdable container
            if (entity is HoldableContainer)
            {
                EntityContainerMover container = entity.Get<EntityContainerMover>();
                if (container != null)
                {
                    container.DoMoveAction(delegate
                    {
                        entity.Center = moveto;
                    });

                    moved = true;
                }
            }

            return moved;
        }
    }

}