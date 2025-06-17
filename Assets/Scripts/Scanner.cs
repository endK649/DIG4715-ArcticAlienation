using UnityEngine;

public class Scanner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject watchedObject;      // Object whose inactive state we're checking
    [SerializeField] private GameObject targetToDeactivate; // Object that will be deactivated if conditions are met
    [SerializeField] private float scanRadius = 5f;
    [SerializeField] private AudioClip noti;
    [SerializeField] private AudioSource AudioSource;

    private bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;

        // Is the watched object inactive in the hierarchy?
        if (!watchedObject.activeInHierarchy)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= scanRadius && Input.GetMouseButtonDown(0))
            {
                Debug.Log("Scanner triggered: Player within range + clicked");

                if (targetToDeactivate != null)
                {
                    targetToDeactivate.SetActive(false);
                    hasTriggered = true;
                    AudioSource.PlayOneShot(noti);
                }
                else
                {
                    Debug.LogWarning("No targetToDeactivate assigned!");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}