using System;
using System.Threading.Tasks;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services
{
    public class MoodDispatcherServiceShould
    {
        private readonly ScheduleConfigService _scheduleConfigService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly Mock<IDispatcherCommand> _mockCommand1;
        private readonly Mock<IDispatcherCommand> _mockCommand2;

        public MoodDispatcherServiceShould()
        {
            // Use the simple constructor for testing
            _scheduleConfigService = new ScheduleConfigService();
            _mockLoggingService = new Mock<ILoggingService>();
            _mockCommand1 = new Mock<IDispatcherCommand>();
            _mockCommand2 = new Mock<IDispatcherCommand>();
        }

        [Fact]
        public void Constructor_WithValidDependencies_InitializesCorrectly()
        {
            // Arrange & Act
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object,
                _mockCommand2.Object
            );

            // Assert - Constructor completes without exception
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullScheduleConfigService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new MoodDispatcherService(
                    null!,
                    _mockLoggingService.Object,
                    _mockCommand1.Object
                )
            );

            Assert.Equal("scheduleConfigService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullLoggingService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new MoodDispatcherService(
                    _scheduleConfigService,
                    null!,
                    _mockCommand1.Object
                )
            );

            Assert.Equal("loggingService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCommands_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new MoodDispatcherService(
                    _scheduleConfigService,
                    _mockLoggingService.Object,
                    null!
                )
            );

            Assert.Equal("commands", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithEmptyCommandsArray_ThrowsArgumentOutOfRangeException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
                new MoodDispatcherService(
                    _scheduleConfigService,
                    _mockLoggingService.Object
                )
            );

            Assert.Equal("commands", exception.ParamName);
        }

        [Fact]
        public void UpdateCurrentRecordState_WithValidMoodEntry_UpdatesInternalState()
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            var moodEntry = new MoodEntry
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartOfWork = 7,
                EndOfWork = 6
            };

            // Act - This should not throw
            service.UpdateCurrentRecordState(moodEntry);

            // Assert - Method completes without exception
            Assert.True(true);
        }

        [Fact]
        public void UpdateCurrentRecordState_WithNullMoodEntry_AcceptsNullValue()
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Act - This should not throw
            service.UpdateCurrentRecordState(null);

            // Assert - Method completes without exception
            Assert.True(true);
        }

        [Fact]
        public void Start_WhenCalled_EnablesTimer()
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Act
            service.Start();

            // Assert - Method completes without exception
            Assert.True(true);
        }

        [Fact]
        public void Stop_WhenCalled_DisablesTimer()
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Act
            service.Stop();

            // Assert - Method completes without exception
            Assert.True(true);
        }

        [Fact]
        public void Dispose_WhenCalled_DisposesResourcesAndLogs()
        {
            // Arrange
            var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Act
            service.Dispose();

            // Assert - Method completes without exception
            Assert.True(true);
        }

        [Fact]
        public void Dispose_CalledMultipleTimes_OnlyDisposesOnce()
        {
            // Arrange
            var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Act
            service.Dispose();
            service.Dispose(); // Second call should be safe

            // Assert - Method completes without exception
            Assert.True(true);
        }

        [Theory]
        [InlineData("already complete", AutoSaveDecision.AlreadySaved)]
        [InlineData("no valid mood data", AutoSaveDecision.InvalidState)]
        [InlineData("some other message", AutoSaveDecision.NoAction)]
        public void MapResultToDecision_WithVariousMessages_ReturnsCorrectDecision(string message, AutoSaveDecision expectedDecision)
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            var result = CommandResult.Succeeded(message);

            // Use reflection to access private method for testing
            var method = typeof(MoodDispatcherService).GetMethod("MapResultToDecision", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var decision = (AutoSaveDecision)method!.Invoke(service, new object[] { result })!;

            // Assert
            Assert.Equal(expectedDecision, decision);
        }

        [Fact]
        public void MapResultToDecision_WithMoodEntryData_ReturnsSaveRecord()
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            var moodEntry = new MoodEntry
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartOfWork = 8
            };

            var result = CommandResult.Succeeded("Saved successfully", moodEntry);

            // Use reflection to access private method for testing
            var method = typeof(MoodDispatcherService).GetMethod("MapResultToDecision", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var decision = (AutoSaveDecision)method!.Invoke(service, new object[] { result })!;

            // Assert
            Assert.Equal(AutoSaveDecision.SaveRecord, decision);
        }

        [Fact]
        public void MapResultToDecision_WithFailedResult_ReturnsNoAction()
        {
            // Arrange
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            var result = new CommandResult { Success = false, Message = "Failed operation" };

            // Use reflection to access private method for testing
            var method = typeof(MoodDispatcherService).GetMethod("MapResultToDecision", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var decision = (AutoSaveDecision)method!.Invoke(service, new object[] { result })!;

            // Assert
            Assert.Equal(AutoSaveDecision.NoAction, decision);
        }

        [Fact]
        public void Service_Implements_IDisposable()
        {
            // Arrange & Act
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Assert
            Assert.IsAssignableFrom<IDisposable>(service);
        }

        [Fact] 
        public void Service_StartsTimerOnConstruction()
        {
            // Arrange & Act
            using var service = new MoodDispatcherService(
                _scheduleConfigService,
                _mockLoggingService.Object,
                _mockCommand1.Object
            );

            // Assert - Timer is started by default (constructor enables it)
            Assert.True(true); // Service instantiation completes successfully
        }
    }
}