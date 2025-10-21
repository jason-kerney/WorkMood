# WorkMood Master Test Execution Plan

## Overview

This document provides a comprehensive, step-by-step execution plan for implementing unit tests across the WorkMood codebase. The plan is ordered from easiest to hardest complexity, designed for execution by AI assistants with built-in verification and maintenance protocols.

**Last Updated**: October 20, 2025  
**Status**: In Progress - Component 1 Starting  
**Total Components to Test**: 58

## Recent Updates
- **October 20, 2025**: Systematic test plan update completed - all 59 individual test plans now include standardized xUnit framework specifications
- **Critical Bug Fixed**: Testing framework specification gap resolved across all plans to prevent NUnit vs xUnit confusion
- **Infrastructure Added**: PowerShell automation script (Update-TestPlans.ps1) for systematic documentation updates

---

## Execution Protocols

### Before Starting Any Component
1. **Generate Baseline Coverage**: Run `generate-coverage-report.ps1` and commit the `CoverageReport/Summary.txt` file to establish pre-test coverage baseline
2. **Verify Plan Accuracy**: Read the component's source code and ensure the individual test plan matches reality
3. **Update Sub-Plan**: Add maintenance requirements and checkpoint protocols to the individual plan
4. **Insert Coverage & Master Plan Update Requirements**: Modify the individual test plan to include as completion steps:
   - "Run `generate-coverage-report.ps1` and commit the updated `CoverageReport/Summary.txt` file showing improved coverage"
   - "Before marking this component complete, re-read and update the Master Test Execution Plan with progress, learnings, and any discovered patterns"
5. **Update Master Plan**: Re-read this master plan and update any outdated information, progress tracking, or learned patterns before proceeding
6. **Establish Baseline**: Confirm component location, dependencies, and current test coverage

### During Component Testing
- **Verification Checkpoints**: Pause every 2-3 tests for verification
- **Master Plan Review**: Re-read this master plan every 10 tests to maintain alignment
- **Complexity Escalation**: For complicated scenarios, propose direction but request feedback
- **Plan Updates**: Keep individual test plans current as work progresses
- **Completion Requirement**: Each individual test plan must include updating this master plan as a final step before completion

### Between Each Sub-Plan
- **Generate Post-Test Coverage**: Run `generate-coverage-report.ps1` and commit updated `CoverageReport/Summary.txt` showing coverage improvements
- **MANDATORY VERIFICATION**: Request human confirmation before proceeding to next component
- **Status Report**: Provide summary of what was accomplished, coverage improvements, and any issues encountered
- **Plan Adjustment**: Allow for any necessary tasks or adjustments based on progress

---

## 🟢 PHASE 1: EASY TIER (Components 1-20)

### Component 1: AutoSaveEventArgs
**Complexity**: 1/10 | **Testability**: 9/10  
**Location**: `MauiApp/Services/MoodDispatcherService.cs` (lines 326-330)
**Update**: Corrected location - found as nested class in MoodDispatcherService, not separate file

**Pre-Execution Checklist**:
- [ ] **Update Master Plan**: Re-read and update this master plan with any new learnings or progress
- [ ] Verify component exists at specified location
- [ ] Confirm it's a pure event args class with no dependencies
- [ ] Update individual test plan with maintenance protocols
- [ ] Add verification checkpoints every 2-3 tests

**Test Focus**: Constructor validation, property accessibility, event args pattern compliance

**Expected Duration**: 15-30 minutes

---

### Component 2: AxisRange
**Complexity**: 1/10 | **Testability**: 9/10  
**Location**: `MauiApp/Models/AxisRange.cs`

**Pre-Execution Checklist**:
- [ ] Verify immutable record structure
- [ ] Confirm no external dependencies
- [ ] Update test plan with checkpoint protocols
- [ ] Master plan review (every 10 tests - check if applicable)

**Test Focus**: Immutable record behavior, value equality, property validation

**Expected Duration**: 15-30 minutes

**🔄 VERIFICATION CHECKPOINT**: Request confirmation before proceeding to Component 3

