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
    public void MultipleCalls_ToProjectRootDirectory_ShouldReturnSameValue()
    {
        // Act
        var root1 = EnvironmentPath.ProjectRootDirectory;
        var root2 = EnvironmentPath.ProjectRootDirectory;

        // Assert
        root1.Should().Be(root2);
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


}