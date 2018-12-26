using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Master
{
    using H = Master.LibQuaternionAritmetics;

    [UpdateAfter(typeof(PHcurveSystem))]
    public class ERFramesSystem : ComponentSystem
    {
        struct Chunks
        {
            //Using as a filter & dataEntries (no actual Enteties)
            public readonly int Length;
            public ComponentArray<Transform> transform;
            public ComponentArray<MotionData> motion;
            public ComponentArray<PathERFs> ERFsData;
        }
        [Inject] Chunks _paths;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _paths.Length; i++) // Paths
            {
                Transform pathTransform = _paths.transform[i];

                if (_paths.ERFsData[i].holders.Length == 0)
                {
                    _paths.ERFsData[i].holders = new GameObject[_paths.motion[i].positions.Length];
                    for (int e=0; e < _paths.ERFsData[i].holders.Length; e++)
                    {
                        _paths.ERFsData[i].holders[e] = InitERFHolder(_paths.motion[i].positions[e],
                                                                      _paths.motion[i].rotations[e],
                                                                      pathTransform);
                    }
                }
                else
                {   // Recalc ERFs depends on motion position and rotation points
                    for (int e = 0; e < _paths.ERFsData[i].holders.Length; e++)
                    {   // Recalculate ERFrame & send it to Gizmos to render
                        _paths.ERFsData[i].holders[e].transform.position = _paths.motion[i].positions[e];
                        _paths.ERFsData[i].holders[e].
                            GetComponent<ERFGizmos>().
                                ERFrame = CalcERFrame(_paths.motion[i].positions[e],
                                                      _paths.motion[i].rotations[e]);
                    }
                }
            }
        }

        private static GameObject InitERFHolder(float3 initPosition, quaternion rotationERF, Transform parent)
        {
            GameObject go = new GameObject("ERFholder");
            go.transform.position = initPosition;
            go.transform.parent = parent;

            ERFGizmos goERF = go.AddComponent<ERFGizmos>();
            goERF.ERFrame = CalcERFrame(initPosition, rotationERF);

            return go;
        }


        private static EulerRodriguesFrame CalcERFrame(float3 position, quaternion rotation)
        {
            float rotLength = H.QuatLength(rotation);

            float3 ERFx = H.QuatToFloat3(H.DivQuatFromConst(
                            H.Mult(H.Mult(rotation, H.ii), H.Conj(rotation)), rotLength));
            float3 ERFy = H.QuatToFloat3(H.DivQuatFromConst(
                            H.Mult(H.Mult(rotation, H.jj), H.Conj(rotation)), rotLength));
            float3 ERFz = H.QuatToFloat3(H.DivQuatFromConst(
                            H.Mult(H.Mult(rotation, H.kk), H.Conj(rotation)), rotLength));

            float3 vecX = position + BootStrap.Settings.ERFheight * ERFx;
            float3 vecY = position + BootStrap.Settings.ERFheight * ERFy;
            float3 vecZ = position + BootStrap.Settings.ERFheight * ERFz;

            return new EulerRodriguesFrame(vecX, vecY, vecZ);
        }

    }
}

