using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;


namespace UnitAgent
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(UnitOrderPreSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UintAddOrdersToChildrenSystem : JobComponentSystem
    {

        private EntityQuery allRootsGroup, fmtRootsGroup;

        protected override void OnCreate()
        {
            var rootsQueryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    // ComponentType.ReadOnly<UnitGroupLeader>(),
                    ComponentType.ReadOnly<UnitGroupChildren>(),
                    ComponentType.ReadOnly<Goal>()
                },
                None = new ComponentType[]
                {
                    typeof(UnitGroupMember)
                },
            };


            allRootsGroup =  GetEntityQuery(rootsQueryDesc);

            var fmtDesc = rootsQueryDesc;
            fmtDesc.All = fmtDesc.All.Concat(new ComponentType[] { ComponentType.ReadOnly<OrderUnitGroupMoveToTag>() }).ToArray();
            fmtRootsGroup = GetEntityQuery(fmtDesc);

        }

        [BurstCompile]
        struct AddFormationMoveTo : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<Goal> GoalType;
            [ReadOnly] public ArchetypeChunkBufferType<UnitGroupChildren> ChildType;
            [ReadOnly] public BufferFromEntity<UnitGroupChildren> ChildFromEntity;

            [NativeDisableContainerSafetyRestriction]
            public ComponentDataFromEntity<Goal> LocalToWorldFromEntity;

            void ChildLocalToWorld(float4x4 parentLocalToWorld, Entity entity)
            {
                // var localToWorldMatrix = math.mul(parentLocalToWorld, localToParent.Value);
                // LocalToWorldFromEntity[entity] = new LocalToWorld { Value = localToWorldMatrix };

                // if (ChildFromEntity.Exists(entity))
                // {
                //     var children = ChildFromEntity[entity];
                //     for (int i = 0; i < children.Length; i++)
                //     {
                //         ChildLocalToWorld(localToWorldMatrix, children[i].Value);
                //     }
                // }
            }

            public void Execute(ArchetypeChunk chunk, int index, int entityOffset)
            {
                var chunkGoal = chunk.GetNativeArray(GoalType);
                var chunkChildren = chunk.GetBufferAccessor(ChildType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var goal = chunkGoal[i].Value;
                    var children = chunkChildren[i];
                    for (int j = 0; j < children.Length; j++)
                    {
                        ChildLocalToWorld(goal, children[j].Value);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var outputDeps = inputDependencies;

            var goalType = GetArchetypeChunkComponentType<Goal>(true);
            var childType = GetArchetypeChunkBufferType<UnitGroupChildren>(true);
            var childFromEntity = GetBufferFromEntity<UnitGroupChildren>(true);
            var goalFromEntity = GetComponentDataFromEntity<Goal>();

            var updateHierarchyJob = new AddFormationMoveTo
            {
                GoalType = goalType,
                ChildType = childType,
                ChildFromEntity = childFromEntity,
                LocalToWorldFromEntity = goalFromEntity
            };
            outputDeps = updateHierarchyJob.Schedule(fmtRootsGroup, outputDeps);
            return outputDeps;

        }


    }
}