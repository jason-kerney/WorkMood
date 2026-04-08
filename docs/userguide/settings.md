<!-- (dl (section-meta (title Application Settings))) -->

The **Settings** page allows you to customize your WorkMood experience by configuring reminder schedules, notifications, and other preferences.

![Settings Page Overview](./images/settings-main.png)
*Figure 1: Main settings page with schedule configuration options*

<!-- (dl (# Page Overview)) -->

The Settings page is organized around **mood tracking schedule configuration**:

- **Header**: Settings icon ⚙️ and clear page description
- **Morning Reminders**: Configure when you want morning mood prompts
- **Evening Reminders**: Set up end-of-day mood tracking reminders
- **Schedule Overrides**: Configure date-specific reminder times
- **Storage and Backup Options**: Manage mood data location and create one-time config backups

<!-- (dl (# Morning Reminder Configuration)) -->

Configure when you want to be reminded to record your morning mood.

![Morning Reminder Settings](./images/settings-morning-reminder.png)
*Figure 2: Morning reminder configuration with sunrise theme*

<!-- (dl (## Morning Reminder Features)) -->

- **Visual Theme**: 🌅 Sunrise emoji with warm secondary color scheme
- **Time Picker**: Intuitive time selection interface
- **Clear Description**: Explains the purpose of morning reminders
- **Save Control**: Time changes are applied when you tap **Save Settings**

<!-- (dl (## Setting Your Morning Time)) -->

1. **Locate Morning Section**: Find the sunrise-themed reminder section
2. **Use Time Picker**: Click on the time picker control
3. **Select Time**: Choose your preferred morning reminder time
4. **Save Settings**: Click **Save Settings** at the bottom of the page to persist your changes

<!-- (dl (## Morning Reminder Best Practices)) -->

- **Consistent Wake Time**: Set reminder 15-30 minutes after your usual wake time
- **Allow Settling Time**: Give yourself time to fully wake up before recording mood
- **Consider Schedule**: Account for different weekday vs. weekend wake times
- **Personal Preference**: Choose a time that works reliably with your routine

<!-- (dl (# Evening Reminder Configuration)) -->

Set up reminders for recording your evening mood before bedtime.

![Evening Reminder Settings](./images/settings-evening-reminder.png)
*Figure 3: Evening reminder configuration with sunset theme*

<!-- (dl (## Evening Reminder Features)) -->

- **Visual Theme**: Sunset colors with primary app color scheme
- **Time Picker**: Same intuitive interface as morning settings
- **Purpose Explanation**: Clear description of evening mood tracking
- **Independent Configuration**: Separate from morning reminder settings

<!-- (dl (## Setting Your Evening Time)) -->

1. **Find Evening Section**: Locate the sunset-themed reminder section
2. **Click Time Picker**: Access the time selection interface
3. **Choose Time**: Select your preferred evening reminder time
4. **Save Settings**: Click **Save Settings** at the bottom of the page to persist your changes

<!-- (dl (## Evening Reminder Best Practices)) -->

- **Pre-Bedtime Timing**: Set reminder 30-60 minutes before your typical bedtime
- **Reflection Time**: Choose a time when you can reflect on your entire day
- **Consistent Schedule**: Try to maintain regular evening reminder times
- **Wind-Down Period**: Consider your evening routine and relaxation time

<!-- (dl (# Reminder System)) -->

<!-- (dl (## How Reminders Work)) -->

The reminder system uses your device's built-in notification capabilities:

- **Native Notifications**: Integrates with your operating system's notification system
- **Daily Recurring**: Reminders repeat at the same time every day
- **Customizable**: Separate morning and evening reminder schedules
- **Non-Intrusive**: Gentle reminders that don't interrupt your workflow

![Notification Examples](./images/settings-notification-examples.png)
*Figure 4: Examples of WorkMood notifications on different platforms*

<!-- (dl (## Notification Permissions)) -->

To receive reminders, ensure WorkMood has notification permissions:

- **Windows**: Check notification settings in Windows Settings
- **macOS**: Allow notifications in System Preferences
- **First Launch**: App may request permission when first setting reminders
- **System Integration**: Reminders appear in your system's notification center

<!-- (dl (# Storage Configuration)) -->

Customize where WorkMood stores your mood tracking data with the **Storage** settings section.

![Storage Settings](./images/settings-storage.png)
*Figure 6: Storage settings with custom folder selection*

<!-- (dl (## Default Data Storage)) -->

By default, WorkMood stores your mood data in your system's user data folder:

- **Windows**: `%USERPROFILE%\Documents\WorkMood`
- **macOS**: `~/Documents/WorkMood`

Your data persists in these locations across app updates and rebuilds.

<!-- (dl (## Custom Storage Location)) -->

You can choose a custom folder to store your mood data:

1. **Open Storage Settings**: Navigate to the Settings tab and scroll to the **Storage** section
2. **Click "Browse" or "Select Folder"**: Opens a folder picker dialog
3. **Choose Your Location**: Select any accessible folder (external drive, shared folder, cloud-synced directory, etc.)
4. **Confirm Migration**: WorkMood will automatically migrate your existing mood data to the new location
5. **Verify Success**: After migration, you'll see the new path displayed in the Storage settings

<!-- (dl (## When to Use Custom Storage)) -->

Consider custom storage if you want to:

- **Backup to External Drive**: Store mood data on an external USB drive for off-site backup
- **Share Across Computers**: Place data in a cloud-synced folder (Google Drive, OneDrive, Dropbox) to access from multiple machines
- **Organize Your Files**: Keep mood data in a dedicated personal archive folder
- **Protect Important Data**: Store data in a location with specific backup or encryption settings
- **Multi-User Systems**: Use separate custom folders for different users on the same computer

<!-- (dl (## Data Migration)) -->

When you change your storage location:

- **Automatic Transfer**: All existing mood entries are automatically moved to the new location
- **No Data Loss**: Every mood entry, mood chart entry, and stored configuration follows your data
- **Seamless Transition**: The app continues working immediately after migration
- **Folder Requirements**: The folder must be writable and accessible from your computer
- **Path Validation**: WorkMood checks that the selected path is valid before migrating

<!-- (dl (## Storage Best Practices)) -->

- **Stable Locations**: Choose fixed folders rather than temporary storage
- **Accessible Paths**: Ensure the folder path won't become inaccessible (avoid network drives that disconnect)
- **Sufficient Space**: Verify there's enough free space in your chosen location (mood entries are small, but plan ahead)
- **Backup Consideration**: If storing on external drive, ensure regular backups of that drive
- **Cloud Sync Caution**: Cloud-synced folders work well but can occasionally experience sync delays
- **Permission Check**: Ensure your user account has full read/write permissions to the chosen folder

<!-- (dl (# Additional Settings)) -->

<!-- (dl (## Schedule Overrides)) -->

Use **Schedule Overrides** when a specific date needs a different reminder time.

1. Enable **Add New Override**
2. Pick the date to override
3. Choose whether to override start of work time, end of work time, or both
4. Set the override time values
5. Click **Save Settings** to apply changes

You can also edit or delete existing overrides directly from the list.

<!-- (dl (## One-Time Configuration Backup)) -->

WorkMood includes a one-time backup action for your schedule configuration.

1. On the Settings page, click **Create One-Time Config Backup...**
2. Choose a destination folder
3. WorkMood writes a timestamped JSON backup file to that folder

Important behavior:

- The backup location is **not** saved for later
- Backups are **manual only** (no automatic schedule)
- This action backs up **schedule configuration**

<!-- (dl (# Privacy and Security)) -->

<!-- (dl (## Data Privacy)) -->

WorkMood respects your privacy with the following practices:

- **Local Storage**: All mood data is stored locally on your device
- **No Cloud Sync**: Data is not automatically uploaded to external servers
- **User Control**: You control all data export and sharing
- **No Tracking**: App doesn't track usage beyond essential functionality

<!-- (dl (## Data Security)) -->

WorkMood keeps your data on your machine and does not transmit entries to external services by default.

<!-- (dl (# Settings Management)) -->

<!-- (dl (## Configuration Tips)) -->

- **Test Reminders**: After setting up, wait for the next scheduled reminder to verify it works
- **Adjust as Needed**: Don't hesitate to modify reminder times if your schedule changes
- **Weekend Considerations**: Consider if you want different weekend reminder times
- **Backup Regularly**: Use the one-time configuration backup action after important schedule updates

<!-- (dl (## Troubleshooting Settings)) -->

If reminders aren't working:

1. **Check Permissions**: Verify WorkMood has notification permissions
2. **System Settings**: Ensure your device's "Do Not Disturb" isn't blocking notifications
3. **App Updates**: Make sure you're running the latest version of WorkMood
4. **Restart App**: Close and reopen WorkMood to refresh settings
5. **Reset Reminders**: Try setting new reminder times to refresh the system