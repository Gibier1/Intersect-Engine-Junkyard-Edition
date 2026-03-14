# Android Build Environment Setup Script for Intersect Mobile Client
# Run this script in PowerShell (may require -ExecutionPolicy Bypass)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Intersect Mobile - Android Environment Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to find Android SDK
function Find-AndroidSdk {
    $candidatePaths = @(
        "$env:LOCALAPPDATA\Android\Sdk",
        "C:\Android\Sdk",
        "$env:USERPROFILE\AppData\Local\Android\Sdk",
        "C:\Users\$env:USERNAME\AppData\Local\Android\Sdk"
    )

    foreach ($path in $candidatePaths) {
        if (Test-Path "$path\platform-tools\adb.exe") {
            return $path
        }
    }
    return $null
}

# Check for Android SDK
$sdkPath = Find-AndroidSdk

if ($null -eq $sdkPath) {
    Write-Host "Android SDK not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Android Studio is being installed via winget." -ForegroundColor Yellow
    Write-Host "Once installation completes, run the SDK Manager to install:"
    Write-Host "  - Android SDK Platform 34 (or latest)"
    Write-Host "  - Android SDK Build-Tools 34.0.0"
    Write-Host "  - Android SDK Platform-Tools"
    Write-Host ""
    Write-Host "Then run this script again." -ForegroundColor Yellow
    Write-Host ""

    # Check if Android Studio installer is still running
    $installer = Get-Process "android-studio-*" -ErrorAction SilentlyContinue
    if ($installer) {
        Write-Host "Android Studio installation is in progress..." -ForegroundColor Green
        Write-Host "This may take 10-20 minutes depending on your connection." -ForegroundColor Green
    }
} else {
    Write-Host "Found Android SDK at: $sdkPath" -ForegroundColor Green

    # Set environment variables for current session
    $env:ANDROID_SDK_ROOT = $sdkPath
    $env:ANDROID_HOME = $sdkPath

    # Add to PATH
    $env:PATH = "$sdkPath\platform-tools;$sdkPath\tools;$env:PATH"

    Write-Host ""
    Write-Host "Environment variables set for current session:" -ForegroundColor Green
    Write-Host "  ANDROID_SDK_ROOT = $env:ANDROID_SDK_ROOT"
    Write-Host "  ANDROID_HOME = $env:ANDROID_HOME"
    Write-Host ""

    # Check for required components
    $apiLevel = 34
    $buildTools = "34.0.0"

    Write-Host "Checking required components..." -ForegroundColor Cyan

    if (Test-Path "$sdkPath\platforms\android-$apiLevel") {
        Write-Host "  [OK] Android API $apiLevel" -ForegroundColor Green
    } else {
        Write-Host "  [MISS] Android API $apiLevel - Install via SDK Manager" -ForegroundColor Yellow
    }

    if (Test-Path "$sdkPath\build-tools\$buildTools") {
        Write-Host "  [OK] Build Tools $buildTools" -ForegroundColor Green
    } else {
        Write-Host "  [MISS] Build Tools $buildTools - Install via SDK Manager" -ForegroundColor Yellow
    }

    if (Test-Path "$sdkPath\platform-tools\adb.exe") {
        Write-Host "  [OK] Platform Tools" -ForegroundColor Green
    } else {
        Write-Host "  [MISS] Platform Tools - Install via SDK Manager" -ForegroundColor Yellow
    }

    # Ask if user wants to persist environment variables
    Write-Host ""
    $persist = Read-Host "Persist environment variables to system? (Y/N)"
    if ($persist -eq 'Y' -or $persist -eq 'y') {
        [Environment]::SetEnvironmentVariable("ANDROID_SDK_ROOT", $sdkPath, "User")
        [Environment]::SetEnvironmentVariable("ANDROID_HOME", $sdkPath, "User")
        Write-Host "Environment variables saved to user environment." -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Setup Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You can now build the mobile client:"
    Write-Host "  cd Intersect.Client.Mobile"
    Write-Host "  dotnet build -c Debug"
    Write-Host ""
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
