using System.Linq;
using BotSimZero.World.Terrain;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Input;
using Stride.Physics;
using Stride.Rendering.Materials;
using Stride.Rendering;
using Stride.Extensions;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Engine.Events;

namespace BotSimZero.World.UI
{
    public class HighlighterSystem : SyncScript
    {
        public float CellSize = 1;
        public float HighlightGroundLevel = 0.2f;
        EventReceiver<Entity> clickReceiver;

        public override void Start()
        {
            clickReceiver = new EventReceiver<Entity>(GlobalEvents.PointerClickEventKey);
        }

        public override void Update()
        {
            if (clickReceiver.TryReceive(out Entity clickedOn))
            {
                if (clickedOn.Get<CellComponent>() is not null and var cell)
                {
                    GlobalEvents.HighlightCellEventKey.Broadcast(cell);
                }
            }
        }
    }
}
