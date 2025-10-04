using System.Collections.Generic;
using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.TypoHelper.Entities
{

	[CustomEntity("Typorium_TypoHelper_Entity_TheoTeleporter")]
	[TrackedAs(typeof(TheoTeleporter))]
	[Tracked(true)]

	public class TheoTeleporter : Entity
	{

        // Appearance
        Sprite sprite;

        // Activation & Flag Activation
        bool activated;
		float cooldown_max;
		float cooldown_current;

        bool flag_activated;
        string flag;

        // Linking
		private string link_id;

        // Work with all Holdables
        bool work_with_all_holdables;

        // Sound Position
        static bool return_SFX_playing;

		// Constructor
		public TheoTeleporter(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
		{

            // Appearance
            this.sprite = GFX.SpriteBank.Create("Typorium_TypoHelper_TheoTeleporter");
			this.sprite.Play("activated", false, false);
			this.Add(this.sprite);

            // Activation & Flag Activation
            this.flag = data.Attr("flag");
            this.flag_activated = true;

			this.activated = true;
			this.cooldown_max = data.Float("cooldown", 0f);
			this.cooldown_current = 0f;

            // Linking
            this.link_id = data.Attr("link_id", "");

            // Work with all holdables
            this.work_with_all_holdables = data.Bool("work_with_all_holdables", defaultValue:false);

            // Collisions
            base.Depth = -1;
			base.Collider = new Hitbox(16f, 16f, -4f, -4f);			
			
		}

		
        // Updater
		public override void Update()
		{

            // Original Update
			base.Update();

            // Changes flag activation based off current flag value
			bool new_flag_state = (this.flag == "") || SaveData.Instance.CurrentSession_Safe.GetFlag(this.flag);

            // Check if there's been a change
            if (this.flag_activated != new_flag_state)
            {

                // Updates theo teleporter state
                this.activated = new_flag_state;
                this.flag_activated = new_flag_state;

                // Resets cooldown
                this.cooldown_current = 0f;

                // Change sprite accordingly
                this.sprite.Play(new_flag_state ? "flagd2a" : "flaga2d");
            }


            // IF flag deactivated, don't do anything
			if (!this.flag_activated)
			{
                return;
            }

            // If isn't activated
            if (!this.activated)
            {

                // Updates cooldown
                this.cooldown_current += Engine.DeltaTime;
                if (this.cooldown_current >= this.cooldown_max)
                {

                    // Activates theo teleporter
                    this.activated = true;
                    this.sprite.Play("d2a", false, false);
                    
                    // Plays audio if hasn't been played
                    if (!return_SFX_playing)
                    {
                        Audio.Play("event:/game/general/diamond_return", this.Position);
                        return_SFX_playing = true;
                    }
                    
                }
                return;
            }


            // If theo teleporter is currently on
            // Gets all entities based off the conditions set by mapper
            List<Entity> entities = new List<Entity>();
            
            foreach (Component component in this.Scene.Tracker.GetComponents<Holdable>())
            {
                Holdable holdable = (Holdable)component;
                if (holdable.Entity != null)
                {
                    if (this.work_with_all_holdables || holdable.Entity is TheoCrystal)
                    {
                        entities.Add(holdable.Entity);
                    }
                }
            }
            
            // If there's no entity corresponding to the filters, stop updating
            if (entities.Count <= 0)
            {
                return;
            }

            // If there's no other theo teleporter in scene, don't do anything
            List<Entity> other_teleporters = this.Scene.Tracker.GetEntities<TheoTeleporter>();
            if (other_teleporters == null)
            {
                return;
            }

            // For all holdables entities
            bool did_an_entity_collide = false;
            foreach (Entity current_entity in entities)
            {

                // Check for collision
                if (!current_entity.CollideCheck(this))
                {
                    continue;
                }

                // Checks if the current theo is held
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null && player.Holding == current_entity.Get<Holdable>())
                {
                    continue;
                }

                // Checks for the closest theo teleporter with the same ID
                float closest_teleporter_distance = float.PositiveInfinity;
                TheoTeleporter closest_valid_teleporter = null;

                foreach(TheoTeleporter current_teleporter in other_teleporters)
                {

                    // Checks if the selected theo teleporter is the same one as self, if so, skip
                    if (current_teleporter == this)
                    {
                        continue;
                    }

                    // Checks if this TT and the current one share the same Link ID, and skip to the next one if they don't
                    if (current_teleporter.link_id != this.link_id)
                    {
                        continue;
                    }

                    // If the selected theo teleporter isn't activated, skip
                    if (!current_teleporter.activated)
                    {
                        continue;
                    }

                    // Checks if self and selected one are the closest ones
                    // If they are, update some variables
                    float distance = Vector2.Distance(this.Center, current_teleporter.Center);
                    if (distance < closest_teleporter_distance)
                    {
                        closest_teleporter_distance = distance;
                        closest_valid_teleporter = current_teleporter;
                    }
                }

                // If no theo teleporters where valid, don't do anything
                if (closest_valid_teleporter == null)
                {
                    return;
                }
                
                // Theo collided
                did_an_entity_collide = true;

                // Update selected theo telepoorter
                closest_valid_teleporter.activated = false;
                closest_valid_teleporter.cooldown_current = 0f;

                // Changes its animation
                closest_valid_teleporter.sprite.Play("a2d", false, false);

                // Moves the entity while considering special cases (such as holdable containers or standboxes)
                bool moved = false;

                // If special entity
                if (TypoHelperModule.EeveeHelper_Loaded)
                {
                    moved = ForTheoTeleporter.EeveeHelperPatches.MoveIfCustomEntity(current_entity, closest_valid_teleporter.Center);
                }

                if (TypoHelperModule.FlaglinesAndSuch_Loaded && !moved)
                {
                    moved = ForTheoTeleporter.FlaglinesAndSuchPatches.MoveIfCustomEntity(current_entity, closest_valid_teleporter.Center);
                }
                
                // If classic entity
                if (!moved)
                {
                    current_entity.Center = closest_valid_teleporter.Center;
                }
            }

            // Updates this theo teleporter if a theo has been teleported
            if (this.activated && did_an_entity_collide)
            {
                this.activated = false;
                this.cooldown_current = 0f;
                this.sprite.Play("a2d", false, false);
                Audio.Play("event:/game/general/diamond_touch", this.Position);
            }
		}
	}
}
