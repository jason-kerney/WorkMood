<!-- (dl (section-meta Windows Deployment)) -->

This guide covers publishing WorkMood for Windows developer handoff and local testing.

> **Developer Focus**: These instructions are for developers who want to create portable builds for testing, sharing with other developers, or preparing for future distribution. Currently, no official installers are published.

<!-- (dl (# Publishing for Windows)) -->

<!-- (dl (## Self-Contained Single-File)) -->

Create a portable, self-contained single-file executable (no .NET runtime required on target):

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

<!-- (dl (## Framework-Dependent)) -->

Smaller output if target machine has .NET 9.0 runtime:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --no-self-contained
```

<!-- (dl (# Published Output Location)) -->

Find the published files at:

```text
MauiApp/bin/Release/net9.0-windows10.0.19041.0/publish/
```

<!-- (dl (# Developer Handoff)) -->

<!-- (dl (## Portable Build Package)) -->

For sharing with other developers or testers:

1. Navigate to the publish directory
2. Create a folder for handoff (e.g., `WorkMood-dev-build-YYYYMMDD`)
3. Copy all publish contents to the handoff folder
4. Include a README with basic usage instructions

<!-- (dl (## Example Handoff Structure)) -->

```text
WorkMood-dev-build-20250929/
├── WorkMood.MauiApp.exe          # Main executable
├── WorkMood.MauiApp.dll          # Core application
├── *.dll                        # Runtime dependencies
├── README-Developer.txt          # Usage notes for testers
└── VERSION.txt                   # Build info and commit hash
```

<!-- (dl (# Running Published Build)) -->

To test the published build:

1. Navigate to the publish folder or extracted handoff package
2. Double-click `WorkMood.MauiApp.exe` or run from command line
3. No installation required - runs directly

<!-- (dl (# Target Requirements)) -->

- **Windows 10** version 1903+ or **Windows 11**
- For framework-dependent builds: .NET 9.0 Desktop Runtime
- For self-contained builds: no additional runtime needed

<!-- (dl (# Troubleshooting Published Builds)) -->

<!-- (dl (## Execution Issues)) -->

1. Verify all files copied correctly from publish folder
2. For framework-dependent: confirm .NET 9.0 runtime installed
3. Check Windows Defender/antivirus isn't blocking unsigned executable
4. Run from command prompt to see any error messages

<!-- (dl (## Performance Notes)) -->

- Self-contained builds are larger (~100MB+) but more portable
- Framework-dependent builds are smaller (~10MB) but require runtime
- ReadyToRun compilation improves startup time but increases size
