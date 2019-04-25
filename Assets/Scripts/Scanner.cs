// This script draws a debug line around mesh triangles
// as you move the mouse over them.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

//TODO: separate data from system
//TODO: use fast ECS NativeCollections
//TODO: check if possible jobified process

public class Scanner : MonoBehaviour
{
    public int numberOfObjects = 4;
    public float radius = 0.1f;
    
    private Camera cam;
    private GameObject coords;
    private HashSet<int> triangleIndexes;

    void Start()
    {
        cam = GetComponent<Camera>();
        triangleIndexes = new HashSet<int>();
    }

    void Update()
    {
        RaycastHit centralHit;
        // ray drone camera to direction of cameras forward
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out centralHit))
            return;
        // Ray rayForward = new Ray(transform.position,centralHit.point);

        LinkedList<RaycastHit> allHits = GetPeriferalHits(centralHit);
        ScanMesh(allHits);
        //DrawRays(centralHit, pointsInCircle);
    }

    private LinkedList<RaycastHit> GetPeriferalHits(RaycastHit centralHit)
    {
        Vector3 hitVector = centralHit.point - transform.position;
        
        //Quaternion q = Quaternion.AngleAxis(90.0f, hitVector);
        //Vector3 vertical =  q * hitVector;
        Quaternion rotor = Quaternion.FromToRotation(Vector3.forward, hitVector);

        LinkedList<RaycastHit> allHits = new LinkedList<RaycastHit>();
        allHits.AddLast(centralHit);
        
        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            Vector3 pos = rotor * new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0) * radius;
            Vector3 rayDirection = centralHit.point + pos - transform.position;
            
            //ray drone camera to direction of cameras forward
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, rayDirection, out hit)) continue;
            allHits.AddLast(hit);
        }
        
        return allHits;
    }
   
    private void ScanMesh(LinkedList<RaycastHit> allHits)
    {
        // Debug.Log(allHits.Count);
        foreach (var hit in allHits)
        {
            if (triangleIndexes.Contains(hit.triangleIndex)) continue;
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null) continue;
            triangleIndexes.Add(hit.triangleIndex);
            Mesh mesh = meshCollider.sharedMesh;
            Transform hitTransform = hit.collider.transform;
        
            Vector3 p0 = hitTransform.TransformPoint(
                mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 0]]);
            Vector3 p1 = hitTransform.TransformPoint(
                mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 1]]);
            Vector3 p2 = hitTransform.TransformPoint(
                mesh.vertices[mesh.triangles[hit.triangleIndex * 3 + 2]]);

            Debug.DrawLine(p0, p1,  Color.magenta, 10f);
            Debug.DrawLine(p1, p2,  Color.magenta, 10f);
            Debug.DrawLine(p2, p0,  Color.magenta, 10f);
            Debug.DrawRay(transform.position, hit.point-transform.position, Color.green,0.3f);
        }        
    }
    
    private void DrawRays(RaycastHit hit, Vector3[] pointsInCircle)
    {
        Debug.DrawLine(transform.position, hit.point,Color.green,0.3f);

        for (int i = 0; i < numberOfObjects; i++)
        {
            Debug.DrawLine(
                pointsInCircle[i],
                pointsInCircle[(int) Mathf.Repeat(i+1, numberOfObjects)],
                Color.green,0.2f
            );
            Debug.DrawLine(
                transform.position,
                pointsInCircle[i],
                Color.green,0.2f
            );
        }
    }
    
}
