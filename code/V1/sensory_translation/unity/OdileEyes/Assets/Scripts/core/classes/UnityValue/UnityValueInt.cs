using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnityValueInt : UnityValue<int>
{
    protected override string GetFormattedValue() => 
        _currentValue.ToString();

    protected override bool IsLargerThanTolerance() => 
        Mathf.Abs(_currentValue - _lastSentValue) > _tolerance;
}