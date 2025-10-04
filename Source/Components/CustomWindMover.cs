using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.TypoHelper.Components
{


    public class CustomWindMover : Component
    {

        // Appearance
        MTexture account_for_level_wind_icon;
        MTexture wind_direction_icon;

        // Wind Specific
        const float WIND_POWER = 0.1f;
        static Color windlines_color = new Color(255, 255, 255, 255);

        // Gameplay
        WindController.Patterns pattern;
        Coroutine coroutine;

        // Wind
        Vector2 windForce;
        Action<Vector2> Move;

        // Options
        bool account_for_level_wind;


        // Constructor
        public CustomWindMover(WindController.Patterns pattern, bool account_for_level_wind, string pattern_raw) : base(true, true)
        {

            // Appearance
            this.account_for_level_wind_icon = GFX.Game["entities/Typorium/TypoHelper/EntitySpecificWind/options/account_for_level_wind"];
            this.wind_direction_icon = GetIconByPatternID(pattern_raw);

            // Gameplay
            this.pattern = pattern;
            this.coroutine = null;

            // Wind
            this.windForce = Vector2.Zero;
            this.Move = delegate (Vector2 force) {

                // Adds current speed to entity
                this.Entity.Position += force;

            };

            // Options
            this.account_for_level_wind = account_for_level_wind;

            // Set Pattern
            this.SetPattern(WindController.Patterns.None);
        }


        // Rendering icons
        public override void Render()
        {

            // Special case, if wind is none, don't render
            if (this.pattern == WindController.Patterns.None)
            {
                return;
            }

            // If no entity attached, don't render
            if (this.Entity == null)
            {
                return;
            }

            // Original
            base.Render();

            // Set origin for icon drawings
            Vector2 origin = this.Entity.Position;

            // Modifies it based on image
            Image entity_image = this.Entity.Get<Image>();
            if (entity_image != null)
            {
                origin = entity_image.RenderPosition;
            }

            // Space In-Between two icons
            int margin = 6;

            // Account For Level Wind Icon Placement
            if (this.account_for_level_wind)
            {
                Vector2 accountlevelwind_drawing_position = origin - new Vector2(this.account_for_level_wind_icon.Width / 2 - margin, 0);
                this.account_for_level_wind_icon.Draw(accountlevelwind_drawing_position);
            }

            // Wind Direction Icon
            Vector2 winddirection_drawing_position = origin - new Vector2(this.wind_direction_icon.Width / 2 + margin, 0);
            this.wind_direction_icon.Draw(winddirection_drawing_position);

        }


        // When component added, update pattern
        public override void Added(Entity entity)
        {

            // Adds component to entity
            base.Added(entity);

            // Checks if the entity has a custom wind moving system
            // and takes the move function of it's windmover component.
            // Otherwise, take default
            WindMover entity_windmover_component = entity.Get<WindMover>();
            this.Move = entity_windmover_component.Move;

            // Changes pattern
            this.SetPattern(this.pattern);
        }


        // Update
        public override void Update()
        {

            // Original update
            base.Update();

            // If no entity attached, don't do anything
            if (this.Entity == null)
            {
                return;
            }

            Vector2 level_wind = this.account_for_level_wind ? (Scene as Level).Wind : Vector2.Zero;
            this.Move( (this.windForce - level_wind) * WIND_POWER * Engine.DeltaTime);

        }


        // Update component to account for level's wind
        public void ChangeAccountForLevelWind(bool account)
        {
            this.account_for_level_wind = account;
        }


        // Update wind direction icon
        public void ChangeWindDirectionIcon(string pattern_raw)
        {
            this.wind_direction_icon = GetIconByPatternID(pattern_raw);
        }


        // Gets icon by pattern's name
        public static MTexture GetIconByPatternID(string pattern_raw)
        {
            return GFX.Game["entities/Typorium/TypoHelper/EntitySpecificWind/windtypes/" + pattern_raw];
        }


        // Adds coroutine to entity
        private void AddCoroutine(Coroutine coroutine)
        {
            if (this.Entity == null)
            {
                return;
            }
            this.Entity.Add(coroutine);
        }


        // Removes coroutine to entity
        private void RemoveCoroutine(Coroutine coroutine)
        {
            if (this.Entity == null)
            {
                return;
            }
            this.Entity.Remove(coroutine);
        }


        // Set Wind Pattern
        public void SetPattern(WindController.Patterns pattern)
        {

            // Reset coroutine
            if (this.coroutine != null)
            {
                this.RemoveCoroutine(this.coroutine);
                this.coroutine = null;
            }

            // Chooses right pattern / coroutine
            this.windForce = Vector2.Zero;
            this.pattern = pattern;
            switch (pattern)
            {
                case WindController.Patterns.None:
                    return;

                case WindController.Patterns.Left:
                    this.windForce.X = -400f;
                    return;

                case WindController.Patterns.Right:
                    this.windForce.X = 400f;
                    return;

                case WindController.Patterns.LeftStrong:
                    this.windForce.X = -800f;
                    return;

                case WindController.Patterns.RightStrong:
                    this.windForce.X = 800f;
                    return;

                case WindController.Patterns.LeftOnOff:
                    this.coroutine = new Coroutine(this.LeftOnOffSequence(), true);
                    this.AddCoroutine(coroutine);
                    return;

                case WindController.Patterns.RightOnOff:
                    this.Entity.Add(this.coroutine = new Coroutine(this.RightOnOffSequence(), true));
                    return;

                case WindController.Patterns.LeftOnOffFast:
                    this.Entity.Add(this.coroutine = new Coroutine(this.LeftOnOffFastSequence(), true));
                    return;

                case WindController.Patterns.RightOnOffFast:
                    this.Entity.Add(this.coroutine = new Coroutine(this.RightOnOffFastSequence(), true));
                    return;

                case WindController.Patterns.Alternating:
                    this.Entity.Add(this.coroutine = new Coroutine(this.AlternatingSequence(), true));
                    return;

                case WindController.Patterns.RightCrazy:
                    this.windForce.X = 1200f;
                    return;

                case WindController.Patterns.Down:
                    this.windForce.Y = 300f;
                    return;

                case WindController.Patterns.Up:
                    this.windForce.Y = -400f;
                    return;

                case WindController.Patterns.Space:
                    this.windForce.Y = -600f;
                    break;

                default:
                    return;
            }
        }


        // Alternate Wind
        private IEnumerator AlternatingSequence()
        {
            while (true)
            {
                this.windForce.X = -400f;
                yield return 3f;
                this.windForce.X = 0f;
                yield return 2f;
                this.windForce.X = 400f;
                yield return 3f;
                this.windForce.X = 0f;
                yield return 2f;
            }
        }

        // Fast Left Wind On/Off
        private IEnumerator LeftOnOffFastSequence()
        {
            while (true)
            {
                this.windForce.X = -800f;
                yield return 2f;
                this.windForce.X = 0f;
                yield return 2f;
            }
        }

        // Left Wind On/Off
        private IEnumerator LeftOnOffSequence()
        {
            while (true)
            {
                this.windForce.X = -400f;
                yield return 3f;
                this.windForce.X = 0f;
                yield return 3f;
            }
        }

        // Fast Right Wind On/Off 
        private IEnumerator RightOnOffFastSequence()
        {
            while (true)
            {
                this.windForce.X = 800f;
                yield return 2f;
                this.windForce.X = 0f;
                yield return 2f;
            }
        }


        private IEnumerator RightOnOffSequence()
        {
            while (true)
            {
                this.windForce.X = 400f;
                yield return 3f;
                this.windForce.X = 0f;
                yield return 3f;
            }
        }


    }
}