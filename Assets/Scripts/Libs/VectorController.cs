using Unity.Mathematics;
using UnityEngine;

namespace Master
{
    public sealed class VectorController
    {
        private static Transform GetVecArrowTransform(Transform vecT)
        {
            foreach (Transform child in vecT)
            {
                if (child.name == "Vector") return child;
            }
            return null;
        }

        public static void SetVector(Transform vecT, float3 point)
        {
            Transform arrow = GetVecArrowTransform(vecT);
            arrow.position = point;
            float3 vecPoint = vecT.position;
            float3 vecValue = vecPoint-point;

            arrow.localScale = new float3(arrow.localScale.x, arrow.localScale.y, math.length(vecValue));

            //arrow.rotation.SetFromToRotation(point, vecPoint);
            //arrow.rotation = Quaternion.FromToRotation(math.normalizesafe(point), math.normalizesafe(vecPoint)); // Vector.right is x aixs in unity
            arrow.rotation = Quaternion.LookRotation(vecValue, Vector3.up);
        }



    }

}

