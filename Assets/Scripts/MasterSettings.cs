﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Master {
    public class MasterSettings : MonoBehaviour
    {
        public bool stopTime = false;
        public bool animationLoop = false;
        public float animationSpeed = 1f;

        public float ERFheight = 1f;

        public bool isVectorArrowOn = true;
        public bool isVectorLineOn = false;


        public float droveSpeed = 40.0f;
        public int pathResolution = 100;

        public float weight0 = 1.0f;

        public float[] TList = { 0.0f, 2.0f, 4.0f };


        public Rect playfield = new Rect { x = -30.0f, y = -30.0f, width = 60.0f, height = 60.0f };

        public Color lineColor = Color.green;
        public float lineWide = 0.01f;

        public Color vectorLineColor = Color.red;
        public float vectorLineWide = 0.01f;

        public Material lineRendererMaterial;

    }

}
