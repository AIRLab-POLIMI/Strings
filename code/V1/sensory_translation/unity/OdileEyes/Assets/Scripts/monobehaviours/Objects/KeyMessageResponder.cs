
using Oasis.GameEvents;
using UnityEngine;


public abstract class KeyMessageResponder : KeyValueGameEventListener
{
    [Space]

    [SerializeField] private StringSO dofKey;
    
    public void OnKeyValueMsgReceived(KeyValueMsg keyValueMsg)
    {
        if (dofKey.runtimeValue == keyValueMsg.key)
            MessageResponse(keyValueMsg.value);
    }
    
    protected abstract void MessageResponse(float val);

    protected abstract void StringMessageResponse(string val);
}
