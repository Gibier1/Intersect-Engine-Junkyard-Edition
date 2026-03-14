using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Intersect.Client.Mobile.Platforms.Android;

namespace Intersect.Client.Mobile;

/// <summary>
/// Main Android Activity for the Intersect Client.
///
/// NOTE: MonoGame.Framework.Android 3.8.4.1+ supports .NET 8 Android (net8.0-android34.0).
/// The integration can now proceed with proper MonoGame Android support.
///
/// TODO when implementing full Android integration:
/// 1. Use AndroidGameActivity from MonoGame.Framework.Android
/// 2. Initialize MobileGame with proper Android context
/// 3. Set up touch input with AndroidTouchInputHandler
/// 4. Integrate virtual controls with the game loop
/// </summary>
[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Icon = "@mipmap/ic_launcher",
    Theme = "@style/AppTheme",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
)]
public class MainActivity : Activity
{
    private MobileGame? _game;
    private AndroidTouchInputHandler? _touchInputHandler;
    private FrameLayout? _frameLayout;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Create the frame layout that will hold the game view
        _frameLayout = new FrameLayout(this)
        {
            Id = ViewCompat.GenerateViewId()
        };

        SetContentView(_frameLayout);

        // TODO: Initialize MobileGame when full MonoGame Android integration is complete
        // For now, show a placeholder message
        var textView = new TextView(this)
        {
            Text = "Intersect Mobile\n\nMonoGame Android integration is in progress.\nVirtual controls architecture is complete.\nDesktop UI will be preserved as-is.",
            Gravity = GravityFlags.Center
        };
        _frameLayout.AddView(textView);
    }

    protected override void OnDestroy()
    {
        _game?.Dispose();
        base.OnDestroy();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);

        if (hasFocus)
        {
            // Hide system UI when game is focused
            HideSystemUI();
        }
    }

    private void HideSystemUI()
    {
        if (Window?.DecorView != null)
        {
            var uiOptions = (SystemUiFlags)
                (SystemUiFlags.LayoutStable
                | SystemUiFlags.LayoutHideNavigation
                | SystemUiFlags.LayoutFullscreen
                | SystemUiFlags.HideNavigation
                | SystemUiFlags.Fullscreen
                | SystemUiFlags.ImmersiveSticky);

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }
    }

    /// <summary>
    /// Gets the path to the app's external files directory for game content.
    /// </summary>
    public static string GetGameContentPath(Context context)
    {
        var filesDir = context.GetExternalFilesDir(null)?.AbsolutePath
            ?? context.FilesDir.AbsolutePath;

        // Create content subdirectory if it doesn't exist
        var contentPath = System.IO.Path.Combine(filesDir, "Content");
        if (!System.IO.Directory.Exists(contentPath))
        {
            System.IO.Directory.CreateDirectory(contentPath);
        }

        return contentPath;
    }
}
