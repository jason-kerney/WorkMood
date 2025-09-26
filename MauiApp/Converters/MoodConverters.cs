using System.Globalization;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Converters;

/// <summary>
/// Converter that returns the average mood as a formatted string
/// </summary>
public class MoodAverageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MoodEntryOld entry)
        {
            var average = entry.GetAverageMood();
            if (average.HasValue)
            {
                return average.Value.ToString("F1");
            }
            
            // If no average but has morning mood, show morning mood
            if (entry.MorningMood.HasValue)
            {
                return entry.MorningMood.Value.ToString("F1");
            }
            
            return "N/A";
        }
        return "N/A";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that returns an emoji based on the average mood
/// </summary>
public class MoodEmojiConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MoodEntryOld entry)
        {
            var average = entry.GetAverageMood();
            double moodValue;
            
            if (average.HasValue)
            {
                moodValue = average.Value;
            }
            else if (entry.MorningMood.HasValue)
            {
                // If no average but has morning mood, use morning mood
                moodValue = entry.MorningMood.Value;
            }
            else
            {
                return "❓"; // Unknown/no data
            }
            
            return moodValue switch
            {
                >= 9.0 => "😄", // Excellent
                >= 8.0 => "😊", // Very Good
                >= 7.0 => "🙂", // Good
                >= 6.0 => "😐", // Okay
                >= 5.0 => "😕", // Neutral
                >= 4.0 => "☹️", // Not Great
                >= 3.0 => "😟", // Bad
                >= 2.0 => "😢", // Very Bad
                _ => "😭"       // Terrible
            };
        }
        return "❓"; // Unknown
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that converts boolean values to colors based on parameters
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string colorParams)
        {
            var colors = colorParams.Split(',');
            if (colors.Length == 2)
            {
                var trueColor = colors[0].Trim();
                var falseColor = colors[1].Trim();
                
                var selectedColor = boolValue ? trueColor : falseColor;
                
                // Handle predefined color names
                return selectedColor switch
                {
                    "White" => Colors.White,
                    "Black" => Colors.Black,
                    "Transparent" => Colors.Transparent,
                    "Gray" => Colors.Gray,
                    _ => Color.FromArgb(selectedColor)
                };
            }
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that inverts boolean values
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Converter that checks if a value is not null
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter that displays nullable mood values with fallback text
/// </summary>
public class NullableMoodConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int mood)
        {
            return mood.ToString();
        }
        return "—"; // Em dash for missing value
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}