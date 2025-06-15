using UnityEngine;

public class EnemyStealthVision : MonoBehaviour
{
    // Detection parameters for spotting the player
    public float detectionRadius = 10f;       // Max distance the enemy can detect the player
    public float detectionAngle = 60f;        // Field of view in degrees
    public int rayCount = 20;                 // Number of rays used for detection
    public string targetTag = "Player";       // Tag used to identify the player
    public LayerMask obstacleMask;            // Layer mask to check for obstacles blocking vision
    private float visibilityReduction = 0.5f; // Factor reducing detection (e.g., shadows, crouching)

    // Adjustable detection height offset to ensure proper raycasting height
    [SerializeField] private float raycastHeightOffset = 1.5f;

    // Reference to the player's transform
    [SerializeField] private Transform playerTransform;

    // Reference to enemy movement logic
    private EnemyMovement enemyMovement;
    public bool playerDetected = false;

    private void Start()
    {
        // Initialize movement script reference
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        // Detect player presence and update movement script accordingly
        playerDetected = DetectPlayer();
        enemyMovement.playerDetected = playerDetected;

        if (playerDetected)
        {
            // If player is detected, rotate enemy towards them
            enemyMovement.RotateTowardPlayer(playerTransform);
        }
    }

    /// <summary>
    /// Casts multiple rays to detect if the player is visible within enemy's field of view.
    /// </summary>
    /// <returns>Returns true if player is detected, false otherwise.</returns>
    private bool DetectPlayer()
    {
        Vector3 forward = transform.forward; // Enemy's current forward direction
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeightOffset; // Adjusted raycast origin

        // Cast multiple rays within detection angle range
        for (int i = 0; i < rayCount; i++)
        {
            float angle = Mathf.Lerp(-detectionAngle, detectionAngle, i / (float)(rayCount - 1));
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward; // Rotate forward direction

            if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, detectionRadius))
            {
                if (hit.collider.CompareTag(targetTag)) // Check if hit object is the player
                {
                    float adjustedRadius = detectionRadius * (IsPlayerHidden(hit.collider) ? visibilityReduction : 1f);

                    // Ensure player is not obstructed by obstacles
                    if (!Physics.Raycast(rayOrigin, hit.point, adjustedRadius, obstacleMask))
                    {
                        // Uncomment for debugging detection success
                        // Debug.DrawRay(rayOrigin, direction * hit.distance, Color.green, 0.1f);
                        return true;
                    }
                }
                else
                {
                    // Uncomment for debugging detection failure
                    // Debug.DrawRay(rayOrigin, direction * hit.distance, Color.red, 0.1f);
                }
            }
            else
            {
                // Uncomment for debugging max detection range
                // Debug.DrawRay(rayOrigin, direction * detectionRadius, Color.white, 0.1f);
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the player is in a hidden state (e.g., crouching).
    /// </summary>
    /// <param name="player">Collider of the detected player.</param>
    /// <returns>Returns true if the player is considered hidden.</returns>
    private bool IsPlayerHidden(Collider player)
    {
        return player.transform.localScale.y < 0.8f; // Simplified crouch detection logic
    }
}