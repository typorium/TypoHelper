using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.TypoHelper.Triggers
{

    [CustomEntity("Typorium_TypoHelper_Trigger_PlayerNoWind")]
    public class PlayerNoWind : Trigger
    {

        // Wind State
        bool is_affected;

        // Component storage
        static WindMover player_windmover_component;


        // Load
        public static void Load()
        {

            // Events
            Everest.Events.Level.OnLoadLevel += delegate {
                player_windmover_component = null;
            };
            
        }

        // Unload
        public static void Unload()
        {}


        // Constructor
        public PlayerNoWind(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {

            // Wind State
            this.is_affected = data.Bool("affected");

        }


        // When player enters trigger
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            // Adds wind mover if affected is true
            if (this.is_affected)
            {
                this.AddWindMover(player);
                return;
            }

            // Else, remove it
            this.RemoveWindMover(player);

        }


        // Adds the wind mover component
        public void AddWindMover(Player player)
        {
            WindMover windmover_component = player.Get<WindMover>();
            if (windmover_component != null)
            {
                return;
            }
            
            player.Add(player_windmover_component);
        }


        // Remove the wind mover component
        public void RemoveWindMover(Player player)
        {

            // Gets the wind mover
            WindMover windmover_component = player.Get<WindMover>();
            if (windmover_component != null)
            {
                player_windmover_component = windmover_component;
                player.Remove(windmover_component);
            }

        }
    }
}