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
        public static void DistributeAcrossColumns(int ncols, int i, out int row, out int col)
        {
            row = i / ncols;
            col = ((i % ncols)) / 2;
            col *= i % 2 == 0 ? 1 : -1;
        }
    }
}