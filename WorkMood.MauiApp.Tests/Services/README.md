# LineGraphService Approval Tests

This directory contains comprehensive approval tests for the `LineGraphService` class using [ApprovalTests.Net](https://github.com/approvals/ApprovalTests.Net).

## Overview

These tests verify the visual output of generated graphs by comparing actual PNG images against approved baseline images. This approach provides high confidence that the rendering logic works correctly and catches visual regressions.

## Test Structure

### Core Test Files

- **`LineGraphServiceApprovalTests.cs`** - Main tests covering all GraphMode variations (Impact, Average, RawData) with different configurations
- **`LineGraphServiceBackgroundTests.cs`** - Tests for custom background image functionality and color contrast scenarios  
- **`LineGraphServiceEdgeCaseTests.cs`** - Comprehensive edge cases including sparse/dense data, boundary values, and performance stress tests
- **`ApprovalTestConfiguration.cs`** - Global configuration for consistent test behavior

### Test Coverage

#### Graph Modes
- **Impact Mode**: Shows mood change throughout the day (-9 to +9 range)
- **Average Mode**: Shows average mood for the day (-5 to +5 range) 
- **RawData Mode**: Shows individual mood recordings (1 to 10 range)

#### Configuration Options
- With/without data points
- With/without axes and grid lines
- With/without titles
- Various line colors
- Different canvas sizes (400x300 to 1600x800)

#### Background Types
- Solid white (default)
- Custom solid colors
- Gradient backgrounds
- Pattern backgrounds
- Non-existent files (fallback behavior)

#### Edge Cases
- Empty datasets
- Single data points
- Very sparse data (1 entry in 30 days)
- Very dense data (365 daily entries)
- Boundary values (all 1s, all 10s)
- Null value handling
- Data outside date ranges
- Same timestamps
- Midnight edge times
- Zero impact scenarios

## Running the Tests

### Prerequisites
1. .NET 9.0 or later
2. Visual Studio or compatible IDE with test runner
3. A diff tool (optional but recommended):
   - Beyond Compare
   - WinMerge  
   - VS Code with diff extension
   - Any tool supported by ApprovalTests

### First Run
```bash
# Run all approval tests
dotnet test --filter "FullyQualifiedName~LineGraphService*Approval*"

# Run specific test class
dotnet test --filter "FullyQualifiedName~LineGraphServiceApprovalTests"
```

On first run, all tests will **fail** because there are no approved baseline images yet.

### Approving Baselines

When tests fail, ApprovalTests will:
1. Generate actual images in `TestResults/` folder
2. Show diff between actual vs approved (empty on first run)
3. Wait for you to approve or reject the images

#### Manual Approval
1. Navigate to the test output directory
2. Review the generated `.received.png` files
3. If they look correct, rename them to `.approved.png`
4. Re-run the tests (should now pass)

#### Automatic Approval (with diff tool)
1. ApprovalTests will launch your configured diff tool
2. Review the generated images 
3. Accept/approve them in the diff tool
4. Tests will pass on next run

### Approval File Organization

```
WorkMood.MauiApp.Tests/
├── Services/
│   ├── ApprovalFiles/
│   │   ├── LineGraphServiceApprovalTests.GenerateLineGraph_ImpactMode_WithDataPointsAndGrid_ShouldMatchApproval.Windows_NT.approved.png
│   │   ├── LineGraphServiceApprovalTests.GenerateLineGraph_AverageMode_WithDataPointsAndGrid_ShouldMatchApproval.Windows_NT.approved.png
│   │   └── ... (all other approved baseline images)
│   └── TestImages/
│       ├── blue_background.png
│       ├── gradient_background.png
│       └── ... (generated test backgrounds)
```

## Troubleshooting

### Tests Failing After Code Changes

This is expected! The tests are designed to catch visual changes.

1. **Intended Changes**: If you modified rendering intentionally
   - Review the new images in diff tool
   - Approve them if they look correct
   - Commit the updated `.approved.png` files

2. **Unintended Changes**: If rendering changed unexpectedly
   - Use the diff tool to see exactly what changed
   - Fix the code to restore expected behavior
   - Re-run tests to verify fix

### Platform Differences

Images may vary slightly between operating systems due to font rendering differences.
- Tests include platform name in approval file names (e.g., `Windows_NT`)
- Run tests on your target deployment platform
- Consider separate approval baselines per platform if needed

### Managing Large Numbers of Approval Files

- Approval files should be committed to source control
- Use `.gitattributes` to mark PNG files as binary
- Consider organizing into subdirectories if collection grows large

## Writing New Tests

### Test Naming Convention
Use descriptive names that clearly indicate what's being tested:
```csharp
[Fact]
public async Task GenerateLineGraph_ImpactMode_WithRedLineColor_ShouldMatchApproval()
```

### Test Structure Template
```csharp
[Fact]
public async Task TestName_ShouldMatchApproval()
{
    // Arrange - Set up test data
    var moodEntries = CreateTestData();
    var dateRange = new DateRange(...);
    
    // Act - Generate the image
    var imageBytes = await _lineGraphService.GenerateLineGraphAsync(...);
    
    // Assert - Verify against approved baseline
    Approvals.VerifyBinaryFile(imageBytes, "png");
}
```

### Adding Test Data Helpers
Place reusable test data creation in helper methods:
```csharp
private static List<MoodEntry> CreateSpecialScenarioData()
{
    return new List<MoodEntry> { /* specific test data */ };
}
```

## Benefits of This Approach

1. **Visual Confidence**: Actually see what the graphs look like
2. **Regression Detection**: Any visual changes are immediately caught
3. **Easy Maintenance**: Just approve new baselines when making intentional changes
4. **Comprehensive Coverage**: Tests all combinations of modes, options, and edge cases
5. **Human Review**: Visual approval process catches issues that unit tests might miss
6. **Documentation**: Approved images serve as visual documentation of expected output