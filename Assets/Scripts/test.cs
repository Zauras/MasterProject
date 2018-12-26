using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Master
{
    public class test : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            MotionData md = GetComponent<MotionData>();
            Debug.Log(md.positions.Length);

        }

        // Update is called once per frame
        void Update()
        {
            MotionData md = GetComponent<MotionData>();
            Debug.Log(md.positions.Length);
        }
    }

}

