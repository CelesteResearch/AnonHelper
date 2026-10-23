using System.Numerics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Entities.Interaction.Boosters;

namespace Celeste.Mods.AnonHelper.Entities.Boosters
{
    [CustomEntity("Anonhelper/OneUseBooster")]
    [RuntimeTypeAlias("Celeste.Mod.Anonhelper.OneUseBooster")]
    public sealed class OneUseBooster : OriginalBoosterState
    {
        public ISourceEngineContext EngineContext { get; }

        public OneUseBooster(ISourceEngineContext engine, Vector2 position, bool red) : base(engine, position, red)
        {
            EngineContext = engine;
        }

        public OneUseBooster(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Bool("red"))
        {
        }
    }
}
