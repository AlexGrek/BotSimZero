using SimuliEngine.Basic;
using SimuliEngine.Tiles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.Subsystems
{
    public class TemperatureSimulationSubsystem : SimulationSubsystem
    {
        public float EffectPower = 0.2f;

        public override void Tick(float dt, WorldState world)
        {
            float effectPowerPerTick = dt * EffectPower;
            world.Temperature.BeginProcessing();
            DoChangeTemp(world, effectPowerPerTick, world.Size);
            world.Temperature.FinalizeProcessing();
        }

        private static void DoChangeTemp(WorldState world, float effectPowerPerTick, (int sizeX, int sizeY) size)
        {
            var temperature = world.Temperature;
            temperature.ProcessAllCellsInParallel((x, y, map, temp) =>
            {
                var neighbors = Utils.NeighboursOfACell(x, y, size.sizeX, size.sizeY)
                                     .Where((cell) => Passability.IsAirPassable(cell, world));
                var sum = 0f;
                var cnt = 0f;
                foreach (var neighbor in neighbors)
                {
                    var (nx, ny) = neighbor;
                    var neighborTemp = temperature[nx, ny];
                    sum += neighborTemp;
                    cnt++;
                }
                var avg = sum / cnt;
                var delta = avg - temp;
                return temp + delta * effectPowerPerTick;
            });
        }
    }
}
