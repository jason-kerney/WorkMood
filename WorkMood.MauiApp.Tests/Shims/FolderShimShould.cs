using WorkMood.MauiApp.Shims;
using Xunit;

namespace WorkMood.MauiApp.Tests.Shims;

/// <summary>
/// Tests for FolderShim - cross-platform folder path operations
/// Location: MauiApp/Shims/FolderShim.cs
/// Purpose: Verify platform-specific folder paths are correct for Windows and macOS
/// </summary>
public class FolderShimShould
{
    #region Path Combination Tests

    [Fact]
    public void CombinePaths_WithMultiplePaths_ShouldCombineCorrectly()
    {
        // Arrange
        var shim = new FolderShim();
        var path1 = "C:\\Users\\Test";
        var path2 = "Documents";
        var path3 = "WorkMood";

        // Act
        var result = shim.CombinePaths(path1, path2, path3);

        // Assert
        var expected = Path.Combine(path1, path2, path3);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CombinePaths_WithSinglePath_ShouldReturnPath()
    {
        // Arrange
        var shim = new FolderShim();
        var path = "C:\\Users\\Test";

        // Act
        var result = shim.CombinePaths(path);

        // Assert
        Assert.Equal(path, result);
    }

    #endregion

    #region Folder Path Tests

    [Fact]
    public void GetApplicationFolder_ShouldReturnWorkMoodInLocalApplicationData()
    {
        // Arrange
        var shim = new FolderShim();
        var expectedBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expected = Path.Combine(expectedBase, "WorkMood");

        // Act
        var result = shim.GetApplicationFolder();

        // Assert
        Assert.Equal(expected, result);
        Assert.Contains("WorkMood", result);
    }

    [Fact]
    public void GetArchiveFolder_ShouldReturnArchivesSubfolderOfApplicationFolder()
    {
        // Arrange
        var shim = new FolderShim();
        var applicationFolder = shim.GetApplicationFolder();
        var expected = Path.Combine(applicationFolder, "archives");

        // Act
        var result = shim.GetArchiveFolder();

        // Assert
        Assert.Equal(expected, result);
        Assert.Contains("WorkMood", result);
        Assert.Contains("archives", result);
    }

    [Fact]
    public void GetDesktopFolder_ShouldReturnValidDesktopPath()
    {
        // Arrange
        var shim = new FolderShim();
        var expected = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Act
        var result = shim.GetDesktopFolder();

        // Assert
        Assert.Equal(expected, result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetDocumentsFolder_ShouldReturnWorkMoodInMyDocuments()
    {
        // Arrange
        var shim = new FolderShim();
        var expectedBase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var expected = Path.Combine(expectedBase, "WorkMood");

        // Act
        var result = shim.GetDocumentsFolder();

        // Assert
        Assert.Equal(expected, result);
        Assert.Contains("WorkMood", result);
    }

    [Fact]
    public void GetDocumentsFolder_ShouldContainMyDocumentsPath()
    {
        // Arrange
        var shim = new FolderShim();
        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Act
        var result = shim.GetDocumentsFolder();

        // Assert
        Assert.StartsWith(myDocuments, result);
    }

    #endregion

    #region Directory Operations Tests

    [Fact]
    public void CreateDirectory_WithValidPath_ShouldCreateDirectory()
    {
        // Arrange
        var shim = new FolderShim();
        var testPath = Path.Combine(Path.GetTempPath(), $"WorkMoodTest_{Guid.NewGuid()}");

        try
        {
            // Act
            shim.CreateDirectory(testPath);

            // Assert
            Assert.True(Directory.Exists(testPath));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testPath))
                Directory.Delete(testPath);
        }
    }

    [Fact]
    public void DirectoryExists_WithExistingDirectory_ShouldReturnTrue()
    {
        // Arrange
        var shim = new FolderShim();
        var tempPath = Path.GetTempPath();

        // Act
        var result = shim.DirectoryExists(tempPath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DirectoryExists_WithNonExistentDirectory_ShouldReturnFalse()
    {
        // Arrange
        var shim = new FolderShim();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");

        // Act
        var result = shim.DirectoryExists(nonExistentPath);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region File Operations Tests

    [Fact]
    public void GetFileName_WithFullPath_ShouldReturnFileName()
    {
        // Arrange
        var shim = new FolderShim();
        var fullPath = @"C:\Users\Test\Documents\myfile.txt";
        var expected = "myfile.txt";

        // Act
        var result = shim.GetFileName(fullPath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFiles_WithValidPath_ShouldReturnArray()
    {
        // Arrange
        var shim = new FolderShim();
        var testPath = Path.Combine(Path.GetTempPath(), $"WorkMoodTest_{Guid.NewGuid()}");
        
        try
        {
            shim.CreateDirectory(testPath);
            File.WriteAllText(Path.Combine(testPath, "test.txt"), "test");

            // Act
            var result = shim.GetFiles(testPath, "*.txt");

            // Assert
            Assert.NotEmpty(result);
            Assert.Single(result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testPath))
                Directory.Delete(testPath, true);
        }
    }

    #endregion

    #region Path Consistency Tests

    [Fact]
    public void ArchiveFolder_ShouldAlwaysBeSubfolderOfApplicationFolder()
    {
        // Arrange
        var shim = new FolderShim();

        // Act
        var applicationFolder = shim.GetApplicationFolder();
        var archiveFolder = shim.GetArchiveFolder();

        // Assert
        Assert.True(archiveFolder.StartsWith(applicationFolder), 
            $"Archive folder '{archiveFolder}' should be under application folder '{applicationFolder}'");
    }

    [Fact]
    public void ApplicationFolder_And_DocumentsFolder_ShouldBothContainWorkMood()
    {
        // Arrange
        var shim = new FolderShim();

        // Act
        var appFolder = shim.GetApplicationFolder();
        var docsFolder = shim.GetDocumentsFolder();

        // Assert
        Assert.Contains("WorkMood", appFolder);
        Assert.Contains("WorkMood", docsFolder);
    }

    #endregion
}
