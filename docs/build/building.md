<!-- (dl (section-meta Building the Application)) -->

Follow these steps to compile WorkMood for personal use:

<!-- (dl (# Clone the Repository)) -->

```bash
git clone https://github.com/jason-kerney/WorkMood.git
cd WorkMood
```

<!-- (dl (# Restore Dependencies)) -->

Navigate to the project directory and restore NuGet packages:

```bash
cd MauiApp
dotnet restore WorkMood.MauiApp.csproj
```

> **Important**: If restore fails with "package not found" errors, you may need to configure the official NuGet source. See the troubleshooting section below.

<!-- (dl (# Build Configuration)) -->

<!-- (dl (## Debug Build)) -->

For development and testing:

```bash
dotnet build WorkMood.MauiApp.csproj -c Debug
```

<!-- (dl (## Release Build)) -->

For production deployment:

```bash
dotnet build WorkMood.MauiApp.csproj -c Release
```

<!-- (dl (# Platform-Specific Builds)) -->

<!-- (dl (## Windows Build)) -->

To build specifically for Windows:

```bash
dotnet build WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0
```

<!-- (dl (## macOS Build)) -->

To build for macOS (Mac Catalyst):

```bash
dotnet build WorkMood.MauiApp.csproj -c Release -f net9.0-maccatalyst
```

<!-- (dl (# Running the Application)) -->

<!-- (dl (## Development Mode)) -->

To run the application in development mode:

```bash
dotnet run --project WorkMood.MauiApp.csproj -c Debug
```

<!-- (dl (## Specific Framework)) -->

To run on a specific target framework:

```bash
# For Windows
dotnet run --project WorkMood.MauiApp.csproj -c Debug -f net9.0-windows10.0.19041.0

# For macOS
dotnet run --project WorkMood.MauiApp.csproj -c Debug -f net9.0-maccatalyst
```

<!-- (dl (# Build Troubleshooting)) -->

<!-- (dl (## Common Issues)) -->

1. **Missing workloads**: Run `dotnet workload install maui` (ignore any "Advertising manifest not updated" warnings)
2. **NuGet package not found errors**: If you receive errors that NuGet packages (especially MAUI components) cannot be found during restore or build, this usually indicates that the official NuGet package source is not configured:

   ```bash
   # Add the official NuGet package source
   dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
   
   # List all configured package sources to verify
   dotnet nuget list source
   
   # Then retry the restore
   dotnet restore WorkMood.MauiApp.csproj
   ```

3. **NuGet restore failures**: Clear package cache with `dotnet nuget locals all --clear`
4. **Platform targeting issues**: Ensure you have the correct SDK versions installed
5. **Build errors on macOS**: Make sure Xcode is installed and up to date

<!-- (dl (## Clean Build)) -->

If you encounter persistent issues, try a clean build:

```bash
dotnet clean WorkMood.MauiApp.csproj
dotnet restore WorkMood.MauiApp.csproj
dotnet build WorkMood.MauiApp.csproj -c Release
```