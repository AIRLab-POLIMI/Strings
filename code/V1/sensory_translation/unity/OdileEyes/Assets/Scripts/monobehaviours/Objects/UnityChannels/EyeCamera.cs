
using TMPro;
using UnityEngine;


public class EyeCamera : MonoBehaviour
{
    [SerializeField] private UnityValueFloat eyeX;
    [SerializeField] private UnityValueFloat eyeY;

    [SerializeField] private TextMeshProUGUI textX;
    [SerializeField] private TextMeshProUGUI textY;
    
    private void Update()
    {
        // check the ROTATION on X and Y axes, and try to update the UNITY VALUE FLOAT.
        // the UnityValues will do the check internally and SEND the current value via udp if 
        // the value is larger than the last one sent by more than TOLERANCE.
        //
        // the X axis has the rotation around Y in unity, and viceversa
        //
        // since they are Euler Angles, they will do strange things after full circles are made. 
        // in order to always get the "true" offset from the 0 angle, we use "DeltaAngle"
        OnNewEyeXValueRcv(Mathf.DeltaAngle(0, -transform.eulerAngles.y));
        // textX.text = transform.eulerAngles.x.ToString();
        OnNewEyeYValueRcv(Mathf.DeltaAngle(0, transform.eulerAngles.x));
        // textY.text = transform.eulerAngles.y.ToString();
    }
    
    public void OnNewEyeXValueRcv(float newVal) => eyeX.OnNewValueRcv(newVal);
    public void OnNewEyeYValueRcv(float newVal) => eyeY.OnNewValueRcv(newVal);
}
