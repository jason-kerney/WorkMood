using Moq;
using SkiaSharp;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services
{
    /// <summary>
    /// Tests for LineGraphGenerator - complex SkiaSharp-based graphics service handling
    /// PNG image generation, canvas drawing, async operations, and file I/O operations
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

        #endregion

        #region GenerateLineGraphAsync Tests (White Background)

        [Fact]
        public async Task GenerateLineGraphAsync_WithWhiteBackground_ShouldCreateBitmapWithCorrectDimensions()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            
            var expectedBytes = new byte[] { 1, 2, 3, 4 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, expectedBytes);
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            var result = await generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, Colors.Blue);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockDrawFactory.Verify(f => f.BitmapFromDimensions(800, 600), Times.Once);
            mockDrawFactory.Verify(f => f.ImageFromBitmap(mockBitmap.Object), Times.Once);
        }

        [Fact]
        public async Task GenerateLineGraphAsync_WithCustomDimensions_ShouldUseSpecifiedSize()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            
            var expectedBytes = new byte[] { 5, 6, 7, 8 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last14Days, DateOnly.FromDateTime(DateTime.Today));
            var customWidth = 1200;
            var customHeight = 800;
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, expectedBytes);
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            var result = await generator.GenerateLineGraphAsync(graphData, dateRange, false, false, false, false, Colors.Red, customWidth, customHeight);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockDrawFactory.Verify(f => f.BitmapFromDimensions(customWidth, customHeight), Times.Once);
        }

        [Fact]
        public async Task GenerateLineGraphAsync_WithEmptyGraphData_ShouldStillGenerateImage()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            
            var expectedBytes = new byte[] { 9, 10, 11, 12 };
            var emptyGraphData = new GraphData 
            { 
                DataPoints = new List<FilledGraphDataPoint>(),
                Title = "Empty Graph",
                YAxisRange = new AxisRange(0, 10),
                CenterLineValue = 5
            };
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, expectedBytes);
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            var result = await generator.GenerateLineGraphAsync(emptyGraphData, dateRange, true, true, true, true, Colors.Cyan);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockDrawFactory.Verify(f => f.BitmapFromDimensions(800, 600), Times.Once);
        }

        #endregion

        #region GenerateLineGraphAsync Tests (Custom Background)

        [Fact]
        public async Task GenerateLineGraphAsync_WithCustomBackground_ShouldLoadBackgroundImage()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            var mockBackgroundBitmap = new Mock<IBitmapShim>();
            
            var expectedBytes = new byte[] { 13, 14, 15, 16 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.LastMonth, DateOnly.FromDateTime(DateTime.Today));
            var backgroundPath = "test-background.png";
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, expectedBytes);
            mockDrawFactory.Setup(f => f.DecodeBitmapFromFile(backgroundPath))
                .Returns(mockBackgroundBitmap.Object);
            
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            var result = await generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, backgroundPath, Colors.Green);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockDrawFactory.Verify(f => f.DecodeBitmapFromFile(backgroundPath), Times.Once);
            mockCanvas.Verify(c => c.DrawBitmap(mockBackgroundBitmap.Object, It.IsAny<SKRect>()), Times.Once);
        }

        [Fact]
        public async Task GenerateLineGraphAsync_WithNullBackgroundPath_ShouldNotLoadBackground()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            
            var expectedBytes = new byte[] { 17, 18, 19, 20 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last3Months, DateOnly.FromDateTime(DateTime.Today));
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, expectedBytes);
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            var result = await generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, null!, Colors.Purple);

            // Assert
            Assert.Equal(expectedBytes, result);
            mockDrawFactory.Verify(f => f.DecodeBitmapFromFile(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region SaveLineGraphAsync Tests

        [Fact]
        public async Task SaveLineGraphAsync_ShouldGenerateAndSaveImageToFile()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockFileShim = new Mock<IFileShim>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            
            var imageBytes = new byte[] { 21, 22, 23, 24 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last6Months, DateOnly.FromDateTime(DateTime.Today));
            var filePath = "test-graph.png";
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, imageBytes);
            mockFileFactory.Setup(f => f.Create())
                .Returns(mockFileShim.Object);
            mockFileShim.Setup(f => f.WriteAllBytesAsync(filePath, imageBytes))
                .Returns(Task.CompletedTask);
            
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            await generator.SaveLineGraphAsync(graphData, dateRange, true, true, true, true, filePath, Colors.Orange);

            // Assert
            mockFileShim.Verify(f => f.WriteAllBytesAsync(filePath, imageBytes), Times.Once);
        }

        [Fact]
        public async Task SaveLineGraphAsync_WithCustomBackground_ShouldGenerateAndSaveImageWithBackground()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockFileShim = new Mock<IFileShim>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            var mockBackgroundBitmap = new Mock<IBitmapShim>();
            
            var imageBytes = new byte[] { 25, 26, 27, 28 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.LastYear, DateOnly.FromDateTime(DateTime.Today));
            var filePath = "test-graph-with-bg.png";
            var backgroundPath = "background.png";
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, imageBytes);
            mockDrawFactory.Setup(f => f.DecodeBitmapFromFile(backgroundPath))
                .Returns(mockBackgroundBitmap.Object);
            mockFileFactory.Setup(f => f.Create())
                .Returns(mockFileShim.Object);
            mockFileShim.Setup(f => f.WriteAllBytesAsync(filePath, imageBytes))
                .Returns(Task.CompletedTask);
            
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act
            await generator.SaveLineGraphAsync(graphData, dateRange, true, true, true, true, filePath, backgroundPath, Colors.Yellow);

            // Assert
            mockDrawFactory.Verify(f => f.DecodeBitmapFromFile(backgroundPath), Times.Once);
            mockFileShim.Verify(f => f.WriteAllBytesAsync(filePath, imageBytes), Times.Once);
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public async Task GenerateLineGraphAsync_WhenBitmapCreationFails_ShouldPropagateException()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            
            mockDrawFactory.Setup(f => f.BitmapFromDimensions(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new InvalidOperationException("Bitmap creation failed"));
            
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, Colors.Black));
            Assert.Equal("Bitmap creation failed", exception.Message);
        }

        [Fact]
        public async Task GenerateLineGraphAsync_WhenBackgroundLoadFails_ShouldPropagateException()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var invalidBackgroundPath = "nonexistent-background.png";
            
            mockDrawFactory.Setup(f => f.BitmapFromDimensions(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mockBitmap.Object);
            mockDrawFactory.Setup(f => f.CanvasFromBitmap(mockBitmap.Object))
                .Returns(mockCanvas.Object);
            mockDrawFactory.Setup(f => f.DecodeBitmapFromFile(invalidBackgroundPath))
                .Throws(new FileNotFoundException("Background file not found"));
            
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(
                () => generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, invalidBackgroundPath, Colors.White));
            Assert.Equal("Background file not found", exception.Message);
        }

        [Fact]
        public async Task SaveLineGraphAsync_WhenFileSaveFails_ShouldPropagateException()
        {
            // Arrange
            var mockDrawFactory = new Mock<IDrawShimFactory>();
            var mockFileFactory = new Mock<IFileShimFactory>();
            var mockFileShim = new Mock<IFileShim>();
            var mockBitmap = new Mock<IBitmapShim>();
            var mockCanvas = new Mock<ICanvasShim>();
            var mockImage = new Mock<IImageShim>();
            var mockDrawData = new Mock<IDrawDataShim>();
            
            var imageBytes = new byte[] { 29, 30, 31, 32 };
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var filePath = "readonly-directory/test-graph.png";
            
            SetupBasicMocks(mockDrawFactory, mockBitmap, mockCanvas, mockImage, mockDrawData, imageBytes);
            mockFileFactory.Setup(f => f.Create())
                .Returns(mockFileShim.Object);
            mockFileShim.Setup(f => f.WriteAllBytesAsync(filePath, imageBytes))
                .ThrowsAsync(new UnauthorizedAccessException("Cannot write to file"));
            
            var generator = new LineGraphGenerator(mockDrawFactory.Object, mockFileFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => generator.SaveLineGraphAsync(graphData, dateRange, true, true, true, true, filePath, Colors.Gray));
            Assert.Equal("Cannot write to file", exception.Message);
        }

        #endregion

        #region Integration-style Tests with Default Constructor

        [Fact]
        public async Task GenerateLineGraphAsync_WithDefaultConstructor_ShouldGenerateValidImageData()
        {
            // Arrange
            var generator = new LineGraphGenerator();
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));

            // Act
            var result = await generator.GenerateLineGraphAsync(graphData, dateRange, true, true, true, true, Colors.Blue);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // PNG files start with specific bytes
            Assert.True(result.Length > 8, "Image data should be substantial");
        }

        [Fact]
        public async Task SaveLineGraphAsync_WithDefaultConstructor_ShouldCreateFile()
        {
            // Arrange
            var generator = new LineGraphGenerator();
            var graphData = CreateTestGraphData();
            var dateRange = new DateRangeInfo(DateRange.Last7Days, DateOnly.FromDateTime(DateTime.Today));
            var tempFile = Path.GetTempFileName() + ".png";

            try
            {
                // Act
                await generator.SaveLineGraphAsync(graphData, dateRange, true, true, true, true, tempFile, Colors.Red);

                // Assert
                Assert.True(File.Exists(tempFile), "Graph file should be created");
                var fileInfo = new FileInfo(tempFile);
                Assert.True(fileInfo.Length > 0, "Graph file should not be empty");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #endregion

        #region Helper Methods

        private static GraphData CreateTestGraphData()
        {
            return new GraphData
            {
                DataPoints = new List<FilledGraphDataPoint>
                {
                    new(DateTime.Today.AddDays(-6), 3),
                    new(DateTime.Today.AddDays(-5), 7),
                    new(DateTime.Today.AddDays(-4), 5),
                    new(DateTime.Today.AddDays(-3), 8),
                    new(DateTime.Today.AddDays(-2), 4),
                    new(DateTime.Today.AddDays(-1), 6)
                },
                Title = "Test Graph",
                YAxisRange = new AxisRange(0, 10),
                CenterLineValue = 5
            };
        }

        private static void SetupBasicMocks(Mock<IDrawShimFactory> mockDrawFactory, Mock<IBitmapShim> mockBitmap, 
            Mock<ICanvasShim> mockCanvas, Mock<IImageShim> mockImage, Mock<IDrawDataShim> mockDrawData, byte[] expectedBytes)
        {
            mockDrawFactory.Setup(f => f.BitmapFromDimensions(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mockBitmap.Object);
            mockDrawFactory.Setup(f => f.CanvasFromBitmap(mockBitmap.Object))
                .Returns(mockCanvas.Object);
            mockDrawFactory.Setup(f => f.ImageFromBitmap(mockBitmap.Object))
                .Returns(mockImage.Object);
                
            mockDrawData.Setup(d => d.ToArray())
                .Returns(expectedBytes);
            mockImage.Setup(i => i.Encode(It.IsAny<SKEncodedImageFormat>(), It.IsAny<int>()))
                .Returns(mockDrawData.Object);
                
            // Setup basic drawing factory dependencies
            var mockColors = new Mock<IColorShims>();
            var mockFontFactory = new Mock<IFontShimFactory>();
            mockDrawFactory.Setup(f => f.Colors).Returns(mockColors.Object);
            mockDrawFactory.Setup(f => f.Fonts).Returns(mockFontFactory.Object);
            
            mockColors.Setup(c => c.White).Returns(Mock.Of<IColorShim>());
            mockColors.Setup(c => c.Black).Returns(Mock.Of<IColorShim>());
            mockColors.Setup(c => c.Gray).Returns(Mock.Of<IColorShim>());
            
            // Setup paint creation
            mockDrawFactory.Setup(f => f.PaintFromArgs(It.IsAny<PaintShimArgs>()))
                .Returns(Mock.Of<IPaintShim>());
        }

        #endregion
    }
}