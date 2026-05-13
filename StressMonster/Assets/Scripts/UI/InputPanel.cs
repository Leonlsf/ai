using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StressMonster.Feeding;

namespace StressMonster.UI
{
    public class InputPanel : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button randomButton;

        [Header("Generator")]
        [SerializeField] private FoodGenerator foodGenerator;

        [Header("Quick Tags")]
        [SerializeField] private Transform quickTagContainer;
        [SerializeField] private GameObject quickTagButtonPrefab;
        [SerializeField] private string[] quickTags = { "加班", "考试", "堵车", "KPI", "deadline", "失眠", "排队", "房贷", "996", "裁员" };

        [Header("Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float offScreenY = -500f;

        private RectTransform _rectTransform;
        private Vector2 _hiddenPosition;
        private Vector2 _visiblePosition;
        private Coroutine _slideCoroutine;
        private bool _isVisible;

        private const int MaxInputLength = 20;

        public event Action<string> OnStressWordSubmitted;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _visiblePosition = _rectTransform.anchoredPosition;
            _hiddenPosition = new Vector2(_visiblePosition.x, _visiblePosition.y + offScreenY);

            _rectTransform.anchoredPosition = _hiddenPosition;
            _isVisible = false;
        }

        private void Start()
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            randomButton.onClick.AddListener(OnRandomClicked);

            inputField.characterLimit = MaxInputLength;
            inputField.onSubmit.AddListener(_ => OnSubmitClicked());

            BuildQuickTags();
        }

        private void OnDestroy()
        {
            submitButton.onClick.RemoveListener(OnSubmitClicked);
            randomButton.onClick.RemoveListener(OnRandomClicked);
            inputField.onSubmit.RemoveAllListeners();
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

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(SlideRoutine(_visiblePosition, _hiddenPosition, slideDuration));
        }

        private void OnSubmitClicked()
        {
            string text = inputField.text;

            if (string.IsNullOrWhiteSpace(text))
                return;

            text = text.Trim();
            if (text.Length > MaxInputLength)
                text = text.Substring(0, MaxInputLength);

            if (foodGenerator != null)
                foodGenerator.GenerateFood(text);

            OnStressWordSubmitted?.Invoke(text);
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        private void OnRandomClicked()
        {
            if (foodGenerator != null)
                foodGenerator.GenerateRandomFood();
        }

        private void BuildQuickTags()
        {
            if (quickTagContainer == null) return;

            foreach (string tag in quickTags)
            {
                GameObject tagObj = quickTagButtonPrefab != null
                    ? Instantiate(quickTagButtonPrefab, quickTagContainer)
                    : CreateDefaultTagButton(quickTagContainer);

                Button tagButton = tagObj.GetComponent<Button>();
                if (tagButton == null)
                    tagButton = tagObj.AddComponent<Button>();

                TMP_Text label = tagObj.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = tag;

                string capturedTag = tag;
                tagButton.onClick.AddListener(() => OnQuickTagClicked(capturedTag));
            }
        }

        private GameObject CreateDefaultTagButton(Transform parent)
        {
            GameObject obj = new GameObject("QuickTag");
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80f, 36f);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);

            TMP_Text tmpText = labelObj.AddComponent<TextMeshProUGUI>();
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontSize = 18f;

            RectTransform labelRt = labelObj.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.sizeDelta = Vector2.zero;

            return obj;
        }

        private void OnQuickTagClicked(string tag)
        {
            inputField.text = tag;
            OnSubmitClicked();
        }

        private IEnumerator SlideRoutine(Vector2 from, Vector2 to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseOutCubic(t);
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                yield return null;
            }

            _rectTransform.anchoredPosition = to;
            _slideCoroutine = null;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
