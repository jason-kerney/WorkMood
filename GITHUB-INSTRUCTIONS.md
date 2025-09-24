# GitHub Repository Instructions - WorkMood

Welcome to the WorkMood repository! This guide will help you get started with contributing to, building, and deploying the WorkMood application.

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Building the Application](#building-the-application)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)

## Overview

WorkMood is a cross-platform desktop application built with .NET 9 and .NET MAUI, designed to help users track how their workday impacts their emotional well-being. The app runs on Windows and macOS platforms.

**Key Technical Details:**
- Built with **.NET 9.0**
- Uses .NET MAUI for cross-platform desktop UI
- Target Frameworks: `net9.0-windows10.0.19041.0` and `net9.0-maccatalyst`
- Supports Windows 10 (1903+), Windows 11, and macOS Big Sur (11.0)+

## Prerequisites

### Required Software

#### .NET SDK
- **.NET 9.0 SDK** (required)
- Download from: https://dotnet.microsoft.com/download/dotnet/9.0

#### Development Environment
Choose one of the following:

**Visual Studio 2022 (Recommended)**
- Version 17.8 or later
- Workloads required:
  - **.NET Multi-platform App UI development**
  - **Mobile development with .NET**

**Visual Studio Code**
- Latest version with C# extension
- .NET MAUI extension (optional but recommended)

#### Platform-Specific Requirements

**For Windows Development:**
- Windows 11 (recommended) or Windows 10 version 1903+
- Windows App SDK (included with Visual Studio 2022)

**For macOS Development:**
- macOS Big Sur (11.0) or later
- Xcode 13.0 or later
- Command Line Tools for Xcode

### Verify Installation

Run these commands to verify your setup:

```bash
dotnet --version
dotnet workload list
```

Expected output should show:
- .NET version 9.0.x or later
- `maui` workload installed

If MAUI workload is missing, install it:
```bash
dotnet workload install maui
```

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/jason-kerney/WorkMood.git
cd WorkMood
```

### 2. Restore Dependencies

```bash
cd MauiApp
dotnet restore WorkMood.MauiApp.csproj
```

### 3. Build the Solution

```bash
# Build entire solution
dotnet build ../WorkMood.sln

# Or build just the MAUI app
dotnet build WorkMood.MauiApp.csproj
```

### 4. Run the Application

```bash
# Run with default framework detection
dotnet run --project WorkMood.MauiApp.csproj

# Run on Windows specifically
dotnet run --project WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# Run on macOS specifically  
dotnet run --project WorkMood.MauiApp.csproj --framework net9.0-maccatalyst
```

## Building the Application

### Framework-Specific Builds

**Important:** When building the MAUI app, you **must** pass the `--framework` parameter with the OS-appropriate version of .NET 9.

#### Windows Build
```bash
dotnet build WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0 -c Release
```

#### macOS Build
```bash
dotnet build WorkMood.MauiApp.csproj --framework net9.0-maccatalyst -c Release
```

### Build Configurations

#### Debug Build (Development)
```bash
dotnet build WorkMood.MauiApp.csproj -c Debug --framework net9.0-windows10.0.19041.0
```

#### Release Build (Production)
```bash
dotnet build WorkMood.MauiApp.csproj -c Release --framework net9.0-windows10.0.19041.0
```

### Using VS Code Tasks

The repository includes predefined tasks for common operations:

```bash
# Build the solution
dotnet run --project . -- task build

# Publish the solution  
dotnet run --project . -- task publish

# Watch mode for development
dotnet run --project . -- task watch
```

Or use VS Code's task runner (Ctrl+Shift+P → "Tasks: Run Task"):
- **build** - Builds the entire solution
- **publish** - Publishes the solution for deployment
- **watch** - Runs in watch mode for development

## Development Workflow

### Project Structure

```
WorkMood/
├── MauiApp/                    # Main MAUI application
│   ├── WorkMood.MauiApp.csproj # Project file with .NET 9 targeting
│   ├── MauiProgram.cs          # App initialization
│   ├── Pages/                  # XAML pages and code-behind
│   ├── ViewModels/             # MVVM view models
│   ├── Services/               # Business logic and data services
│   └── Models/                 # Data models
├── docs/                       # Documentation (auto-generated)
├── WorkMood.sln                # Solution file
└── BUILD.md                    # Detailed build instructions
```

### Code Style

- Follow Microsoft's C# coding conventions
- Use nullable reference types (enabled in project)
- MVVM pattern for UI logic
- Dependency injection for services

### Making Changes

1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Make your changes
3. Build and test: `dotnet build && dotnet test` (if tests exist)
4. Commit with clear messages
5. Push and create a pull request

## Testing

### Running Tests

```bash
# Run all tests (if test projects exist)
dotnet test WorkMood.sln

# Run tests for specific framework
dotnet test --framework net9.0
```

### Manual Testing

Test the application on your target platform:

```bash
# Windows testing
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# macOS testing
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-maccatalyst
```

## Deployment

### Publishing for Distribution

#### Windows Deployment
```bash
cd MauiApp
dotnet publish WorkMood.MauiApp.csproj \
  -c Release \
  --framework net9.0-windows10.0.19041.0 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true
```

#### macOS Deployment
```bash
cd MauiApp
dotnet publish WorkMood.MauiApp.csproj \
  -c Release \
  --framework net9.0-maccatalyst \
  --self-contained true
```

### Published Files Location

After publishing, find your deployment files at:
- **Windows**: `MauiApp/bin/Release/net9.0-windows10.0.19041.0/publish/`
- **macOS**: `MauiApp/bin/Release/net9.0-maccatalyst/publish/`

## Contributing

### Before Contributing

1. Read the existing code and documentation
2. Check open issues for planned work
3. Ensure your development environment meets all prerequisites
4. Test your changes on your target platform(s)

### Pull Request Guidelines

- Include clear description of changes
- Reference any related issues
- Ensure builds pass on both Windows and macOS (if possible)
- Update documentation if needed
- Follow the existing code style and patterns

### Areas for Contribution

- Bug fixes and performance improvements
- UI/UX enhancements
- Cross-platform compatibility improvements
- Documentation improvements
- Test coverage

## Troubleshooting

### Common Build Issues

#### "workload 'maui' not found"
```bash
dotnet workload install maui
```

#### Framework targeting errors
Ensure you're using the correct framework parameter:
- Windows: `--framework net9.0-windows10.0.19041.0`
- macOS: `--framework net9.0-maccatalyst`

#### NuGet restore failures
```bash
dotnet nuget locals all --clear
dotnet restore WorkMood.MauiApp.csproj --force
```

#### Clean build after errors
```bash
dotnet clean WorkMood.MauiApp.csproj
dotnet restore WorkMood.MauiApp.csproj
dotnet build WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0
```

### Platform-Specific Issues

#### Windows Issues
- Ensure Windows App SDK is installed
- Check Windows version compatibility (10.0.17763.0 minimum)
- Verify Visual Studio workloads are installed

#### macOS Issues  
- Ensure Xcode is installed and up to date
- Install Xcode Command Line Tools: `xcode-select --install`
- Check macOS version compatibility (11.0+ required)

### Getting Help

1. Check existing [issues](https://github.com/jason-kerney/WorkMood/issues)
2. Review the detailed [BUILD.md](./BUILD.md) guide
3. Create a new issue with:
   - Your operating system and version
   - .NET SDK version (`dotnet --version`)
   - Complete error messages
   - Steps to reproduce the problem

## Additional Resources

- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [.NET 9 Release Notes](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-9)
- [User Guide](./USER-GUIDE.md) - How to use the application
- [Detailed Build Guide](./BUILD.md) - Comprehensive build and deployment instructions

---

Thank you for your interest in contributing to WorkMood! 🎯