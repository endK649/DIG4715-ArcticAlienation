using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public PlayerController CharacterController;

    private InputAction _moveAction, _lookAction;

    private InputAction _crouchAction;

    private void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _lookAction = InputSystem.actions.FindAction("Look");
        _crouchAction = InputSystem.actions.FindAction("Crouch");

        _crouchAction.performed += ctx => CharacterController.SetCrouch(true);
        _crouchAction.canceled += ctx => CharacterController.SetCrouch(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 movementVector = _moveAction.ReadValue<Vector2>();
        CharacterController.Move(movementVector);

        Vector2 lookVector = _lookAction.ReadValue<Vector2>();
        CharacterController.Rotate(lookVector);
    }
}
