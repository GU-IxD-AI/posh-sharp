using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using POSH.sys;



namespace POSH.unity
{
	public class RobotMovement : POSHBehaviour
	{
        public float patrolSpeed = 2f;							// The nav mesh agent's speed when patrolling.
        public float chaseSpeed = 5f;							// The nav mesh agent's speed when chasing.
        public float chaseWaitTime = 5f;						// The amount of time to wait when the last sighting is reached.
        public float patrolWaitTime = 1f;						// The amount of time to wait when the patrol way point is reached.
        public Transform[] patrolWayPoints;						// An array of transforms for the patrol route.

		public float fieldOfViewAngle = 110f;				// Number of degrees, centred on forward, for the enemy see.
        public bool playerInSight;							// Whether or not the player is currently sighted.
        public bool nextToCharger;                          // Enemy is next to a Charger
        public Vector3 personalLastSighting;				// Last place this enemy spotted the player.


        public float[] activations;
        public bool[] activegoals;

        public bool alive;

        private DoneEnemySight enemySight;						// Reference to the EnemySight script.
        private ChargingBehaviour charging;						// Reference to the EnemySight script.
        private NavMeshAgent nav;								// Reference to the nav mesh agent.
        private GameObject player;								// Reference to the player's transform.
        private DonePlayerHealth playerHealth;					// Reference to the PlayerHealth script.
        private DoneLastPlayerSighting lastPlayerSighting;		// Reference to the last global sighting of the player.
        private float chaseTimer;								// A timer for the chaseWaitTime.
        private float patrolTimer;								// A timer for the patrolWaitTime.
        private int wayPointIndex;								// A counter for the way point array.
        private bool running = false;                           // A trigger for when the game actually starts and POSH can start interacting.

        private SphereCollider col;							// Reference to the sphere collider trigger component.
        private Animator anim;								// Reference to the Animator.
        private Animator playerAnim;						// Reference to the player's animator component.
        private DoneHashIDs hash;							// Reference to the HashIDs.
        private Vector3 previousSighting;					// Where the player was sighted last frame.
        private float previousSightingTime;

        private Vector3 navDestination;
        private float tickTimer;
        private DoneAlarmLight alarm;


        // ramps for behaviours
        public Dictionary<string,RampActivation> ergos;

        protected override POSHInnerBehaviour InstantiateInnerBehaviour(AgentBase agent)
        {
            ergos = new Dictionary<string, RampActivation>();
            
            ergos.Add("patrol", new RampActivation  (0,  10, 0, 0.1f, 1.1f, 1));
            ergos.Add("charging", new RampActivation(0, 10, 0, 0.1f, 1.1f, 1));
            ergos.Add("chasing", new RampActivation(0, 10, 0, 0.1f, 1.5f, 1));
            activations = new float[ergos.Count];
            activegoals = new bool[ergos.Count];
            charging.ergos = ergos;
            running = true;
            previousSightingTime = - 3;

            return new RobotMovementInner(agent,this,null);

        }

        protected override void ConfigureParameters(Dictionary<string, object> parameters) { }


        protected override void ConfigureParameter(string para, object value)
        {
            switch (para)
            {
                case "startloc": 
                    SetStartLocation(value as string);
                    break;
                case "waypoints":
                    SetWaypoints(value as string);
                    break;
                default:
                    break;
            }

        }

        private void SetStartLocation(string waypoint)
        {
            GameObject wp = GameObject.Find(waypoint);
            if (wp is GameObject)
                gameObject.transform.position = wp.transform.position;
        }

        private void SetWaypoints(string waypointList)
        {
            string[] wayPoints = waypointList.Trim().Split(' ');
            if (wayPoints.Length > 0)
            {
                List<Transform> locations = new List<Transform>();
                foreach(string wpString in wayPoints) {
                    Transform wp =GameObject.Find(wpString).transform;

                    if (wp is Transform)
                        locations.Add(wp);
                    }
                patrolWayPoints = locations.ToArray();
            }
        }

        void Awake()
        {
            // Setting up the references.
            enemySight = GetComponent<DoneEnemySight>();
            charging = GetComponent<ChargingBehaviour>();
            nav = GetComponent<NavMeshAgent>();
            player = GameObject.FindGameObjectWithTag(DoneTags.player);
            playerHealth = player.GetComponent<DonePlayerHealth>();
            lastPlayerSighting = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneLastPlayerSighting>();

            col = GetComponent<SphereCollider>();
            anim = GetComponent<Animator>();
            playerAnim = player.GetComponent<Animator>();
            hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();

            // Set the personal sighting and the previous sighting to the reset position.
            personalLastSighting = lastPlayerSighting.resetPosition;
            previousSighting = lastPlayerSighting.resetPosition;
            previousSightingTime = -10;
            navDestination = Vector3.one;
            tickTimer = 0;
            alarm = GameObject.FindGameObjectWithTag("AlarmLight").GetComponent<DoneAlarmLight>();
        }


