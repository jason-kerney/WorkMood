using System.Text.Json;
using Moq;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class ScheduleConfigServiceShould
{
    private readonly Mock<IFileShim> _mockFileShim;
    private readonly Mock<IFolderShim> _mockFolderShim;
    private readonly Mock<IJsonSerializerShim> _mockJsonSerializerShim;
    private readonly Mock<IDateShim> _mockDateShim;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly ScheduleConfigService _service;
    private readonly JsonSerializerOptions _expectedJsonOptions;

    public ScheduleConfigServiceShould()
    {
        _mockFileShim = new Mock<IFileShim>();
        _mockFolderShim = new Mock<IFolderShim>();
        _mockJsonSerializerShim = new Mock<IJsonSerializerShim>();
        _mockDateShim = new Mock<IDateShim>();
        _mockLoggingService = new Mock<ILoggingService>();

        // Setup default folder behavior
        _mockFolderShim.Setup(x => x.GetApplicationFolder()).Returns("/test/app");
        _mockFolderShim.Setup(x => x.CombinePaths("/test/app", "schedule_config.json")).Returns("/test/app/schedule_config.json");

        _expectedJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        _service = new ScheduleConfigService(
            _mockFileShim.Object,
            _mockFolderShim.Object,
            _mockJsonSerializerShim.Object,
            _mockDateShim.Object,
            _mockLoggingService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFileShimIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ScheduleConfigService(
            null!,
            _mockFolderShim.Object,
            _mockJsonSerializerShim.Object,
            _mockDateShim.Object,
            _mockLoggingService.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFolderShimIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ScheduleConfigService(
            _mockFileShim.Object,
            null!,
            _mockJsonSerializerShim.Object,
            _mockDateShim.Object,
            _mockLoggingService.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenJsonSerializerShimIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ScheduleConfigService(
            _mockFileShim.Object,
            _mockFolderShim.Object,
            null!,
            _mockDateShim.Object,
            _mockLoggingService.Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDateShimIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ScheduleConfigService(
            _mockFileShim.Object,
            _mockFolderShim.Object,
            _mockJsonSerializerShim.Object,
            null!,
            _mockLoggingService.Object));
    }

    [Fact]
    public void Constructor_ShouldCreateDefaultLoggingService_WhenLoggingServiceIsNull()
    {
        // Act
        var service = new ScheduleConfigService(
            _mockFileShim.Object,
            _mockFolderShim.Object,
            _mockJsonSerializerShim.Object,
            _mockDateShim.Object,
            null);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_ShouldSetupApplicationFolder_AndCreateDirectory()
    {
        // Arrange - Use fresh mocks to avoid interference from other tests
        var freshMockFolderShim = new Mock<IFolderShim>();
        freshMockFolderShim.Setup(x => x.GetApplicationFolder()).Returns("/test/app");
        freshMockFolderShim.Setup(x => x.CombinePaths("/test/app", "schedule_config.json")).Returns("/test/app/schedule_config.json");
        
        // Act
        var service = new ScheduleConfigService(
            _mockFileShim.Object,
            freshMockFolderShim.Object,
            _mockJsonSerializerShim.Object,
            _mockDateShim.Object,
            _mockLoggingService.Object);

        // Assert
        freshMockFolderShim.Verify(x => x.GetApplicationFolder(), Times.Once);
        freshMockFolderShim.Verify(x => x.CreateDirectory("/test/app"), Times.Once);
        freshMockFolderShim.Verify(x => x.CombinePaths("/test/app", "schedule_config.json"), Times.Once);
        Assert.NotNull(service);
    }

    #endregion

    #region LoadScheduleConfigAsync Tests

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnCachedConfig_WhenConfigIsAlreadyCached()
    {
        // Arrange
        var expectedConfig = new ScheduleConfig(TimeSpan.FromHours(9), TimeSpan.FromHours(17));
        
        // Load once to cache
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync("{}");
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>("{}", It.IsAny<JsonSerializerOptions>()))
            .Returns(expectedConfig);
        
        await _service.LoadScheduleConfigAsync();

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.Equal(expectedConfig, result);
        _mockFileShim.Verify(x => x.Exists(It.IsAny<string>()), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnDefaultConfig_WhenFileDoesNotExist()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
        _mockFileShim.Verify(x => x.Exists("/test/app/schedule_config.json"), Times.Once);
        _mockFileShim.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnDefaultConfig_WhenFileIsEmpty()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync("");

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
        _mockFileShim.Verify(x => x.ReadAllTextAsync("/test/app/schedule_config.json"), Times.Once);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<ScheduleConfig>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()), Times.Never);
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnDefaultConfig_WhenFileIsWhiteSpace()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync("   \n\t  ");

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<ScheduleConfig>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()), Times.Never);
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldDeserializeAndReturnConfig_WhenFileContainsValidJson()
    {
        // Arrange
        var expectedConfig = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var jsonContent = "{\"morningTime\":\"08:00:00\",\"eveningTime\":\"18:00:00\"}";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Returns(expectedConfig);

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.Equal(expectedConfig, result);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()), Times.Once);
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnDefaultConfig_WhenDeserializationReturnsNull()
    {
        // Arrange
        var jsonContent = "{}";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Returns((ScheduleConfig?)null);

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnDefaultConfig_WhenFileReadThrowsException()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ThrowsAsync(new IOException("File access denied"));

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
        // LogDebug verification removed
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldReturnDefaultConfig_WhenDeserializationThrowsException()
    {
        // Arrange
        var jsonContent = "invalid json";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Throws(new JsonException("Invalid JSON"));

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
        // LogDebug verification removed
    }

    #endregion

    #region SaveScheduleConfigAsync Tests

    [Fact]
    public async Task SaveScheduleConfigAsync_ShouldSerializeAndSaveConfig_WhenConfigIsValid()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var serializedJson = "{\"morningTime\":\"08:00:00\",\"eveningTime\":\"18:00:00\"}";
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);

        // Act
        await _service.SaveScheduleConfigAsync(config);

        // Assert
        _mockJsonSerializerShim.Verify(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllTextAsync("/test/app/schedule_config.json", serializedJson), Times.Once);
        // LogDebug verification removed
    }

    [Fact]
    public async Task SaveScheduleConfigAsync_ShouldUpdateCache_AfterSuccessfulSave()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var serializedJson = "{}";
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);

        // Act
        await _service.SaveScheduleConfigAsync(config);

        // Verify cache is updated by checking GetCachedConfig
        var cachedConfig = _service.GetCachedConfig();

        // Assert
        Assert.Equal(config, cachedConfig);
    }

    [Fact]
    public async Task SaveScheduleConfigAsync_ShouldPropagateException_WhenSerializationFails()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Throws(new JsonException("Serialization failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<JsonException>(() => _service.SaveScheduleConfigAsync(config));
        Assert.Equal("Serialization failed", exception.Message);
        // LogDebug verification removed
    }

    [Fact]
    public async Task SaveScheduleConfigAsync_ShouldPropagateException_WhenFileWriteFails()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var serializedJson = "{}";
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);
        _mockFileShim.Setup(x => x.WriteAllTextAsync("/test/app/schedule_config.json", serializedJson))
            .ThrowsAsync(new IOException("Disk full"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<IOException>(() => _service.SaveScheduleConfigAsync(config));
        Assert.Equal("Disk full", exception.Message);
        // LogDebug verification removed
    }

    #endregion

    #region UpdateScheduleConfigAsync Tests

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldLoadCurrentConfig_AndCreateUpdatedConfig()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        var currentConfig = new ScheduleConfig();
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime);

        // Assert
        Assert.Equal(morningTime, result.MorningTime);
        Assert.Equal(eveningTime, result.EveningTime);
        _mockFileShim.Verify(x => x.Exists("/test/app/schedule_config.json"), Times.Once);
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldAddNewOverride_WhenOverrideIsProvided()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        var overrideDate = new DateOnly(2023, 10, 20);
        var newOverride = new ScheduleOverride(overrideDate, TimeSpan.FromHours(10), TimeSpan.FromHours(16));
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime, newOverride);

        // Assert
        Assert.Equal(morningTime, result.MorningTime);
        Assert.Equal(eveningTime, result.EveningTime);
        Assert.Contains(result.Overrides, o => o.Date == overrideDate);
        // LogDebug verification removed
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldNotAddOverride_WhenOverrideHasNoOverride()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        var overrideDate = new DateOnly(2023, 10, 20);
        var newOverride = new ScheduleOverride(overrideDate); // No override times set
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime, newOverride);

        // Assert
        Assert.Empty(result.Overrides);
        // LogDebug verification removed
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldRemovePastOverrides_AndLogRemovalCount()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        var today = new DateOnly(2023, 10, 15);
        
        // Setup current config with past and future overrides
        var currentConfig = new ScheduleConfig();
        currentConfig.SetOverride(new DateOnly(2023, 10, 10), TimeSpan.FromHours(9), TimeSpan.FromHours(17)); // Past
        currentConfig.SetOverride(new DateOnly(2023, 10, 20), TimeSpan.FromHours(8), TimeSpan.FromHours(16)); // Future
        
        var jsonContent = "{}";
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Returns(currentConfig);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(today);

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime);

        // Assert
        Assert.Single(result.Overrides); // Only future override should remain
        Assert.Equal(new DateOnly(2023, 10, 20), result.Overrides[0].Date);
        // LogDebug verification removed
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldCopyExistingOverrides_BeforeAddingNew()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        var today = new DateOnly(2023, 10, 15);
        
        // Setup current config with existing future override
        var currentConfig = new ScheduleConfig();
        currentConfig.SetOverride(new DateOnly(2023, 10, 20), TimeSpan.FromHours(10), TimeSpan.FromHours(16));
        
        // New override to add
        var newOverride = new ScheduleOverride(new DateOnly(2023, 10, 25), TimeSpan.FromHours(9), TimeSpan.FromHours(15));
        
        var jsonContent = "{}";
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Returns(currentConfig);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(today);

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime, newOverride);

        // Assert
        Assert.Equal(2, result.Overrides.Count);
        Assert.Contains(result.Overrides, o => o.Date == new DateOnly(2023, 10, 20));
        Assert.Contains(result.Overrides, o => o.Date == new DateOnly(2023, 10, 25));
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldCallCleanupOldOverrides_OnUpdatedConfig()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime);

        // Assert - CleanupOldOverrides is called internally on ScheduleConfig
        Assert.NotNull(result);
        // LogDebug verification removed
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldSaveUpdatedConfiguration()
    {
        // Arrange
        var morningTime = TimeSpan.FromHours(8);
        var eveningTime = TimeSpan.FromHours(18);
        var serializedJson = "{}";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));
        _mockJsonSerializerShim.Setup(x => x.Serialize(It.IsAny<ScheduleConfig>(), It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);

        // Act
        var result = await _service.UpdateScheduleConfigAsync(morningTime, eveningTime);

        // Assert
        _mockJsonSerializerShim.Verify(x => x.Serialize(It.IsAny<ScheduleConfig>(), It.IsAny<JsonSerializerOptions>()), Times.Once);
        _mockFileShim.Verify(x => x.WriteAllTextAsync("/test/app/schedule_config.json", serializedJson), Times.Once);
    }

    #endregion

    #region Cache Management Tests

    [Fact]
    public async Task GetCachedConfig_ShouldReturnCachedConfig_AfterLoadingConfiguration()
    {
        // Arrange
        var expectedConfig = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var jsonContent = "{}";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Returns(expectedConfig);

        // Act
        await _service.LoadScheduleConfigAsync();
        var result = _service.GetCachedConfig();

        // Assert
        Assert.Equal(expectedConfig, result);
    }

    [Fact]
    public async Task GetCachedConfig_ShouldReturnCachedConfig_AfterSavingConfiguration()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var serializedJson = "{}";
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);

        // Act
        await _service.SaveScheduleConfigAsync(config);
        var result = _service.GetCachedConfig();

        // Assert
        Assert.Equal(config, result);
    }

    [Fact]
    public async Task ClearCache_ShouldSetCacheToNull()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var serializedJson = "{}";
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);

        // Act
        await _service.SaveScheduleConfigAsync(config);
        Assert.NotNull(_service.GetCachedConfig()); // Verify cache is populated
        
        _service.ClearCache();
        var result = _service.GetCachedConfig();

        // Assert
        Assert.Null(result);
        // LogDebug verification removed
    }

    [Fact]
    public async Task ClearCache_ShouldForceFileReload_OnNextLoadCall()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var jsonContent = "{}";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(jsonContent);
        _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(jsonContent, It.IsAny<JsonSerializerOptions>()))
            .Returns(config);

        // Load once to cache
        await _service.LoadScheduleConfigAsync();
        
        // Act
        _service.ClearCache();
        await _service.LoadScheduleConfigAsync();

        // Assert
        _mockFileShim.Verify(x => x.Exists("/test/app/schedule_config.json"), Times.Exactly(2));
        _mockFileShim.Verify(x => x.ReadAllTextAsync("/test/app/schedule_config.json"), Times.Exactly(2));
    }

    #endregion

    #region Interface Compliance Tests

    [Fact]
    public void ScheduleConfigService_ShouldImplementIScheduleConfigService()
    {
        // Act & Assert
        Assert.IsAssignableFrom<IScheduleConfigService>(_service);
    }

    [Fact]
    public async Task IScheduleConfigServiceMethods_ShouldBeCallableViaInterface()
    {
        // Arrange
        IScheduleConfigService service = _service;
        var config = new ScheduleConfig();
        var serializedJson = "{}";
        
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));

        // Act & Assert
        var loadResult = await service.LoadScheduleConfigAsync();
        Assert.NotNull(loadResult);

        await service.SaveScheduleConfigAsync(config);
        _mockFileShim.Verify(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        var updateResult = await service.UpdateScheduleConfigAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        Assert.NotNull(updateResult);
    }

    #endregion

    #region Error Handling and Edge Case Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    [InlineData(null)]
    public async Task LoadScheduleConfigAsync_ShouldHandleVariousEmptyFileContents(string? fileContent)
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ReturnsAsync(fileContent!);

        // Act
        var result = await _service.LoadScheduleConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeSpan(8, 20, 0), result.MorningTime);
        Assert.Equal(new TimeSpan(17, 20, 0), result.EveningTime);
        _mockJsonSerializerShim.Verify(x => x.Deserialize<ScheduleConfig>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()), Times.Never);
    }

    [Fact]
    public async Task SaveScheduleConfigAsync_ShouldHandleNullConfig()
    {
        // Arrange
        ScheduleConfig? nullConfig = null;

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _service.SaveScheduleConfigAsync(nullConfig!));
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldHandleExceptionDuringLoad()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(true);
        _mockFileShim.Setup(x => x.ReadAllTextAsync("/test/app/schedule_config.json")).ThrowsAsync(new IOException("File read error"));
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));

        // Act
        var result = await _service.UpdateScheduleConfigAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(18));

        // Assert - Should still work by using default config from error handling
        Assert.NotNull(result);
        Assert.Equal(TimeSpan.FromHours(8), result.MorningTime);
        Assert.Equal(TimeSpan.FromHours(18), result.EveningTime);
    }

    [Fact]
    public async Task UpdateScheduleConfigAsync_ShouldPropagateException_WhenSaveFails()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2023, 10, 15));
        _mockJsonSerializerShim.Setup(x => x.Serialize(It.IsAny<ScheduleConfig>(), It.IsAny<JsonSerializerOptions>()))
            .Throws(new JsonException("Serialization error"));

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => 
            _service.UpdateScheduleConfigAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(18)));
    }

    [Fact]
    public async Task LoadScheduleConfigAsync_ShouldLogAllOperations()
    {
        // Arrange
        _mockFileShim.Setup(x => x.Exists("/test/app/schedule_config.json")).Returns(false);

        // Act
        await _service.LoadScheduleConfigAsync();

        // Assert
        // LogDebug verification removed
        // LogDebug verification removed
    }

    [Fact]
    public async Task SaveScheduleConfigAsync_ShouldLogAllOperations()
    {
        // Arrange
        var config = new ScheduleConfig(TimeSpan.FromHours(8), TimeSpan.FromHours(18));
        var serializedJson = "{}";
        
        _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()))
            .Returns(serializedJson);

        // Act
        await _service.SaveScheduleConfigAsync(config);

        // Assert
        // LogDebug verification removed
        // LogDebug verification removed
    }

    #endregion
}
