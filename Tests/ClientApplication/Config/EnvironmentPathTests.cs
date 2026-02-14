using ClientApplication.Config;
using Xunit;
using FluentAssertions;

namespace Tests.ClientApplication.Config;

/// <summary>
/// EnvironmentPath class unit tests
///  path resolution and file existence checking functionality
/// </summary>
public class EnvironmentPathTests
{
    [Fact]
    public void ProjectRootDirectory_ShouldNotBeNull()
    {
        // Act
        var projectRoot = EnvironmentPath.ProjectRootDirectory;

        // Assert
        projectRoot.Should().NotBeNull();
        projectRoot.Should().NotBeEmpty();
    }

    [Fact]
    public void BannerPath_Property_ShouldReturnCorrectRelativePath()
    {
        // Act
        var bannerPath = EnvironmentPath.BannerPath;

        // Assert
        bannerPath.Should().Be("Resources/Banner.txt");
    }

    [Fact]
    public void GetBannerPath_ShouldReturnValidFullPath()
    {
        // Act
        var fullPath = EnvironmentPath.GetBannerPath();

        // Assert
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();
        fullPath.Should().Contain("Resources");
        fullPath.Should().EndWith("Banner.txt");
    }

    [Fact]
    public void IsBannerFileExists_ShouldReturnBoolean()
    {
        // Act
        var exists = EnvironmentPath.IsBannerFileExists();

        // Assert
        Assert.IsType<bool>(exists);
    }

    [Fact]
    public void MultipleCalls_ToProjectRootDirectory_ShouldReturnSameValue()
    {
        // Act
        var root1 = EnvironmentPath.ProjectRootDirectory;
        var root2 = EnvironmentPath.ProjectRootDirectory;

        // Assert
        root1.Should().Be(root2);
    }

    [Fact]
    public void GetBannerPath_MultipleCalls_ShouldReturnConsistentResults()
    {
        // Act
        var path1 = EnvironmentPath.GetBannerPath();
        var path2 = EnvironmentPath.GetBannerPath();

        // Assert
        path1.Should().Be(path2);
    }

    [Fact]
    public void EnvironmentPath_StaticConstructor_ShouldInitializeSuccessfully()
    {
        // This test verifies that the static constructor runs without throwing exceptions
        // Act & Assert
        Action action = () =>
        {
            var _ = EnvironmentPath.ProjectRootDirectory;
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void BannerFile_ExistenceCheck_ShouldBeDeterministic()
    {
        // Act
        var exists1 = EnvironmentPath.IsBannerFileExists();
        var exists2 = EnvironmentPath.IsBannerFileExists();

        // Assert
        exists1.Should().Be(exists2);
    }

    [Fact]
    public void BannerFile_ShouldActuallyExist()
    {
        // This test verifies that the Banner.txt file actually exists at the resolved path
        // Act
        var fullPath = EnvironmentPath.GetBannerPath();
        var exists = EnvironmentPath.IsBannerFileExists();

        // Assert
        exists.Should().BeTrue("Banner.txt file should exist at the resolved path");
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();
    }

    [Fact]
    public void ProjectRootDirectory_ShouldPointToValidDirectory()
    {
        // Act
        var projectRoot = EnvironmentPath.ProjectRootDirectory;

        // Assert
        projectRoot.Should().NotBeNull();
        projectRoot.Should().NotBeEmpty();

        // Verify it's actually a directory that exists
        var directoryInfo = new DirectoryInfo(projectRoot);
        directoryInfo.Exists.Should().BeTrue("Project root directory should exist");
    }

    [Fact]
    public void GetBannerPath_ShouldCreateValidFileSystemPath()
    {
        // Act
        var fullPath = EnvironmentPath.GetBannerPath();

        // Assert
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();

        // Verify path format is valid
        Action pathValidation = () => Path.GetFullPath(fullPath);
        pathValidation.Should().NotThrow("Path should be a valid file system path");
    }
}