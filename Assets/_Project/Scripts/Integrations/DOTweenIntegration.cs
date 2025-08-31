using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace WWIII.Integrations
{
    /// <summary>
    /// DOTween integration for all game animations and juice
    /// Follows project rule: "DOTween Pro for all tweens/juice"
    /// </summary>
    public static class DOTweenIntegration
    {
        /// <summary>
        /// Initialize DOTween with WWIII project settings
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // Configure DOTween for mobile performance
            DOTween.Init(true, true, LogBehaviour.ErrorsOnly)
                .SetCapacity(200, 50); // Tweeners, Sequences

            // Optimize for mobile battery life
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultUpdateType = UpdateType.Normal;
            DOTween.defaultTimeScaleIndependent = false;

            Debug.Log("✅ DOTween initialized for WWIII SideScroller");
        }

        #region Player Animation Juice

        /// <summary>
        /// Character landing juice effect
        /// </summary>
        public static void PlayerLandingJuice(Transform player)
        {
            var sequence = DOTween.Sequence();
            
            // Quick squash and stretch
            sequence.Append(player.DOScaleY(0.8f, 0.1f).SetEase(Ease.OutQuart))
                   .Append(player.DOScaleY(1.1f, 0.15f).SetEase(Ease.OutBounce))
                   .Append(player.DOScaleY(1f, 0.1f).SetEase(Ease.InOutQuart));

            sequence.Play();
        }

        /// <summary>
        /// Character jump anticipation
        /// </summary>
        public static void PlayerJumpAnticipation(Transform player)
        {
            var sequence = DOTween.Sequence();
            
            // Quick crouch before jump
            sequence.Append(player.DOScaleY(0.7f, 0.08f).SetEase(Ease.OutQuart))
                   .Append(player.DOScaleY(1.2f, 0.12f).SetEase(Ease.OutQuart))
                   .Append(player.DOScaleY(1f, 0.1f).SetEase(Ease.InOutQuart));

            sequence.Play();
        }

        /// <summary>
        /// Player damage hit reaction
        /// </summary>
        public static void PlayerHitReaction(Transform player, SpriteRenderer spriteRenderer)
        {
            var sequence = DOTween.Sequence();
            
            // Flash red and shake using material color (Unity 6 compatible)
            Color originalColor = spriteRenderer.color;
            sequence.Append(DOTween.To(() => spriteRenderer.color, x => spriteRenderer.color = x, Color.red, 0.1f))
                   .Join(player.DOShakePosition(0.2f, 0.3f, 20, 90, false, true))
                   .Append(DOTween.To(() => spriteRenderer.color, x => spriteRenderer.color = x, originalColor, 0.1f));

            sequence.Play();
        }

        #endregion

        #region UI Animation Effects

        /// <summary>
        /// Button press juice effect
        /// </summary>
        public static void ButtonPressJuice(Transform button)
        {
            var sequence = DOTween.Sequence();
            
            sequence.Append(button.DOScale(0.95f, 0.05f).SetEase(Ease.OutQuart))
                   .Append(button.DOScale(1.05f, 0.1f).SetEase(Ease.OutBack))
                   .Append(button.DOScale(1f, 0.05f).SetEase(Ease.InOutQuart));

            sequence.Play();
        }

        /// <summary>
        /// Collectible pickup animation
        /// </summary>
        public static void CollectiblePickup(Transform collectible, System.Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            var spriteRenderer = collectible.GetComponent<SpriteRenderer>();
            
            // Scale up, move up, and fade out using alpha
            sequence.Append(collectible.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack))
                   .Join(collectible.DOLocalMoveY(collectible.localPosition.y + 2f, 0.5f).SetEase(Ease.OutQuart))
                   .Join(DOTween.To(() => spriteRenderer.color.a, 
                        x => spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, x), 
                        0f, 0.3f).SetDelay(0.2f))
                   .OnComplete(() => {
                       collectible.gameObject.SetActive(false);
                       onComplete?.Invoke();
                   });

            sequence.Play();
        }

        /// <summary>
        /// Level transition wipe effect
        /// </summary>
        public static void LevelTransitionWipe(CanvasGroup transitionPanel, System.Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Fade in, pause, fade out using alpha property
            sequence.Append(DOTween.To(() => transitionPanel.alpha, x => transitionPanel.alpha = x, 1f, 0.3f).SetEase(Ease.OutQuart))
                   .AppendInterval(0.5f)
                   .Append(DOTween.To(() => transitionPanel.alpha, x => transitionPanel.alpha = x, 0f, 0.3f).SetEase(Ease.InQuart))
                   .OnComplete(() => onComplete?.Invoke());

            sequence.Play();
        }

        #endregion

        #region Environmental Effects

        /// <summary>
        /// Platform moving animation
        /// </summary>
        public static Tween MovingPlatform(Transform platform, Vector3 targetPosition, float duration)
        {
            return platform.DOMove(targetPosition, duration)
                          .SetEase(Ease.InOutSine)
                          .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// Checkpoint activation effect
        /// </summary>
        public static void CheckpointActivation(Transform checkpoint, SpriteRenderer flagSprite)
        {
            var sequence = DOTween.Sequence();
            Color originalColor = flagSprite.color;
            
            // Flag wave and glow using color property
            sequence.Append(checkpoint.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f))
                   .Join(DOTween.To(() => flagSprite.color, x => flagSprite.color = x, Color.green, 0.2f))
                   .Append(DOTween.To(() => flagSprite.color, x => flagSprite.color = x, originalColor, 0.3f));

            sequence.Play();
        }

        /// <summary>
        /// Enemy death animation
        /// </summary>
        public static void EnemyDeathAnimation(Transform enemy, System.Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Spin and shrink
            sequence.Append(enemy.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutQuart))
                   .Join(enemy.DOScale(0f, 0.5f).SetEase(Ease.InBack))
                   .OnComplete(() => {
                       enemy.gameObject.SetActive(false);
                       onComplete?.Invoke();
                   });

            sequence.Play();
        }

        #endregion

        #region Narrative Effects

        /// <summary>
        /// Dialog text typewriter effect (placeholder until TMPro is properly referenced)
        /// </summary>
        public static void DialogTypewriter(UnityEngine.UI.Text textComponent, string fullText, float duration = 1f)
        {
            // Placeholder for TMPro integration
            if (textComponent != null)
            {
                textComponent.text = fullText;
                Debug.Log($"⚠️ TMPro typewriter effect pending: {fullText}");
            }
        }

        /// <summary>
        /// Grand video reveal animation
        /// </summary>
        public static void GrandVideoReveal(Transform videoPanel, System.Action onComplete = null)
        {
            var sequence = DOTween.Sequence();
            
            // Dramatic entrance
            videoPanel.localScale = Vector3.zero;
            
            sequence.Append(videoPanel.DOScale(1.2f, 0.5f).SetEase(Ease.OutBack))
                   .Append(videoPanel.DOScale(1f, 0.2f).SetEase(Ease.InOutQuart))
                   .AppendInterval(0.3f)
                   .OnComplete(() => onComplete?.Invoke());

            sequence.Play();
        }

        #endregion

        /// <summary>
        /// Kill all tweens for cleanup
        /// </summary>
        public static void KillAllTweens()
        {
            DOTween.KillAll();
            Debug.Log("🧹 All DOTween animations killed");
        }

        /// <summary>
        /// Pause all tweens (for game pause)
        /// </summary>
        public static void PauseAllTweens()
        {
            DOTween.PauseAll();
        }

        /// <summary>
        /// Resume all tweens (unpause game)
        /// </summary>
        public static void ResumeAllTweens()
        {
            DOTween.PlayAll();
        }
    }
}