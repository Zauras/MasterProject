using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Master
{
    public class SetupSplinesSystem : ComponentSystem
    {
        struct PHchunk
        {
            public readonly int Length;
            public ComponentArray<PHmotionMarker> markers;
            public ComponentArray<Transform> transforms;
            public ComponentArray<MotionData> motions;
        }
        [Inject] private PHchunk _phs;

        struct IRchunk
        {
            public readonly int Length;
            public ComponentArray<IRp3v2motionMarker> markers;
            public ComponentArray<Transform> transforms;
            public ComponentArray<MotionData> motions;
        }
        [Inject] private IRchunk _irs;


        private int CountCurves(Transform transform, string curveType, bool isClosedSpline)
        {
            int counter = 0;
            foreach (Transform childTrans in transform)
            {
                if (childTrans.tag == "ControlPoint") { counter++; }
            }

            if (curveType == "PH") //PHcurve
            {
                if (isClosedSpline) return counter;
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

        protected override void OnStartRunning()
        {
            UpdateInjectedComponentGroups();
            for (int i = 0; i < _phs.Length; i++)
            {
                _phs.motions[i].curveCount = CountCurves(_phs.transforms[i],
                                                          _phs.markers[i].TYPE,
                                                          _phs.motions[i].isClosedSpline);

            }
        }

        protected override void OnUpdate() { }
    }
}
