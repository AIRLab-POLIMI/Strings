
using System.Collections.Generic;


namespace Oasis.GameEvents
{
    public abstract class DoubleValueGameEventSO<TInput, TTInput> : BaseGameEventSO
    {
        List<DoubleValueGameEventListener<TInput, TTInput>> _listeners = new List<DoubleValueGameEventListener<TInput, TTInput>>();

        public void Subscribe(DoubleValueGameEventListener<TInput, TTInput> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);

                // sort listeners by their PROPERTY VALUE (based on the INT underlying the enum value)
                PriorityLevelHelper.SortByPriorityLevelDescending(_listeners);
            }
        }

        public void Unsubscribe(DoubleValueGameEventListener<TInput, TTInput> listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }

        public void Invoke(TInput value1, TTInput value2)
        {
            // Debug.Log($"[ValueGameEventSO][Invoke] - VALUE: {typeof(TInput)} - CALLED with '{_listeners.Count}' listeners");
            foreach (var listener in _listeners)
                listener.RaiseEvent(value1, value2);
        }
    }
}