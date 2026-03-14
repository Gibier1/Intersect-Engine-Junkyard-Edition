using System;
using System.Collections.Generic;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Gwen;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Input;
using Intersect.Client.Framework.Input;
using Intersect.Client.Mobile.Input;
using Intersect.Client.Interface.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Intersect.Client.Mobile.VirtualControls;

/// <summary>
/// Manages on-screen virtual controls for mobile.
/// Uses desktop UI with minimal touch-specific controls layered on top.
/// </summary>
public class VirtualControlsManager
{
    private readonly ITouchInputHandler _touchInputHandler;
    private readonly Texture2D _defaultTexture;
    private readonly SpriteBatch _spriteBatch;

    private readonly List<VirtualJoystick> _joysticks = new();
    private readonly List<VirtualButton> _buttons = new();
    private readonly List<VirtualMouseButton> _mouseButtons = new();

    private readonly Dictionary<int, TrackedTouch> _touchMapping = new();

    // Window mode definitions
    private readonly List<WindowEntry> _windowEntries = new();

    // Hotbar and window button lists (managed separately for toggle functionality)
    private List<VirtualButton> _hotbarButtons = new();
    private List<VirtualButton> _windowButtons = new();

    /// <summary>
    /// Creates a new virtual controls manager.
    /// </summary>
    public VirtualControlsManager(
        ITouchInputHandler touchInputHandler,
        Texture2D defaultTexture,
        SpriteBatch spriteBatch)
    {
        _touchInputHandler = touchInputHandler ?? throw new ArgumentNullException(nameof(touchInputHandler));
        _defaultTexture = defaultTexture ?? throw new ArgumentNullException(nameof(defaultTexture));
        _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));

        // Subscribe to touch events
        _touchInputHandler.TouchStarted += OnTouchStarted;
        _touchInputHandler.TouchMoved += OnTouchMoved;
        _touchInputHandler.TouchEnded += OnTouchEnded;

        // Setup window entries
        SetupWindowEntries();
    }

    /// <summary>
    /// Gets whether the virtual controls are visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the global opacity of the virtual controls.
    /// </summary>
    public float Opacity { get; set; } = 0.75f;

    /// <summary>
    /// Gets the current display mode (Hotbar or Windows).
    /// </summary>
    public DisplayMode CurrentDisplayMode { get; private set; } = DisplayMode.Hotbar;

    /// <summary>
    /// Gets the left mouse click button.
    /// </summary>
    public VirtualMouseButton? LeftClickButton { get; private set; }

    /// <summary>
    /// Gets the right mouse click button.
    /// </summary>
    public VirtualMouseButton? RightClickButton { get; private set; }

    /// <summary>
    /// Gets the window/escape menu button.
    /// </summary>
    public VirtualButton? WindowButton { get; private set; }

    /// <summary>
    /// Occurs when the display mode changes.
    /// </summary>
    public event EventHandler<DisplayModeChangedEventArgs>? DisplayModeChanged;

    /// <summary>
    /// Adds a joystick to the manager.
    /// </summary>
    public void AddJoystick(VirtualJoystick joystick)
    {
        if (joystick == null)
        {
            throw new ArgumentNullException(nameof(joystick));
        }

        _joysticks.Add(joystick);
    }

    /// <summary>
    /// Adds a button to the manager.
    /// </summary>
    public void AddButton(VirtualButton button)
    {
        if (button == null)
        {
            throw new ArgumentNullException(nameof(button));
        }

        _buttons.Add(button);

        // Subscribe to button events for key mapping
        button.Pressed += (s, e) => OnButtonPressed(button);
        button.Released += (s, e) => OnButtonReleased(button);
    }

    /// <summary>
    /// Adds a mouse button to the manager.
    /// </summary>
    public void AddMouseButton(VirtualMouseButton mouseButton)
    {
        if (mouseButton == null)
        {
            throw new ArgumentNullException(nameof(mouseButton));
        }

        _mouseButtons.Add(mouseButton);

        // Store references to click buttons
        if (mouseButton.MouseButton == Framework.Input.MouseButton.Left)
        {
            LeftClickButton = mouseButton;
        }
        else if (mouseButton.MouseButton == Framework.Input.MouseButton.Right)
        {
            RightClickButton = mouseButton;
        }

        // Subscribe to mouse button events
        mouseButton.Pressed += (s, e) => OnMouseButtonPressed(mouseButton);
        mouseButton.Released += (s, e) => OnMouseButtonReleased(mouseButton);
        mouseButton.Clicked += (s, e) => OnMouseButtonClicked(mouseButton);
    }

    /// <summary>
    /// Updates all virtual controls.
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var button in _buttons)
        {
            button.Update(deltaTime);
        }

        foreach (var mouseButton in _mouseButtons)
        {
            mouseButton.Update(deltaTime);
        }

        // Update toggle button animation
        _toggleButton?.Update(deltaTime);
    }

    /// <summary>
    /// Draws all visible virtual controls.
    /// Rendering order: Joysticks/ClickButtons (bottom layer) -> Toggle+Hotbar/Windows (middle layer) -> WindowButton (top)
    /// </summary>
    public void Draw()
    {
        if (!IsVisible)
        {
            return;
        }

        // Layer 1: Joysticks and Click Buttons (bottom layer)
        foreach (var joystick in _joysticks)
        {
            joystick.Draw(_spriteBatch, Opacity);
        }

        foreach (var mouseButton in _mouseButtons)
        {
            mouseButton.Draw(_spriteBatch, Opacity);
        }

        // Layer 2: Toggle Button + Hotbar or Windows (rendered above joystick/clicks)
        _toggleButton?.Draw(_spriteBatch, Opacity);

        if (CurrentDisplayMode == DisplayMode.Hotbar)
        {
            // Show hotbar buttons
            foreach (var button in _hotbarButtons)
            {
                button.Draw(_spriteBatch, Opacity);
            }
        }
        else
        {
            // Show window buttons
            foreach (var button in _windowButtons)
            {
                button.Draw(_spriteBatch, Opacity);
            }
        }

        // Layer 3: Window/Esc Button (topmost, on top of everything)
        _windowButton?.Draw(_spriteBatch, Opacity);
    }

    /// <summary>
    /// Creates mobile virtual controls using the desktop UI.
    /// Layout: Joystick (bottom-left), L/R Clicks (bottom-right), Toggle (bottom-center),
    /// Hotbar/Windows (above toggle), Esc button (top-right).
    /// </summary>
    public void CreateDefaultControls(int screenWidth, int screenHeight)
    {
        // --- BOTTOM LEFT: Movement Joystick ---
        var joystickPosition = new Point(
            (int)(screenWidth * 0.12f),
            (int)(screenHeight * 0.72f)
        );

        var movementJoystick = CreateDefaultJoystick();
        movementJoystick.Position = joystickPosition;
        AddJoystick(movementJoystick);

        // --- BOTTOM RIGHT: Click Buttons ---
        var clickButtonsX = (int)(screenWidth * 0.85f);
        var clickButtonsY = (int)(screenHeight * 0.72f);
        var clickButtonSize = 70;
        var clickButtonSpacing = 80;

        // Left Click Button (green) - Attack/Interact
        var leftClickButton = CreateLeftClickButton();
        leftClickButton.Bounds = new Rectangle(
            clickButtonsX,
            clickButtonsY,
            clickButtonSize,
            clickButtonSize
        );
        AddMouseButton(leftClickButton);

        // Right Click Button (red) - Block
        var rightClickButton = CreateRightClickButton();
        rightClickButton.Bounds = new Rectangle(
            clickButtonsX + clickButtonSize,
            clickButtonsY,
            clickButtonSize,
            clickButtonSize
        );
        AddMouseButton(rightClickButton);

        // --- BOTTOM CENTER: Toggle Bar ---
        var toggleBarY = (int)(screenHeight * 0.84f);
        var toggleButtonSize = 35;
        var toggleButtonX = screenWidth / 2 - toggleButtonSize / 2;

        // Toggle Switch Button [<] / [^]
        _toggleButton = CreateToggleButton();
        _toggleButton.Bounds = new Rectangle(
            toggleButtonX,
            toggleBarY,
            toggleButtonSize,
            toggleButtonSize
        );

        // Subscribe to toggle button
        _toggleButton.Pressed -= OnToggleButtonPressed;
        _toggleButton.Pressed += OnToggleButtonPressed;
        AddButton(_toggleButton);

        // --- BOTTOM CENTER: Hotbar / Windows (above toggle) ---
        var actionBarY = (int)(screenHeight * 0.76f);
        var buttonSize = 42;
        var buttonSpacing = 48;

        // Create Hotbar Buttons (1-10)
        _hotbarButtons = new List<VirtualButton>();
        for (var i = 0; i < 10; i++)
        {
            var hotkeyButton = CreateHotbarButton(i + 1);
            hotkeyButton.Bounds = new Rectangle(
                toggleButtonX + (i - 5) * buttonSpacing + toggleButtonSize / 2 - (buttonSize / 2),
                actionBarY,
                buttonSize,
                buttonSize
            );

            hotkeyButton.MappedKey = i switch
            {
                0 => Key.D1,
                1 => Key.D2,
                2 => Key.D3,
                3 => Key.D4,
                4 => Key.D5,
                5 => Key.D6,
                6 => Key.D7,
                7 => Key.D8,
                8 => Key.D9,
                9 => Key.D0,
                _ => Key.None
            };

            _hotbarButtons.Add(hotkeyButton);
            AddButton(hotkeyButton);
        }

        // Create Window Buttons (same positions as hotbar)
        _windowButtons = new List<VirtualButton>();
        var windowConfigs = new[]
        {
            (Icon: "🎒", Name: "Inventory", Key: Key.I),
            (Icon: "✦", Name: "Spells", Key: Key.K),
            (Icon: "👤", Name: "Character", Key: Key.C),
            (Icon: "⚔", Name: "Party", Key: Key.P),
            (Icon: "📜", Name: "Quests", Key: Key.L),
            (Icon: "👥", Name: "Friends", Key: Key.F),
            (Icon: "🏰", Name: "Guild", Key: Key.G),
            (Icon: "⚙", Name: "Settings", Key: Key.O),
        };

        for (var i = 0; i < windowConfigs.Length && i < 10; i++)
        {
            var config = windowConfigs[i];
            var windowButton = CreateWindowButton(config.Icon, config.Name);
            windowButton.Bounds = new Rectangle(
                toggleButtonX + (i - 5) * buttonSpacing + toggleButtonSize / 2 - (buttonSize / 2),
                actionBarY,
                buttonSize,
                buttonSize
            );

            windowButton.MappedKey = config.Key;
            _windowButtons.Add(windowButton);
            AddButton(windowButton);
        }

        // --- TOP RIGHT: Window/Escape Menu Button ---
        var windowButtonSize = 48;
        var windowButtonX = screenWidth - windowButtonSize - 15;
        var windowButtonY = 15;

        var windowButton = CreateWindowButton();
        windowButton.Bounds = new Rectangle(
            windowButtonX,
            windowButtonY,
            windowButtonSize,
            windowButtonSize
        );

        _windowButton = windowButton;
        windowButton.Pressed -= OnWindowButtonPressed;
        windowButton.Pressed += OnWindowButtonPressed;
        AddButton(windowButton);
    }

    private VirtualButton? _toggleButton;

    private void SetupWindowEntries()
    {
        _windowEntries = new List<WindowEntry>
        {
            new WindowEntry("Inventory", "🎒", Key.I),
            new WindowEntry("Spells", "✦", Key.K),
            new WindowEntry("Character", "👤", Key.C),
            new WindowEntry("Party", "⚔", Key.P),
            new WindowEntry("Quests", "📜", Key.L),
            new WindowEntry("Friends", "👥", Key.F),
            new WindowEntry("Guild", "🏰", Key.G),
            new WindowEntry("Settings", "⚙", Key.O),
        };
    }

    /// <summary>
    /// Toggles between Hotbar and Windows display mode.
    /// </summary>
    public void ToggleDisplayMode()
    {
        CurrentDisplayMode = CurrentDisplayMode == DisplayMode.Hotbar
            ? DisplayMode.Windows
            : DisplayMode.Hotbar;

        DisplayModeChanged?.Invoke(this, new DisplayModeChangedEventArgs(CurrentDisplayMode));

        Android.Util.Log.Debug("IntersectMobile", $"Display mode: {CurrentDisplayMode}");
    }

    private VirtualJoystick CreateDefaultJoystick()
    {
        var backgroundTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 100, new Color(50, 50, 50, 140));
        var knobTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 50, new Color(200, 200, 200, 170));

        return new VirtualJoystick(backgroundTexture, knobTexture, new VirtualJoystickOptions
        {
            MaxTravelDistance = 55f,
            TouchRadius = 110f,
            Deadzone = 0.15f,
            IsVisible = true
        });
    }

    private VirtualMouseButton CreateLeftClickButton()
    {
        // Green for left click - Attack/Interact
        var texture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 70, new Color(34, 139, 34, 160));
        var pressedTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 70, new Color(60, 179, 60, 200));

        return new VirtualMouseButton(
            texture,
            Framework.Input.MouseButton.Left,
            new VirtualMouseButtonOptions
            {
                PressScaleAmount = 0.2f,
                IsVisible = true
            },
            pressedTexture
        );
    }

    private VirtualMouseButton CreateRightClickButton()
    {
        // Red for right click - Block
        var texture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 70, new Color(178, 34, 34, 160));
        var pressedTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 70, new Color(220, 60, 60, 200));

        return new VirtualMouseButton(
            texture,
            Framework.Input.MouseButton.Right,
            new VirtualMouseButtonOptions
            {
                PressScaleAmount = 0.2f,
                IsVisible = true
            },
            pressedTexture
        );
    }

    private VirtualButton CreateHotbarButton(int number)
    {
        // Semi-transparent purple for hotbar
        var baseColor = new Color(138, 43, 226, 140);
        var pressedColor = new Color(168, 98, 246, 180);

        var texture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 42, baseColor);
        var pressedTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 42, pressedColor);

        return new VirtualButton(
            texture,
            new VirtualButtonOptions
            {
                PressScaleAmount = 0.15f,
                IsVisible = true,
                Label = number.ToString()
            },
            pressedTexture
        );
    }

    private VirtualButton CreateWindowButton(string icon, string name)
    {
        // Orange/coral for window buttons
        var baseColor = new Color(255, 140, 80, 150);
        var pressedColor = new Color(255, 180, 130, 190);

        var texture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 42, baseColor);
        var pressedTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 42, pressedColor);

        return new VirtualButton(
            texture,
            new VirtualButtonOptions
            {
                PressScaleAmount = 0.15f,
                IsVisible = false, // Hidden by default, shown when toggled
                Label = icon
            },
            pressedTexture
        );
    }

    private VirtualButton CreateToggleButton()
    {
        // Blue/teal for toggle switch
        var baseColor = new Color(70, 130, 180, 180);
        var hotbarColor = new Color(70, 180, 130, 180);
        var windowsColor = new Color(180, 130, 70, 180);

        var texture = CreateToggleSwitchTexture(_spriteBatch.GraphicsDevice, 35, baseColor, hotbarColor, windowsColor);
        var pressedTexture = CreateToggleSwitchTexture(_spriteBatch.GraphicsDevice, 35, baseColor, hotbarColor, windowsColor, true);

        return new VirtualButton(
            texture,
            new VirtualButtonOptions
            {
                PressScaleAmount = 0.1f,
                IsVisible = true,
                Label = "↔"
            },
            pressedTexture
        );
    }

    private VirtualButton CreateWindowButton()
    {
        // Orange/coral for escape/menu button
        var baseColor = new Color(255, 140, 80, 190);
        var pressedColor = new Color(255, 180, 130, 230);

        var texture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 48, baseColor);
        var pressedTexture = CreateCircleTexture(_spriteBatch.GraphicsDevice, 48, pressedColor);

        return new VirtualButton(
            texture,
            new VirtualButtonOptions
            {
                PressScaleAmount = 0.15f,
                IsVisible = true,
                Label = "≡"
            },
            pressedTexture
        );
    }

    private Texture2D CreateCircleTexture(GraphicsDevice device, int diameter, Color color)
    {
        var texture = new Texture2D(device, diameter, diameter);
        var data = new Color[diameter * diameter];

        var center = diameter / 2f;
        var radius = diameter / 2f;

        for (var y = 0; y < diameter; y++)
        {
            for (var x = 0; x < diameter; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance <= radius)
                {
                    var alpha = Math.Min(1f, Math.Max(0f, 1f - (distance - radius + 2f) / 2f));
                    data[y * diameter + x] = new Color(
                        (byte)(color.R * alpha),
                        (byte)(color.G * alpha),
                        (byte)(color.B * alpha),
                        (byte)(color.A * alpha)
                    );
                }
                else
                {
                    data[y * diameter + x] = Color.Transparent;
                }
            }
        }

        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateToggleSwitchTexture(GraphicsDevice device, int size, Color baseColor, Color hotbarColor, Color windowsColor, bool isPressed = false)
    {
        var texture = new Texture2D(device, size, size * 2);
        var data = new Color[size * size * 2];

        // Top half: hotbar indicator, Bottom half: windows indicator
        for (var y = 0; y < size * 2; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var isInUpperHalf = y < size;
                var activeColor = isInUpperHalf ? hotbarColor : windowsColor;
                var inactiveColor = isInUpperHalf ? windowsColor : hotbarColor;
                var currentModeIsUpper = CurrentDisplayMode == DisplayMode.Hotbar;

                // Determine final color based on current mode and which half
                Color finalColor;
                if (isPressed)
                {
                    // When pressed, both halves show the opposite mode's color
                    finalColor = isInUpperHalf == currentModeIsUpper ? inactiveColor : activeColor;
                }
                else
                {
                    // When not pressed, upper half shows active, lower shows inactive
                    finalColor = isInUpperHalf ? activeColor : inactiveColor;
                }

                var dx = x - size / 2f;
                var dy = (y % size) - size / 2f;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Create arrow shape
                var inArrow = isInUpperHalf ? dx < 0 : dx > 0; // Arrow points opposite direction
                var distanceFromArrow = Math.Abs(dx) + (isInUpperHalf == (CurrentDisplayMode == DisplayMode.Hotbar) ? 0 : size);
                var inArrowShape = inArrow && distanceFromArrow < 10 && distance < size / 2f;

                if (inArrowShape)
                {
                    data[y * size + x] = Color.White;
                }
                else if (distance <= size / 2f - 4)
                {
                    var alpha = Math.Min(1f, Math.Max(0f, 1f - (distance - size / 2f + 4) / 2f));
                    data[y * size + x] = new Color(
                        (byte)(finalColor.R * alpha),
                        (byte)(finalColor.G * alpha),
                        (byte)(finalColor.B * alpha),
                        (byte)(finalColor.A * alpha)
                    );
                }
                else
                {
                    data[y * size + x] = Color.Transparent;
                }
            }
        }

        texture.SetData(data);
        return texture;
    }

    #region Touch Event Handlers

    private void OnTouchStarted(object? sender, TouchEventArgs e)
    {
        // Try joysticks first
        foreach (var joystick in _joysticks)
        {
            if (joystick.HandleTouchDown(e.TouchId, e.Position))
            {
                _touchMapping[e.TouchId] = new TrackedTouch { Control = joystick, Type = VirtualControlType.Joystick };
                return;
            }
        }

        // Try mouse buttons
        foreach (var mouseButton in _mouseButtons)
        {
            if (mouseButton.HandleTouchDown(e.TouchId, e.Position))
            {
                _touchMapping[e.TouchId] = new TrackedTouch { Control = mouseButton, Type = VirtualControlType.MouseButton };
                return;
            }
        }

        // Try action buttons
        foreach (var button in _buttons)
        {
            if (button.HandleTouchDown(e.TouchId, e.Position))
            {
                _touchMapping[e.TouchId] = new TrackedTouch { Control = button, Type = VirtualControlType.Button };
                return;
            }
        }

        // If no virtual control captured the touch, pass it to the game UI
        OnUnhandledTouch(e);
    }

    private void OnTouchMoved(object? sender, TouchEventArgs e)
    {
        if (!_touchMapping.TryGetValue(e.TouchId, out var trackedTouch))
        {
            return;
        }

        if (trackedTouch.Control is VirtualJoystick joystick)
        {
            joystick.HandleTouchMove(e.TouchId, e.Position);
        }
    }

    private void OnTouchEnded(object? sender, TouchEventArgs e)
    {
        if (!_touchMapping.TryGetValue(e.TouchId, out var trackedTouch))
        {
            return;
        }

        switch (trackedTouch.Control)
        {
            case VirtualJoystick joystick:
                joystick.HandleTouchUp(e.TouchId, e.Position);
                break;
            case VirtualButton button:
                button.HandleTouchUp(e.TouchId, e.Position);
                break;
            case VirtualMouseButton mouseButton:
                mouseButton.HandleTouchUp(e.TouchId, e.Position);
                break;
        }

        _touchMapping.Remove(e.TouchId);
    }

    private void OnUnhandledTouch(TouchEventArgs e)
    {
        // Pass unhandled touches to the game for UI interaction
        _touchAdapter?.HandleUnhandledTouch(e);
    }

    #endregion

    #region Button Event Handlers

    private void OnButtonPressed(VirtualButton button)
    {
        if (button.MappedKey != Key.None)
        {
            SimulateKeyPress(button.MappedKey);
        }
    }

    private void OnButtonReleased(VirtualButton button)
    {
        if (button.MappedKey != Key.None)
        {
            SimulateKeyRelease(button.MappedKey);
        }
    }

    private void OnToggleButtonPressed(object? sender, EventArgs e)
    {
        ToggleDisplayMode();
        // Update toggle button texture to reflect new state
        if (_toggleButton != null)
        {
            // Remove and re-add to update texture
            _buttons.Remove(_toggleButton);

            var toggleBarY = _toggleButton.Bounds.Y;
            var toggleButtonSize = 35;

            _toggleButton = CreateToggleButton();
            _toggleButton.Bounds = new Rectangle(
                _toggleButton.Bounds.X,
                toggleBarY,
                toggleButtonSize,
                toggleButtonSize
            );

            // Re-subscribe
            _toggleButton.Pressed += OnToggleButtonPressed;
            AddButton(_toggleButton);
        }
    }

    private void OnWindowButtonPressed(object? sender, EventArgs e)
    {
        TriggerEscapeMenu();
    }

    private void OnMouseButtonPressed(VirtualMouseButton mouseButton)
    {
        SimulateMouseButtonDown(mouseButton.MouseButton);
    }

    private void OnMouseButtonReleased(VirtualMouseButton mouseButton)
    {
        SimulateMouseButtonUp(mouseButton.MouseButton);
    }

    private void OnMouseButtonClicked(VirtualMouseButton mouseButton)
    {
        SimulateMouseClick(mouseButton.MouseButton);
    }

    #endregion

    #region Input Simulation

    private TouchToGwenAdapter? _touchAdapter;

    /// <summary>
    /// Sets the touch adapter for handling unhandled touches and mouse simulation.
    /// </summary>
    public void SetTouchAdapter(TouchToGwenAdapter adapter)
    {
        _touchAdapter = adapter;
    }

    private void SimulateKeyPress(Key key)
    {
        // TODO: Integrate with existing input system
        Android.Util.Log.Debug("IntersectMobile", $"Key pressed: {key}");
    }

    private void SimulateKeyRelease(Key key)
    {
        Android.Util.Log.Debug("IntersectMobile", $"Key released: {key}");
    }

    private void SimulateMouseButtonDown(Framework.Input.MouseButton mouseButton)
    {
        var control = FindControlUnderCursor();
        if (control != null)
        {
            InputHandler.Focus(FocusSource.Mouse, control);
        }
    }

    private void SimulateMouseButtonUp(Framework.Input.MouseButton mouseButton)
    {
        InputHandler.Focus(FocusSource.Mouse, null);
    }

    private void SimulateMouseClick(Framework.Input.MouseButton mouseButton)
    {
        var control = FindControlUnderCursor();
        if (control != null)
        {
            InputHandler.Focus(FocusSource.Mouse, control);

            if (control is IClickable clickable)
            {
                clickable.InvokeClicked();
            }
        }
    }

    private Base? FindControlUnderCursor()
    {
        try
        {
            return Interface.Interface.FindComponentUnderCursor(
                new Framework.Gwen.Control.NodeFilter(
                    (in Base baseControl) =>
                    {
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

    private void TriggerEscapeMenu()
    {
        // Trigger the same SimplifiedEscapeMenu that desktop uses
        try
        {
            if (Interface.Interface.HasInGameUI)
            {
                var gameUi = Interface.Interface.GameUi;

                // Check if using simplified escape menu or full escape menu
                if (Globals.Database?.SimplifiedEscapeMenu == true)
                {
                    var menu = gameUi.SimplifiedEscapeMenu;
                    if (menu != null)
                    {
                        menu.ToggleHidden(null);
                    }
                }
                else
                {
                    var menu = gameUi.EscapeMenu;
                    if (menu != null && menu.IsHidden)
                    {
                        menu.Show();
                    }
                    else if (menu != null)
                    {
                        menu.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("IntersectMobile", $"Error triggering escape menu: {ex}");
        }
    }

    #endregion

    private class TrackedTouch
    {
        public object? Control { get; set; }
        public VirtualControlType Type { get; set; }
    }

    private enum VirtualControlType
    {
        Joystick,
        Button,
        MouseButton
    }
}

/// <summary>
/// Display modes for the bottom center area.
/// </summary>
public enum DisplayMode
{
    /// <summary>
    /// Shows hotbar skills 1-10.
    /// </summary>
    Hotbar,

    /// <summary>
    /// Shows window buttons (Inventory, Spells, Character, Party, Quests, Friends, Guild, Settings).
    /// </summary>
    Windows
}

/// <summary>
/// Event arguments for display mode changes.
/// </summary>
public class DisplayModeChangedEventArgs : EventArgs
{
    public DisplayMode DisplayMode { get; }

    public DisplayModeChangedEventArgs(DisplayMode displayMode)
    {
        DisplayMode = displayMode;
    }
}

/// <summary>
/// Represents a window entry for the window selector.
/// </summary>
public class WindowEntry
{
    public WindowEntry(string name, string icon, Key key)
    {
        Name = name;
        Icon = icon;
        Key = key;
    }

    public string Name { get; }
    public string Icon { get; }
    public Key Key { get; }
}
