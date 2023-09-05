
using System.Collections.Generic;
using UnityEngine;


public class BtnFeedbackController : MonoBehaviour
{
    [SerializeField] private FeedbackBoxController boxPrefab;

[Header("button messages")]
    
    [SerializeField] private StringSO btnMsgRightBtn;

[Header("UI")]
    
    [SerializeField] private string wrongBtnText;
    [SerializeField] private string rightBtnText;

    private List<string> _receivedBtns = new List<string>();
    
    private List<FeedbackBoxController> _boxes = new List<FeedbackBoxController>();
    
    
    public void OnMsgRcv(string msg)
    {
        // Debug.Log($"[BtnFeedbackController][OnMsgRcv] - msg: {msg}");
        
        if (_receivedBtns.Contains(msg))
        {   
            foreach (var box in _boxes)
            {
                if (msg == box.Id)
                    box.Highlight();
            }
            return;
        }

        FeedbackBoxController newBox = InstantiateBox();
        newBox.Init(msg);
        _boxes.Add(newBox);
        
        if (msg == btnMsgRightBtn.runtimeValue)
            OnRightBtnReceived(newBox);
        else 
            OnWrongBtnReceived(newBox);
        
        _receivedBtns.Add(msg);
    }
    
    private void OnWrongBtnReceived(FeedbackBoxController newBox)
    {
        newBox.SetImageColor(false);
        newBox.SetText(wrongBtnText);
    }

    private void OnRightBtnReceived(FeedbackBoxController newBox)
    {
        newBox.SetImageColor(true);
        newBox.SetText(rightBtnText);
    }

    private void OnMsgReceived()
    {
        
    }
    
    // instantiate a gameobject from the boxPrefab as a child to this object and extract the FeedbackBoxController component from it
    private FeedbackBoxController InstantiateBox() => 
        Instantiate(boxPrefab, transform);
}
