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
/// Note: This is a placeholder implementation. The actual MonoGame Android integration
/// requires AndroidGameActivity from MonoGame.Framework.Android which is not compatible
/// with .NET 8 Android. This will need to be updated when MonoGame adds proper .NET 8 support.
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

        // TODO: Initialize the game when MonoGame .NET 8 Android support is available
        // For now, show a placeholder message
        var textView = new TextView(this)
        {
            Text = "Intersect Mobile\n\nMonoGame .NET 8 Android integration is pending.\nPlease use the desktop client or wait for MonoGame update.",
            Gravity = GravityFlags.Center
        };
        _frameLayout.AddView(textView);
    }

    protected override void OnResume()
    {
        base.OnResume();
        _game?.Resume();
    }

    protected override void OnPause()
    {
        base.OnPause();
        _game?.Pause();
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