---

### Component 3: DisplayAlertEventArgs
**Complexity**: 1/10 | **Testability**: 9/10  
**Location**: `MauiApp/ViewModels/MainPageViewModel.cs` (lines 377-394)
**Update**: Corrected location - found as nested class in MainPageViewModel, not separate file

**Pre-Execution Checklist**:
- [ ] Verify simple event args structure
- [ ] Confirm property-only implementation
- [ ] Update individual test plan
- [ ] Add verification checkpoints

**Test Focus**: Property validation, constructor testing, event args compliance

**Expected Duration**: 15-30 minutes

---

### Component 4: MorningReminderEventArgs
**Complexity**: 1/10 | **Testability**: 9/10  
**Location**: `MauiApp/Services/MoodDispatcherService.cs` (lines 335-342)
**Update**: Corrected location - found as nested class in MoodDispatcherService, not separate file

**Pre-Execution Checklist**:
- [x] Verify event args pattern implementation
- [x] Confirm no business logic complexity
- [x] Update test plan with protocols
- [x] Check for any DateTime dependencies

**Test Focus**: Event args structure, property validation, constructor testing

**Expected Duration**: 15-30 minutes
**Actual Duration**: 45 minutes ✅ **COMPLETED**

**🔄 VERIFICATION CHECKPOINT**: Request confirmation before proceeding to Component 5

---

### Component 5: DateRangeItem
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/ViewModels/GraphViewModel.cs` (lines 896-907)
**Update**: Corrected location - found as class in GraphViewModel file, not separate Models file

**Pre-Execution Checklist**:
- [x] ✅ **Master Plan Updated**: Updated with learnings from Components 1-4
- [x] ✅ **Location Verified**: Component confirmed at GraphViewModel.cs lines 896-907 
- [x] ✅ **Dependencies Confirmed**: Single IDateShim dependency, wrapper class pattern
- [x] ✅ **Individual Plan Updated**: Enhanced with completion requirements and coverage tracking
- [x] ✅ **Coverage Baseline**: Established 0% baseline coverage for DateRangeItem

**Test Focus**: Property testing, IDateShim integration, UI binding compatibility, DateRangeInfo wrapper testing
**Status**: ✅ **COMPLETED** - 38 tests implemented with 100% coverage achieved
**Duration**: ~90 minutes including dependency mocking setup

---

### Component 6: GraphModeItem
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/Models/GraphModeItem.cs`

**Pre-Execution Checklist**:
- [ ] Verify UI binding model
- [ ] Confirm ToString() implementation
- [ ] Update test plan protocols
- [ ] Check for enum or mode dependencies

**Test Focus**: UI binding properties, ToString() validation, equality testing

**Expected Duration**: 20-35 minutes

**🔄 VERIFICATION CHECKPOINT**: Request confirmation before proceeding to Component 7

---

### Component 7: RelayCommand
**Complexity**: 2/10 | **Testability**: 9/10  
**Location**: `MauiApp/Infrastructure/RelayCommand.cs`

**Pre-Execution Checklist**:
- [ ] Verify command pattern implementation
- [ ] Confirm delegate injection design
- [ ] Update individual test plan
- [ ] Check CommandManager dependencies

**Test Focus**: ICommand interface, Execute/CanExecute logic, delegate handling

**Expected Duration**: 25-40 minutes

---

### Component 8: RelayCommand<T>
**Complexity**: 2/10 | **Testability**: 9/10  
**Location**: `MauiApp/Infrastructure/RelayCommand.cs`

**Pre-Execution Checklist**:
- [x] ✅ **Master Plan Updated**: Updated with learnings from Components 1-7
- [x] ✅ **Location Verified**: Component confirmed at Infrastructure/RelayCommand.cs lines 50-89
- [x] ✅ **Dependencies Confirmed**: Mixed delegate signatures - reference types use Action<T?>, value types use Action<T>
- [x] ✅ **Individual Plan Updated**: Enhanced with completion requirements and coverage tracking
- [x] ✅ **Coverage Baseline**: Established 0% baseline coverage for RelayCommand<T>

