﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.UI;

namespace Master
{
    using H = Master.LibQuaternionAritmetics;

    [UpdateAfter(typeof(PHmotionSystem))]
    [UpdateAfter(typeof(IRmotionSystem))]
    public class MovementControllerSystem : ComponentSystem
    {
        private Button StepButton;
        private Button AnimationButton;
        private bool animationMove = false;

        /*
        struct Chunks
        {
            //Using as a filter & dataEntries (no actual Enteties)
            public readonly int Length;
            //public ComponentArray<PathID> pathID;
            public ComponentArray<Transform> transform;
            [ReadOnly] public ComponentArray<TravelerData> travelerData;
        }
        [Inject] Chunks _travelers;
        */

        public void MovementStep()
        {
            EntityManager em = World.Active.EntityManager;

            Entities.ForEach((Transform transform, TravelerData travelerData) =>
            {
                // Fitted Motion:
                Entity motionEnt = GetEntityFromGOE(travelerData.motion);
                MotionData motion = em.GetComponentObject<MotionData>(motionEnt);
                float3[] positions = motion.positions;
                quaternion[] rotations = motion.rotations;
                // Traveler Data:
                Transform travelerTransform = transform;


                if (travelerData.pathIndex >= positions.Length) // Jei kelio pabaiga
                {
                    travelerData.pathIndex = 0;
                    travelerTransform.position = positions[0];
                    travelerTransform.rotation = rotations[0];
                }
                else
                {
                    travelerTransform.position = positions[travelerData.pathIndex];
                    travelerTransform.rotation = rotations[travelerData.pathIndex];
                    travelerData.pathIndex++;
                }
            });

            /*
            for (int i = 0; i < _travelers.Length; i++) // Travelers
            {
                // Fitted Motion:
                Entity motionEnt = GetEntityFromGOE(_travelers.travelerData[i].motion);
                MotionData motion = em.GetComponentObject<MotionData>(motionEnt);
                float3[] positions = motion.positions;
                quaternion[] rotations = motion.rotations;
                // Traveler Data:
                Transform travelerTransform = _travelers.transform[i];
               

                if (_travelers.travelerData[i].pathIndex >= positions.Length) // Jei kelio pabaiga
                {
                    _travelers.travelerData[i].pathIndex = 0;
                    travelerTransform.position = positions[0];
                    travelerTransform.rotation = rotations[0];
                }
                else
                {
                    travelerTransform.position = positions[_travelers.travelerData[i].pathIndex];
                    travelerTransform.rotation = rotations[_travelers.travelerData[i].pathIndex];
                    _travelers.travelerData[i].pathIndex++;
                }
            }
            */
        }

        public void AnimatedMotion()
        {
            EntityManager em = World.Active.EntityManager;
            //float step = BootStrap.Settings.animationSpeed * Time.deltaTime;
            //float startTime = Time.realtimeSinceStartup;

            Entities.ForEach((Transform transform, TravelerData travelerData) =>
            {
                // Fitted Motion:
                if (em.Exists(travelerData.motion.Entity))
                {
                    Entity motionEnt = GetEntityFromGOE(travelerData.motion);
                    MotionData motion = em.GetComponentObject<MotionData>(motionEnt);
                    float3[] positions = motion.positions;
                    quaternion[] rotations = motion.rotations;

                    //Debug.Log(_travelers.travelerData[i].pathIndex);
                    // Traveler Data:
                    Transform travelerTransform = transform;

                    if (travelerData.pathIndex == 0) // Jei kelio pradzia
                    {
                        travelerTransform.rotation = rotations[0];
                        travelerTransform.position = positions[0];
                        travelerData.pathIndex++;
                    }

                    if (animationMove)
                    {
                        float stepPos = BootStrap.Settings.animationSpeed * Time.deltaTime;
                        float stepRot = BootStrap.Settings.animationSpeed * Time.deltaTime * 1000f;

                        travelerTransform.position = Vector3.MoveTowards(
                                                        travelerTransform.position,
                                                        positions[travelerData.pathIndex],
                                                        stepPos);

                        if (travelerData.pathIndex > 0)
                        {
                            travelerTransform.rotation = Quaternion.Lerp(
                                travelerTransform.rotation,
                                rotations[travelerData.pathIndex],
                                stepRot);
                        }

                        if (H.isEqual(travelerTransform.position, positions[travelerData.pathIndex]))
                        {
                            travelerData.pathIndex++;
                        }

                        if (travelerData.pathIndex >= positions.Length)
                        {
                            // animationMove = false;
                            travelerData.pathIndex = 0;
                        }
                    }

                }
            });

            /*
            for (int i = 0; i < _travelers.Length; i++) // Travelers
            {
                // Fitted Motion:
                if (em.Exists(_travelers.travelerData[i].motion.Entity))
                {
                    Entity motionEnt = GetEntityFromGOE(_travelers.travelerData[i].motion);
                    MotionData motion = em.GetComponentObject<MotionData>(motionEnt);
                    float3[] positions = motion.positions;
                    quaternion[] rotations = motion.rotations;

                    //Debug.Log(_travelers.travelerData[i].pathIndex);
                    // Traveler Data:
                    Transform travelerTransform = _travelers.transform[i];

                    if (_travelers.travelerData[i].pathIndex == 0) // Jei kelio pradzia
                    {
                        travelerTransform.rotation = rotations[0];
                        travelerTransform.position = positions[0];
                        _travelers.travelerData[i].pathIndex++;
                    }

                    if (animationMove)
                    {
                        float stepPos = BootStrap.Settings.animationSpeed * Time.deltaTime;
                        float stepRot = BootStrap.Settings.animationSpeed * Time.deltaTime*1000f;

                        travelerTransform.position = Vector3.MoveTowards(
                                                        travelerTransform.position,
                                                        positions[_travelers.travelerData[i].pathIndex],
                                                        stepPos);

                        if (_travelers.travelerData[i].pathIndex > 0)
                        {
                            travelerTransform.rotation = Quaternion.Lerp(
                                travelerTransform.rotation,
                                rotations[_travelers.travelerData[i].pathIndex],
                                stepRot);
                        }

                        if (H.isEqual(travelerTransform.position, positions[_travelers.travelerData[i].pathIndex]))
                        {
                            _travelers.travelerData[i].pathIndex++;
                        }

                        if (_travelers.travelerData[i].pathIndex >= positions.Length)
                        {
                            // animationMove = false;
                            _travelers.travelerData[i].pathIndex = 0;
                        }
                    }
                }
            }
            */
        }

    private static Entity GetEntityFromGOE(GameObjectEntity goe)
        {
            return goe.GetComponent<GameObjectEntity>().Entity;
        }

        public void ToogleAnimation()
        {
            animationMove = !animationMove;
        }

        protected override void OnStartRunning()
        {
            StepButton = GameObject.Find("StepButton").GetComponent<Button>();
            StepButton.onClick.AddListener(MovementStep);
            StepButton.gameObject.SetActive(true);

            AnimationButton = GameObject.Find("AnimationButton").GetComponent<Button>();
            AnimationButton.onClick.AddListener(ToogleAnimation);
            AnimationButton.gameObject.SetActive(true);
            
        }

        protected override void OnUpdate()
        {
            if (animationMove) { AnimatedMotion(); }

        }

    }
}

