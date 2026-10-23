using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.LinearAlgebra;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.LevelLoader;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Gameplay.Holdables.Gliders;
using Celeste.Mods.AnonHelper.Entities.Clouds;
using Celeste.Mods.AnonHelper.Modules;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Dash;
using Celeste.Simulation.Gameplay.Player.Presentation.Hair;
using Celeste.Simulation.Gameplay.Platforms.Collision;
using Celeste.Simulation.Presentation.Player.Death;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.AnonHelper.Runtime
{
    public sealed class CustomDashPlayerHooks : IDisposable
    {
        private readonly AnonHelperModule module;
        private readonly List<IDisposable> hooks = new();
        private readonly Func<PlayerHair, ISourceEngineContext> hairOwner = CreateHairOwner();

        public CustomDashPlayerHooks(AnonHelperModule module)
        {
            this.module = module;
            try
            {
                hooks.Add(new Hook(Method(typeof(OriginalLevelLoaderScene), "LoadingThread"),
                    (Action<Action<OriginalLevelLoaderScene>, OriginalLevelLoaderScene>)LoadingThread));
                hooks.Add(new Hook(Method(typeof(DashSequence), "Begin"),
                    (Action<Action<DashSequence>, DashSequence>)DashBegin));
                hooks.Add(new Hook(Method(typeof(RoomPlayerSession), "Die"),
                    (Func<Func<RoomPlayerSession, Vector2, bool, bool, PlayerDeathPresentation>, RoomPlayerSession,
                        Vector2, bool, bool, PlayerDeathPresentation>)Die));
                hooks.Add(new Hook(Method(typeof(PlayerHair), "GetHairColor"),
                    (Func<Func<PlayerHair, int, PackedColor>, PlayerHair, int, PackedColor>)HairColor));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private static MethodInfo Method(Type type, string name) => type.GetMethod(name,
                                                                        BindingFlags.Instance | BindingFlags.Public |
                                                                        BindingFlags.NonPublic |
                                                                        BindingFlags.DeclaredOnly)
                                                                    ?? throw new MissingMethodException(type.FullName,
                                                                        name);

        private static Func<PlayerHair, ISourceEngineContext> CreateHairOwner()
        {
            var field = typeof(PlayerHair).GetField("engine", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? throw new MissingFieldException(typeof(PlayerHair).FullName, "engine");
            var method = new DynamicMethod("GetHairEngine", typeof(ISourceEngineContext), new[] { typeof(PlayerHair) },
                typeof(CustomDashPlayerHooks), true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Ret);
            return (Func<PlayerHair, ISourceEngineContext>)method.CreateDelegate(
                typeof(Func<PlayerHair, ISourceEngineContext>));
        }

        private void LoadingThread(Action<OriginalLevelLoaderScene> original, OriginalLevelLoaderScene loader)
        {
            original(loader);
            if (ReferenceEquals(loader.EngineContext, module.Engine)) Reset();
        }

        private void DashBegin(Action<DashSequence> original, DashSequence dash)
        {
            if (ReferenceEquals(dash.EngineContext, module.Engine))
            {
                if (dash.World is not RoomPlayerWorld world)
                {
                    if (module.Session.HasCloudDash || module.Session.HasJellyDash)
                        throw new InvalidOperationException(
                            "Anon dash spawning requires an actual scene player world.");
                }
                else
                {
                    var player = world.SourcePlayer;
                    if (module.Session.HasCloudDash)
                    {
                        module.Session.HasCloudDash = false;
                        player.Add(new SimulationCoroutine(module.Engine, CloudDelay(player)));
                    }

                    if (module.Session.HasJellyDash)
                    {
                        module.Session.HasJellyDash = false;
                        player.Add(new SimulationCoroutine(module.Engine, JellyDelay(player)));
                    }
                }
            }

            original(dash);
        }

        private PlayerDeathPresentation Die(
            Func<RoomPlayerSession, Vector2, bool, bool, PlayerDeathPresentation> original,
            RoomPlayerSession player, Vector2 direction, bool evenIfInvincible, bool registerDeath)
        {
            if (ReferenceEquals(player.EngineContext, module.Engine)) Reset();
            return original(player, direction, evenIfInvincible, registerDeath);
        }

        private PackedColor HairColor(Func<PlayerHair, int, PackedColor> original, PlayerHair hair, int index)
        {
            if (ReferenceEquals(hairOwner(hair), module.Engine))
            {
                if (module.Session.HasCloudDash) return PackedColor.LightCyan;
                if (module.Session.HasJellyDash) return new PackedColor(0x1e, 0x7b, 0xde);
            }

            return original(hair, index);
        }

        private IEnumerator CloudDelay(RoomPlayerSession player)
        {
            yield return player.Settings.SuperDashing ? .2f : .08f;
            var position = SpawnPosition(player);
            var level = (OriginalLevelSceneRoot)player.Scene;
            var world = ((ISourcePlatformCollisionScene)level).SolidWorldFactory.CreateJumpThru(position, false);
            level.AddEntity(new AnonCloud(module.Engine, position, false, true, false, world));
        }

        private IEnumerator JellyDelay(RoomPlayerSession player)
        {
            yield return player.Settings.SuperDashing ? .2f : .1f;
            var level = (OriginalLevelSceneRoot)player.Scene;
            level.AddEntity(new OriginalGliderState(SpawnPosition(player), true, false, module.Engine));
        }

        private Vector2 SpawnPosition(RoomPlayerSession player) => SourceVector2Math.Add(
            SourceVector2Math.Add(player.Position,
                ((ISourceVectorEnvironment)module.Engine).VectorArithmetic.Divide(player.Speed, 17)),
            player.Speed.X >= 288 ? new Vector2(1 / 2, 1) : new Vector2(0, -1));

        private void Reset()
        {
            module.Session.HasCloudDash = false;
            module.Session.HasJellyDash = false;
        }

        public void Dispose()
        {
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear();
        }
    }
}
