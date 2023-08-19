using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoypadFeedback : MonoBehaviour
{
    [Range(1, 10)] [SerializeField] private float scaleFactor;
    [Range(1, 10)] [SerializeField] private float maxOffset;
    
    [SerializeField] public InputActionProperty moveAnimationAction;
    [SerializeField] public InputActionProperty trigAnimationAction;
    [SerializeField] public InputActionProperty gripAnimationAction;
    
    
    private float _initScale;
    private Vector3 _initPos;
    
    // Start is called before the first frame update
    void Start()
    {
        // initialize _initScale with the current scale of the object
        _initScale = transform.localScale.x;
        _initPos = transform.position;
    }
  
    // Update is called once per frame
    void Update()
    {
        // get vector 2 value from move action
        Vector2 moveValue = moveAnimationAction.action.ReadValue<Vector2>();
        // assign absolute values of moveValue.x to scale x and moveValue.y to scale y, multiplied by scaleFactor
        transform.localScale = new Vector3(_initScale + Mathf.Abs(moveValue.x) * scaleFactor, _initScale + Mathf.Abs(moveValue.y) * scaleFactor, _initScale);
        
        // get float value from pinch action
        var pinchValue = trigAnimationAction.action.ReadValue<float>();
        // get float value from grab action
        var grabValue = gripAnimationAction.action.ReadValue<float>();

        // offset initial position by pinch value and grab value on x and y axes respectively, multiplied by maxOffsetValue
        transform.position = new Vector3(
            _initPos.x,
            _initPos.y + pinchValue * maxOffset,
            _initPos.z + grabValue * maxOffset);
    }
}
