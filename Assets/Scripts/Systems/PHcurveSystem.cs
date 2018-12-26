using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Master
{
    public class PHcurveSystem : ComponentSystem
    {
        struct Chunks
        {
            //public EntityArray entities;
            private ComponentArray<PHmotionMarker> markers;

            public readonly int Length;
            public ComponentArray<Transform> transform;
            public ComponentArray<MotionData> motion;
            public ComponentArray<LineRenderer> lineRenderers;
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
            for (int i = 0; i < transArr.Length; i++)
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

        private void SetupVectorsRendering(CPsAndVectors controlData)
        {
            LineRendererSystem.SetPolygonPoints(
                controlData.vecPoints[0].GetComponent<LineRenderer>(),
                new List<float3>() { controlData.controlPoints[0].position, controlData.vecPoints[0].position }
                );

            LineRendererSystem.SetPolygonPoints(
                controlData.vecPoints[1].GetComponent<LineRenderer>(),
                new List<float3>() { controlData.controlPoints[1].position, controlData.vecPoints[1].position }
                );
        }


        private (float3[], quaternion[]) Calc_PH_motion(Transform pathTransform)
        {
            
            List<float3> posSpline = new List<float3>();
            List<quaternion> rotSpline = new List<quaternion>();

            foreach (Transform curveTrans in pathTransform) // Curves
            {   if (curveTrans.name == "ERFholder") break;

                //Debug.Log(curveTrans.name);
                if (curveTrans.tag == "Trajectory")
                {
                    /// the Algorithm:
                    CPsAndVectors controlData = Extract_CPsAndVec(curveTrans, 2);
                    Transform p0T = controlData.controlPoints[0];
                    Transform p1T = controlData.controlPoints[1];
                    SetupVectorsRendering(controlData);

                    //quaternion v0 = CalcDistanceQuat(controlData.controlPoints[0].position, controlData.vecPoints[0].position);
                    //quaternion v1 = CalcDistanceQuat(controlData.controlPoints[1].position, controlData.vecPoints[1].position);
                    quaternion v0 = CalcDistanceQuat( controlData.vecPoints[0].position, controlData.controlPoints[0].position);
                    quaternion v1 = CalcDistanceQuat( controlData.vecPoints[1].position, controlData.controlPoints[1].position);

                    // Results:
                    (List<float3>, List<quaternion>) PHcurveData =
                            PHodCurve.FindPHmotion(p0T, p1T, v0, v1); // positions, rotations

                    posSpline.AddRange(PHcurveData.Item1); // position
                    rotSpline.AddRange(PHcurveData.Item2); // rotations
                } 
            }
            return (posSpline.ToArray(), rotSpline.ToArray());
        }

        protected override void OnUpdate()
        {
           
            if (!BootStrap.Settings.stopTime)
            {
                
                 //BootStrap.Settings.stopTime = true;

                for (int i = 0; i < _chunks.Length; i++) // Paths
                {
                    Transform pathTransform = _chunks.transform[i];
                    (float3[], quaternion[]) PHmotion = Calc_PH_motion(pathTransform);
                    LineRendererSystem.SetPolygonPoints(_chunks.lineRenderers[i], PHmotion.Item1);
                    
                    _chunks.motion[i].positions = PHmotion.Item1;
                    _chunks.motion[i].rotations = PHmotion.Item2;
                }
            }
        }

    }
}
