using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Master
    // TODO: Rotations vec & PH
    // TODO: Motion frames
    // TODO: splines
    // optional : 5pCurve
{
    public class MasterEngineSystem : ComponentSystem
    {
        struct Chunks
        {
            public readonly int Length;
            //public EntityArray entities;
            public ComponentArray<Transform> T;
            public ComponentArray<PathMarker> paths;
            public ComponentArray<LineRenderer> lrs;
        }
        [Inject] private Chunks _chunks;

        struct CPsAndVectors
        {
            public CPsAndVectors(Transform[] controlPoints, Transform[] vecPoints) : this()
            {
                this.controlPoints = controlPoints;
                this.vecPoints = vecPoints;
            }
            public Transform[] vecPoints;
            public Transform[] controlPoints;
        }

        private float4[] TransformArr_toPosF4Arr(Transform[] transArr)
        {
            float4[] posArr = new float4[transArr.Length];
            for(int i = 0; i < transArr.Length; i++)
            {
                posArr[i] = new float4(transArr[i].position.x, transArr[i].position.y, transArr[i].position.z, 1f);
            }
            return posArr;
        }

        private CPsAndVectors Extract_CPsAndVec(Transform transform, byte countCP)
        {
            Transform[] controlPoints = new Transform[countCP];
            Transform[] vecPoints = new Transform[2];
            byte indexCP = 0;
            byte indexVec = 0;
            foreach (Transform childTrans in transform)
            {
                if (childTrans.tag == "ControlPoint")
                {
                    controlPoints[indexCP] = childTrans;
                    indexCP++;
                }

                if (childTrans.tag == "VecPoint")
                {
                    vecPoints[indexVec] = childTrans;
                    indexVec++;
                }
            }
            return new CPsAndVectors(controlPoints, vecPoints);
        }

        private quaternion CalcDistanceQuat(float3 cp, float3 vecP)
        {
            return new quaternion(cp.x - vecP.x, cp.y - vecP.y, cp.z - vecP.z, 0f);
        }

        private float4 CalcDistanceFloat4(float3 cp, float3 vecP)
        {
            return new float4(cp.x - vecP.x, cp.y - vecP.y, cp.z - vecP.z, 0f);
        }


        protected override void OnUpdate()
        {
            
            if (!BootStrap.Settings.stopTime)
            {
                // BootStrap.Settings.stopTime = true;
                // EntityManager em = World.Active.GetOrCreateManager<EntityManager>();
                for (int pth = 0; pth < _chunks.Length; pth++) // Paths2
                {
                    Transform pathTransform = _chunks.T[pth];
                    byte curveType = _chunks.paths[pth].curveType;
                    List<float3> pathPoints = null; // = new List<float3>();

                    //SelectAlgorithm(pathTransform, curveType, ref pathPoints);
                    if (curveType == 0)
                    {
                        pathPoints = Calc_PH_Curve(pathTransform);
                        //Debug.Log(pathPoints.Count);
                    }
                    else if (curveType == 1)
                    {
                        pathPoints = Calc_3Pv2Quat_Curve(pathTransform);
                    }
                    else if (curveType == 2)
                    {
                        Calc_5PQuat_Curve();
                    }

                    LineRendererSystem.SetPolygonPoints(_chunks.lrs[pth], pathPoints);
                }
            }
            
        }

        private List<float3> Calc_PH_Curve(Transform pathTransform)
        {
            List<float3> spline = new List<float3>();
            foreach (Transform curveTrans in pathTransform) // Curves
            {
                if (curveTrans.tag == "Trajectory")
                {
                    /// DO ALGORITHMIC STUFF:
                    CPsAndVectors controlData = Extract_CPsAndVec(curveTrans, 2);
                    Debug.Log(controlData);
                    Transform p0T = controlData.controlPoints[0];
                    Transform p1T = controlData.controlPoints[1];

                    LineRendererSystem.SetPolygonPoints(
                        controlData.vecPoints[0].GetComponent<LineRenderer>(),
                        new List<float3>() { controlData.controlPoints[0].position, controlData.vecPoints[0].position }
                        );

                    LineRendererSystem.SetPolygonPoints(
                        controlData.vecPoints[1].GetComponent<LineRenderer>(),
                        new List<float3>() { controlData.controlPoints[1].position, controlData.vecPoints[1].position }
                        );

                    quaternion v0 = CalcDistanceQuat(controlData.controlPoints[0].position, controlData.vecPoints[0].position);
                    quaternion v1 = CalcDistanceQuat(controlData.controlPoints[1].position, controlData.vecPoints[1].position);
                    //quaternion v0 = new quaternion(3f, 4f, 1f, 0f);
                    //quaternion v1 = new quaternion(2f, 2f, 0f, 0f);
                    
                    spline.AddRange(PHodCurve.CalcPHodCurve(p0T, p1T, v0, v1));
                }
            }
            return spline;
        }


        private List<float3> Calc_3Pv2Quat_Curve(Transform pathTransform)
        {
            List<float3> spline = new List<float3>();
            foreach (Transform curveTrans in pathTransform) // Curves
            {
                if (curveTrans.tag == "Trajectory")
                {
                    /// DO ALGORITHMIC STUFF:
                    CPsAndVectors controlData = Extract_CPsAndVec(curveTrans, 3);
                    // Debug.Log(controlData);

                    LineRendererSystem.SetPolygonPoints(
                        controlData.vecPoints[0].GetComponent<LineRenderer>(),
                        new List<float3>() { controlData.controlPoints[0].position, controlData.vecPoints[0].position }
                        );

                    LineRendererSystem.SetPolygonPoints(
                        controlData.vecPoints[1].GetComponent<LineRenderer>(),
                        new List<float3>() { controlData.controlPoints[2].position, controlData.vecPoints[1].position }
                        );

                    float4 v0 = CalcDistanceFloat4(controlData.controlPoints[0].position, controlData.vecPoints[0].position);
                    float4 v1 = CalcDistanceFloat4(controlData.controlPoints[1].position, controlData.vecPoints[1].position);
                    float4[] controlPoints = TransformArr_toPosF4Arr(controlData.controlPoints);
                    FittedMovement motion = QuatVecCurve.CalcQuatVecCurve(controlPoints, v0, v1);
                    spline.AddRange(motion.positions);
                }
            }
            return spline;
        }

        private void Calc_5PQuat_Curve()
        {

        }

    }
}

