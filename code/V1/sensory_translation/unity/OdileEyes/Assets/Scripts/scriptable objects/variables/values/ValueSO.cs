
using UnityEngine;


public abstract class ValueSO<T> : CallbackScriptableObject
{
    [SerializeField] private T startValue;

    // [HideInInspector]
    public T runtimeValue;

    public bool initOutside;

    
    public override void OnMonoBehaviourStart()
    {
        // Debug.Log($"[ValueSO][OnMonoBehaviourStart] - started!!");
        
        if (!initOutside)
            runtimeValue = startValue;
    }
}
