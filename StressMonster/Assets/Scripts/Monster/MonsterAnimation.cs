using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Monster
{
    public class MonsterAnimation : MonoBehaviour
    {
        public Animator animator;
        public SpriteRenderer spriteRenderer;

        private Coroutine _bounceCoroutine;
        private Coroutine _flashCoroutine;
        private Coroutine _shakeCoroutine;
        private Vector3 _originalScale;
        private Color _originalColor;
        private Vector3 _originalPosition;

        private static readonly int HungryHash = Animator.StringToHash("Hungry");
        private static readonly int ChewingHash = Animator.StringToHash("Chewing");
        private static readonly int SatisfiedHash = Animator.StringToHash("Satisfied");
        private static readonly int BurpingHash = Animator.StringToHash("Burping");
        private static readonly int BattleModeHash = Animator.StringToHash("BattleMode");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int AngerReactHash = Animator.StringToHash("AngerReact");
        private static readonly int AnxietyReactHash = Animator.StringToHash("AnxietyReact");
        private static readonly int SadnessReactHash = Animator.StringToHash("SadnessReact");
        private static readonly int IrritationReactHash = Animator.StringToHash("IrritationReact");

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _originalScale = transform.localScale;
            _originalPosition = transform.localPosition;

            if (spriteRenderer != null)
            {
                _originalColor = spriteRenderer.color;
            }
        }

        public void PlayState(MonsterState state)
        {
            if (animator == null) return;

            ResetStateBools();

            switch (state)
            {
                case MonsterState.Hungry:
                    animator.SetBool(HungryHash, true);
                    break;
                case MonsterState.Chewing:
                    animator.SetBool(ChewingHash, true);
                    break;
                case MonsterState.Satisfied:
                    animator.SetBool(SatisfiedHash, true);
                    break;
                case MonsterState.Burping:
                    animator.SetBool(BurpingHash, true);
                    break;
                case MonsterState.BattleMode:
                    animator.SetBool(BattleModeHash, true);
                    break;
                case MonsterState.Hit:
                    animator.SetTrigger(HitHash);
                    break;
            }
        }

        private void ResetStateBools()
        {
            if (animator == null) return;

            animator.SetBool(HungryHash, false);
            animator.SetBool(ChewingHash, false);
            animator.SetBool(SatisfiedHash, false);
            animator.SetBool(BurpingHash, false);
            animator.SetBool(BattleModeHash, false);
        }

        public void PlayEmotionReaction(EmotionType emotion)
        {
            if (animator == null) return;

            switch (emotion)
            {
                case EmotionType.Anger:
                    animator.SetTrigger(AngerReactHash);
                    break;
                case EmotionType.Anxiety:
                    animator.SetTrigger(AnxietyReactHash);
                    break;
                case EmotionType.Sadness:
                    animator.SetTrigger(SadnessReactHash);
                    break;
                case EmotionType.Irritation:
                    animator.SetTrigger(IrritationReactHash);
                    break;
            }
        }

        public void PlayEvolutionAnimation()
        {
            if (_bounceCoroutine != null)
            {
                StopCoroutine(_bounceCoroutine);
            }
            _bounceCoroutine = StartCoroutine(EvolutionAnimationRoutine());
        }

        private IEnumerator EvolutionAnimationRoutine()
        {
            yield return StartCoroutine(BounceScaleRoutine(0.6f, 0.4f));

            if (spriteRenderer != null)
            {
                yield return StartCoroutine(FlashColorRoutine(Color.yellow, 0.5f));
            }

            yield return StartCoroutine(BounceScaleRoutine(0.4f, 0.3f));
        }

        public void BounceScale(float duration = 0.3f, float intensity = 0.2f)
        {
            if (_bounceCoroutine != null)
            {
                StopCoroutine(_bounceCoroutine);
                transform.localScale = _originalScale;
            }
            _bounceCoroutine = StartCoroutine(BounceScaleRoutine(duration, intensity));
        }

        private IEnumerator BounceScaleRoutine(float duration, float intensity)
        {
            float elapsed = 0f;
            Vector3 targetScale = _originalScale * (1f + intensity);

            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.5f);
                transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.5f);
                transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
                yield return null;
            }

            transform.localScale = _originalScale;
            _bounceCoroutine = null;
        }

        public void FlashColor(Color color, float duration = 0.2f)
        {
            if (spriteRenderer == null) return;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                spriteRenderer.color = _originalColor;
            }
            _flashCoroutine = StartCoroutine(FlashColorRoutine(color, duration));
        }

        private IEnumerator FlashColorRoutine(Color flashColor, float duration)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = _originalColor;
            _flashCoroutine = null;
        }

        public void Shake(float duration = 0.3f, float intensity = 5f)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                transform.localPosition = _originalPosition;
            }
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, intensity));
        }

        private IEnumerator ShakeRoutine(float duration, float intensity)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-intensity, intensity);
                float y = Random.Range(-intensity, intensity);
                transform.localPosition = _originalPosition + new Vector3(x, y, 0f);
                yield return null;
            }

            transform.localPosition = _originalPosition;
            _shakeCoroutine = null;
        }
    }
}
