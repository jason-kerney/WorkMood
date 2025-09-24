<!-- GENERATED DOCUMENT DO NOT EDIT! -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- Compiled with doculisp https://www.npmjs.com/package/doculisp -->
<!-- Written By: WorkMood Team -->

# WorkMood User Guide #

### Complete guide to using WorkMood - the workday mood impact tracker ###

## Table of Contents ##

1. [Getting Started](#getting-started)
2. [Main Dashboard](#main-dashboard)
3. [Recording Your Mood](#recording-your-mood)
4. [Viewing History](#viewing-history)
5. [Mood Visualization](#mood-visualization)
6. [Application Settings](#application-settings)
7. [About WorkMood](#about-workmood)

## Getting Started ##

WorkMood is a cross-platform desktop application designed to help you **track and analyze how your workday impacts your mood**. Unlike general mood tracking apps, WorkMood focuses specifically on understanding the relationship between your work environment, activities, and emotional well-being.

### What is WorkMood? ###

WorkMood is a **work mood impact tracker** that allows you to:

- **Monitor Work Impact**: Track how your workday affects your emotional state
- **Record Morning & Evening Moods**: Capture how you feel at work start and work end
- **Visualize Work Patterns**: See how different work situations impact your mood over time
- **Schedule-Based Reminders**: Get automatic reminders based on your work schedule
- **Analyze Workday Changes**: Understand how your mood changes during work hours
- **Export Your Data**: Generate reports on work-related mood patterns for personal analysis

### System Requirements ###

- **Windows 10** version 1903 or later, or **Windows 11**
- **macOS Big Sur** (11.0) or later
- At least **100 MB** of free disk space
- **.NET 9.0 Runtime** (included in self-contained deployments)

### First Launch ###

When you first launch WorkMood, you'll see the main dashboard with a clean, intuitive interface designed for daily use.

![WorkMood Main Dashboard](./images/main-dashboard.png)
*Figure 1: WorkMood main dashboard on first launch*

### Navigation ###

WorkMood uses a **tabbed interface** with the following main sections:

- **Main** - Dashboard and quick work mood entry
- **Record Mood** - Detailed work mood recording interface
- **History** - View and manage past work mood entries
- **Visualization** - Charts and analytics for work mood patterns
- **Settings** - Configure work schedule and reminder preferences
- **About** - Application information and version details

### Quick Start Guide ###

#### Step 1: Record Your First Work Mood ####

1. Click on the **"Record Mood"** tab
2. Record your **morning work mood** using the slider or buttons (how you feel starting work)
3. At the end of your work day, record your **evening work mood** (how you feel finishing work)
4. Click **"Save Entry"** to record your work mood data

![Mood Recording Interface](./images/mood-recording-interface.png)
*Figure 2: Recording your first work mood entry*

#### Step 2: View Your Work Impact Data ####

1. Navigate to the **"History"** tab to see all your past work mood entries
2. Use the **"Visualization"** tab to see work impact trends and patterns
3. Explore different chart types to understand how work affects your mood over time

#### Step 3: Set Up Work Schedule Reminders (Optional) ####

1. Go to the **"Settings"** tab
2. Configure your **work start time** for morning mood reminders
3. Configure your **work end time** for evening mood reminders
4. Set your preferred notification preferences
5. Enable auto-save if desired

### Understanding Work Mood Levels ###

WorkMood uses a **standardized work mood scale** from 1-10:

- **1-2**: Work Very Negatively Impacting Mood (overwhelmed, stressed)
- **3-4**: Work Negatively Impacting Mood (frustrated, down about work)
- **5-6**: Neutral Work Impact (okay, manageable work mood)
- **7-8**: Work Positively Impacting Mood (good about work, satisfied)
- **9-10**: Work Very Positively Impacting Mood (energized, motivated)

This consistent scale helps you track meaningful changes in your emotional state during work hours.

## Main Dashboard ##

The **Main Dashboard** is the central navigation hub of WorkMood, providing quick access to all major features of the application. The dashboard serves as your starting point for mood tracking activities.

### Dashboard Layout ###

The main dashboard is organized into three main sections:

#### Header Section ####

The header displays essential information:

- **Application Title**: "WorkMood" with the subtitle "Daily Mood Tracker"
- **Current Date**: Shows today's date in full format (e.g., "Wednesday, September 24, 2025")
- The date automatically updates when the app detects a new day

#### Welcome Section ####

A prominent welcome area features:

- **Greeting**: "👋 Welcome!" message in a colored border
- **Tagline**: "Track your daily mood and build better habits"
- This section uses the app's secondary color scheme for visual emphasis

#### Navigation Buttons ####

Four main action buttons provide access to core features:

- **📝 Record Today's Mood**: Navigate to the mood recording interface
- **📊 View History**: Access your mood history and visualizations
- **⚙️ Settings**: Configure app preferences and reminder schedules
- **ℹ️ About**: View app information and details

Each button is styled with:
- Large, easily tappable design
- Emoji icons for visual identification
- Color-coded backgrounds (primary, secondary, or custom colors)
- Bold text for clear readability

### Footer Section ###

The bottom of the dashboard displays status information:

- **Auto-save Status**: "Auto-save enabled • Data synced locally"
- This indicates that your mood data is automatically saved and stored locally on your device

### Dashboard Functionality ###

#### Navigation System ####

The dashboard serves as the primary navigation hub:

- **Command Binding**: Each button is connected to a specific command in the view model
- **Error Handling**: Failed navigation attempts display informative error messages
- **Service Integration**: Navigation passes required services to destination pages

#### Date Management ####

The dashboard automatically handles date-related functionality:

- **Automatic Updates**: The current date display refreshes when the page appears
- **Date Change Detection**: The app monitors for day changes and updates accordingly
- **Event-Driven Updates**: Date changes trigger internal processes for mood data management

#### Background Services ####

While using the dashboard, several services operate automatically:

- **Dispatcher Service**: Manages scheduled reminders and date change detection
- **Auto-save Service**: Handles automatic saving of mood data
- **Window Activation**: Brings the app to foreground when reminders trigger
- **Logging Service**: Records app events for troubleshooting

### User Notifications ###

The dashboard can display various types of alerts and notifications:

#### Auto-save Notifications ####

When automatic saving occurs:
- **Date Change Auto-save**: Notifies when previous day's data is automatically saved
- **Alert Display**: Shows confirmation message that auto-save completed successfully

#### Reminder Alerts ####

The app displays reminder notifications directly on the dashboard:

- **Morning Reminders**: Customizable morning mood tracking reminders
- **Evening Reminders**: Evening reminders with context-aware messages
- **Missed Reminders**: Special alerts for missed morning mood entries
- **Window Activation**: App automatically comes to foreground when reminders trigger

### Usage Tips ###

#### Efficient Navigation ####

1. **Use as Home Base**: Return to the dashboard between different app sections
2. **Quick Access**: Tap any navigation button to jump directly to that feature
3. **Stay Updated**: Check the current date display to confirm you're tracking the right day
4. **Monitor Status**: Review the footer status to ensure auto-save is working

#### Best Practices ####

- **Regular Check-ins**: Visit the dashboard when starting your mood tracking session
- **Respond to Reminders**: Act on reminder notifications when they appear
- **Use Navigation Shortcuts**: Leverage the quick navigation buttons instead of complex menu systems
- **Trust Auto-save**: The app automatically handles data persistence, so focus on tracking your mood

## Recording Your Mood ##

The **Mood Recording** page is the core feature of WorkMood, designed specifically to track **how your workday impacts your mood**. Unlike general mood trackers, WorkMood focuses on measuring the change in your emotional state from the start to the end of your work day.

![Mood Recording Interface](./images/mood-recording-main.png)
*Figure 1: Complete mood recording interface*

### Page Overview ###

The mood recording page displays as **"Daily Mood Tracker"** with the current date prominently shown and is designed for **workday impact tracking**:

- **Morning Mood**: Record how you feel when your **work day starts**
- **Evening Mood**: Log your mood near the **end of your work day**
- **Today's Summary**: Automatic analysis of your mood change throughout the day
- **Manual Save System**: Explicit save buttons for both morning and evening moods
- **Sequential Workflow**: Evening mood selection is enabled only after morning mood is saved
- **Schedule-Based Reminders**: Automatic reminders based on your work schedule configuration
- **Work Impact Analysis**: Track how your work day affects your emotional well-being
- **Historical Editing**: Modify past entries if needed

### Morning Mood Section ###

The morning section features a **warm sunrise theme** and focuses on **how you feel when starting your workday**.

![Morning Mood Section](./images/mood-recording-morning.png)
*Figure 2: Morning mood recording section with sunrise theme*

#### Morning Mood Features ####

- **Visual Indicator**: 🌅 Sunrise emoji and warm color scheme
- **Scale Guidance**: Clear 1-10 scale explanation (1 = Very Bad, 10 = Excellent)
- **Button Interface**: Ten numbered buttons for quick mood selection
- **Current Selection**: Large display shows your chosen mood level
- **Locked State**: Shows saved mood with option to edit
- **Work Schedule Integration**: Reminders based on your configured work start time

#### Morning Mood Recording Process ####

1. **Start of Workday**: Navigate to the mood recording page at the beginning of your work day
2. **Select Your Mood**: Click any number from 1 to 10 representing your current state
3. **Review Selection**: Your choice appears prominently in the mood label
4. **Save Mood**: Click the "Save Morning Mood" button to confirm and save your selection
5. **Edit Later**: Click "Edit" to modify previously saved moods

**Important**: Evening mood selection remains disabled until morning mood is saved, enforcing the sequential workflow.

### Evening Mood Section ###

The evening section uses a **calming sunset theme** with a **🌙 moon emoji** for **end-of-workday reflection**.

![Evening Mood Section](./images/mood-recording-evening.png)
*Figure 3: Evening mood recording section with sunset theme*

#### Evening Mood Features ####

- **Visual Indicator**: 🌙 Moon emoji and calming color scheme
- **Consistent Scale**: Same 1-10 rating system as morning
- **Dependency on Morning**: Evening mood buttons are disabled until morning mood is saved
- **Work Impact Focus**: Designed for assessing how your workday affected your mood
- **Manual Save Required**: Click "Save Evening Mood" button to confirm your selection
- **Schedule Integration**: Reminders based on your configured work end time

#### Evening Recording Process ####

1. **Complete Morning First**: Ensure your morning mood has been saved
2. **End-of-Workday Check**: Record your mood as you finish your work day
3. **Reflect on Work Impact**: Consider how your work experiences affected your mood
4. **Select Rating**: Choose number that best represents your current emotional state (buttons are now enabled)
5. **Save Mood**: Click "Save Evening Mood" button to confirm and save your selection
6. **View Summary**: The app automatically generates a summary showing your mood change

### Mood Scale Guide ###

WorkMood uses a standardized **10-point scale** for consistent tracking of your emotional state relative to work:

![Mood Scale Reference](./images/mood-scale-guide.png)
*Figure 4: Visual mood scale reference guide*

#### Scale Breakdown ####

- **1-2 (Very Low)**: Feeling stressed, overwhelmed, or very negative about work
- **3-4 (Low)**: Down, frustrated, or generally unpleasant mood related to work
- **5-6 (Neutral)**: Okay, neither good nor bad, stable work-related mood
- **7-8 (Good)**: Happy, positive, generally feeling good about work
- **9-10 (Excellent)**: Energized, extremely positive, or exceptionally good work mood

### Today's Summary Section ###

When both morning and evening moods are recorded, WorkMood automatically displays a **"📊 Today's Summary"** section that provides insights into your workday's emotional impact.

![Today's Summary](./images/mood-summary-section.png)
*Figure 6: Automatic workday mood summary display*

#### Summary Features ####

- **Automatic Generation**: Appears only when both moods are recorded
- **Mood Change Analysis**: Shows how your mood shifted during your work day
- **Visual Indicator**: 📊 Chart emoji with clear summary text
- **Work Impact Assessment**: Specifically focuses on work-related mood changes
- **Daily Insights**: Provides context for understanding your workday's emotional journey

#### Summary Analysis Types ####

The summary provides different insights based on your mood change:

- **Positive Change**: When evening mood is higher than morning mood
- **Negative Change**: When evening mood is lower than morning mood
- **Stable Mood**: When morning and evening moods are similar
- **Magnitude Assessment**: Shows the degree of change between morning and evening

### Action Buttons Section ###

The mood recording page includes dedicated action buttons at the bottom for saving your mood entries and navigation.

![Action Buttons](./images/action-buttons-section.png)
*Figure 7: Action buttons for saving moods and navigation*

#### Save Buttons ####

- **Save Morning Mood**: Enabled only when a morning mood is selected and not yet saved
- **Save Evening Mood**: Enabled only when morning mood is saved and evening mood is selected
- **Dynamic Colors**: Buttons change color based on enabled/disabled state (active: purple, disabled: gray)
- **Independent Operation**: Each mood can be saved separately

#### Navigation Button ####

- **Back to Main**: Always available button to return to the main application screen
- **Consistent Styling**: Uses tertiary color scheme for clear visual distinction

### Advanced Features ###

#### Edit Saved Moods ####

Both morning and evening sections allow **editing of saved entries**:

1. **View Saved**: Locked sections show previously recorded moods
2. **Click Edit**: Button unlocks the mood selection interface
3. **Choose New Value**: Select a different mood rating
4. **Auto-Save**: Changes are saved immediately

![Edit Mode Interface](./images/mood-recording-edit-mode.png)
*Figure 5: Mood recording in edit mode*

#### Visual Feedback ####

The interface provides **clear visual feedback** throughout the mood recording process:

- **Selected State**: Chosen mood buttons appear highlighted with white background
- **Color Coding**: Different colors for morning (warm) and evening (cool) themes
- **Section State Colors**:
  - **Active Purple**: When section is ready for input or being edited
  - **Saved Gray**: When mood is saved and locked from editing
- **Button States**: Save buttons change color based on availability
- **Lock Icons**: 🔒 Indicates saved moods that can be edited
- **State Messages**: Clear text describing current status and mood labels
- **Enabled/Disabled States**: Evening mood buttons are visually disabled until morning is saved

#### Date Navigation ####

- **Current Date Display**: Shows the date you're recording for
- **Historical Access**: Can record moods for past dates (via History page)
- **Real-time Updates**: Interface updates automatically as you make selections

### Best Practices ###

#### Timing Recommendations ####

- **Morning**: Record within 30 minutes of starting your work day
- **Evening**: Log mood 30-60 minutes before ending your work day
- **Sequential Process**: Always save morning mood before attempting evening mood
- **Consistency**: Try to record at similar times relative to your work schedule
- **Honesty**: Rate how you actually feel about work, not how you think you should feel

#### Effective Work Mood Tracking ####

1. **Work Context Focus**: Assess how work activities and environment affect your mood
2. **Consider Work Factors**: Think about meetings, tasks, colleagues, and work stress
3. **Sequential Recording**: Complete morning mood and save before moving to evening
4. **Manual Save Confirmation**: Use save buttons to confirm your mood selections
5. **Be Specific**: Use the full 1-10 range based on work-related feelings
6. **Track Work Patterns**: Look for trends in how different work situations impact mood
7. **Review Daily Summary**: Pay attention to the automatic summary to understand your workday impact

#### Data Quality Tips ####

- **Work-Related Assessment**: Focus specifically on work-related mood, not personal life
- **Sequential Workflow**: Always save morning mood first, then proceed to evening
- **Don't Overthink**: Go with your first instinct about work feelings
- **Use Save Buttons**: Confirm your selections with the dedicated save buttons
- **Use the Full Scale**: Don't avoid extreme ratings when work situations warrant them
- **Be Consistent**: Use the same interpretation of the work mood scale each time
- **Review Summary**: Check the daily summary to understand your workday's emotional impact
- **Regular Recording**: Daily work mood tracking provides the most useful data

## Viewing History ##

The **History** page provides access to all your past mood entries with analytics insights to help you understand your mood patterns over time.

### Page Layout ###

The History page is organized into three main sections:

- **Header**: Page title with analytics emoji 📊
- **Statistics Summary**: Key metrics and trend analysis
- **Recent Entries List**: Display of your most recent mood entries

### Statistics Summary ###

The statistics section provides an at-a-glance view of your mood tracking data in a rounded container.

#### Available Statistics ####

The statistics panel displays:

- **Total Entries**: Complete count of all recorded mood entries
- **Overall Average**: Mean mood rating across all entries (displayed to 1 decimal place)
- **Last 7 Days**: Average mood for the past week
- **Last 30 Days**: Average mood for the past month
- **Trend**: Analysis showing "Improving", "Declining", "Stable", or "N/A"

#### Trend Indicators ####

Trend text is color-coded for quick visual assessment:

- **Green**: "Improving" trend
- **Red**: "Declining" trend
- **Orange**: "Stable" trend
- **Gray**: "N/A" when insufficient data

#### Visualization Access ####

A prominent button labeled "📊 View 2-Week Visualization" provides access to the graphical mood visualization page, showing your mood trends over the past two weeks.

### Recent Entries List ###

The main content area displays your **10 most recent mood entries** in reverse chronological order (newest first).

#### Entry Display ####

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

#### Data States ####

The page handles different data states appropriately:

- **Loading State**: Shows activity indicator while data loads
- **No Data State**: Displays "No mood entries found." message when no entries exist
- **Error State**: Shows error message in red text if data loading fails

### Navigation Features ###

#### Visualization Integration ####

The "View 2-Week Visualization" button navigates to a dedicated visualization page showing:

- Graphical representation of mood trends
- Two-week historical view
- Visual pattern analysis

#### Automatic Data Refresh ####

The page automatically refreshes data when:

- The page appears (OnAppearing event)
- Returning from other pages in the app
- The app resumes from background

### Data Insights ###

#### Understanding Your Trends ####

Use the statistics to identify patterns:

- **Consistent averages** across time periods suggest stable mood patterns
- **Improving trends** indicate positive changes in your overall well-being
- **Declining trends** may suggest areas needing attention
- **Large differences** between 7-day and 30-day averages show recent changes

#### Making the Most of History ####

**Regular Review Tips**:

- Check your weekly averages to spot recent changes
- Use the visualization page for deeper trend analysis
- Note correlations between mood patterns and life events
- Track progress toward personal wellness goals

**Data Quality Tips**:

- Consistent daily recording provides more accurate trends
- Both morning and evening entries give fuller daily pictures
- Regular app usage ensures up-to-date statistics

## Mood Visualization ##

The **Visualization** page displays your 2-week mood data as a color-coded chart, making it easy to identify day-to-day mood changes and overall trends.

### Overview ###

The visualization page shows a **2-week mood change analysis** that compares each day's mood to the previous day, helping you spot patterns in your emotional well-being over time.

#### Chart Structure ####

The main visualization displays:

- **Date Range Display**: Shows the specific 2-week period being analyzed
- **Color-Coded Chart**: Visual bars representing daily mood changes
- **Day Labels**: Individual dates across the 2-week period
- **Week Divisions**: Clear separation between Week 1 and Week 2

#### Reading the Visualization ####

The chart uses a color-coding system to show how your mood changed from day to day:

- **Significantly Improved** (Lime Green): Major positive mood changes
- **Improved** (Light Green): Moderate positive mood changes
- **No Change** (Light Blue): Stable mood from previous day
- **Declined** (Light Coral): Moderate negative mood changes
- **Significantly Declined** (Red): Major negative mood changes
- **No Data** (Light Gray): Days where mood wasn't recorded

#### Interactive Features ####

The visualization page provides simple interaction options:

##### Available Actions #####

- **Refresh Data**: Updates the visualization with the latest mood entries
- **Back to History**: Returns to the History page for detailed mood records
- **Loading States**: Shows activity indicator while data loads

**Note**: The visualization is a static display - it does not support zooming, panning, or other chart interactions.

### Understanding Your Data ###

#### Daily Change Analysis ####

The visualization helps you understand mood patterns by showing day-to-day changes rather than absolute values:

##### Change Indicators #####

- **Positive Changes**: Green colors indicate mood improvements from the previous day
- **Negative Changes**: Red/coral colors show mood declines from the previous day
- **Stability**: Blue indicates consistent mood levels
- **Missing Data**: Gray bars appear when mood data is not available

##### Pattern Recognition #####

Look for these patterns in your visualization:

- **Consecutive Improvements**: Multiple green bars in sequence
- **Declining Streaks**: Series of red/coral indicators
- **Recovery Patterns**: Red bars followed by green (bouncing back)
- **Stable Periods**: Consistent blue bars showing mood stability

### Data Display Details ###

#### Summary Information ####

Below the main chart, you'll find:

- **Date Range**: Shows the exact 2-week period displayed
- **Summary Text**: Provides key insights about your mood patterns
- **Daily Details List**: Comprehensive breakdown of each day's data

#### Daily Details List ####

The detailed view shows for each day:

- **Date**: Day of the month
- **Color Indicator**: Visual representation of the day's mood change
- **Description**: Text explanation of the mood change
- **Value**: Numerical change value (when available)

### Understanding the Legend ###

The color legend explains what each color represents:

- **Lime Green**: Significant mood improvement
- **Light Green**: Moderate mood improvement
- **Light Blue**: No significant change
- **Light Coral**: Moderate mood decline
- **Red**: Significant mood decline
- **Light Gray**: No mood data recorded

### Tips for Effective Use ###

#### Regular Review ####

- **Weekly Check-ins**: Review your visualization weekly to spot emerging patterns
- **Focus on Trends**: Look for sequences of similar colors rather than individual days
- **Context Awareness**: Remember life events that might explain significant changes

#### Data Quality ####

- **Consistent Recording**: Regular mood entries provide better visualizations
- **Honest Rating**: Accurate mood ratings create meaningful change analysis
- **Complete Records**: Try to record moods daily for the clearest patterns

#### Using Insights ####

- **Identify Triggers**: Spot patterns that precede mood improvements or declines
- **Recognize Recovery**: Notice how quickly you bounce back from difficult days
- **Track Progress**: Monitor overall trends in your mood stability
- **Share with Healthcare Providers**: Use visualizations to discuss mood patterns with professionals

## Application Settings ##

The **Settings** page allows you to customize your WorkMood experience by configuring reminder schedules, notifications, and other preferences.

![Settings Page Overview](./images/settings-main.png)
*Figure 1: Main settings page with schedule configuration options*

### Page Overview ###

The Settings page is organized around **mood tracking schedule configuration**:

- **Header**: Settings icon ⚙️ and clear page description
- **Morning Reminders**: Configure when you want morning mood prompts
- **Evening Reminders**: Set up end-of-day mood tracking reminders
- **Additional Options**: Data management and app preferences

### Morning Reminder Configuration ###

Configure when you want to be reminded to record your morning mood.

![Morning Reminder Settings](./images/settings-morning-reminder.png)
*Figure 2: Morning reminder configuration with sunrise theme*

#### Morning Reminder Features ####

- **Visual Theme**: 🌅 Sunrise emoji with warm secondary color scheme
- **Time Picker**: Intuitive time selection interface
- **Clear Description**: Explains the purpose of morning reminders
- **Persistent Settings**: Your time preference is saved automatically

#### Setting Your Morning Time ####

1. **Locate Morning Section**: Find the sunrise-themed reminder section
2. **Use Time Picker**: Click on the time picker control
3. **Select Time**: Choose your preferred morning reminder time
4. **Auto-Save**: Settings are saved immediately when changed

#### Morning Reminder Best Practices ####

- **Consistent Wake Time**: Set reminder 15-30 minutes after your usual wake time
- **Allow Settling Time**: Give yourself time to fully wake up before recording mood
- **Consider Schedule**: Account for different weekday vs. weekend wake times
- **Personal Preference**: Choose a time that works reliably with your routine

### Evening Reminder Configuration ###

Set up reminders for recording your evening mood before bedtime.

![Evening Reminder Settings](./images/settings-evening-reminder.png)
*Figure 3: Evening reminder configuration with sunset theme*

#### Evening Reminder Features ####

- **Visual Theme**: Sunset colors with primary app color scheme
- **Time Picker**: Same intuitive interface as morning settings
- **Purpose Explanation**: Clear description of evening mood tracking
- **Independent Configuration**: Separate from morning reminder settings

#### Setting Your Evening Time ####

1. **Find Evening Section**: Locate the sunset-themed reminder section
2. **Click Time Picker**: Access the time selection interface
3. **Choose Time**: Select your preferred evening reminder time
4. **Automatic Save**: Settings persist immediately upon selection

#### Evening Reminder Best Practices ####

- **Pre-Bedtime Timing**: Set reminder 30-60 minutes before your typical bedtime
- **Reflection Time**: Choose a time when you can reflect on your entire day
- **Consistent Schedule**: Try to maintain regular evening reminder times
- **Wind-Down Period**: Consider your evening routine and relaxation time

### Reminder System ###

#### How Reminders Work ####

The reminder system uses your device's built-in notification capabilities:

- **Native Notifications**: Integrates with your operating system's notification system
- **Daily Recurring**: Reminders repeat at the same time every day
- **Customizable**: Separate morning and evening reminder schedules
- **Non-Intrusive**: Gentle reminders that don't interrupt your workflow

![Notification Examples](./images/settings-notification-examples.png)
*Figure 4: Examples of WorkMood notifications on different platforms*

#### Notification Permissions ####

To receive reminders, ensure WorkMood has notification permissions:

- **Windows**: Check notification settings in Windows Settings
- **macOS**: Allow notifications in System Preferences
- **First Launch**: App may request permission when first setting reminders
- **System Integration**: Reminders appear in your system's notification center

### Additional Settings ###

#### Data Management ####

Access data-related settings and options:

- **Auto-Save**: Automatically save mood entries when recorded
- **Backup Settings**: Configure automatic data backup options
- **Export Data**: Quick access to data export functionality
- **Clear Data**: Option to reset all mood tracking data (with confirmation)

![Data Management Settings](./images/settings-data-management.png)
*Figure 5: Data management and backup options*

#### Application Preferences ####

Customize general app behavior:

- **Theme Settings**: Choose between light/dark themes (if available)
- **Language Options**: Select your preferred language
- **Units and Formats**: Configure date/time display formats
- **Startup Behavior**: Set which page opens when launching the app

### Privacy and Security ###

#### Data Privacy ####

WorkMood respects your privacy with the following practices:

- **Local Storage**: All mood data is stored locally on your device
- **No Cloud Sync**: Data is not automatically uploaded to external servers
- **User Control**: You control all data export and sharing
- **No Tracking**: App doesn't track usage beyond essential functionality

#### Data Security ####

Your mood tracking data is protected:

- **Encrypted Storage**: Mood data is encrypted on your device
- **No Network Transmission**: Data doesn't leave your device unless you export it
- **Access Control**: Only you have access to your mood tracking data
- **Secure Export**: Exported data uses secure file formats

### Settings Management ###

#### Configuration Tips ####

- **Test Reminders**: After setting up, wait for the next scheduled reminder to verify it works
- **Adjust as Needed**: Don't hesitate to modify reminder times if your schedule changes
- **Weekend Considerations**: Consider if you want different weekend reminder times
- **Backup Regularly**: Use export features to backup your configuration and data

#### Troubleshooting Settings ####

If reminders aren't working:

1. **Check Permissions**: Verify WorkMood has notification permissions
2. **System Settings**: Ensure your device's "Do Not Disturb" isn't blocking notifications
3. **App Updates**: Make sure you're running the latest version of WorkMood
4. **Restart App**: Close and reopen WorkMood to refresh settings
5. **Reset Reminders**: Try setting new reminder times to refresh the system

#### Settings Backup ####

Your settings are automatically saved but consider:

- **Export Configuration**: Include settings in your data exports
- **Document Preferences**: Keep a note of your preferred reminder times
- **Regular Backups**: Export data regularly to preserve both data and settings
- **Multiple Devices**: If using WorkMood on multiple devices, manually sync your preferred settings

## About WorkMood ##

The **About** page provides information about the WorkMood application, displaying app details, acknowledgments, and attribution information.

### App Identity ###

The About page displays basic application information:

- **App Logo**: The distinctive WorkMood smiles icon (100x100 pixels)
- **Application Title**: "WorkMood - Daily Mood Tracker"
- **Version Number**: Current version of the application (e.g., "Version 1.0")

### App Description ###

The page includes a description of WorkMood's purpose:

"WorkMood helps you track your daily mood and build healthy emotional awareness habits. Record your feelings throughout the day and view your mood patterns over time."

This section explains the core functionality and benefits of using the application.

### Special Thanks ###

The About page includes dedicated acknowledgment sections for contributors and supporters:

#### Hunter Industries ####

A highlighted acknowledgment section for Hunter Industries, displayed in a green-themed frame, thanking them for providing the opportunity and environment to develop the application.

#### GitHub Copilot ####

A cyan-themed acknowledgment section crediting GitHub Copilot for assistance with code development, UI design, and helping bring the mood tracking application to life.

### Icon Attribution ###

This section provides proper attribution for the app icon:

- **Creator**: riajulislam
- **Source**: Information about where the icon was obtained
- **Interactive Link**: A button labeled "View Icon on Flaticon" that opens the external link to the icon's source page

The button uses the app's browser service to open the attribution URL when tapped.

### Developer Information ###

A simple developer credit section displaying:

"Built with ❤️ using .NET MAUI"

This provides basic information about the development framework and approach used to create WorkMood.

### Page Layout ###

The About page uses a scrollable layout with consistent spacing and visual hierarchy:

- Centered app logo and title at the top
- Descriptive text sections with clear headings
- Color-coded acknowledgment frames for visual distinction
- Interactive button for external link access
- Proper spacing and padding throughout for readability

<!-- Written By: WorkMood Team -->
<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->
<!-- GENERATED DOCUMENT DO NOT EDIT! -->