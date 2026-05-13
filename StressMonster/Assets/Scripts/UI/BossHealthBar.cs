using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StressMonster.Emotion;

namespace StressMonster.UI
{
    public class BossHealthBar : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private TMP_Text bossNameText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image weaknessIcon;

        [Header("Weakness Sprites")]
        [SerializeField] private Sprite angerWeaknessSprite;
        [SerializeField] private Sprite anxietyWeaknessSprite;
        [SerializeField] private Sprite sadnessWeaknessSprite;
        [SerializeField] private Sprite irritationWeaknessSprite;

        [Header("Colors")]
        [SerializeField] private Color healthBarColor = new Color(0.85f, 0.15f, 0.15f);
        [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f);
        [SerializeField] private Color flashColor = Color.white;

        [Header("Animation")]
        [SerializeField] private float lerpDuration = 0.3f;
        [SerializeField] private float flashDuration = 0.12f;
        [SerializeField] private float lowHealthPulseSpeed = 3f;
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offScreenY = 200f;

        private RectTransform _rectTransform;
        private Vector2 _hiddenPosition;
        private Vector2 _visiblePosition;
        private Coroutine _lerpCoroutine;
        private Coroutine _flashCoroutine;
        private Coroutine _slideCoroutine;
        private Coroutine _lowHealthPulseCoroutine;

        private float _maxHealth;
        private float _currentDisplayFill;
        private float _targetFill;
        private bool _isVisible;
        private bool _isLowHealth;

        private const float LowHealthThreshold = 0.25f;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _visiblePosition = _rectTransform.anchoredPosition;
            _hiddenPosition = new Vector2(_visiblePosition.x, _visiblePosition.y + offScreenY);

            _rectTransform.anchoredPosition = _hiddenPosition;
            _isVisible = false;

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = 1f;
                healthBarFill.color = healthBarColor;
            }
        }

        public void SetBoss(string name, float maxHealth, EmotionType weakness)
        {
            _maxHealth = maxHealth;
            _currentDisplayFill = 1f;
            _targetFill = 1f;
            _isLowHealth = false;

            if (bossNameText != null)
                bossNameText.text = name;

            if (healthText != null)
                healthText.text = $"{maxHealth}/{maxHealth}";

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = 1f;
                healthBarFill.color = healthBarColor;
            }

            SetWeaknessIcon(weakness);
        }

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            _maxHealth = maxHealth;
            float newFill = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

            bool tookDamage = newFill < _targetFill;
            _targetFill = newFill;

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";

            if (tookDamage)
                PlayDamageFlash();

            if (_lerpCoroutine != null)
                StopCoroutine(_lerpCoroutine);

            _lerpCoroutine = StartCoroutine(LerpHealthRoutine());

            bool wasLowHealth = _isLowHealth;
            _isLowHealth = newFill <= LowHealthThreshold && newFill > 0f;

            if (_isLowHealth && !wasLowHealth)
                StartLowHealthPulse();
            else if (!_isLowHealth && wasLowHealth)
                StopLowHealthPulse();
        }

        public void SetHealth(float healthPercent)
        {
            float currentHealth = healthPercent * _maxHealth;
            UpdateHealth(currentHealth, _maxHealth);
        }

        public void Show()
        {
            if (_isVisible) return;
            _isVisible = true;

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(SlideRoutine(_hiddenPosition, _visiblePosition, slideDuration));
        }

        public void Hide()
        {
            if (!_isVisible) return;
            _isVisible = false;

            StopLowHealthPulse();

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(SlideRoutine(_visiblePosition, _hiddenPosition, slideDuration));
        }

        private void SetWeaknessIcon(EmotionType weakness)
        {
            if (weaknessIcon == null) return;

            Sprite sprite = GetWeaknessSprite(weakness);
            if (sprite != null)
            {
                weaknessIcon.sprite = sprite;
                weaknessIcon.gameObject.SetActive(true);
            }
            else
            {
                weaknessIcon.gameObject.SetActive(false);
            }
        }

        private Sprite GetWeaknessSprite(EmotionType weakness)
        {
            switch (weakness)
            {
                case EmotionType.Anger: return angerWeaknessSprite;
                case EmotionType.Anxiety: return anxietyWeaknessSprite;
                case EmotionType.Sadness: return sadnessWeaknessSprite;
                case EmotionType.Irritation: return irritationWeaknessSprite;
                default: return null;
            }
        }

        private void PlayDamageFlash()
        {
            if (healthBarFill == null) return;

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        private void StartLowHealthPulse()
        {
            if (_lowHealthPulseCoroutine != null)
                StopCoroutine(_lowHealthPulseCoroutine);

            _lowHealthPulseCoroutine = StartCoroutine(LowHealthPulseRoutine());
        }

        private void StopLowHealthPulse()
        {
            if (_lowHealthPulseCoroutine != null)
            {
                StopCoroutine(_lowHealthPulseCoroutine);
                _lowHealthPulseCoroutine = null;
            }

            if (healthBarFill != null && !_isLowHealth)
                healthBarFill.color = healthBarColor;
        }

        private IEnumerator LerpHealthRoutine()
        {
            if (healthBarFill == null) yield break;

            float startFill = _currentDisplayFill;
            float elapsed = 0f;

            while (elapsed < lerpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / lerpDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _currentDisplayFill = Mathf.LerpUnclamped(startFill, _targetFill, eased);
                healthBarFill.fillAmount = _currentDisplayFill;
                yield return null;
            }

            _currentDisplayFill = _targetFill;
            healthBarFill.fillAmount = _targetFill;
            _lerpCoroutine = null;
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (healthBarFill == null) yield break;

            Color original = _isLowHealth ? lowHealthColor : healthBarColor;
            healthBarFill.color = flashColor;

            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flashDuration);
                healthBarFill.color = Color.Lerp(flashColor, original, t);
                yield return null;
            }

            healthBarFill.color = original;
            _flashCoroutine = null;
        }

        private IEnumerator LowHealthPulseRoutine()
        {
            if (healthBarFill == null) yield break;

            while (_isLowHealth)
            {
                float pulse = (Mathf.Sin(Time.time * lowHealthPulseSpeed) + 1f) * 0.5f;
                healthBarFill.color = Color.Lerp(healthBarColor, lowHealthColor, pulse);
                yield return null;
            }

            healthBarFill.color = healthBarColor;
            _lowHealthPulseCoroutine = null;
        }

        private IEnumerator SlideRoutine(Vector2 from, Vector2 to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                yield return null;
            }

            _rectTransform.anchoredPosition = to;
            _slideCoroutine = null;
        }
    }
}
