using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Virtual joystick for mobile touch controls
/// Optimized for iPhone 16 Pro touch responsiveness
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Joystick Settings")]
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private bool snapToCenter = true;
    [SerializeField] private float snapSpeed = 10f;
    
    private Image backgroundImage;
    private Image handleImage;
    private RectTransform handleTransform;
    private Vector2 inputVector;
    private Vector2 centerPosition;
    private bool isDragging = false;
    
    public System.Action<Vector2> OnValueChanged;
    public Vector2 Value => inputVector;
    
    public void Setup(Image background, Image handle, float maxDist)
    {
        backgroundImage = background;
        handleImage = handle;
        handleTransform = handle.rectTransform;
        maxDistance = maxDist;
        centerPosition = Vector2.zero;
    }
    
    private void Update()
    {
        if (!isDragging && snapToCenter)
        {
            // Smoothly return handle to center
            handleTransform.anchoredPosition = Vector2.Lerp(
                handleTransform.anchoredPosition, 
                centerPosition, 
                snapSpeed * Time.deltaTime
            );
            
            // Update input vector
            Vector2 direction = handleTransform.anchoredPosition / maxDistance;
            if (direction.magnitude < 0.1f)
            {
                inputVector = Vector2.zero;
                OnValueChanged?.Invoke(inputVector);
            }
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        OnDrag(eventData);
        
        // Haptic feedback for touch start
        #if UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
        #endif
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        
        if (snapToCenter)
        {
            inputVector = Vector2.zero;
            OnValueChanged?.Invoke(inputVector);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        
        // Clamp to max distance
        Vector2 clampedPosition = Vector2.ClampMagnitude(localPoint, maxDistance);
        handleTransform.anchoredPosition = clampedPosition;
        
        // Calculate input vector (-1 to 1 range)
        inputVector = clampedPosition / maxDistance;
        
        // Apply deadzone
        if (inputVector.magnitude < 0.2f)
        {
            inputVector = Vector2.zero;
        }
        else
        {
            // Normalize past deadzone
            inputVector = inputVector.normalized * ((inputVector.magnitude - 0.2f) / 0.8f);
        }
        
        OnValueChanged?.Invoke(inputVector);
    }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    
    public void SetOpacity(float alpha)
    {
        var bgColor = backgroundImage.color;
        bgColor.a = alpha * 0.6f;
        backgroundImage.color = bgColor;
        
        var handleColor = handleImage.color;
        handleColor.a = alpha;
        handleImage.color = handleColor;
    }
}