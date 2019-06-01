using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UnitAgent
{
    [RequiresEntityConversion]
    public class FormationProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new FormationLeader {} );
        }
    }
}