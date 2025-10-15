using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class LoggingServiceShould
{
    private readonly Mock<IFileShim> _mockFileShim;
    private readonly Mock<IDateShim> _mockDateShim;
    private readonly Mock<IFolderShim> _mockFolderShim;
    private readonly string _testLogPath;
    private readonly DateTime _testDateTime;

    public LoggingServiceShould()
    {
        _mockFileShim = new Mock<IFileShim>();
        _mockDateShim = new Mock<IDateShim>();
        _mockFolderShim = new Mock<IFolderShim>();
        _testLogPath = "C:\\TestDesktop\\WorkMood_Debug.log";
        _testDateTime = new DateTime(2024, 10, 15, 14, 30, 45, 123);
        
        // Setup default mock behavior
        _mockFolderShim.Setup(x => x.GetDesktopFolder()).Returns("C:\\TestDesktop");
        _mockFolderShim.Setup(x => x.CombinePaths("C:\\TestDesktop", "WorkMood_Debug.log")).Returns(_testLogPath);
        _mockDateShim.Setup(x => x.Now()).Returns(_testDateTime);
    }

    private LoggingService CreateLoggingService()
    {
        return new LoggingService(_mockFileShim.Object, _mockDateShim.Object, _mockFolderShim.Object);
    }

    #region Constructor Tests

    [Fact]
    public void CreateInstance_Successfully_WhenAllDependenciesProvided()
    {
        // Act
        var sut = CreateLoggingService();

        // Assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<LoggingService>();
        _mockFolderShim.Verify(x => x.GetDesktopFolder(), Times.Once);
        _mockFolderShim.Verify(x => x.CombinePaths("C:\\TestDesktop", "WorkMood_Debug.log"), Times.Once);
    }

    [Fact]
    public void CreateInstance_Successfully_WithDefaultConstructor()
    {
        // Act
        var sut = new LoggingService();

        // Assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<LoggingService>();
    }

    [Fact]
    public void ThrowArgumentNullException_WhenFileShimIsNull()
    {
        // Act
        var act = () => new LoggingService(null!, _mockDateShim.Object, _mockFolderShim.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("fileShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenDateShimIsNull()
    {
        // Act
        var act = () => new LoggingService(_mockFileShim.Object, null!, _mockFolderShim.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("dateShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenFolderShimIsNull()
    {
        // Act
        var act = () => new LoggingService(_mockFileShim.Object, _mockDateShim.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("folderShim");
    }

    #endregion

    #region Log Method Tests

    [Fact]
    public void Log_WriteCorrectFormat_WhenValidMessageProvided()
    {
        // Arrange
        var sut = CreateLoggingService();
        var message = "Test log message";
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [INFO   ] Test log message" + Environment.NewLine;

        // Act
        sut.Log(message);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    [Fact]
    public void Log_WithLogLevel_WriteCorrectFormat_WhenValidMessageProvided()
    {
        // Arrange
        var sut = CreateLoggingService();
        var message = "Error occurred";
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Error occurred" + Environment.NewLine;

        // Act
        sut.Log(LogLevel.Error, message);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "DEBUG  ")]
    [InlineData(LogLevel.Info, "INFO   ")]
    [InlineData(LogLevel.Warning, "WARNING")]
    [InlineData(LogLevel.Error, "ERROR  ")]
    public void Log_FormatLogLevelCorrectly_ForAllLogLevels(LogLevel logLevel, string expectedLevelString)
    {
        // Arrange
        var sut = CreateLoggingService();
        var message = "Test message";
        var expectedLogEntry = $"[2024-10-15 14:30:45.123] [{expectedLevelString}] Test message" + Environment.NewLine;

        // Act
        sut.Log(logLevel, message);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    [Fact]
    public void Log_DoNothing_WhenMessageIsNull()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.Log(null!);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Log_DoNothing_WhenMessageIsEmpty()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.Log(string.Empty);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Log_DoNothing_WhenMessageIsWhitespace()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.Log("   ");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Log_DoNothing_WhenMessageWithLogLevelIsNull()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.Log(LogLevel.Error, null!);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Log_DoNothing_WhenMessageWithLogLevelIsWhitespace()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.Log(LogLevel.Warning, "  \t  ");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region LogException Tests

    [Fact]
    public void LogException_WriteExceptionDetails_WhenExceptionProvided()
    {
        // Arrange
        var sut = CreateLoggingService();
        var exception = new InvalidOperationException("Something went wrong");
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Exception: Something went wrong" + Environment.NewLine;

        // Act
        sut.LogException(exception);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    [Fact]
    public void LogException_WriteExceptionWithMessage_WhenMessageProvided()
    {
        // Arrange
        var sut = CreateLoggingService();
        var exception = new ArgumentException("Invalid argument");
        var message = "Data validation failed";
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Data validation failed - Exception: Invalid argument" + Environment.NewLine;

        // Act
        sut.LogException(exception, message);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    [Fact]
    public void LogException_WriteStackTrace_WhenExceptionHasStackTrace()
    {
        // Arrange
        var sut = CreateLoggingService();
        Exception exception;
        
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        sut.LogException(exception);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.Is<string>(s => s.Contains("Exception: Test exception"))), Times.Once);
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.Is<string>(s => s.Contains("Stack Trace:"))), Times.Once);
    }

    [Fact]
    public void LogException_WriteInnerException_WhenInnerExceptionExists()
    {
        // Arrange
        var sut = CreateLoggingService();
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);

        // Act
        sut.LogException(outerException);

        // Assert
        // Should log the outer exception
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.Is<string>(s => s.Contains("Exception: Outer error"))), Times.Once);
        // Should log the inner exception
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.Is<string>(s => s.Contains("Inner Exception - Exception: Inner error"))), Times.Once);
    }

    [Fact]
    public void LogException_DoNothing_WhenExceptionIsNull()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.LogException(null!);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void LogException_HandleEmptyMessage_WhenMessageIsEmpty()
    {
        // Arrange
        var sut = CreateLoggingService();
        var exception = new InvalidOperationException("Test error");
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Exception: Test error" + Environment.NewLine;

        // Act
        sut.LogException(exception, "");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    [Fact]
    public void LogException_HandleWhitespaceMessage_WhenMessageIsWhitespace()
    {
        // Arrange
        var sut = CreateLoggingService();
        var exception = new InvalidOperationException("Test error");
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Exception: Test error" + Environment.NewLine;

        // Act
        sut.LogException(exception, "   ");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Log_SilentlyIgnoreErrors_WhenFileOperationFails()
    {
        // Arrange
        var sut = CreateLoggingService();
        _mockFileShim.Setup(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new UnauthorizedAccessException("File access denied"));

        // Act & Assert (should not throw)
        var act = () => sut.Log("Test message");
        act.Should().NotThrow();
    }

    [Fact]
    public void LogException_SilentlyIgnoreErrors_WhenFileOperationFails()
    {
        // Arrange
        var sut = CreateLoggingService();
        var exception = new InvalidOperationException("Test error");
        _mockFileShim.Setup(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new IOException("Disk full"));

        // Act & Assert (should not throw)
        var act = () => sut.LogException(exception);
        act.Should().NotThrow();
    }

    [Fact]
    public void Log_SilentlyIgnoreErrors_WhenDateShimFails()
    {
        // Arrange
        var sut = CreateLoggingService();
        _mockDateShim.Setup(x => x.Now()).Throws(new SystemException("System time error"));

        // Act & Assert (should not throw)
        var act = () => sut.Log("Test message");
        act.Should().NotThrow();
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task Log_IsThreadSafe_WhenMultipleThreadsLogConcurrently()
    {
        // Arrange
        var sut = CreateLoggingService();
        var tasks = new Task[10];
        var logCallCount = 0;

        _mockFileShim.Setup(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback(() => 
                    {
                        System.Threading.Interlocked.Increment(ref logCallCount);
                        // Simulate some processing time
                        System.Threading.Thread.Sleep(10);
                    });

        // Act
        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => sut.Log($"Message from task {taskId}"));
        }

        await Task.WhenAll(tasks);

        // Assert
        logCallCount.Should().Be(10);
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.IsAny<string>()), Times.Exactly(10));
    }

    [Fact]
    public async Task LogException_IsThreadSafe_WhenMultipleThreadsLogConcurrently()
    {
        // Arrange
        var sut = CreateLoggingService();
        var tasks = new Task[5];
        var logCallCount = 0;

        _mockFileShim.Setup(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()))
                    .Callback(() => 
                    {
                        System.Threading.Interlocked.Increment(ref logCallCount);
                        System.Threading.Thread.Sleep(10);
                    });

        // Act
        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => 
            {
                var exception = new InvalidOperationException($"Error from task {taskId}");
                sut.LogException(exception);
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        // Each LogException call results in at least one log entry (exception message)
        logCallCount.Should().BeGreaterOrEqualTo(5);
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.IsAny<string>()), Times.AtLeast(5));
    }

    #endregion

    #region Integration-like Tests

    [Fact]
    public void Log_UsesCorrectLogPath_WhenConstructorConfiguresPath()
    {
        // Arrange
        var customDesktopPath = "C:\\CustomDesktop";
        var customLogPath = "C:\\CustomDesktop\\WorkMood_Debug.log";
        
        _mockFolderShim.Setup(x => x.GetDesktopFolder()).Returns(customDesktopPath);
        _mockFolderShim.Setup(x => x.CombinePaths(customDesktopPath, "WorkMood_Debug.log")).Returns(customLogPath);
        
        var sut = CreateLoggingService();
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [INFO   ] Test message" + Environment.NewLine;

        // Act
        sut.Log("Test message");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(customLogPath, expectedLogEntry), Times.Once);
        _mockFolderShim.Verify(x => x.GetDesktopFolder(), Times.Once);
        _mockFolderShim.Verify(x => x.CombinePaths(customDesktopPath, "WorkMood_Debug.log"), Times.Once);
    }

    [Fact]
    public void Log_CallsDateShimForEveryLog_WhenLoggingMultipleMessages()
    {
        // Arrange
        var sut = CreateLoggingService();
        var firstTime = new DateTime(2024, 10, 15, 10, 0, 0);
        var secondTime = new DateTime(2024, 10, 15, 11, 0, 0);
        
        _mockDateShim.SetupSequence(x => x.Now())
                    .Returns(firstTime)
                    .Returns(secondTime);

        // Act
        sut.Log("First message");
        sut.Log("Second message");

        // Assert
        _mockDateShim.Verify(x => x.Now(), Times.Exactly(2));
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.Is<string>(s => s.Contains("10:00:00"))), Times.Once);
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, It.Is<string>(s => s.Contains("11:00:00"))), Times.Once);
    }

    #endregion

    #region IsEnabled Property Tests

    [Fact]
    public void IsEnabled_DefaultsToTrue_WhenConstructedWithParameters()
    {
        // Act
        var sut = CreateLoggingService();

        // Assert
        sut.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_DefaultsToTrue_WhenConstructedWithDefaultConstructor()
    {
        // Act
        var sut = new LoggingService();

        // Assert
        sut.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_CanBeSetToFalse()
    {
        // Arrange
        var sut = CreateLoggingService();

        // Act
        sut.IsEnabled = false;

        // Assert
        sut.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_CanBeSetToTrue()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;

        // Act
        sut.IsEnabled = true;

        // Assert
        sut.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Log_DoesNothing_WhenIsEnabledIsFalse()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;

        // Act
        sut.Log("Test message");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockDateShim.Verify(x => x.Now(), Times.Never);
    }

    [Fact]
    public void Log_WithLogLevel_DoesNothing_WhenIsEnabledIsFalse()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;

        // Act
        sut.Log(LogLevel.Error, "Test error message");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockDateShim.Verify(x => x.Now(), Times.Never);
    }

    [Fact]
    public void LogException_DoesNothing_WhenIsEnabledIsFalse()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;
        var exception = new InvalidOperationException("Test exception");

        // Act
        sut.LogException(exception);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockDateShim.Verify(x => x.Now(), Times.Never);
    }

    [Fact]
    public void LogException_WithMessage_DoesNothing_WhenIsEnabledIsFalse()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;
        var exception = new ArgumentException("Test exception");

        // Act
        sut.LogException(exception, "Additional message");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockDateShim.Verify(x => x.Now(), Times.Never);
    }

    [Fact]
    public void Log_WorksNormally_WhenIsEnabledIsTrue()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = true;
        var message = "Test message";
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [INFO   ] Test message" + Environment.NewLine;

        // Act
        sut.Log(message);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
        _mockDateShim.Verify(x => x.Now(), Times.Once);
    }

    [Fact]
    public void Log_WithLogLevel_WorksNormally_WhenIsEnabledIsTrue()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = true;
        var message = "Error occurred";
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Error occurred" + Environment.NewLine;

        // Act
        sut.Log(LogLevel.Error, message);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
        _mockDateShim.Verify(x => x.Now(), Times.Once);
    }

    [Fact]
    public void LogException_WorksNormally_WhenIsEnabledIsTrue()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = true;
        var exception = new InvalidOperationException("Something went wrong");
        var expectedLogEntry = "[2024-10-15 14:30:45.123] [ERROR  ] Exception: Something went wrong" + Environment.NewLine;

        // Act
        sut.LogException(exception);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(_testLogPath, expectedLogEntry), Times.Once);
        _mockDateShim.Verify(x => x.Now(), Times.Once);
    }

    [Fact]
    public void Log_CanBeToggled_BetweenEnabledAndDisabled()
    {
        // Arrange
        var sut = CreateLoggingService();
        var message = "Test message";

        // Act & Assert - Initially enabled (default)
        sut.Log(message);
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        // Reset mock for next verification
        _mockFileShim.Reset();
        _mockDateShim.Reset();
        
        // Act & Assert - Disabled
        sut.IsEnabled = false;
        sut.Log(message);
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // Reset mock for next verification
        _mockFileShim.Reset();
        _mockDateShim.Reset();
        
        // Act & Assert - Re-enabled
        sut.IsEnabled = true;
        sut.Log(message);
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Info)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void Log_WithAllLogLevels_DoesNothing_WhenIsEnabledIsFalse(LogLevel logLevel)
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;

        // Act
        sut.Log(logLevel, "Test message");

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockDateShim.Verify(x => x.Now(), Times.Never);
    }

    [Fact]
    public async Task Log_RespectIsEnabled_WhenLoggingConcurrently()
    {
        // Arrange
        var sut = CreateLoggingService();
        sut.IsEnabled = false;
        var tasks = new Task[5];

        // Act
        for (int i = 0; i < tasks.Length; i++)
        {
            int taskId = i;
            tasks[i] = Task.Run(() => sut.Log($"Message from task {taskId}"));
        }

        await Task.WhenAll(tasks);

        // Assert
        _mockFileShim.Verify(x => x.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockDateShim.Verify(x => x.Now(), Times.Never);
    }

    #endregion
}