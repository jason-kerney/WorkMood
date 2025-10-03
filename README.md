<!-- GENERATED DOCUMENT DO NOT EDIT! -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- Compiled with doculisp https://www.npmjs.com/package/doculisp -->

# WorkMood #

### Work mood impact tracker for understanding how your workday affects your emotional well-being ###

## Table of Contents ##

1. [Overview](#overview)
2. [Key-Features](#key-features)
3. [Getting-Started](#getting-started)
4. [Documentation](#documentation)

## Overview ##

WorkMood is a cross-platform desktop application designed to help you understand how your workday impacts your emotional state. Rather than general mood tracking, WorkMood focuses specifically on the relationship between your work environment and emotional well-being.

The application allows you to record your mood at the beginning and end of your work day, then visualizes patterns to help you identify what work situations affect your mood positively or negatively.

#### Platform Support ####

- **Windows 10** (version 1903 or later) and **Windows 11**
- **macOS Big Sur** (11.0) or later
- Built with .NET 9.0 and MAUI for reliable cross-platform performance

## Key-Features ##

#### Core Functionality ####

- **Work-focused tracking**: Record morning and evening moods to measure daily work impact
- **Mood visualization**: View charts and trends showing how work affects your emotional patterns
- **Historical analysis**: Browse past entries to identify work-related mood patterns
- **Schedule integration**: Set work-based reminders for consistent mood tracking

#### Smart Features ####

- **Automatic calculations**: Analyzes mood changes throughout your work day
- **Flexible scheduling**: Configure reminders based on your actual work hours
- **Data export**: Generate reports for personal analysis or sharing with healthcare providers
- **Clean interface**: Designed for quick daily use without complexity

## Getting-Started ##

> **For Developers**: WorkMood is designed for developers who want to track how their job impacts their mood. Since developers are comfortable compiling applications, we're not providing pre-built installers. Simply build from source to get started using the app.

#### Prerequisites ####

- .NET 9.0 SDK (confirm with `dotnet --version`)
- NuGet package source configured (verify with `dotnet nuget list source` - should include nuget.org)
- MAUI workload: `dotnet workload install maui`
- (Windows) Windows 10 1903+ / Windows 11; optional Visual Studio 2022 with MAUI workload
- (macOS) Xcode + Mac Catalyst toolchain (only if building on macOS)

#### Clone the Repository ####

```bash
git clone https://github.com/jason-kerney/WorkMood.git
cd WorkMood/MauiApp
```

**First-Time Setup**: If this is your first .NET MAUI project, ensure NuGet sources are properly configured:

```bash
# Verify NuGet sources (should include nuget.org)
dotnet nuget list source

# If nuget.org is missing, add it:
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# Install MAUI workload (may show advertising manifest warnings - these are safe to ignore)
dotnet workload install maui
```

#### Build From Source ####

First, restore packages:

```bash
dotnet restore WorkMood.MauiApp.csproj
```

Debug build (fast inner-loop):

```bash
dotnet build WorkMood.MauiApp.csproj -c Debug
```

Release build:

```bash
dotnet build WorkMood.MauiApp.csproj -c Release
```

Target a specific platform (examples):

```bash
# Windows
dotnet build WorkMood.MauiApp.csproj -c Debug -f net9.0-windows10.0.19041.0

# macOS (Mac Catalyst)
dotnet build WorkMood.MauiApp.csproj -c Debug -f net9.0-maccatalyst
```

Run (Debug):

```bash
dotnet run --project WorkMood.MauiApp.csproj -c Debug
```

Run targeting a framework:

```bash
dotnet run --project WorkMood.MauiApp.csproj -c Debug -f net9.0-windows10.0.19041.0
```

Publish (framework-dependent) for local handoff/testing:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --no-self-contained
```

Self-contained single-file publish:

```bash
dotnet publish WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --self-contained -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

Output path pattern:

```text
MauiApp/bin/<Configuration>/<TFM>/<optional RID>/publish/
```

#### Quick Start ####

**Just compiled WorkMood? Here's how to start tracking your work mood:**

##### Running the Application #####

After a successful build, run WorkMood:

```bash
# From project root, run the app
dotnet run --project MauiApp/WorkMood.MauiApp.csproj -c Debug

# Or navigate to build output and run executable
cd MauiApp/bin/Debug/net9.0-windows10.0.19041.0
./WorkMood.MauiApp.exe  # Windows
```

For published builds, navigate to the publish folder and run the executable directly.

##### Data Storage #####

WorkMood stores your mood data locally:

- **Windows**: `%APPDATA%\WorkMood` or `%USERPROFILE%\.workmood`
- **macOS**: `~/Library/Application Support/WorkMood` or `~/.workmood`

Your data persists across different builds and updates.

##### Getting Started with Mood Tracking #####

1. **Record your first mood**: Open the "Record Mood" tab and log how you're feeling at work right now
2. **Explore the interface**: Check out History, Visualization, and Settings tabs to understand the app
3. **Set your schedule**: Configure your work start/end times in Settings for reminder notifications
4. **Start tracking**: Record your mood at the beginning and end of work days to see patterns

##### Usage Tips for Developers #####

- **Consistent tracking**: Regular mood entries provide better insights into how your coding work affects you
- **Work context**: Note what projects or tasks correlate with mood changes
- **Schedule flexibility**: Adjust work hours in settings to match your actual development schedule (early mornings, late nights, etc.)
- **Long-term patterns**: Use visualizations to identify which types of development work impact your mood most

#### Troubleshooting ####

- **Missing MAUI workloads**: `dotnet workload install maui` (ignore advertising manifest warnings)
- **NuGet package not found**: Add official source `dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org`
- **Restore issues**: `dotnet restore` or clear cache `dotnet nuget locals all --clear`
- **Platform errors**: ensure the TFM (e.g. `net9.0-windows10.0.19041.0`) matches installed SDKs

For deeper build details see the [Build Guide](./BUILD.md) and for feature usage see the [User Guide](./USER-GUIDE.md).

## Documentation ##

- [Build and Deployment Guide](./BUILD.md) - Complete instructions for building and deploying WorkMood
- [User Guide](./USER-GUIDE.md) - Comprehensive guide for using all WorkMood features

### Contributors ✨ ###

<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Hunter-Industries"><img src="https://avatars.githubusercontent.com/u/30634197?v=4?s=100" width="100px;" alt="Hunter Industries Software Development"/><br /><sub><b>Hunter Industries Software Development</b></sub></a><br /><a href="#financial-Hunter-Industries" title="Financial">💵</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/jason-kerney"><img src="https://avatars.githubusercontent.com/u/5097968?v=4?s=100" width="100px;" alt="Jason Kerney"/><br /><sub><b>Jason Kerney</b></sub></a><br /><a href="#ideas-jason-kerney" title="Ideas, Planning, & Feedback">🤔</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/JKerney-HunterIndustries"><img src="https://avatars.githubusercontent.com/u/16826501?v=4?s=100" width="100px;" alt="Jason Kerney"/><br /><sub><b>Jason Kerney</b></sub></a><br /><a href="https://github.com/jason-kerney/WorkMood/commits?author=JKerney-HunterIndustries" title="Code">💻</a></td>
    </tr>
  </tbody>
  <tfoot>
    <tr>
      <td align="center" size="13px" colspan="7">
        <img src="https://raw.githubusercontent.com/all-contributors/all-contributors-cli/1b8533af435da9854653492b1327a23a4dbd0a10/assets/logo-small.svg">
          <a href="https://all-contributors.js.org/docs/en/bot/usage">Add your contributions</a>
        </img>
      </td>
    </tr>
  </tfoot>
</table>

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->
<!-- GENERATED DOCUMENT DO NOT EDIT! -->