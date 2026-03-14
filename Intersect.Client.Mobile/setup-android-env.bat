@echo off
REM Setup script for Android build environment
REM This script configures environment variables for building the Intersect Mobile Client

echo ========================================
echo Intersect Mobile Client - Android Build Environment Setup
echo ========================================
echo.

REM Default Android SDK installation paths (try to find where Android Studio installed it)
set "ANDROID_SDK_CANDIDATES=%LOCALAPPDATA%\Android\Sdk C:\Android\Sdk %USERPROFILE%\AppData\Local\Android\Skd"

REM Find Android SDK location
for %%i in (%ANDROID_SDK_CANDIDATES%) do (
    if exist "%%i\platform-tools\adb.exe" (
        set "ANDROID_SDK_ROOT=%%i"
        goto :found_sdk
    )
)

echo Android SDK not found in standard locations.
echo Please install Android Studio or the Android SDK command-line tools.
echo After installation, set ANDROID_SDK_ROOT environment variable manually.
pause
exit /b 1

:found_sdk
echo Found Android SDK at: %ANDROID_SDK_ROOT%

REM Add Android SDK tools to PATH
set "PATH=%ANDROID_SDK_ROOT%\platform-tools;%ANDROID_SDK_ROOT%\tools;%PATH%"

echo.
echo Setting up environment variables...
setx ANDROID_SDK_ROOT "%ANDROID_SDK_ROOT%" >nul
setx ANDROID_HOME "%ANDROID_SDK_ROOT%" >nul

echo.
echo Environment variables configured:
echo   ANDROID_SDK_ROOT = %ANDROID_SDK_ROOT%
echo   ANDROID_HOME = %ANDROID_SDK_ROOT%
echo.

REM Install required Android API level and build tools if not present
echo Checking for required Android components...

set "API_LEVEL=34"
set "BUILD_TOOLS=34.0.0"

if not exist "%ANDROID_SDK_ROOT%\platforms\android-%API_LEVEL%" (
    echo.
    echo API Level %API_LEVEL% not found.
    echo Please open Android Studio and install:
    echo   - Android SDK Platform %API_LEVEL%
    echo   - Android SDK Build-Tools %BUILD_TOOLS%
    echo   - Android SDK Platform-Tools
    echo.
    echo To install in Android Studio:
    echo   1. Open Android Studio
    echo   2. Go to Tools -^> SDK Manager
    echo   3. Select "SDK Platforms" tab and check Android %API_LEVEL% (or latest)
    echo   4. Select "SDK Tools" tab and check "Android SDK Build-Tools"
    echo   5. Click Apply to install
    echo.
) else (
    echo   Android API %API_LEVEL%: Installed
)

if not exist "%ANDROID_SDK_ROOT%\build-tools\%BUILD_TOOLS%" (
    echo   Build Tools %BUILD_TOOLS%: Not found (install via SDK Manager)
) else (
    echo   Build Tools %BUILD_TOOLS%: Installed
)

echo.
echo ========================================
echo Setup complete!
echo ========================================
echo.
echo You can now build the mobile client:
echo   cd Intersect.Client.Mobile
echo   dotnet build -c Debug
echo.
pause
