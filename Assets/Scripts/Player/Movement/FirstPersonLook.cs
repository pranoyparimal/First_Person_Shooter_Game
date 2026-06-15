using UnityEngine;

[DisallowMultipleComponent]
public class FirstPersonLook : LookController
{
    public override Transform ReferenceTransform => transform;

    [SerializeField] private MovementSettings settings = new MovementSettings();
    [SerializeField] private Transform cameraPitch;

    private float pitch;
    private float yaw;

    private void Awake()
    {
        if (cameraPitch == null)
        {
            Debug.LogError($"{nameof(FirstPersonLook)} requires a camera pitch transform.", this);
        }

        var euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = cameraPitch != null ? cameraPitch.localEulerAngles.x : 0f;
        if (pitch > 180f)
        {
            pitch -= 360f;
        }
    }

    public override void ApplyLook(Vector2 lookInput)
    {
        if (cameraPitch == null)
        {
            return;
        }

        yaw += lookInput.x * settings.lookSensitivity;
        pitch -= lookInput.y * settings.lookSensitivity;
        pitch = Mathf.Clamp(pitch, settings.minPitch, settings.maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraPitch.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
