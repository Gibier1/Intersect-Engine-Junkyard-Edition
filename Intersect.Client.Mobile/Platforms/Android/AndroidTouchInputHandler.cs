using System;
using System.Collections.Generic;
using Android.Views;
using Intersect.Client.Framework.GenericClasses;
using Microsoft.Xna.Framework;
using APoint = Android.Graphics.Point;

namespace Intersect.Client.Mobile.Platforms.Android;

/// <summary>
/// Android implementation of ITouchInputHandler using Android's touch API.
/// </summary>
public class AndroidTouchInputHandler : Java.Lang.Object, View.IOnTouchListener, ITouchInputHandler
{
    private readonly List<TouchPoint> _activeTouches = new();
    private readonly object _touchLock = new();

    private TouchPoint? _primaryTouch;
    private TouchPoint? _secondaryTouch;

    private long _lastTapTime;
    private APoint? _lastTapPosition;
    private const int LongPressTimeout = 500; // milliseconds
    private const int DoubleTapTimeout = 300; // milliseconds

    public AndroidTouchInputHandler(View view)
    {
        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        view.SetOnTouchListener(this);
    }

    #region ITouchInputHandler Implementation

    public Point PrimaryTouchPosition => _primaryTouch?.Position ?? new Point(0, 0);

    public bool IsTouching => _activeTouches.Count > 0;

    public int TouchCount => _activeTouches.Count;

    public event EventHandler<TouchEventArgs>? TouchStarted;
    public event EventHandler<TouchEventArgs>? TouchMoved;
    public event EventHandler<TouchEventArgs>? TouchEnded;
    public event EventHandler<TouchEventArgs>? LongPressDetected;
    public event EventHandler<TouchEventArgs>? TapDetected;
    public event EventHandler<PinchEventArgs>? PinchStarted;
    public event EventHandler<PinchEventArgs>? PinchChanged;
    public event EventHandler<PinchEventArgs>? PinchEnded;

    private bool _isPinching;

    public void Update()
    {
        lock (_touchLock)
        {
            var now = DateTime.UtcNow;
            var touchesToRemove = new List<int>();

            // Check for long press
            foreach (var touch in _activeTouches)
            {
                if (touch.IsLongPressTriggered)
                {
                    continue;
                }

                var duration = (now - touch.StartTime).TotalMilliseconds;
                if (duration >= LongPressTimeout)
                {
                    touch.IsLongPressTriggered = true;
                    var distance = Point.Distance(touch.StartPosition, touch.Position);
                    if (distance < 20) // Didn't move much
                    {
                        LongPressDetected?.Invoke(this, CreateTouchEventArgs(touch));
                    }
                }
            }

            // Check for pinch gesture
            if (_activeTouches.Count >= 2)
            {
                var touch1 = _activeTouches[0];
                var touch2 = _activeTouches[1];

                var center = new Point(
                    (touch1.Position.X + touch2.Position.X) / 2,
                    (touch1.Position.Y + touch2.Position.Y) / 2
                );

                var distance = Point.Distance(touch1.Position, touch2.Position);

                if (!_isPinching)
                {
                    _isPinching = true;
                    PinchStarted?.Invoke(this, new PinchEventArgs
                    {
                        Center = center,
                        Distance = distance,
                        PreviousDistance = distance
                    });
                }
                else
                {
                    PinchChanged?.Invoke(this, new PinchEventArgs
                    {
                        Center = center,
                        Distance = distance,
                        PreviousDistance = touch1.PreviousDistanceToTouch2
                    });
                }

                touch1.PreviousDistanceToTouch2 = distance;
            }
            else if (_isPinching)
            {
                _isPinching = false;
                PinchEnded?.Invoke(this, new PinchEventArgs
                {
                    Center = new Point(0, 0),
                    Distance = 0,
                    PreviousDistance = 0
                });
            }
        }
    }

    public void Reset()
    {
        lock (_touchLock)
        {
            _activeTouches.Clear();
            _primaryTouch = null;
            _secondaryTouch = null;
            _isPinching = false;
        }
    }

    #endregion

    #region View.IOnTouchListener Implementation

