using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem.TaskingSystem
{
    public class CompositeLogicalTask : LogicalTask, IEnumerable<LogicalTask>
    {
        public LogicalTask? CurrentTask { get; protected set; }

        public List<LogicalTask> CompletedTasks { get; protected set; } = new List<LogicalTask>();

        public IEnumerator<LogicalTask> GetEnumerator()
        {
            return _tasks.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void AddTask(LogicalTask task)
        {
            _tasks.Enqueue(task);
        }
        public LogicalTask RemoveNextTask(LogicalTask task)
        {
            return _tasks.Dequeue();
        }
        public LogicalTask PeekNextTask(LogicalTask task)
        {
            return _tasks.Peek();
        }

        public void RemoveTask(LogicalTask task)
        {
            _tasks = new Queue<LogicalTask>(_tasks.Where(t => t != task));
        }

        protected Queue<LogicalTask> _tasks = new Queue<LogicalTask>();

        public CompositeLogicalTask((int x, int y) point, object createdBy, params LogicalTask[] args) : base(createdBy)
        {
            foreach (var task in args)
            {
                AddTask(task);
            }
        }

        public override void OnTaskStart<T>(T actor, WorldState world)
        {
            if (CurrentTask != null)
            {
                throw new InvalidOperationException("CurrentTask is not null. This can only happen if OnTaskStart was called twice.");
            }
            if (_tasks.Count == 0)
            {
                this.Finish();
                return;
            }
            BeginNextTask(actor, world);
        }

        private void BeginNextTask<T>(T actor, WorldState world) where T : MovingActor
        {
            CurrentTask = _tasks.Dequeue();
            CurrentTask.SetStarted();
            CurrentTask.OnTaskStart(actor, world);
        }

        public override bool IsInterruptable => CurrentTask?.IsInterruptable ?? true;

        public override void OnTaskCancelled<T>(T actor, WorldState world)
        {
            if (CurrentTask != null)
            {
                CurrentTask.OnTaskCancelled(actor, world);
            }
        }

        public override void InterruptByMoreImportantTask<T>(LogicalTask moreImportant, T actor, WorldState world)
        {
            CurrentTask?.InterruptByMoreImportantTask(moreImportant, actor, world);
        }

        public override void OnTaskResumed<T>(T actor, WorldState state)
        {
            CurrentTask?.OnTaskResumed(actor, state);
        }

        public override void ConsiderCenterChanged<T>(T actor, WorldState world)
        {
            CurrentTask?.ConsiderCenterChanged(actor, world);
        }

        public override void Finish()
        {
            base.Finish();
            while (CurrentTask != null)
            {
                CurrentTask.Finish();
                CompletedTasks.Add(CurrentTask);
                CurrentTask = null;
                if (_tasks.Count >= 0)
                {
                    CurrentTask = _tasks.Dequeue();
                }
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            while (CurrentTask != null)
            {
                CurrentTask.Cancel();
                CurrentTask = null;
                if (_tasks.Count >= 0)
                {
                    CurrentTask = _tasks.Dequeue();
                }
            }
        }

        public override void ExecuteTask<T>(float deltaTime, T actor, WorldState state)
        {
            if (CurrentTask == null)
            {
                throw new InvalidOperationException("CurrentTask is null. This can only happen if OnTaskStart was never called.");
            }
            CurrentTask.ExecuteTask(deltaTime, actor, state);
            if (CurrentTask.Status == LogicalTaskStatus.Finished)
            {
                CurrentTask.OnTaskCompleted(actor, state);
                CompletedTasks.Add(CurrentTask);
                CurrentTask = null;
                if (_tasks.Count > 0)
                {
                    BeginNextTask(actor, state);
                }
                else
                {
                    this.Finish();
                }
            }
            if (CurrentTask != null && CurrentTask.Status == LogicalTaskStatus.Cancelled)
            {
                CurrentTask.OnTaskCancelled(actor, state);
                Cancel();
            }
        }

        public override void OnTaskCompleted<T>(T actor, WorldState world)
        {
            // we have already handled this in ExecuteTask
        }

        public override LowLevelTask? ActiveLowLevelTask
        {
            get => CurrentTask?.ActiveLowLevelTask;
            protected set
            {
                if (CurrentTask == null)
                {
                    throw new InvalidOperationException("CurrentTask is null. This can only happen if OnTaskStart was never called.");
                }

                // Fix: Cast 'CurrentTask' to 'CompositeLogicalTask' to access the protected member  
                if (CurrentTask is CompositeLogicalTask compositeTask)
                {
                    compositeTask.ActiveLowLevelTask = value;
                }
                else
                {
                    throw new InvalidOperationException("CurrentTask is not of type CompositeLogicalTask, so ActiveLowLevelTask cannot be set.");
                }
            }
        }

        public override bool RecoverFromInterruption<T>(T actor, WorldState state, IObstacle failedToMove)
        {
            return CurrentTask?.RecoverFromInterruption(actor, state, failedToMove) ?? false; 
        }
    }
}
