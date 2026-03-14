@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Installing Android SDK Components
echo ========================================
echo.

set "SDK_DIR=C:\Users\jessy\AppData\Local\Android\Sdk"
set "ZIP_FILE=C:\Users\jessy\AppData\Local\Temp\cmdline-tools.zip"

echo SDK Location: %SDK_DIR%
echo.

REM Create directory for command-line tools
echo Creating directory for command-line tools...
if not exist "%SDK_DIR%\cmdline-tools" mkdir "%SDK_DIR%\cmdline-tools"
if not exist "%SDK_DIR%\cmdline-tools\latest" mkdir "%SDK_DIR%\cmdline-tools\latest"

echo Extracting command-line tools...
powershell -Command "Expand-Archive -Path '%ZIP_FILE%' -DestinationPath '%SDK_DIR%\cmdline-tools\latest' -Force"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Failed to extract command-line tools.
    echo.
    echo Manual steps:
    echo   1. Open Android Studio
    echo   2. Go to Tools -^> SDK Manager
    echo   3. Install "Android SDK Command-line Tools"
    echo   4. Install "Android SDK Platform 34"
    echo   5. Install "Android SDK Build-Tools 34.0.0"
    echo.
    pause
    exit /b 1
)

echo [OK] Command-line tools extracted.
echo.

REM Install required SDK components
echo Installing Android SDK components...
echo.

echo Installing Android Platform 34...
"%SDK_DIR%\cmdline-tools\latest\bin\sdkmanager.bat" "platforms;android-34"

echo.
echo Installing Build Tools 34.0.0...
"%SDK_DIR%\cmdline-tools\latest\bin\sdkmanager.bat" "build-tools;34.0.0"

echo.
echo Installing Platform Tools...
"%SDK_DIR%\cmdline-tools\latest\bin\sdkmanager.bat" "platform-tools"

echo.
echo ========================================
echo Installation Complete!
echo ========================================
echo.
echo You can now build the mobile client:
echo   .\build-android.bat
echo.
pause
