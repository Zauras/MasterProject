using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Master
{
    public class SetupSplinesSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            // PH paths
            Entities.ForEach((
                PHmotionMarker marker,
                Transform transform,
                MotionData motions
            ) =>
            {
                motions.curveCount = CountCurves(
                    transform,
                    marker.TYPE,
                    motions.isClosedSpline);
            });

            // IRp3v2 paths
            Entities.ForEach((
                IRp3v2motionMarker marker,
                Transform transform,
                MotionData motions
            ) =>
            {
                motions.curveCount = CountCurves(
                    transform,
                    marker.TYPE,
                    motions.isClosedSpline);
            });
        }
        
        private int CountCurves(Transform transform, string curveType, bool isClosedSpline)
        {
            int counter = 0;
            foreach (Transform childTrans in transform)
            {
                if (childTrans.CompareTag("ControlPoint")) { counter++; }
            }

            if (curveType == "PH") //PHcurve
            {
                if (isClosedSpline) 
                    return counter;
                return counter - 1;
            }
            else if (curveType == "IRp3v2") //IRp3v2 curve
            {
                return (int) (counter / 2);
            }
            else if (curveType == "IRp5") //IRp5 curve
            {
                return 1;
            }
            return 0;
        }

        protected override void OnUpdate()
        {
            // PH paths
            Entities.ForEach((
                PHmotionMarker marker,
                Transform transform,
                MotionData motions
            ) =>
            {
                motions.curveCount = CountCurves(
                    transform,
                    marker.TYPE,
                    motions.isClosedSpline);
            });

            // IRp3v2 paths
            Entities.ForEach((
                IRp3v2motionMarker marker,
                Transform transform,
                MotionData motions
            ) =>
            {
                motions.curveCount = CountCurves(
                    transform,
                    marker.TYPE,
                    motions.isClosedSpline);
            });
        }
    }
}
