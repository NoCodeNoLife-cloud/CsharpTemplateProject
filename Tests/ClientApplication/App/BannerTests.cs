using ClientApplication.App;
using Xunit;
using FluentAssertions;

namespace Tests.ClientApplication.App;

/// <summary>
/// Banner class unit tests
/// Tests banner path resolution and file existence checking functionality
/// </summary>
public class BannerTests
{
    [Fact]
    public void BannerPath_Constant_ShouldReturnCorrectRelativePath()
    {
        // Act
        var bannerPath = Banner.BannerPath;

        // Assert
        bannerPath.Should().Be("Resources/Banner.txt");
    }

    [Fact]
    public void GetBannerPath_ShouldReturnValidFullPath()
    {
        // Act
        var fullPath = Banner.GetBannerPath();

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
        var exists = Banner.IsBannerFileExists();

        // Assert
        Assert.IsType<bool>(exists);
    }

    [Fact]
    public void GetBannerPath_CalledMultipleTimes_ShouldReturnSameResult()
    {
        // Act
        var path1 = Banner.GetBannerPath();
        var path2 = Banner.GetBannerPath();

        // Assert
        path1.Should().Be(path2);
    }

    [Fact]
    public void IsBannerFileExists_CalledMultipleTimes_ShouldReturnSameResult()
    {
        // Act
        var exists1 = Banner.IsBannerFileExists();
        var exists2 = Banner.IsBannerFileExists();

        // Assert
        exists1.Should().Be(exists2);
    }

    [Fact]
    public void BannerFile_ShouldActuallyExist()
    {
        // This test verifies that the Banner.txt file actually exists at the resolved path
        // Act
        var fullPath = Banner.GetBannerPath();
        var exists = Banner.IsBannerFileExists();

        // Assert
        exists.Should().BeTrue("Banner.txt file should exist at the resolved path");
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();
    }

    [Fact]
    public void GetBannerPath_ShouldCreateValidFileSystemPath()
    {
        // Act
        var fullPath = Banner.GetBannerPath();

        // Assert
        fullPath.Should().NotBeNull();
        fullPath.Should().NotBeEmpty();

        // Verify path format is valid
        Action pathValidation = () => Path.GetFullPath(fullPath);
        pathValidation.Should().NotThrow("Path should be a valid file system path");
    }
}