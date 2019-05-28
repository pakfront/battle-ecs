using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    //[Serializable] public struct Subordinate : IComponentData { public Entity Superior; }    
    [Serializable] public struct Unit : IComponentData { }   

    [Serializable] public struct Team : ISharedComponentData { public int Value; }

    [Serializable] public struct Detached : IComponentData { }

    // Orders
    [Serializable] public struct OrderMoveTo : IComponentData { public float3 Position; }
    [Serializable] public struct OrderAttack : IComponentData { public Entity Target; }
    [Serializable] public struct OrderHold : IComponentData { }
    [Serializable] public struct OrderMarch : IComponentData { public Entity Next; }
    [Serializable] public struct OrderFormation : IComponentData { }

    [Serializable] public struct OrderFormationMoveTo : IComponentData { public float3 Position; }

}
