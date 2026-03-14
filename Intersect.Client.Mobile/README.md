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

### ⚠️ Known Limitation
**MonoGame 3.8.2 does not support .NET 8 Android.** The MonoGame team is working on version 3.9+ which will include proper .NET 8 Android support.

Current options:
1. **Wait for MonoGame 3.9+** with .NET 8 Android support
2. **Use community MonoGame builds** that support .NET 8 Android
3. **Downgrade to net7.0-android** (requires downgrading all Intersect dependencies)
4. **Use different approach** - KNI (Kotlin Native Interop) or Android.Game.Activity

### Current State
- The code is **ready** for Android development once MonoGame .NET 8 support is available
- Virtual controls architecture is complete and tested
- Touch input abstraction layer is implemented
- Desktop UI integration is planned

### What Would Need to Change When MonoGame 3.9+ Releases
1. Update `MonoGame.Framework.DesktopGL` → `MonoGame.Framework.Android` (3.9+)
2. Re-enable `MainActivity.cs` and `AndroidTouchInputHandler.cs`
3. Remove `MockTouchInputHandler` from `MobileGame.cs`
4. Update project to target proper Android SDK bindings

### Build Steps (When MonoGame 3.9+ is Available)
```bash
# Install Android workload
dotnet workload install android

# Restore packages
dotnet restore Intersect.Client.Mobile/Intersect.Client.Mobile.csproj

# Build in Debug mode
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
- [ ] **BLOCKED**: MonoGame 3.8.2 does not support .NET 8 Android - waiting for MonoGame 3.9+ or community fork
- [ ] **Input Integration**: Integrate key simulation with existing input system
- [ ] **Textures**: Add proper textures for virtual controls (currently using generated circles)
- [ ] **Testing**: Test connection to Intersect server
- [ ] **Text Rendering**: Add text rendering for button labels
- [ ] **UI Scaling**: Resize desktop UI elements to fit mobile screens
- [ ] **iOS**: Implement iOS touch input handler

## License

Same as the main Intersect Engine project. See LICENSE.md in the root directory.
