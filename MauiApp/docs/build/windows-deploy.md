<!-- (dl (section-meta Windows Deployment)) -->

This guide covers deploying WorkMood on Windows systems using copy-paste deployment.

<!-- (dl (# Publishing for Windows)) -->

<!-- (dl (## Single-File Deployment)) -->

Create a self-contained, single-file deployment:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

<!-- (dl (## Framework-Dependent Deployment)) -->

If the target machine has .NET 9.0 installed:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --no-self-contained
```

<!-- (dl (# Deployment Locations)) -->

After publishing, the deployment files will be located at:

```
MauiApp/bin/Release/net9.0-windows10.0.19041.0/publish/
```

<!-- (dl (# Copy-Paste Deployment)) -->

<!-- (dl (## Step 1: Prepare the Package)) -->

1. Navigate to the publish directory
2. Create a new folder for distribution (e.g., `WorkMood-v1.0-Windows`)
3. Copy all contents from the publish folder to your distribution folder

<!-- (dl (## Step 2: Create Installation Package)) -->

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

<!-- (dl (## Step 3: Create Installation Script)) -->

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

<!-- (dl (## Step 4: Create Uninstallation Script)) -->

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

<!-- (dl (# Manual Installation Instructions)) -->

For users who prefer manual installation:

1. **Download** the WorkMood distribution package
2. **Extract** all files to a folder of your choice (e.g., `C:\Programs\WorkMood`)
3. **Run** `WorkMood.MauiApp.exe` to launch the application
4. **Optional**: Create a shortcut on your desktop or start menu

<!-- (dl (# System Requirements)) -->

- **Windows 10** version 1903 or later, or **Windows 11**
- **Visual C++ Redistributable** (usually already installed)
- At least **100 MB** of free disk space
- **Administrative privileges** may be required for installation scripts

<!-- (dl (# Troubleshooting)) -->

<!-- (dl (## Application Won't Start)) -->

1. Ensure all files were copied correctly
2. Check that the target machine meets system requirements
3. Try running as administrator
4. Check Windows Event Viewer for error details

<!-- (dl (## Missing Dependencies)) -->

If you encounter DLL errors, try the self-contained deployment option which includes all required runtime files.