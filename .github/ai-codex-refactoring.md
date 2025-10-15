# AI Codex: Refactoring Methodology

## Overview
This codex provides comprehensive guidance for systematic refactoring within the WorkMood application, focusing on the Shim Factory Pattern, disciplined incremental approaches, and dependency injection transformation.

**When to Reference**: Planning refactoring sessions; implementing dependency injection patterns; creating shim abstractions; systematic code improvement; transforming hard dependencies into testable abstractions.

## Refactoring Philosophy: Disciplined Incremental Approach

### Core Principles
**Safety and Testability First** - Every refactoring decision prioritizes code safety, maintainability, and testability over speed or convenience.

#### Fundamental Rules
1. **ONE Method at a Time**: Never batch refactor multiple methods simultaneously
2. **Manual Verification**: Test after each individual change and ask user to verify behavior
3. **Small Commits**: One method/concept per commit using Arlo's notation
4. **Preserve Behavior**: Zero functional changes during refactoring - only structural improvements
5. **Systematic Progression**: Follow established sequence for each method transformation

#### Why This Approach Works
- **Risk Mitigation**: Small changes reduce probability of introducing bugs
- **Easy Rollback**: Individual commits allow precise rollback of problematic changes
- **Clear History**: Each commit tells story of exactly what was changed and why
- **Testable Progress**: Every step can be validated independently
- **Maintainable Pace**: Sustainable approach that doesn't overwhelm or rush changes

### Refactoring vs. Feature Development
**Refactoring Sessions** are distinct from feature development:
- **Purpose**: Improve code structure without changing behavior
- **Scope**: Internal code quality improvements only
- **Testing**: Behavior must remain identical after each change
- **Commits**: Use `^r` risk/intention notation exclusively
- **Timeline**: Can span multiple days with incremental progress

## Shim Factory Pattern Implementation

### Pattern Overview
The Shim Factory Pattern abstracts external dependencies through three layers:
1. **Shim Interface**: Defines contract for dependency operations
2. **Shim Implementation**: Wraps external dependency with testable interface
3. **Shim Factory**: Creates shim instances with proper configuration

### Benefits of Shim Factory Pattern
- **Complete Testability**: Mock entire external dependency surface
- **Dependency Isolation**: Isolate code from framework changes
- **Consistent Abstraction**: Uniform interface across different dependency types
- **Configuration Control**: Centralize dependency configuration and creation
- **Future Flexibility**: Easy to swap implementations or add new platforms

### Implementation Process: Two-Phase Approach

#### Phase 1: Infrastructure Setup (if not exists)
Complete this entire phase before starting method refactoring:

**Step 1: Create Shim Interface**
```csharp
// Example: File operations shim interface
public interface IFileShim : IDisposable
{
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
    Task<byte[]> ReadAllBytesAsync(string path);
    Task WriteAllBytesAsync(string path, byte[] bytes);
    bool Exists(string path);
    Task DeleteAsync(string path);
}
```

**Step 2: Create Shim Implementation**
```csharp
// Example: File operations shim implementation
public class FileShim : IFileShim
{
    private readonly string _filePath;
    
    public FileShim(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }
    
    public async Task<string> ReadAllTextAsync(string path) => await File.ReadAllTextAsync(path);
    public async Task WriteAllTextAsync(string path, string content) => await File.WriteAllTextAsync(path, content);
    public async Task<byte[]> ReadAllBytesAsync(string path) => await File.ReadAllBytesAsync(path);
    public async Task WriteAllBytesAsync(string path, byte[] bytes) => await File.WriteAllBytesAsync(path, bytes);
    public bool Exists(string path) => File.Exists(path);
    public async Task DeleteAsync(string path) => await Task.Run(() => File.Delete(path));
    
    public void Dispose() { /* Cleanup if needed */ }
}
```

**Step 3: Create Factory Interface**
```csharp
// Example: File shim factory interface
public interface IFileShimFactory
{
    IFileShim CreateFile(string filePath);
    IFileShim CreateTemporaryFile();
    IFileShim CreateFileWithBackup(string filePath, string backupPath);
}
```

**Step 4: Create Factory Implementation**
```csharp
// Example: File shim factory implementation
public class FileShimFactory : IFileShimFactory
{
    public IFileShim CreateFile(string filePath)
    {
        return new FileShim(filePath);
    }
    
    public IFileShim CreateTemporaryFile()
    {
        var tempPath = Path.GetTempFileName();
        return new FileShim(tempPath);
    }
    
    public IFileShim CreateFileWithBackup(string filePath, string backupPath)
    {
        // Could create specialized shim that handles backup logic
        return new FileShim(filePath);
    }
}
```