**Test Focus**: Generic type safety, parameter validation, ICommand compliance
**Status**: ✅ **COMPLETED** - 30 tests implemented with 100% coverage achieved
**Duration**: ~90 minutes including namespace conflict resolution

**Key Learnings**:
- **Namespace Conflict Discovery**: RelayCommand<T> exists in both Infrastructure and ViewModels namespaces
- **Mixed Delegate Signatures**: Reference types (string) use `Action<string?>`, value types (int) use `Action<int>`
- **Nullable Behavior**: `RelayCommand<int?>` does NOT accept null parameters despite nullable type
- **Type Safety Implementation**: Runtime type checking prevents invalid parameter execution
- **Using Alias Solution**: Resolved namespace conflict with `using RelayCommand = WorkMood.MauiApp.Infrastructure.RelayCommand;`

**Testing Patterns Established**:
- **3-Checkpoint Methodology**: Applied successfully across ICommand interface, generic type safety, edge cases
- **Type-Specific Testing**: Comprehensive testing across reference types, value types, nullable types
- **Parameter Validation**: Verified type checking, conversion handling, null safety
- **Integration Testing**: CommandManager coordination, MVVM patterns, event handling

**🔄 VERIFICATION CHECKPOINT**: Request confirmation before proceeding to Component 9

---

### Component 9: CommandManager
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/Infrastructure/RelayCommand.cs` (lines 40-48)
**Update**: Corrected location - found as static class in RelayCommand.cs file, not separate file

**Pre-Execution Checklist**:
- [x] ✅ **Master Plan Updated**: Updated with learnings from Components 1-8
- [x] ✅ **Location Verified**: Component confirmed at Infrastructure/RelayCommand.cs lines 40-48
- [x] ✅ **Dependencies Confirmed**: Static event coordination class with EventHandler and EventArgs
- [x] ✅ **Individual Plan Updated**: Enhanced with completion requirements and coverage tracking
- [x] ✅ **Coverage Baseline**: Established 100% baseline coverage (from RelayCommand integration)

**Test Focus**: Static event coordination, subscription/notification patterns
**Status**: ✅ **COMPLETED** - 18 tests implemented with maintained 100% coverage
**Duration**: ~60 minutes including comprehensive static class testing

**Key Learnings**:
- **Location Discovery**: CommandManager is nested in RelayCommand.cs file, not separate infrastructure file
- **Pre-existing Coverage**: Already had 100% coverage from RelayCommand integration testing
- **Static Event Behavior**: .NET event invocation stops at first exception (documented expected behavior)
- **Event State Management**: Static events maintain state across all callers and multiple invocations
- **Integration Pattern**: Works seamlessly with RelayCommand CanExecuteChanged subscription pattern

**Testing Patterns Established**:
- **3-Checkpoint Methodology**: Applied successfully across static class structure, event management, integration patterns
- **Static Class Testing**: Comprehensive testing of static event coordination without instantiation
- **Event Lifecycle Testing**: Subscription, unsubscription, multiple handlers, exception handling
- **Integration Testing**: RelayCommand pattern integration, multiple command type coordination

**Expected Duration**: 30-45 minutes
**Actual Duration**: ~60 minutes including static class methodology development

---

### Component 10: MorningReminderData
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/Models/MorningReminderData.cs`

**Pre-Execution Checklist**:
- [ ] Verify data transfer object structure
- [ ] Confirm validation logic
- [ ] Update test plan protocols
- [ ] Master plan review (every 10 tests)

**Test Focus**: Property validation, business rules, data integrity

**Expected Duration**: 25-40 minutes

**🔄 VERIFICATION CHECKPOINT**: Request confirmation before proceeding to Component 11

---

### Components 11-15: Simple Converters Group
**Batch Processing Recommended**: These are all similar converter implementations

#### Component 11: InverseBoolConverter
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/Converters/InverseBoolConverter.cs`

#### Component 12: InvertedBoolConverter  
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/Converters/InvertedBoolConverter.cs`

