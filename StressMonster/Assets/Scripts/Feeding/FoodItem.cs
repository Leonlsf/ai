using System;
using System.Collections;
using UnityEngine;
using StressMonster.Emotion;

namespace StressMonster.Feeding
{
    public class FoodItem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private string stressWord;
        [SerializeField] private EmotionType emotionType;
        [SerializeField] private Sprite foodSprite;
        [SerializeField] private float pointValue = 10f;
        [SerializeField] private bool isDraggable = true;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float floatAmplitude = 0.3f;
        [SerializeField] private float floatSpeed = 2f;

        public string StressWord => stressWord;
        public EmotionType EmotionType => emotionType;
        public Sprite FoodSprite => foodSprite;
        public float PointValue => pointValue;
        public bool IsDraggable => isDraggable;

        public event Action<FoodItem> OnFoodEaten;
        public event Action<FoodItem> OnFoodMissed;

        private Vector3 _basePosition;
        private bool _isFloating = true;
        private bool _isBeingDragged;
        private Coroutine _shrinkCoroutine;
        private Coroutine _fallCoroutine;
        private Coroutine _returnCoroutine;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            _basePosition = transform.position;
        }

        private void Update()
        {
            if (_isFloating && !_isBeingDragged)
            {
                Vector3 pos = _basePosition;
                pos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.position = pos;
            }
        }

        public void Initialize(string word, EmotionType emotion, Sprite sprite)
        {
            stressWord = word;
            emotionType = emotion;
            foodSprite = sprite;

            if (spriteRenderer != null && sprite != null)
                spriteRenderer.sprite = sprite;

            _basePosition = transform.position;
        }

        public void OnDragStart()
        {
            if (!isDraggable) return;

            _isBeingDragged = true;
            _isFloating = false;
            transform.localScale = Vector3.one * 1.1f;
        }

        public void OnDragEnd()
        {
            _isBeingDragged = false;
            _isFloating = true;
            _basePosition = transform.position;
            transform.localScale = Vector3.one;
        }

        public void OnEaten()
        {
            _isFloating = false;
            _isBeingDragged = false;

            if (_shrinkCoroutine != null)
                StopCoroutine(_shrinkCoroutine);

            _shrinkCoroutine = StartCoroutine(ShrinkAndDestroyRoutine());
        }

        public void OnMissed()
        {
            _isFloating = false;
            _isBeingDragged = false;

            if (_fallCoroutine != null)
                StopCoroutine(_fallCoroutine);

            _fallCoroutine = StartCoroutine(FallAndDestroyRoutine());
        }

        public void ReturnToOriginalPosition(Vector3 originalPosition)
        {
            _isFloating = false;
            _isBeingDragged = false;

            if (_returnCoroutine != null)
                StopCoroutine(_returnCoroutine);

            _returnCoroutine = StartCoroutine(ReturnToPositionRoutine(originalPosition));
        }

        public void SetBasePosition(Vector3 position)
        {
            _basePosition = position;
        }

        public void SetDraggable(bool draggable)
        {
            isDraggable = draggable;
        }

        private IEnumerator ShrinkAndDestroyRoutine()
        {
            float elapsed = 0f;
            float duration = 0.2f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            OnFoodEaten?.Invoke(this);
            Destroy(gameObject);
        }

        private IEnumerator FallAndDestroyRoutine()
        {
            float fallSpeed = 5f;
            float offScreenY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, -0.1f, 0f)).y;

            while (transform.position.y > offScreenY)
            {
                Vector3 pos = transform.position;
                pos.y -= fallSpeed * Time.deltaTime;
                fallSpeed += 9.8f * Time.deltaTime;
                transform.position = pos;
                yield return null;
            }

            OnFoodMissed?.Invoke(this);
            Destroy(gameObject);
        }

        private IEnumerator ReturnToPositionRoutine(Vector3 targetPosition)
        {
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);
                transform.position = Vector3.Lerp(startPos, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
            _basePosition = targetPosition;
            _isFloating = true;
            _returnCoroutine = null;
        }
    }
}
