using BotSimZero.Core;
using Stride.Engine;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static BotSimZero.Core.WorldAwareSyncScript;

namespace BotSimZero.World.UI
{
    public class MultiCellHighlighter : StartupScript
    {
        protected Dictionary<HighlightType, Stack<Entity>> _highlights = new Dictionary<HighlightType, Stack<Entity>>();
        protected Dictionary<HighlightType, Stack<Entity>> _pool = new Dictionary<HighlightType, Stack<Entity>>();

        public Vector3 Nowhere => new Vector3(-1000, -1000, -1000);
        public int MaxPoolSize = 100;
        public float Height = 0.5f;

        public override void Start()
        {
            // initialize the highlight pools
            foreach (var highlightType in Enum.GetValues(typeof(HighlightType)).Cast<HighlightType>())
            {
                _highlights[highlightType] = new Stack<Entity>();
                _pool[highlightType] = new Stack<Entity>();
                int n = 1;
                // add one entity to each pool
                var newEntity = CreateHighlightEntity(highlightType, n);
                _pool[highlightType].Push(newEntity);
            }
        }

        /// <summary>
        /// Picks an entity from the pool of the specified type (does not track entity after this point). If the pool is empty, it creates a new entity.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Entity PickEntityOfType(HighlightType type)
        {
            var stack = _pool[type];
            if (stack.TryPop(out var entity))
            {
                return entity;
            }
            else
            {
                // if the stack is empty, create a new entity
                var newEntity = CreateHighlightEntity(type, _highlights[type].Count + 1);
                return newEntity;
            }
        }

        /// <summary>
        /// Uses an entity of the specified type (adds it to used entities). If the pool is empty, it creates a new entity.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Entity UseEntityOfType(HighlightType type)
        {
            var entity = PickEntityOfType(type);
            _highlights[type].Push(entity);
            return entity;
        }

        /// <summary>
        /// Clears all highlights of the specified type. If the pool is empty, it removes the entity from the scene.
        /// </summary>
        /// <param name="type"></param>
        public void ClearHighlight(HighlightType type)
        {
            var stack = _highlights[type];
            while (stack.Count > 0)
            {
                var entity = stack.Pop();
                entity.Transform.Position = Nowhere; // move it out of the way
                var scaler = entity.Get<MultiCellHighlightComponent>();
                if (scaler != null)
                {
                    entity.Remove(scaler); // remove the scaler
                }
                entity.Transform.Scale = new Vector3(1f, 0f, 1f);
                if (_pool[type].Count < MaxPoolSize)
                {
                    _pool[type].Push(entity); // return to pool
                }
                else
                {
                    entity.SetParent(null); // remove from scene
                }
            }
        }

        public void ClearAllHighlights()
        {
            foreach (var highlightType in Enum.GetValues(typeof(HighlightType)).Cast<HighlightType>())
            {
                ClearHighlight(highlightType);
            }
        }

        private Entity CreateHighlightEntity(HighlightType highlightType, int n)
        {
            var material = MaterialFactory.CreateSimpleMaterial(GraphicsDevice, MatchColorByHighlightType(highlightType));
            var entity = new Entity($"Highlight_{highlightType}_{n}");
            Entity.AddChild(entity);
            var modelComponent = new ModelComponent
            {
                Model = ModelFactory.CreateCube(GraphicsDevice)
            };
            //var material = Content.Load<Material>("UiAssets/AlmostTransparent");
            modelComponent.Materials.Add(new(0, material));
            entity.Add(modelComponent);
            entity.Transform.Position = Nowhere; // move it out of the way
            return entity;
        }

        private Stride.Core.Mathematics.Color MatchColorByHighlightType(HighlightType highlightType)
        {
            switch (highlightType)
            {
                case HighlightType.Transparent:
                    return Stride.Core.Mathematics.Color.Transparent;
                case HighlightType.Green:
                    return Stride.Core.Mathematics.Color.Green;
                case HighlightType.Red:
                    return Stride.Core.Mathematics.Color.Red;
                case HighlightType.Blue:
                    return Stride.Core.Mathematics.Color.Blue;
                case HighlightType.Yellow:
                    return Stride.Core.Mathematics.Color.Yellow;
                default:
                    return Stride.Core.Mathematics.Color.White;
            }
        }

        public void HighlightCell((int x, int y) cell, HighlightType color = HighlightType.Default, float delay = 0, float hvalue = 0.2f)
        {
            Vector3 position = GetWorldCellPosition(cell.x, cell.y);
            var entity = UseEntityOfType(color);
            entity.Transform.Position = position;
            entity.Transform.Scale = new Vector3(0f, hvalue, 0f); // scale zero at the beginning
            MultiCellHighlightComponent scaler = new MultiCellHighlightComponent
            {
                Delay = delay
            };
            entity.Add(scaler);
        }

        public void HighlightSubCell((int x, int y) cell, (int x, int y) subcell, int subdivisions, HighlightType color = HighlightType.Default, float hvalue = 0.2f)
        {
            Vector3 position = GetWorldCellPosition(cell.x, cell.y);
            var entity = UseEntityOfType(color);
            float size = GlobalGameContext.CellSize / subdivisions;
            Vector3 shift = new Vector3(subcell.x * size, 0, subcell.y * size);
            entity.Transform.Position = position + shift - new Vector3(GlobalGameContext.CellHalfSize, 0, GlobalGameContext.CellHalfSize);
            entity.Transform.Scale = new Vector3(size, hvalue, size); // scale to expected size
        }

        private Vector3 GetWorldCellPosition(int x, int y)
        {
            return new Vector3(x * GlobalGameContext.CellSize, Height, y * GlobalGameContext.CellSize);
        }

        public void HighlightPath(IEnumerable<(int x, int y)> cells, HighlightType color = HighlightType.Default, float delay = 0f, float hvalue = 0.2f)
        {
            float i = 0f;
            foreach (var cell in cells)
            {
                HighlightCell(cell, color, delay*i, hvalue);
                i++;
            }
        }

        public void HighlightValues(IEnumerable<(int x, int y, float value)> cells, HighlightType color = HighlightType.Default, float delay = 0f, float zeroValue = 0f)
        {
            foreach (var cell in cells)
            {
                HighlightCell((cell.x, cell.y), color, delay, zeroValue + cell.value);
            }
        }

        public enum HighlightType
        {
            Default,
            Transparent,
            Green,
            Red,
            Blue,
            Yellow
        }
    }
}
