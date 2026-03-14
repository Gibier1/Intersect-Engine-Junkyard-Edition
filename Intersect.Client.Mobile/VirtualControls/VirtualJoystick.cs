using System;
using Intersect.Client.Framework.GenericClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Intersect.Client.Mobile.VirtualControls;

/// <summary>
/// Represents a virtual joystick for touch-based directional input.
/// Maps joystick movements to keyboard input for game control.
/// </summary>
public class VirtualJoystick
{
    private readonly Texture2D _backgroundTexture;
    private readonly Texture2D _knobTexture;
    private readonly VirtualJoystickOptions _options;

    private Point _position;
    private Point _knobPosition;
    private bool _isActive;
    private int? _activeTouchId;
    private Vector2 _currentDirection;

    /// <summary>
    /// Creates a new virtual joystick.
    /// </summary>
    public VirtualJoystick(Texture2D backgroundTexture, Texture2D knobTexture, VirtualJoystickOptions? options = null)
    {
        _backgroundTexture = backgroundTexture ?? throw new ArgumentNullException(nameof(backgroundTexture));
        _knobTexture = knobTexture ?? throw new ArgumentNullException(nameof(knobTexture));
        _options = options ?? VirtualJoystickOptions.Default;
        _currentDirection = Vector2.Zero;
    }

    /// <summary>
    /// Gets or sets the position of the joystick on screen.
    /// </summary>
    public Point Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateKnobPositionFromDirection();
        }
    }

    /// <summary>
    /// Gets whether the joystick is currently being manipulated.
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Gets the current normalized direction vector (-1 to 1 on both axes).
    /// </summary>
    public Vector2 Direction => _currentDirection;

    /// <summary>
    /// Gets the current magnitude of the joystick (0 to 1).
    /// </summary>
    public float Magnitude => _currentDirection.Length();

    /// <summary>
    /// Occurs when the joystick direction changes.
    /// </summary>
    public event EventHandler<VirtualJoystickEventArgs>? DirectionChanged;

    /// <summary>
    /// Handles touch down events for the joystick.
    /// Returns true if the touch was captured by this joystick.
    /// </summary>
    public bool HandleTouchDown(int touchId, Point position)
    {
        if (_isActive)
        {
            return false; // Already active with another touch
        }

        // Check if touch is within the joystick's bounds
        var distance = Point.Distance(position, _position);
        if (distance <= _options.TouchRadius)
        {
            _isActive = true;
            _activeTouchId = touchId;
            UpdateFromTouchPosition(position);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles touch move events for the joystick.
    /// Returns true if this touch is being tracked.
    /// </summary>
    public bool HandleTouchMove(int touchId, Point position)
    {
        if (!_isActive || _activeTouchId != touchId)
        {
            return false;
        }

        UpdateFromTouchPosition(position);
        return true;
    }

    /// <summary>
    /// Handles touch up events for the joystick.
    /// Returns true if this touch was being tracked.
    /// </summary>
    public bool HandleTouchUp(int touchId, Point position)
    {
        if (!_isActive || _activeTouchId != touchId)
        {
            return false;
        }

        _isActive = false;
        _activeTouchId = null;
        _currentDirection = Vector2.Zero;
        UpdateKnobPositionFromDirection();
        DirectionChanged?.Invoke(this, new VirtualJoystickEventArgs(Vector2.Zero, 0f));
        return true;
    }

    /// <summary>
    /// Draws the virtual joystick.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, float opacity = 1.0f)
    {
        if (!_options.IsVisible && !_isActive)
        {
            return;
        }

        // Draw background
        spriteBatch.Draw(
            _backgroundTexture,
            new Rectangle(
                _position.X - _backgroundTexture.Width / 2,
                _position.Y - _backgroundTexture.Height / 2,
                _backgroundTexture.Width,
                _backgroundTexture.Height
            ),
            new Color(255, 255, 255, (byte)(255 * opacity * (_options.IsVisible ? 1f : 0.5f)))
        );

        // Draw knob
        spriteBatch.Draw(
            _knobTexture,
            new Rectangle(
                _knobPosition.X - _knobTexture.Width / 2,
                _knobPosition.Y - _knobTexture.Height / 2,
                _knobTexture.Width,
                _knobTexture.Height
            ),
            new Color(255, 255, 255, (byte)(255 * opacity * (_isActive ? 1f : 0.5f)))
        );
    }

    private void UpdateFromTouchPosition(Point touchPosition)
    {
        var direction = new Vector2(
            touchPosition.X - _position.X,
            touchPosition.Y - _position.Y
        );

        var length = direction.Length();
        var maxDistance = _options.MaxTravelDistance;

        if (length > maxDistance)
        {
            direction = Vector2.Normalize(direction) * maxDistance;
            length = maxDistance;
        }

        // Calculate normalized direction (-1 to 1)
        _currentDirection = length > 0
            ? new Vector2(direction.X / maxDistance, direction.Y / maxDistance)
            : Vector2.Zero;

        // Apply deadzone
        if (_currentDirection.Length() < _options.Deadzone)
        {
            _currentDirection = Vector2.Zero;
        }
        else
        {
            // Rescale after deadzone
            _currentDirection = Vector2.Normalize(_currentDirection) *
                ((_currentDirection.Length() - _options.Deadzone) / (1f - _options.Deadzone));
        }

        UpdateKnobPositionFromDirection();
        DirectionChanged?.Invoke(this, new VirtualJoystickEventArgs(_currentDirection, _currentDirection.Length()));
    }

    private void UpdateKnobPositionFromDirection()
    {
        _knobPosition = new Point(
            _position.X + (int)(_currentDirection.X * _options.MaxTravelDistance),
            _position.Y + (int)(_currentDirection.Y * _options.MaxTravelDistance)
        );
    }
}

/// <summary>
/// Configuration options for the virtual joystick.
/// </summary>
public class VirtualJoystickOptions
{
    public static readonly VirtualJoystickOptions Default = new();

    /// <summary>
    /// Maximum distance the knob can travel from center.
    /// </summary>
    public float MaxTravelDistance { get; set; } = 50f;

    /// <summary>
    /// Radius around the joystick center where touches are captured.
    /// </summary>
    public float TouchRadius { get; set; } = 80f;

    /// <summary>
    /// Deadzone where small movements are ignored (0 to 1).
    /// </summary>
    public float Deadzone { get; set; } = 0.15f;

    /// <summary>
    /// Whether the joystick is visible when not active.
    /// </summary>
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Event arguments for joystick direction changes.
/// </summary>
public class VirtualJoystickEventArgs : EventArgs
{
    public VirtualJoystickEventArgs(Vector2 direction, float magnitude)
    {
        Direction = direction;
        Magnitude = magnitude;
    }

    /// <summary>
    /// The normalized direction vector.
    /// </summary>
    public Vector2 Direction { get; }

    /// <summary>
    /// The magnitude of the direction (0 to 1).
    /// </summary>
    public float Magnitude { get; }
}
