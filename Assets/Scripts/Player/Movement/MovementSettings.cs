using UnityEngine;

namespace FPSGame.Player.Movement
{
    [System.Serializable]
    public class MovementSettings
    {
        [Header("Locomotion")]
        public float moveSpeed = 6f;
        public float sprintSpeed = 10f;
        public float acceleration = 20f;

        [Header("Jump")]
        public float jumpForce = 7f;

        [Header("Look")]
        public float lookSensitivity = 0.15f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Ground Check")]
        public float groundCheckDistance = 0.6f;
        public LayerMask groundLayers = ~0;

        [Header("Camera Collision")]
        public LayerMask cameraObstacleLayers = ~0;
        public float cameraMinHeight = 0.5f;
        public float cameraMaxHeight = 10f;
    }
}
