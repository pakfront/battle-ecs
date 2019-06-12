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
    public enum EFormation : int
    {
        None = -1,

        Line = 0,
        Column = 1,
        Reserve = 2,
        Mob = 3
    }

    public static class Formation
    {
        public static readonly int FormationCount = 4;
        public static readonly int MaxAgentsPerFormation = 80;
        public static readonly int AgentFormationOffsetsLength = FormationCount * MaxAgentsPerFormation;
        public static readonly float AgentColumnWidth = 1.6f, AgentRowHeight = 2f;

        public static readonly float UnitColumnSeperation = 20, UnitRowSeperation = 6;
        public static readonly int MaxUnitsPerFormation = 10;
        public static readonly float MaxUnitWidth = AgentColumnWidth * MaxAgentsPerFormation;
        public static readonly int UnitFormationOffsetsLength = FormationCount * MaxUnitsPerFormation;

        /// <summary>
        /// return the positive or negative integer offsets from position 0
        /// </summary>
        /// <remarks>
        /// make 0 the center at 0,0 with odds on the left(negative column)
        /// and evens on the right (positive col)
        // first row is 0, second is -1, etc.
        /// 03 01 00 02 04
        /// 09 07 05 06 08
        ///    11 10 12
        /// </remarks>
        public static void DistributeAcrossColumns(int ncols, int i, out int row, out int col)
        {
            row = 1 * i / ncols;
            col = (i % ncols);

            col = (col + 1) / 2;
            col *= i % 2 == 0 ? 1 : -1;
        }

        public static void CalcUnitFormations(out float3[] formationOffsets, out int[] formationTypes)
        {
            formationOffsets = new float3[UnitFormationOffsetsLength];
            formationTypes = new int[UnitFormationOffsetsLength];
            float3 agentSpacing = new float3(AgentColumnWidth, 0, AgentRowHeight);
            float3 originOffset = new float3(0, 0, -AgentRowHeight);
            float3 lineOffset = new float3(0, 0, 0);
            // float3 lineOffset = new float3( AgentColumnWidth*AgentOffsetsPerFormation/2f ,0,0);

            // these could be read from disk and there could be a lot more than these few variations
            int f = (int)EFormation.Mob;
            for (int i = 0; i < MaxUnitsPerFormation; i++)
            {
                formationOffsets[f * MaxUnitsPerFormation + i] = new float3(i, 0, -i) * agentSpacing + originOffset;
                formationTypes[f * MaxUnitsPerFormation + i] = (int)EFormation.Line;
            }

            f = (int)EFormation.Line;
            formationOffsets[f * MaxUnitsPerFormation] = new float3(0, 0, -8) * agentSpacing + originOffset;
            for (int i = 1; i < MaxUnitsPerFormation; i++)
            {
                Formation.DistributeAcrossColumns(5, i - 1, out int row, out int col);
                formationOffsets[f * MaxUnitsPerFormation + i] = new float3(col * (MaxAgentsPerFormation + 4) / 2f, 0, -row * 2) * agentSpacing + originOffset + lineOffset;
                formationTypes[f * MaxUnitsPerFormation + i] = (int)EFormation.Line;
            }

            f = (int)EFormation.Column;
            formationOffsets[f * MaxUnitsPerFormation] = new float3(0, 0, +2) * agentSpacing + originOffset;
            for (int i = 1; i < MaxUnitsPerFormation; i++)
            {
                Formation.DistributeAcrossColumns(1, i - 1, out int row, out int col);
                formationOffsets[f * MaxUnitsPerFormation + i] = new float3(col * 5, 0, -row * 2) * agentSpacing + originOffset;
                formationTypes[f * MaxUnitsPerFormation + i] = (int)EFormation.Line;
            }

            f = (int)EFormation.Reserve;
            formationOffsets[f * MaxUnitsPerFormation] = new float3(0, 0, +2) * agentSpacing + originOffset;
            for (int i = 1; i < MaxUnitsPerFormation; i++)
            {
                Formation.DistributeAcrossColumns(12, i - 1, out int row, out int col);
                formationOffsets[f * MaxUnitsPerFormation + i] = new float3(col * 10, 0, -row) * agentSpacing + originOffset + lineOffset;
                formationTypes[f * MaxUnitsPerFormation + i] = (int)EFormation.Column;
            }
        }

        public static float3[] CalcAgentFormations()
        {
            float3[] formationOffsets = new float3[AgentFormationOffsetsLength];
            float3 agentSpacing = new float3(AgentColumnWidth, 0, AgentRowHeight);
            float3 originOffset = new float3(AgentColumnWidth, 0, 0) / 2f;

            // these could be read from disk and there could be a lot more than these few variations
            int f = (int)EFormation.Mob;
            for (int i = 0; i < MaxAgentsPerFormation; i++)
            {
                formationOffsets[f * MaxAgentsPerFormation + i] = new float3(i, 0, -i) * agentSpacing + originOffset;
            }

            f = (int)EFormation.Line;
            for (int i = 0; i < MaxAgentsPerFormation; i++)
            {
                Formation.DistributeAcrossColumns(40, i, out int row, out int col);
                formationOffsets[f * MaxAgentsPerFormation + i] = new float3(col, 0, -row) * agentSpacing + originOffset;
            }

            f = (int)EFormation.Column;
            for (int i = 0; i < MaxAgentsPerFormation; i++)
            {
                Formation.DistributeAcrossColumns(2, i, out int row, out int col);
                formationOffsets[f * MaxAgentsPerFormation + i] = new float3(col, 0, -row) * agentSpacing + originOffset;
            }

            f = (int)EFormation.Reserve;
            for (int i = 0; i < MaxAgentsPerFormation; i++)
            {
                Formation.DistributeAcrossColumns(20, i, out int row, out int col);
                formationOffsets[f * MaxAgentsPerFormation + i] = new float3(col, 0, -row) * agentSpacing + originOffset;
            }

            return formationOffsets;
        }

        public static int CalcUnitFormationStartIndex(int formation, int formationTable)
        {
            return formationTable * Formation.FormationCount * Formation.MaxAgentsPerFormation + formation * Formation.MaxUnitsPerFormation;
        }
    }
}