#### Component 13: IsNotNullConverter
**Complexity**: 2/10 | **Testability**: 8/10  
**Location**: `MauiApp/Converters/IsNotNullConverter.cs`

#### Component 14: BoolToColorConverter
**Complexity**: 3/10 | **Testability**: 7/10  
**Location**: `MauiApp/Converters/BoolToColorConverter.cs`

#### Component 15: MoodEmojiConverter
**Complexity**: 3/10 | **Testability**: 7/10  
**Location**: `MauiApp/Converters/MoodEmojiConverter.cs`

**Group Pre-Execution Checklist**:
- [ ] Verify all converters implement IValueConverter
- [ ] Confirm Convert/ConvertBack method signatures
- [ ] Update individual test plans for each
- [ ] Check for any parameter dependencies

**Group Test Focus**: IValueConverter compliance, conversion logic, parameter handling, edge cases

**Expected Duration**: 2-3 hours total

**🔄 VERIFICATION CHECKPOINT**: Request confirmation before proceeding to Component 16

---

### Components 16-20: Final Easy Tier Group

#### Component 16: MoodAverageConverter
**Complexity**: 3/10 | **Testability**: 7/10

#### Component 17: NullableMoodConverter  
**Complexity**: 3/10 | **Testability**: 7/10

#### Component 18: ScheduleOverride
**Complexity**: 2/10 | **Testability**: 8/10

#### Component 19: ScheduleConfig
**Complexity**: 2/10 | **Testability**: 8/10

#### Component 20: ViewModelBase
**Complexity**: 2/10 | **Testability**: 8/10

**Group Expected Duration**: 2.5-4 hours total

**🔄 PHASE 1 COMPLETION CHECKPOINT**: 
- Provide comprehensive summary of Phase 1 results
- Report any patterns or issues discovered
- Request confirmation and any adjustments before Phase 2

---

## 🟡 PHASE 2: MODERATE TIER (Components 21-40)

### Component 21: MoodEntry
**Complexity**: 4/10 | **Testability**: 7/10  
**Location**: `MauiApp/Models/MoodEntry.cs`

**Pre-Execution Checklist**:
- [ ] Verify DateTime dependencies (static calls to DateTime.Now/Today)
- [ ] Confirm need for IDateShim abstraction
- [ ] Update individual test plan with time control requirements
- [ ] Check for existing time abstraction patterns in codebase

**⚠️ COMPLEXITY ALERT**: This component has static DateTime dependencies that will require mocking strategies.

**Test Focus**: Business logic with controlled time, validation rules, state management

**Expected Duration**: 1-1.5 hours

**🔄 VERIFICATION CHECKPOINT**: Request confirmation and review mocking strategy before proceeding

---

### Component 22: LoggingService
**Complexity**: 5/10 | **Testability**: 6/10  
**Location**: `MauiApp/Services/LoggingService.cs`

**Pre-Execution Checklist**:
- [ ] Verify file I/O operations and platform dependencies
- [ ] Confirm need for file system abstractions
- [ ] Update test plan with async testing requirements
- [ ] Check for existing file system mocking patterns

**⚠️ COMPLEXITY ALERT**: File I/O operations require careful abstraction for testing.

**Test Focus**: Async file operations, platform path handling, error scenarios

**Expected Duration**: 1.5-2 hours

**🔄 VERIFICATION CHECKPOINT**: Request confirmation of file system mocking approach

---

### Components 23-30: Service and Model Group
*[Continue with similar detailed breakdowns for each component...]*

### Master Plan Review Checkpoint (After Component 30)
- [ ] Re-read this master plan entirely
- [ ] Assess progress against original timeline
- [ ] Review any patterns in complexity handling
- [ ] Update execution strategies based on learnings

**🔄 PHASE 2 MID-POINT VERIFICATION**: 
- Comprehensive progress review
- Strategy adjustment if needed
- Confirm approach for remaining moderate complexity items

---

### Components 31-40: Graphics and Data Processing Group
*[Detailed breakdowns continue...]*

