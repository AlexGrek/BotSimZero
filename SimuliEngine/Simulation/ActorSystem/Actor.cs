using SimuliEngine.Basic;
using SimuliEngine.Simulation.Obstacles;
using SimuliEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimuliEngine.Simulation.ActorSystem
{ 
    public abstract class Actor: IObstacle
    {
        private WorldState _stateReference;
        protected IRealPositionProvider _positionProvider;
        private bool _instantiated = false;
        private bool _removed = false;
        protected Switch CenterPositionChanged = new Switch();

        public Vector2 GetNormalizedPosition()
        {
            return _positionProvider.GetNormalizedPosition();
        }

        public void SetCenterPositionChanged()
        {
            CenterPositionChanged.Set(true);
        }

        public abstract float Size { get; }

        public abstract float HalfSize { get; }

        public (int x, int y) MainPosition { get; set; }

        protected WorldState State => _stateReference;

        public bool IsTemporary => true;

        public bool IsMoving => true;

        public Actor(WorldState stateReference, IRealPositionProvider positionProvider) {
            this._stateReference = stateReference;
            this._positionProvider = positionProvider;
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

        public abstract void Think(float deltaTime);

        public abstract bool TryMove(float deltaTime);

        public abstract void MovePosition(float deltaTime);
    }
}
