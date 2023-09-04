
using Core;
using UnityEngine;


public class BtnFeedbackController : MonoBehaviour
{
    [SerializeField] private FeedbackBoxController boxPrefab;

[Header("button messages")]
    
    [SerializeField] private StringSO btnMsgWrongBtn;
    [SerializeField] private StringSO btnMsgRightBtn;

[Header("UI")]
    
    [SerializeField] private string wrongBtnText;
    [SerializeField] private string rightBtnText;

    
    public void OnMsgRcv(string msg)
    {
        if (msg == btnMsgWrongBtn.runtimeValue)
            OnWrongBtnReceived();
        else if (msg == btnMsgRightBtn.runtimeValue)
            OnRightBtnReceived();
    }
    
    private void OnWrongBtnReceived()
    {
        var newBox = InstantiateBox();
        newBox.SetImageColor(false);
        newBox.SetText(wrongBtnText);
    }

    private void OnRightBtnReceived()
    {
        var newBox = InstantiateBox();
        newBox.SetImageColor(true);
        newBox.SetText(rightBtnText);
    }

    // instantiate a gameobject from the boxPrefab as a child to this object and extract the FeedbackBoxController component from it
    private FeedbackBoxController InstantiateBox() => 
        Instantiate(boxPrefab, transform);
}
