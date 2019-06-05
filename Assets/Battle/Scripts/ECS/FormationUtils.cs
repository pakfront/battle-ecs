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

    public static class FormationUtils
    {
        public static readonly int FormationCount = 4;
        public static readonly int AgentOffsetsPerFormation = 200;
        public static readonly int AgentFormationOffsetsLength = FormationCount * AgentOffsetsPerFormation;
        public static readonly float AgentColumnWidth = 1.6f, AgentRowHeight = 2f;

        public static readonly float UnitColumnSeperation = 20, UnitRowSeperation = 6;
        public static readonly int UnitOffsetsPerFormation = 12;
        public static readonly int UnitFormationOffsetsLength = FormationCount * UnitOffsetsPerFormation;


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
            row = -1 * i / ncols;
            col = (i % ncols);

            col = (col + 1) / 2;
            col *= i % 2 == 0 ? 1 : -1;
        }

        public static float3[] CalcUnitFormations()
        {
            float3[] formationOffsets = new float3[UnitFormationOffsetsLength];
            float3 agentSpacing = new float3(AgentColumnWidth, 0, AgentRowHeight);

            // these could be read from disk and there could be a lot more than these few variations
            int f = (int)EFormation.Mob;
            for (int i = 0; i < UnitOffsetsPerFormation; i++)
            {
                formationOffsets[f * UnitOffsetsPerFormation + i] = new float3(i , 0, i ) * agentSpacing;
            }

            f = (int)EFormation.Line;
            for (int i = 0; i < UnitOffsetsPerFormation; i++)
            {
                FormationUtils.DistributeAcrossColumns(5, i, out int row, out int col);
                formationOffsets[f * UnitOffsetsPerFormation + i] = new float3(col * 40, 0, row*2) * agentSpacing;
            }

            f = (int)EFormation.Column;
            for (int i = 0; i < UnitOffsetsPerFormation; i++)
            {
                FormationUtils.DistributeAcrossColumns(1, i, out int row, out int col);
                formationOffsets[f * UnitOffsetsPerFormation + i] = new float3(col * 5, 0, row*2) * agentSpacing;
            }

            f = (int)EFormation.Reserve;
            for (int i = 0; i < UnitOffsetsPerFormation; i++)
            {
                FormationUtils.DistributeAcrossColumns(3, i, out int row, out int col);
                formationOffsets[f * UnitOffsetsPerFormation + i] = new float3(col, 0, row) * agentSpacing;
            }

            return formationOffsets;
        }

        public static float3[] CalcAgentFormations()
        {
            float3[] formationOffsets = new float3[AgentFormationOffsetsLength];
            float3 agentSpacing = new float3(AgentColumnWidth, 0, AgentRowHeight);

            // these could be read from disk and there could be a lot more than these few variations
            int f = (int)EFormation.Mob;
            for (int i = 0; i < AgentOffsetsPerFormation; i++)
            {
                formationOffsets[f * AgentOffsetsPerFormation + i] = new float3(i, 0, i) * agentSpacing;
            }

            f = (int)EFormation.Line;
            for (int i = 0; i < AgentOffsetsPerFormation; i++)
            {
                FormationUtils.DistributeAcrossColumns(40, i, out int row, out int col);
                formationOffsets[f * AgentOffsetsPerFormation + i] = new float3(col, 0, row) * agentSpacing;
            }

            f = (int)EFormation.Column;
            for (int i = 0; i < AgentOffsetsPerFormation; i++)
            {
                FormationUtils.DistributeAcrossColumns(4, i, out int row, out int col);
                formationOffsets[f * AgentOffsetsPerFormation + i] = new float3(col, 0, row) * agentSpacing;
            }

            f = (int)EFormation.Reserve;
            for (int i = 0; i < AgentOffsetsPerFormation; i++)
            {
                FormationUtils.DistributeAcrossColumns(20, i, out int row, out int col);
                formationOffsets[f * AgentOffsetsPerFormation + i] = new float3(col, 0, row) * agentSpacing;
            }

            return formationOffsets;
        }
    }
}