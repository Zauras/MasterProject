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


        private void AddToSpline(List<float3> posSpline, List<quaternion> rotSpline,
                                 (List<float3>, List<quaternion>) curveData)
        {
            posSpline.AddRange(curveData.Item1); // position
            rotSpline.AddRange(curveData.Item2); // rotations
        }




        private (float3[], quaternion[]) Calc_PH_motion(Transform pT, bool isClosedSpline, int curveCount)
        {
            List<float3> posSpline = new List<float3>();
            List<quaternion> rotSpline = new List<quaternion>();

            int cp = 0, vp = 1;
            Transform firstCP = null, currCP = null;

            for (int c = 0; c < curveCount; c++) // visi vaikai
            {
                if (c == 0) // BEGINING OF SPLINE // full Algorithm (as independent curves)
                {
                    firstCP = pT.GetChild(cp);
                    cp += 2; // To skip vecPoint index
                    currCP = pT.GetChild(cp);
                    cp += 2; // To skip vecPoint index

                    quaternion vH0 = CalcDistanceQuat(pT.GetChild(vp).position, firstCP.position);
                    vp += 2; // To skip controlPoint index
                    quaternion vH1 = CalcDistanceQuat(pT.GetChild(vp).position, currCP.position);
                    vp += 2; // To skip controlPoint index

                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHCmotion(firstCP, currCP, vH0, vH1); // positions, rotations

                    AddToSpline(posSpline, rotSpline, PHcurveData);
                }

                else if (c == curveCount-1 && isClosedSpline) // Closed Splane End
                {
                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHCmotion(currCP, firstCP); // positions, rotations

                    AddToSpline(posSpline, rotSpline, PHcurveData);
                }

                else // MIDDLEWARE || Open Splane End
                {
                    Transform nextCP = pT.GetChild(cp);
                    cp += 2; // To skip vecPoint index

                    quaternion vH1 = CalcDistanceQuat(pT.GetChild(vp).position, nextCP.position);
                    vp += 2; // To skip controlPoint index

                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHCmotion(currCP, nextCP, vH1); // positions, rotations
                    currCP = nextCP;

                    AddToSpline(posSpline, rotSpline, PHcurveData);
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
                    (float3[], quaternion[]) PHmotion = Calc_PH_motion(pathTransform,
                                                                       _chunks.motion[i].isClosedSpline,
                                                                       _chunks.motion[i].curveCount);
                    _chunks.motion[i].positions = PHmotion.Item1;
                    _chunks.motion[i].rotations = PHmotion.Item2;
                    LineRendererSystem.SetPolygonPoints(_chunks.lineRenderers[i], PHmotion.Item1);
                }
            }
        }

    }
}
