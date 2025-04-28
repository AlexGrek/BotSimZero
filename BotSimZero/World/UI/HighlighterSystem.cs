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
using static BotSimZero.World.UI.MultiCellHighlighter;
using System.Collections;
using System.Collections.Generic;

namespace BotSimZero.World.UI
{
    public class HighlighterSystem : SyncScript
    {
        public float CellSize = 1;
        public float HighlightGroundLevel = 0.2f;
        EventReceiver<Entity> clickReceiver;
        EventReceiver<(dynamic, HighlightType)> highlightAnyReceiver;
        private string botDebugInfo = null;
        private BotComponent botComponentDebug = null;
        private MultiCellHighlighter _multiHighlighter = null;

        public override void Start()
        {
            clickReceiver = new EventReceiver<Entity>(GlobalEvents.PointerClickEventKey);
            highlightAnyReceiver = new EventReceiver<(dynamic, HighlightType)>(UiContext.UiEvents.OnHighlightAnything);
            _multiHighlighter = Entity.Get<MultiCellHighlighter>();
            if (_multiHighlighter == null)
            {
                _multiHighlighter = new MultiCellHighlighter();
                this.Entity.Add(_multiHighlighter);
            }
        }

        public override void Update()
        {
            if (highlightAnyReceiver.TryReceive(out var highlightInfo))
            {
                var (a, htype) = highlightInfo;
                if (a is CellComponent cell)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightCell((cell.Xlocation, cell.Ylocation), htype);
                }
                else if (a is ValueTuple<int,int> cellCoord)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightCell((cellCoord.Item1, cellCoord.Item2), htype);
                }
                else if (a is IEnumerable<ValueTuple<int, int>> list)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightPath(list.Select( (tuple) => (tuple.Item1, tuple.Item2)), htype);
                }
                else if (a is IEnumerable<ValueTuple<int, int, float>> listf)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightValues(listf.Select((tuple) => (tuple.Item1, tuple.Item2, tuple.Item3)), htype);
                }
                else
                {
                    throw new ArgumentException($"Invalid type {a.GetType()} for highlighting");
                }
            }
            if (clickReceiver.TryReceive(out Entity clickedOn))
            {
                if (clickedOn.Get<CellComponent>() is not null and var cell)
                {
                    GlobalEvents.HighlightCellEventKey.Broadcast(cell);
                    GlobalGameContext.Instance.UiContext.SetHighlightedCell((cell.Xlocation, cell.Ylocation));
                    GlobalGameContext.Instance.UiContext.HighlightAny((cell.Xlocation, cell.Ylocation), HighlightType.Green);
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
                        //GlobalEvents.SendShowCellEvent(state, mov.MovementTarget.Value.targetX, mov.MovementTarget.Value.targetY, "Green");
                        GlobalGameContext.Instance.UiContext.HighlightAny((mov.MovementTarget.Value.targetX, mov.MovementTarget.Value.targetY), HighlightType.Red);
                    }
                }
            }
        }
    }
}
