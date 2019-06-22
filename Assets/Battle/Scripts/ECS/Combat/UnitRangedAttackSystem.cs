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
    [DisableAutoCreation]
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

        [BurstCompile]
        struct SumDamageJob : IJobForEachWithEntity<Opponent, AgentCount, Ranged>
        {
            public float DeltaTime;
            public NativeMultiHashMap<Entity, float>.Concurrent Damage;
            public void Execute( Entity entity, int index, [ReadOnly] ref Opponent opponent,
            [ReadOnly] ref AgentCount agentCount, [ReadOnly] ref Ranged ranged)
            {
                if (opponent.DistanceSq < ranged.Range * ranged.Range)
                {
                    Damage.Add(opponent.Entity, 0.01f * agentCount.Value * DeltaTime);
                }
            }
        }

        // these would be moved into a general purpose damage system
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
                    found = Damage.TryGetNextValue(out item, ref it);
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