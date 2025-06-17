using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public float pickupRadius = 3f;
    public Transform player;

    private bool isCollected = false;
    public AudioSource AudioSource;
    public AudioClip noti;

    void Update()
    {
        if (isCollected) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pickupRadius && Input.GetMouseButtonDown(0))
        {
            CollectItem();
        }
    }

    void CollectItem()
    {
        isCollected = true;

        // Optionally play a sound or visual effect here
        AudioSource.PlayOneShot(noti);
        Debug.Log("Item collected!");

        // Hide or destroy the item
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}