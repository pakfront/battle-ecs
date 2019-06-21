using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnitAgent
{
    // [DisableAutoCreation]
    //[UpdateAfter(typeof(MoveToGoal))]
    [UpdateInGroup(typeof(CombatSystemGroup))]
    [UpdateAfter(typeof(UnitFindOpponentSystem))]
    public class UnitRangedAttackSystem : JobComponentSystem
    {
        EntityQuery opponentGroup;

        protected override void OnCreate()
        {
            opponentGroup = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(Opponent) }
                }
            );
        }

        [RequireComponentTag(typeof(Ranged))]
        [BurstCompile]
        struct SumDamageJob : IJobForEachWithEntity<Opponent>
        {
            public float DeltaTime;
            public NativeMultiHashMap<Entity, float>.Concurrent Damage;
            public void Execute(Entity entity, int index, [ReadOnly] ref Opponent opponent)
            {
                Damage.Add(opponent.Entity, DeltaTime);
            }
        }

        [BurstCompile]
        struct ApplyDamageJob : IJobForEachWithEntity<AgentCount>
        {
            [ReadOnly] public NativeMultiHashMap<Entity, float> Damage;
            public void Execute(Entity entity, int index, [ReadOnly] ref AgentCount agentCount)
            {
                bool found = Damage.TryGetFirstValue(entity, out float item, out NativeMultiHashMapIterator<Entity> it);
                while (found)
                {
                    agentCount.Value -= item;
                    found =Damage.TryGetNextValue(out item, ref it);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var opponentCount = opponentGroup.CalculateLength();
            NativeMultiHashMap<Entity, float> damage = new NativeMultiHashMap<Entity, float>(opponentCount, Allocator.TempJob);
            // look into shedule for hashmap
            var outputDeps = inputDeps;
            outputDeps = new SumDamageJob
            {
                DeltaTime = Time.deltaTime,
                Damage = damage.ToConcurrent()
            }.Schedule(this, outputDeps);

            outputDeps = new ApplyDamageJob
            {
                Damage = damage
            }.Schedule(this, outputDeps);

            outputDeps.Complete();
            damage.Dispose();
            return outputDeps;
        }
    }
}