// using System;
// using Unity.Entities;
// using Unity.Mathematics;
// class ImpulseSystem : ComponentSystem
// {
//     EntityQuery m_Group;

//     protected override void OnCreate(int capacity)
//     {
//         m_Group = GetEntityQuery(typeof(Position), typeof(Displacement), typeof(SharedGrouping));
//     }

//     protected override void OnUpdate()
//     {
//         // Only iterate over entities that have the SharedGrouping data set to 1
//         m_Group.SetFilter(new SharedGrouping { Group = 1 });

//         var positions = m_Group.ToComponentDataArray<Position>(Allocator.Temp);
//         var displacememnts = m_Group.ToComponentDataArray<Displacement>(Allocator.Temp);

//         for (int i = 0; i != positions.Length; i++)
//             positions[i].Value = positions[i].Value + displacememnts[i].Value;
//     }
// }
