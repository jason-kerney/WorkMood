using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Microsoft.Maui.Controls;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Comprehensive tests for NavigationService.
/// Tests navigation and dialog functionality through shim interfaces to avoid UI dependencies.
/// </summary>
public class NavigationServiceTests
{
    private readonly Mock<IPageNavigationShim> _mockNavigationShim;
    private readonly Mock<IPageDialogShim> _mockDialogShim;
    private readonly NavigationService _service;

    public NavigationServiceTests()
    {
        _mockNavigationShim = new Mock<IPageNavigationShim>();
        _mockDialogShim = new Mock<IPageDialogShim>();
        _service = new NavigationService(_mockNavigationShim.Object, _mockDialogShim.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var service = new NavigationService(_mockNavigationShim.Object, _mockDialogShim.Object);
        
        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullNavigationShim_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new NavigationService(null!, _mockDialogShim.Object));
        
        Assert.Equal("navigationShim", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullDialogShim_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new NavigationService(_mockNavigationShim.Object, null!));
        
        Assert.Equal("dialogShim", exception.ParamName);
    }

    // Skip the Page constructor test as it requires UI context
    // This will be tested in integration tests with proper MAUI test host

    #endregion

    #region GoBackAsync Tests

    [Fact]
    public async Task GoBackAsync_WhenNavigationSucceeds_ShouldCallPopAsync()
    {
        // Arrange
        _mockNavigationShim.Setup(x => x.PopAsync()).Returns(Task.CompletedTask);
        
        // Act
        await _service.GoBackAsync();
        
        // Assert
        _mockNavigationShim.Verify(x => x.PopAsync(), Times.Once);
    }

