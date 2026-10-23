using System.Numerics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Everest.Tracking.Attributes;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation.Gameplay.Enemies.Seeker;
using Celeste.Simulation.Gameplay.Platforms.Collision;
using Celeste.Simulation.Gameplay.Platforms.Foundation;

namespace Celeste.Mods.AnonHelper.Entities.Barriers
{
    [CustomEntity("Anonhelper/InvisibleSeekerBarrier")]
    [EverestTrackedAs(typeof(SeekerBarrierState))]
    [RuntimeTypeAlias("Celeste.Mod.Anonhelper.InvisibleSeekerBarrier")]
    public sealed class InvisibleSeekerBarrier : SeekerBarrierState
    {
        public HitboxColliderGeometry HideCollider;
        public HitboxColliderGeometry RegCollider;

        public InvisibleSeekerBarrier(Vector2 position, float width, float height, ISourceSolidWorld world)
            : base(position, width, height, world)
        {
            Visible = false;
            RegCollider = new HitboxColliderGeometry(width, height);
            HideCollider = new HitboxColliderGeometry(0, height);
            Collider = RegCollider;
        }

        public InvisibleSeekerBarrier(EntityData data, Vector2 offset, ISourceSolidWorld world)
            : this(data.Position + offset, data.Width, data.Height, world)
        {
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new InvisibleSeekerBarrier(data, offset,
            ((ISourcePlatformCollisionScene)level).SolidWorldFactory.Create(data.Position + offset, false));

        public override void Added(object scene)
        {
            base.Added(scene);
            ((ISourceScene)scene).SourceScene.Entities
                .GetTrackedEntity<SeekerBarrierRendererState>("Celeste.SeekerBarrierRenderer").Untrack(this);
        }
    }
}
