using System.Collections;
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
  //  [UpdateAfter(typeof(TrajectorySystem))]
    public class MovementControllerSystem : ComponentSystem
    {
        /*
        struct Travaler
        {
            //Using as a filter & dataEntries (no actual Enteties)
            public readonly int Length;
            public ComponentDataArray<PathID> pathID;
            public ComponentArray<Transform> transform;
           // [ReadOnly] public TravelerMarker marker;
        }

        [Inject] Travaler travelerGroup;
        */
        public Button NewGameButton;


        /*
        public static void MovementStep()
        {
            EntityManager em = World.Active.GetOrCreateManager<EntityManager>();
            var dicTrajectory = Database.ObjectMovementPath;
            var dicTravalers = Database.Travelers;
            foreach (var travalerEntry in dicTravalers)
            {
                
                int pathID = travalerEntry.Key;
                Entity traveler = travalerEntry.Value;
                int progressCounter = Database.MovementProgress[pathID];

                DisplacementData[] trajectory = dicTrajectory[pathID];

                var entTrasnform = em.GetComponentObject<Transform>(traveler);
                if (progressCounter >= (Database.ObjectMovementPath[pathID].Length-1))
                {
                    Database.MovementProgress[pathID] = 0;

                    entTrasnform.position = Database.ObjectMovementPath[pathID][0].position;
                    entTrasnform.rotation = Database.ObjectMovementPath[pathID][0].rotation;
                } else
                {
                    entTrasnform.position = Database.ObjectMovementPath[pathID][++progressCounter].position;
                    entTrasnform.rotation = Database.ObjectMovementPath[pathID][++progressCounter].rotation;
                    Database.MovementProgress[pathID] = progressCounter;
                }

                Debug.Log(progressCounter);
            }
                
        }
        */

        protected override void OnStartRunning()
        {
           // NewGameButton = GameObject.Find("StepButton").GetComponent<Button>();

            //NewGameButton.onClick.AddListener(MovementStep);
            
            //NewGameButton.gameObject.SetActive(true);
           // Debug.Log("SetupButton");
        }


        protected override void OnUpdate()
        {
            /*
            for (int i = 0; i < travelerGroup.Length; i++)
            {
                int pathID = travelerGroup.pathID[i].Value;
                var x = travelerGroup.transform[i].position;
               // Debug.Log(x);
                travelerGroup.transform[i].position = Database.ObjectMovementPath[pathID][0];

            }
            */
        }
    }
}

