using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Providers;
using FluentAssertions;
using Xunit;

namespace Tests.ConfigurationTests;

public class JsonConfigurationProviderTests : IDisposable
{
    private readonly JsonConfigurationProvider _provider = new();
    private readonly List<string> _createdFiles = new();
    private readonly string _testFilePrefix = $"json-test-{Guid.NewGuid():N}-";

    [Fact]
    public void Name_Should_Return_JSON()
    {
        var name = _provider.Name;
        name.Should().Be("JSON");
    }

    [Theory]
    [InlineData("config.json")]
    [InlineData("settings.JSON")]
    [InlineData("data.Json")]
    public void CanHandleSource_With_Json_File_Should_Return_True(string source)
    {
        var result = _provider.CanHandleSource(source);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("config.xml")]
    [InlineData("settings.yaml")]
    [InlineData("data.txt")]
    public void CanHandleSource_With_NonJson_File_Should_Return_False(string source)
    {
        var result = _provider.CanHandleSource(source);
        result.Should().BeFalse();
    }

    [Fact]
    public void LoadConfiguration_With_Valid_Json_Should_Return_Correct_Data()
    {
        const string jsonContent = @"{
            ""AppName"": ""TestApp"",
            ""Version"": ""1.0.0"",
            ""Port"": 8080,
            ""Debug"": true
        }";
        var fileName = $"{_testFilePrefix}{Path.GetRandomFileName()}.json";
        File.WriteAllText(fileName, jsonContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("AppName");
        result["AppName"].Should().Be("TestApp");
        result.Should().ContainKey("Port");
        result["Port"].Should().Be(8080);
        result.Should().ContainKey("Debug");
        result["Debug"].Should().Be(true);
        result.Should().ContainKey("Version");
        result["Version"].Should().Be("1.0.0");
    }

    [Fact]
    public void LoadConfiguration_With_Nested_Json_Should_Flatten_Correctly()
    {
        const string jsonContent = @"{
            ""Database"": {
                ""ConnectionString"": ""Server=localhost;Database=testdb"",
                ""Timeout"": 30
            }
        }";
        var fileName = $"{_testFilePrefix}{Path.GetRandomFileName()}.json";
        File.WriteAllText(fileName, jsonContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("Database.ConnectionString");
        result["Database.ConnectionString"].Should().Be("Server=localhost;Database=testdb");
        result.Should().ContainKey("Database.Timeout");
        result["Database.Timeout"].Should().Be(30);
    }

    [Fact]
    public void LoadConfiguration_With_NonExistent_File_Should_Throw_ConfigurationFileNotFoundException()
    {
        const string nonExistentFile = "non-existent.json";
        Action act = () => _provider.LoadConfiguration(nonExistentFile);
        act.Should().Throw<ConfigurationFileNotFoundException>().WithMessage($"*{nonExistentFile}*");
    }

    [Fact]
    public void LoadConfiguration_With_Invalid_Json_Should_Throw_JsonException()
    {
        const string invalidJson = @"{ invalid json }";
        var fileName = $"{_testFilePrefix}{Path.GetRandomFileName()}.json";
        File.WriteAllText(fileName, invalidJson);
        _createdFiles.Add(fileName);

        Action act = () => _provider.LoadConfiguration(fileName);
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    public void Dispose()
    {
        Console.WriteLine($"Starting to clean up JSON Provider test files...");
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
                    Console.WriteLine($"Deleted JSON test file: {file}");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    Console.WriteLine($"Warning: Unable to delete JSON test file {file}: {ex.Message}");
                }
            }
            else
            {
                _createdFiles.Remove(file);
            }
        }

        Console.WriteLine($"JSON Provider cleanup completed: Deleted {deletedCount} files, failed {failedCount}");
    }
}