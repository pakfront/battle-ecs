using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
// using Unity.Physics;

namespace UnitAgent
{
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerPointerSystem))]
    public class PlayerSelectableUpdateSystem : JobComponentSystem
    {
        public struct PlayerSelectableSyncTranslationJob : IJobForEach<PlayerSelectable, Translation>
        {
            //Keep our box collider in sync with the position of the player
            public void Execute(ref PlayerSelectable aabb, [ChangedFilter] ref Translation pos)
            {
                aabb.max = pos.Value + aabb.center + aabb.halfwidth;
                aabb.min = pos.Value + aabb.center - aabb.halfwidth;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new PlayerSelectableSyncTranslationJob();
            return job.Schedule(this, inputDeps);
        }
    }
}
