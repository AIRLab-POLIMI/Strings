
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


[RequireComponent(typeof(ActionBasedController))]
public class SphereController : MonoBehaviour
{
    [Space]
    [Header("References")] 
        [SerializeField] private SphereMeshController sphereMeshController;
        [SerializeField] private UnityValueFloat ledBrightness;
    
    [Space]
    [Header("Control Parameters")]
    
        [Tooltip("The brightness can be set either with TRIGGER BTN (if this flag is TRUE) or with GRAB (if FALSE)")]
        [SerializeField] private bool trig;

    [Space]
    [Header("Sphere Parameters")]
        [Range(0, 1)]
        [SerializeField] float minScale;
        [SerializeField] private float minBrightness;
        [SerializeField] private float maxBrightness;    
        
        [Tooltip("input values below this threshold set scale and brightness to 0. Until that values, they reach the MIN value")]
        [Range(0, 1)]
        [SerializeField] private float minThreshold;

        [SerializeField] private TextMeshProUGUI trigText;
        
    private ActionBasedController controller;
    private float _curInput;
    
    private void Start()
    {
        controller = GetComponent<ActionBasedController>();
    }

    public void OnGameStarted()
    {
        sphereMeshController.gameObject.SetActive(true);
        sphereMeshController.Init(0, 0);
    }
     
    private void Update()
    {
        OnNewInputRcv(controller.activateAction.action.ReadValue<float>());
    }

    public void OnNewInputRcv(float newInputVal)
    {
        // trigText.text = newInputVal.ToString();
        
        // get the input from the controller
        _curInput = newInputVal;
        
        // send the new value to the UnityValue
        ledBrightness.OnNewValueRcv(_curInput);
        
        // update the sphere size and emission
        SetSphere();
    }

    private void SetSphere()
    {
        // set the sphere EMISSION and SIZE based on the INPUT, in range [0, 1]
        var belowThresh = _curInput < minThreshold;

        // scale is 0 if input < minThresh, otherwise in range [minScale, 1]
        var newScale = belowThresh
            ? 0
            : MathHelper.MapRange(_curInput, 0, 1, minScale, 0.45f);
        // brightness is 0 if input < minThresh, otherwise in range [minBrightness, maxBrightness]
        var newBrightness = belowThresh
            ? 0
            : MathHelper.MapRange(_curInput, 0, 1, minBrightness, maxBrightness);
        
        sphereMeshController.OnInputChanged(newBrightness, newScale);
    }
}
