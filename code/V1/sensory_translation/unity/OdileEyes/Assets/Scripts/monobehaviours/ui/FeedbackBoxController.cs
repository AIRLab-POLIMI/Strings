using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackBoxController : MonoBehaviour
{
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    
    
    public void SetText (string newText) => text.text = newText;
    
    public void SetImageColor(Color newColor) => image.color = newColor;
    
    public void SetImageColor(bool isPositive) => image.color = isPositive ? positiveColor : negativeColor;
}
