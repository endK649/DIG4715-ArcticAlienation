using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonCam : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] CinemachineFreeLook fpCamera;
    [SerializeField] CharacterController characterController;

    [Header("Input")]
    public Vector2 LookInput;

    [Header("Looking Parameters")]
    public Vector2 LookSens = new Vector2(0.1f, 0.1f);

    public float PitchLimit = 85f;

    [SerializeField] float currentPitch = 0f;

    [Header("Crouch Settings")]
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private float crouchOffset = -0.5f; // How much to lower the camera
    [SerializeField] private float crouchSpeed = 5f;

    private float defaultCameraY; // Original Y offset
    private Vector3 targetCameraPosition;

    private void Start()
    {
        defaultCameraY = fpCamera.transform.localPosition.y;
        targetCameraPosition = fpCamera.transform.localPosition;
    }

    public float CurrentPitch
    {
        get => currentPitch;

        set
        {
            currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit);
        }
    }

    #region Unity Methods
    private void OnValidate()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    #endregion

    private void Update()
    {
        LookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        HandleCrouch();
        LookUpdate();
    }

    void LookUpdate()
    {
        Vector2 input = new Vector2(LookInput.x * LookSens.x, LookInput.y * LookSens.y);
        // Looking up and down
        CurrentPitch -= input.y;

        fpCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);

        // Looking left and right
        transform.Rotate(Vector3.up * input.x);
    }

    private void HandleCrouch()
    {
        Vector3 currentPos = fpCamera.transform.localPosition;

        if (Input.GetKey(crouchKey))
        {
            targetCameraPosition = new Vector3(currentPos.x, defaultCameraY + crouchOffset, currentPos.z);
        }
        else
        {
            targetCameraPosition = new Vector3(currentPos.x, defaultCameraY, currentPos.z);
        }

        fpCamera.transform.localPosition = Vector3.Lerp(currentPos, targetCameraPosition, Time.deltaTime * crouchSpeed);
    }

}
