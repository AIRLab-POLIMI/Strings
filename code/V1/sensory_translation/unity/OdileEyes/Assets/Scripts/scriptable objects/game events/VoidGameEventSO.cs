
using System.Collections.Generic;
using UnityEngine;


namespace Oasis.GameEvents
{
    // THE CHANNEL
    [CreateAssetMenu(fileName = "Void Game Event", menuName = "Scriptable Objects/Game Events/Void Game Event")]
    public class VoidGameEventSO : BaseGameEventSO
    {
        List<VoidGameEventListener> _listeners = new List<VoidGameEventListener>();

        public void Subscribe(VoidGameEventListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
                
                // sort listeners by their PROPERTY VALUE (based on the INT underlying the enum value)
                PriorityLevelHelper.SortByPriorityLevelDescending(_listeners);
            }
        }
        public void Unsubscribe(VoidGameEventListener listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }
        
        public void Invoke()
        {
            foreach (var listener in _listeners) 
                listener.RaiseEvent();
        }
    }
}