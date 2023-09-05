
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FeedbackBoxController : MonoBehaviour
{
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    
    public string Id { get; private set; }

    private Coroutine _highlightCoroutine;
    private Color _currentColor;
    private float _initialSize;
    private RectTransform _rectTransform;
    
    public float duration = 0.4f;
    public int repeatCount = 1;
    public int blinkCount = 5;
    public float scaleFactor = 1.5f;

    private Vector3 originalScale;
    
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        // _initialSize = _rectTransform.sizeDelta.x;
        originalScale = transform.localScale;
        
        // Highlight();
    }

    public void Init(string id) => Id = id;
    
    public void SetText (string newText) => text.text = newText;
    
    public void SetImageColor(Color newColor) => image.color = newColor;
    
    public void SetImageColor(bool isPositive)
    {
        _currentColor = isPositive ? positiveColor : negativeColor;
        image.color = _currentColor;
    }
    
    public void Highlight() {
        
        if (_highlightCoroutine != null)
            StopHighlight();
        
        _highlightCoroutine = StartCoroutine(AnimateImage());
    }

    private void StopHighlight()
    {
        // reset to initial size and colour
        SetImageColor(_currentColor);
        transform.localScale = originalScale;
        
        StopCoroutine(_highlightCoroutine);
        _highlightCoroutine = null;
    }
    
    private IEnumerator AnimateImage()
    {
        for (int i = 0; i < repeatCount; i++)
        {
            float halfDuration = duration / 2;

            // Simultaneously scale and blink for the first half of the duration
            float startTime = Time.time;
            while (Time.time - startTime < halfDuration)
            {
                float t = (Time.time - startTime) / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, t);
                float alpha = Mathf.PingPong(t * 5, 1); // Blink at 5 times per second
                Color newColor = _currentColor;
                newColor.a = alpha;
                SetImageColor(newColor);
                yield return null;
            }

            // Reset the image to its original state over the second half of the duration
            startTime = Time.time;
            while (Time.time - startTime < halfDuration)
            {
                float t = (Time.time - startTime) / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, t);
                yield return null;
            }

            // Reset the image to its original state
            transform.localScale = originalScale;
            SetImageColor(_currentColor);
        }
    }
}
