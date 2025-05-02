using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem.Bots;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public class Intellect<T>: IDumpable where T : MovingActor
    {
        protected T Actor;
        public Perception Perception { get; protected set; } = new Perception();

        public List<IBehavior<T>> Behaviors { get; protected set; } = [];

        public List<LogicalTask> Tasks { get; protected set; } = [];

        public LogicalTask? currentTask { get; protected set; } = null;

        public ConcurrentQueue<Action<Intellect<T>>> Mutators { get; private set; } = new ConcurrentQueue<Action<Intellect<T>>>();

        public void Mutate(Action<Intellect<T>> mutator)
        {
            Mutators.Enqueue(mutator);
        }

        protected void ApplyMutations()
        {
            while (Mutators.TryDequeue(out var mutator))
            {
                mutator(this);
            }
        }

        public void AddBehavior(IBehavior<T> behavior)
        {
            Behaviors.Add(behavior);
            behavior.OnBehaviorAdd(Actor, Actor.State);
        }

        public void RemoveBehavior(IBehavior<T> behavior)
        {
            Behaviors.Remove(behavior);
            behavior.OnBehaviorRemove(Actor, Actor.State);
        }

        public Intellect(T actor)
        {
            Actor = actor;
        }

        protected void InterruptCurrentTask(LogicalTask moreImportant, WorldState state)
        {
            if (currentTask != null)
            {
                currentTask.InterruptedAt = state.InGameTime;
                currentTask.InterruptByMoreImportantTask(moreImportant, Actor, state);
            }
        }

        public void ProcessBehaviors(float deltaTime, WorldState state)
        {
            // process behaviors
            foreach (var behavior in Behaviors)
            {
                // allow behaviors to see all finished and cancelled tasks
                // also allow behaviors to cancel tasks
                behavior.Update(deltaTime, Actor, state);
            }
            ApplyMutations();
            // remove finished and cancelled tasks
            Tasks.RemoveAll(t => t.Ended());
            Tasks.Sort(); // Sort tasks by priority if behaviors affected them
            if (Tasks.Count == 0)
            {
                currentTask = null;
                return;
            }
            var topPriority = Tasks.Last(); // Get the task with the highest priority
            if (currentTask != topPriority)
            {
                // deal with current task
                if (currentTask != null)
                {
                    if (currentTask.IsInterruptable)
                    {
                        InterruptCurrentTask(topPriority, state);
                    }
                    else
                    {
                        CancelCurrentTask(state);
                    }
                }

                // change current task
                currentTask = topPriority;
                if (currentTask != null)
                {
                    if (currentTask.Status == LogicalTaskStatus.Interrupted)
                    {
                        // resume task
                        currentTask.Resume();
                        currentTask.OnTaskResumed(Actor, state);
                    }
                    else
                    {
                        // start task
                        currentTask.SetStarted();
                        currentTask.OnTaskStart(Actor, state);
                    }
                }
            }
        }

        protected void CancelCurrentTask(WorldState state)
        {
            if (currentTask == null)
                return;
            currentTask.Cancel();
            currentTask.OnTaskCancelled(Actor, state);
            currentTask = null;
        }

        public void ProcessLogicalTask(float deltaTime, WorldState state)
        {
            if (currentTask == null)
                return;
            if (currentTask.Status == LogicalTaskStatus.Finished)
            {
                currentTask.OnTaskCompleted(Actor, state);
                currentTask = null;
            } else
            {
                if (currentTask.Status == LogicalTaskStatus.Cancelled)
                {
                    currentTask.OnTaskCancelled(Actor, state);
                    currentTask = null;
                }
            }
            if (currentTask == null)
                return;
            currentTask.ExecuteTask(deltaTime, Actor, state);
            currentTask.ActiveLowLevelTask?.ExecuteConcurrently(deltaTime, Actor, state);
            return;
        }

        public virtual void ConsiderCenterChanged()
        {
            if (currentTask != null)
            {
                currentTask.ConsiderCenterChanged(Actor, Actor.State);
            }
        }

        public virtual void ConsiderMovementStepCompleted()
        {
            if (currentTask != null)
            {
                currentTask.ConsiderMovementStepCompleted(Actor, Actor.State);
            }
        }

        internal void ConsiderMovementStepFailed(IObstacle failedToMove, WorldState state)
        {
            // run fail recovery chain
            // try to recover at low level
            bool recovered = false;
            if (currentTask != null && currentTask.ActiveLowLevelTask != null)
            {
                recovered = currentTask.ActiveLowLevelTask.RecoverFromInterruption(failedToMove);
            }
            if (!recovered && currentTask != null)
            {
                // try to recover at high level
                recovered = currentTask.RecoverFromInterruption(Actor, state, failedToMove);
            }
            if (!recovered)
            {
                if (currentTask != null)
                {
                    var behavior = Behaviors.FirstOrDefault(b => currentTask.CreatedBy == b);
                    if (behavior != null)
                    {
                        // try to recover at behavior level
                        recovered = behavior.RecoverFromInterruption(Actor, state, failedToMove);
                    }
                    if (!recovered)
                    {
                        // cancel task
                        if (currentTask != null)
                        {
                            currentTask.Cancel();
                            currentTask.OnTaskCancelled(Actor, state);
                            currentTask = null;
                        }
                    }
                }
            }
        }
    }
}
