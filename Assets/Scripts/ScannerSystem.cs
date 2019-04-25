using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// TODO: jobify?

namespace Master
{
   public class ScannerSystem : ComponentSystem
   {
      // Global HashSet<int> for scanned hologram?
      private static HashSet<int> triangleIndexes;

      protected override void OnStartRunning()
      {
         triangleIndexes = new HashSet<int>();
      }

      protected override void OnUpdate()
      {
         Entities.ForEach((
            Camera cam,
            ScannerData scannerData,
            Transform transform
         ) =>
         {
            RaycastHit centralHit;
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out centralHit))
               return;
            
            float3 hitPoint = centralHit.point;
            float3 originPosition = transform.position;
            ScanMesh(centralHit, hitPoint, originPosition);

            // Periferal hits
            float3 hitDirection = hitPoint - originPosition;
            quaternion rotor = quaternion.LookRotationSafe(Vector3.forward, hitDirection);

            for (int i = 0; i < scannerData.numberOfObjects; i++)
            {         
               float angle = i * Mathf.PI * 2 / scannerData.numberOfObjects;
               float3 pos = math.mul(rotor, new float3(math.cos(angle),math.sin(angle),0)) * scannerData.radius;
               float3 rayDirection = hitPoint + pos - originPosition;
               
               //ray drone camera to direction of cameras forward
               RaycastHit hit;
               // may want use with job RaycastCommand
               if (!Physics.Raycast(transform.position, rayDirection, out hit)) continue;
               ScanMesh(hit, hit.point, originPosition);
            }
         });
      }
      
      private void ScanMesh(RaycastHit hit, float3 hitPoint, float3 originPosition)
      {
         if (triangleIndexes.Contains(hit.triangleIndex)) return;
         MeshCollider meshCollider = hit.collider as MeshCollider;
         if (meshCollider == null || meshCollider.sharedMesh == null) return;
         triangleIndexes.Add(hit.triangleIndex);
         Mesh mesh = meshCollider.sharedMesh;
         Transform hitTransform = hit.collider.transform;
     
         float3 p0 = hitTransform.TransformPoint(
            mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 0]]);
         float3 p1 = hitTransform.TransformPoint(
            mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 1]]);
         float3 p2 = hitTransform.TransformPoint(
            mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 2]]);

         Debug.DrawLine(p0, p1,  Color.magenta, 10f);
         Debug.DrawLine(p1, p2,  Color.magenta, 10f);
         Debug.DrawLine(p2, p0,  Color.magenta, 10f);
         Debug.DrawRay(originPosition, hitPoint-originPosition, Color.green,0.3f);      
      }

   } 
}


