using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services
{
    /// <summary>
    /// Tests for LineGraphService - complex service handling graph generation 
    /// from mood data with GraphMode switching and background image support
    /// </summary>
    public class LineGraphServiceShould
    {
        #region Constructor Tests
        
        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            
            // Act
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullTransformer_ShouldCreateInstanceWithoutThrowingException()
        {
            // Arrange
            var mockGenerator = new Mock<ILineGraphGenerator>();
            
            // Act
            var service = new LineGraphService(null!, mockGenerator.Object);
            
            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullGenerator_ShouldCreateInstanceWithoutThrowingException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            
            // Act
            var service = new LineGraphService(mockTransformer.Object, null!);
            
            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void DefaultConstructor_ShouldCreateInstanceWithDefaultDependencies()
        {
            // Act
            var service = new LineGraphService();
            
            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region GenerateGraphAsync Tests (White Background)

        [Theory]
        [InlineData(GraphMode.Impact)]
        [InlineData(GraphMode.Average)]
        [InlineData(GraphMode.RawData)]
        public async Task GenerateGraphAsync_WithWhiteBackground_ShouldCallTransformerWithCorrectGraphMode(GraphMode graphMode)
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var expectedBytes = new byte[] { 1, 2, 3 };
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.GenerateLineGraphAsync(transformedData, dateRange, true, true, true, true, Colors.Blue, 800, 600))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await service.GenerateGraphAsync(moodEntries, graphMode, dateRange, true, true, true, true, Colors.Blue);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockTransformer.Verify(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange), Times.Once);
            mockGenerator.Verify(g => g.GenerateLineGraphAsync(transformedData, dateRange, true, true, true, true, Colors.Blue, 800, 600), Times.Once);
        }

        [Fact]
        public async Task GenerateGraphAsync_WithInvalidGraphMode_ShouldThrowArgumentException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var invalidGraphMode = (GraphMode)999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.GenerateGraphAsync(moodEntries, invalidGraphMode, dateRange, true, true, true, true, Colors.Blue));
            Assert.Contains("Unsupported graph mode", exception.Message);
        }

        #endregion

        #region GenerateGraphAsync Tests (Custom Background)

        [Theory]
        [InlineData(GraphMode.Impact)]
        [InlineData(GraphMode.Average)]
        [InlineData(GraphMode.RawData)]
        public async Task GenerateGraphAsync_WithCustomBackground_ShouldCallTransformerWithCorrectGraphMode(GraphMode graphMode)
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var expectedBytes = new byte[] { 4, 5, 6 };
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.Last14Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var backgroundPath = "test-background.png";
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.GenerateLineGraphAsync(transformedData, dateRange, true, true, true, true, backgroundPath, Colors.Green, 800, 600))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await service.GenerateGraphAsync(moodEntries, graphMode, dateRange, true, true, true, true, backgroundPath, Colors.Green);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockTransformer.Verify(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange), Times.Once);
            mockGenerator.Verify(g => g.GenerateLineGraphAsync(transformedData, dateRange, true, true, true, true, backgroundPath, Colors.Green, 800, 600), Times.Once);
        }

        [Fact]
        public async Task GenerateGraphAsync_WithCustomBackgroundAndInvalidMode_ShouldThrowArgumentException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var backgroundPath = "test-background.png";
            var invalidGraphMode = (GraphMode)999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.GenerateGraphAsync(moodEntries, invalidGraphMode, dateRange, true, true, true, true, backgroundPath, Colors.Blue));
            Assert.Contains("Unsupported graph mode", exception.Message);
        }

        #endregion

        #region SaveGraphAsync Tests (White Background)

        [Theory]
        [InlineData(GraphMode.Impact)]
        [InlineData(GraphMode.Average)]
        [InlineData(GraphMode.RawData)]
        public async Task SaveGraphAsync_WithWhiteBackground_ShouldCallTransformerAndGeneratorCorrectly(GraphMode graphMode)
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.LastMonth, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var filePath = "test-graph.png";
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.SaveLineGraphAsync(transformedData, dateRange, true, true, true, true, filePath, Colors.Red, 800, 600))
                .Returns(Task.CompletedTask);

            // Act
            await service.SaveGraphAsync(moodEntries, graphMode, dateRange, true, true, true, true, filePath, Colors.Red);

            // Assert
            mockTransformer.Verify(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange), Times.Once);
            mockGenerator.Verify(g => g.SaveLineGraphAsync(transformedData, dateRange, true, true, true, true, filePath, Colors.Red, 800, 600), Times.Once);
        }

        [Fact]
        public async Task SaveGraphAsync_WithInvalidGraphMode_ShouldThrowArgumentException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var filePath = "test-graph.png";
            var invalidGraphMode = (GraphMode)999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.SaveGraphAsync(moodEntries, invalidGraphMode, dateRange, true, true, true, true, filePath, Colors.Blue));
            Assert.Contains("Unsupported graph mode", exception.Message);
        }

        #endregion

        #region SaveGraphAsync Tests (Custom Background)

        [Theory]
        [InlineData(GraphMode.Impact)]
        [InlineData(GraphMode.Average)]  
        [InlineData(GraphMode.RawData)]
        public async Task SaveGraphAsync_WithCustomBackground_ShouldCallTransformerAndGeneratorCorrectly(GraphMode graphMode)
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.Last3Months, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var filePath = "test-graph-with-bg.png";
            var backgroundPath = "save-background.png";
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.SaveLineGraphAsync(transformedData, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Purple, 800, 600))
                .Returns(Task.CompletedTask);

            // Act
            await service.SaveGraphAsync(moodEntries, graphMode, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Purple);

            // Assert
            mockTransformer.Verify(t => t.TransformMoodEntries(moodEntries, graphMode, dateRange), Times.Once);
            mockGenerator.Verify(g => g.SaveLineGraphAsync(transformedData, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Purple, 800, 600), Times.Once);
        }

        [Fact]
        public async Task SaveGraphAsync_WithCustomBackgroundAndInvalidMode_ShouldThrowArgumentException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var filePath = "test-graph.png";
            var backgroundPath = "test-background.png";
            var invalidGraphMode = (GraphMode)999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.SaveGraphAsync(moodEntries, invalidGraphMode, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Blue));
            Assert.Contains("Unsupported graph mode", exception.Message);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task GenerateGraphAsync_WithEmptyMoodEntries_ShouldStillCallDependencies()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var expectedBytes = new byte[] { 7, 8, 9 };
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.Last6Months, DateOnly.FromDateTime(DateTime.Today));
            var emptyMoodEntries = new List<MoodEntry>();
            
            mockTransformer.Setup(t => t.TransformMoodEntries(emptyMoodEntries, GraphMode.Impact, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.GenerateLineGraphAsync(transformedData, dateRange, false, false, false, false, Colors.Black, 800, 600))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await service.GenerateGraphAsync(emptyMoodEntries, GraphMode.Impact, dateRange, false, false, false, false, Colors.Black);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockTransformer.Verify(t => t.TransformMoodEntries(emptyMoodEntries, GraphMode.Impact, dateRange), Times.Once);
        }

        [Fact]
        public async Task SaveGraphAsync_WithCustomDimensions_ShouldPassCorrectParameters()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.LastYear, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var filePath = "large-graph.png";
            var customWidth = 1200;
            var customHeight = 800;
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, GraphMode.Average, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.SaveLineGraphAsync(transformedData, dateRange, true, false, true, false, filePath, Colors.Orange, customWidth, customHeight))
                .Returns(Task.CompletedTask);

            // Act
            await service.SaveGraphAsync(moodEntries, GraphMode.Average, dateRange, true, false, true, false, filePath, Colors.Orange, customWidth, customHeight);

            // Assert
            mockGenerator.Verify(g => g.SaveLineGraphAsync(transformedData, dateRange, true, false, true, false, filePath, Colors.Orange, customWidth, customHeight), Times.Once);
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public async Task GenerateGraphAsync_WhenTransformerThrows_ShouldPropagateException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, GraphMode.Impact, dateRange))
                .Throws(new InvalidOperationException("Transform failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GenerateGraphAsync(moodEntries, GraphMode.Impact, dateRange, true, true, true, true, Colors.Blue));
            Assert.Equal("Transform failed", exception.Message);
        }

        [Fact]
        public async Task GenerateGraphAsync_WhenGeneratorThrows_ShouldPropagateException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, GraphMode.RawData, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.GenerateLineGraphAsync(transformedData, dateRange, true, true, true, true, Colors.Cyan, 800, 600))
                .ThrowsAsync(new IOException("Generation failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<IOException>(
                () => service.GenerateGraphAsync(moodEntries, GraphMode.RawData, dateRange, true, true, true, true, Colors.Cyan));
            Assert.Equal("Generation failed", exception.Message);
        }

        [Fact]
        public async Task SaveGraphAsync_WhenSaveThrows_ShouldPropagateException()
        {
            // Arrange
            var mockTransformer = new Mock<IGraphDataTransformer>();
            var mockGenerator = new Mock<ILineGraphGenerator>();
            var service = new LineGraphService(mockTransformer.Object, mockGenerator.Object);
            
            var transformedData = new GraphData();
            var dateRange = new DateRangeInfo(DateRange.LastMonth, DateOnly.FromDateTime(DateTime.Today));
            var moodEntries = new List<MoodEntry>();
            var filePath = "failing-save.png";
            var backgroundPath = "background.png";
            
            mockTransformer.Setup(t => t.TransformMoodEntries(moodEntries, GraphMode.Average, dateRange))
                .Returns(transformedData);
            mockGenerator.Setup(g => g.SaveLineGraphAsync(transformedData, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Yellow, 800, 600))
                .ThrowsAsync(new UnauthorizedAccessException("Cannot save file"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.SaveGraphAsync(moodEntries, GraphMode.Average, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Yellow));
            Assert.Equal("Cannot save file", exception.Message);
        }

        #endregion
    }
}