**Step 5: Update Service Constructor**
```csharp
// Before: Direct dependency
public class MoodDataService
{
    public MoodDataService() { }  // Direct File usage throughout
}

// After: Factory injection
public class MoodDataService
{
    private readonly IFileShimFactory _fileShimFactory;
    
    public MoodDataService(IFileShimFactory fileShimFactory)
    {
        _fileShimFactory = fileShimFactory ?? throw new ArgumentNullException(nameof(fileShimFactory));
    }
}
```

#### Phase 2: Method-by-Method Refactoring
Apply this sequence to **each method individually**:

**CRITICAL SEQUENCE** (never deviate):

**Step 1: Add Factory Method**
Extend factory interface with method needed for current refactoring target:
```csharp
// Add to IDrawingShimFactory for graphics refactoring
public interface IDrawingShimFactory
{
    // Existing methods...
    IPaintShim CreateBackgroundPaint(); // Add this for DrawBackground refactoring
}
```

**Step 2: Implement Factory Method**
Add concrete implementation to factory:
```csharp
public class DrawingShimFactory : IDrawingShimFactory
{
    // Existing methods...
    
    public IPaintShim CreateBackgroundPaint()
    {
        return new PaintShim(new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });
    }
}
```

**Step 3: Refactor Target Method**
Transform the specific method to use factory:
```csharp
// Before: Hard dependency
private void DrawBackground(SKCanvas canvas, SKRect area)
{
    using var backgroundPaint = new SKPaint  // Hard dependency here
    {
        Color = SKColors.White,
        Style = SKPaintStyle.Fill
    };
    canvas.DrawRect(area, backgroundPaint);
}

// After: Factory usage
private void DrawBackground(SKCanvas canvas, SKRect area)
{
    using var backgroundPaint = _drawingShimFactory.CreateBackgroundPaint(); // Factory call
    canvas.DrawRect(area, backgroundPaint.SKPaint);
}
```

**Step 4: Manual Test**
- Run application manually
- Verify identical behavior
- Ask user to confirm everything works as expected
- **Do not proceed without confirmation**

**Step 5: Commit**
Use exact Arlo's notation format:
```bash
^r - extract DrawBackground to use color factory for dependency injection
```

**Step 6: Next Method**
Move to next method in service and repeat entire sequence

### Advanced Shim Patterns

#### Argument-Based Shim Creation
For complex object creation scenarios:
```csharp
// Shim arguments for flexible creation
public class PaintShimArgs
{
    public IColorShim? Color { get; set; }
    public SKPaintStyle Style { get; set; } = SKPaintStyle.Fill;
    public bool IsAntialias { get; set; } = true;
    public float StrokeWidth { get; set; } = 1.0f;
    public IPathEffectShim? PathEffect { get; set; }
}

// Factory method using arguments
public IPaintShim CreatePaint(PaintShimArgs args)
{
    var paint = new SKPaint
    {
        Style = args.Style,
        IsAntialias = args.IsAntialias,
        StrokeWidth = args.StrokeWidth
    };
    
    if (args.Color != null)
        paint.Color = args.Color.SKColor;
        
    if (args.PathEffect != null)
        paint.PathEffect = args.PathEffect.SKPathEffect;
        
    return new PaintShim(paint);
}
```

#### Composite Shim Factories
For related dependency groups:
```csharp
public interface IDrawingShimFactory
{
    // Paint operations
    IPaintShim CreatePaint(PaintShimArgs args);
    IPaintShim CreateSolidBrush(IColorShim color);
    IPaintShim CreatePen(IColorShim color, float width);
    
    // Color operations
    IColorShim CreateColor(byte r, byte g, byte b, byte a = 255);
    IColorShim WhiteColor();
    IColorShim BlackColor();
    IColorShim TransparentColor();
    
    // Canvas operations
    ICanvasShim CreateCanvas(SKCanvas canvas);
    ICanvasShim CreateCanvas(int width, int height);
}
```

## Existing Shim Abstractions

### File System Shims (Fully Implemented)
**Files**: `Shims/FileShims.cs`, `Factories/FileShimFactory.cs`

#### IFileShim / FileShim
- **Purpose**: Abstract individual file operations
- **Methods**: ReadAllTextAsync, WriteAllTextAsync, ReadAllBytesAsync, WriteAllBytesAsync, Exists, DeleteAsync
- **Usage**: Single file operations with full async support

