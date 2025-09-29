using System.Text.Json;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Tests;

/// <summary>
/// Test class to verify JSON deserialization
/// </summary>
public class JsonDeserializationTest
{
    public static async Task TestDeserializationAsync()
    {
        var jsonPath = @"C:\Users\jasonkerney\AppData\Local\WorkMood\mood_data.json";
        
        Console.WriteLine("Testing JSON deserialization...");
        Console.WriteLine($"Reading from: {jsonPath}");
        
        try
        {
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("ERROR: File does not exist!");
                return;
            }
            
            // Read the JSON file
            var json = await File.ReadAllTextAsync(jsonPath);
            Console.WriteLine($"File read successfully. Content length: {json.Length} characters");
            
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("WARNING: File is empty or contains only whitespace");
                return;
            }
            
            // Create the exact same JsonSerializerOptions as MoodDataService
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // Don't use camelCase to preserve original property names
                WriteIndented = true,
                Converters = { new DateOnlyJsonConverter() }
            };
            
            Console.WriteLine("Attempting to deserialize to List<MoodEntry>...");
            
            // Try to deserialize using the same logic as MoodDataService
            var entries = JsonSerializer.Deserialize<List<MoodEntry>>(json, jsonOptions);
            
            if (entries == null)
            {
                Console.WriteLine("WARNING: Deserialization returned null");
                return;
            }
            
            Console.WriteLine($"SUCCESS: Deserialized {entries.Count} entries!");
            
            // Verify each entry
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                Console.WriteLine($"\nEntry {i + 1}:");
                Console.WriteLine($"  Date: {entry.Date} (Type: {entry.Date.GetType()})");
                Console.WriteLine($"  StartOfWork: {entry.StartOfWork}");
                Console.WriteLine($"  EndOfWork: {entry.EndOfWork}");
                Console.WriteLine($"  CreatedAt: {entry.CreatedAt}");
                Console.WriteLine($"  LastModified: {entry.LastModified}");
                Console.WriteLine($"  IsValid(): {entry.IsValid()}");
                Console.WriteLine($"  ShouldSave(): {entry.ShouldSave()}");
                Console.WriteLine($"  Value: {entry.Value}");
            }
            
            Console.WriteLine($"\nAll entries successfully deserialized and validated!");
            
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"JSON DESERIALIZATION ERROR: {jsonEx.Message}");
            Console.WriteLine($"Path: {jsonEx.Path}");
            Console.WriteLine($"Line: {jsonEx.LineNumber}, Position: {jsonEx.BytePositionInLine}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GENERAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}

/// <summary>
/// JSON converter for DateOnly type (copied from MoodDataService)
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.ParseExact(reader.GetString()!, "yyyy-MM-dd");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}