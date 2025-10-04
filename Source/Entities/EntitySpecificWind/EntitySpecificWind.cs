using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.TypoHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.TypoHelper.Entities {


    [TrackedAs(typeof(EntitySpecificWind))]
    [CustomEntity("Typorium_TypoHelper_Entity_EntitySpecificWind")]
    public class EntitySpecificWind : Entity
    {

        // Appearance
        public float opacity;
        public Color color;
        private const float COLOR_ALPHA = 0.3f;
        Sprite indicator;

        // Particles
        List<Vector2> particles;
        int xoffset;
        Coroutine coroutine;

        // Gameplay
        string pattern_raw;
        WindController.Patterns pattern;
        bool account_for_level_wind;
        bool can_player_interact;
        bool can_entities_interact;

        // Compatibility mode
        bool compatibility;

        // Constructor
        public EntitySpecificWind(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {

            // Collisions
            this.Collider = new Hitbox(data.Width, data.Height);

            // Gameplay
            this.pattern_raw = data.Attr("wind");
            this.pattern = GetPattern( this.pattern_raw );
            this.account_for_level_wind = data.Bool("account_for_level_wind");
            this.can_player_interact = data.Bool("can_player_interact");
            this.can_entities_interact = data.Bool("can_entities_interact");

            // Appearance
            this.opacity = data.Float("opacity");
            this.color = data.HexColor("color");
            this.color = this.color * COLOR_ALPHA * this.opacity;
            
            this.indicator = GFX.SpriteBank.Create("Typorium_TypoHelper_EntitySpecificWind");
            this.Add(this.indicator);
            this.indicator.Play( data.Attr("wind") );
            this.indicator.Color *= this.opacity;

            // Particles
            this.xoffset = 0;

            float max_particles = this.Width * this.Height / 12;
            this.particles = new List<Vector2>();
            for (int i = 0; i < max_particles; i++)
            {
                this.particles.Add(new Vector2(
                    (int)( Calc.Random.NextFloat() * this.Width ),
                    (int)( Calc.Random.NextFloat() * this.Height )
                ));
            }

            this.coroutine = new Coroutine(this.ChangeWindLineRendering(), false);
            this.Add(this.coroutine);

            // Compatibility mode
            this.compatibility = data.Bool("compatibility_mode");
        }


        // Get wind pattern based off of string value
        public static WindController.Patterns GetPattern(string pattern)
        {
            switch (pattern)
            {
                case "none":
                    return WindController.Patterns.None;

                case "left":
                    return WindController.Patterns.Left;

                case "right":
                    return WindController.Patterns.Right;
                
                case "left_strong":
                    return WindController.Patterns.LeftStrong;

                case "right_strong":
                    return WindController.Patterns.RightStrong;

                case "left_onoff":
                    return WindController.Patterns.LeftOnOff;
                
                case "right_onoff":
                    return WindController.Patterns.RightOnOff;
                
                case "left_fast_onoff":
                    return WindController.Patterns.LeftOnOffFast;

                case "right_fast_onoff":
                    return WindController.Patterns.RightOnOffFast;

                case "alternating":
                    return WindController.Patterns.Alternating;

                case "right_crazy":
                    return WindController.Patterns.RightCrazy;

                case "down":
                    return WindController.Patterns.Down;

                case "up":
                    return WindController.Patterns.Up;

                case "space":
                    return WindController.Patterns.Space;

                default:
                    return WindController.Patterns.None;

            }
        }


        // Compatibility Collide Check (checks for collisions even when collider is unactive)
        public bool NewCollideCheck(Entity entity)
        {

            // If compatibility mode not activated, do normal collide check
            if (!this.compatibility)
            {
                return this.CollideCheck(entity);
            }

            // Else, do compatibility mode collisions
            bool collider_state = entity.Collidable;
            entity.Collidable = true;
            
            bool collide = this.CollideCheck(entity);

            entity.Collidable = collider_state;

            // Return result
            return collide;

        }
        

        // Update
        public override void Update()
        {

            // Original update
            base.Update();

            // Gets level
            Level level = Scene as Level;

            // Modifie indicator position
            this.indicator.Position = new Vector2(
                this.Width / 2,
                this.Height / 2
            );

            // Gets all entities that have a windmover component
            List<Component> all_windmover_component = SceneAs<Level>().Tracker.GetComponents<WindMover>();
            List<Entity> entities = new List<Entity>();

            all_windmover_component.ForEach(component => {
                if (component.Entity != null)
                {
                    entities.Add(component.Entity);
                }
            });

            // Checks if any entity is touching the rectangle
            for (int i = 0; i < entities.Count; i++)
            {

                // Get next entity
                Entity current_entity = entities[i];

                // If entity is player, and player can't interact, skip iteration
                if (!this.can_player_interact && current_entity is Player)
                {
                    continue;
                }

                // If entity is not a player and entities can't interact, skip interaction
                if (!this.can_entities_interact && !(current_entity is Player) )
                {
                    continue;
                }

                // If entity collides
                if (this.NewCollideCheck(current_entity))
                {

                    // If the entity already has a custom wind mover, change pattern
                    CustomWindMover custom_windmover_component = current_entity.Get<CustomWindMover>();
                    if (custom_windmover_component != null)
                    {
                        custom_windmover_component.SetPattern(this.pattern);
                        custom_windmover_component.ChangeAccountForLevelWind(this.account_for_level_wind);
                        custom_windmover_component.ChangeWindDirectionIcon(this.pattern_raw);
                        continue;
                    }

                    // If not, check if it's a wind-movable entity. If so, add a custom windmover
                    current_entity.Add(new CustomWindMover(this.pattern, this.account_for_level_wind, this.pattern_raw));
                }
            }
        }


        // Render
        private IEnumerator ChangeWindLineRendering()
        {
            while (true)
            {
                yield return 0.1f;
                this.xoffset += 3;
            }
        }
        

        public override void Render()
        {

            // Original
            base.Render();

            // Rectangle
            Draw.Rect(this.Collider, this.color);

            // Wind Lines
            int line_length = 3;
            this.particles.ForEach(particle => {

                // Start of the line
                Vector2 start = new Vector2(
                    (int) (particle.X + this.xoffset) % (this.Right - this.Left),
                    particle.Y                   
                ) + this.TopLeft;

                // End of the line
                Vector2 end = new Vector2(
                    Math.Clamp(start.X + line_length, this.Left, this.Right),
                    Math.Clamp(start.Y, this.Top, (int)this.Bottom)
                );

                // Draws line
                Draw.Line(start, end, this.color);
            });
        }
    }
}