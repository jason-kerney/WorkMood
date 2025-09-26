using WorkMood.MauiApp.Models;
using Microsoft.Maui.Controls.Shapes;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Interface for creating mood entry views following the Open/Closed Principle
/// </summary>
public interface IMoodEntryViewFactory
{
    /// <summary>
    /// Creates a view for displaying a mood entry
    /// </summary>
    /// <param name="entry">The mood entry to display</param>
    /// <returns>A view representing the mood entry</returns>
    View CreateEntryView(MoodEntry entry);
}

/// <summary>
/// Factory for creating mood entry views following the Open/Closed Principle and Single Responsibility Principle
/// </summary>
public class MoodEntryViewFactory : IMoodEntryViewFactory
{
    /// <summary>
    /// Creates a view for displaying a mood entry
    /// </summary>
    /// <param name="entry">The mood entry to display</param>
    /// <returns>A view representing the mood entry</returns>
    public View CreateEntryView(MoodEntry entry)
    {
        var border = CreateEntryContainer();
        var grid = CreateEntryGrid();
        
        // Add all the components to the grid
        grid.Children.Add(CreateDateColumn(entry));
        grid.Children.Add(CreateMoodColumn(entry));
        grid.Children.Add(CreateAverageColumn(entry));
        grid.Children.Add(CreateEmojiColumn(entry));
        
        border.Content = grid;
        return border;
    }

    /// <summary>
    /// Creates the container border for the entry
    /// </summary>
    private Border CreateEntryContainer()
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#F8F9FA"),
            Padding = new Thickness(15),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Margin = new Thickness(0, 2)
        };
    }

    /// <summary>
    /// Creates the grid layout for the entry
    /// </summary>
    private Grid CreateEntryGrid()
    {
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 15
        };
    }

    /// <summary>
    /// Creates the date column for the entry
    /// </summary>
    private StackLayout CreateDateColumn(MoodEntry entry)
    {
        var dateStack = new StackLayout { VerticalOptions = LayoutOptions.Center };
        
        dateStack.Children.Add(new Label
        {
            Text = entry.Date.ToString("MMM"),
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Gray
        });
        
        dateStack.Children.Add(new Label
        {
            Text = entry.Date.ToString("dd"),
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        });
        
        dateStack.Children.Add(new Label
        {
            Text = entry.Date.ToString("ddd"),
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Gray
        });
        
        Grid.SetColumn(dateStack, 0);
        return dateStack;
    }

    /// <summary>
    /// Creates the mood values column for the entry
    /// </summary>
    private StackLayout CreateMoodColumn(MoodEntry entry)
    {
        var moodStack = new StackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 5 };
        
        var moodRow = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 10 };
        moodRow.Children.Add(new Label { Text = "🟢", FontSize = 16 });
        moodRow.Children.Add(new Label 
        { 
            Text = entry.StartOfWork?.ToString() ?? "—", 
            FontSize = 16, 
            FontAttributes = FontAttributes.Bold 
        });
        moodRow.Children.Add(new Label { Text = "🔴", FontSize = 16 });
        moodRow.Children.Add(new Label 
        { 
            Text = entry.EndOfWork?.ToString() ?? "—", 
            FontSize = 16, 
            FontAttributes = FontAttributes.Bold 
        });
        
        moodStack.Children.Add(moodRow);
        moodStack.Children.Add(new Label
        {
            Text = $"Updated: {entry.LastModified:MMM dd, HH:mm}",
            FontSize = 12,
            TextColor = Colors.Gray
        });
        
        Grid.SetColumn(moodStack, 1);
        return moodStack;
    }

    /// <summary>
    /// Creates the average column for the entry
    /// </summary>
    private StackLayout CreateAverageColumn(MoodEntry entry)
    {
        var avgStack = new StackLayout { VerticalOptions = LayoutOptions.Center };
        
        avgStack.Children.Add(new Label
        {
            Text = "Avg",
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Gray
        });
        
        var avgValue = entry.GetAverageMood()?.ToString("F1") ?? 
                      (entry.StartOfWork?.ToString("F1") ?? "N/A");
        
        avgStack.Children.Add(new Label
        {
            Text = avgValue,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Color.FromArgb("#512BD4") // Primary color
        });
        
        Grid.SetColumn(avgStack, 2);
        return avgStack;
    }

    /// <summary>
    /// Creates the emoji column for the entry
    /// </summary>
    private Label CreateEmojiColumn(MoodEntry entry)
    {
        var emoji = GetMoodEmoji(entry);
        var emojiLabel = new Label
        {
            Text = emoji,
            FontSize = 24,
            VerticalOptions = LayoutOptions.Center
        };
        
        Grid.SetColumn(emojiLabel, 3);
        return emojiLabel;
    }

    /// <summary>
    /// Gets the appropriate emoji for a mood entry
    /// </summary>
    private string GetMoodEmoji(MoodEntry entry)
    {
        var average = entry.GetAverageMood();
        double moodValue;
        
        if (average.HasValue)
        {
            moodValue = average.Value;
        }
        else if (entry.StartOfWork.HasValue)
        {
            moodValue = entry.StartOfWork.Value;
        }
        else
        {
            return "❓";
        }
        
        return moodValue switch
        {
            >= 9.0 => "😄",
            >= 8.0 => "😊",
            >= 7.0 => "🙂",
            >= 6.0 => "😐",
            >= 5.0 => "😕",
            >= 4.0 => "☹️",
            >= 3.0 => "😟",
            >= 2.0 => "😢",
            _ => "😭"
        };
    }
}