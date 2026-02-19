using ClientApplication.App.Banner;
using Xunit;
using FluentAssertions;

namespace Tests.ClientApplication.App;

/// <summary>
/// Banner class unit tests
///  banner path resolution and file existence checking functionality
/// </summary>
public class BannerManagerTests
{
    [Fact]
    public void BannerPath_Constant_ShouldReturnCorrectRelativePath()
    {
        // Act
        const string bannerPath = BannerManager.BannerPath;

        // Assert
        bannerPath.Should().Be("App/Banner/Banner.txt");
    }

    [Fact]
    public void GetBannerPath_ShouldReturnValidFullPath()
    {
        // Act
        var fullPath = BannerManager.GetBannerPath();

        // Assert
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();
        fullPath.Should().Contain("App");
        fullPath.Should().Contain("Banner");
        fullPath.Should().EndWith("Banner.txt");
    }

    [Fact]
    public void IsBannerFileExists_ShouldReturnBoolean()
    {
        // Act
        var exists = BannerManager.IsBannerFileExists();

        // Assert
        Assert.IsType<bool>(exists);
    }

    [Fact]
    public void GetBannerPath_CalledMultipleTimes_ShouldReturnSameResult()
    {
        // Act
        var path1 = BannerManager.GetBannerPath();
        var path2 = BannerManager.GetBannerPath();

        // Assert
        path1.Should().Be(path2);
    }

    [Fact]
    public void IsBannerFileExists_CalledMultipleTimes_ShouldReturnSameResult()
    {
        // Act
        var exists1 = BannerManager.IsBannerFileExists();
        var exists2 = BannerManager.IsBannerFileExists();

        // Assert
        exists1.Should().Be(exists2);
    }

    [Fact]
    public void BannerFile_ShouldActuallyExist()
    {
        // This test verifies that the Banner.txt file actually exists at the resolved path
        // Act
        var fullPath = BannerManager.GetBannerPath();
        var exists = BannerManager.IsBannerFileExists();

        // Assert
        exists.Should().BeTrue("Banner.txt file should exist at the resolved path");
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();
    }

    [Fact]
    public void GetBannerPath_ShouldCreateValidFileSystemPath()
    {
        // Act
        var fullPath = BannerManager.GetBannerPath();

        // Assert
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();

        // Verify path format is valid
        Action pathValidation = () => Path.GetFullPath(fullPath);
        pathValidation.Should().NotThrow("Path should be a valid file system path");
    }
}