using Moq;
using SkiaSharp;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services
{
    /// <summary>
    /// Tests for LineGraphGenerator - SkiaSharp-based graphics service handling
    /// PNG image generation, focusing on constructor validation and integration tests
    /// </summary>
    public class LineGraphGeneratorShould
    {
        #region Constructor Tests
        
        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            
            // Act
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);
            
            // Assert
            Assert.NotNull(generator);
        }

        [Fact]
        public void DefaultConstructor_ShouldCreateInstanceWithDefaultFactories()
        {
            // Act
            var generator = new LineGraphGenerator();
            
            // Assert
            Assert.NotNull(generator);
        }

        [Fact]
        public void Constructor_WithNullDrawFactory_ShouldCreateInstanceWithoutThrowingException()
        {
            // Arrange
            var mockFileFactory = new Mock<IFileShimFactory>();
            
            // Act
            var generator = new LineGraphGenerator(null!, mockFileFactory.Object);
            
            // Assert
            Assert.NotNull(generator);
        }

        [Fact]
        public void Constructor_WithNullFileFactory_ShouldCreateInstanceWithoutThrowingException()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            
            // Act
            var generator = new LineGraphGenerator(mockDrawFactory.Object, null!);
            
            // Assert
            Assert.NotNull(generator);
        }

        [Fact]
        public void Constructor_WithAllNullDependencies_ShouldCreateInstanceWithoutThrowingException()
        {
            // Act
            var generator = new LineGraphGenerator(null!, null!);
            
            // Assert
            Assert.NotNull(generator);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task GenerateLineGraphAsync_WithDefaultConstructor_ShouldCompleteSuccessfully()
        {
            // Arrange - Use default constructor (real implementations)
            var generator = new LineGraphGenerator();
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));

            // Act & Assert - Should not throw exception
            var result = await generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, Colors.Blue);
            
            // Assert - Should return valid PNG byte array
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            // Verify it's a valid PNG by checking PNG header
            Assert.True(result.Length >= 8, "PNG file should be at least 8 bytes");
            Assert.Equal(0x89, result[0]); // PNG signature start
            Assert.Equal((byte)'P', result[1]);
            Assert.Equal((byte)'N', result[2]); 
            Assert.Equal((byte)'G', result[3]);
        }

        [Fact]
        public async Task SaveLineGraphAsync_WithDefaultConstructor_ShouldCreateFileSuccessfully()
        {
            // Arrange - Use default constructor (real implementations)
            var generator = new LineGraphGenerator();
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var testFilePath = Path.Combine(Path.GetTempPath(), $"test_graph_{Guid.NewGuid()}.png");

            try
            {
                // Act
                await generator.SaveLineGraphAsync(graphData, dateRange, true, true, true, true, testFilePath, Colors.Blue);

                // Assert - File should be created
                Assert.True(File.Exists(testFilePath), "Graph file should be created");
                
                var fileInfo = new FileInfo(testFilePath);
                Assert.True(fileInfo.Length > 0, "Graph file should not be empty");
                
                // Verify it's a valid PNG by reading header
                var bytes = await File.ReadAllBytesAsync(testFilePath);
                Assert.True(bytes.Length >= 8, "PNG file should be at least 8 bytes");
                Assert.Equal(0x89, bytes[0]); // PNG signature start
                Assert.Equal((byte)'P', bytes[1]);
                Assert.Equal((byte)'N', bytes[2]); 
                Assert.Equal((byte)'G', bytes[3]);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        #endregion

        #region Helper Methods

        private static GraphData CreateTestGraphData()
        {
            var entries = new List<MoodEntry>
            {
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-6)),
                    StartOfWork = 7,
                    EndOfWork = 8,
                    CreatedAt = DateTime.Now.AddDays(-6),
                    LastModified = DateTime.Now.AddDays(-6)
                },
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
                    StartOfWork = 6,
                    EndOfWork = 7,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    LastModified = DateTime.Now.AddDays(-5)
                },
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-4)),
                    StartOfWork = 8,
                    EndOfWork = 6,
                    CreatedAt = DateTime.Now.AddDays(-4),
                    LastModified = DateTime.Now.AddDays(-4)
                },
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
                    StartOfWork = 5,
                    EndOfWork = 9,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    LastModified = DateTime.Now.AddDays(-3)
                },
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                    StartOfWork = 7,
                    EndOfWork = 7,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    LastModified = DateTime.Now.AddDays(-2)
                },
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                    StartOfWork = 8,
                    EndOfWork = 9,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    LastModified = DateTime.Now.AddDays(-1)
                },
                new MoodEntry
                {
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    StartOfWork = 6,
                    EndOfWork = 8,
                    CreatedAt = DateTime.Now,
                    LastModified = DateTime.Now
                }
            };

            var dataPoints = entries.Select(entry => new FilledGraphDataPoint(
                entry.CreatedAt,
                entry.StartOfWork
            )).ToList();

            return new GraphData
            {
                DataPoints = dataPoints,
                Title = "Test Graph Data",
                YAxisLabel = "Mood Scale",
                XAxisLabel = "Days",
                YAxisRange = new AxisRange(1, 10),
                IsRawData = true
            };
        }

        #endregion
    }
}