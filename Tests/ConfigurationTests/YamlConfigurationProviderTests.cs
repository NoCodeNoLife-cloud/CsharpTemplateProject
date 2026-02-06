using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Providers;
using FluentAssertions;
using Xunit;

namespace Tests.ConfigurationTests;

public class YamlConfigurationProviderTests : IDisposable
{
    private readonly YamlConfigurationProvider _provider = new();
    private readonly List<string> _createdFiles = new();
    private readonly string _testFilePrefix = $"yaml-test-{Guid.NewGuid():N}-";

    [Fact]
    public void Name_Should_Return_YAML()
    {
        var name = _provider.Name;
        name.Should().Be("YAML");
    }

    [Theory]
    [InlineData("config.yaml")]
    [InlineData("settings.YAML")]
    [InlineData("data.yml")]
    [InlineData("config.Yml")]
    public void CanHandleSource_With_Yaml_File_Should_Return_True(string source)
    {
        var result = _provider.CanHandleSource(source);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("config.json")]
    [InlineData("settings.xml")]
    [InlineData("data.txt")]
    public void CanHandleSource_With_NonYaml_File_Should_Return_False(string source)
    {
        var result = _provider.CanHandleSource(source);
        result.Should().BeFalse();
    }

    [Fact]
    public void LoadConfiguration_With_Valid_Yaml_Should_Return_Correct_Data()
    {
        const string yamlContent = @"
AppName: TestApp
Version: 1.0.0
Port: 8080
Debug: true
";
        var fileName = $"{_testFilePrefix}{Path.GetRandomFileName()}.yaml";
        File.WriteAllText(fileName, yamlContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("AppName");
        result["AppName"].Should().Be("TestApp");
        result.Should().ContainKey("Port");
        result["Port"].Should().Be("8080");
        result.Should().ContainKey("Debug");
        result["Debug"].Should().Be("true");
    }

    [Fact]
    public void LoadConfiguration_With_Nested_Yaml_Should_Flatten_Correctly()
    {
        const string yamlContent = @"
Database:
  ConnectionString: Server=localhost;Database=testdb
  Timeout: 30
";
        var fileName = $"{_testFilePrefix}{Path.GetRandomFileName()}.yaml";
        File.WriteAllText(fileName, yamlContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("Database.ConnectionString");
        result["Database.ConnectionString"].Should().Be("Server=localhost;Database=testdb");
        result.Should().ContainKey("Database.Timeout");
        result["Database.Timeout"].Should().Be("30");
    }

    [Fact]
    public void LoadConfiguration_With_Yaml_Array_Should_Handle_Correctly()
    {
        const string yamlContent = @"
Servers:
  - server1.local
  - server2.local
  - server3.local
";
        var fileName = $"{_testFilePrefix}{Path.GetRandomFileName()}.yaml";
        File.WriteAllText(fileName, yamlContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("Servers[0]");
        result.Should().ContainKey("Servers[1]");
        result.Should().ContainKey("Servers[2]");
    }

    [Fact]
    public void LoadConfiguration_With_NonExistent_File_Should_Throw_ConfigurationFileNotFoundException()
    {
        const string nonExistentFile = "non-existent.yaml";
        Action act = () => _provider.LoadConfiguration(nonExistentFile);
        act.Should().Throw<ConfigurationFileNotFoundException>().WithMessage($"*{nonExistentFile}*");
    }

    public void Dispose()
    {
        Console.WriteLine($"Starting to clean up YAML Provider test files...");
        var deletedCount = 0;
        var failedCount = 0;

        foreach (var file in _createdFiles.ToList())
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    _createdFiles.Remove(file);
                    deletedCount++;
                    Console.WriteLine($"Deleted YAML test file: {file}");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    Console.WriteLine($"Warning: Unable to delete YAML test file {file}: {ex.Message}");
                }
            }
            else
            {
                _createdFiles.Remove(file);
            }
        }

        Console.WriteLine($"YAML Provider cleanup completed: Deleted {deletedCount} files, failed {failedCount}");
    }
}