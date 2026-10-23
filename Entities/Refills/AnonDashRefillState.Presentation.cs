using System.Runtime.CompilerServices;
using Celeste.Game.Gameplay.Collectibles.Refills;

namespace Celeste.Mods.AnonHelper.Entities.Refills
{
    public abstract partial class AnonDashRefillState
    {
        private readonly IRefillPresentationBinding presentation;

        private readonly struct PresentationPhases(AnonDashRefillState owner) : IRefillPresentationPhases
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UpdateTimerAndGlow<TContext>(in TContext context)
                where TContext : struct, IRefillPresentationContext
                => RefillPresentation.UpdateTimerAndGlow(owner, in context, owner.level, ref owner.respawnTimer,
                    owner.respawnAction, owner.glow);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UpdateY() => RefillPresentation.UpdateY(owner.sprite, owner.flash, owner.bloom, owner.sine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UpdateLightAndFlash<TContext>(in TContext context)
                where TContext : struct, IRefillPresentationContext
                => RefillPresentation.UpdateLighting(owner, in context, owner.sprite, owner.flash, owner.light,
                    owner.bloom);
        }
    }
}
