using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _characterController;

    public float MovementSpeed = 10f, RotationSpeed = 5f;

    private float _rotationY;

    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeed = 5f;

    private bool _isCrouching = false;

    public void SetCrouch(bool isCrouching)
    {
        _isCrouching = isCrouching;

        _characterController.height = isCrouching ? crouchHeight : standingHeight;
        MovementSpeed = isCrouching ? crouchSpeed : 10f; // Or cache original speed if needed
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector2 movememntVector)
    {
        Vector3 move = transform.forward * movememntVector.y + transform.right * movememntVector.x;
        move = move * MovementSpeed * Time.deltaTime;
        _characterController.Move(move);
    }

    public void Rotate(Vector2 rotationVector)
    {
        _rotationY += rotationVector.x * RotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0, _rotationY, 0);
    }
}
