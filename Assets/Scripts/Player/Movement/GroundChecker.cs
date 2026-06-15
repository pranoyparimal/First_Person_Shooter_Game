using UnityEngine;

[DisallowMultipleComponent]
public class GroundChecker : MonoBehaviour
{
    [SerializeField] private MovementSettings settings = new MovementSettings();

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        var origin = transform.position + Vector3.up * 0.1f;
        IsGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            settings.groundCheckDistance,
            settings.groundLayers,
            QueryTriggerInteraction.Ignore);
    }

    public void Configure(MovementSettings movementSettings)
    {
        settings = movementSettings;
    }
}
