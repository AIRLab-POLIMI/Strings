
using UnityEngine;
using UnityEngine.Events;


public class EventKeyMessageResponder : KeyMessageResponder
{
    [SerializeField] private UnityEvent<float> _floatEventResponse;
    [SerializeField] private UnityEvent<string> _stringEventResponse;

    protected override void MessageResponse(float val) => _floatEventResponse?.Invoke(val);
    
    protected override void StringMessageResponse(string val) => _stringEventResponse?.Invoke(val);
    
}
