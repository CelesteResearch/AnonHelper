using System;
using System.Numerics;
using Celeste.Core.Scheduling;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display.Colors;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.AnonHelper.Presentation.Clouds
{
    public sealed class AnonCloudParticles
    {
        public ParticleTypePresentation Cloud { get; }
        public ParticleTypePresentation FragileCloud { get; }

        public AnonCloudParticles(ISourceEngineContext engine)
        {
            Cloud = Create(engine, "2c5fcc");
            FragileCloud = Create(engine, "5e22ae");
        }

        private static ParticleTypePresentation Create(ISourceEngineContext engine, string color) => new()
        {
            Source = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas["particles/cloud"],
            Color = SourceHexColor.HexToColor(color),
            FadeMode = ParticleTypePresentation.FadeModes.None,
            LifeMin = .25f, LifeMax = .3f, Size = .7f, SizeRange = .25f, ScaleOut = true,
            Direction = 4.712389f, DirectionRange = .17453292f, SpeedMin = 10f, SpeedMax = 20f,
            SpeedMultiplier = .01f, Acceleration = new Vector2(0, 90)
        };
    }
}
