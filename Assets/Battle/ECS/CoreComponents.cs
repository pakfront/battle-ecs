using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct Unit : IComponentData { public Entity Parent; }    
    [Serializable] public struct Agent : IComponentData { public Entity Unit; }
    
    
    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Unit;}
    
    [Serializable] public struct Team : ISharedComponentData { public int Value;}
    // [Serializable] public struct TeamA : ISharedComponentData { }
    // [Serializable] public struct TeamB : ISharedComponentData { }
}
