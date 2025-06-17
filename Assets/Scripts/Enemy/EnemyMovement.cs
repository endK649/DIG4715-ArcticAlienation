using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyMovement : MonoBehaviour
{
    // Reference to the NavMeshAgent component
    private NavMeshAgent agent;

    // Patrol points for enemy movement
    [SerializeField] private GameObject[] patrolPts;
    private GameObject currentPt;
    private int currentIndex = 0;

    // Player detection and reaction settings
    public bool playerDetected = false;
    public int reactTime = 0;
    private bool chasingPlayer = false; // Tracks if enemy is actively chasing
    private bool isPatroling = true;

    // Reference to the player GameObject
    [SerializeField] private GameObject player;

    // Rotation parameters for turning toward the player
    [SerializeField] private float rotationSpeed = 5f; // Adjust rotation speed

    // Footstep Audio
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioSource VocalAudioSource;

    [SerializeField] private AudioClip stepClip;

    // Growl and Roar
    [SerializeField] private AudioClip roarClip;
    [SerializeField] private AudioClip growlClip;
    private bool hasRoared = false;

    private Vector3 lastStepPosition;
    [SerializeField] private float stepDistanceThreshold = 1.5f; // Tune this to match stride size


    private void Start()
    {
        // Initialize the NavMeshAgent and set the first patrol point
        agent = GetComponent<NavMeshAgent>();
        currentPt = patrolPts[currentIndex];
        isPatroling = true;

        StartPatrolling(); // Ensure enemy starts patrolling immediately
    }

    private void Update()
    {
        if (playerDetected && !chasingPlayer && isPatroling) // Transition to chase mode only once
        {
            VocalAudioSource.PlayOneShot(growlClip);
            StopAllCoroutines(); // Prevents stacked behaviors
            StopPatrolling(); // Halt movement immediately
            StartCoroutine(HesitateBeforeChasing()); // Adds hesitation before chasing
        }
        else if (!playerDetected && chasingPlayer) // Player lost AFTER chase
        {
            chasingPlayer = false; // Unlock chase mode for patrol transitions
            StartCoroutine(HesitateBeforeResumingPatrol()); // Hesitate before returning to patrol
        }
        // Patrol logic remains active when NOT in chase mode
        if (!playerDetected && !chasingPlayer)
        {
            EnemyPatrol(); // Ensure patrolling continues normally
        }
        if (playerDetected && chasingPlayer)
        {
            PlayFootstepIfMoved();
            ChasePlayer();
        }
        PlayFootstepIfMoved();


    }

    /// <summary>
    /// Adds hesitation before initiating chase mode.
    /// </summary>
    private IEnumerator HesitateBeforeChasing()
    {
        isPatroling = false;
        // Debug.Log($"Hesitating before chasing for {reactTime} seconds...");
        yield return new WaitForSeconds(GenerateRandomInt(reactTime));

        // Debug.Log("Chase initiated!");
        chasingPlayer = true; // Set chase state AFTER hesitation delay
        
    }


    /// <summary>
    /// Adds hesitation before resuming patrol mode.
    /// </summary>
    private IEnumerator HesitateBeforeResumingPatrol()
    {
        // Debug.Log($"Hesitating before resuming patrol for 3 seconds...");
        yield return new WaitForSeconds(3);
        // Debug.Log("Hesitation over. Returning to patrol...");
        StartCoroutine(ReturnToPatrolAfterDelay());
    }

    /// <summary>
    /// Returns to patrol mode after a set delay.
    /// </summary>
    private IEnumerator ReturnToPatrolAfterDelay()
    {
        // Debug.Log("Final delay before patrol restart: 2 seconds...");
        yield return new WaitForSeconds(2f);
        //Debug.Log("Resuming patrol.");
        isPatroling = true;
        StartPatrolling();
    }

    /// <summary>
    /// Handles enemy movement between patrol points.
    /// </summary>
    private void EnemyPatrol()
    {
        agent.SetDestination(currentPt.transform.position); // Move toward current patrol point

        // Check if the enemy has reached the patrol point and switch to the next one
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            ChangePoint();
        }
    }

    /// <summary>
    /// Updates the patrol point index and selects the next patrol destination.
    /// </summary>
    private void ChangePoint()
    {
        int previousIndex = currentIndex; // Save current index to prevent repeats

        // Keep selecting a random patrol point until it's different from the previous one
        while (currentIndex == previousIndex && patrolPts.Length > 1)
        {
            currentIndex = UnityEngine.Random.Range(0, patrolPts.Length);
        }

        currentPt = patrolPts[currentIndex]; // Assign new patrol point
        agent.SetDestination(currentPt.transform.position); // Move to the new random patrol point
    }


    /// <summary>
    /// Initiates chase behavior toward the player's position when detected.
    /// </summary>
    private void ChasePlayer()
    {
        agent.isStopped = false;
        if (!hasRoared)
        {
            VocalAudioSource.PlayOneShot(roarClip);
            hasRoared = true;
        }

        agent.SetDestination(player.transform.position);
    }

    /// <summary>
    /// Rotates the enemy toward the player's position.
    /// </summary>
    /// <param name="playerTransform">The transform of the player.</param>
    public void RotateTowardPlayer(Transform playerTransform)
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Lock rotation to the Y-axis
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// Stops enemy movement but allows rotation.
    /// </summary>
    private void StopPatrolling()
    {
        agent.isStopped = true;
    }

    /// <summary>
    /// Resumes enemy movement and patrol behavior.
    /// </summary>
    private void StartPatrolling()
    {
            agent.isStopped = false;
            EnemyPatrol(); // Ensure enemy continues moving
    }
    /// <summary>
    /// Generates a random integer between the specified min and max values.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (exclusive).</param>
    /// <returns>A randomly generated integer.</returns>
    public int GenerateRandomInt(int reactTime)
    {
        return Random.Range(1, reactTime); // Unity's built-in random generator
    }
    void PlayFootstepIfMoved()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastStepPosition);
        if (distanceMoved >= stepDistanceThreshold)
        {
            AudioSource.PlayOneShot(stepClip);
            lastStepPosition = transform.position;
        }
    }


}