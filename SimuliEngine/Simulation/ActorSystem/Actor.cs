using SimuliEngine.Basic;
using SimuliEngine.Simulation.ActorSystem.ActorComponentSystem;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem
{ 
    public abstract class Actor: IObstacle, IDumpable, ILoggable
    {
        protected WorldState _stateReference;
        public string Name;
        protected IRealPositionProvider _positionProvider;
        protected IRealRotationProvider _rotationProvider;
        private bool _instantiated = false;
        private bool _removed = false;
        protected Switch CenterPositionChanged = new Switch();
        public (int x, int y) PrevMainPosition { get; protected set; } = (0, 0);
        public (int x, int y)? MovementTargetPosition { get; protected set; } = null;

        public Vector2 GetNormalizedPosition()
        {
            return _positionProvider.GetWorldCoordinates();
        }

        public virtual void SetCenterPositionChanged((int x, int y) prevCenterCell)
        {
            CenterPositionChanged.Set(true);
            PrevMainPosition = prevCenterCell;
        }

        public abstract float Size { get; }

        public abstract float HalfSize { get; }

        public (int x, int y) MainPosition { get; set; }

        public WorldState State => _stateReference;

        public bool IsTemporary => true;

        public bool IsMoving => true;

        public Actor(WorldState stateReference, IRealPositionProvider positionProvider, IRealRotationProvider rotationProvider)
        {
            this._stateReference = stateReference;
            this._positionProvider = positionProvider;
            _rotationProvider = rotationProvider;
        }

        public void ChangeWorld(WorldState world)
        {
            this._stateReference = world;
        }

        public void Instantiate(int x, int y)
        {
            if (_instantiated)
                throw new InvalidOperationException("Actor is already instantiated.");
            // instantiate this entity
            _stateReference.InstantiateActor(this, x, y);
            this._positionProvider.SetWorldCoordinates(new Vector2(x, y));
            _instantiated = true;
        }

        public void RemoveInstance()
        {
            if (!_instantiated)
                throw new InvalidOperationException("Removing component before it was instantiated.");
            if (_removed)
                throw new InvalidOperationException("Actor is already removed.");
            // remove this entity
            _stateReference.RemoveActor(this);
            _removed = true;
        }

        public void UpdateComponentsFrame(float deltaTime)
        {
            if (_uninitialized.Count > 0)
            {
                while (_uninitialized.TryDequeue(out var uninit))
                {
                    uninit.Initialize(this, _stateReference);
                }
            }
            foreach (var component in _componentsToUpdateEveryFrame.Values)
            {
                component.Update(deltaTime, this, _stateReference);
            }
        }

        public void UpdateComponentsLow(float deltaTime)
        {
            foreach (var component in _componentsToUpdateLow.Values)
            {
                component.Update(deltaTime, this, _stateReference);
            }
        }

        public void UpdateComponentsHigh(float deltaTime)
        {
            foreach (var component in _componentsToUpdateHigh.Values)
            {
                component.Update(deltaTime, this, _stateReference);
            }
        }

        public abstract void Think(float deltaTime);

        public abstract bool TryMove(float deltaTime);

        public abstract void MovePosition(float deltaTime);

        public virtual bool IsPassable(WorldState state, (int x, int y) coordinates)
        {
            return false;
        }

        #region Internal entity component system

        protected Queue<IActorComponent> _uninitialized = new Queue<IActorComponent>();

        protected Dictionary<Type, IActorComponent> _components = new Dictionary<Type, IActorComponent>();

        protected Dictionary<Type, IActorComponent> _componentsToUpdateEveryFrame = new Dictionary<Type, IActorComponent>();

        protected Dictionary<Type, IActorComponent> _componentsToUpdateLow = new Dictionary<Type, IActorComponent>();

        protected Dictionary<Type, IActorComponent> _componentsToUpdateHigh = new Dictionary<Type, IActorComponent>();

        public void AddActorComponent<T>(T component) where T : IActorComponent
        {
            if (_components.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"Component of type {typeof(T)} already exists.");
            _components.Add(typeof(T), component);
            OrganizeComponentByFrequency(component);
        }

        public void AddActorComponent(IActorComponent component)
        {
            if (_components.ContainsKey(component.GetType()))
                throw new InvalidOperationException($"Component of type {component.GetType()} already exists.");
            _components.Add(component.GetType(), component);
            OrganizeComponentByFrequency(component);
        }

        private void OrganizeComponentByFrequency(IActorComponent component)
        {
            switch (component.RequiredUpdateFrequency)
            {
                case UpdateFrequency.EveryFrame:
                    _componentsToUpdateEveryFrame.Add(component.GetType(), component);
                    break;
                case UpdateFrequency.High:
                    _componentsToUpdateHigh.Add(component.GetType(), component);
                    break;
                case UpdateFrequency.Low:
                    _componentsToUpdateLow.Add(component.GetType(), component);
                    break;
                case UpdateFrequency.OnDemand:
                    break;
                default:
                    throw new ArgumentException($"Invalid update frequency: {component.RequiredUpdateFrequency}");
            }
        }

        public T GetActorComponent<T>() where T : IActorComponent
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                return (T)component;
            }
            throw new KeyNotFoundException($"Component of type {typeof(T)} not found.");
        }

        public void GetActorComponentOut<T>(out T component) where T : IActorComponent
        {
            if (_components.TryGetValue(typeof(T), out var gotComponent))
            {
                component = (T)gotComponent;
                return;
            }
            throw new KeyNotFoundException($"Component of type {typeof(T)} not found.");
        }

        public bool HasActorComponent<T>()
        {
            return _components.ContainsKey(typeof(T));
        }

        public void RemoveActorComponent<T>()
        {
            if (_components.ContainsKey(typeof(T)))
            {
                _components.Remove(typeof(T));
                _componentsToUpdateEveryFrame.Remove(typeof(T));
                _componentsToUpdateHigh.Remove(typeof(T));
                _componentsToUpdateLow.Remove(typeof(T));
            }
            else
            {
                throw new KeyNotFoundException($"Component of type {typeof(T)} not found.");
            }
        }

        public IReadOnlyList<IActorComponent> ActorComponents => _components.Values.ToList();

        #endregion

        public override string ToString()
        {
            return $"${this.GetType().Name}-{Name}";
        }
    }
}
