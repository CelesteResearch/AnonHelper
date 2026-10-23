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
    [RuntimeTypeAlias("Celeste.Mod.Anonhelper.CloudRefill")]
    [CustomEntity("Anonhelper/CloudRefill = Create")]
    public class CloudRefill : AnonDashRefillState
    {
        public bool addDash;
        public bool HasCloudDash;
        public bool cloudDashActive;
        public bool dashStarted;
        public RoomPlayerSession player;
        private readonly AnonHelperModule module;

        public static CloudRefill Create(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new(level.EngineContext, data, offset);

        public CloudRefill(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Bool("oneUse"))
        {
        }

        public CloudRefill(ISourceEngineContext engine, Vector2 position, bool oneUse)
            : base(engine, position, oneUse, "cloudRefill", AnonRefillParticles.For(engine).CloudShatter,
                AnonRefillParticles.For(engine).CloudGlow, AnonRefillParticles.For(engine).CloudRegen)
        {
            module = AnonHelperModule.For(engine);
        }

        protected override bool HasDash
        {
            get => module.Session.HasCloudDash;
            set => module.Session.HasCloudDash = value;
        }

        protected override void SetCurrentPlayer(RoomPlayerSession value) => player = value;
    }
}
