
using System;
using UnityEngine;


public class KeyValueMsg
{
    public const char delimiter = ':';

    public string key;
    public float value;
    public string stringValue;
    public bool success;

    public KeyValueMsg(string key, float value)
    {
        this.key = key;
        this.value = value;
        this.success = true;
    }

    public KeyValueMsg(string key, string value)
    {
        this.key = key;
        this.value = StringToFloat(value, out this.success);
        this.stringValue = value;
    }
    
    public static KeyValueMsg ParseKeyValueMsg(string msg)
    {
        var keyVal = msg.Split(delimiter);
        
        // if the number of substrings is not exactly 2, it's not a key value msg, and we return NULL
        return keyVal.Length != 2
            ? null 
            : new KeyValueMsg(keyVal[0], keyVal[1]);
    }
    
    public static float StringToFloat(string msg, out bool succ)
    {
        try
        {
            succ = true;
            return float.Parse(msg);
        }
        catch (Exception e)
        {
            succ = false;
            Debug.Log($"[KEYVALUEMSG][StringToFloat] - ERROR: {e}");
            return 0;
        }
    }

}
