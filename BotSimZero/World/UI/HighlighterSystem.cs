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
using BotSimZero.Entities;
using SimuliEngine.Simulation.ActorSystem;
using Vortice.Mathematics;
using Int2 = Stride.Core.Mathematics.Int2;
using Color = Stride.Core.Mathematics.Color;
using System;
using BotSimZero.Core;

namespace BotSimZero.World.UI
{
    public class HighlighterSystem : SyncScript
    {
        public float CellSize = 1;
        public float HighlightGroundLevel = 0.2f;
        EventReceiver<Entity> clickReceiver;
        private string botDebugInfo = null;
        private BotComponent botComponentDebug = null;

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
                if (clickedOn.Get<BotComponent>() is not null and var bot)
                {
                    botComponentDebug = bot;
                    RenderBotnfo(bot);
                }
            }
            if (botComponentDebug != null)
            {
                botDebugInfo = $"Bot: {botComponentDebug.Id} ({botComponentDebug.Actor.MainPosition}@{botComponentDebug.Actor.GetNormalizedPosition()} -> {(botComponentDebug.Actor as MovingActor)?.MovementTarget})";
            }
            else
            {
                botDebugInfo = null;
            }

            if (botDebugInfo != null)
            {
                DebugText.Print(botDebugInfo, new Int2(20, 100), Color.DarkSeaGreen);
            }
        }

        private void RenderBotnfo(BotComponent bot)
        {
            var globalWorldController = this.Entity.Scene.Entities
                .FirstOrDefault(e => e.Get<GlobalController>() != null)
                ?.Get<GlobalController>();
            if (globalWorldController != null && globalWorldController.WorldState != null)
            {
                var state = globalWorldController.WorldState;
                var mov = bot.Actor as MovingActor;
                if (mov != null)
                {
                    if (mov.MovementTarget != null)
                    {
                        GlobalEvents.SendShowCellEvent(state, mov.MovementTarget.Value.targetX, mov.MovementTarget.Value.targetY, "Green");
                    }
                }
            }
        }
    }
}
