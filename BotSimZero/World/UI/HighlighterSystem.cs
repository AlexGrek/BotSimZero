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
using SimuliEngine.World;

namespace BotSimZero.World.UI
{
    public class HighlighterSystem : SyncScript
    {
        private int _frameCtr = 0;
        public int UpdateEveryNFrames = 10;

        public struct CellSubcellHighlight
        {
            public int X;
            public int Y;
            public IEnumerable<(int x, int y)> Subcells;
            public HighlightType Type;
            public float Height = 0.2f;

            public CellSubcellHighlight()
            {
            }
        }

        public float CellSize = 1;
        public float HighlightGroundLevel = 0.2f;
        EventReceiver<Entity> clickReceiver;
        EventReceiver<(dynamic, HighlightType)> highlightAnyReceiver;
        EventReceiver<IEnumerable<CellSubcellHighlight>> highlightSubcellReceiver;
        private string botDebugInfo = null;
        private BotComponent botComponentDebug = null;
        private MultiCellHighlighter _multiHighlighter = null;

        public override void Start()
        {
            clickReceiver = new EventReceiver<Entity>(GlobalEvents.PointerClickEventKey);
            highlightAnyReceiver = new EventReceiver<(dynamic, HighlightType)>(UiContext.UiEvents.OnHighlightAnything);
            highlightSubcellReceiver = new EventReceiver<IEnumerable<CellSubcellHighlight>>(UiContext.UiEvents.OnHighlightSubcells);
            _multiHighlighter = Entity.Get<MultiCellHighlighter>();
            if (_multiHighlighter == null)
            {
                _multiHighlighter = new MultiCellHighlighter();
                this.Entity.Add(_multiHighlighter);
            }
        }

        public override void Update()
        {
            if (highlightSubcellReceiver.TryReceive(out var subcellList))
            {
                _multiHighlighter.ClearHighlight(HighlightType.Green);
                var first = true;
                HighlightType highlightType = HighlightType.Default;
                foreach (var s in subcellList)
                {
                    if (first)
                    {
                        _multiHighlighter.ClearHighlight(s.Type);
                        highlightType = s.Type;
                        first = false;
                    }
                    var cell = (s.X, s.Y);

                    foreach (var subcellCoord in s.Subcells)
                    {
                        _multiHighlighter.HighlightSubCell(cell, subcellCoord, WorldState.SubdivisionSize, highlightType);
                    }
                    _multiHighlighter.HighlightCell(cell, HighlightType.Green);
                }
            }
            if (highlightAnyReceiver.TryReceive(out var highlightInfo))
            {
                var (a, htype) = highlightInfo;
                if (a is CellComponent cell)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightCell((cell.Xlocation, cell.Ylocation), htype);
                }
                else if (a is ValueTuple<int, int> cellCoord)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightCell((cellCoord.Item1, cellCoord.Item2), htype);
                }
                else if (a is IEnumerable<ValueTuple<int, int>> list)
                {
                    _multiHighlighter.ClearHighlight(htype);
                    _multiHighlighter.HighlightPath(list.Select((tuple) => (tuple.Item1, tuple.Item2)), htype);
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
                    GlobalGameContext.Instance.UiContext.SetHighlightedBot(bot);
                    RenderBotnfo(bot);
                }
            }
            if (botComponentDebug != null)
            {
                botDebugInfo = $"Bot: {botComponentDebug.Id} {(botComponentDebug.Actor as MovingActor)?.GetNormalizedRotation()})";
            }
            else
            {
                botDebugInfo = null;
            }

            if (botDebugInfo != null)
            {
                DebugText.Print(botDebugInfo, new Int2(20, 100), Color.DarkSeaGreen);
            }

            if (_frameCtr >= UpdateEveryNFrames)
            {
                _frameCtr = 0;
                if (GlobalGameContext.Instance.UiContext.HighlightedBot != null)
                {
                    var bot = GlobalGameContext.Instance.UiContext.HighlightedBot;
                    if (bot != null)
                    {
                        RenderBotnfo(bot);
                    }
                }
            }

            _frameCtr++;
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
                    if (mov.MovementStep.HasValue)
                    {
                        //GlobalEvents.SendShowCellEvent(state, mov.MovementTarget.Value.targetX, mov.MovementTarget.Value.targetY, "Green");
                        GlobalGameContext.Instance.UiContext.HighlightAny((mov.MovementStep.Value.TargetCell.x, mov.MovementStep.Value.TargetCell.y), HighlightType.Red);
                    }
                    var subs = mov.TouchingSubcells().GroupBy(sub => sub.Item1).Select(group =>
                    new CellSubcellHighlight { Type = HighlightType.Blue, X = group.Key.cellX, Y = group.Key.cellY, Subcells = group.Select(sub => sub.Item2) });
                    GlobalGameContext.Instance.UiContext.HighlightSubcells(subs);
                }
            }
        }
    }
}
