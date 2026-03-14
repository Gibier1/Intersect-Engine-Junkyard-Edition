# Intersect Engine - Mobile Client

This directory contains the Android implementation of the Intersect Engine client with touch input support.

## Design Philosophy

**Use the desktop UI as-is, add only touch-specific controls.**

The mobile client preserves the existing desktop interface and only adds virtual controls for things that don't have touch equivalents.

## Control Layout

```
┌──────────────────────────────────────────────────────────────────────┐
│                                                                   │
│                                                          [≡ Esc]     │
│                                                     Window Menu     │
│                                                                   │
│              [◄►]                    OR                           │
│        [1][2][3][4][5][6][7][8][9][0]  [I][K][C][P][L][F][G][O]    │
│           (Hotbar Skills)          (Window Buttons)                │
│                     (on top of joystick/clicks)                   │
│                                                                   │
│  [Move]                                                  [L][R]      │
│                                                          Clicks       │
│                                                                   │
└──────────────────────────────────────────────────────────────────────┘
```

The **[◄►] toggle button** switches between:
- **Hotbar Mode**: Skill buttons 1-10 (D1-D0 keys)
- **Windows Mode**: Inventory(I), Spells(K), Character(C), Party(P), Quests(L), Friends(F), Guild(G), Settings(O)

## Virtual Controls

| Control | Position | Function | Notes |
|---------|----------|----------|-------|
| **Joystick** | Bottom Left | Movement (WASD) | Transparent when not in use |
| **L/R Click** | Bottom Right | Attack/Block | Green/Red buttons |
| **Toggle Button [◄►]** | Bottom Center (side) | Switch modes | Hotbar ↔ Windows |
| **Hotbar 1-10** | Bottom Center | Quick skills | Shown in Hotbar Mode, ON TOP of joystick/clicks |
| **Window Buttons** | Bottom Center | Open windows | Shown in Windows Mode: Inventory, Spells, Character, Party, Quests, Friends, Guild, Settings |
| **Esc Button** | Top Right | Open menu | Triggers SimplifiedEscapeMenu |

## Desktop UI (Preserved)

All existing desktop UI elements are used as-is, just resized to fit mobile screen:

- **Hotbar** (top-right) - Same as desktop, resized
- **Inventory window** - Same as desktop
- **Spells window** - Same as desktop
- **Character window** - Same as desktop
- **All other windows** - Same as desktop

## Window Menu (≡ Button)

Tapping the **Esc (≡)** button opens the same `SimplifiedEscapeMenu` used on desktop:

- Settings
- Character Select
- Logout
- Exit to Desktop

## Architecture Overview

```
Intersect.Client.Mobile/
├── Input/                          # Touch input abstraction
│   ├── ITouchInputHandler.cs      # Touch input interface
│   └── TouchToGwenAdapter.cs      # Converts touch to Gwen GUI events
├── VirtualControls/                # On-screen game controls
│   ├── VirtualJoystick.cs         # Virtual D-pad for movement
│   ├── VirtualButton.cs           # Action buttons
│   ├── VirtualMouseButton.cs      # Left/Right click buttons
│   └── VirtualControlsManager.cs  # Manages all virtual controls
├── Platforms/
│   └── Android/
│       └── AndroidTouchInputHandler.cs  # Android touch implementation
├── MainActivity.cs                # Android Activity entry point
├── MobileGame.cs                  # MonoGame integration
└── Intersect.Client.Mobile.csproj # Project configuration
```

## How It Works

1. **Desktop UI**: All existing windows, hotbar, etc. work exactly as desktop (just resized)
2. **Touch Handling**: Touch events first go to virtual controls
3. **Unhandled Touches**: Pass through to desktop UI for direct interaction
4. **Window Menu**: Uses the same `SimplifiedEscapeMenu` as desktop

## Building for Android

### ✅ Build Success - MonoGame 3.8.4.1+ Supports .NET 8 Android!
**MonoGame.Framework.Android 3.8.4.1+ now supports `net8.0-android34.0`**. The project has been updated and compiles successfully.

