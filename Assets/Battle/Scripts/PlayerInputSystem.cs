// using UnityEngine;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;

// namespace Dux.Battle
// {
//     public class PlayerInputSystem : ComponentSystem
//     {
 

//         // A variable to manipulate movement speed.
//         private float speed = 5;

//         protected override void OnUpdate()
//         {
//            Entities.ForEach((ref Move rotationSpeed) =>
//             {
//                 var deltaTime = Time.deltaTime;
//                 rotation.Value = math.mul(math.normalize(rotation.Value),
//                     quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
//             });
//         }
//     }
// }