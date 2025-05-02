using SimuliEngine.Simulation.ActorSystem.TaskingSystem;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.Tiles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.Bots
{
    public class ChargeAnxietyBehavior: IBehavior<MovingActor>
    {
        private RunsOnBatteries? _batteryComponent;
        private LogicalTask? _task = null;

        public float ChargeAnxietyLevel { get; set; } = 30f;

        public void Update(float deltaTime, MovingActor actor, WorldState world)
        {
            if (_task != null)
            {
                _task.Priority = 100f - _batteryComponent?.ChargeLevel ?? 0;
                if (_task.Status == LogicalTaskStatus.Finished)
                {
                    _task = null;
                }
                else if (_task.Status == LogicalTaskStatus.Cancelled)
                {
                    _task = null;
                }
            }
            if (_batteryComponent == null)
            {
                return;
            }
            if (_batteryComponent.ChargeLevel < ChargeAnxietyLevel && _task == null)
            {
                Predicate<(int x, int y)> pred = (p) =>
                {
                    if (world.TileTypeMap[p.x, p.y] is TileType.InteractiveObject { ObjectData: ChargingStation station } && station.IsUsable)
                    {
                        return true;
                    }
                    return false;
                };

                var task = new FindAndGoToStaticPointLogicalTask(pred, 100, this);
                task.Priority = 100f - _batteryComponent.ChargeLevel;
                actor.Intellect.Tasks.Add(task);
                _task = task;
            }
        }
        public void OnBehaviorAdd(MovingActor actor, WorldState world)
        {
            actor.GetActorComponentOut<RunsOnBatteries>(out _batteryComponent);
        }
        public void OnBehaviorRemove(MovingActor actor, WorldState world)
        {
            // Logic to execute when the behavior is removed from the actor.
        }
        public bool RecoverFromInterruption(MovingActor actor, WorldState state, IObstacle failedToMove)
        {
            return false;
        }
    }
}
