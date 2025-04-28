using BotSimZero.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSimZero.VirtualUI
{
    internal class FloatingCellInfoSyncScript : WorldAwareSyncScript
    {
        EventReceiver<(int x, int y)?> cellEventReceiver;
        public float Height = 0.5f; // Height of the floating cell info above the ground
        private bool _movementAnimation = false;
        private Vector3 _targetPosition = Vector3.Zero;
        public float FadeSpeed { get; set; } = 2.0f;

        private float cellSize => GlobalGameContext.CellSize;

        public override void Start()
        {
            base.Start();
            cellEventReceiver = new EventReceiver<(int x, int y)?>(UiContext.UiEvents.OnCellHighlightChanged);
        }

        public override void Update()
        {
            if (cellEventReceiver.TryReceive(out (int x, int y)? coordinates))
            {
                if (coordinates.HasValue)
                {
                    _targetPosition = new Vector3(coordinates.Value.x, Height, coordinates.Value.y) * cellSize;
                    _movementAnimation = true;
                }
            }
            if (_movementAnimation)
            {
                // Calculate the direction and distance to the target
                var direction = _targetPosition - Entity.Transform.Position;
                var distance = direction.Length();

                if (distance < 0.1f) // Use constant speed for the final part
                {
                    // Normalize the direction and move at a constant speed
                    direction.Normalize();
                    Entity.Transform.Position += direction * (float)(FadeSpeed * Game.UpdateTime.Elapsed.TotalSeconds);

                    // Stop the animation if close enough to the target
                    if (distance < 0.01f)
                    {
                        Entity.Transform.Position = _targetPosition;
                        _movementAnimation = false;
                    }
                }
                else
                {
                    // Use Lerp for smooth movement
                    Entity.Transform.Position = Vector3.Lerp(
                        Entity.Transform.Position,
                        _targetPosition,
                        (float)(FadeSpeed * 10 * Game.UpdateTime.Elapsed.TotalSeconds));
                }
            }
        }
    }
}
