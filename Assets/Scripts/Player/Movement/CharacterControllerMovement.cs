using UnityEngine;

namespace FPSGame.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerMovement : PlayerMovement
    {
        [SerializeField] private MovementSettings ccSettings = new MovementSettings();
        
        private CharacterController characterController;
        private Vector3 velocity;
        private Vector3 targetMoveVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public override void Move(Vector2 input, bool sprinting, Transform referenceTransform, bool rotateToMovement = false)
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

            var speed = sprinting ? ccSettings.sprintSpeed : ccSettings.moveSpeed;
            targetMoveVelocity = moveDirection * speed;

            if (rotateToMovement && moveDirection.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }

        public override void Jump()
        {
            if (characterController.isGrounded)
            {
                // Calculate jump velocity from jumpForce using classic physics equation
                // v = sqrt(-2 * gravity * height)
                // We use ccSettings.jumpForce as a height proxy for CharacterController
                velocity.y = Mathf.Sqrt(ccSettings.jumpForce * -2f * Physics.gravity.y);
            }
        }

        protected override void FixedUpdate()
        {
            // Do nothing in FixedUpdate for CC, we use Update
        }

        private void Update()
        {
            if (characterController == null) return;

            // Apply horizontal acceleration
            Vector3 currentHorizontal = new Vector3(velocity.x, 0, velocity.z);
            Vector3 desiredHorizontal = new Vector3(targetMoveVelocity.x, 0, targetMoveVelocity.z);
            Vector3 newHorizontal = Vector3.MoveTowards(
                currentHorizontal, 
                desiredHorizontal, 
                ccSettings.acceleration * Time.deltaTime
            );

            velocity.x = newHorizontal.x;
            velocity.z = newHorizontal.z;

            // Apply Gravity
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            
            velocity.y += Physics.gravity.y * Time.deltaTime;

            // Apply movement
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
