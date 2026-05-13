using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StressMonster.Emotion;
using StressMonster.Monster;

namespace StressMonster.UI
{
    public class EvolutionPanel : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private Image monsterPreview;

        [Header("Slots")]
        [SerializeField] private Image[] slotImages = new Image[4];
        [SerializeField] private TMP_Text[] featureTexts = new TMP_Text[4];
        [SerializeField] private GameObject[] slotGlowObjects = new GameObject[4];

        [Header("Stage")]
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_Text progressLabel;

        [Header("Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offScreenX = 800f;
        [SerializeField] private float glowDuration = 1.5f;
        [SerializeField] private float glowPulseSpeed = 3f;

        [Header("Emotion Colors")]
        [SerializeField] private Color angerColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color anxietyColor = new Color(0.95f, 0.85f, 0.2f);
        [SerializeField] private Color sadnessColor = new Color(0.3f, 0.5f, 0.9f);
        [SerializeField] private Color irritationColor = new Color(0.95f, 0.6f, 0.15f);

        private RectTransform _rectTransform;
        private Vector2 _hiddenPosition;
        private Vector2 _visiblePosition;
        private Coroutine _slideCoroutine;
        private Coroutine[] _glowCoroutines = new Coroutine[4];
        private bool _isVisible;

        private static readonly int[] StageThresholds = { 0, 50, 200, 500 };
        private static readonly string[] StageNames = { "幼年期", "成长期", "成熟期", "终极形态" };

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _visiblePosition = _rectTransform.anchoredPosition;
            _hiddenPosition = new Vector2(_visiblePosition.x + offScreenX, _visiblePosition.y);

            _rectTransform.anchoredPosition = _hiddenPosition;
            _isVisible = false;

            SetSlotGlowsActive(false);
        }

        public void Show(MonsterStats stats)
        {
            if (stats == null) return;

            UpdatePanel(stats);

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

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(SlideRoutine(_visiblePosition, _hiddenPosition, slideDuration));
        }

        public void PlayEvolutionUnlockAnimation(int slotIndex, string featureName, Sprite featureSprite)
        {
            if (slotIndex < 0 || slotIndex >= slotImages.Length) return;

            if (featureSprite != null && slotImages[slotIndex] != null)
                slotImages[slotIndex].sprite = featureSprite;

            if (featureTexts[slotIndex] != null)
                featureTexts[slotIndex].text = featureName;

            if (_glowCoroutines[slotIndex] != null)
                StopCoroutine(_glowCoroutines[slotIndex]);

            _glowCoroutines[slotIndex] = StartCoroutine(SlotGlowRoutine(slotIndex));
        }

        private void UpdatePanel(MonsterStats stats)
        {
            UpdateStageDisplay(stats);
            UpdateProgressDisplay(stats);
            UpdateSlotDisplay(stats);
        }

        private void UpdateStageDisplay(MonsterStats stats)
        {
            if (stageText == null) return;

            EvolutionStage stage = stats.GetCurrentStage();
            int stageIndex = (int)stage;

            if (stageIndex >= 0 && stageIndex < StageNames.Length)
                stageText.text = StageNames[stageIndex];
            else
                stageText.text = stage.ToString();
        }

        private void UpdateProgressDisplay(MonsterStats stats)
        {
            EvolutionStage currentStage = stats.GetCurrentStage();
            int stageIndex = (int)currentStage;
            int currentThreshold = stageIndex < StageThresholds.Length ? StageThresholds[stageIndex] : StageThresholds[StageThresholds.Length - 1];
            int nextThreshold = stageIndex + 1 < StageThresholds.Length ? StageThresholds[stageIndex + 1] : currentThreshold;

            float progress = 1f;
            if (nextThreshold > currentThreshold)
            {
                progress = Mathf.Clamp01((float)(stats.totalFeeds - currentThreshold) / (nextThreshold - currentThreshold));
            }

            if (progressBar != null)
                progressBar.fillAmount = progress;

            if (progressLabel != null)
            {
                if (currentStage == EvolutionStage.Ultimate)
                    progressLabel.text = "MAX";
                else
                    progressLabel.text = $"{stats.totalFeeds}/{nextThreshold}";
            }
        }

        private void UpdateSlotDisplay(MonsterStats stats)
        {
            EmotionType dominant = stats.GetDominantEmotion();
            Color emotionColor = GetEmotionColor(dominant);

            EvolutionSlot[] slots = (EvolutionSlot[])Enum.GetValues(typeof(EvolutionSlot));

            for (int i = 0; i < slotImages.Length && i < slots.Length; i++)
            {
                if (slotImages[i] != null)
                    slotImages[i].color = emotionColor;
            }

            for (int i = 0; i < featureTexts.Length && i < slots.Length; i++)
            {
                if (featureTexts[i] != null)
                    featureTexts[i].color = emotionColor;
            }
        }

        private Color GetEmotionColor(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Anger: return angerColor;
                case EmotionType.Anxiety: return anxietyColor;
                case EmotionType.Sadness: return sadnessColor;
                case EmotionType.Irritation: return irritationColor;
                default: return Color.white;
            }
        }

        private void SetSlotGlowsActive(bool active)
        {
            foreach (GameObject glow in slotGlowObjects)
            {
                if (glow != null)
                    glow.SetActive(active);
            }
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

        private IEnumerator SlotGlowRoutine(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slotGlowObjects.Length) yield break;

            GameObject glow = slotGlowObjects[slotIndex];
            if (glow == null) yield break;

            glow.SetActive(true);

            CanvasGroup glowCanvas = glow.GetComponent<CanvasGroup>();
            if (glowCanvas == null)
                glowCanvas = glow.AddComponent<CanvasGroup>();

            float elapsed = 0f;

            while (elapsed < glowDuration)
            {
                elapsed += Time.deltaTime;
                float pulse = (Mathf.Sin(elapsed * glowPulseSpeed) + 1f) * 0.5f;
                glowCanvas.alpha = pulse;
                yield return null;
            }

            glowCanvas.alpha = 0f;
            glow.SetActive(false);
            _glowCoroutines[slotIndex] = null;
        }
    }
}
