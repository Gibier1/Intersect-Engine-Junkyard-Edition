using System;
using Intersect.Client.Mobile.Input;
using Intersect.Client.Mobile.VirtualControls;
using Intersect.Client.Core;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace Intersect.Client.Mobile;

/// <summary>
/// Main MonoGame class for the mobile version of Intersect Client.
///
/// NOTE: MonoGame.Framework.Android 3.8.4.1+ supports .NET 8 Android.
/// The Android touch input (AndroidTouchInputHandler) is now available.
///
/// TODO for full integration:
/// 1. Create MonoRenderer and MonoInput
/// 2. Initialize Gwen with mobile touch support
/// 3. Initialize VirtualControlsManager with AndroidTouchInputHandler
/// 4. Integrate with existing game loop
/// 5. Pass unhandled touch events to desktop UI
/// </summary>
public sealed class MobileGame : Game
{
    private GraphicsDeviceManager? _graphics;
    private SpriteBatch? _spriteBatch;

    // Framework integration (will be initialized when MonoGame Android is available)
    private bool _isInitialized;

    public MobileGame()
    {
        // Subscribe to the Exiting event for cleanup
        Exiting += OnExiting;

        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1920,
            PreferredBackBufferHeight = 1080,
            IsFullScreen = true,
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight,
            SynchronizeWithVerticalRetrace = false,
            PreferMultiSampling = true
        };

        _graphics.PreparingDeviceSettings += (sender, args) =>
        {
            args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage =
                RenderTargetUsage.DiscardContents;
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
        };

        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (_graphics == null)
        {
            throw new InvalidOperationException("GraphicsDeviceManager not initialized");
        }

        try
        {
            // Create sprite batch for rendering virtual controls
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: Initialize full framework when MonoGame Android is available
            // - Create MonoRenderer
            // - Create MonoInput
            // - Initialize Gwen with mobile touch support
            // - Initialize virtual controls

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to initialize game: {ex}");
            throw;
        }
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        // TODO: Load game content when MonoGame Android is available
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!_isInitialized)
        {
            return;
        }

        // TODO: Update game logic when MonoGame Android is available
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(MonoGameColor.CornflowerBlue);

        base.Draw(gameTime);

        // TODO: Draw game and virtual controls when MonoGame Android is available
    }

    private void OnExiting(object? sender, EventArgs args)
    {
        // Clean up resources
        _spriteBatch?.Dispose();
        _graphics?.Dispose();
    }
}

/// <summary>
/// Entry point for Windows development builds.
/// On Android, MainActivity.cs provides the entry point and this is ignored.
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main()
    {
        using var game = new MobileGame();
        game.Run();
    }
}
