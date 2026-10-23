using System;
using System.Numerics;
using Celeste.Core.Audio;
using Celeste.Core.Randomness;
using Celeste.Core.Rendering;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Gameplay.Platforms.Clouds;
using Celeste.Core.Mathematics.LinearAlgebra;
using Celeste.Graphics.Animation;
using Celeste.Mods.AnonHelper.Modules;
using Celeste.Scene.Components.Oscillation;
using Celeste.Scene.Entities;
using Celeste.Simulation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Gameplay.Platforms.Collision;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.AnonHelper.Entities.Clouds
{
    [CustomEntity("Anonhelper/AnonCloud")]
    [RuntimeTypeAlias("Celeste.Mod.Anonhelper.AnonCloud")]
    public sealed class AnonCloud : SourceJumpThruEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly AnonHelperModule module;
        public Sprite sprite;
        public WigglerComponent wiggler;
        public bool fragile, pink, Small;
        public float startY;
        private ParticleTypePresentation particleType;
        private SoundSourceComponentState sfx;
        private bool waiting = true, canRumble, returning;
        private float speed, timer, respawnTimer;
        private Vector2 scale;

        public AnonCloud(ISourceEngineContext engine, Vector2 position, bool pink, bool fragile, bool small,
            ISourceSolidWorld world) : base(position, 32, false, world)
        {
            this.engine = engine;
            module = AnonHelperModule.For(engine);
            Small = small;
            this.fragile = fragile;
            this.pink = pink;
            startY = Y;
            Collider.Position.X = -16f;
            timer = SourceRandomMath.NextFloat(((ISourceRandomEnvironment)engine).Random) * 4f;
            Add(wiggler = ((ISourceWigglerPoolScope)engine).WigglerPool.Create(.3f, 4));
            particleType = pink
                ? module.CloudParticles.FragileCloud
                : ((IOriginalGameplayProcess)engine).ParticleTypes.Cloud.P_Cloud;
            SurfaceSoundIndex = 4;
            Add(new LightOccludeState(.2f));
            scale = Vector2.One;
            Add(sfx = new SoundSourceComponentState(engine));
        }

        public AnonCloud(ISourceEngineContext engine, EntityData data, Vector2 offset, ISourceSolidWorld world)
            : this(engine, data.Position + offset, data.Bool("pink"), data.Bool("fragile", true), data.Bool("small"),
                world)
        {
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new AnonCloud(level.EngineContext, data, offset,
            ((ISourcePlatformCollisionScene)level).SolidWorldFactory.CreateJumpThru(data.Position + offset, false));

        public override void Added(object scene)
        {
            base.Added(scene);
            string name = pink ? "pinkcloud" : "whitecloud";
            if (Small)
            {
                Collider.Position.X += 2;
                Collider.Width -= 6;
                name += "Remix";
            }

            Add(sprite = module.Sprites.Create(name));
            sprite.Origin = new Vector2(sprite.Width / 2f, 8);
            sprite.OnFrameChange = animation =>
            {
                if (animation == "spawn" && sprite.CurrentAnimationFrame == 6) wiggler.Start();
                if (animation == "fade" && sprite.CurrentAnimationFrame == 4 && fragile) RemoveSelf();
            };
        }

        public override void Update()
        {
            base.Update();
            BouncingCloudMotion.Update(new MotionState(this));
        }

        public override void Render(ISourceRenderContext context)
        {
            var value = scale;
            value = SourceVector2Math.Multiply(value, 1f + .1f * wiggler.Value);
            sprite.Scale = value;
            base.Render(context);
        }

        private readonly struct MotionState : IBouncingCloudState
        {
            private readonly AnonCloud owner;

            public MotionState(AnonCloud owner)
            {
                this.owner = owner;
            }

            public ISourceEngineContext Engine => owner.engine;
            public SourceJumpThruEntityState Platform => owner;
            public Sprite Sprite => owner.sprite;
            public SoundSourceComponentState Sound => owner.sfx;
            public ParticleTypePresentation ParticleType => owner.particleType;
            public bool Fragile => owner.fragile;
            public bool ShouldFade => owner.Collidable && !owner.HasPlayerRider() && owner.fragile;
            public bool Pink => owner.pink;
            public bool SquashWhileRising => true;
            public ref Vector2 Scale => ref owner.scale;
            public ref float Timer => ref owner.timer;
            public ref float RespawnTimer => ref owner.respawnTimer;
            public ref float Speed => ref owner.speed;
            public ref float StartY => ref owner.startY;
            public ref bool Waiting => ref owner.waiting;
            public ref bool Returning => ref owner.returning;
            public ref bool CanRumble => ref owner.canRumble;

            public void BeginFade(bool respawn)
            {
                owner.Collidable = false;
                owner.sprite.Play("fade");
            }
        }
    }
}
