using SimuliEngine.Simulation.ActorSystem;
using SimuliEngine.Simulation.ActorSystem.Bots;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Tiles
{
    internal class ChargingStation : IInteractableObject
    {
        public bool IsOnline { get; set; } = true;

        public bool IsUsed { get; set; } = false;

        public bool IsUsable => IsOnline && !IsUsed;

        public void EndInteraction(Actor actor, WorldState world)
        {
            var charge = actor.GetActorComponent<RunsOnBatteries>();
            charge.IsCharging = false;
            IsUsed = false;
        }

        public void Interact(float dt, Actor actor, WorldState world)
        {

        }

        public void StartInteraction(Actor actor, WorldState world)
        {
            var charge = actor.GetActorComponent<RunsOnBatteries>();
            charge.IsCharging = true;
            IsUsed = true;
        }
    }
}