        void Update()
        {
            // setting ticks only every third of a second
            if (ergos is Dictionary<string, RampActivation> && ergos.Count > 1 && Time.realtimeSinceStartup - tickTimer > 0.3f)
            {
                ergos["patrol"].Tick(false);
                ergos["charging"].Tick(charging.NeedToCharge());

                if (Time.realtimeSinceStartup - previousSightingTime < 5 || alarm.alarmOn)
                ergos["chasing"].Tick((Time.realtimeSinceStartup - previousSightingTime < 3) || alarm.alarmOn);
                
                activations[0] = ergos["patrol"].GetActivation();
                activations[1] = ergos["charging"].GetActivation();
                activations[2] = ergos["chasing"].GetActivation();

                activegoals[0] = ergos["patrol"].IsActive();
                activegoals[1] = ergos["charging"].IsActive();
                activegoals[2] = ergos["chasing"].IsActive();
                tickTimer = Time.realtimeSinceStartup;

                alive = GetComponent<ChargingBehaviour>().alive;
                if (!alive)
                    nav.Stop();
                
                if (!alarm.alarmOn && Time.realtimeSinceStartup - previousSightingTime > 5)
                {
                        ergos["chasing"].Reset();

                }
            }
        }

        internal bool GameRunning()
        {
            Loom.QueueOnMainThread(() =>
            {
                running = Application.isPlaying;
            });

            return running;
        }

        float CalculatePathLength(Vector3 targetPosition)
        {
            // Create a path and set it based on a target position.
            NavMeshPath path = new NavMeshPath();
            if (nav.enabled)
                nav.CalculatePath(targetPosition, path);

            // Create an array of points which is the length of the number of corners in the path + 2.
            Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];

            // The first point is the enemy's position.
            allWayPoints[0] = transform.position;

            // The last point is the target position.
            allWayPoints[allWayPoints.Length - 1] = targetPosition;

            // The points inbetween are the corners of the path.
            for (int i = 0; i < path.corners.Length; i++)
            {
                allWayPoints[i + 1] = path.corners[i];
            }

            // Create a float to store the path length that is by default 0.
            float pathLength = 0;

            // Increment the path length by an amount equal to the distance between each waypoint and the next.
            for (int i = 0; i < allWayPoints.Length - 1; i++)
            {
                pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
            }

            return pathLength;
        }


        void OnTriggerExit(Collider other)
        {
            // If the player leaves the trigger zone...
            if (other.gameObject == player)
            {
                // ... the player is not in sight.
                playerInSight = false;
                anim.SetBool(hash.playerInSightBool, false);
            }
            else if (other.gameObject == charging.chargerLocation.gameObject)
                nextToCharger = false;
        }

        void OnTriggerStay(Collider other)
        {
            // If the player has entered the trigger sphere...
            if (other.gameObject == player)
                TriggeredPlayer(other.transform);
            else if (other.gameObject == charging.chargerLocation)
                TriggerCharger(other.transform);
        }

        private void TriggerCharger(Transform other)
        {
            // Create a vector from the enemy to the charger
            Vector3 direction = other.position - transform.position;
            
            RaycastHit hit;

            // ... and if a raycast towards the charger hits something...
            if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
            {
                // ... and if the raycast hits the charger location...
                if (hit.collider.gameObject == charging.chargerLocation.gameObject)
                {
                    if (direction.sqrMagnitude <= charging.chargingDistance * charging.chargingDistance)
                        nextToCharger = true;
                    else
                        nextToCharger = false;
                }
            }
            
        }

        private void TriggeredPlayer(Transform other)
        {
            // By default the player is not in sight.
            playerInSight = false;

            // Create a vector from the enemy to the player and store the angle between it and forward.
            Vector3 direction = other.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);

