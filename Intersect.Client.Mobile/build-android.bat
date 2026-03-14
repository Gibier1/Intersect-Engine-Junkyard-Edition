@echo off
REM Build script for Intersect Mobile Client Android APK

setlocal enabledelayedexpansion

echo ========================================
echo Intersect Mobile Client - Android Build
echo ========================================
echo.

REM Get the script directory (project root)
set "PROJECT_DIR=%~dp0"
set "SOLUTION_DIR=%PROJECT_DIR%..\"

echo Project Directory: %PROJECT_DIR%
echo Solution Directory: %SOLUTION_DIR%
echo.

REM Check if Android SDK is configured
if "%ANDROID_SDK_ROOT%"=="" (
    if "%ANDROID_HOME%"=="" (
        echo [WARNING] ANDROID_SDK_ROOT or ANDROID_HOME environment variable not set.
        echo Attempting to continue anyway...
        echo.
    ) else (
        set "ANDROID_SDK_ROOT=%ANDROID_HOME%"
    )
)

echo Using Android SDK: %ANDROID_SDK_ROOT%
echo.

REM Navigate to solution directory
cd /d "%SOLUTION_DIR%" || (
    echo [ERROR] Could not navigate to solution directory
    pause
    exit /b 1
)

echo.
echo ========================================
echo Building Mobile Client (Debug)...
echo ========================================
echo.

REM Build the project
dotnet build "%PROJECT_DIR%Intersect.Client.Mobile.csproj" -c Debug -v n /p:AndroidSdkDirectory="%ANDROID_SDK_ROOT%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build SUCCESS!
    echo ========================================
    echo.
    echo Output location:
    echo   %PROJECT_DIR%bin\Debug\net8.0-android34.0\
    echo.

    REM Check if APK was created
    if exist "%PROJECT_DIR%bin\Debug\net8.0-android34.0\*.apk" (
        echo Generated APK files:
        dir /b "%PROJECT_DIR%bin\Debug\net8.0-android34.0\*.apk"
    ) else (
        echo [NOTE] No APK file found. This is expected if building on Windows without full Android SDK.
        echo The DLL assembly was built successfully and can be used on Android with proper SDK.
    )
) else (
    echo.
    echo ========================================
    echo Build FAILED!
    echo ========================================
    echo.
    echo Common issues:
    echo   1. Android SDK not installed or not found
    echo      Run: setup-android-env.bat
    echo.
    echo   2. Missing Android API level or build tools
    echo      Open Android Studio SDK Manager and install:
    echo      - Android SDK Platform 34
    echo      - Android SDK Build-Tools 34.0.0
    echo.
    echo   3. Java JDK not installed
    echo      Install JDK 11 or later from:
    echo      https://www.oracle.com/java/technologies/downloads/
    echo.
)

echo.
pause
