using System;
using System.Runtime.CompilerServices;
using Celeste.Core.Display;
using Celeste.Core.Scheduling;
using Celeste.Graphics.Content;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.AnonHelper.Presentation.Refills
{
    public sealed class AnonRefillParticles
    {
        private static readonly ConditionalWeakTable<ISourceEngineContext, AnonRefillParticles> Owners = new();

        public static AnonRefillParticles For(ISourceEngineContext engine) =>
            Owners.GetValue(engine, e => new AnonRefillParticles(e));

        public static void Reset(ISourceEngineContext engine) => Owners.Remove(engine);
        public readonly ParticleTypePresentation CloudShatter, CloudGlow, CloudRegen;
        public readonly ParticleTypePresentation JellyShatter, JellyGlow, JellyRegen;

        private AnonRefillParticles(ISourceEngineContext engine)
        {
            var triangle = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas["particles/triangle"];
            CloudShatter = Shatter(triangle, PackedColor.White, PackedColor.White);
            CloudGlow = Glow(PackedColor.White, PackedColor.White);
            CloudRegen = Regen(PackedColor.White, PackedColor.White);
            JellyShatter = Shatter(triangle, PackedColor.FromHex("30a0e6"), PackedColor.FromHex("1e7bde"));
            JellyGlow = Glow(PackedColor.FromHex("30a0e6"), PackedColor.FromHex("1e7bde"));
            JellyRegen = Regen(PackedColor.FromHex("30a0e6"), PackedColor.FromHex("1e7bde"));
        }

        private static ParticleTypePresentation Shatter(Celeste.Graphics.Content.Atlas.AtlasEntry triangle,
            PackedColor color, PackedColor color2) => new()
        {
            Source = triangle, Color = color, Color2 = color2,
            FadeMode = ParticleTypePresentation.FadeModes.Late,
            LifeMin = .25f, LifeMax = .4f, Size = 1f,
            Direction = 4.712389f, DirectionRange = .87266463f,
            SpeedMin = 140f, SpeedMax = 210f, SpeedMultiplier = .005f,
            RotationMode = ParticleTypePresentation.RotationModes.Random,
            SpinMin = (float)Math.PI / 2f, SpinMax = 4.712389f, SpinFlippedChance = true
        };

        private static ParticleTypePresentation Glow(PackedColor color, PackedColor color2) => new()
        {
            LifeMin = .4f, LifeMax = .6f, Size = 1f, SizeRange = 0f,
            DirectionRange = (float)Math.PI * 2f, SpeedMin = 4f, SpeedMax = 8f,
            FadeMode = ParticleTypePresentation.FadeModes.Late,
            Color = color, Color2 = color2, ColorMode = ParticleTypePresentation.ColorModes.Blink
        };

        private static ParticleTypePresentation Regen(PackedColor color, PackedColor color2) => new()
        {
            LifeMin = .4f, LifeMax = .6f, Size = 1f, SizeRange = 0f,
            FadeMode = ParticleTypePresentation.FadeModes.Late,
            Color = color, Color2 = color2, ColorMode = ParticleTypePresentation.ColorModes.Blink,
            SpeedMin = 30f, SpeedMax = 40f, SpeedMultiplier = .2f, DirectionRange = (float)Math.PI * 2f
        };
    }
}
