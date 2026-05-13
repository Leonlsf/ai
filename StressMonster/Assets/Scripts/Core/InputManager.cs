using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDragMove;
    public event Action<Vector2> OnDragEnd;
    public event Action<Vector2> OnTap;

    [Header("Settings")]
    [SerializeField] private float tapThreshold = 15f;
    [SerializeField] private float tapTimeThreshold = 0.3f;

    private bool _isDragging;
    private Vector2 _dragStartPosition;
    private float _dragStartTime;
    private bool _isMobile;

    public bool IsMobilePlatform()
    {
        return _isMobile;
    }

    public Vector2 GetInputPosition()
    {
        if (_isMobile)
        {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).position;
            return Vector2.zero;
        }
        return Input.mousePosition;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DetectPlatform();
    }

    private void DetectPlatform()
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
        _isMobile = true;
#else
        _isMobile = SystemInfo.deviceType == DeviceType.Handheld;
#endif
    }

    private void Update()
    {
        if (_isMobile)
            ProcessTouchInput();
        else
            ProcessMouseInput();
    }

    private void ProcessTouchInput()
    {
        if (Input.touchCount == 0)
        {
            if (_isDragging)
            {
                _isDragging = false;
                OnDragEnd?.Invoke(_dragStartPosition);
            }
            return;
        }

        Touch touch = Input.GetTouch(0);
        Vector2 position = touch.position;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                _isDragging = true;
                _dragStartPosition = position;
                _dragStartTime = Time.unscaledTime;
                OnDragStart?.Invoke(position);
                break;

            case TouchPhase.Moved:
                if (_isDragging)
                    OnDragMove?.Invoke(position);
                break;

            case TouchPhase.Stationary:
                if (_isDragging)
                    OnDragMove?.Invoke(position);
                break;

            case TouchPhase.Ended:
                if (_isDragging)
                {
                    _isDragging = false;
                    float distance = Vector2.Distance(position, _dragStartPosition);
                    float elapsed = Time.unscaledTime - _dragStartTime;

                    if (distance < tapThreshold && elapsed < tapTimeThreshold)
                        OnTap?.Invoke(position);
                    else
                        OnDragEnd?.Invoke(position);
                }
                break;

            case TouchPhase.Canceled:
                if (_isDragging)
                {
                    _isDragging = false;
                    OnDragEnd?.Invoke(_dragStartPosition);
                }
                break;
        }
    }

    private void ProcessMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 position = Input.mousePosition;
            _isDragging = true;
            _dragStartPosition = position;
            _dragStartTime = Time.unscaledTime;
            OnDragStart?.Invoke(position);
        }
        else if (Input.GetMouseButton(0))
        {
            if (_isDragging)
                OnDragMove?.Invoke(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging)
            {
                _isDragging = false;
                Vector2 position = Input.mousePosition;
                float distance = Vector2.Distance(position, _dragStartPosition);
                float elapsed = Time.unscaledTime - _dragStartTime;

                if (distance < tapThreshold && elapsed < tapTimeThreshold)
                    OnTap?.Invoke(position);
                else
                    OnDragEnd?.Invoke(position);
            }
        }
    }
}