**🔄 PHASE 2 COMPLETION CHECKPOINT**: 
- Complete phase summary
- Review mocking strategies developed
- Assessment of testing infrastructure needs
- Preparation strategy for hard tier complexity

---

## 🔴 PHASE 3: HARD TIER (Components 41-58)

### ⚠️ PRE-PHASE 3 REQUIREMENTS
Before beginning Phase 3, ensure the following infrastructure is in place:
- [ ] Comprehensive mocking framework established
- [ ] Service abstraction patterns documented
- [ ] Complex dependency injection testing patterns proven
- [ ] Platform abstraction strategies validated

### Component 41: AboutViewModel
**Complexity**: 6/10 | **Testability**: 5/10  
**Location**: `MauiApp/ViewModels/AboutViewModel.cs`

**Pre-Execution Checklist**:
- [ ] Verify service dependencies and injection patterns
- [ ] Confirm version info integration complexity
- [ ] Update individual test plan with service mocking requirements
- [ ] Establish ViewModel testing patterns

**⚠️ COMPLEXITY ALERT**: Multiple service dependencies require careful orchestration.

**🔄 VERIFICATION CHECKPOINT**: Detailed strategy review before proceeding

---

### Components 42-50: Complex ViewModels and Service Integration
*[Continue with increasingly detailed complexity handling...]*

### Component 57: MainPageViewModel  
**Complexity**: 8/10 | **Testability**: 2/10  
**Location**: `MauiApp/ViewModels/MainPageViewModel.cs`

**⚠️ CRITICAL COMPLEXITY**: This component may require refactoring before effective testing.

**Pre-Execution Strategy**:
1. **Analysis Phase**: Thoroughly analyze concrete dependencies
2. **Refactoring Proposal**: Propose interface abstractions needed
3. **Human Consultation**: **MANDATORY** - Present analysis and get approval before proceeding
4. **Incremental Approach**: Test smaller pieces first, build up to full integration

**🔄 MANDATORY VERIFICATION**: This component requires extensive consultation

---

### Component 58: MauiProgram
**Complexity**: 10/10 | **Testability**: 1/10  
**Location**: `MauiApp/MauiProgram.cs`

**⚠️ MAXIMUM COMPLEXITY**: Application bootstrap testing requires framework-level mocking.

**Pre-Execution Strategy**:
1. **Framework Analysis**: Deep dive into MAUI testing patterns
2. **Integration Test Strategy**: Focus on integration rather than unit testing
3. **Consultation Required**: **MANDATORY** - Extensive discussion of approach
4. **Incremental Validation**: Test DI configuration separately from framework bootstrap

**🔄 MANDATORY VERIFICATION**: Requires comprehensive strategy approval

---

## Execution Status Tracking

### Phase Completion Status
- [ ] Phase 1 (Easy): Components 1-20
- [ ] Phase 2 (Moderate): Components 21-40  
- [ ] Phase 3 (Hard): Components 41-58

### Current Progress
- **Last Completed Component**: Component 9 (CommandManager) - ✅ COMPLETE (18 tests, 100% pass rate, maintained 100% coverage with static event coordination)
- **Current Component**: Component 10 (MorningReminderData) - Ready for execution  
- **Next Verification Point**: After Component 9 (MANDATORY human confirmation required)
- **Plan Corrections Made**: 7 (AutoSaveEventArgs location corrected, DisplayAlertEventArgs location corrected, MorningReminderEventArgs location corrected, DateRangeItem location corrected, GraphModeItem location corrected, CommandManager location corrected, testing framework specifications added to all plans)

### Key Metrics to Track
- **Tests Written**: 238 (AutoSaveEventArgs: 13, AxisRange: 33, DisplayAlertEventArgs: 20, MorningReminderEventArgs: 32, DateRangeItem: 38, GraphModeItem: 28, RelayCommand: 26, RelayCommand&lt;T&gt;: 30, CommandManager: 18)
- **Components Completed**: 9/58 (Component 1: AutoSaveEventArgs ✅, Component 2: AxisRange ✅, Component 3: DisplayAlertEventArgs ✅, Component 4: MorningReminderEventArgs ✅, Component 5: DateRangeItem ✅, Component 6: GraphModeItem ✅, Component 7: RelayCommand ✅, Component 8: RelayCommand&lt;T&gt; ✅, Component 9: CommandManager ✅)
- **Coverage Improvements**: Track line coverage changes via `CoverageReport/Summary.txt` commits
- **Infrastructure Pieces Created**: 1 (PowerShell automation script)
- **Refactoring Recommendations Made**: 0

