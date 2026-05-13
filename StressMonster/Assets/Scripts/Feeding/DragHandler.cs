using System.Collections;
using UnityEngine;
using StressMonster.Emotion;
using StressMonster.Core;
using StressMonster.Monster;

namespace StressMonster.Feeding
{
    public class DragHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private MonsterController monsterController;
        [SerializeField] private Transform mouthTarget;

        [Header("Settings")]
        [SerializeField] private float mouthRadius = 80f;
        [SerializeField] private float glowIntensity = 0.5f;
        [SerializeField] private Color glowColor = new Color(1f, 0.9f, 0.3f, 1f);
        [SerializeField] private float mouthOpenScale = 1.3f;

        private FoodItem _currentDragFood;
        private Vector3 _foodOriginalPosition;
        private bool _isDragging;
        private Camera _mainCamera;
        private SpriteRenderer _currentFoodRenderer;
        private Color _originalFoodColor;
        private Vector3 _originalMouthScale;
        private bool _isNearMouth;

        public float MouthRadius => mouthRadius;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (inputManager == null)
                inputManager = FindObjectOfType<InputManager>();

            if (monsterController == null)
                monsterController = FindObjectOfType<MonsterController>();

            if (mouthTarget == null && monsterController != null)
                mouthTarget = monsterController.mouthPosition;
        }

        private void OnEnable()
        {
            if (inputManager != null)
            {
                inputManager.OnDragStart += HandleDragStart;
                inputManager.OnDragMove += HandleDragMove;
                inputManager.OnDragEnd += HandleDragEnd;
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.OnDragStart -= HandleDragStart;
                inputManager.OnDragMove -= HandleDragMove;
                inputManager.OnDragEnd -= HandleDragEnd;
            }
        }

        public void SetMouthTarget(Transform mouthTransform)
        {
            mouthTarget = mouthTransform;
        }

        public void SetMouthRadius(float radius)
        {
            mouthRadius = radius;
        }

        private void HandleDragStart(Vector2 screenPosition)
        {
            if (_isDragging) return;

            Vector3 worldPos = ScreenToWorldPoint(screenPosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit == null) return;

            FoodItem food = hit.GetComponent<FoodItem>();
            if (food == null || !food.IsDraggable) return;

            _currentDragFood = food;
            _foodOriginalPosition = food.transform.position;
            _isDragging = true;
            _isNearMouth = false;

            _currentFoodRenderer = food.GetComponent<SpriteRenderer>();
            if (_currentFoodRenderer != null)
                _originalFoodColor = _currentFoodRenderer.color;

            if (mouthTarget != null)
                _originalMouthScale = mouthTarget.localScale;

            food.OnDragStart();
        }

        private void HandleDragMove(Vector2 screenPosition)
        {
            if (!_isDragging || _currentDragFood == null) return;

            Vector3 worldPos = ScreenToWorldPoint(screenPosition);
            _currentDragFood.transform.position = worldPos;

            UpdateProximityFeedback();
        }

        private void HandleDragEnd(Vector2 screenPosition)
        {
            if (!_isDragging || _currentDragFood == null) return;

            _isDragging = false;

            if (IsNearMouth(screenPosition))
            {
                FeedMonster();
            }
            else
            {
                ResetProximityFeedback();
                _currentDragFood.ReturnToOriginalPosition(_foodOriginalPosition);
            }

            _currentDragFood.OnDragEnd();
            _currentDragFood = null;
            _currentFoodRenderer = null;
        }

        private void FeedMonster()
        {
            if (monsterController != null)
                monsterController.Feed(_currentDragFood.EmotionType);

            _currentDragFood.OnEaten();
            ResetProximityFeedback();
        }

        private bool IsNearMouth(Vector2 screenPosition)
        {
            if (mouthTarget == null) return false;

            Vector2 mouthScreenPos = _mainCamera.WorldToScreenPoint(mouthTarget.position);
            float distance = Vector2.Distance(screenPosition, mouthScreenPos);
            return distance <= mouthRadius;
        }

        private void UpdateProximityFeedback()
        {
            if (mouthTarget == null || _currentDragFood == null) return;

            Vector2 foodScreenPos = _mainCamera.WorldToScreenPoint(_currentDragFood.transform.position);
            Vector2 mouthScreenPos = _mainCamera.WorldToScreenPoint(mouthTarget.position);
            float distance = Vector2.Distance(foodScreenPos, mouthScreenPos);
            bool wasNearMouth = _isNearMouth;
            _isNearMouth = distance <= mouthRadius;

            if (_currentFoodRenderer != null)
            {
                if (_isNearMouth)
                {
                    float t = 1f - Mathf.Clamp01(distance / mouthRadius);
                    Color glow = Color.Lerp(_originalFoodColor, glowColor, t * glowIntensity);
                    _currentFoodRenderer.color = glow;
                }
                else
                {
                    _currentFoodRenderer.color = _originalFoodColor;
                }
            }

            if (mouthTarget != null)
            {
                if (_isNearMouth)
                {
                    float t = 1f - Mathf.Clamp01(distance / mouthRadius);
                    Vector3 targetScale = _originalMouthScale * Mathf.Lerp(1f, mouthOpenScale, t);
                    mouthTarget.localScale = targetScale;
                }
                else if (wasNearMouth)
                {
                    mouthTarget.localScale = _originalMouthScale;
                }
            }
        }

        private void ResetProximityFeedback()
        {
            if (_currentFoodRenderer != null)
                _currentFoodRenderer.color = _originalFoodColor;

            if (mouthTarget != null)
                mouthTarget.localScale = _originalMouthScale;

            _isNearMouth = false;
        }

        private Vector3 ScreenToWorldPoint(Vector2 screenPosition)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _mainCamera.nearClipPlane));
            worldPos.z = 0f;
            return worldPos;
        }
    }
}
