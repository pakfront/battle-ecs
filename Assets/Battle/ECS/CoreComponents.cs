using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{

    [Serializable] public struct Subordinate : IComponentData { public Entity Superior; }    
    [Serializable] public struct Unit : IComponentData { }    
    [Serializable] public struct Agent : IComponentData { }
    
    
    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Unit;}
    
    [Serializable] public struct Team : ISharedComponentData { public int Value;}
    // [Serializable] public struct TeamA : ISharedComponentData { }
    // [Serializable] public struct TeamB : ISharedComponentData { }
}
