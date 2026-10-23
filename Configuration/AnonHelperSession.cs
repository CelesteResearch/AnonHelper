using Celeste.Everest.Modules.Persistence;

namespace Celeste.Mods.AnonHelper.Configuration
{
    public sealed class AnonHelperSession : EverestModuleSession
    {
        public bool HasCloudDash { get; set; }
        public bool HasJellyDash { get; set; }
    }
}
