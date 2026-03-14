@echo off
REM Script to install Android SDK components via Android Studio

echo ========================================
echo Install Android SDK Components
echo ========================================
echo.
echo This script will guide you through installing the required
echo Android SDK components for building the Intersect Mobile Client.
echo.

REM Check if Android Studio is running
tasklist /FI "IMAGENAME eq studio64.exe" 2>nul | find /I "studio64.exe" >nul
if %ERRORLEVEL% EQU 0 (
    echo [WARNING] Android Studio appears to be running.
    echo Please close Android Studio before continuing, or press Ctrl+C to cancel.
    echo.
    pause
)

echo.
echo Opening Android Studio SDK Manager...
echo.

REM Launch Android Studio's SDK Manager directly
start "" "C:\Program Files\Android\Android Studio\bin\studio.bat" --command=vector

echo.
echo ========================================
echo Instructions:
echo ========================================
echo.
echo 1. In the Android Studio SDK Manager window:
echo.
echo    A. Select the "SDK Platforms" tab
echo       - Check "Android 14.0 (API 34)" or the latest available
echo.
echo    B. Select the "SDK Tools" tab
echo       - Check "Android SDK Build-Tools 34.0.0" or latest
echo       - Check "Android SDK Command-line Tools (latest)"
echo       - Check "Android Emulator" (optional, for testing)
echo.
echo    C. Click "Apply" or "OK" to install
echo.
echo 2. Wait for download and installation to complete.
echo.
echo 3. Return here and run: build-android.bat
echo.
echo ========================================
echo.
echo Press any key to open SDK Manager...
pause >nul

REM Try opening SDK Manager via command line
"C:\Program Files\Android\Android Studio\bin\studio.sh" --command=vector 2>nul
goto :manual

:manual
echo.
echo ========================================
echo Manual Installation Guide:
echo ========================================
echo.
echo If the SDK Manager didn't open automatically:
echo.
echo 1. Open Android Studio
echo 2. On the welcome screen, click "More Actions" → "SDK Manager"
echo    OR go to: Tools → SDK Manager
echo 3. Follow the instructions above
echo.
echo ========================================
echo.
pause
