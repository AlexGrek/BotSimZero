using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem.ActorComponentSystem;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Bots
{
    public class RunsOnBatteries : IActorComponent
    {
        public float ChargeLevel { get; set; } = 100f; // Percentage of charge level
        public float ChargeConsumptionRate { get; set; } = 1f; // Percentage per second

        public float ChargingRate { get; set; } = 5f; // Percentage per second

        public bool IsCharging { get; set; } = false;

        public bool IsDead => ChargeLevel <= 0f; // Actor is dead if charge level is 0 or below

        public string ComponentName => "RunsOnBatteries";

        public bool IsIndependent => true;

        public UpdateFrequency RequiredUpdateFrequency => UpdateFrequency.EveryFrame;

        public void Initialize(Actor actor, WorldState world)
        {
            
        }

        public void Update(float deltaTime, Actor actor, WorldState world)
        {
            if (IsCharging)
            {
                ChargeLevel += ChargingRate * deltaTime;
                if (ChargeLevel > 100f)
                {
                    ChargeLevel = 100f; // Cap the charge level at 100%
                }
            }
            else
            {
                // Consume charge when not charging
                ChargeLevel -= ChargeConsumptionRate * deltaTime;
                if (ChargeLevel < 0f)
                {
                    ChargeLevel = 0f; // Prevent negative charge level
                }
            }
        }
    }
}
