using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Touch button for mobile controls with visual feedback
/// Optimized for iPhone 16 Pro responsiveness
/// </summary>
public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Settings")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressAlpha = 0.5f;
    [SerializeField] private bool hapticFeedback = true;
    
    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    
    public System.Action OnPointerDown;
    public System.Action OnPointerUp;
    
    public void Setup(Image image)
    {
        buttonImage = image;
        originalScale = transform.localScale;
        originalColor = buttonImage.color;
    }
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        // Visual feedback
        transform.localScale = originalScale * pressScale;
        
        var pressedColor = originalColor;
        pressedColor.a = pressAlpha;
        buttonImage.color = pressedColor;
        
        // Haptic feedback
        if (hapticFeedback)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
            #endif
        }
        
        OnPointerDown?.Invoke();
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        // Restore visual state
        transform.localScale = originalScale;
        buttonImage.color = originalColor;
        
        OnPointerUp?.Invoke();
    }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    
    public void SetOpacity(float alpha)
    {
        var color = originalColor;
        color.a = alpha;
        buttonImage.color = color;
        originalColor = color;
    }
}