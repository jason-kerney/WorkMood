using FluentAssertions;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class DataArchiveServiceShould
{
    private readonly Mock<IFolderShim> _mockFolderShim;
    private readonly Mock<IDateShim> _mockDateShim;
    private readonly Mock<IFileShim> _mockFileShim;
    private readonly Mock<IJsonSerializerShim> _mockJsonSerializerShim;
    private readonly DataArchiveService _sut;

    public DataArchiveServiceShould()
    {
        _mockFolderShim = new Mock<IFolderShim>();
        _mockDateShim = new Mock<IDateShim>();
        _mockFileShim = new Mock<IFileShim>();
        _mockJsonSerializerShim = new Mock<IJsonSerializerShim>();

        // Setup default behavior for constructor
        _mockFolderShim.Setup(x => x.GetArchiveFolder()).Returns(@"C:\Archive");
        _mockFolderShim.Setup(x => x.CreateDirectory(It.IsAny<string>()));

        _sut = new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ThrowArgumentNullException_WhenFolderShimIsNull()
    {
        // Act & Assert
        // This is what we WANT the constructor to do - proper parameter validation
        var act = () => new DataArchiveService(
            null!, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("folderShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenDateShimIsNull()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            null!, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("dateShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenFileShimIsNull()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            null!, 
            _mockJsonSerializerShim.Object);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("fileShim");
    }

    [Fact]
    public void ThrowArgumentNullException_WhenJsonSerializerShimIsNull()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            null!);
        
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("jsonSerializerShim");
    }

    [Fact]
    public void SuccessfullyCreateService_WhenAllParametersAreValid()
    {
        // Act & Assert
        var act = () => new DataArchiveService(
            _mockFolderShim.Object, 
            _mockDateShim.Object, 
            _mockFileShim.Object, 
            _mockJsonSerializerShim.Object);
        
        act.Should().NotThrow();
    }

    #endregion
}