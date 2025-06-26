using System;
using System.Collections;
using UnityEngine;

// Attach this script to a canva parent to the image gameobject window to open and close
public class OpenCloseWindow : MonoBehaviour
{
    [Header("Window Setup")]
    [SerializeField] private GameObject window;

    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private CanvasGroup windowCanvasGroup;

    // Direction of animation is declared in inspector
    public enum AnimateDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }

    [Header("Animation Setup")]
    [SerializeField] private AnimateDirection openDirection = AnimateDirection.Top;
    [SerializeField] private AnimateDirection closeDirection = AnimateDirection.Bottom;
    [Space]

    // x=leftOffset; x=-rightOffset; y=upOffset; y=-bottomOffset
    [SerializeField] private Vector2 distanceToAnimate = new Vector2(100, 100);

    // Possible to chose a different type of easing curve, visualize on Easing.net 
    [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Range(0, 1f)] // Comment over the Range to allow very slow animation
    [SerializeField] private float animationDuration = 0.5f;

    private bool _isOpen;
    [SerializeField]// !:this serializefield may be the cause of a bug during closing
    private Vector2 _initialPosition;
    [SerializeField]
    private Vector2 _currentPosition;
    [SerializeField]
    private Vector2 targetPosition;

    private Vector2 _upOffset;
    private Vector2 _bottomOffset;
    private Vector2 _leftOffset;
    private Vector2 _rightOffset;

    // Coroutine is stored to ensure a second one in not overwriting the currently playing one
    private Coroutine _animateWindowCoroutine;

    // Create empty events to subsribe to if needed
    public static event Action OnOpenWindow;
    public static event Action OnCloseWindow;

    private void OnValidate()
    {
        if (window != null)
        {
            windowRectTransform = window.GetComponent<RectTransform>();
            windowCanvasGroup = window.GetComponent<CanvasGroup>();
        }

        // Animation distance is >= 0 
        distanceToAnimate.x = Math.Max(0, distanceToAnimate.x);
        distanceToAnimate.y = Math.Max(0, distanceToAnimate.y);

        _initialPosition = window.transform.position;
    }
    void Start()
    {
        _initialPosition = window.transform.position;

        InitializeOffsetPositions();
    }

    [ContextMenu("Toggle Open Close")]
    public void ToggleOpenClose()
    {
        if (_isOpen)
            CloseWindow();
        else
            OpenWindow();
    }

    [ContextMenu("Open Window")]
    public void OpenWindow()
    {
        if (_isOpen)
            return;

        _isOpen = true;
        // if OnOpenWindow is not null, invoke the static event
        OnOpenWindow?.Invoke();
        // Stops any other animateWindow coroutine already ongoing
        if (_animateWindowCoroutine != null)
            StopCoroutine(_animateWindowCoroutine);

        _animateWindowCoroutine = StartCoroutine(AnimateWindow(open: true));
    }

    [ContextMenu("Close Window")]
    public void CloseWindow()
    {
        if (!_isOpen)
            return;

        _isOpen = false;
        // if OnCloseWindow is not null, invoke the static event
        OnCloseWindow?.Invoke();
        // Stops any other animateWindow coroutine already ongoing
        if (_animateWindowCoroutine != null)
            StopCoroutine(_animateWindowCoroutine);

        _animateWindowCoroutine = StartCoroutine(AnimateWindow(open: false));
    }

    private IEnumerator AnimateWindow(bool open)
    {
        if (open)
            window.SetActive(true);

        _currentPosition = window.transform.position;

        float elapsedTime = 0;

        if (open)
            targetPosition = _currentPosition + getOffset(openDirection);
        else
            targetPosition = _currentPosition + getOffset(closeDirection);

        while (elapsedTime < animationDuration)
        {
            float animationCurrentTime = easingCurve.Evaluate(elapsedTime / animationDuration);

            // Transition window position from current to target position
            window.transform.position = Vector2.Lerp(_currentPosition, targetPosition, animationCurrentTime);

            // Change opacity alpha of the window depending on bool open
            if (open)
                windowCanvasGroup.alpha = Mathf.Lerp(0f, 1f, animationCurrentTime);
            else
                windowCanvasGroup.alpha = Mathf.Lerp(1f, 0f, animationCurrentTime);

            elapsedTime += Time.unscaledDeltaTime;

            yield return null;
        }

        // Make sure the window is at the target position (prevent issues from floats)
        window.transform.position = targetPosition;

        // Make sure the window opacity is at the desired state (prevent issues from floats)
        windowCanvasGroup.alpha = open ? 1 : 0;

        // Render the window interactable or not at the end of the animation
        windowCanvasGroup.interactable = open;
        windowCanvasGroup.blocksRaycasts = open;

        // Deactivate and Reset position of the window at the end of the closing animation
        if (!open)
        {
            window.gameObject.SetActive(false);
            window.transform.position = _initialPosition;
        }
    }

    

    // Initializes the values with the window when it is set in the inspector
    private void InitializeOffsetPositions()
    {
        _upOffset = new Vector2(0, distanceToAnimate.y);
        _bottomOffset = new Vector2(0, -distanceToAnimate.y);
        _leftOffset = new Vector2(distanceToAnimate.x, 0);
        _rightOffset = new Vector2(-distanceToAnimate.x, 0);
    }

    private Vector2 getOffset(AnimateDirection direction)
    {
        switch (direction)
        {
            case AnimateDirection.Top:
                return _upOffset;
            case AnimateDirection.Bottom:
                return _bottomOffset;
            case AnimateDirection.Left:
                return _leftOffset;
            case AnimateDirection.Right:
                return _rightOffset;
            default:
                return Vector3.zero;
        }
    }

}
