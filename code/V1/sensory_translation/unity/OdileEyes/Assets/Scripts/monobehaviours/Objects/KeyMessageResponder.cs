
using Oasis.GameEvents;
using UnityEngine;


public abstract class KeyMessageResponder : KeyValueGameEventListener
{
    [Space]

    [SerializeField] protected StringSO dofKey;
    
    public void OnKeyValueMsgReceived(KeyValueMsg keyValueMsg)
    {
        // Debug.Log($"[KeyMessageResponder][OnKeyValueMsgReceived] - k: {keyValueMsg.key} - sv: {keyValueMsg.stringValue} / MY key: {dofKey.runtimeValue} / same Key? {dofKey.runtimeValue == keyValueMsg.key}");
        
        if (dofKey.runtimeValue == keyValueMsg.key)
        {
            if (keyValueMsg.success)
                MessageResponse(keyValueMsg.value);
            
            StringMessageResponse(keyValueMsg.stringValue);
        }
    }
    
    protected abstract void MessageResponse(float val);

    protected abstract void StringMessageResponse(string val);
}
