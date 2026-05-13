using System;
using System.Collections;
using UnityEngine;

namespace StressMonster.Feeding
{
    public class ComboSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float comboWindow = 2f;
        [SerializeField] private float screenShakeDuration = 0.3f;
        [SerializeField] private float screenShakeIntensity = 5f;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.3f);

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private SpriteRenderer screenOverlay;

        public int CurrentCombo => _currentCombo;
        public int ComboMultiplier => GetMultiplier(_currentCombo);
        public float TimerPercent => comboWindow > 0f ? 1f - (_comboTimer / comboWindow) : 0f;

        public event Action<int> OnComboChanged;
        public event Action<int> OnComboMilestone;

        private int _currentCombo;
        private float _comboTimer;
        private Coroutine _timerCoroutine;
        private Vector3 _originalCameraPosition;
        private Color _originalOverlayColor;
        private bool _hasOverlay;

        private static readonly int[] MilestoneThresholds = { 3, 6, 10 };

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            _hasOverlay = screenOverlay != null;
            if (_hasOverlay)
                _originalOverlayColor = screenOverlay.color;
        }

        public void OnFeed()
        {
            int previousCombo = _currentCombo;
            _currentCombo++;
            _comboTimer = 0f;

            RestartTimer();
            OnComboChanged?.Invoke(_currentCombo);

            CheckMilestone(previousCombo);
            ApplyComboEffects();
        }

        public void ResetCombo()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            _currentCombo = 0;
            _comboTimer = 0f;
            OnComboChanged?.Invoke(_currentCombo);
        }

        private int GetMultiplier(int combo)
        {
            if (combo >= 10) return 5;
            if (combo >= 6) return 3;
            if (combo >= 3) return 2;
            return 1;
        }

        private void RestartTimer()
        {
            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            _timerCoroutine = StartCoroutine(ComboTimerRoutine());
        }

        private IEnumerator ComboTimerRoutine()
        {
            _comboTimer = 0f;

            while (_comboTimer < comboWindow)
            {
                _comboTimer += Time.deltaTime;
                yield return null;
            }

            _currentCombo = 0;
            _comboTimer = 0f;
            OnComboChanged?.Invoke(_currentCombo);
            _timerCoroutine = null;
        }

        private void CheckMilestone(int previousCombo)
        {
            foreach (int threshold in MilestoneThresholds)
            {
                if (previousCombo < threshold && _currentCombo >= threshold)
                {
                    OnComboMilestone?.Invoke(threshold);
                    break;
                }
            }
        }

        private void ApplyComboEffects()
        {
            if (_currentCombo >= 10)
            {
                TriggerScreenShake();
                TriggerScreenFlash();
            }
            else if (_currentCombo >= 6)
            {
                TriggerParticleIncrease();
            }
            else if (_currentCombo >= 3)
            {
                TriggerEdgeGlow();
            }
        }

        private void TriggerEdgeGlow()
        {
            if (!_hasOverlay) return;

            StopCoroutine(nameof(EdgeGlowRoutine));
            StartCoroutine(EdgeGlowRoutine());
        }

        private IEnumerator EdgeGlowRoutine()
        {
            Color glowColor = new Color(1f, 0.8f, 0.2f, 0.25f);
            screenOverlay.color = glowColor;

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                screenOverlay.color = Color.Lerp(glowColor, _originalOverlayColor, t);
                yield return null;
            }

            screenOverlay.color = _originalOverlayColor;
        }

        private void TriggerParticleIncrease()
        {
            if (!_hasOverlay) return;

            StopCoroutine(nameof(ParticleGlowRoutine));
            StartCoroutine(ParticleGlowRoutine());
        }

        private IEnumerator ParticleGlowRoutine()
        {
            Color glowColor = new Color(1f, 0.6f, 0.1f, 0.35f);
            screenOverlay.color = glowColor;

            float elapsed = 0f;
            float duration = 0.6f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                screenOverlay.color = Color.Lerp(glowColor, _originalOverlayColor, t);
                yield return null;
            }

            screenOverlay.color = _originalOverlayColor;
        }

        private void TriggerScreenShake()
        {
            if (mainCamera == null) return;

            _originalCameraPosition = mainCamera.transform.localPosition;
            StopCoroutine(nameof(ScreenShakeRoutine));
            StartCoroutine(ScreenShakeRoutine());
        }

        private IEnumerator ScreenShakeRoutine()
        {
            float elapsed = 0f;

            while (elapsed < screenShakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-screenShakeIntensity, screenShakeIntensity);
                float y = Random.Range(-screenShakeIntensity, screenShakeIntensity);
                mainCamera.transform.localPosition = _originalCameraPosition + new Vector3(x, y, 0f);
                yield return null;
            }

            mainCamera.transform.localPosition = _originalCameraPosition;
        }

        private void TriggerScreenFlash()
        {
            if (!_hasOverlay) return;

            StopCoroutine(nameof(ScreenFlashRoutine));
            StartCoroutine(ScreenFlashRoutine());
        }

        private IEnumerator ScreenFlashRoutine()
        {
            screenOverlay.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            screenOverlay.color = _originalOverlayColor;
        }
    }
}
