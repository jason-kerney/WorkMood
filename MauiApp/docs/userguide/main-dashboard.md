<!-- (dl (section-meta (title Main Dashboard))) -->

The **Main Dashboard** is the central navigation hub of WorkMood, providing quick access to all major features of the application. The dashboard serves as your starting point for mood tracking activities.

<!-- (dl (# Dashboard Layout)) -->

The main dashboard is organized into three main sections:

<!-- (dl (## Header Section)) -->

The header displays essential information:

- **Application Title**: "WorkMood" with the subtitle "Daily Mood Tracker"
- **Current Date**: Shows today's date in full format (e.g., "Wednesday, September 24, 2025")
- The date automatically updates when the app detects a new day

<!-- (dl (## Welcome Section)) -->

A prominent welcome area features:

- **Greeting**: "👋 Welcome!" message in a colored border
- **Tagline**: "Track your daily mood and build better habits"
- This section uses the app's secondary color scheme for visual emphasis

<!-- (dl (## Navigation Buttons)) -->

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

<!-- (dl (# Footer Section)) -->

The bottom of the dashboard displays status information:

- **Auto-save Status**: "Auto-save enabled • Data synced locally"
- This indicates that your mood data is automatically saved and stored locally on your device

<!-- (dl (# Dashboard Functionality)) -->

<!-- (dl (## Navigation System)) -->

The dashboard serves as the primary navigation hub:

- **Command Binding**: Each button is connected to a specific command in the view model
- **Error Handling**: Failed navigation attempts display informative error messages
- **Service Integration**: Navigation passes required services to destination pages

<!-- (dl (## Date Management)) -->

The dashboard automatically handles date-related functionality:

- **Automatic Updates**: The current date display refreshes when the page appears
- **Date Change Detection**: The app monitors for day changes and updates accordingly
- **Event-Driven Updates**: Date changes trigger internal processes for mood data management

<!-- (dl (## Background Services)) -->

While using the dashboard, several services operate automatically:

- **Dispatcher Service**: Manages scheduled reminders and date change detection
- **Auto-save Service**: Handles automatic saving of mood data
- **Window Activation**: Brings the app to foreground when reminders trigger
- **Logging Service**: Records app events for troubleshooting

<!-- (dl (# User Notifications)) -->

The dashboard can display various types of alerts and notifications:

<!-- (dl (## Auto-save Notifications)) -->

When automatic saving occurs:
- **Date Change Auto-save**: Notifies when previous day's data is automatically saved
- **Alert Display**: Shows confirmation message that auto-save completed successfully

<!-- (dl (## Reminder Alerts)) -->

The app displays reminder notifications directly on the dashboard:

- **Morning Reminders**: Customizable morning mood tracking reminders
- **Evening Reminders**: Evening reminders with context-aware messages
- **Missed Reminders**: Special alerts for missed morning mood entries
- **Window Activation**: App automatically comes to foreground when reminders trigger

<!-- (dl (# Usage Tips)) -->

<!-- (dl (## Efficient Navigation)) -->

1. **Use as Home Base**: Return to the dashboard between different app sections
2. **Quick Access**: Tap any navigation button to jump directly to that feature
3. **Stay Updated**: Check the current date display to confirm you're tracking the right day
4. **Monitor Status**: Review the footer status to ensure auto-save is working

<!-- (dl (## Best Practices)) -->

- **Regular Check-ins**: Visit the dashboard when starting your mood tracking session
- **Respond to Reminders**: Act on reminder notifications when they appear
- **Use Navigation Shortcuts**: Leverage the quick navigation buttons instead of complex menu systems
- **Trust Auto-save**: The app automatically handles data persistence, so focus on tracking your mood