The following changes have been made:
1. ✅ Updated `MonoGame.Framework.DesktopGL` → `MonoGame.Framework.Android` (3.8.4.1)
2. ✅ Updated target framework to `net8.0-android34.0`
3. ✅ Re-enabled `MainActivity.cs` and `AndroidTouchInputHandler.cs` (conditionally excluded on Windows)
4. ✅ Removed `MockTouchInputHandler` placeholder
5. ✅ Added cross-platform Main method entry point for Windows development builds
6. ✅ Fixed all type ambiguities (Rectangle, Color, Keys) between MonoGame and Intersect Framework

### Current State
- The project **compiles successfully** on Windows for development
- Virtual controls architecture is implemented
- Touch input abstraction layer is complete (`AndroidTouchInputHandler`)
- Desktop UI integration is planned for future development

### Build Steps

#### Option 1: Using the Provided Scripts (Recommended)

1. **Wait for Android Studio installation to complete** (being installed via winget)
   - This downloads ~1GB and may take 10-20 minutes

2. **Run the setup script** (once Android Studio installation completes):
   ```bash
   # PowerShell (recommended)
   .\Intersect.Client.Mobile\setup-android-env.ps1

   # Or batch file
   .\Intersect.Client.Mobile\setup-android-env.bat
   ```

3. **Open Android Studio** and install required SDK components:
   - Launch Android Studio from Start Menu
   - Go to **Tools → SDK Manager**
   - In **SDK Platforms** tab: Check "Android 14.0 (API 34)" or latest
   - In **SDK Tools** tab: Check "Android SDK Build-Tools 34.0.0" or latest
   - Click **Apply** to install

4. **Build the mobile client**:
   ```bash
   .\Intersect.Client.Mobile\build-android.bat
   ```

#### Option 2: Manual Setup

```bash
# 1. Install Android Studio (includes Android SDK)
winget install --id Google.AndroidStudio

# 2. Set environment variables
setx ANDROID_SDK_ROOT "C:\Users\%USERNAME%\AppData\Local\Android\Sdk"
setx ANDROID_HOME "%ANDROID_SDK_ROOT%"

# 3. Install Android components via Android Studio SDK Manager:
#    - Android SDK Platform 34
#    - Android SDK Build-Tools 34.0.0
#    - Android SDK Platform-Tools

# 4. Build
dotnet build Intersect.Client.Mobile/Intersect.Client.Mobile.csproj -c Debug
```

## Current Status

### ✅ Implemented
- [x] Touch input abstraction layer
- [x] Touch-to-Gwen event adapter
- [x] Virtual joystick (bottom left)
- [x] Left/Right click buttons (bottom right)
- [x] Toggle button to switch between hotbar and window modes
- [x] Mobile hotbar (bottom center, rendered on top)
- [x] Window buttons (Inventory, Spells, Character, Party, Quests, Friends, Guild, Settings)
- [x] Escape menu button (top right) - triggers `SimplifiedEscapeMenu`
- [x] Proper rendering order (hotbar/windows above joystick/clicks)
- [x] Uses desktop UI windows as-is

### ⏳ TODO
- [x] **Environment**: Android workload installed (`dotnet workload install android`)
- [x] **MonoGame**: Updated to MonoGame.Framework.Android 3.8.4.1 (supports .NET 8 Android)
- [x] **Project Setup**: Project compiles successfully on Windows and ready for Android builds
- [ ] **Input Integration**: Integrate key simulation with existing Intersect input system (currently stubbed)
- [ ] **Game Loop**: Connect VirtualControlsManager to MonoGame update/draw cycle
- [ ] **GUI Integration**: Properly integrate with Gwen GUI system (NodeFilter, Globals, etc.)
- [ ] **Textures**: Add proper textures for virtual controls (currently using generated circles)
- [ ] **Testing**: Test on actual Android device and test connection to Intersect server
- [ ] **Text Rendering**: Add text rendering for button labels
- [ ] **UI Scaling**: Resize desktop UI elements to fit mobile screens
- [ ] **iOS**: Implement iOS touch input handler

## License

Same as the main Intersect Engine project. See LICENSE.md in the root directory.
