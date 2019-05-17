using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct PlayerSelected : IComponentData {  }

    [Serializable] public struct PlayerOwned : ISharedComponentData {  }
    // [Serializable] public struct PlayerAlly : ISharedComponentData {  }
    // [Serializable] public struct PlayerEnemy : ISharedComponentData {  }
}
