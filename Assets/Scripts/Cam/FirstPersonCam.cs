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
}
