using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
// using Unity.Physics;

namespace UnitAgent
{
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(PlayerMouseOverSystem))]
    public class AABBUpdateSystem : JobComponentSystem
    {
        public struct AABBMovmentJob : IJobForEach<AABB, Translation>
        {
            //Keep our box collider in sync with the position of the player
            public void Execute(ref AABB aabb, [ChangedFilter] ref Translation pos)
            {
                aabb.max = pos.Value + 0.5f;
                aabb.min = pos.Value - 0.5f;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new AABBMovmentJob();
            return job.Schedule(this, inputDeps);
        }
    }
}
