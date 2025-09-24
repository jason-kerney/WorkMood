<!-- GENERATED DOCUMENT DO NOT EDIT! -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- Compiled with doculisp https://www.npmjs.com/package/doculisp -->
<!-- Written By: Development Team -->

# WorkMood - Build & Installation Guide #

### Complete guide for building and deploying WorkMood on Windows and macOS ###

## Table of Contents ##

1. Prerequisites: [Prerequisites](#prerequisites)
2. Building the Application: [Building the Application](#building-the-application)
3. Windows Deployment: [Windows Deployment](#windows-deployment)
4. macOS Deployment: [macOS Deployment](#macos-deployment)

## Prerequisites ##

Before building WorkMood, ensure you have the following installed on your development machine:

### .NET Requirements ###

- **.NET 9.0 SDK** or later
- **Visual Studio 2022** (17.8 or later) with the following workloads:
  - **.NET Multi-platform App UI development**
  - **Mobile development with .NET** (includes Android SDK)
- Alternatively, you can use **Visual Studio Code** with the C# extension

### Platform-Specific Requirements ###

#### Windows Development ####

- **Windows 11** (recommended) or **Windows 10** version 1903 or higher
- **Windows App SDK** (included with Visual Studio 2022)
- **Windows Subsystem for Linux (WSL)** (optional, for cross-platform testing)

#### macOS Development ####

- **macOS Big Sur** (11.0) or later
- **Xcode** 13.0 or later (for Mac Catalyst deployment)
- **Command Line Tools for Xcode**

### Additional Tools ###

- **Git** for version control
- **PowerShell** 7.0+ (cross-platform, recommended for build scripts)
- **Doculisp CLI** for documentation compilation (if modifying docs)

### Verification ###

Run the following commands to verify your setup:

```bash
dotnet --version
dotnet workload list
```

You should see `.NET 9.0` or later and the `maui` workload listed.

## Building the Application ##

Follow these steps to build WorkMood from source code:

### Clone the Repository ###

```bash
git clone https://github.com/jason-kerney/WorkMood.git
cd WorkMood
```

### Restore Dependencies ###

Navigate to the project directory and restore NuGet packages:

```bash
cd MauiApp
dotnet restore WorkMood.MauiApp.csproj
```

### Build Configuration ###

#### Debug Build ####

For development and testing:

```bash
dotnet build WorkMood.MauiApp.csproj -c Debug
```

#### Release Build ####

For production deployment:

```bash
dotnet build WorkMood.MauiApp.csproj -c Release
```

### Platform-Specific Builds ###

#### Windows Build ####

To build specifically for Windows:

```bash
dotnet build WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0
```

#### macOS Build ####

To build for macOS (Mac Catalyst):

```bash
dotnet build WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst
```

### Running the Application ###

#### Development Mode ####

To run the application in development mode:

```bash
dotnet run --project WorkMood.MauiApp.csproj -c Debug
```

#### Specific Framework ####

To run on a specific target framework:

```bash
# For Windows
dotnet run --project WorkMood.MauiApp.csproj -c Debug -f net9.0-windows10.0.19041.0

# For macOS
dotnet run --project WorkMood.MauiApp.csproj -c Debug -f net9.0-maccatalyst
```

### Build Troubleshooting ###

#### Common Issues ####

1. **Missing workloads**: Run `dotnet workload install maui`
2. **NuGet restore failures**: Clear package cache with `dotnet nuget locals all --clear`
3. **Platform targeting issues**: Ensure you have the correct SDK versions installed
4. **Build errors on macOS**: Make sure Xcode is installed and up to date

#### Clean Build ####

If you encounter persistent issues, try a clean build:

```bash
dotnet clean WorkMood.MauiApp.csproj
dotnet restore WorkMood.MauiApp.csproj
dotnet build WorkMood.MauiApp.csproj -c Release
```

## Windows Deployment ##

This guide covers deploying WorkMood on Windows systems using copy-paste deployment.

### Publishing for Windows ###

#### Single-File Deployment ####

Create a self-contained, single-file deployment:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

#### Framework-Dependent Deployment ####

If the target machine has .NET 9.0 installed:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --no-self-contained
```

### Deployment Locations ###

After publishing, the deployment files will be located at:

```
MauiApp/bin/Release/net9.0-windows10.0.19041.0/publish/
```

### Copy-Paste Deployment ###

#### Step 1: Prepare the Package ####

1. Navigate to the publish directory
2. Create a new folder for distribution (e.g., `WorkMood-v1.0-Windows`)
3. Copy all contents from the publish folder to your distribution folder

#### Step 2: Create Installation Package ####

Create the following structure for easy deployment:

```
WorkMood-v1.0-Windows/
├── WorkMood.MauiApp.exe          # Main application
├── WorkMood.MauiApp.dll          # Application library
├── *.dll                        # Required dependencies
├── install.bat                   # Installation script
├── uninstall.bat                 # Uninstallation script
└── README-Installation.txt       # Installation instructions
```

#### Step 3: Create Installation Script ####

Create `install.bat` with the following content:

```batch
@echo off
echo Installing WorkMood...

REM Create application directory
if not exist "%USERPROFILE%\AppData\Local\WorkMood" (
    mkdir "%USERPROFILE%\AppData\Local\WorkMood"
)

REM Copy application files
xcopy /Y /E *.* "%USERPROFILE%\AppData\Local\WorkMood\"

REM Create desktop shortcut (optional)
echo Creating desktop shortcut...
powershell -command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\WorkMood.lnk'); $Shortcut.TargetPath = '%USERPROFILE%\AppData\Local\WorkMood\WorkMood.MauiApp.exe'; $Shortcut.Save()"

echo Installation complete!
echo You can run WorkMood from your desktop or from:
echo %USERPROFILE%\AppData\Local\WorkMood\WorkMood.MauiApp.exe
pause
```

#### Step 4: Create Uninstallation Script ####

Create `uninstall.bat`:

```batch
@echo off
echo Uninstalling WorkMood...

REM Remove desktop shortcut
if exist "%USERPROFILE%\Desktop\WorkMood.lnk" (
    del "%USERPROFILE%\Desktop\WorkMood.lnk"
)

REM Remove application directory
if exist "%USERPROFILE%\AppData\Local\WorkMood" (
    rmdir /S /Q "%USERPROFILE%\AppData\Local\WorkMood"
)

echo WorkMood has been uninstalled.
pause
```

### Manual Installation Instructions ###

For users who prefer manual installation:

1. **Download** the WorkMood distribution package
2. **Extract** all files to a folder of your choice (e.g., `C:\Programs\WorkMood`)
3. **Run** `WorkMood.MauiApp.exe` to launch the application
4. **Optional**: Create a shortcut on your desktop or start menu

### System Requirements ###

- **Windows 10** version 1903 or later, or **Windows 11**
- **Visual C++ Redistributable** (usually already installed)
- At least **100 MB** of free disk space
- **Administrative privileges** may be required for installation scripts

### Troubleshooting ###

#### Application Won't Start ####

1. Ensure all files were copied correctly
2. Check that the target machine meets system requirements
3. Try running as administrator
4. Check Windows Event Viewer for error details

#### Missing Dependencies ####

If you encounter DLL errors, try the self-contained deployment option which includes all required runtime files.

## macOS Deployment ##

This guide covers deploying WorkMood on macOS systems using Mac Catalyst copy-paste deployment.

### Publishing for macOS ###

#### Mac Catalyst Build ####

Create a Mac Catalyst deployment package:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst --self-contained -p:CreatePackage=true
```

#### Architecture-Specific Builds ####

For specific architectures:

```bash
# For Intel Macs (x64)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst -r maccatalyst-x64 --self-contained

# For Apple Silicon Macs (ARM64)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst -r maccatalyst-arm64 --self-contained

# Universal binary (both architectures)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst --self-contained
```

### Deployment Locations ###

After publishing, the deployment files will be located at:

```text
MauiApp/bin/Release/net9.0-maccatalyst/publish/
```

The Mac app bundle will be:

```text
MauiApp/bin/Release/net9.0-maccatalyst/publish/WorkMood.MauiApp.app
```

### Copy-Paste Deployment ###

#### Step 1: Prepare the Package ####

1. Navigate to the publish directory
2. Locate the `WorkMood.MauiApp.app` bundle
3. Create a distribution folder (e.g., `WorkMood-v1.0-macOS`)

#### Step 2: Create Installation Package ####

Create the following structure for easy deployment:

```text
WorkMood-v1.0-macOS/
├── WorkMood.MauiApp.app/         # Complete app bundle
├── install.sh                    # Installation script
├── uninstall.sh                  # Uninstallation script
└── README-Installation.txt       # Installation instructions
```

#### Step 3: Create Installation Script ####

Create `install.sh` with the following content:

```bash
#!/bin/bash

echo "Installing WorkMood for macOS..."

# Check if running on macOS
if [[ "$OSTYPE" != "darwin"* ]]; then
    echo "Error: This installer is for macOS only."
    exit 1
fi

# Create Applications directory if it doesn't exist (shouldn't happen)
if [ ! -d "/Applications" ]; then
    echo "Error: /Applications directory not found."
    exit 1
fi

# Copy the app bundle to Applications
echo "Copying WorkMood to /Applications..."
cp -R "WorkMood.MauiApp.app" "/Applications/"

# Set proper permissions
chmod +x "/Applications/WorkMood.MauiApp.app/Contents/MacOS/WorkMood.MauiApp"

echo "Installation complete!"
echo "You can now find WorkMood in your Applications folder or launch it from Spotlight."

# Ask if user wants to launch the app
read -p "Would you like to launch WorkMood now? (y/n): " launch_app
if [[ $launch_app =~ ^[Yy]$ ]]; then
    open "/Applications/WorkMood.MauiApp.app"
fi
```

#### Step 4: Create Uninstallation Script ####

Create `uninstall.sh`:

```bash
#!/bin/bash

echo "Uninstalling WorkMood from macOS..."

# Remove the application
if [ -d "/Applications/WorkMood.MauiApp.app" ]; then
    echo "Removing WorkMood from Applications..."
    rm -rf "/Applications/WorkMood.MauiApp.app"
    echo "WorkMood has been uninstalled."
else
    echo "WorkMood is not installed in /Applications."
fi

# Optional: Remove user data (ask user first)
read -p "Would you like to remove all WorkMood user data? This cannot be undone. (y/n): " remove_data
if [[ $remove_data =~ ^[Yy]$ ]]; then
    rm -rf "$HOME/Library/Application Support/WorkMood"
    rm -rf "$HOME/Library/Preferences/com.workmood.mauiapp.plist"
    rm -rf "$HOME/Library/Caches/com.workmood.mauiapp"
    echo "User data removed."
fi

echo "Uninstallation complete."
```

#### Step 5: Make Scripts Executable ####

```bash
chmod +x install.sh
chmod +x uninstall.sh
```

### Manual Installation Instructions ###

For users who prefer manual installation:

1. **Download** the WorkMood distribution package
2. **Extract** the ZIP file if necessary
3. **Drag** `WorkMood.MauiApp.app` to your `/Applications` folder
4. **Launch** WorkMood from Applications or Spotlight

### Code Signing and Notarization ###

**Important**: For distribution outside the Mac App Store, the app should be code signed and notarized:

```bash
# Sign the app (requires Apple Developer account)
codesign --force --deep --sign "Developer ID Application: Your Name" WorkMood.MauiApp.app

# Create a ZIP for notarization
ditto -c -k --keepParent WorkMood.MauiApp.app WorkMood.zip

# Submit for notarization (requires Apple Developer credentials)
xcrun notarytool submit WorkMood.zip --apple-id your-apple-id --password your-app-password --team-id your-team-id
```

**Note**: Without code signing, users will see a security warning and need to allow the app in System Preferences > Security & Privacy.

### System Requirements ###

- **macOS Big Sur** (11.0) or later
- **Intel or Apple Silicon** Mac
- At least **100 MB** of free disk space
- **Administrator privileges** for installation (if using scripts)

### Troubleshooting ###

#### App Won't Launch ####

1. **Right-click** the app and select "Open" (for unsigned apps)
2. Check **System Preferences > Security & Privacy** for blocked apps
3. Ensure the app bundle is complete and not corrupted
4. Try launching from Terminal: `/Applications/WorkMood.MauiApp.app/Contents/MacOS/WorkMood.MauiApp`

#### Permission Denied Errors ####

1. Check file permissions: `ls -la /Applications/WorkMood.MauiApp.app`
2. Fix permissions: `chmod +x /Applications/WorkMood.MauiApp.app/Contents/MacOS/*`
3. Ensure your user account has admin privileges

#### Gatekeeper Issues ####

For unsigned applications, users may need to:

1. Go to **System Preferences > Security & Privacy**
2. Click **"Allow Anyway"** next to the WorkMood warning
3. Try launching the app again

<!-- Written By: Development Team -->
<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->
<!-- GENERATED DOCUMENT DO NOT EDIT! -->