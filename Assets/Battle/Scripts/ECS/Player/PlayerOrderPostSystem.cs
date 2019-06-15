using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    [UpdateInGroup(typeof(PlayerSystemGroup))]
    [UpdateAfter(typeof(PlayerOrderSystem))]
    public class PlayerOrderPostSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // currently just place holder for ordering
        }
        
    }
}
