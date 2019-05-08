using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct Team : IComponentData { public int Value; }

    [Serializable] public struct Unit : IComponentData { }
    [Serializable] public struct Agent : IComponentData { public Entity Unit; }
    
    
    //using SCD triggers chunking per unit
    // [Serializable] public struct UnitMembership : ISharedComponentData { public Entity Unit;}
    
    // [Serializable] public struct TeamMembership : ISharedComponentData { public int Team;}

}
