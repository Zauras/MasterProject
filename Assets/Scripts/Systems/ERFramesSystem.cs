
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Master
{
    using H = Master.LibQuaternionAritmetics;

    [UpdateAfter(typeof(PHmotionSystem))]
    public class ERFramesSystem : ComponentSystem
    {
        struct Chunks
        {
            //Using as a filter & dataEntries (no actual Enteties)
            public readonly int Length;
            public ComponentArray<Transform> transform;
            public ComponentArray<MotionData> motion;
            public ComponentArray<PathERFs> pathERFs;
        }
        [Inject] Chunks _paths;

        protected override void OnUpdate()
        {
            for (int i = 0; i < _paths.Length; i++) // Paths
            {
                Transform pathTransform = _paths.transform[i];
                
                if (_paths.pathERFs[i].ERframes == null)
                {
                   _paths.pathERFs[i].ERframes = 
                        new EulerRodriguesFrame[_paths.motion[i].positions.Length];
                }

                if ( _paths.motion[i].positions.Length > 0
                    || _paths.motion[i].positions.Length != _paths.pathERFs[i].ERframes.Length)
                {
                    _paths.pathERFs[i].ERframes =
                        new EulerRodriguesFrame[_paths.motion[i].positions.Length];
                }

                for (int e = 0; e < _paths.motion[i].positions.Length; e++)
                {   // Recalculate ERFrame & send it to Gizmos to render
                    _paths.pathERFs[i].ERframes[e] = CalcERFrame(_paths.motion[i].positions[e],
                                                                    _paths.motion[i].rotations[e]);
                }
            }
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

