using System.Collections.Generic;
using UnityEngine;

namespace FPSGame.Core.Events
{
    /// <summary>
    /// A ScriptableObject-based event channel for fully decoupled communication.
    /// Create instances via: Right-click in Project → Create → Events → Game Event.
    /// Any script can Raise() it, and any GameEventListener can react to it,
    /// without either side knowing the other exists.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

        /// <summary>
        /// Fires the event, notifying all registered listeners.
        /// </summary>
        public void Raise()
        {
            // Iterate in reverse so listeners can safely unregister during the callback
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        /// <summary>
        /// Registers a listener to receive notifications when this event is raised.
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregisters a listener so it no longer receives notifications.
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}
