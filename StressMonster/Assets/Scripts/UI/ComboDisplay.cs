using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StressMonster.UI
{
    public class ComboDisplay : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private Image comboBar;

        [Header("Milestone Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color milestone3Color = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color milestone6Color = new Color(1f, 0.5f, 0.1f);
        [SerializeField] private Color milestone10Color = new Color(1f, 0.2f, 0.2f);

        [Header("Multiplier Colors")]
        [SerializeField] private Color multiplierNormalColor = Color.white;
        [SerializeField] private Color multiplierMilestoneColor = new Color(1f, 0.9f, 0.2f);

        [Header("Animation")]
        [SerializeField] private float punchScale = 1.4f;
        [SerializeField] private float punchDuration = 0.25f;
        [SerializeField] private float colorTransitionDuration = 0.3f;
        [SerializeField] private float screenFlashDuration = 0.15f;

        [Header("Screen Flash")]
        [SerializeField] private Image screenFlashOverlay;
        [SerializeField] private Color flashColor3 = new Color(1f, 0.85f, 0.2f, 0.2f);
        [SerializeField] private Color flashColor6 = new Color(1f, 0.5f, 0.1f, 0.3f);
        [SerializeField] private Color flashColor10 = new Color(1f, 0.2f, 0.2f, 0.4f);

        [Header("Low Combo Pulse")]
        [SerializeField] private float lowComboPulseSpeed = 2f;

        private int _currentCombo;
        private float _currentMultiplier;
        private Coroutine _punchCoroutine;
        private Coroutine _colorTransitionCoroutine;
        private Coroutine _flashCoroutine;
        private Vector3 _originalComboScale;
        private bool _isActive;

        private static readonly int[] MilestoneThresholds = { 3, 6, 10 };

        private void Awake()
        {
            if (comboText != null)
                _originalComboScale = comboText.transform.localScale;

            _isActive = false;
            SetDisplayActive(false);
        }

        public void UpdateCombo(int combo, float multiplier, float timerPercent)
        {
            int previousCombo = _currentCombo;
            _currentCombo = combo;
            _currentMultiplier = multiplier;

            if (combo <= 0)
            {
                ResetDisplay();
                return;
            }

            if (!_isActive)
            {
                _isActive = true;
                SetDisplayActive(true);
            }

            UpdateComboText(combo);
            UpdateMultiplierText(multiplier);
            UpdateComboBar(timerPercent);
            UpdateComboColor(combo);

            if (combo > previousCombo)
            {
                PlayPunchAnimation();
                CheckMilestoneFlash(previousCombo, combo);
            }
        }

        public void ResetDisplay()
        {
            _currentCombo = 0;
            _currentMultiplier = 0f;
            _isActive = false;

            if (comboText != null)
            {
                comboText.transform.localScale = _originalComboScale;
                comboText.color = normalColor;
                comboText.text = string.Empty;
            }

            if (multiplierText != null)
            {
                multiplierText.color = multiplierNormalColor;
                multiplierText.text = string.Empty;
            }

            if (comboBar != null)
                comboBar.fillAmount = 0f;

            SetDisplayActive(false);
        }

        private void UpdateComboText(int combo)
        {
            if (comboText != null)
                comboText.text = combo.ToString();
        }

        private void UpdateMultiplierText(float multiplier)
        {
            if (multiplierText == null) return;

            if (multiplier <= 1f)
            {
                multiplierText.text = string.Empty;
                return;
            }

            int intMult = Mathf.RoundToInt(multiplier);
            multiplierText.text = $"x{intMult}";

            multiplierText.color = intMult >= 2 ? multiplierMilestoneColor : multiplierNormalColor;
        }

        private void UpdateComboBar(float timerPercent)
        {
            if (comboBar != null)
                comboBar.fillAmount = Mathf.Clamp01(timerPercent);
        }

        private void UpdateComboColor(int combo)
        {
            Color targetColor = GetMilestoneColor(combo);

            if (_colorTransitionCoroutine != null)
                StopCoroutine(_colorTransitionCoroutine);

            _colorTransitionCoroutine = StartCoroutine(ColorTransitionRoutine(targetColor));
        }

        private Color GetMilestoneColor(int combo)
        {
            if (combo >= 10) return milestone10Color;
            if (combo >= 6) return milestone6Color;
            if (combo >= 3) return milestone3Color;
            return normalColor;
        }

        private void PlayPunchAnimation()
        {
            if (_punchCoroutine != null)
                StopCoroutine(_punchCoroutine);

            _punchCoroutine = StartCoroutine(PunchScaleRoutine());
        }

        private void CheckMilestoneFlash(int previousCombo, int currentCombo)
        {
            foreach (int threshold in MilestoneThresholds)
            {
                if (previousCombo < threshold && currentCombo >= threshold)
                {
                    PlayScreenFlash(threshold);
                    break;
                }
            }
        }

        private void PlayScreenFlash(int milestone)
        {
            if (screenFlashOverlay == null) return;

            Color flashColor = GetFlashColor(milestone);

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(ScreenFlashRoutine(flashColor));
        }

        private Color GetFlashColor(int milestone)
        {
            if (milestone >= 10) return flashColor10;
            if (milestone >= 6) return flashColor6;
            return flashColor3;
        }

        private void SetDisplayActive(bool active)
        {
            if (comboText != null) comboText.gameObject.SetActive(active);
            if (multiplierText != null) multiplierText.gameObject.SetActive(active);
            if (comboBar != null) comboBar.gameObject.SetActive(active);
        }

        private IEnumerator PunchScaleRoutine()
        {
            if (comboText == null) yield break;

            Transform t = comboText.transform;
            float elapsed = 0f;
            float halfDuration = punchDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / halfDuration);
                float scale = Mathf.Lerp(1f, punchScale, progress);
                t.localScale = _originalComboScale * scale;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / halfDuration);
                float scale = Mathf.Lerp(punchScale, 1f, progress);
                t.localScale = _originalComboScale * scale;
                yield return null;
            }

            t.localScale = _originalComboScale;
            _punchCoroutine = null;
        }

        private IEnumerator ColorTransitionRoutine(Color targetColor)
        {
            if (comboText == null) yield break;

            Color startColor = comboText.color;
            float elapsed = 0f;

            while (elapsed < colorTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / colorTransitionDuration);
                comboText.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            comboText.color = targetColor;
            _colorTransitionCoroutine = null;
        }

        private IEnumerator ScreenFlashRoutine(Color flashCol)
        {
            if (screenFlashOverlay == null) yield break;

            screenFlashOverlay.color = flashCol;
            screenFlashOverlay.gameObject.SetActive(true);

            float elapsed = 0f;

            while (elapsed < screenFlashDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / screenFlashDuration);
                Color c = Color.Lerp(flashCol, Color.clear, t);
                screenFlashOverlay.color = c;
                yield return null;
            }

            screenFlashOverlay.color = Color.clear;
            screenFlashOverlay.gameObject.SetActive(false);
            _flashCoroutine = null;
        }
    }
}
