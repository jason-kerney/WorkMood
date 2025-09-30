<!-- (dl (section-meta Prerequisites)) -->

Before compiling WorkMood for personal use, ensure you have the following installed on your machine:

<!-- (dl (# .NET Requirements)) -->

- **.NET 9.0 SDK** or later
- **Visual Studio 2022** (17.8 or later) with the following workloads:
  - **.NET Multi-platform App UI development**
  - **Mobile development with .NET** (includes Android SDK)
- Alternatively, you can use **Visual Studio Code** with the C# extension

<!-- (dl (# Platform-Specific Requirements)) -->

<!-- (dl (## Windows Development)) -->

- **Windows 11** (recommended) or **Windows 10** version 1903 or higher
- **Windows App SDK** (included with Visual Studio 2022)
- **Windows Subsystem for Linux (WSL)** (optional, for cross-platform testing)

<!-- (dl (## macOS Development)) -->

- **macOS Big Sur** (11.0) or later
- **Xcode** 13.0 or later (for Mac Catalyst deployment)
- **Command Line Tools for Xcode**

<!-- (dl (# Additional Tools)) -->

- **Git** for version control
- **PowerShell** 7.0+ (cross-platform, recommended for build scripts)
- **Doculisp CLI** for documentation compilation (if modifying docs)

<!-- (dl (# Verification)) -->

Run the following commands to verify your setup:

```bash
dotnet --version
dotnet workload list
```

You should see `.NET 9.0` or later and the `maui` workload listed.