using System.Numerics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.AnonHelper.Modules;
using Celeste.Mods.AnonHelper.Presentation.Refills;
using Celeste.Simulation;

namespace Celeste.Mods.AnonHelper.Entities.Refills
{
    [RuntimeTypeAlias("Celeste.Mod.Anonhelper.JellyRefill")]
    [CustomEntity("Anonhelper/JellyRefill = Create")]
    public class JellyRefill : AnonDashRefillState
    {
        public bool addDash;
        public bool HasJellyDash;
        public bool jellyDashActive;
        public bool dashStarted;
        public RoomPlayerSession player;
        private readonly AnonHelperModule module;

        public static JellyRefill Create(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new(level.EngineContext, data, offset);

        public JellyRefill(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Bool("oneUse"))
        {
        }

        public JellyRefill(ISourceEngineContext engine, Vector2 position, bool oneUse)
            : base(engine, position, oneUse, "jellyRefill", AnonRefillParticles.For(engine).JellyShatter,
                AnonRefillParticles.For(engine).JellyGlow, AnonRefillParticles.For(engine).JellyRegen)
        {
            module = AnonHelperModule.For(engine);
        }

        protected override bool HasDash
        {
            get => module.Session.HasJellyDash;
            set => module.Session.HasJellyDash = value;
        }

        protected override void SetCurrentPlayer(RoomPlayerSession value) => player = value;
    }
}
