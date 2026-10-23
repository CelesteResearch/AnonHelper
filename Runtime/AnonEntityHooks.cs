using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Numerics;
using Celeste.Core.Scheduling;
using Celeste.Game.Entities.Interaction.Boosters;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.AnonHelper.Entities.Barriers;
using Celeste.Mods.AnonHelper.Entities.Boosters;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Scenes;
using Celeste.Simulation;
using Celeste.Simulation.Presentation.Lighting;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.AnonHelper.Runtime
{
    public sealed class AnonEntityHooks : IDisposable
    {
        private readonly ISourceEngineContext engine;
        private readonly Hook render;
        private readonly Hook boost;
        private readonly Func<OriginalBoosterState, SceneEntityState> outline = ReadField<SceneEntityState>("outline");
        private readonly Func<OriginalBoosterState, VertexLightState> light = ReadField<VertexLightState>("light");

        public AnonEntityHooks(ISourceEngineContext engine)
        {
            this.engine = engine;
            render = new Hook(typeof(OriginalLevelSceneRoot).GetMethod("Render"),
                (Action<Action<OriginalLevelSceneRoot>, OriginalLevelSceneRoot>)Render);
            try
            {
                boost = new Hook(
                    typeof(OriginalBoosterState).GetMethod("BoostRoutine",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly),
                    (Func<Func<OriginalBoosterState, RoomPlayerSession, Vector2, IEnumerator>, OriginalBoosterState,
                        RoomPlayerSession, Vector2, IEnumerator>)BoostRoutine);
            }
            catch
            {
                render.Dispose();
                throw;
            }
        }

        private void Render(Action<OriginalLevelSceneRoot> original, OriginalLevelSceneRoot level)
        {
            if (!ReferenceEquals(level.EngineContext, engine))
            {
                original(level);
                return;
            }

            foreach (var entity in level.SourceScene.Entities.GetTrackedEntities("Celeste.SeekerBarrier"))
                if (entity is InvisibleSeekerBarrier barrier)
                    barrier.Collider = barrier.HideCollider;
            original(level);
            foreach (var entity in level.SourceScene.Entities.GetTrackedEntities("Celeste.SeekerBarrier"))
                if (entity is InvisibleSeekerBarrier barrier)
                    barrier.Collider = barrier.RegCollider;
        }

        private IEnumerator BoostRoutine(Func<OriginalBoosterState, RoomPlayerSession, Vector2, IEnumerator> original,
            OriginalBoosterState booster, RoomPlayerSession player, Vector2 direction)
        {
            if (booster is OneUseBooster owned && ReferenceEquals(owned.EngineContext, engine))
            {
                ((SourceSceneState)booster.Scene).RemoveEntity(outline(booster));
                light(booster).Visible = false;
            }

            yield return new SourceSwapImmediately(original(booster, player, direction));
            if (booster is OneUseBooster delayed && ReferenceEquals(delayed.EngineContext, engine))
            {
                yield return .8f;
                booster.RemoveSelf();
            }
        }

        private static Func<OriginalBoosterState, TValue> ReadField<TValue>(string name)
        {
            var field = typeof(OriginalBoosterState).GetField(name,
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        ?? throw new MissingFieldException(typeof(OriginalBoosterState).FullName, name);
            var method = new DynamicMethod("ReadBooster" + name, typeof(TValue), new[] { typeof(OriginalBoosterState) },
                typeof(AnonEntityHooks), true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);
            return (Func<OriginalBoosterState, TValue>)method.CreateDelegate(
                typeof(Func<OriginalBoosterState, TValue>));
        }

        public void Dispose()
        {
            boost.Dispose();
            render.Dispose();
        }
    }
}
