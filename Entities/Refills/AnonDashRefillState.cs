using System;
using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Gameplay.Collectibles.Refills;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Display;
using Celeste.Scene.Components.Oscillation;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Collision.Components;
using Celeste.Simulation.Presentation.Effects.Mirrors;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.PostProcessing.Bloom;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.AnonHelper.Entities.Refills
{
    public abstract partial class AnonDashRefillState : SceneEntityState
    {
        protected readonly IOriginalRefillProcess engine;
        private readonly Sprite sprite;
        private readonly Sprite flash;
        private readonly SpriteImage outline;
        private readonly WigglerComponent wiggler;
        private readonly BloomPointState bloom;
        private readonly VertexLightState light;
        private readonly bool oneUse;
        private readonly SineWaveState sine;
        private IOriginalRefillScene level;
        private float respawnTimer;
        private readonly Action respawnAction;
        private readonly ParticleTypePresentation shatter, glow, regen;
        protected abstract bool HasDash { get; set; }
        protected abstract void SetCurrentPlayer(RoomPlayerSession player);

        protected AnonDashRefillState(ISourceEngineContext engine, Vector2 position, bool oneUse, string spriteId,
            ParticleTypePresentation shatter, ParticleTypePresentation glow, ParticleTypePresentation regen)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            this.engine = (IOriginalRefillProcess)engine;
            this.oneUse = oneUse;
            this.shatter = shatter;
            this.glow = glow;
            this.regen = regen;
            respawnAction = Respawn;
            Collider = new Celeste.Physics.Collision.Shapes.HitboxColliderGeometry(16f, 16f, -8f, -8f);
            Add(new PlayerColliderComponentState(OnPlayer));
            var graphics = this.engine.GameGraphics;
            Add(sprite = graphics.SpriteFactory.Create(spriteId));
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = graphics.SpriteFactory.Create(spriteId));
            flash.OnFinish = _ => flash.Visible = false;
            flash.CenterOrigin();
            string spritePath = sprite.GetFrame("idle", 0).ToString();
            spritePath = spritePath.Remove(spritePath.LastIndexOf('/') + 1) + "outline";
            if (!graphics.GameAtlas.Has(spritePath)) spritePath = $"objects/AnonHelper/{spriteId}/outline";
            Add(outline = new SpriteImage(graphics.GameAtlas[spritePath]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(wiggler = ((ISourceWigglerPoolScope)engine).WigglerPool.Create(1f, 4f,
                v => sprite.Scale = flash.Scale = Vector2.One * (1f + v * .2f)));
            Add(new MirrorReflectionState());
            Add(bloom = new BloomPointState(.8f, 16f));
            Add(light = new VertexLightState(engine, Vector2.Zero, Celeste.Core.Display.PackedColor.White, 1f, 16, 48));
            Add(sine = new SineWaveState(engine, .6f, 0f));
            sine.Randomize();
            RefillPresentation.UpdateY(sprite, flash, bloom, sine);
            presentation = RefillPresentation.BindPresentation(this.engine, new PresentationPhases(this));
            Depth = -100;
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            level = (IOriginalRefillScene)Scene;
        }

        public override void Update()
        {
            SetCurrentPlayer(((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>());
            base.Update();
            presentation.Update();
        }

        private void Respawn() => RefillPresentation.Respawn(this, engine, level, sprite, outline, wiggler, regen,
            "event:/game/general/diamond_return");

        public override void Render(ISourceRenderContext context)
        {
            RefillPresentation.RenderOutline(context, sprite);
            base.Render(context);
        }

        private void OnPlayer(RoomPlayerSession player)
        {
            if (!HasDash)
            {
                player.UseRefill(false);
                HasDash = true;
                engine.SpatialAudio.Play("event:/game/general/diamond_touch", Position);
                engine.RumbleSession.Rumble(1, 1);
                Collidable = false;
                Add(new SimulationCoroutine(engine, RefillRoutine(player)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(RoomPlayerSession player)
        {
            RefillPresentation.BeginRoutine(engine);
            yield return null;
            level.Shake();
            RefillPresentation.HideForRoutine(this, sprite, flash, outline, oneUse);
            yield return .05f;
            RefillPresentation.FinishRoutine(this, engine, level, shatter, player);
            if (oneUse) RemoveSelf();
        }
    }
}
