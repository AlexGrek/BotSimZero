using SimuliEngine.Basic;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public abstract class LogicalTask : IDumpable, IComparable<LogicalTask>
    {
        public LogicalTaskStatus Status { get; protected set; } = LogicalTaskStatus.NotStarted;

        public virtual void SetStarted()
        {
            if (this.Status != LogicalTaskStatus.NotStarted)
            {
                throw new InvalidOperationException("Task has already been started.");
            }
            this.Status = LogicalTaskStatus.InProgress;
        }

        public virtual void Cancel()
        {
            if (this.Status == LogicalTaskStatus.Finished)
            {
                throw new InvalidOperationException("Task has already been finished.");
            }
            this.Status = LogicalTaskStatus.Cancelled;
        }

        public virtual void Finish()
        {
            if (this.Status == LogicalTaskStatus.Cancelled)
            {
                throw new InvalidOperationException("Task has already been cancelled.");
            }
            this.Status = LogicalTaskStatus.Finished;
        }

        private object _createdBy;
        private readonly bool _isInterruptable = true;

        public object CreatedBy
        {
            get => _createdBy;
        }

        public LogicalTask(object createdBy)
        {
            _createdBy = createdBy;
        }

        public virtual bool IsInterruptable => _isInterruptable;

        public TimeSpan InterruptedAt { get; set; } = TimeSpan.Zero; // Time when the task was interrupted

        public virtual void InterruptByMoreImportantTask<T>(LogicalTask moreImportant, T actor, WorldState world) where T : MovingActor
        {
            Status = LogicalTaskStatus.Interrupted;
        }

        

        public virtual void OnTaskStart<T>(T actor, WorldState world) where T : MovingActor
        {
            
        }

        public virtual void OnTaskCompleted<T>(T actor, WorldState world) where T : MovingActor
        {

        }

        public virtual void OnTaskCancelled<T>(T actor, WorldState world) where T : MovingActor
        {

        }

        public virtual void ConsiderCenterChanged<T>(T actor, WorldState world) where T : MovingActor
        {

        }

        public float Priority { get; set; }

        public abstract LowLevelTask? ActiveLowLevelTask { get; protected set; }

        public int CompareTo(LogicalTask? other)
        {
            if (other == null)
            {
                return 1; // Current instance is greater than null
            }

            return Priority.CompareTo(other.Priority); // Compare based on Priority
        }

        public virtual void Resume()
        {
            if (this.Status == LogicalTaskStatus.Interrupted)
            {
                throw new InvalidOperationException($"Task was not interrupted, it was {this.Status}.");
            }
            this.Status = LogicalTaskStatus.InProgress;
        }

        public virtual void OnTaskResumed<T>(T actor, WorldState state) where T : MovingActor
        {
            
        }

        public virtual void ExecuteTask<T>(float deltaTime, T actor, WorldState state) where T : MovingActor
        {
            
        }

        public virtual bool RecoverFromInterruption<T>(T actor, WorldState state, IObstacle failedToMove) where T : MovingActor
        {
            return false; // Default implementation does not recover from interruption
        }

        public virtual bool Ended()
        {
            return this.Status == LogicalTaskStatus.Finished || this.Status == LogicalTaskStatus.Cancelled;
        }

        public virtual void ConsiderMovementStepCompleted<T>(T actor, WorldState state) where T : MovingActor
        {

        }
    }
}
