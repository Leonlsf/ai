using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using StressMonster.Emotion;

namespace StressMonster.UI
{
    public class EmotionSelector : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private TMP_Text stressWordText;

        [Header("Buttons")]
        [SerializeField] private Button[] emotionButtons = new Button[4];
        [SerializeField] private Image[] emotionIcons = new Image[4];

        [Header("Colors")]
        [SerializeField] private Color angerColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color anxietyColor = new Color(0.95f, 0.85f, 0.2f);
        [SerializeField] private Color sadnessColor = new Color(0.3f, 0.5f, 0.9f);
        [SerializeField] private Color irritationColor = new Color(0.95f, 0.6f, 0.15f);

        [Header("Animation")]
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float bounceScale = 1.15f;
        [SerializeField] private float bounceDuration = 0.15f;

        [Header("Auto Detect")]
        [SerializeField] private EmotionKeywordDB keywordDB;

        private static readonly EmotionType[] EmotionTypes = { EmotionType.Anger, EmotionType.Anxiety, EmotionType.Sadness, EmotionType.Irritation };
        private static readonly string[] EmotionLabels = { "愤怒", "焦虑", "悲伤", "烦躁" };
        private static readonly string[] EmotionEmoji = { "😠", "😰", "😢", "😤" };

        private Action<EmotionType> _onSelectedCallback;
        private Coroutine _fadeOutCoroutine;
        private Coroutine[] _bounceCoroutines = new Coroutine[4];
        private bool _isShowing;

        private void Awake()
        {
            for (int i = 0; i < emotionButtons.Length && i < EmotionTypes.Length; i++)
            {
                int index = i;
                emotionButtons[i].onClick.AddListener(() => OnEmotionButtonClicked(index));

                EmotionButtonHover hover = emotionButtons[i].gameObject.GetComponent<EmotionButtonHover>();
                if (hover == null)
                    hover = emotionButtons[i].gameObject.AddComponent<EmotionButtonHover>();

                hover.Initialize(emotionButtons[i].transform, bounceScale, bounceDuration);
            }

            ApplyButtonColors();

            if (panel != null)
                panel.SetActive(false);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < emotionButtons.Length; i++)
            {
                if (emotionButtons[i] != null)
                    emotionButtons[i].onClick.RemoveAllListeners();
            }
        }

        public void Show(string stressWord, Action<EmotionType> onSelected)
        {
            if (keywordDB != null)
            {
                EmotionType detected = keywordDB.GetEmotionType(stressWord);
                if (detected != EmotionType.Irritation || IsWordInDB(stressWord))
                {
                    onSelected?.Invoke(detected);
                    return;
                }
            }

            _onSelectedCallback = onSelected;
            _isShowing = true;

            if (stressWordText != null)
                stressWordText.text = stressWord;

            if (panel != null)
                panel.SetActive(true);

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f;
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            if (!_isShowing) return;

            if (_fadeOutCoroutine != null)
                StopCoroutine(_fadeOutCoroutine);

            _fadeOutCoroutine = StartCoroutine(FadeOutRoutine());
        }

        private void OnEmotionButtonClicked(int index)
        {
            if (index < 0 || index >= EmotionTypes.Length) return;

            EmotionType selected = EmotionTypes[index];
            Action<EmotionType> callback = _onSelectedCallback;
            _onSelectedCallback = null;

            Hide();
            callback?.Invoke(selected);
        }

        private void ApplyButtonColors()
        {
            Color[] colors = { angerColor, anxietyColor, sadnessColor, irritationColor };

            for (int i = 0; i < emotionButtons.Length && i < colors.Length; i++)
            {
                if (emotionButtons[i] == null) continue;

                ColorBlock cb = emotionButtons[i].colors;
                cb.normalColor = colors[i];
                cb.highlightedColor = Color.Lerp(colors[i], Color.white, 0.3f);
                cb.pressedColor = Color.Lerp(colors[i], Color.black, 0.2f);
                cb.selectedColor = colors[i];
                emotionButtons[i].colors = cb;
            }
        }

        private bool IsWordInDB(string word)
        {
            if (keywordDB == null) return false;

            var allKeywords = keywordDB.GetAllKeywords();
            return allKeywords != null && allKeywords.Contains(word);
        }

        private IEnumerator FadeOutRoutine()
        {
            if (panelCanvasGroup != null)
            {
                float elapsed = 0f;
                float startAlpha = panelCanvasGroup.alpha;

                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;

                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                    panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                    yield return null;
                }

                panelCanvasGroup.alpha = 0f;
            }

            _isShowing = false;

            if (panel != null)
                panel.SetActive(false);

            _fadeOutCoroutine = null;
        }

        private class EmotionButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
        {
            private Transform _target;
            private float _bounceScale;
            private float _bounceDuration;
            private Vector3 _originalScale;
            private Coroutine _bounceCoroutine;

            public void Initialize(Transform target, float scale, float duration)
            {
                _target = target;
                _bounceScale = scale;
                _bounceDuration = duration;
                _originalScale = target.localScale;
            }

            private void Awake()
            {
                _originalScale = transform.localScale;
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                PlayBounce(_bounceScale);
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                PlayBounce(1f);
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                PlayBounce(_bounceScale * 0.9f);
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                PlayBounce(1f);
            }

            private void PlayBounce(float targetScale)
            {
                if (_bounceCoroutine != null)
                    StopCoroutine(_bounceCoroutine);

                _bounceCoroutine = StartCoroutine(BounceRoutine(targetScale));
            }

            private IEnumerator BounceRoutine(float targetScaleMultiplier)
            {
                Transform t = _target != null ? _target : transform;
                Vector3 targetScale = _originalScale * targetScaleMultiplier;
                float elapsed = 0f;

                while (elapsed < _bounceDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / _bounceDuration);
                    t.localScale = Vector3.LerpUnclamped(t.localScale, targetScale, progress * 0.5f + 0.5f);
                    yield return null;
                }

                t.localScale = targetScale;
                _bounceCoroutine = null;
            }
        }
    }
}