            // If the angle between forward and where the player is, is less than half the angle of view...
            if (angle < fieldOfViewAngle * 0.5f)
            {
                RaycastHit hit;

                // ... and if a raycast towards the player hits something...
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius ))
                {
                    // ... and if the raycast hits the player...
                    if (hit.collider.gameObject == player)
                    {
                        // ... the player is in sight.
                        playerInSight = true;

                        // Set the last global sighting is the players current position.
                        lastPlayerSighting.position = player.transform.position;
                    }
                    else
                    {
                        anim.SetBool(hash.playerInSightBool, playerInSight);
                    }
                    
                }
            }

            // Store the name hashes of the current states.
            int playerLayerZeroStateHash = playerAnim.GetCurrentAnimatorStateInfo(0).nameHash;
            int playerLayerOneStateHash = playerAnim.GetCurrentAnimatorStateInfo(1).nameHash;

            // If the player is running or is attracting attention...
            if (playerLayerZeroStateHash == hash.locomotionState || playerLayerOneStateHash == hash.shoutState)
            {
                // ... and if the player is within hearing range...
                if (CalculatePathLength(player.transform.position) <= col.radius)
                    // ... set the last personal sighting of the player to the player's current position.
                    personalLastSighting = player.transform.position;
                
    
            }
        }

        internal void Shooting()
        {
            // Stop the enemy where it is.
            Loom.QueueOnMainThread(() =>
            {
                nav.Stop();
                AnimateShooting();
            });
            
        }

        internal void AnimateShooting()
        {
            // If the last global sighting of the player has changed...
            if (lastPlayerSighting.position != previousSighting)
                // ... then update the personal sighting to be the same as the global sighting.
                personalLastSighting = lastPlayerSighting.position;

            // Set the previous sighting to the be the sighting from this frame.
            previousSighting = lastPlayerSighting.position;
            previousSightingTime = Time.realtimeSinceStartup;
            // If the player is alive...
            if (playerHealth.health > 0f)
                // ... set the animator parameter to whether the player is in sight or not.
                anim.SetBool(hash.playerInSightBool, playerInSight);
            else
                // ... set the animator parameter to false.
                anim.SetBool(hash.playerInSightBool, false);
        }

        internal bool WantsToCharge()
        {
            //ergos["charging"].Tick(charging.NeedToCharge());
            
            return charging.WantsToCharge();
        }

        internal void Recharging()
        {
            // Set an appropriate speed for the NavMeshAgent.
            Loom.QueueOnMainThread(() =>
            {
                if (nav.speed != patrolSpeed)
                    nav.speed = patrolSpeed;

                // Set the destination to the charging WayPoint.
                navDestination = charging.chargerLocation.position;

                if (nav.destination != navDestination)
                    nav.destination = navDestination;

                // If near the next waypoint or there is no destination...
                if (nav.remainingDistance < nav.stoppingDistance)
                {
                    nav.Stop();
                    // ... increment the timer.
                    //patrolTimer += Time.deltaTime;
                    Loom.RunAsync(() =>
                    {
                        charging.Charging();
                    });
                }
               
            });
        }
        
        internal void Retreating()
        {
           
        }

        

        internal void Chasing()
        {

            Loom.QueueOnMainThread(() =>
            {
                // Create a vector from the enemy to the last sighting of the player.
                Vector3 sightingDeltaPos = enemySight.personalLastSighting - transform.position;

                

                // If the the last personal sighting of the player is not close...
                if (sightingDeltaPos.sqrMagnitude > 4f)
                {
                    // ... set the destination for the NavMeshAgent to the last personal sighting of the player.
                    navDestination = enemySight.personalLastSighting;
                    if (nav.destination != navDestination)
                        nav.destination = navDestination;
                }
                // Set the appropriate speed for the NavMeshAgent.
                nav.speed = chaseSpeed;

                // If near the last personal sighting...
                if (nav.remainingDistance < nav.stoppingDistance)
                {
                    // ... increment the timer.
                    chaseTimer += Time.deltaTime;

                    // Fullfilled the need to chase player
                    ergos["chasing"].ReachedGoal();

                    // If the timer exceeds the wait time...
                    if (chaseTimer >= chaseWaitTime)
                    {
                        // ... reset last global sighting, the last personal sighting and the timer.
                        lastPlayerSighting.position = lastPlayerSighting.resetPosition;
                        enemySight.personalLastSighting = lastPlayerSighting.resetPosition;
                        chaseTimer = 0f;
                        
                    }
                }
                else
                    // If not near the last sighting personal sighting of the player, reset the timer.
                    chaseTimer = 0f;
            });
        }

        internal bool WantsToPatrol()
        {
            //ergos["patrol"].Tick(false);
            
            return ergos["patrol"].Challenge(ergos);
        }

        internal void Patrolling()
        {
            // Set an appropriate speed for the NavMeshAgent.
            Loom.QueueOnMainThread( ()=> {
            nav.speed = patrolSpeed;
            
                // If near the next waypoint or there is no destination...
                if (nav.destination == lastPlayerSighting.resetPosition || nav.remainingDistance < nav.stoppingDistance)
                {
                    
                        // ... increment the timer.
                        patrolTimer += Time.deltaTime;
                        Loom.RunAsync(() =>
                        {
                        // If the timer exceeds the wait time...
                        if (patrolTimer >= patrolWaitTime)
                        {
                            ergos["patrol"].ReachedGoal();
                            // ... increment the wayPointIndex.
                            if (wayPointIndex == patrolWayPoints.Length - 1)
                            {
                                wayPointIndex = 0;
                                

                            }
                            else
                                wayPointIndex++;

                            // Reset the timer.
                            patrolTimer = 0;
                        }
                    });
                }
                else
                    // If not near a destination, reset the timer.
                    patrolTimer = 0;

            // Set the destination to the patrolWayPoint.
                navDestination = patrolWayPoints[wayPointIndex].position;
                if (nav.destination != navDestination)
                    nav.destination = navDestination;
            });
        }


        internal void Idling()
        {

            //ergos["patrol"].Tick(true);
        }



        internal bool WantsToChase()
        { 
            return ergos["chasing"].Challenge(ergos);
        }
    }
}
