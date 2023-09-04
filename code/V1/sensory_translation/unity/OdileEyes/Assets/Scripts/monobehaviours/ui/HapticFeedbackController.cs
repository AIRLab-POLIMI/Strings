
using Core;
using UnityEngine;


public class HapticFeedbackController : MonoBehaviour
{
    [SerializeField] private FeedbackBoxController box;

    [SerializeField] private string touchText;
    [SerializeField] private string noTouchText;
    
[Header("touch messages")]
        
    [SerializeField] private StringSO touchMsgTouchStart;
    [SerializeField] private StringSO touchMsgTouchEnd;

    
    private void Awake()
    {
        SetBoxNoTouchActive();
    }

    public void OnMsgRcv(string msg)
    {
        if (msg == touchMsgTouchStart.runtimeValue)
            SetBoxTouchActive();
        else if (msg == touchMsgTouchEnd.runtimeValue)
            SetBoxNoTouchActive();
    }

    private void SetBoxTouchActive()
    {
        box.SetImageColor(true);
        box.SetText(touchText);
    }
    
    private void SetBoxNoTouchActive()
    {
        box.SetImageColor(false);
        box.SetText(noTouchText);
    }
    
}
