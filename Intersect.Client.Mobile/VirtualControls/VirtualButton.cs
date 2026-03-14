using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Intersect.Client.Framework.Gwen;

namespace Intersect.Client.Mobile.VirtualControls;

/// <summary>
/// Represents a virtual button for touch-based input.
/// Maps touch to keyboard input for game control.
/// </summary>
public class VirtualButton
{
    private readonly Texture2D _texture;
    private readonly Texture2D? _pressedTexture;
    private readonly VirtualButtonOptions _options;

    private Rectangle _bounds;
    private bool _isPressed;
    private int? _activeTouchId;
    private float _pressAnimation;

    /// <summary>
    /// Creates a new virtual button.
    /// </summary>
    public VirtualButton(
        Texture2D texture,
        VirtualButtonOptions? options = null,
        Texture2D? pressedTexture = null)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _pressedTexture = pressedTexture;
        _options = options ?? VirtualButtonOptions.Default;
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
        set => _bounds = new Rectangle(value.X, value.Y, _bounds.Width, _bounds.Height);
    }

    /// <summary>
    /// Gets whether the button is currently pressed.
    /// </summary>
    public bool IsPressed => _isPressed;

    /// <summary>
    /// Gets or sets the key that this button simulates.
    /// </summary>
    public Key MappedKey { get; set; }

    /// <summary>
    /// Occurs when the button is pressed.
    /// </summary>
    public event EventHandler? Pressed;

    /// <summary>
    /// Occurs when the button is released.
    /// </summary>
    public event EventHandler? Released;

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
            Pressed?.Invoke(this, EventArgs.Empty);
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
        Released?.Invoke(this, EventArgs.Empty);
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
    /// Draws the virtual button.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, float opacity = 1.0f)
    {
        if (!_options.IsVisible && !_isPressed)
        {
            return;
        }

        var scale = 1f - (_pressAnimation * _options.PressScaleAmount);
        var scaledBounds = new Rectangle(
            _bounds.Center.X - (int)(_bounds.Width * scale / 2),
            _bounds.Center.Y - (int)(_bounds.Height * scale / 2),
            (int)(_bounds.Width * scale),
            (int)(_bounds.Height * scale)
        );

        var currentTexture = _pressedTexture != null && _isPressed ? _pressedTexture : _texture;
        var color = new Color(255, 255, 255, (byte)(255 * opacity * (_options.IsVisible || _isPressed ? 1f : 0.3f)));

        spriteBatch.Draw(currentTexture, scaledBounds, color);

        // Draw label if specified
        if (!string.IsNullOrEmpty(_options.Label))
        {
            // TODO: Draw text label - requires font reference
        }
    }
}

/// <summary>
/// Configuration options for virtual buttons.
/// </summary>
public class VirtualButtonOptions
{
    public static readonly VirtualButtonOptions Default = new();

    /// <summary>
    /// How much the button scales when pressed (0 to 1).
    /// </summary>
    public float PressScaleAmount { get; set; } = 0.1f;

    /// <summary>
    /// Whether the button is visible when not pressed.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Optional label text to display on the button.
    /// </summary>
    public string? Label { get; set; }
}
