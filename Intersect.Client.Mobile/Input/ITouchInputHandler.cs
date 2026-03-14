using System;
using Intersect.Client.Framework.GenericClasses;

namespace Intersect.Client.Mobile.Input;

/// <summary>
/// Defines touch input capabilities for mobile platforms.
/// Abstracts touch events to work with the existing Gwen GUI system.
/// </summary>
public interface ITouchInputHandler
{
    /// <summary>
    /// Gets the current primary touch position (equivalent to mouse position).
    /// </summary>
    Point PrimaryTouchPosition { get; }

    /// <summary>
    /// Gets whether there is an active primary touch.
    /// </summary>
    bool IsTouching { get; }

    /// <summary>
    /// Gets the current number of active touches.
    /// </summary>
    int TouchCount { get; }

    /// <summary>
    /// Occurs when a touch begins.
    /// </summary>
    event EventHandler<TouchEventArgs> TouchStarted;

    /// <summary>
    /// Occurs when a touch moves.
    /// </summary>
    event EventHandler<TouchEventArgs> TouchMoved;

    /// <summary>
    /// Occurs when a touch ends.
    /// </summary>
    event EventHandler<TouchEventArgs> TouchEnded;

    /// <summary>
    /// Occurs when a long press is detected (equivalent to right-click).
    /// </summary>
    event EventHandler<TouchEventArgs> LongPressDetected;

    /// <summary>
    /// Occurs when a tap is detected (equivalent to left-click).
    /// </summary>
    event EventHandler<TouchEventArgs> TapDetected;

    /// <summary>
    /// Occurs when a pinch gesture begins (for zoom).
    /// </summary>
    event EventHandler<PinchEventArgs> PinchStarted;

    /// <summary>
    /// Occurs when a pinch gesture changes.
    /// </summary>
    event EventHandler<PinchEventArgs> PinchChanged;

    /// <summary>
    /// Occurs when a pinch gesture ends.
    /// </summary>
    event EventHandler<PinchEventArgs> PinchEnded;

    /// <summary>
    /// Updates the touch input state. Call once per frame.
    /// </summary>
    void Update();

    /// <summary>
    /// Resets all touch state.
    /// </summary>
    void Reset();
}

/// <summary>
/// Arguments for touch events.
/// </summary>
public class TouchEventArgs : EventArgs
{
    /// <summary>
    /// The unique identifier for this touch.
    /// </summary>
    public int TouchId { get; set; }

    /// <summary>
    /// The current position of the touch.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// The previous position of the touch (for drag calculations).
    /// </summary>
    public Point PreviousPosition { get; set; }

    /// <summary>
    /// The delta since the last touch event.
    /// </summary>
    public Point Delta => new Point(Position.X - PreviousPosition.X, Position.Y - PreviousPosition.Y);

    /// <summary>
    /// The timestamp when this touch event occurred.
    /// </summary>
    public TimeSpan Timestamp { get; set; }

    /// <summary>
    /// The pressure of the touch (0.0 to 1.0), if available.
    /// </summary>
    public float Pressure { get; set; }
}

/// <summary>
/// Arguments for pinch (zoom) gestures.
/// </summary>
public class PinchEventArgs : EventArgs
{
    /// <summary>
    /// The center point of the pinch gesture.
    /// </summary>
    public Point Center { get; set; }

    /// <summary>
    /// The current distance between the two touch points.
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// The previous distance between the two touch points.
    /// </summary>
    public float PreviousDistance { get; set; }

    /// <summary>
    /// The scale factor (CurrentDistance / PreviousDistance).
    /// </summary>
    public float ScaleFactor => PreviousDistance > 0 ? Distance / PreviousDistance : 1.0f;
}
