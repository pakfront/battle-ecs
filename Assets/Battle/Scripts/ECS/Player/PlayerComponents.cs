using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    [Serializable] public struct PlayerSelection : IComponentData {  }
    [Serializable] public struct PlayerTarget : IComponentData {  }
    [Serializable] public struct PlayerFollow : IComponentData {  }

    [Serializable] public struct PlayerOwned : ISharedComponentData {  }
    [Serializable] public struct PlayerAlly : ISharedComponentData {  }
    [Serializable] public struct PlayerEnemy : ISharedComponentData {  }
}
