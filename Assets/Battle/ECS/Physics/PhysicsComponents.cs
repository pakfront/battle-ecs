﻿using Unity.Entities;
using Unity.Mathematics;

namespace UnitAgent
{
    public struct AABB : IComponentData
    {
        public float3 min;
        public float3 max;
    }
}