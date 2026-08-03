<!-- (dl (section-meta (title Mood Visualization))) -->

The **Visualization** page displays your 2-week mood data as a color-coded chart, making it easy to identify day-to-day mood changes and overall trends.

<!-- (dl (# Overview)) -->

The visualization page shows a **2-week mood change analysis** that compares each day's mood to the previous day, helping you spot patterns in your emotional well-being over time.

<!-- (dl (## Chart Structure)) -->

The main visualization displays:

- **Date Range Display**: Shows the specific 2-week period being analyzed
- **Color-Coded Chart**: Visual bars representing daily mood changes
- **Day Labels**: Individual dates for each displayed slot in the 2-week chart

<!-- (dl (## Reading the Visualization)) -->

The chart uses a color-coding system to show how your mood changed from day to day:

- **Significantly Improved** (Lime Green): Major positive mood changes
- **Improved** (Light Green): Moderate positive mood changes  
- **No Change** (Light Blue): Stable mood from previous day
- **Declined** (Light Coral): Moderate negative mood changes
- **Significantly Declined** (Red): Major negative mood changes
- **No Data** (Light Gray): Days where mood wasn't recorded

**Weekend compression on the 2-week visualization**: Unrecorded Saturday and Sunday entries do not consume horizontal chart space on this page.

- Friday to Monday with no weekend records: the chart connects those displayed days without reserving blank weekend spacing.
- Weekend day with records: that Saturday or Sunday stays visible in the chart and keeps its own slot.
- Missing weekday: behavior is unchanged; missing weekdays still keep their place in the 14-day calendar window and do not collapse out of the display.

**Example**:

1. Record mood on Friday.
2. Do not record mood on Saturday or Sunday.
3. Record mood on Monday.
4. Open the Visualization page.

Expected result:

- Friday and Monday appear next to each other in the chart.
- Saturday and Sunday do not consume chart width when they are unrecorded.
- The Daily Details list still shows the full calendar-based 2-week range.

<!-- (dl (## Interactive Features)) -->

The visualization page provides simple interaction options:

<!-- (dl (### Available Actions)) -->

- **Refresh Data**: Updates the visualization with the latest mood entries
- **Back to History**: Returns to the History page for detailed mood records
- **Loading States**: Shows activity indicator while data loads
- **Hide Graph Controls**: Collapses graph configuration controls to free more space for the graph image
- **Show Graph Controls**: Restores graph configuration controls when you want to change options

<!-- (dl (### Background Color Suggestions)) -->

When you open the background color picker in the Graph controls, WorkMood shows quick suggestions based on the current line color:

- **High Contrast** inverts the line color and flips brightness so dark lines get a light opposite-tone background and light lines get a dark opposite-tone background.
- **Complementary** uses the opposite hue of the current line color.
- **Triadic 1** uses the first triadic companion of the current line color.
- **Triadic 2** uses the second triadic companion of the current line color.

**Example**:

1. Open the Graph page.
2. Choose a line color in the graph controls.
3. Open the background color picker.
4. Select **High Contrast**.
5. Review the updated background preview before loading the graph again.

<!-- (dl (### Graph Modes)) -->

The graph view supports multiple modes so you can focus on different insights:

- **Impact (Change Over Day)**: Plots daily work impact using `(EndOfWork ?? StartOfWork) - StartOfWork`
- **General Impact (Outside Work)**: Compares today's start mood to the previous recorded work-period mood
- **Average (Daily Mood Level)**: Uses your adjusted daily average mood value
- **Opening Mood (Start of Day)**: Plots one point per day using `StartOfWork`
- **Closing Mood (End of Day)**: Plots one point per day using `EndOfWork`, falling back to `StartOfWork` if end-of-work is missing
- **Raw Data (Individual Recordings)**: Shows start and end recordings as separate timestamped points

**Missing Days**: Weekend days (Saturday and Sunday) are treated specially in Graph mode. If a weekend day has no recorded mood data, that day is compressed out of the horizontal axis instead of reserving blank timeline space.

- Friday to Monday with no weekend records: the line stays continuous and weekend spacing is removed.
- Weekend day with records: that weekend day keeps its horizontal spacing.
- Missing weekday (for example, Tuesday): behavior is unchanged; the line still breaks at that weekday gap.

This applies to every graph mode, including Raw Data. In Raw Data mode, start/end points within the same day stay ordered within that day while unrecorded weekend days are compressed.

<!-- (dl (### Gap Display Mode)) -->

Graph rendering includes a dropdown named **Gap Display Mode** in the graph controls.

- When **Gap Display Mode** is set to **Show Gaps**, missing weekdays remain gaps in the data line.
- When **Gap Display Mode** is set to **Gaps as Min**, missing weekdays are rendered as minimum-value points for the active graph mode.
- When **Gap Display Mode** is set to **Gaps as Max**, missing weekdays are rendered as max-value points for the active graph mode.
- When **Gap Display Mode** is set to **Gaps as Average**, missing weekdays are rendered using the arithmetic mean of all currently visible recorded points for the selected graph mode and date range.
- When **Gap Display Mode** is set to **Gaps as Surrounding Average**, each missing weekday is rendered using the arithmetic mean of the nearest previous and nearest next visible recorded points.
- When **Gap Display Mode** is set to **Match the Previous Value**, each missing weekday is rendered using the nearest previous visible recorded value.
- When **Gap Display Mode** is set to **Match the Following Value**, each missing weekday is rendered using the nearest next visible recorded value.

Min-value mapping by graph mode:

- **Impact** and **General Impact** use `-9`.
- **Average** uses `-5`.
- **Opening Mood**, **Closing Mood**, and **Raw Data** use `1`.

Max-value mapping by graph mode:

- **Impact** and **General Impact** use `9`.
- **Average** uses `5`.
- **Opening Mood**, **Closing Mood**, and **Raw Data** use `10`.

When gap-fill is on (**Gaps as Min**, **Gaps as Max**, **Gaps as Average**, **Gaps as Surrounding Average**, **Match the Previous Value**, or **Match the Following Value**), you can also choose a **Gap Fill Color** for line segments and synthetic point markers generated for gap-fill days:

- **Complementary** uses the opposite hue of the primary line color.
- **First Triadic** uses the first triadic companion of the primary line color.
- **Second Triadic** uses the second triadic companion of the primary line color.
- **Match Line Color** keeps gap-fill segments and synthetic gap markers on the same primary line color.

When **Gap Display Mode** is **Show Gaps**, the gap fill color setting is not applied and point markers continue using the primary marker color.

Weekend compression behavior does not change for either mode.

For **Gaps as Average**:

- The mean is calculated from visible recorded points only (not synthetic gap-fill points).
- In **Raw Data**, both recorded points per day (start and end) are included in the mean.
- If no recorded points are visible in the selected date range, no synthetic average-fill points are added.

For **Gaps as Surrounding Average**:

- Each missing weekday uses local context instead of a global mean.
- The value is computed as: (nearest previous visible point + nearest next visible point) / 2.
- If either surrounding side is missing, that day is left as a gap.
- In **Raw Data**, nearest surrounding points are taken from the visible timestamp-ordered points.

For **Match the Previous Value**:

- Each missing weekday is filled from the nearest visible recorded point before that day.
- Consecutive missing weekdays between two recorded days all use the same last-known value.
- If no previous visible value exists, that day is left as a gap.
- A previous point just outside the selected date range can still be used as the source value.

For **Match the Following Value**:

- Each missing weekday is filled from the nearest visible recorded point after that day.
- Consecutive missing weekdays between two recorded days all use the same next-known value.
- If no following visible value exists, that day is left as a gap.
- A following point just outside the selected date range can still be used as the source value.

Example task:

1. On the main dashboard, select **Generate Graph**.
2. In the graph controls, set **Gap Display Mode** to **Gaps as Min**, **Gaps as Max**, **Gaps as Average**, **Gaps as Surrounding Average**, **Match the Previous Value**, or **Match the Following Value**.
3. Optionally choose a **Gap Fill Color**.
4. Record mood on Monday and Wednesday only.
5. Review the graph.

Expected result:

- Tuesday is rendered as a synthetic gap-fill point at the mode minimum for **Gaps as Min**, or at the mode maximum for **Gaps as Max**.
- For **Gaps as Average**, Tuesday is rendered as a synthetic gap-fill point at the arithmetic mean of visible recorded points in the selected graph mode and date range.
- For **Gaps as Surrounding Average**, Tuesday is rendered as a synthetic gap-fill point at the average of the nearest visible point before Tuesday and the nearest visible point after Tuesday.
- For **Match the Previous Value**, Tuesday is rendered as a synthetic gap-fill point using the nearest visible recorded value before Tuesday.
- For **Match the Following Value**, Tuesday is rendered as a synthetic gap-fill point using the nearest visible recorded value after Tuesday.
- The generated Tuesday point marker and adjacent line segments use the selected gap fill color. With **Match Line Color**, they stay on the primary line color instead of switching to a contrasting accent.
- If Saturday and Sunday are unrecorded, they are still compressed out of spacing.

<!-- (dl (### Graph Controls Visibility)) -->

Use the **Hide Graph Controls** button above the graph controls when you want a larger graph preview.

- Hiding controls collapses date range, graph mode, display options, line color tools, and background controls.
- The graph section expands into the newly available space.
- Use **Show Graph Controls** at any time to bring the controls back.

**Example**:

1. Open the Graph page.
2. Choose a date range and graph mode.
3. Select **Hide Graph Controls**.
4. Review the larger graph preview.
5. Select **Show Graph Controls** when you need to adjust settings.

**Example**:

1. Record mood on Friday.
2. Do not record mood on Saturday or Sunday.
3. Record mood on Monday.
4. Open the graph.

You will see a continuous Friday-to-Monday line segment, and the unrecorded weekend will not consume axis width.

**Note**: The visualization is a static display - it does not support zooming, panning, or other chart interactions.

<!-- (dl (# Understanding Your Data)) -->

<!-- (dl (## Daily Change Analysis)) -->

The visualization helps you understand mood patterns by showing day-to-day changes rather than absolute values:

<!-- (dl (### Change Indicators)) -->

- **Positive Changes**: Green colors indicate mood improvements from the previous day
- **Negative Changes**: Red/coral colors show mood declines from the previous day  
- **Stability**: Blue indicates consistent mood levels
- **Missing Data**: Gray bars appear when mood data is not available

<!-- (dl (### Pattern Recognition)) -->

Look for these patterns in your visualization:

- **Consecutive Improvements**: Multiple green bars in sequence
- **Declining Streaks**: Series of red/coral indicators
- **Recovery Patterns**: Red bars followed by green (bouncing back)
- **Stable Periods**: Consistent blue bars showing mood stability

<!-- (dl (# Data Display Details)) -->

<!-- (dl (## Summary Information)) -->

Below the main chart, you'll find:

- **Date Range**: Shows the exact 2-week period displayed
- **Summary Text**: Provides key insights about your mood patterns
- **Daily Details List**: Comprehensive breakdown of each day's data

<!-- (dl (## Daily Details List)) -->

The detailed view shows for each day:

- **Date**: Day of the month
- **Color Indicator**: Visual representation of the day's mood change
- **Description**: Text explanation of the mood change
- **Value**: Numerical change value (when available)

The Daily Details list remains calendar-based even when the chart compresses an unrecorded weekend out of the horizontal display.

<!-- (dl (# Understanding the Legend)) -->

The color legend explains what each color represents:

- **Lime Green**: Significant mood improvement
- **Light Green**: Moderate mood improvement
- **Light Blue**: No significant change
- **Light Coral**: Moderate mood decline
- **Red**: Significant mood decline
- **Light Gray**: No mood data recorded

<!-- (dl (# Tips for Effective Use)) -->

<!-- (dl (## Regular Review)) -->

- **Weekly Check-ins**: Review your visualization weekly to spot emerging patterns
- **Focus on Trends**: Look for sequences of similar colors rather than individual days
- **Context Awareness**: Remember life events that might explain significant changes

<!-- (dl (## Data Quality)) -->

- **Consistent Recording**: Regular mood entries provide better visualizations
- **Honest Rating**: Accurate mood ratings create meaningful change analysis
- **Complete Records**: Try to record moods daily for the clearest patterns

<!-- (dl (## Using Insights)) -->

- **Identify Triggers**: Spot patterns that precede mood improvements or declines
- **Recognize Recovery**: Notice how quickly you bounce back from difficult days
- **Track Progress**: Monitor overall trends in your mood stability
- **Share with Healthcare Providers**: Use visualizations to discuss mood patterns with professionals
