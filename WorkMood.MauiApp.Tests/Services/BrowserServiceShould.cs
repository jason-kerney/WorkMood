using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class BrowserServiceShould
{
    private readonly Mock<IBrowserShim> _mockBrowserShim;
    private readonly BrowserService _sut;

    public BrowserServiceShould()
    {
        _mockBrowserShim = new Mock<IBrowserShim>();
        _sut = new BrowserService(_mockBrowserShim.Object);
    }

    [Fact]
    public async Task ReturnTrue_WhenUrlIsValidAndBrowserShimSucceeds()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, It.IsAny<BrowserLaunchOptions?>()))
                        .ReturnsAsync(true);

        // Act
        var result = await _sut.OpenAsync(validUrl);

        // Assert
        result.Should().BeTrue();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, null), Times.Once);
    }

    [Fact]
    public async Task ReturnFalse_WhenUrlIsValidButBrowserShimFails()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, It.IsAny<BrowserLaunchOptions?>()))
                        .ReturnsAsync(false);

        // Act
        var result = await _sut.OpenAsync(validUrl);

        // Assert
        result.Should().BeFalse();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, null), Times.Once);
    }

    [Fact]
    public async Task PassOptionsCorrectly_WhenOptionsAreProvided()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        var options = new BrowserLaunchOptions
        {
            LaunchMode = BrowserLaunchMode.External
        };
        
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, options))
                        .ReturnsAsync(true);

        // Act
        var result = await _sut.OpenAsync(validUrl, options);

        // Assert
        result.Should().BeTrue();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, options), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task ReturnFalse_WhenUrlIsNullOrWhitespace(string? invalidUrl)
    {
        // Act
        var result = await _sut.OpenAsync(invalidUrl!);

        // Assert
        result.Should().BeFalse();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(It.IsAny<string>(), It.IsAny<BrowserLaunchOptions?>()), Times.Never);
    }

    [Fact]
    public async Task ReturnFalse_WhenBrowserShimThrowsUriFormatException()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, It.IsAny<BrowserLaunchOptions?>()))
                        .ThrowsAsync(new UriFormatException("Invalid URI format"));

        // Act
        var result = await _sut.OpenAsync(validUrl);

        // Assert
        result.Should().BeFalse();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, null), Times.Once);
    }

    [Fact]
    public async Task UseSystemPreferredLaunchMode_WhenOptionsIsNull()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, null))
                        .ReturnsAsync(true);

        // Act
        var result = await _sut.OpenAsync(validUrl, null);

        // Assert
        result.Should().BeTrue();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, null), Times.Once);
    }

    [Fact]
    public async Task UseSystemPreferredLaunchMode_WhenOptionsLaunchModeIsNotSet()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        var optionsWithoutLaunchMode = new BrowserLaunchOptions();
        
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, optionsWithoutLaunchMode))
                        .ReturnsAsync(true);

        // Act
        var result = await _sut.OpenAsync(validUrl, optionsWithoutLaunchMode);

        // Assert
        result.Should().BeTrue();
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, optionsWithoutLaunchMode), Times.Once);
    }

    [Fact]
    public async Task NotCatchNonUriFormatExceptions()
    {
        // Arrange
        const string validUrl = "https://www.example.com";
        var expectedException = new InvalidOperationException("Some other exception");
        
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(validUrl, It.IsAny<BrowserLaunchOptions?>()))
                        .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.OpenAsync(validUrl));
        exception.Should().BeSameAs(expectedException);
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(validUrl, null), Times.Once);
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://github.com/microsoft/maui")]
    public async Task CallBrowserShimWithCorrectUrl_ForVariousValidUrls(string url)
    {
        // Arrange
        _mockBrowserShim.Setup(x => x.OpenDefaultAsync(url, It.IsAny<BrowserLaunchOptions?>()))
                        .ReturnsAsync(true);

        // Act
        await _sut.OpenAsync(url);

        // Assert
        _mockBrowserShim.Verify(x => x.OpenDefaultAsync(url, null), Times.Once);
    }
}
