using UnityEngine;

public abstract class LookController : MonoBehaviour
{
    public abstract Transform ReferenceTransform { get; }
    public abstract void ApplyLook(Vector2 lookInput);
}
