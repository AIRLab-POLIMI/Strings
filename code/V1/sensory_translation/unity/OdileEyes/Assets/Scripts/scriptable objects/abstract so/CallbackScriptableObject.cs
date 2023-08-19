
using UnityEngine;

public abstract class CallbackScriptableObject : ScriptableObject, IPrioritized
{
    // A BIT OF A HACK
    // All the SOs of this type will be gathered by the 
    // ScriptableObjectManager MonoBehaviour in the scene,
    // which will call each of these methods in every one of its corresponding MonoBehaviour event function 
    // for each of the SOs of this type, collected though "FindObjectOfTypeAll"

    // Also, the order of execution will depend on the priority level. 
    // For this simple implementation, hte priority is global for the WHOLE CallbackSO, meaning it will influence the
    // order of execution of all the callbacks in the same way.
   
    
    [SerializeField] private PriorityLevel priorityLevel = PriorityLevel.Low;
    public PriorityLevel Priority => priorityLevel;

    public virtual void OnMonoBehaviourAwake() {}
    public virtual void OnMonoBehaviourEnable() {}
    public virtual void OnMonoBehaviourStart() {}
    public virtual void OnMonoBehaviourDisable() {}
}
