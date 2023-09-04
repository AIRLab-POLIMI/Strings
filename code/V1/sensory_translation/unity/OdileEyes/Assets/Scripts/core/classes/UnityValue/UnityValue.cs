
using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class UnityValue<T>
{
    // - serialized object. 
    // it comes with a KEY and a value of a TYPE, a TOLERANCE and the LAST SENT VALUE. 
    // there is a method that is called every time a new val is coming. 
    // it updates the current value and checks if the difference from the LAST SENT VALUE is larger than tolerance.
    // if it is, SEND the KEY:CURRENT_VALUE pair via UDP to RASP.
    
    [SerializeField] protected StringSO key;
    [SerializeField] protected T _tolerance;
    protected T _currentValue;
    protected T _lastSentValue;

    public string Key => key.runtimeValue;
    public T CurrentValue => _currentValue;

    public string GetMsg()
    {
        // we assume that when this msg is called, the value is sent via udp, so the LAST_SENT_VALUE is updated
        _lastSentValue = _currentValue;

        return Key + KeyValueMsg.delimiter + GetFormattedValue();
    }

    public void OnNewValueRcv(T newVal)
    {
        // update the value. If the distance from the last sent value is LARGER than tolerance, 
        // send the current value via UDP
        _currentValue = newVal;
        
        if (IsLargerThanTolerance())
            UDPManager.Instance.SendStringUpdToDefaultEndpoint(GetMsg()); 
    }
    
    // used to get the formatted "currentValue". E.g. for "float" it's rounded to some decimals
    protected abstract string GetFormattedValue();
    
    // each CURRENT_VALUE type has its own implementation of the DISTANCE from TOLERANCE
    protected abstract bool IsLargerThanTolerance();
}
