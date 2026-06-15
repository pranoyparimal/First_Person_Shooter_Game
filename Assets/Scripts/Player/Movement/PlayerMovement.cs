using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MovementSettings settings = new MovementSettings();
    [SerializeField] private Rigidbody body;
    [SerializeField] private GroundChecker groundChecker;

    private Vector3 targetVelocity;

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }

        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        if (groundChecker != null)
        {
            groundChecker.Configure(settings);
        }
    }

    public void Move(Vector2 input, bool sprinting, Transform referenceTransform, bool rotateToMovement = false)
    {
        var forward = referenceTransform != null ? referenceTransform.forward : transform.forward;
        var right = referenceTransform != null ? referenceTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        var moveDirection = forward * input.y + right * input.x;
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        var speed = sprinting ? settings.sprintSpeed : settings.moveSpeed;
        targetVelocity = moveDirection * speed;

        if (rotateToMovement && moveDirection.sqrMagnitude > 0.01f)
        {
            var targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

    public void Jump()
    {
        if (groundChecker == null || !groundChecker.IsGrounded)
        {
            return;
        }

        var velocity = body.linearVelocity;
        velocity.y = 0f;
        body.linearVelocity = velocity;
        body.AddForce(Vector3.up * settings.jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        var currentVelocity = body.linearVelocity;
        var desiredVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);
        var newVelocity = Vector3.MoveTowards(
            currentVelocity,
            desiredVelocity,
            settings.acceleration * Time.fixedDeltaTime);

        body.linearVelocity = newVelocity;
    }
}