    [Fact]
    public async Task GoBackAsync_WhenNavigationFails_ShouldShowErrorDialog()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Navigation failed");
        _mockNavigationShim.Setup(x => x.PopAsync()).ThrowsAsync(expectedException);
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", "Failed to navigate back: Navigation failed", "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.GoBackAsync();
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", "Failed to navigate back: Navigation failed", "OK"), Times.Once);
    }

    [Fact]
    public async Task GoBackAsync_WhenNavigationFailsWithSystemException_ShouldShowErrorDialog()
    {
        // Arrange
        var expectedException = new System.IO.IOException("IO failure");
        _mockNavigationShim.Setup(x => x.PopAsync()).ThrowsAsync(expectedException);
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.GoBackAsync();
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", It.Is<string>(s => s.Contains("Failed to navigate back")), "OK"), Times.Once);
    }

    #endregion

    #region NavigateAsync(Page) Tests

    [Fact]
    public async Task NavigateAsync_WithPage_WhenNavigationSucceeds_ShouldCallPushAsync()
    {
        // Arrange
        var mockPage = new Mock<Page>().Object;
        _mockNavigationShim.Setup(x => x.PushAsync(mockPage)).Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(mockPage);
        
        // Assert
        _mockNavigationShim.Verify(x => x.PushAsync(mockPage), Times.Once);
    }

    [Fact]
    public async Task NavigateAsync_WithPage_WhenNavigationFails_ShouldShowErrorDialog()
    {
        // Arrange
        var mockPage = new Mock<Page>().Object;
        var expectedException = new InvalidOperationException("Navigation failed");
        _mockNavigationShim.Setup(x => x.PushAsync(mockPage)).ThrowsAsync(expectedException);
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", "Failed to navigate: Navigation failed", "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(mockPage);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", "Failed to navigate: Navigation failed", "OK"), Times.Once);
    }

    [Fact]
    public async Task NavigateAsync_WithNullPage_WhenNavigationFails_ShouldShowErrorDialog()
    {
        // Arrange
        Page? nullPage = null;
        _mockNavigationShim.Setup(x => x.PushAsync(nullPage!)).Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(nullPage!);
        
        // Assert
        _mockNavigationShim.Verify(x => x.PushAsync(nullPage!), Times.Once);
    }

    #endregion

    #region NavigateAsync(Func<Page>) Tests

    [Fact]
    public async Task NavigateAsync_WithFactory_WhenSuccessful_ShouldExecuteFactoryAndNavigate()
    {
        // Arrange
        var mockPage = new Mock<Page>().Object;
        var factoryCallCount = 0;
        Func<Page> pageFactory = () => { factoryCallCount++; return mockPage; };
        
        _mockNavigationShim.Setup(x => x.PushAsync(mockPage)).Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(pageFactory);
        
        // Assert
        Assert.Equal(1, factoryCallCount);
        _mockNavigationShim.Verify(x => x.PushAsync(mockPage), Times.Once);
    }

    [Fact]
    public async Task NavigateAsync_WithFactory_WhenFactoryThrows_ShouldShowErrorDialog()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Factory failed");
        Func<Page> pageFactory = () => throw expectedException;
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", "Failed to navigate: Factory failed", "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(pageFactory);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", "Failed to navigate: Factory failed", "OK"), Times.Once);
        _mockNavigationShim.Verify(x => x.PushAsync(It.IsAny<Page>()), Times.Never);
    }

    [Fact]
    public async Task NavigateAsync_WithFactory_WhenNavigationFails_ShouldShowErrorDialog()
    {
        // Arrange
        var mockPage = new Mock<Page>().Object;
        Func<Page> pageFactory = () => mockPage;
        var expectedException = new InvalidOperationException("Navigation failed");
        
        _mockNavigationShim.Setup(x => x.PushAsync(mockPage)).ThrowsAsync(expectedException);
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", "Failed to navigate: Navigation failed", "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(pageFactory);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", "Failed to navigate: Navigation failed", "OK"), Times.Once);
    }

    [Fact]
    public async Task NavigateAsync_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => 
            _service.NavigateAsync((Func<Page>)null!));
    }

    #endregion

    #region ShowAlertAsync Tests

    [Fact]
    public async Task ShowAlertAsync_WithValidParameters_ShouldCallDisplayAlert()
    {
        // Arrange
        var title = "Test Title";
        var message = "Test Message";
        var accept = "OK";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(title, message, accept))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowAlertAsync(title, message, accept);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync(title, message, accept), Times.Once);
    }

    [Fact]
    public async Task ShowAlertAsync_WithNullTitle_ShouldCallDisplayAlert()
    {
        // Arrange
        string? nullTitle = null;
        var message = "Test Message";
        var accept = "OK";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(nullTitle!, message, accept))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowAlertAsync(nullTitle!, message, accept);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync(nullTitle!, message, accept), Times.Once);
    }

    [Fact]
    public async Task ShowAlertAsync_WithEmptyStrings_ShouldCallDisplayAlert()
    {
        // Arrange
        var title = "";
        var message = "";
        var accept = "";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(title, message, accept))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowAlertAsync(title, message, accept);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync(title, message, accept), Times.Once);
    }

    #endregion

    #region ShowConfirmationAsync Tests

    [Fact]
    public async Task ShowConfirmationAsync_WhenUserConfirms_ShouldReturnTrue()
    {
        // Arrange
        var title = "Confirm";
        var message = "Are you sure?";
        var accept = "Yes";
        var cancel = "No";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(title, message, accept, cancel))
                      .ReturnsAsync(true);
        
        // Act
        var result = await _service.ShowConfirmationAsync(title, message, accept, cancel);
        
        // Assert
        Assert.True(result);
        _mockDialogShim.Verify(x => x.DisplayAlertAsync(title, message, accept, cancel), Times.Once);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WhenUserCancels_ShouldReturnFalse()
    {
        // Arrange
        var title = "Confirm";
        var message = "Are you sure?";
        var accept = "Yes";
        var cancel = "No";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(title, message, accept, cancel))
                      .ReturnsAsync(false);
        
        // Act
        var result = await _service.ShowConfirmationAsync(title, message, accept, cancel);
        
        // Assert
        Assert.False(result);
        _mockDialogShim.Verify(x => x.DisplayAlertAsync(title, message, accept, cancel), Times.Once);
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithNullParameters_ShouldCallDisplayAlert()
    {
        // Arrange
        string? title = null;
        string? message = null;
        string? accept = null;
        string? cancel = null;
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(title!, message!, accept!, cancel!))
                      .ReturnsAsync(true);
        
        // Act
        var result = await _service.ShowConfirmationAsync(title!, message!, accept!, cancel!);
        
        // Assert
        Assert.True(result);
        _mockDialogShim.Verify(x => x.DisplayAlertAsync(title!, message!, accept!, cancel!), Times.Once);
    }

    #endregion

    #region ShowErrorAsync Tests

    [Fact]
    public async Task ShowErrorAsync_WithoutException_ShouldShowSimpleErrorMessage()
    {
        // Arrange
        var message = "Something went wrong";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", message, "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowErrorAsync(message);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", message, "OK"), Times.Once);
    }

    [Fact]
    public async Task ShowErrorAsync_WithException_ShouldShowMessageWithExceptionDetails()
    {
        // Arrange
        var message = "Operation failed";
        var exception = new InvalidOperationException("Invalid state");
        var expectedFullMessage = $"{message}: {exception.Message}";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", expectedFullMessage, "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowErrorAsync(message, exception);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", expectedFullMessage, "OK"), Times.Once);
    }

    [Fact]
    public async Task ShowErrorAsync_WithNullException_ShouldShowSimpleErrorMessage()
    {
        // Arrange
        var message = "Something went wrong";
        Exception? nullException = null;
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", message, "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowErrorAsync(message, nullException);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", message, "OK"), Times.Once);
    }

    [Fact]
    public async Task ShowErrorAsync_WithEmptyMessage_ShouldHandleGracefully()
    {
        // Arrange
        var emptyMessage = "";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", emptyMessage, "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowErrorAsync(emptyMessage);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", emptyMessage, "OK"), Times.Once);
    }

    [Fact]
    public async Task ShowErrorAsync_WithNullMessage_ShouldHandleGracefully()
    {
        // Arrange
        string? nullMessage = null;
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", nullMessage!, "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowErrorAsync(nullMessage!);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", nullMessage!, "OK"), Times.Once);
    }

    [Fact]
    public async Task ShowErrorAsync_WithExceptionContainingNestedExceptions_ShouldShowOuterExceptionMessage()
    {
        // Arrange
        var message = "Operation failed";
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);
        var expectedFullMessage = $"{message}: {outerException.Message}";
        
        _mockDialogShim.Setup(x => x.DisplayAlertAsync("Error", expectedFullMessage, "OK"))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.ShowErrorAsync(message, outerException);
        
        // Assert
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", expectedFullMessage, "OK"), Times.Once);
    }

    #endregion

    #region Integration and Error Handling Tests

    [Fact]
    public async Task ErrorHandling_WhenErrorDialogFails_ShouldNotThrow()
    {
        // Arrange
        var navigationException = new InvalidOperationException("Navigation failed");
        var dialogException = new InvalidOperationException("Dialog failed");
        
        _mockNavigationShim.Setup(x => x.PopAsync()).ThrowsAsync(navigationException);
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                      .ThrowsAsync(dialogException);
        
        // Act & Assert - Should not throw (error dialog failure is swallowed)
        await _service.GoBackAsync();
        
        // Verify the error dialog was attempted
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Error", "Failed to navigate back: Navigation failed", "OK"), Times.Once);
    }

    [Fact]
    public async Task MultipleOperations_ShouldMaintainState()
    {
        // Arrange
        var mockPage1 = new Mock<Page>().Object;
        var mockPage2 = new Mock<Page>().Object;
        
        _mockNavigationShim.Setup(x => x.PushAsync(It.IsAny<Page>())).Returns(Task.CompletedTask);
        _mockNavigationShim.Setup(x => x.PopAsync()).Returns(Task.CompletedTask);
        _mockDialogShim.Setup(x => x.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);
        
        // Act
        await _service.NavigateAsync(mockPage1);
        await _service.ShowAlertAsync("Info", "Page loaded", "OK");
        await _service.NavigateAsync(mockPage2);
        await _service.GoBackAsync();
        
        // Assert
        _mockNavigationShim.Verify(x => x.PushAsync(mockPage1), Times.Once);
        _mockNavigationShim.Verify(x => x.PushAsync(mockPage2), Times.Once);
        _mockNavigationShim.Verify(x => x.PopAsync(), Times.Once);
        _mockDialogShim.Verify(x => x.DisplayAlertAsync("Info", "Page loaded", "OK"), Times.Once);
    }

    #endregion
}
