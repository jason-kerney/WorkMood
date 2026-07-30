# Two-Week Visualization Weekend Collapse Implementation

## Status
Implemented in the 2-week Visualization page rendering path.

## Purpose
Document the engineering change that collapses weekend spacing in the 2-week visualization graph when weekend days are not recorded.

## Problem Statement
Current 2-week visualization uses a fixed 14-calendar-day slot layout, so Saturday/Sunday always consume horizontal space.

That behavior is defined across:
- `MauiApp/Services/MoodVisualizationService.cs` (produces fixed-size 14-day `DailyValues`)
- `MauiApp/Processors/VisualizationDataProcessor.cs` (fills one item per calendar day in range)
- `MauiApp/Graphics/EnhancedLineGraphDrawable.cs` (renders with fixed `pointSpacing = graphWidth / 13f`)
- `MauiApp/Pages/Visualization.xaml` (hardcoded 14 columns)
- `MauiApp/Pages/Visualization.xaml.cs` (loops from day `0..13` for labels and render wiring)

Result: unrecorded weekends appear as visible graph gaps and "No Data" placeholders.

## Target Behavior
Collapse weekend spacing only when weekend days are unrecorded.

Rules:
1. If Saturday/Sunday have no recorded data, those days should not consume horizontal plotting space in the visualization graph.
2. If either weekend day has recorded data, it should remain visible and consume space as a normal point/day.
3. Weekday ordering must remain chronological and stable.
4. Existing color semantics and value semantics remain unchanged.
5. The daily details list can remain calendar-based unless explicitly changed later.

## Recommended Implementation Strategy
Implement as a dedicated "display axis" layer for the 2-week visualization rather than mutating core mood data semantics.

### 1) Add Display-Day Projection Model
Introduce a view/render model that represents only display slots for the graph.

Suggested model concepts:
- `DisplayDate` (DateOnly)
- `SourceDate` (DateOnly)
- `HasData` (bool)
- `Value` (double?)
- `Color` (Color)
- `IsCollapsedWeekendPlaceholder` (bool, optional; likely false for collapsed-out days)

Alternative: add a derived collection in `MoodVisualizationData` such as `DisplayValues` while preserving existing `DailyValues` for compatibility.

### 2) Build Weekend-Collapse Projection
In service/processor layer, compute display sequence from the existing 14-day window:
- Include all weekdays.
- Include weekend day only if it has recorded data.
- Exclude weekend day if no recorded data.

Keep source range metadata (`StartDate`, `EndDate`) unchanged for summary text and auditability.

### 3) Update Renderer to Use Dynamic Slot Count
In `EnhancedLineGraphDrawable`:
- Replace hardcoded `14` assumptions with `displayCount` from projected data.
- Compute spacing as:
  - if `displayCount <= 1`: center single point
  - else `pointSpacing = graphWidth / (displayCount - 1)`
- Ensure line, points, grid, and missing-data indicators all use the same projected collection.

### 4) Update Visualization Grid/Labels
In `Visualization.xaml` and `Visualization.xaml.cs`:
- Remove hardcoded 14-column dependency for top day labels in programmatic rendering path.
- Generate columns dynamically to match projected display count.
- Keep week labels only if still meaningful; otherwise replace with date-span label.

### 5) Preserve Backward Compatibility Where Needed
- Avoid breaking consumers that expect `DailyValues.Length == 14` unless intentionally migrated.
- If required, keep `DailyValues` unchanged and add `DisplayValues` specifically for rendering.

## Edge Cases to Handle
1. All 14 days empty -> show empty-state behavior without crashes.
2. Only one recorded weekday in range -> single centered point.
3. Weekend-only data present -> weekend points displayed (not collapsed).
4. Friday data and Monday data with empty weekend -> no weekend spacing between displayed points.
5. Mixed missing weekdays (non-weekend) -> do not auto-collapse unless specifically requested.

## Test Plan

### Unit Tests
Add/update tests in:
- `WorkMood.MauiApp.Tests/Services/MoodVisualizationServiceShould.cs`
- `WorkMood.MauiApp.Tests/Processors/` (or create dedicated processor tests)
- `WorkMood.MauiApp.Tests/Graphics/` (if drawable unit tests exist; otherwise add)

Test cases:
1. Collapses unrecorded Saturday/Sunday.
2. Retains Saturday when recorded.
3. Retains Sunday when recorded.
4. Retains both weekend days when both recorded.
5. Preserves chronological order after projection.
6. Handles single-point and zero-point projections safely.

### UI/Integration Tests
- Verify day labels align with rendered points after collapse.
- Verify no out-of-range indexing from former fixed 14-day loops.

### Approval/Visual Regression (Recommended)
Create screenshot/approval tests for representative patterns:
- No weekend data
- Weekend partial data
- Weekend full data
- Sparse weekdays

## Risks and Mitigations
Risk: Breaking assumptions tied to `14` fixed slots.
Mitigation: Introduce projection model and migrate renderer incrementally behind tests.

Risk: Misalignment between labels and plotted points.
Mitigation: Single source of truth collection for both labels and plotting.

Risk: Behavior confusion between Graph page and Visualization page.
Mitigation: Explicitly document that this pass is for the 2-week visualization page only.

## Out of Scope
1. Changes to Graph page/export pipeline (`LineGraphService`/`LineGraphGenerator`).
2. Altering mood value calculation semantics.
3. Changing summary statistics definitions.
4. Collapsing non-weekend missing days.

## Suggested Commit
`^f - collapse unrecorded weekend spacing in 2-week visualization with dynamic display slots`

## Definition of Done
1. 2-week visualization graph no longer reserves space for unrecorded weekend days.
2. Recorded weekend days remain visible.
3. All existing and new relevant tests pass.
4. Documentation updated if public behavior description changes.
