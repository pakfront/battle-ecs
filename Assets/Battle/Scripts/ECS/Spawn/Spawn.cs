using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace UnitAgent
{
    public static class Spawn
    {
        public struct UnitSettings
        {
            public enum EOrder { None, InFormation, HoldPosition, MoveToPosition, FollowUnit, PursueUnit }

            [Header("Team")]
            public int team;

            [Header("Unit")]
            public UnitProxy unitPrefab;

            public UnitSpawn superior;
            public float unitTranslationUnitsPerSecond;
            public EOrder initialOrders;

            [Header("Agent")]
            public AgentProxy agentPrefab;
            public float agentSpacing;
            public int columns, rows;
            public float agentTranslationUnitsPerSecond;
            // public UnitSettings()
            // {
            //     unitTranslationUnitsPerSecond = 1;
            //     agentSpacing = 1.3F;
            //     columns = 6;
            //     rows = 2;
            //     agentTranslationUnitsPerSecond = .5f;

            // }
        }

    }
}
