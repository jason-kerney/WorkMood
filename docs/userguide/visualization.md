<!-- (dl (section-meta (title Mood Visualization))) -->

The **Visualization** page displays your 2-week mood data as a color-coded chart, making it easy to identify day-to-day mood changes and overall trends.

<!-- (dl (# Overview)) -->

The visualization page shows a **2-week mood change analysis** that compares each day's mood to the previous day, helping you spot patterns in your emotional well-being over time.

<!-- (dl (## Chart Structure)) -->

The main visualization displays:

- **Date Range Display**: Shows the specific 2-week period being analyzed
- **Color-Coded Chart**: Visual bars representing daily mood changes
- **Day Labels**: Individual dates across the 2-week period
- **Week Divisions**: Clear separation between Week 1 and Week 2

<!-- (dl (## Reading the Visualization)) -->

The chart uses a color-coding system to show how your mood changed from day to day:

- **Significantly Improved** (Lime Green): Major positive mood changes
- **Improved** (Light Green): Moderate positive mood changes  
- **No Change** (Light Blue): Stable mood from previous day
- **Declined** (Light Coral): Moderate negative mood changes
- **Significantly Declined** (Red): Major negative mood changes
- **No Data** (Light Gray): Days where mood wasn't recorded

<!-- (dl (## Interactive Features)) -->

The visualization page provides simple interaction options:

<!-- (dl (### Available Actions)) -->

- **Refresh Data**: Updates the visualization with the latest mood entries
- **Back to History**: Returns to the History page for detailed mood records
- **Loading States**: Shows activity indicator while data loads

<!-- (dl (### Graph Modes)) -->

The graph view supports multiple modes so you can focus on different insights:

- **Impact (Change Over Day)**: Plots daily work impact using `(EndOfWork ?? StartOfWork) - StartOfWork`
- **General Impact (Outside Work)**: Compares today's start mood to the previous recorded work-period mood
- **Average (Daily Mood Level)**: Uses your adjusted daily average mood value
- **Opening Mood (Start of Day)**: Plots one point per day using `StartOfWork`
- **Closing Mood (End of Day)**: Plots one point per day using `EndOfWork`, falling back to `StartOfWork` if end-of-work is missing
- **Raw Data (Individual Recordings)**: Shows start and end recordings as separate timestamped points

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
