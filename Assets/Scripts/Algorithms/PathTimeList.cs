using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Master {
    public class PathTimeList
    {
        public static float[] timePath;
        private static float[] TList;
        private static int rez;

        public static void CreateTimeList()
        {
            Debug.Log("3. CalculateTimeListSystem: Skaiciuojam TimeList");
            TList = BootStrap.Settings.TList;
            rez = BootStrap.Settings.pathResolution;

            timePath = GetTimeList(rez, TList);
        }

        protected static float[] GetTimeList(int rez, float[] TList)
        {
            float deltaStep = TList[TList.Length - 1] / rez;
            // Darant Spline - kaupiasi paklaida!
            float t = 0.0f;
            float[] timePath = new float[rez+1];
            for (int i = 0; i < rez + 1; i++)
            {
                timePath[i] = t;
                t += deltaStep;
            }
            return timePath;
        }

        protected static void GetTimeList(DynamicBuffer<TimeListElement> timeBuffer)
        {
            float deltaStep = TList[TList.Length-1] / rez;
            // Darant Spline - kaupiasi paklaida!
            float t = 0.0f;
            for (int i = 0; i < rez + 1; i++)
            {
                timeBuffer.Add(new TimeListElement { Value = t });
                t += deltaStep;
            }       
        }

    }
}