#### IFolderShim / FolderShim  
- **Purpose**: Abstract directory operations
- **Methods**: CreateDirectory, DeleteDirectory, EnumerateFiles, Exists, GetDirectories
- **Usage**: Directory manipulation and file system traversal

#### IFileShimFactory / FileShimFactory
- **Purpose**: Create file and folder shims
- **Methods**: CreateFile, CreateFolder, CreateTemporaryFile, CreateBackupFile
- **Usage**: Centralized file system object creation

### Drawing Shims (SkiaSharp - Implemented in `DrawingShims.cs`)
**File**: `Shims/DrawingShims.cs`

#### IPaintShim / PaintShim
- **Purpose**: Abstract SKPaint operations
- **Arguments**: PaintShimArgs for flexible configuration
- **Properties**: Color, Style, StrokeWidth, IsAntialias, PathEffect
- **Usage**: All drawing operations requiring paint/brush

#### IColorShim / ColorShim
- **Purpose**: Abstract color creation and manipulation
- **Methods**: FromRgb, FromArgb, WithAlpha, ToHex
- **Properties**: R, G, B, A components, SKColor access
- **Usage**: Color calculations and conversions

#### ITypeFaceShim / TypeFaceShim
- **Purpose**: Abstract font and typography operations
- **Methods**: CreateFromFamily, CreateFromFile, GetMetrics
- **Usage**: Text rendering and font management

#### IPathEffectShim / PathEffectShim
- **Purpose**: Abstract line styles and effects
- **Methods**: CreateDash, CreateCorner, CreateCompose
- **Usage**: Advanced line drawing effects

#### ICanvasShim / CanvasShim
- **Purpose**: Abstract drawing canvas operations
- **Methods**: DrawRect, DrawCircle, DrawPath, DrawText, Save, Restore
- **Usage**: All drawing operations through abstracted canvas

### Utility Shims (Implemented)
**Files**: `Shims/UtilityShims.cs`

#### IDateShim / DateShim
- **Purpose**: Abstract date/time operations for testing
- **Methods**: Now(), Today(), UtcNow(), AddDays(), Format()
- **Usage**: All date/time calculations and formatting

#### IJsonSerializerShim / JsonSerializerShim
- **Purpose**: Abstract JSON serialization operations
- **Methods**: SerializeAsync, DeserializeAsync, SerializeToString, DeserializeFromString
- **Usage**: All JSON data persistence operations

#### IBrowserShim / BrowserShim
- **Purpose**: Abstract web browser launching
- **Methods**: OpenAsync, CanOpenUrl, OpenUrlInDefaultBrowser
- **Usage**: External URL navigation and web authentication

## Refactoring Targets by Priority

### High Impact Dependencies (Address First)
These dependencies appear frequently and have significant testing/maintenance impact:

#### File I/O Operations
- **Target**: `File.ReadAllTextAsync()`, `File.WriteAllBytesAsync()`, `File.Exists()`
- **Impact**: Data persistence, configuration loading, export functionality
- **Shim**: Use `IFileShimFactory` → `IFileShim`
- **Priority**: Highest - affects core application functionality

#### Object Creation
- **Target**: `new SKBitmap()`, `new SKPaint()`, `new SKCanvas()`
- **Impact**: All graphics rendering and chart generation
- **Shim**: Use `IDrawingShimFactory` → various drawing shims
- **Priority**: Highest - affects primary application features

#### Constants and Static Values
- **Target**: `SKColors.White`, `SKColors.Black`, `Path.DirectorySeparatorChar`
- **Impact**: Cross-platform compatibility, theming, file system operations
- **Shim**: Use factory methods for color/path constants
- **Priority**: High - affects platform-specific behavior

### Medium Impact Dependencies
These dependencies are less frequent but still important for testing:

#### Path Effects and Styles
- **Target**: `SKPathEffect.CreateDash()`, `SKPaintStyle.Fill`
- **Impact**: Advanced graphics features, chart styling
- **Shim**: Use `IPathEffectShim`, paint style factories
- **Priority**: Medium - affects visual appearance

#### Image Encoding
- **Target**: `SKEncodedImageFormat.Png`, `image.Encode()`
- **Impact**: Export functionality, image generation
- **Shim**: Use encoding shims and format factories
- **Priority**: Medium - affects export features

### Lower Impact Dependencies
These can be addressed after higher priorities:

#### Geometric Objects
- **Target**: `new SKRect()`, `new SKPoint()`, `SKMatrix.CreateScale()`
- **Impact**: Graphics calculations, layout positioning
- **Shim**: Geometric object factories
- **Priority**: Lower - mostly calculations

