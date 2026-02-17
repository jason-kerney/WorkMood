---
name: build-engineer
description: >
  Guidance for building, troubleshooting, and packaging WorkMood for end users on Windows and macOS. Covers environment setup, .NET MAUI build steps, platform-specific issues, and user-friendly deployment without CI/CD.
user-invokable: true
argument-hint: "[platform] [build/deploy scenario]"
disable-model-invocation: false
---

# Build Engineer Skill (User-Focused)

## When to Use This Skill
- Building WorkMood for personal use on Windows or macOS
- Setting up the development environment (SDKs, dependencies)
- Troubleshooting local build errors
- Packaging and deploying the app for your OS
- Creating installers or signed packages for distribution

---

## Core Concepts
- **.NET MAUI Multi-Platform**: Windows and macOS builds require different SDKs and settings
- **Environment Setup**: Install correct .NET SDK, MAUI workload, and platform tools
- **Explicit Framework Targeting**: Always specify the target framework for your OS
- **Packaging**: Create user-friendly installers or signed apps for each platform
- **Troubleshooting**: Common build errors and how to resolve them

---

## How-To: Build & Run WorkMood

### 1. Environment Setup
- Install [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Install MAUI workload:
  ```sh
  dotnet workload install maui
  ```
- For Windows: Visual Studio 2022+ with MAUI workload
- For macOS: Xcode 15+, .NET 9.0, MAUI workload

### 2. Build for Your Platform
- **Windows:**
  ```sh
  dotnet build --framework net9.0-windows10.0.19041.0
  dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0
  ```
- **macOS:**
  ```sh
  dotnet build --framework net9.0-maccatalyst
  dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-maccatalyst
  ```

### 3. Packaging & Deployment
- **Windows:**
  - Use Visual Studio to publish as MSIX or EXE installer
  - Sign app if distributing externally
- **macOS:**
  - Use `dotnet publish` for .app bundle
  - Notarize and sign for Gatekeeper if needed

---

## Troubleshooting Checklist
- [ ] .NET SDK and MAUI workload installed for your OS
- [ ] Target framework matches your platform
- [ ] All dependencies restored (`dotnet restore`)
- [ ] Platform-specific tools (Xcode, VS) are up to date
- [ ] Check error messages for missing SDKs or permissions
- [ ] Clean build (`dotnet clean` then build again)

---

## Anti-Patterns/When NOT to Use
- Don’t attempt to build for unsupported platforms (no Linux/mobile)
- Don’t skip environment setup—most errors are missing SDKs/tools
- Don’t use CI/CD instructions—this skill is for local, user-driven builds

---

## References
- [WorkMood BUILD.md](../../../../BUILD.md)
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Windows Deployment Guide](../docs/build/windows-deploy.md)
- [macOS Deployment Guide](../docs/build/macos-deploy.md)
