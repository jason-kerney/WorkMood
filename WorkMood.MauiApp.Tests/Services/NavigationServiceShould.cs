using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Moq;
using Xunit;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Tests.Services
{
    public class NavigationServiceShould
    {
        private readonly Mock<Page> _mockPage;
        private readonly Mock<INavigation> _mockNavigation;

        public NavigationServiceShould()
        {
            _mockPage = new Mock<Page>();
            _mockNavigation = new Mock<INavigation>();
            
            // Setup Page.Navigation property
            _mockPage.Setup(p => p.Navigation).Returns(_mockNavigation.Object);
        }

        private NavigationService CreateNavigationService()
        {
            return new NavigationService(_mockPage.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenPageIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new NavigationService(null!));
        }

        [Fact]
        public void Constructor_ShouldAcceptValidPage()
        {
            // Act & Assert - Should not throw
            var service = CreateNavigationService();
            
            Assert.NotNull(service);
        }

        #endregion

        #region GoBackAsync Tests

        [Fact]
        public async Task GoBackAsync_ShouldCallPageNavigationPopAsync()
        {
            // Arrange
            var service = CreateNavigationService();

            // Act
            await service.GoBackAsync();

            // Assert
            _mockNavigation.Verify(n => n.PopAsync(), Times.Once);
        }

        [Fact]
        public async Task GoBackAsync_ShouldHandleException_WhenNavigationFails()
        {
            // Arrange
            var service = CreateNavigationService();
            var exception = new InvalidOperationException("Navigation failed");
            
            _mockNavigation.Setup(n => n.PopAsync()).ThrowsAsync(exception);
            
            // Setup DisplayAlert to prevent null reference
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act - Should not throw
            await service.GoBackAsync();

            // Assert
            _mockNavigation.Verify(n => n.PopAsync(), Times.Once);
            _mockPage.Verify(p => p.DisplayAlert("Error", It.Is<string>(s => s.Contains("Failed to navigate back")), "OK"), Times.Once);
        }

        #endregion

        #region NavigateAsync(Page) Tests

        [Fact]
        public async Task NavigateAsync_ShouldCallPageNavigationPushAsync_WithProvidedPage()
        {
            // Arrange
            var service = CreateNavigationService();
            var targetPage = new Mock<Page>().Object;

            // Act
            await service.NavigateAsync(targetPage);

            // Assert
            _mockNavigation.Verify(n => n.PushAsync(targetPage), Times.Once);
        }

        [Fact]
        public async Task NavigateAsync_ShouldHandleException_WhenNavigationFails()
        {
            // Arrange
            var service = CreateNavigationService();
            var targetPage = new Mock<Page>().Object;
            var exception = new InvalidOperationException("Navigation failed");
            
            _mockNavigation.Setup(n => n.PushAsync(It.IsAny<Page>())).ThrowsAsync(exception);
            
            // Setup DisplayAlert to prevent null reference
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act - Should not throw
            await service.NavigateAsync(targetPage);

            // Assert
            _mockNavigation.Verify(n => n.PushAsync(targetPage), Times.Once);
            _mockPage.Verify(p => p.DisplayAlert("Error", It.Is<string>(s => s.Contains("Failed to navigate")), "OK"), Times.Once);
        }

        #endregion

        #region NavigateAsync(Func<Page>) Tests

        [Fact]
        public async Task NavigateAsync_ShouldCallPageFactory_AndNavigateToResultingPage()
        {
            // Arrange
            var service = CreateNavigationService();
            var targetPage = new Mock<Page>().Object;
            var factoryCalled = false;
            
            Func<Page> pageFactory = () =>
            {
                factoryCalled = true;
                return targetPage;
            };

            // Act
            await service.NavigateAsync(pageFactory);

            // Assert
            Assert.True(factoryCalled);
            _mockNavigation.Verify(n => n.PushAsync(targetPage), Times.Once);
        }

        [Fact]
        public async Task NavigateAsync_ShouldHandleException_WhenPageFactoryFails()
        {
            // Arrange
            var service = CreateNavigationService();
            var exception = new InvalidOperationException("Factory failed");
            
            Func<Page> pageFactory = () => throw exception;
            
            // Setup DisplayAlert to prevent null reference
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act - Should not throw
            await service.NavigateAsync(pageFactory);

            // Assert
            _mockNavigation.Verify(n => n.PushAsync(It.IsAny<Page>()), Times.Never);
            _mockPage.Verify(p => p.DisplayAlert("Error", It.Is<string>(s => s.Contains("Failed to navigate")), "OK"), Times.Once);
        }

        [Fact]
        public async Task NavigateAsync_ShouldHandleException_WhenNavigationFailsAfterFactorySuccess()
        {
            // Arrange
            var service = CreateNavigationService();
            var targetPage = new Mock<Page>().Object;
            var navigationException = new InvalidOperationException("Navigation failed");
            
            Func<Page> pageFactory = () => targetPage;
            
            _mockNavigation.Setup(n => n.PushAsync(targetPage)).ThrowsAsync(navigationException);
            
            // Setup DisplayAlert to prevent null reference
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act - Should not throw
            await service.NavigateAsync(pageFactory);

            // Assert
            _mockNavigation.Verify(n => n.PushAsync(targetPage), Times.Once);
            _mockPage.Verify(p => p.DisplayAlert("Error", It.Is<string>(s => s.Contains("Failed to navigate")), "OK"), Times.Once);
        }

        #endregion

        #region ShowAlertAsync Tests

        [Fact]
        public async Task ShowAlertAsync_ShouldCallPageDisplayAlert_WithProvidedParameters()
        {
            // Arrange
            var service = CreateNavigationService();
            var title = "Test Title";
            var message = "Test Message";
            var accept = "OK";
            
            _mockPage.Setup(p => p.DisplayAlert(title, message, accept))
                     .Returns(Task.CompletedTask);

            // Act
            await service.ShowAlertAsync(title, message, accept);

            // Assert
            _mockPage.Verify(p => p.DisplayAlert(title, message, accept), Times.Once);
        }

        [Theory]
        [InlineData("", "Message", "OK")]
        [InlineData("Title", "", "OK")]
        [InlineData("Title", "Message", "")]
        public async Task ShowAlertAsync_ShouldHandleVariousStringParameters(string title, string message, string accept)
        {
            // Arrange
            var service = CreateNavigationService();
            
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act & Assert - Should not throw
            await service.ShowAlertAsync(title, message, accept);
            
            _mockPage.Verify(p => p.DisplayAlert(title, message, accept), Times.Once);
        }

        [Fact]
        public async Task ShowAlertAsync_ShouldHandleNullParameters()
        {
            // Arrange
            var service = CreateNavigationService();
            
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act & Assert - Should not throw
            await service.ShowAlertAsync(null!, "Message", "OK");
            await service.ShowAlertAsync("Title", null!, "OK");
            await service.ShowAlertAsync("Title", "Message", null!);
            
            _mockPage.Verify(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        }

        #endregion

        #region ShowConfirmationAsync Tests

        [Fact]
        public async Task ShowConfirmationAsync_ShouldCallPageDisplayAlert_WithProvidedParameters()
        {
            // Arrange
            var service = CreateNavigationService();
            var title = "Confirm Title";
            var message = "Confirm Message";
            var accept = "Yes";
            var cancel = "No";
            var expectedResult = true;
            
            _mockPage.Setup(p => p.DisplayAlert(title, message, accept, cancel))
                     .ReturnsAsync(expectedResult);

            // Act
            var result = await service.ShowConfirmationAsync(title, message, accept, cancel);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockPage.Verify(p => p.DisplayAlert(title, message, accept, cancel), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShowConfirmationAsync_ShouldReturnCorrectResult(bool expectedResult)
        {
            // Arrange
            var service = CreateNavigationService();
            var title = "Test";
            var message = "Test";
            var accept = "Yes";
            var cancel = "No";
            
            _mockPage.Setup(p => p.DisplayAlert(title, message, accept, cancel))
                     .ReturnsAsync(expectedResult);

            // Act
            var result = await service.ShowConfirmationAsync(title, message, accept, cancel);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("", "Message", "Accept", "Cancel")]
        [InlineData("Title", "", "Accept", "Cancel")]
        [InlineData("Title", "Message", "", "Cancel")]
        [InlineData("Title", "Message", "Accept", "")]
        public async Task ShowConfirmationAsync_ShouldHandleVariousStringParameters(string title, string message, string accept, string cancel)
        {
            // Arrange
            var service = CreateNavigationService();
            
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(false);

            // Act & Assert - Should not throw
            var result = await service.ShowConfirmationAsync(title, message, accept, cancel);
            
            Assert.False(result); // Default mock return value
            _mockPage.Verify(p => p.DisplayAlert(title, message, accept, cancel), Times.Once);
        }

        #endregion

        #region ShowErrorAsync Tests

        [Fact]
        public async Task ShowErrorAsync_ShouldCallShowAlertAsync_WithErrorTitle()
        {
            // Arrange
            var service = CreateNavigationService();
            var message = "Test error message";
            
            _mockPage.Setup(p => p.DisplayAlert("Error", message, "OK"))
                     .Returns(Task.CompletedTask);

            // Act
            await service.ShowErrorAsync(message);

            // Assert
            _mockPage.Verify(p => p.DisplayAlert("Error", message, "OK"), Times.Once);
        }

        [Fact]
        public async Task ShowErrorAsync_ShouldIncludeExceptionMessage_WhenExceptionProvided()
        {
            // Arrange
            var service = CreateNavigationService();
            var message = "Operation failed";
            var exception = new InvalidOperationException("Specific error details");
            var expectedFullMessage = $"{message}: {exception.Message}";
            
            _mockPage.Setup(p => p.DisplayAlert("Error", expectedFullMessage, "OK"))
                     .Returns(Task.CompletedTask);

            // Act
            await service.ShowErrorAsync(message, exception);

            // Assert
            _mockPage.Verify(p => p.DisplayAlert("Error", expectedFullMessage, "OK"), Times.Once);
        }

        [Fact]
        public async Task ShowErrorAsync_ShouldHandleNullException()
        {
            // Arrange
            var service = CreateNavigationService();
            var message = "Test error message";
            
            _mockPage.Setup(p => p.DisplayAlert("Error", message, "OK"))
                     .Returns(Task.CompletedTask);

            // Act
            await service.ShowErrorAsync(message, null);

            // Assert
            _mockPage.Verify(p => p.DisplayAlert("Error", message, "OK"), Times.Once);
        }

        [Theory]
        [InlineData("")]
        public async Task ShowErrorAsync_ShouldHandleEmptyMessage(string message)
        {
            // Arrange
            var service = CreateNavigationService();
            
            _mockPage.Setup(p => p.DisplayAlert("Error", It.IsAny<string>(), "OK"))
                     .Returns(Task.CompletedTask);

            // Act & Assert - Should not throw
            await service.ShowErrorAsync(message);
            
            _mockPage.Verify(p => p.DisplayAlert("Error", message, "OK"), Times.Once);
        }

        [Fact]
        public async Task ShowErrorAsync_ShouldHandleNullMessage()
        {
            // Arrange
            var service = CreateNavigationService();
            
            _mockPage.Setup(p => p.DisplayAlert("Error", It.IsAny<string>(), "OK"))
                     .Returns(Task.CompletedTask);

            // Act & Assert - Should not throw
            await service.ShowErrorAsync(null!);
            
            _mockPage.Verify(p => p.DisplayAlert("Error", It.IsAny<string>(), "OK"), Times.Once);
        }

        #endregion

        #region Error Handling Integration Tests

        [Fact]
        public async Task ErrorHandling_ShouldNotThrowException_WhenDisplayAlertFails()
        {
            // Arrange
            var service = CreateNavigationService();
            var navigationException = new InvalidOperationException("Navigation failed");
            var displayAlertException = new InvalidOperationException("Display alert failed");
            
            _mockNavigation.Setup(n => n.PopAsync()).ThrowsAsync(navigationException);
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ThrowsAsync(displayAlertException);

            // Act & Assert - Should not throw any exception
            await service.GoBackAsync();
            
            // Verify navigation was attempted
            _mockNavigation.Verify(n => n.PopAsync(), Times.Once);
            _mockPage.Verify(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ErrorHandling_ShouldHandleNestedExceptions()
        {
            // Arrange
            var service = CreateNavigationService();
            var innerException = new ArgumentException("Inner error");
            var outerException = new InvalidOperationException("Outer error", innerException);
            
            _mockNavigation.Setup(n => n.PopAsync()).ThrowsAsync(outerException);
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            // Act
            await service.GoBackAsync();

            // Assert
            _mockPage.Verify(p => p.DisplayAlert("Error", It.Is<string>(s => s.Contains("Failed to navigate back") && s.Contains("Outer error")), "OK"), Times.Once);
        }

        #endregion

        #region Interface Compliance Tests

        [Fact]
        public void NavigationService_ShouldImplementINavigationService()
        {
            // Arrange & Act
            var service = CreateNavigationService();

            // Assert
            Assert.IsAssignableFrom<INavigationService>(service);
        }

        [Fact]
        public async Task INavigationServiceMethods_ShouldBeCallableViaInterface()
        {
            // Arrange
            INavigationService service = CreateNavigationService();
            var targetPage = new Mock<Page>().Object;
            
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);
            _mockPage.Setup(p => p.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(true);

            // Act & Assert - All interface methods should be callable
            await service.GoBackAsync();
            await service.NavigateAsync(targetPage);
            await service.NavigateAsync(() => targetPage);
            await service.ShowAlertAsync("Title", "Message", "OK");
            var result = await service.ShowConfirmationAsync("Title", "Message", "Yes", "No");
            await service.ShowErrorAsync("Error message");

            // Verify calls were made
            _mockNavigation.Verify(n => n.PopAsync(), Times.Once);
            _mockNavigation.Verify(n => n.PushAsync(targetPage), Times.Exactly(2));
            _mockPage.Verify(p => p.DisplayAlert("Title", "Message", "OK"), Times.Once);
            _mockPage.Verify(p => p.DisplayAlert("Title", "Message", "Yes", "No"), Times.Once);
            _mockPage.Verify(p => p.DisplayAlert("Error", "Error message", "OK"), Times.Once);
            Assert.True(result);
        }

        #endregion
    }
}