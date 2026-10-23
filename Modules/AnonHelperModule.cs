using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Registry;
using Celeste.Everest.Modules.Runtime;
using Celeste.Formats.Graphics.Sprites;
using Celeste.Graphics.Content;
using Celeste.Mods.AnonHelper.Configuration;
using Celeste.Mods.AnonHelper.Runtime;
using Celeste.Mods.AnonHelper.Presentation.Clouds;
using Celeste.Mods.AnonHelper.Presentation.Refills;

namespace Celeste.Mods.AnonHelper.Modules
{
    public sealed class AnonHelperModule : SourceEverestModule
    {
        private static readonly ConditionalWeakTable<ISourceEngineContext, AnonHelperModule> modules = new();
        private CustomDashPlayerHooks hooks;
        private AnonEntityHooks entityHooks;

        public AnonHelperModule(ISourceEngineContext engine) : base(engine)
        {
        }

        public ISourceEngineContext Engine => SourceEngine;
        public AnonHelperSession Session => (AnonHelperSession)_Session;
        public SpriteBank Sprites { get; private set; }
        public AnonCloudParticles CloudParticles { get; private set; }
        public override Type SessionType => typeof(AnonHelperSession);

        public static AnonHelperModule For(ISourceEngineContext engine) => modules.TryGetValue(engine, out var module)
            ? module
            : throw new InvalidOperationException("AnonHelper is not loaded for this engine.");

        public override void Load()
        {
            modules.Add(SourceEngine, this);
            try
            {
                hooks = new CustomDashPlayerHooks(this);
                entityHooks = new AnonEntityHooks(SourceEngine);
            }
            catch
            {
                hooks?.Dispose();
                hooks = null;
                modules.Remove(SourceEngine);
                throw;
            }
        }

        public override void Unload()
        {
            hooks?.Dispose();
            hooks = null;
            entityHooks?.Dispose();
            entityHooks = null;
            AnonRefillParticles.Reset(SourceEngine);
            modules.Remove(SourceEngine);
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            var xml = new XmlDocument();
            using (var stream = ((IEverestContentRegistryEnvironment)SourceEngine).SourceContentRegistry
                   .Get("Graphics/carelessAnonymous/Sprites").Stream) xml.Load(stream);
            Sprites = new SpriteBank(SourceEngine,
                ((ISourceGameGraphicsEnvironment)SourceEngine).GameGraphics.GameAtlas, xml);
            CloudParticles = new AnonCloudParticles(SourceEngine);
            AnonRefillParticles.For(SourceEngine);
        }
    }
}
