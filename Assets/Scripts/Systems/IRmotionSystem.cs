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


        private (float3[], quaternion[]) Calc_IRp3v2_motion(
            Transform splineT, bool isRotWithWeight, bool isClosedSpline, int curveCount)
        {
            List<float3> posSpline = new List<float3>();
            List<quaternion> rotSpline = new List<quaternion>();

            int cp = 0, vp = 1;
            Transform firstCP = null;
            Transform firstVP = null;
            Transform vp0 = null;
            Transform vp1 = null;
            float3[] CPs = new float3[3];
            float4 firstWeight = LibQuaternionAritmetics.one;

            for (int c = 0; c < curveCount; c++) // visi vaikai
            {
                if (c == 0) // BEGINING OF SPLINE // full Algorithm (as independent curves)
                {
                    Transform cp0T = firstCP = splineT.GetChild(cp);
                    CPs[0] = cp0T.localPosition;  // startPoint
                    cp += 2; // To skip vecPoint index

                    Transform cp1T = splineT.GetChild(cp);
                    CPs[1] = cp1T.localPosition;            // midPoint
                    cp ++;

                    Transform cp2T = splineT.GetChild(cp);
                    CPs[2] = cp2T.localPosition;            // endPoint

                    vp0 = firstVP = splineT.GetChild(vp); // startVector
                    //vp0 = firstVP = vpT.localPosition;    

                    if (BootStrap.Settings.isVectorLineOn == true)
                    {
                        LineRendererSystem.SetLinePoints(vp0.GetComponent<LineRenderer>(),
                                                           cp0T.position, vp0.position);
                    }
                    else if (BootStrap.Settings.isVectorArrowOn == true)
                    {
                        VectorController.SetVector(vp0, cp0T.position);
                    }

                    vp += 3; // To skip controlPoint indexes
                    vp1 = splineT.GetChild(vp);              // endVector

                    if (BootStrap.Settings.isVectorLineOn == true)
                    {
                        LineRendererSystem.SetLinePoints(vp1.GetComponent<LineRenderer>(),
                                                       cp2T.position, vp1.position);
                    }
                    else if (BootStrap.Settings.isVectorArrowOn == true)
                    {
                        VectorController.SetVector(vp1, cp2T.position);
                    }

                    (List<float3>, List<quaternion>) IRcurveData =
                        IRp3v2Curve.FindIRcMotion(
                            splineT, ref firstWeight, isRotWithWeight, CPs, vp0.localPosition, vp1.localPosition); // positions, rotations
                    AddToSpline(posSpline, rotSpline, IRcurveData);
                }

                else if (c == curveCount - 1 && isClosedSpline) // Closed Splane End
                {
                    CPs[0] = CPs[2];                    // startPoint
                    cp += 2; // To skip vecPoint index
                    CPs[1] = splineT.GetChild(cp).localPosition;  // midPoint
                    CPs[2] = firstCP.localPosition;               // endPoint

                    (List<float3>, List<quaternion>) IRcurveData =
                        IRp3v2Curve.FindIRcMotion(
                            splineT, ref firstWeight, isRotWithWeight, CPs, vp1.localPosition, firstVP.localPosition); // positions, rotations

                    AddToSpline(posSpline, rotSpline, IRcurveData);
                }

                else // MIDDLEWARE || Open Splane End
                {
                    CPs[0] = CPs[2];                              // startPoint
                    cp += 2; // To skip vecPoint index

                    CPs[1] = splineT.GetChild(cp).localPosition;            // midPoint
                    cp++;

                    Transform cp2T = splineT.GetChild(cp);
                    CPs[2] = cp2T.localPosition;            // endPoint

                    vp0 = vp1;                                    // startVector
                    vp += 3; // To skip controlPoint indexes

                    vp1 = splineT.GetChild(vp);          // endVector

                    if (BootStrap.Settings.isVectorLineOn == true)
                    {
                        LineRendererSystem.SetLinePoints(vp1.GetComponent<LineRenderer>(),
                                                            cp2T.position, vp1.position);
                    }
                    else if (BootStrap.Settings.isVectorArrowOn == true)
                    {
                        VectorController.SetVector(vp1, cp2T.position);
                    }

                    (List<float3>, List<quaternion>) IRcurveData =
                        IRp3v2Curve.FindIRcMotion(
                            splineT, ref firstWeight, isRotWithWeight, CPs, vp0.localPosition, vp1.localPosition); // positions, rotations

                    //List<float3> globalPositions = ConvertToGlobalPositions(IRcurveData.);
                    //IRcurveData.Item1 = 
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
                                                _paths.markers[i].useRotWithWeight,
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

