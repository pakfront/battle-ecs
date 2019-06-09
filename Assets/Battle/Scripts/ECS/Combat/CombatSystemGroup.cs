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
    // cribbed from 
    // https://forum.unity.com/threads/how-do-you-get-a-bufferfromentity-or-componentdatafromentity-without-inject.587857/#post-3924478
    [UpdateInGroup(typeof(SimulationSystemGroup ))]
    [UpdateAfter(typeof(MovementSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    
    public class CombatSystemGroup : ComponentSystemGroup
    {
 
    }
}
