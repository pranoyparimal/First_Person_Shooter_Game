using UnityEngine;

namespace FPSGame.Player.Movement
{
    [DisallowMultipleComponent]
    public class ThirdPersonLook : LookController
    {
        [SerializeField] private MovementSettings settings = new MovementSettings();
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float distance = 4f;

        private float pitch;
        private float yaw;

        public override Transform ReferenceTransform => cameraPivot;

        private void Awake()
        {
            if (cameraPivot == null)
            {
                Debug.LogError($"{nameof(ThirdPersonLook)} requires a camera pivot transform.", this);
                return;
            }

            var euler = cameraPivot.eulerAngles;
            yaw = euler.y;
            pitch = euler.x;
            if (pitch > 180f) pitch -= 360f;
        }

        public override void ApplyLook(Vector2 lookInput)
        {
            if (cameraPivot == null) return;

            yaw += lookInput.x * settings.lookSensitivity;
            pitch -= lookInput.y * settings.lookSensitivity;
            pitch = Mathf.Clamp(pitch, settings.minPitch, settings.maxPitch);

            cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

            if (cameraTransform != null)
            {
                float targetDistance = distance;
                if (Physics.SphereCast(cameraPivot.position, 0.2f, -cameraPivot.forward, out RaycastHit hit, distance, settings.cameraObstacleLayers))
                {
                    targetDistance = hit.distance;
                }

                cameraTransform.localPosition = new Vector3(0, 0, -targetDistance);

                Vector3 worldPos = cameraTransform.position;
                if (worldPos.y < settings.cameraMinHeight)
                {
                    worldPos.y = settings.cameraMinHeight;
                    cameraTransform.position = worldPos;
                }
                else if (worldPos.y > settings.cameraMaxHeight)
                {
                    worldPos.y = settings.cameraMaxHeight;
                    cameraTransform.position = worldPos;
                }
            }
        }
    }
}
