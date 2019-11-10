using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum SwipeDirection
    {
        Up,
        Right,
        Down,
        Left,
        None
    }

    [SerializeField] private Camera mainCamera;

    public static event Action<Vector2> OnTapDown;
    public static event Action<Vector2> OnTap;
    public static event Action<Vector2> OnTapUp;
    public static event Action<SwipeDirection> OnSwipe;

    public static bool IsActive = true;

    private Vector2 prevPosition;
    private Vector2 firstTapPosition;
    private Vector2 secondTapPosition;
    private Vector2 currentSwipe;

    private void Update()
    {
        if (!IsActive)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            prevPosition = Input.mousePosition;
            firstTapPosition = Input.mousePosition;
            OnTapDown?.Invoke(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        }
        else if (Input.GetMouseButton(0))
        {
            if (prevPosition.x != Input.mousePosition.x || prevPosition.y != Input.mousePosition.y)
            {
                prevPosition = Input.mousePosition;
                OnTap?.Invoke(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnTapUp?.Invoke(mainCamera.ScreenToWorldPoint(Input.mousePosition));

            secondTapPosition = Input.mousePosition;
            currentSwipe = secondTapPosition - firstTapPosition;
            currentSwipe.Normalize();

            if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                OnSwipe?.Invoke(SwipeDirection.Up);
            }
            else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                OnSwipe?.Invoke(SwipeDirection.Down);
            }
            else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                OnSwipe?.Invoke(SwipeDirection.Left);
            }
            else if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                OnSwipe?.Invoke(SwipeDirection.Right);
            }
            else
            {
                OnSwipe?.Invoke(SwipeDirection.None);
            }
        }
    }
}
