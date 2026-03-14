using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameRectangle = Microsoft.Xna.Framework.Rectangle;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace Intersect.Client.Mobile.VirtualControls;

/// <summary>
/// A virtual button that simulates mouse clicks (left or right).
/// Used for clicking on game entities, UI elements, and interacting with the world.
/// </summary>
public class VirtualMouseButton
{
    private readonly Texture2D _texture;
    private readonly Texture2D? _pressedTexture;
    private readonly VirtualMouseButtonOptions _options;
    private readonly Framework.Input.MouseButton _mouseButton;

    private Rectangle _bounds;
    private bool _isPressed;
    private int? _activeTouchId;
    private float _pressAnimation;

    /// <summary>
    /// Creates a new virtual mouse button.
    /// </summary>
    public VirtualMouseButton(
        Texture2D texture,
        Framework.Input.MouseButton mouseButton,
        VirtualMouseButtonOptions? options = null,
        Texture2D? pressedTexture = null)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _mouseButton = mouseButton;
        _pressedTexture = pressedTexture;
        _options = options ?? VirtualMouseButtonOptions.Default;
        _pressAnimation = 0f;
    }

    /// <summary>
    /// Gets or sets the button's position and size.
    /// </summary>
    public Rectangle Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }

    /// <summary>
    /// Gets or sets the button's position.
    /// </summary>
    public Point Position
    {
        get => new Point(_bounds.X, _bounds.Y);
        set => _bounds = new MonoGameRectangle(value.X, value.Y, _bounds.Width, _bounds.Height);
    }

    /// <summary>
    /// Gets the mouse button this simulates.
    /// </summary>
    public Framework.Input.MouseButton MouseButton => _mouseButton;

    /// <summary>
    /// Gets whether the button is currently pressed.
    /// </summary>
    public bool IsPressed => _isPressed;

    /// <summary>
    /// Occurs when the button is pressed (mouse down).
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? Pressed;

    /// <summary>
    /// Occurs when the button is released (mouse up).
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? Released;

    /// <summary>
    /// Occurs when the button is clicked (pressed then released).
    /// </summary>
    public event EventHandler<MouseButtonEventArgs>? Clicked;

    /// <summary>
    /// Handles touch down events for the button.
    /// Returns true if the touch was captured by this button.
    /// </summary>
    public bool HandleTouchDown(int touchId, Point position)
    {
        if (_isPressed)
        {
            return false; // Already pressed with another touch
        }

        // Check if touch is within the button's bounds
        if (_bounds.Contains(position.X, position.Y))
        {
            _isPressed = true;
            _activeTouchId = touchId;

            Pressed?.Invoke(this, new MouseButtonEventArgs(_mouseButton, position));

            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles touch up events for the button.
    /// Returns true if this touch was being tracked.
    /// </summary>
    public bool HandleTouchUp(int touchId, Point position)
    {
        if (!_isPressed || _activeTouchId != touchId)
        {
            return false;
        }

        _isPressed = false;
        _activeTouchId = null;

        Released?.Invoke(this, new MouseButtonEventArgs(_mouseButton, position));
        Clicked?.Invoke(this, new MouseButtonEventArgs(_mouseButton, position));

        return true;
    }

    /// <summary>
    /// Updates the button's animation state.
    /// </summary>
    public void Update(float deltaTime)
    {
        // Animate press effect
        var targetAnimation = _isPressed ? 1f : 0f;
        _pressAnimation = MathHelper.Lerp(_pressAnimation, targetAnimation, deltaTime * 15f);
    }

    /// <summary>
    /// Draws the virtual mouse button.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, float opacity = 1.0f)
    {
        if (!_options.IsVisible && !_isPressed)
        {
            return;
        }

        var scale = 1f - (_pressAnimation * _options.PressScaleAmount);
        var scaledBounds = new MonoGameRectangle(
            _bounds.Center.X - (int)(_bounds.Width * scale / 2),
            _bounds.Center.Y - (int)(_bounds.Height * scale / 2),
            (int)(_bounds.Width * scale),
            (int)(_bounds.Height * scale)
        );

        var currentTexture = _pressedTexture != null && _isPressed ? _pressedTexture : _texture;
        var baseOpacity = _options.IsVisible || _isPressed ? 1f : 0.3f;
        var color = new Microsoft.Xna.Framework.Color((byte)255, (byte)255, (byte)255, (byte)(255 * opacity * baseOpacity));

        spriteBatch.Draw(currentTexture, scaledBounds, color);

        // Draw icon/label
        if (!string.IsNullOrEmpty(_options.Label))
        {
            // TODO: Draw text label when font system is available
        }
    }
}

/// <summary>
/// Configuration options for virtual mouse buttons.
/// </summary>
public class VirtualMouseButtonOptions
{
    public static readonly VirtualMouseButtonOptions Default = new();

    /// <summary>
    /// How much the button scales when pressed (0 to 1).
    /// </summary>
    public float PressScaleAmount { get; set; } = 0.15f;

    /// <summary>
    /// Whether the button is visible when not pressed.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Optional label text to display on the button.
    /// </summary>
    public string? Label { get; set; }
}

/// <summary>
/// Event arguments for mouse button events.
/// </summary>
public class MouseButtonEventArgs : EventArgs
{
    public MouseButtonEventArgs(Framework.Input.MouseButton mouseButton, Point position)
    {
        MouseButton = mouseButton;
        Position = position;
    }

    /// <summary>
    /// The mouse button that was pressed/released.
    /// </summary>
    public Framework.Input.MouseButton MouseButton { get; }

    /// <summary>
    /// The position where the event occurred.
    /// </summary>
    public Point Position { get; }
}
