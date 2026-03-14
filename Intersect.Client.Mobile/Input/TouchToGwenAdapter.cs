using System;
using System.Collections.Generic;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Input;
using Intersect.Client.Framework.Input;
using Microsoft.Extensions.Logging;

namespace Intersect.Client.Mobile.Input;

/// <summary>
/// Adapts touch input events to Gwen GUI input events.
/// This allows the existing Gwen-based UI to work with touch input.
///
/// Updated: Now uses dedicated left/right click buttons instead of tap/long-press.
/// Touch is only used for direct UI interaction when not hitting virtual controls.
/// </summary>
public class TouchToGwenAdapter
{
    private readonly ITouchInputHandler _touchInputHandler;
    private readonly ILogger _logger;
    private readonly Dictionary<int, TouchTracker> _activeTouches = new();
    private Base? _lastHoveredControl;

    // Configuration
    private const float HoverActivationDistancePixels = 5f;

    public TouchToGwenAdapter(ITouchInputHandler touchInputHandler, ILogger logger)
    {
        _touchInputHandler = touchInputHandler ?? throw new ArgumentNullException(nameof(touchInputHandler));
        _logger = logger;

        // Subscribe to touch events
        _touchInputHandler.TouchStarted += OnTouchStarted;
        _touchInputHandler.TouchMoved += OnTouchMoved;
        _touchInputHandler.TouchEnded += OnTouchEnded;
    }

    /// <summary>
    /// Gets the simulated mouse position based on primary touch.
    /// </summary>
    public Point SimulatedMousePosition => _touchInputHandler.PrimaryTouchPosition;

    /// <summary>
    /// Updates the adapter state. Call this each frame to simulate hover behavior.
    /// </summary>
    public void Update()
    {
        // Simulate hover by moving mouse to touch position
        if (_touchInputHandler.IsTouching)
        {
            InputHandler.MousePosition.X = _touchInputHandler.PrimaryTouchPosition.X;
            InputHandler.MousePosition.Y = _touchInputHandler.PrimaryTouchPosition.Y;

            // Update hovered control
            UpdateHoveredControl();
        }
    }

    /// <summary>
    /// Handles touch events that weren't captured by virtual controls.
    /// These touches interact directly with the game UI.
    /// </summary>
    public void HandleUnhandledTouch(TouchEventArgs e)
    {
        // Update mouse position
        InputHandler.MousePosition.X = e.Position.X;
        InputHandler.MousePosition.Y = e.Position.Y;

        // Find and update hovered control
        UpdateHoveredControl();
    }

    private void OnTouchStarted(object? sender, TouchEventArgs e)
    {
        // This is called for touches not captured by virtual controls
        // Treat it as hovering the control
        InputHandler.MousePosition.X = e.Position.X;
        InputHandler.MousePosition.Y = e.Position.Y;

        var control = FindControlUnderPosition(e.Position);
        if (control != null)
        {
            // Simulate left mouse button down on the control
            InputHandler.Focus(FocusSource.Mouse, control);
        }

        _activeTouches[e.TouchId] = new TouchTracker
        {
            TouchId = e.TouchId,
            StartTime = DateTime.UtcNow,
            StartPosition = e.Position,
            CurrentPosition = e.Position,
            Control = control
        };

        _logger.LogTrace("Touch started (unhandled): {TouchId} at ({X}, {Y})", e.TouchId, e.Position.X, e.Position.Y);
    }

    private void OnTouchMoved(object? sender, TouchEventArgs e)
    {
        if (!_activeTouches.TryGetValue(e.TouchId, out var tracker))
        {
            return;
        }

        tracker.CurrentPosition = e.Position;

        // Simulate mouse move
        InputHandler.MousePosition.X = e.Position.X;
        InputHandler.MousePosition.Y = e.Position.Y;

        UpdateHoveredControl();
    }

    private void OnTouchEnded(object? sender, TouchEventArgs e)
    {
        if (!_activeTouches.TryGetValue(e.TouchId, out var tracker))
        {
            return;
        }

        // Simulate mouse button up
        InputHandler.Focus(FocusSource.Mouse, null);

        // If we were on a control and didn't move much, trigger a click
        var distance = Point.Distance(tracker.StartPosition, e.Position);
        var duration = (DateTime.UtcNow - tracker.StartTime).TotalMilliseconds;

        if (tracker.Control != null && distance < 20 && duration < 500)
        {
            // It's a tap on a control - trigger clicked
            if (tracker.Control is IClickable clickable)
            {
                clickable.InvokeClicked();
            }
        }

        _activeTouches.Remove(e.TouchId);
        _logger.LogTrace("Touch ended (unhandled): {TouchId}", e.TouchId);
    }

    private void UpdateHoveredControl()
    {
        var control = FindControlUnderPosition(InputHandler.MousePosition);
        InputHandler.HoveredControl = control;
        _lastHoveredControl = control;
    }

    private Base? FindControlUnderPosition(Point position)
    {
        try
        {
            return Interface.Interface.FindComponentUnderCursor(
                new Framework.Gwen.Control.NodeFilter(
                    (in Base baseControl) =>
                    {
                        // Filter for visible, enabled controls that accept mouse input
                        return baseControl is { IsHidden: false, IsDisabled: false, MouseInputEnabled: true };
                    }
                )
            );
        }
        catch
        {
            return null;
        }
    }

    private class TouchTracker
    {
        public int TouchId { get; set; }
        public DateTime StartTime { get; set; }
        public Point StartPosition { get; set; }
        public Point CurrentPosition { get; set; }
        public Base? Control { get; set; }
    }
}

/// <summary>
/// Interface for controls that can be clicked.
/// </summary>
public interface IClickable
{
    void InvokeClicked();
    void InvokeDoubleClicked();
}
