using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    public float MovementSpeed = 3f, RotationSpeed = 5f;

    private float _rotationY;

    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1.5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float fallSpeed = 0f;
    private float _defaultSpeed; // Stores original standing movement speed

    private bool _isCrouching = false;

    public void SetCrouch(bool isCrouching)
    {
        _isCrouching = isCrouching;

        _characterController.height = isCrouching ? crouchHeight : standingHeight;
        MovementSpeed = isCrouching ? crouchSpeed : _defaultSpeed; // Or cache original speed if needed
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _defaultSpeed = MovementSpeed; // Cache the initial movement speed
    }

    public void Move(Vector2 movememntVector)
    {
        Vector3 move = transform.forward * movememntVector.y + transform.right * movememntVector.x;
        move = move * MovementSpeed * Time.deltaTime;
        // Apply gravity
        if (_characterController.isGrounded)
        {
            fallSpeed = 0f; // Reset fall speed when grounded
        }
        else
        {
            fallSpeed -= gravity * Time.deltaTime; // Apply gravity
        }

        move.y = fallSpeed * Time.deltaTime; // Add vertical movement

        _characterController.Move(move);
    }

    public void Rotate(Vector2 rotationVector)
    {
        _rotationY += rotationVector.x * RotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0, _rotationY, 0);
    }
}
