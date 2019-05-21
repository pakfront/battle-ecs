// // https://forum.unity.com/threads/best-way-to-handle-player-input.636649/#post-4264951

// using Unity.Entities;
// using Unity.Mathematics;
// using UnityEngine.Experimental.Input;
// using UnityEngine.Experimental.PlayerLoop;
// using


//  namespace UnitAgent
// {

//     /// <summary>
//     /// The input system.
//     /// </summary>
//     [UpdateBefore(typeof(Update))]
//     public class InputSystem : ComponentSystem
//     {
//         private Plane groundplane = new Plane(Vector3.up, 0);

//         protected override OnCreateManager()
//         {
//             EntityManager.CreateEntity(typeof(InputValues));
//             SetSingleton(new InputValues());
//         }
//         /// <inheritdoc/>
//         protected override void OnUpdate()
//         {
//             var keyboard = Keyboard.current;
//             var mouse = Mouse.current;
//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


//             // var x = (keyboard.aKey.isPressed ? -1 : 0) + (keyboard.dKey.isPressed ? 1 : 0);
//             // var y = (keyboard.sKey.isPressed ? -1 : 0) + (keyboard.wKey.isPressed ? 1 : 0);

//             this.SetSingleton(new InputValues
//             {
//                 Pointer = mouse.delta.ReadValue(),

//                 PrimaryDown = mouse.leftButton.wasPressedThisFrame,
//                 SecondaryDown = mouse.rightButton.wasPressedThisFrame,
//                 Attack = keyboard.yKey.isPressed;
//                 Move = keyboard.mKey.isPressed;
//             });
//         }
//     }

//     struct InputValues : IComponentData
//     {
//         public float Pointer;
//         public float3 WorldPosition;
//         public int PrimaryDown, SecondaryDown;
//         public int Attack, Move;
//     }
// }
