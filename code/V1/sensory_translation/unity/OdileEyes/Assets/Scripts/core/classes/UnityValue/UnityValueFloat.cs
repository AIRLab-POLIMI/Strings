
using UnityEngine;


[System.Serializable]
public class UnityValueFloat : UnityValue<float>
{

    [Tooltip("decimals to format the float to")]
    [Range(0, 5)]
    [SerializeField] protected int decimals;
    
    protected override string GetFormattedValue()
    {
        float dec = Mathf.Pow(10, decimals);
        var a = (Mathf.Round(_currentValue * dec)/ dec).ToString();  
        // Debug.Log($" cur val: '{_currentValue}' - formatted: '{a}'");
        return a;
    }
    
    protected override bool IsLargerThanTolerance() => 
        Mathf.Abs(_currentValue - _lastSentValue) > _tolerance;
}
