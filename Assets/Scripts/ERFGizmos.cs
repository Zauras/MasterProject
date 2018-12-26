using UnityEngine;
using System.Collections;
namespace Master
{
    public class ERFGizmos : MonoBehaviour
    {
        public EulerRodriguesFrame ERFrame;

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, ERFrame.vecX);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, ERFrame.vecY);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, ERFrame.vecZ);
        }
    }
}
