using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Microsoft.Maui.Controls;

namespace WorkMood.MauiApp.Tests.Services;

public class SimpleNavigationTest
{
    [Fact]
    public void NavigationService_Constructor_WithShims_ShouldWork()
    {
        // Arrange
        var navShim = new Mock<IPageNavigationShim>().Object;
        var dialogShim = new Mock<IPageDialogShim>().Object;

        // Act & Assert
        var service = new NavigationService(navShim, dialogShim);
        Assert.NotNull(service);
    }

    [Fact]
    public void NavigationService_Constructor_WithPage_ShouldWork()
    {
        // Arrange
        var page = new ContentPage();

        // Act & Assert
        var service = new NavigationService(page);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task NavigationService_GoBackAsync_ShouldCallShim()
    {
        // Arrange
        var mockNavShim = new Mock<IPageNavigationShim>();
        var mockDialogShim = new Mock<IPageDialogShim>();
        mockNavShim.Setup(x => x.PopAsync()).Returns(Task.FromResult(new ContentPage()));
        
        var service = new NavigationService(mockNavShim.Object, mockDialogShim.Object);

        // Act
        await service.GoBackAsync();

        // Assert
        mockNavShim.Verify(x => x.PopAsync(), Times.Once);
    }
}
