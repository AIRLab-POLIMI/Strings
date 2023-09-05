
using UnityEngine;


public class HapticFeedbackController : MonoBehaviour
{
    [SerializeField] private FeedbackBoxController box;

    [SerializeField] private string touchText;
    [SerializeField] private string noTouchText;
    
[Header("touch messages")]
        
    [SerializeField] private StringSO touchMsgTouchStart;
    [SerializeField] private StringSO touchMsgTouchEnd;


    private bool _prevState;
    
    
    private void Awake()
    {
        // SetBoxNoTouchActive();
        SetBoxTouchActive();
        _prevState = false;
    }

    public void OnMsgRcv(string msg)
    {
        if (msg == touchMsgTouchStart.runtimeValue)
        {
            if (!_prevState)
            {
                SetBoxTouchActive();
                _prevState = true;
            }
        }
        else if (msg == touchMsgTouchEnd.runtimeValue)
        {
            if (_prevState)
            {
                SetBoxNoTouchActive();
                _prevState = false;
            }
        }
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
