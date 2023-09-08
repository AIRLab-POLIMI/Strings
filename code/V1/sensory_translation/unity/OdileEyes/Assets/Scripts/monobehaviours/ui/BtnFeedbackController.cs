
using System.Collections.Generic;
using UnityEngine;


public class BtnFeedbackController : MonoBehaviour
{
    [SerializeField] private FeedbackBoxController boxPrefab;

    private List<string> _receivedBtnsRight = new List<string>();
    private List<string> _receivedBtnsWrong = new List<string>();
    
    private List<FeedbackBoxController> _boxesRight = new List<FeedbackBoxController>();
    private List<FeedbackBoxController> _boxesWrong = new List<FeedbackBoxController>();


    public void OnRightBtnMsgRcv(string msg) => 
        OnMsgReceived(_boxesRight, _receivedBtnsRight, msg, true);
    
    public void OnWrongBtnMsgRcv(string msg) => 
        OnMsgReceived(_boxesWrong, _receivedBtnsWrong, msg, false);
    
    
    private void OnMsgReceived(List<FeedbackBoxController> boxes, List<string> receivedBtns, string msg, bool rightBtn)
    {
        Debug.Log($"[BtnFeedbackController][OnMsgRcv] - msg: {msg} - right btn? {rightBtn}");
        
        if (receivedBtns.Contains(msg))
        {   
            foreach (var box in boxes)
            {
                if (msg == box.Id)
                    box.Highlight();
            }
            return;
        }
    
        FeedbackBoxController newBox = InstantiateBox();
        newBox.Init(msg);
        boxes.Add(newBox);
        
        newBox.SetImageColor(rightBtn);
        newBox.SetText(msg);
        
        receivedBtns.Add(msg);
    }
    
    // instantiate a gameobject from the boxPrefab as a child to this object and extract the FeedbackBoxController component from it
    private FeedbackBoxController InstantiateBox() => 
        Instantiate(boxPrefab, transform);
}
