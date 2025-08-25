using UnityEngine;
using DG.Tweening;

/// <summary>
/// Helper class for common DOTween animations in the side-scroller
/// </summary>
public static class DOTweenHelper
{
    /// <summary>
    /// Animate UI element entrance from the side
    /// </summary>
    public static void SlideInFromLeft(RectTransform rectTransform, float duration = 0.5f)
    {
        Vector3 startPos = rectTransform.anchoredPosition;
        startPos.x -= Screen.width;
        rectTransform.anchoredPosition = startPos;
        
        rectTransform.DOAnchorPosX(0, duration).SetEase(Ease.OutBack);
    }
    
    /// <summary>
    /// Animate UI element exit to the right
    /// </summary>
    public static void SlideOutToRight(RectTransform rectTransform, float duration = 0.3f)
    {
        rectTransform.DOAnchorPosX(Screen.width, duration).SetEase(Ease.InBack);
    }
    
    /// <summary>
    /// Pulse animation for pickups or important items
    /// </summary>
    public static void PulseLoop(Transform target, float scaleTo = 1.2f, float duration = 1f)
    {
        target.DOScale(scaleTo, duration * 0.5f)
              .SetLoops(-1, LoopType.Yoyo)
              .SetEase(Ease.InOutSine);
    }
    
    /// <summary>
    /// Screen shake effect for explosions or impacts
    /// </summary>
    public static void ScreenShake(Transform camera, float duration = 0.3f, float strength = 0.5f)
    {
        camera.DOShakePosition(duration, strength, 20, 90, false, true);
    }
    
    /// <summary>
    /// Smooth health bar animation
    /// </summary>
    public static void AnimateHealthBar(UnityEngine.UI.Image healthBar, float targetFillAmount, float duration = 0.2f)
    {
        healthBar.DOFillAmount(targetFillAmount, duration).SetEase(Ease.OutQuad);
    }
    
    /// <summary>
    /// Floating damage text animation
    /// </summary>
    public static void FloatingText(Transform textTransform, string text, Color color, float duration = 1f)
    {
        var textMesh = textTransform.GetComponent<TMPro.TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            
            // Animate upward movement and fade out
            textTransform.DOMoveY(textTransform.position.y + 2f, duration);
            textMesh.DOFade(0f, duration).OnComplete(() => 
            {
                if (textTransform != null)
                    Object.Destroy(textTransform.gameObject);
            });
        }
    }
    
    /// <summary>
    /// Enemy death animation
    /// </summary>
    public static void EnemyDeath(Transform enemy, System.Action onComplete = null)
    {
        enemy.DOScale(0f, 0.3f)
             .SetEase(Ease.InBack)
             .OnComplete(() => onComplete?.Invoke());
    }
    
    /// <summary>
    /// Smooth camera follow transition
    /// </summary>
    public static void SmoothCameraTransition(Transform camera, Vector3 targetPosition, float duration = 1f)
    {
        camera.DOMove(targetPosition, duration).SetEase(Ease.OutQuad);
    }
    
    /// <summary>
    /// Power-up collect animation
    /// </summary>
    public static void PowerUpCollect(Transform powerUp, Transform target, System.Action onComplete = null)
    {
        powerUp.DOMove(target.position, 0.5f)
               .SetEase(Ease.InQuad)
               .OnComplete(() => onComplete?.Invoke());
        
        powerUp.DOScale(0.2f, 0.5f);
    }
}