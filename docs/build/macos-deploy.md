<!-- (dl (section-meta macOS Deployment)) -->

This guide covers deploying WorkMood on macOS systems using Mac Catalyst copy-paste deployment.

<!-- (dl (# Publishing for macOS)) -->

<!-- (dl (## Mac Catalyst Build)) -->

Create a Mac Catalyst deployment package:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst --self-contained -p:CreatePackage=true
```

<!-- (dl (## Architecture-Specific Builds)) -->

For specific architectures:

```bash
# For Intel Macs (x64)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst -r maccatalyst-x64 --self-contained

# For Apple Silicon Macs (ARM64)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst -r maccatalyst-arm64 --self-contained

# Universal binary (both architectures)
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst --self-contained
```

<!-- (dl (# Deployment Locations)) -->

After publishing, the deployment files will be located at:

```text
MauiApp/bin/Release/net9.0-maccatalyst/publish/
```

The Mac app bundle will be:

```text
MauiApp/bin/Release/net9.0-maccatalyst/publish/WorkMood.MauiApp.app
```

<!-- (dl (# Copy-Paste Deployment)) -->

<!-- (dl (## Step 1: Prepare the Package)) -->

1. Navigate to the publish directory
2. Locate the `WorkMood.MauiApp.app` bundle
3. Create a distribution folder (e.g., `WorkMood-v1.0-macOS`)

<!-- (dl (## Step 2: Create Installation Package)) -->

Create the following structure for easy deployment:

```text
WorkMood-v1.0-macOS/
├── WorkMood.MauiApp.app/         # Complete app bundle
├── install.sh                    # Installation script
├── uninstall.sh                  # Uninstallation script
└── README-Installation.txt       # Installation instructions
```

<!-- (dl (## Step 3: Create Installation Script)) -->

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

<!-- (dl (## Step 4: Create Uninstallation Script)) -->

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

<!-- (dl (## Step 5: Make Scripts Executable)) -->

```bash
chmod +x install.sh
chmod +x uninstall.sh
```

<!-- (dl (# Manual Installation Instructions)) -->

For users who prefer manual installation:

1. **Download** the WorkMood distribution package
2. **Extract** the ZIP file if necessary
3. **Drag** `WorkMood.MauiApp.app` to your `/Applications` folder
4. **Launch** WorkMood from Applications or Spotlight

<!-- (dl (# Code Signing and Notarization)) -->

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

<!-- (dl (# System Requirements)) -->

- **macOS Big Sur** (11.0) or later
- **Intel or Apple Silicon** Mac
- At least **100 MB** of free disk space
- **Administrator privileges** for installation (if using scripts)

<!-- (dl (# Troubleshooting)) -->

<!-- (dl (## App Won't Launch)) -->

1. **Right-click** the app and select "Open" (for unsigned apps)
2. Check **System Preferences > Security & Privacy** for blocked apps
3. Ensure the app bundle is complete and not corrupted
4. Try launching from Terminal: `/Applications/WorkMood.MauiApp.app/Contents/MacOS/WorkMood.MauiApp`

<!-- (dl (## Permission Denied Errors)) -->

1. Check file permissions: `ls -la /Applications/WorkMood.MauiApp.app`
2. Fix permissions: `chmod +x /Applications/WorkMood.MauiApp.app/Contents/MacOS/*`
3. Ensure your user account has admin privileges

<!-- (dl (## Gatekeeper Issues)) -->

For unsigned applications, users may need to:

1. Go to **System Preferences > Security & Privacy**
2. Click **"Allow Anyway"** next to the WorkMood warning
3. Try launching the app again