    public bool OnTouch(View? v, MotionEvent? e)
    {
        if (e == null)
        {
            return false;
        }

        var action = e.ActionMasked;
        var pointerIndex = e.ActionIndex;
        var pointerId = e.GetPointerId(pointerIndex);
        var position = new Point((int)e.GetX(pointerIndex), (int)e.GetY(pointerIndex));

        switch (action)
        {
            case MotionEventActions.Down:
            case MotionEventActions.PointerDown:
                HandleTouchDown(pointerId, position);
                return true;

            case MotionEventActions.Move:
                HandleTouchMove(e);
                return true;

            case MotionEventActions.Up:
            case MotionEventActions.PointerUp:
                HandleTouchUp(pointerId, position);
                return true;

            case MotionEventActions.Cancel:
                Reset();
                return true;
        }

        return false;
    }

    #endregion

    #region Touch Handling

    private void HandleTouchDown(int pointerId, Point position)
    {
        lock (_touchLock)
        {
            var touch = new TouchPoint
            {
                Id = pointerId,
                Position = position,
                PreviousPosition = position,
                StartPosition = position,
                StartTime = DateTime.UtcNow
            };

            _activeTouches.Add(touch);

            // Update primary/secondary touch
            if (_primaryTouch == null)
            {
                _primaryTouch = touch;
            }
            else if (_secondaryTouch == null)
            {
                _secondaryTouch = touch;
            }

            TouchStarted?.Invoke(this, CreateTouchEventArgs(touch));
        }
    }

    private void HandleTouchMove(MotionEvent e)
    {
        lock (_touchLock)
        {
            for (var i = 0; i < e.PointerCount; i++)
            {
                var pointerId = e.GetPointerId(i);
                var position = new Point((int)e.GetX(i), (int)e.GetY(i));

                var touch = FindTouch(pointerId);
                if (touch != null)
                {
                    touch.PreviousPosition = touch.Position;
                    touch.Position = position;

                    TouchMoved?.Invoke(this, CreateTouchEventArgs(touch));
                }
            }
        }
    }

    private void HandleTouchUp(int pointerId, Point position)
    {
        lock (_touchLock)
        {
            var touch = FindTouch(pointerId);
            if (touch == null)
            {
                return;
            }

            touch.Position = position;

            // Check for tap
            var duration = (DateTime.UtcNow - touch.StartTime).TotalMilliseconds;
            var distance = Point.Distance(touch.StartPosition, position);

            if (duration < LongPressTimeout && distance < 20)
            {
                // Check for double tap
                var timeSinceLastTap = duration;
                if (_lastTapPosition != null)
                {
                    timeSinceLastTap = (DateTime.UtcNow - new DateTime(_lastTapTime)).TotalMilliseconds;
                    var distanceFromLastTap = Point.Distance(
                        new Point(_lastTapPosition.X, _lastTapPosition.Y),
                        position
                    );

                    if (timeSinceLastTap < DoubleTapTimeout && distanceFromLastTap < 40)
                    {
                        // Double tap detected - send second tap
                        TapDetected?.Invoke(this, CreateTouchEventArgs(touch));
                    }
                }

                _lastTapTime = DateTime.UtcNow.Ticks;
                _lastTapPosition = new APoint(position.X, position.Y);
                TapDetected?.Invoke(this, CreateTouchEventArgs(touch));
            }

            TouchEnded?.Invoke(this, CreateTouchEventArgs(touch));

            _activeTouches.Remove(touch);

            // Update primary/secondary touch
            if (touch == _primaryTouch)
            {
                _primaryTouch = _activeTouches.Count > 0 ? _activeTouches[0] : null;
            }

            if (touch == _secondaryTouch)
            {
                _secondaryTouch = _activeTouches.Count > 1 ? _activeTouches[1] : null;
            }
        }
    }

    private TouchPoint? FindTouch(int pointerId)
    {
        foreach (var touch in _activeTouches)
        {
            if (touch.Id == pointerId)
            {
                return touch;
            }
        }
        return null;
    }

    private TouchEventArgs CreateTouchEventArgs(TouchPoint touch)
    {
        return new TouchEventArgs
        {
            TouchId = touch.Id,
            Position = touch.Position,
            PreviousPosition = touch.PreviousPosition,
            Timestamp = DateTime.UtcNow - touch.StartTime,
            Pressure = 1.0f
        };
    }

    #endregion

    private class TouchPoint
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public Point PreviousPosition { get; set; }
        public Point StartPosition { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsLongPressTriggered { get; set; }
        public float PreviousDistanceToTouch2 { get; set; }
    }
}
