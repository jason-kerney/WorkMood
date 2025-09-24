<!-- (dl (section-meta (title Viewing History))) -->

The **History** page provides access to all your past mood entries with analytics insights to help you understand your mood patterns over time.

<!-- (dl (# Page Layout)) -->

The History page is organized into three main sections:

- **Header**: Page title with analytics emoji 📊
- **Statistics Summary**: Key metrics and trend analysis
- **Recent Entries List**: Display of your most recent mood entries

<!-- (dl (# Statistics Summary)) -->

The statistics section provides an at-a-glance view of your mood tracking data in a rounded container.

<!-- (dl (## Available Statistics)) -->

The statistics panel displays:

- **Total Entries**: Complete count of all recorded mood entries
- **Overall Average**: Mean mood rating across all entries (displayed to 1 decimal place)
- **Last 7 Days**: Average mood for the past week
- **Last 30 Days**: Average mood for the past month
- **Trend**: Analysis showing "Improving", "Declining", "Stable", or "N/A"

<!-- (dl (## Trend Indicators)) -->

Trend text is color-coded for quick visual assessment:

- **Green**: "Improving" trend
- **Red**: "Declining" trend  
- **Orange**: "Stable" trend
- **Gray**: "N/A" when insufficient data

<!-- (dl (## Visualization Access)) -->

A prominent button labeled "📊 View 2-Week Visualization" provides access to the graphical mood visualization page, showing your mood trends over the past two weeks.

<!-- (dl (# Recent Entries List)) -->

The main content area displays your **10 most recent mood entries** in reverse chronological order (newest first).

<!-- (dl (## Entry Display)) -->

Each entry is displayed in a rounded container showing:

**Date Information** (left column):

- **Month**: Three-letter abbreviation (e.g., "Sep")
- **Day**: Two-digit day number (e.g., "24")
- **Weekday**: Three-letter abbreviation (e.g., "Tue")

**Mood Values** (center column):

- **Morning Mood**: Displayed with 🌅 emoji and numerical value (or "—" if not recorded)
- **Evening Mood**: Displayed with 🌙 emoji and numerical value (or "—" if not recorded)
- **Last Updated**: Shows when the entry was last modified (format: "MMM dd, HH:mm")

**Average** (right-center column):

- **"Avg" Label**: Indicates average calculation
- **Average Value**: Calculated average of morning and evening moods, or single mood if only one recorded

**Mood Emoji** (right column):

- **Visual Indicator**: Large emoji representing the overall mood level
- **Emoji Scale**: 😭 (1-2), 😢 (2-3), 😟 (3-4), ☹️ (4-5), 😕 (5-6), 😐 (6-7), 🙂 (7-8), 😊 (8-9), 😄 (9-10), ❓ (no data)

<!-- (dl (## Data States)) -->

The page handles different data states appropriately:

- **Loading State**: Shows activity indicator while data loads
- **No Data State**: Displays "No mood entries found." message when no entries exist
- **Error State**: Shows error message in red text if data loading fails

<!-- (dl (# Navigation Features)) -->

<!-- (dl (## Visualization Integration)) -->

The "View 2-Week Visualization" button navigates to a dedicated visualization page showing:

- Graphical representation of mood trends
- Two-week historical view
- Visual pattern analysis

<!-- (dl (## Automatic Data Refresh)) -->

The page automatically refreshes data when:

- The page appears (OnAppearing event)
- Returning from other pages in the app
- The app resumes from background

<!-- (dl (# Data Insights)) -->

<!-- (dl (## Understanding Your Trends)) -->

Use the statistics to identify patterns:

- **Consistent averages** across time periods suggest stable mood patterns
- **Improving trends** indicate positive changes in your overall well-being
- **Declining trends** may suggest areas needing attention
- **Large differences** between 7-day and 30-day averages show recent changes

<!-- (dl (## Making the Most of History)) -->

**Regular Review Tips**:

- Check your weekly averages to spot recent changes
- Use the visualization page for deeper trend analysis
- Note correlations between mood patterns and life events
- Track progress toward personal wellness goals

**Data Quality Tips**:

- Consistent daily recording provides more accurate trends
- Both morning and evening entries give fuller daily pictures
- Regular app usage ensures up-to-date statistics
