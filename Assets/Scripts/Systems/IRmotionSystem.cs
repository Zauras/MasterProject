using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Master
{
    public class IRmotionSystem : ComponentSystem
    {
        struct Chunks
        {
            public ComponentArray<IRp3v2motionMarker> markers;
            public readonly int Length;
            public ComponentArray<Transform> T;
            public ComponentArray<MotionData> motions;
            public ComponentArray<LineRenderer> lrs;
        }
        [Inject] private Chunks _paths;

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

        private void AddToSpline(List<float3> posSpline, List<quaternion> rotSpline,
                                 (List<float3>, List<quaternion>) curveData)
        {
            posSpline.AddRange(curveData.Item1); // position
            rotSpline.AddRange(curveData.Item2); // rotations
        }


        private (float3[], quaternion[]) Calc_IRp3v2_motion(Transform pT, bool isClosedSpline, int curveCount)
        {
            List<float3> posSpline = new List<float3>();
            List<quaternion> rotSpline = new List<quaternion>();

            int cp = 0, vp = 1;
            float3 firstCP = new float3();
            float3 firstVP = new float3();
            float3 vp0 = new float3();
            float3 vp1 = new float3();
            float3[] CPs = new float3[3];

            for (int c = 0; c < curveCount; c++) // visi vaikai
            {
                if (c == 0) // BEGINING OF SPLINE // full Algorithm (as independent curves)
                {
                    CPs[0] = firstCP = pT.GetChild(cp).position;  // startPoint
                    cp += 2; // To skip vecPoint index
                    CPs[1] = pT.GetChild(cp).position;            // midPoint
                    cp ++;
                    CPs[2] = pT.GetChild(cp).position;            // endPoint

                    Transform vpT = pT.GetChild(vp);
                    vp0 = firstVP = vpT.position;                 // startVector
                    LineRendererSystem.SetLinePoints(vpT.GetComponent<LineRenderer>(),
                                                        CPs[0], vp0);

                    vp += 3; // To skip controlPoint indexes
                    vpT = pT.GetChild(vp);
                    vp1 = vpT.position;                          // endVector
                    LineRendererSystem.SetLinePoints(vpT.GetComponent<LineRenderer>(),
                                                        CPs[2], vp1);

                    (List<float3>, List<quaternion>) IRcurveData =
                        IRp3v2Curve.FindIRcMotion(CPs, vp0, vp1); // positions, rotations

                    AddToSpline(posSpline, rotSpline, IRcurveData);
                }

                else if (c == curveCount - 1 && isClosedSpline) // Closed Splane End
                {
                    CPs[0] = CPs[2];                    // startPoint
                    cp += 2; // To skip vecPoint index
                    CPs[1] = pT.GetChild(cp).position;  // midPoint
                    CPs[2] = firstCP;                   // endPoint

                    (List<float3>, List<quaternion>) IRcurveData =
                        IRp3v2Curve.FindIRcMotion(CPs, vp1, firstVP); // positions, rotations

                    AddToSpline(posSpline, rotSpline, IRcurveData);
                }

                else // MIDDLEWARE || Open Splane End
                {
                    CPs[0] = CPs[2];                              // startPoint
                    cp += 2; // To skip vecPoint index
                    CPs[1] = pT.GetChild(cp).position;            // midPoint
                    cp++;
                    CPs[2] = pT.GetChild(cp).position;            // endPoint

                    vp0 = vp1;                                    // startVector
                    vp += 3; // To skip controlPoint indexes
                    Transform vpT = pT.GetChild(vp);
                    vp1 = vpT.position;                          // endVector
                    LineRendererSystem.SetLinePoints(vpT.GetComponent<LineRenderer>(),
                                                        CPs[2], vp1);

                    (List<float3>, List<quaternion>) IRcurveData =
                        IRp3v2Curve.FindIRcMotion(CPs, vp0, vp1); // positions, rotations

                    AddToSpline(posSpline, rotSpline, IRcurveData);
                }

            }
            return (posSpline.ToArray(), rotSpline.ToArray());
        }

        private void Calc_5PQuat_Curve()
        {

        }

        protected override void OnUpdate()
        {
            if (!BootStrap.Settings.stopTime)
            {
                //BootStrap.Settings.stopTime = true;
                // EntityManager em = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < _paths.Length; i++) // Paths2
                {
                    Transform pathTransform = _paths.T[i];
                    (float3[], quaternion[]) IRmotion  =
                            Calc_IRp3v2_motion( pathTransform,
                                                _paths.motions[i].isClosedSpline,
                                                _paths.motions[i].curveCount);

                    _paths.motions[i].positions = IRmotion.Item1;
                    _paths.motions[i].rotations = IRmotion.Item2;
                    LineRendererSystem.SetPolygonPoints(_paths.lrs[i], IRmotion.Item1);
                }
            }
        }

    }
}

