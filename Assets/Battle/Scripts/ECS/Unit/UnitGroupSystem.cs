using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;


namespace UnitAgent
{
    // [DisableAutoCreation]
    [UpdateBefore(typeof(UnitOrderPreSystem))]
    [UpdateInGroup(typeof(UnitSystemGroup))]
    public class UnitGroupSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {

        }
    }
}