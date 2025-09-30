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

Before compiling WorkMood for personal use, ensure you have the following installed on your machine:

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

Follow these steps to compile WorkMood for personal use:

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

This guide covers publishing WorkMood for Windows developer handoff and local testing.

> **Developer Focus**: These instructions are for developers who want to create portable builds for testing, sharing with other developers, or preparing for future distribution. Currently, no official installers are published.

### Publishing for Windows ###

#### Self-Contained Single-File ####

Create a portable, self-contained single-file executable (no .NET runtime required on target):

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

#### Framework-Dependent ####

Smaller output if target machine has .NET 9.0 runtime:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --no-self-contained
```

### Published Output Location ###

Find the published files at:

```text
MauiApp/bin/Release/net9.0-windows10.0.19041.0/publish/
```

### Developer Handoff ###

#### Portable Build Package ####

For sharing with other developers or testers:

1. Navigate to the publish directory
2. Create a folder for handoff (e.g., `WorkMood-dev-build-YYYYMMDD`)
3. Copy all publish contents to the handoff folder
4. Include a README with basic usage instructions

#### Example Handoff Structure ####

```text
WorkMood-dev-build-20250929/
├── WorkMood.MauiApp.exe          # Main executable
├── WorkMood.MauiApp.dll          # Core application
├── *.dll                        # Runtime dependencies
├── README-Developer.txt          # Usage notes for testers
└── VERSION.txt                   # Build info and commit hash
```

### Running Published Build ###

To test the published build:

1. Navigate to the publish folder or extracted handoff package
2. Double-click `WorkMood.MauiApp.exe` or run from command line
3. No installation required - runs directly

### Target Requirements ###

- **Windows 10** version 1903+ or **Windows 11**
- For framework-dependent builds: .NET 9.0 Desktop Runtime
- For self-contained builds: no additional runtime needed

### Troubleshooting Published Builds ###

#### Execution Issues ####

1. Verify all files copied correctly from publish folder
2. For framework-dependent: confirm .NET 9.0 runtime installed
3. Check Windows Defender/antivirus isn't blocking unsigned executable
4. Run from command prompt to see any error messages

#### Performance Notes ####

- Self-contained builds are larger (~100MB+) but more portable
- Framework-dependent builds are smaller (~10MB) but require runtime
- ReadyToRun compilation improves startup time but increases size

## macOS Deployment ##

This guide covers publishing WorkMood for macOS developer handoff and local testing.

> **Developer Focus**: These instructions help developers create Mac Catalyst builds for testing, sharing with other Mac developers, or preparing for future distribution. Currently, no signed/notarized packages are published.

### Publishing for macOS ###

#### Universal Mac Catalyst Build ####

Create a universal Mac Catalyst app bundle (Intel + Apple Silicon):

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst --self-contained
```

#### Architecture-Specific Builds ####

For specific architectures (smaller builds):

```bash
# Intel Macs only
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst -r maccatalyst-x64 --self-contained

# Apple Silicon only
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst -r maccatalyst-arm64 --self-contained

# Framework-dependent (requires .NET runtime on target Mac)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst --no-self-contained
```

### Published Output Location ###

Find the published app bundle at:

```text
MauiApp/bin/Release/net9.0-maccatalyst/publish/WorkMood.MauiApp.app
```

### Developer Handoff ###

#### Portable App Package ####

For sharing with other Mac developers:

1. Navigate to the publish directory
2. Create a handoff folder (e.g., `WorkMood-mac-dev-YYYYMMDD`)
3. Copy the entire `WorkMood.MauiApp.app` bundle
4. Add developer documentation

#### Example Handoff Structure ####

```text
WorkMood-mac-dev-20250929/
├── WorkMood.MauiApp.app/         # Complete Mac app bundle
├── README-Developer.txt          # Usage and testing notes
├── BUILD-INFO.txt               # Build details and commit
└── KNOWN-ISSUES.txt             # Current limitations
```

#### Step 2: Create Installation Package ####

Create the following structure for easy deployment:

```text
WorkMood-v1.0-macOS/
├── WorkMood.MauiApp.app/         # Complete app bundle
├── install.sh                    # Installation script
├── uninstall.sh                  # Uninstallation script
└── README-Installation.txt       # Installation instructions
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