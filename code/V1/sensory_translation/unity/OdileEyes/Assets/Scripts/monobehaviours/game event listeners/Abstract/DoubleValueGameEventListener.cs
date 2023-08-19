
using UnityEngine;
using UnityEngine.Events;


namespace Oasis.GameEvents
{
    public abstract class DoubleValueGameEventListener<TInput, TTInput> : BaseGameEventListener
    {
        [SerializeField] private UnityEvent<TInput, TTInput> _unityEvent;
        [SerializeField] private DoubleValueGameEventSO<TInput, TTInput> _gameEvent;
        
        private void OnEnable() => _gameEvent.Subscribe(this);
        private void OnDisable() => _gameEvent.Unsubscribe(this);
        
        public void RaiseEvent(TInput value1, TTInput value2) => _unityEvent?.Invoke(value1, value2);
    }
}