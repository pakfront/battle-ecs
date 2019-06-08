using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

namespace UnitAgent
{
    // [DisableAutoCreation] 
    [UpdateInGroup(typeof(PlayerSystemGroup))]
    [UpdateBefore(typeof(PlayerPointerPreSystem))]

    public class PlayerInputSystem : ComponentSystem
    {

        protected override void OnCreate()
        {
            EntityManager.CreateEntity(typeof(PlayerPointer));
            SetSingleton(new PlayerPointer{ Click = (uint) EClick.None });
        }

        protected override void OnUpdate()
        {
            var playerPointer = GetSingleton<PlayerPointer>();
            playerPointer = new PlayerPointer
            {
                MousePosition = Input.mousePosition,
                Click = (uint)EClick.None,
                FormationIndex = (int)EFormation.None,
                // copy from previous
                WorldHitPosition = playerPointer.WorldHitPosition,
                CurrentEntity = playerPointer.CurrentEntity,
            };
            // mouse
            if (Input.GetMouseButtonDown(0)) playerPointer.Click |= (uint)EClick.PrimaryPointerButton;
            if (Input.GetMouseButtonDown(1)) playerPointer.Click |= (uint)EClick.SecondaryPointerButton;
            
            // modifiers
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) playerPointer.Click |= (uint)EClick.Ctrl;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) playerPointer.Click |= (uint)EClick.Shift;
            
            // commands
            if (Input.GetKeyDown(KeyCode.Alpha0))  playerPointer.FormationIndex = (int)EFormation.Mob;
            else if (Input.GetKeyDown(KeyCode.Alpha1))  playerPointer.FormationIndex = (int)EFormation.Line;
            else if (Input.GetKeyDown(KeyCode.Alpha2))  playerPointer.FormationIndex = (int)EFormation.Column;
            else if (Input.GetKeyDown(KeyCode.Alpha3))  playerPointer.FormationIndex = (int)EFormation.Reserve;
            

            SetSingleton(playerPointer);

        }
    }
}
