using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Master
{
    public class RphMotionSystem : ComponentSystem
    {
        private float3[] GetControlPoints(Transform transform) 
        {
            List<float3> controlPoints = new List<float3>();
            foreach (Transform childTrans in transform)
            {
                if (childTrans.CompareTag("ControlPoint"))
                {
                    controlPoints.Add(childTrans.position);
                }
            }
            return controlPoints.ToArray();
        }
        
        private quaternion CalcDistanceQuat(float3 cp, float3 vecP)
        {
            return new quaternion(cp.x - vecP.x, cp.y - vecP.y, cp.z - vecP.z, 0f);
        }
        
        private quaternion CalcDirectionQuat(float3 target, float3 position)
        {
            return new quaternion(target.x - position.x, target.y - position.y, target.z - position.z, 0f);
        }

        private float4 CalcDirectionFloat4(float3 cp, float3 vecP)
        {
            return new float4(cp.x - vecP.x, cp.y - vecP.y, cp.z - vecP.z, 0f);
        }

        private void AddToSpline(List<float3> posSpline, List<quaternion> rotSpline,
                                 (List<float3>, List<quaternion>) curveData)
        {
            posSpline.AddRange(curveData.Item1); // position
            rotSpline.AddRange(curveData.Item2); // rotations
        }

        private (float3[], quaternion[], quaternion[]) Calc_PH_motion(Transform pathTransform, bool isClosedSpline, int curveCount)
        {
            List<float3> posSpline = new List<float3>();
            List<quaternion> rotSpline = new List<quaternion>();
            
            List<quaternion> vHList = new List<quaternion>();

            int cp = 0, vp = 1;
            Transform firstCP = null, currCP = null;

            for (int c = 0; c < curveCount; c++) // visi fragmentai
            {
                if (c == 0) // BEGINING OF SPLINE // full Algorithm (as independent curves)
                {
                    firstCP = pathTransform.GetChild(cp);
                    cp += 2; // To skip vecPoint index
                    currCP = pathTransform.GetChild(cp);
                    cp += 2; // To skip vecPoint index

                    Transform vec0 = pathTransform.GetChild(vp);
                    quaternion vH0 = CalcDistanceQuat(vec0.position, firstCP.position);

                    if(BootStrap.Settings.isVectorLineOn == true)
                    {
                        LineRendererSystem.SetLinePoints(vec0.GetComponent<LineRenderer>(),
                                  firstCP.position, vec0.position);
                    }
                    else if (BootStrap.Settings.isVectorArrowOn == true)
                    {
                        VectorController.SetVector(vec0, firstCP.position);
                    }
                    vp += 2; // To skip controlPoint index

                    Transform vec1 = pathTransform.GetChild(vp);
                    quaternion vH1 = CalcDistanceQuat(pathTransform.GetChild(vp).position, currCP.position);

                    if (BootStrap.Settings.isVectorLineOn == true)
                    {
                        LineRendererSystem.SetLinePoints(vec1.GetComponent<LineRenderer>(),
                                                      currCP.position, vec1.position);
                    }
                    else if (BootStrap.Settings.isVectorArrowOn == true)
                    {
                        VectorController.SetVector(vec1, currCP.position);
                    }
                    vp += 2; // To skip controlPoint index

                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHcMotion(firstCP, currCP, vH0, vH1); // positions, rotations

                    // Outcome
                    AddToSpline(posSpline, rotSpline, PHcurveData);
                    vHList.Add(vH0);
                    vHList.Add(vH1);
                }

                else if (c == curveCount-1 && isClosedSpline) // Closed Splane End
                {
                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHcMotion(currCP, firstCP); // positions, rotations

                    AddToSpline(posSpline, rotSpline, PHcurveData);
                }

                else // MIDDLEWARE || Open Splane End
                {
                    Transform nextCP = pathTransform.GetChild(cp);
                    cp += 2; // To skip vecPoint index

                    Transform vec1 = pathTransform.GetChild(vp);
                    quaternion vH1 = CalcDistanceQuat(vec1.position, nextCP.position);

                    if (BootStrap.Settings.isVectorLineOn == true)
                    {
                        LineRendererSystem.SetLinePoints(vec1.GetComponent<LineRenderer>(),
                                                          nextCP.position, vec1.position);
                    }
                    else if (BootStrap.Settings.isVectorArrowOn == true)
                    {
                        VectorController.SetVector(vec1, nextCP.position);
                    }
                    vp += 2; // To skip controlPoint index

                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHcMotion(currCP, nextCP, vH1); // positions, rotations
                    currCP = nextCP;

                    AddToSpline(posSpline, rotSpline, PHcurveData);
                    vHList.Add(vH1);
                }

            }
            return (posSpline.ToArray(), rotSpline.ToArray(), vHList.ToArray());
        }

        private float3[] Calc_PH_rotationCurve(Transform pathTransform, quaternion[] vecRots,
                                                float3[] movementControlPoints,
                                                bool isClosedSpline, int curveCount,
                                                bool useMovmentVectors, bool drawDebugVectors)
        {
            List<float3> posSpline = new List<float3>();
            int cp = 0, vp = 0;
            Transform firstCP = null, currCP = null;
            quaternion vH1 = new quaternion(), vH0 = new quaternion(), vHFirst = new quaternion();

            for (int c = 0; c < curveCount; c++) // visi fragmentai
            {
                if (c == 0) // BEGINING OF SPLINE // full Algorithm (as independent curves)
                {
                    float3 firstMovementCp = movementControlPoints[cp];
                    firstCP = pathTransform.GetChild(cp++);
                    float3 currMovementCp = movementControlPoints[cp];
                    currCP = pathTransform.GetChild(cp++);

                    LineRendererSystem.SetLinePoints(firstCP.GetComponent<LineRenderer>(),firstCP.position, firstMovementCp);
                    LineRendererSystem.SetLinePoints(currCP.GetComponent<LineRenderer>(),currCP.position, currMovementCp);

                    if (useMovmentVectors)
                    {
                        vH0 = vHFirst = vecRots[vp++];
                        vH1 = vecRots[vp++];
                    }
                    else
                    {
                        vH0 = vHFirst = CalcDirectionQuat(currCP.position, firstCP.position);
                        if (pathTransform.childCount > cp)
                            vH1 = CalcDirectionQuat(pathTransform.GetChild(cp).position, currCP.position);
                        else
                            vH1 = vH0;
                    }

                    if (drawDebugVectors)
                    {
                        Debug.DrawRay(firstCP.position, new float3(vH0.value.x, vH0.value.y, vH0.value.z), Color.magenta);
                        if (pathTransform.childCount == cp) 
                            Debug.DrawRay(currCP.position, new float3(vH1.value.x, vH1.value.y, vH1.value.z), Color.magenta); 
                    }
                    
                    // DARANT SPLINE - ypac closed, perziuretis
                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHcMotion(firstCP, currCP, vH0, vH1, false); // positions, rotations
                    
                    posSpline.AddRange(PHcurveData.Item1); // position
                    vH0 = vH1;
                } 
                else if (c == curveCount - 1 && isClosedSpline) // Closed Splane End
                {         
                    (List<float3>, List<quaternion>) PHcurveData =
                        PHodCurve.FindPHcMotion(currCP, firstCP, vH1, vHFirst, false); // positions, rotations
                    
                    Debug.DrawRay(currCP.position, new float3(vH1.value.x, vH1.value.y, vH1.value.z), Color.magenta);
                    Debug.DrawRay(firstCP.position, new float3(vHFirst.value.x, vHFirst.value.y, vHFirst.value.z), Color.magenta);

                    posSpline.AddRange(PHcurveData.Item1); // position
                }
                else // MIDDLEWARE || Open Splane End
                {
                    float3 nextMovementCp = movementControlPoints[cp];
                    Transform nextCP = pathTransform.GetChild(cp++);

                    LineRendererSystem.SetLinePoints(nextCP.GetComponent<LineRenderer>(),nextCP.position, nextMovementCp);

                    if (useMovmentVectors) vH1 = vecRots[vp++];
                    else
                    {
                        if (pathTransform.childCount > cp) 
                            vH1 = CalcDirectionQuat(pathTransform.GetChild(cp).position, nextCP.position);
                        else
                        {
                            if (isClosedSpline) vH1 = CalcDirectionQuat(firstCP.position, nextCP.position);
                            else vH1 = vH0;  
                        }
                    }
                      
                    if (drawDebugVectors)
                    {
                       Debug.DrawRay(currCP.position, new float3(vH0.value.x, vH0.value.y, vH0.value.z), Color.magenta);
                       Debug.DrawRay(nextCP.position, new float3(vH1.value.x, vH1.value.y, vH1.value.z), Color.magenta);
                    }
                      
                    (List<float3>, List<quaternion>) PHcurveData =
                    PHodCurve.FindPHcMotion(currCP, nextCP, vH0, vH1, false); // positions, rotations
                    currCP = nextCP;
                    vH0 = vH1;
                      
                    posSpline.AddRange(PHcurveData.Item1); // position
                }
            }

            return posSpline.ToArray();
        }

        private quaternion[] CalcPhMotionRotations (float3[] movementCurve, float3[] rotationCurve)
        {
            quaternion[] rotSpline = new quaternion[movementCurve.Length]; //phRotations; //new quaternion[movementCurve.Length];
            quaternion swapXtoZrotor = Quaternion.Euler(0f, -90f, 0f);
            
            for (int i = 0; i < movementCurve.Length; i++)
            {
                float3 directionVector = rotationCurve[i] - movementCurve[i];
                rotSpline[i] = Quaternion.LookRotation(directionVector) * swapXtoZrotor;
            }
            
            return rotSpline;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((
                RphMotionMarker motionMarker,
                Transform motionTransform,
                MotionData motions
            ) =>
            {
                Transform movementPhObjTransform = motionTransform.GetChild(0);
                 (float3[], quaternion[], quaternion[]) phMovementCurve = Calc_PH_motion(movementPhObjTransform,
                                                                    motions.isClosedSpline,
                                                                    motions.curveCount);

                 LineRendererSystem.SetPolygonPoints(movementPhObjTransform.GetComponent<LineRenderer>(), phMovementCurve.Item1);
                 quaternion[] vecRots = phMovementCurve.Item3;

                 Transform rotationPhObjTransform = motionTransform.GetChild(1);
                 float3[] movementControlPoints = GetControlPoints(movementPhObjTransform);
                 
                 float3[] phRotationCurve = Calc_PH_rotationCurve(rotationPhObjTransform,
                                                                   vecRots,
                                                                   movementControlPoints,
                                                                   motions.isClosedSpline,
                                                                   motions.curveCount,
                                                                   motionMarker.useMovmentVectors,
                                                                   motionMarker.drawDebugVectors);

                 LineRendererSystem.SetPolygonPoints(movementPhObjTransform.GetComponent<LineRenderer>(), phMovementCurve.Item1);
                 LineRendererSystem.SetPolygonPoints(rotationPhObjTransform.GetComponent<LineRenderer>(), phRotationCurve);

                 motions.positions = phMovementCurve.Item1;
                 motions.rotations = CalcPhMotionRotations(phMovementCurve.Item1, phRotationCurve);
            });
        }
    }
}
