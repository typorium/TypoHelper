using FlaglinesAndSuch;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.TypoHelper.Entities.ForTheoTeleporter
{

    public static class FlaglinesAndSuchPatches
    {


        // Moves the entity if it needs special attention (such as stand boxes)
        public static bool MoveIfCustomEntity(Entity entity, Vector2 moveto)
        {

            bool moved = false;

            // If entity is standbox
            if (entity is StandBox)
            {

                // Cast entity into standbox
                StandBox standbox = (StandBox)entity;

                // Calculate change in position
                Vector2 offset = moveto - standbox.Center;

                // Move standbox AND stand
                standbox.Center = moveto;
                standbox.Stand.Position += offset;

                moved = true;
            }

            return moved;
        }
    }

}