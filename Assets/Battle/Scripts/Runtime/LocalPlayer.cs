using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitAgent
{
    public class LocalPlayer : MonoBehaviour
    {
        static LocalPlayer instance;
        public static ETeam Team => (instance == null ? 0 : instance.team);
        [SerializeField] private ETeam team = 0;


        void Awake()
        {
            instance = this;
        }

    }
}
