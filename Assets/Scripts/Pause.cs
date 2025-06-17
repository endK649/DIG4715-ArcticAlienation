using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject objectToToggle; // Assign in Inspector
    public KeyCode toggleKey = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (objectToToggle != null)
            {
                objectToToggle.SetActive(!objectToToggle.activeSelf);
            }
        }
    }
}
