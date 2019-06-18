using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnitAgent
{
    [RequiresEntityConversion]
    public class UnitGroupProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new UnitGroupLeader {} );
            // dstManager.AddComponentData(entity, new Goal {} );
        }
    }
}