### Completed Components Summary

#### ✅ Component 1: AutoSaveEventArgs (13 tests)
**Pattern**: Pure EventArgs testing | **Duration**: 45 min | **Coverage**: 100%
**Key Achievement**: Established 3-checkpoint testing methodology and xUnit framework patterns

#### ✅ Component 2: AxisRange (33 tests)  
**Pattern**: Immutable record with parameterized tests | **Duration**: 30 min | **Coverage**: 100%
**Key Achievement**: Mastered `[Theory]` testing for method overloads and factory patterns

#### ✅ Component 3: DisplayAlertEventArgs (20 tests)
**Pattern**: Event args with string validation | **Duration**: 30 min | **Coverage**: 100%
**Key Achievement**: Confirmed nested class location pattern in ViewModels

#### ✅ Component 4: MorningReminderEventArgs (32 tests)
**Pattern**: Multi-property event args | **Duration**: 45 min | **Coverage**: 100%
**Key Achievement**: Comprehensive edge case testing with multiple data types

#### ✅ Component 5: DateRangeItem (38 tests)
**Pattern**: UI binding wrapper with dependency injection | **Duration**: 90 min | **Coverage**: 100%
**Key Achievement**: Established Moq framework patterns for IDateShim mocking

#### ✅ Component 6: GraphModeItem (28 tests)
**Pattern**: Simple enum wrapper class | **Duration**: 30 min | **Coverage**: 100%
**Key Achievement**: Efficient testing for UI binding enum wrappers

#### ✅ Component 7: RelayCommand (26 tests)
**Pattern**: ICommand infrastructure with delegates | **Duration**: 45 min | **Coverage**: 100% + CommandManager
**Key Achievement**: Complete MVVM command testing with event coordination patterns

#### ✅ Component 8: RelayCommand&lt;T&gt; (30 tests)
**Pattern**: Generic ICommand with type safety | **Duration**: 90 min | **Coverage**: 100%
**Key Achievement**: Namespace conflict resolution, mixed delegate signatures, nullable type behavior discovery

#### ✅ Component 9: CommandManager (18 tests)
**Pattern**: Static event coordination class | **Duration**: 60 min | **Coverage**: 100% (maintained)
**Key Achievement**: Static class testing methodology, event lifecycle management, RelayCommand integration patterns

---

## Emergency Protocols

### When to Escalate Immediately
1. **Component doesn't exist** at specified location
2. **Major architectural differences** from plan assumptions
3. **Testing infrastructure failures** that block progress
4. **Time estimates consistently wrong** by >100%

### Pause and Reassess Triggers
1. **Three consecutive components** require significant plan updates
2. **Testing patterns** aren't working across multiple components
3. **Infrastructure needs** are significantly different than anticipated

### Success Criteria for Each Phase
- **Phase 1**: Establish testing patterns, validate approach, build confidence
- **Phase 2**: Develop mocking strategies, handle moderate complexity effectively  
- **Phase 3**: Successfully test complex components, provide refactoring guidance

---

## Final Notes

This plan is designed to be **living documentation**. Each AI instance executing this plan should:

1. **Update this master plan** before starting each component - re-read entirely and update any outdated information
2. **Update progress** after each component in the Execution Status Tracking section
3. **Revise estimates** based on actual experience in component descriptions
4. **Document patterns** discovered for future components in relevant sections
5. **Escalate early** when encountering unexpected complexity
6. **Maintain human collaboration** especially for architectural decisions

The goal is not just to write tests, but to establish sustainable testing practices for the WorkMood codebase.