using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCem : MonoBehaviour
{
    [Header("References")]
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;


    // Use MoveCamera script in conjunction with this one
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks cursor in place
        Cursor.visible = false; // Makes cursor invisible
    }
    void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
