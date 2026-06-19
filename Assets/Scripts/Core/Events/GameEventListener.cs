using UnityEngine;
using UnityEngine.Events;

namespace FPSGame.Core.Events
{
    /// <summary>
    /// Attach this MonoBehaviour to any GameObject that needs to react to a GameEvent.
    /// Wire up the response in the Unity Inspector using the UnityEvent field.
    /// For example: attach to a "Game Over" UI panel, assign the OnPlayerDied GameEvent,
    /// and set the Response to call SetActive(true) on the panel.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("The ScriptableObject GameEvent asset to listen for.")]
        public GameEvent Event;

        [Tooltip("The actions to perform when the event is raised. Configure in the Inspector.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            if (Event != null)
            {
                Event.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (Event != null)
            {
                Event.UnregisterListener(this);
            }
        }

        /// <summary>
        /// Called by the GameEvent when it is raised. Invokes all configured responses.
        /// </summary>
        public void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}
