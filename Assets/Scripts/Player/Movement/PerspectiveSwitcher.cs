using UnityEngine;

[DisallowMultipleComponent]
public class PerspectiveSwitcher : MonoBehaviour
{
    [SerializeField] private LookController firstPersonLook;
    [SerializeField] private LookController thirdPersonLook;

    [SerializeField] private GameObject firstPersonCamera;
    [SerializeField] private GameObject thirdPersonCamera;
    
    [SerializeField] private GameObject playerVisuals;

    public bool IsFirstPerson { get; private set; } = true;
    public LookController ActiveLookController => IsFirstPerson ? firstPersonLook : thirdPersonLook;
    public Transform ActiveCameraTransform => IsFirstPerson ? firstPersonCamera.transform : thirdPersonCamera.transform;

    private void Start()
    {
        SetPerspective(true);
    }

    public void TogglePerspective()
    {
        SetPerspective(!IsFirstPerson);
    }

    private void SetPerspective(bool firstPerson)
    {
        IsFirstPerson = firstPerson;

        if (firstPersonLook != null) firstPersonLook.enabled = firstPerson;
        if (thirdPersonLook != null) thirdPersonLook.enabled = !firstPerson;

        if (firstPersonCamera != null) firstPersonCamera.SetActive(firstPerson);
        if (thirdPersonCamera != null) thirdPersonCamera.SetActive(!firstPerson);
        
        if (playerVisuals != null) playerVisuals.SetActive(!firstPerson);
    }
}
