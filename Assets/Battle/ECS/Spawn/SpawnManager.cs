using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace UnitAgent
{
    public class SpawnManager : MonoBehaviour
    {
        void Start()
        {
            // SpawnUnits();
            Spawn.SpawnUnits(World.Active.EntityManager);
        }
    }
}
