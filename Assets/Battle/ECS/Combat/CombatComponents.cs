﻿using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    // [Serializable] public struct Combat : IComponentData { public Entity Opponent; }
    [Serializable] public struct Opponent : IComponentData { 
        public float3 Position;
        public float DistanceSq;
        }


}