#### Async Patterns
- **Target**: `Task.Run()`, `Task.Delay()`, `CancellationToken`
- **Impact**: Async operation testing
- **Shim**: Async operation abstractions
- **Priority**: Lower - less critical for functionality

## Anti-Patterns to Avoid

### ❌ Big Bang Refactoring
**Problem**: Attempting to refactor multiple methods or entire classes simultaneously

**Why It Fails**:
- Increases risk of introducing bugs exponentially
- Makes it impossible to isolate issues when problems occur
- Creates commits that are too large to review effectively
- Overwhelming scope leads to shortcuts and mistakes

**Instead**: Refactor ONE method at a time, commit after each method

### ❌ Batch Commits
**Problem**: Combining multiple method refactors into a single commit

**Why It's Harmful**:
- Loses granular history of what was changed when
- Makes it impossible to rollback specific problematic changes
- Violates single responsibility principle for commits
- Makes code review difficult and less effective

**Instead**: One commit per method refactored, with descriptive Arlo's notation

### ❌ Breaking Changes During Refactoring
**Problem**: Changing public APIs, method signatures, or behavior during internal refactoring

**Why It's Dangerous**:
- Mixes refactoring with feature changes, violating refactoring definition
- Makes it impossible to verify behavior preservation
- Can break dependent code unexpectedly
- Complicates rollback if issues arise

**Instead**: Preserve all public interfaces exactly; only change internal implementation

### ❌ Skip Testing
**Problem**: Committing refactored code without manual verification

**Why It's Risky**:
- Behavior changes can be subtle and not caught by automated tests
- User-visible regressions may not be immediately apparent
- Lost opportunity to verify refactoring actually improved code
- Builds technical debt through unverified changes

**Instead**: Test manually after each method and get user confirmation

### ❌ Inadequate Shim Interfaces
**Problem**: Creating shim interfaces that are too broad or too narrow

**Too Broad Issues**:
- Violates interface segregation principle
- Forces implementations to handle unrelated concerns
- Makes testing more complex than necessary

**Too Narrow Issues**:
- Requires multiple interfaces for related operations
- Creates artificial boundaries that don't reflect real usage
- Leads to excessive interface proliferation

**Instead**: Design shim interfaces around client usage patterns, not implementation details

### ❌ Premature Abstraction
**Problem**: Creating shims for dependencies that don't actually need abstraction

**When to Avoid Shimming**:
- Simple data structures that don't change (`DateTime`, `string`, `int`)
- Framework types that are already well-abstracted (`IEnumerable<T>`, `Task<T>`)
- Dependencies that are unlikely to be mocked in tests
- Internal implementation details not visible to external clients

**Instead**: Focus on dependencies that genuinely benefit from abstraction and testing

## Historical Reference & Success Metrics

### Proven Methodology Results
**Reference Commits**: `6d7b2feb` through `e7c36441` applied to `LineGraphService.cs`

**Quantitative Success**:
- **20+ individual commits** over 2-day period
- **Zero breaking changes** to public APIs
- **100% behavior preservation** verified manually
- **Complete SkiaSharp abstraction** achieved
- **Full test coverage** enabled through shims

**Qualitative Improvements**:
- **Maintainability**: Code became easier to understand and modify
- **Testability**: Complete isolation of external dependencies achieved
- **Flexibility**: Easy to swap rendering implementations in future
- **Documentation**: Clear commit history showing transformation process

### Success Patterns Observed

#### Rhythm and Pacing
- **Daily Sessions**: 2-3 hours maximum to avoid fatigue
- **Method Frequency**: 3-5 methods per day depending on complexity
- **Break Points**: Natural stopping points after each commit
- **Progress Tracking**: Clear visibility into what's completed vs. remaining

#### Risk Management
- **No Rollbacks Required**: Every commit was successful due to incremental approach
- **User Confidence**: Each change was verified before proceeding
- **Minimal Debugging**: Small changes made issues easy to identify and fix
- **Sustainable Pace**: No pressure or rushing led to higher quality results

#### Team Collaboration
- **Clear History**: Other developers could understand each transformation step
- **Easy Reviews**: Small commits enabled thorough code review
- **Knowledge Transfer**: Methodology became reusable for other refactoring efforts
- **Continuous Integration**: Build never broken due to incremental changes

This methodology prioritizes **safety, testability, and maintainability** over speed, leading to sustainable improvements in code quality and development velocity.

---

*This codex serves as the definitive guide for all refactoring activities within the WorkMood application. All systematic code improvements should follow these established patterns and principles to ensure safety, testability, and long-term maintainability.*