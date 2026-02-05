using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Providers;
using FluentAssertions;
using Xunit;

namespace Ci.Ut;

public class JsonConfigurationProviderTests : IDisposable
{
    private readonly JsonConfigurationProvider _provider;
    private readonly List<string> _createdFiles;

    public JsonConfigurationProviderTests()
    {
        _provider = new JsonConfigurationProvider();
        _createdFiles = new List<string>();
    }

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
        var fileName = Path.GetRandomFileName() + ".json";
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
        var fileName = Path.GetRandomFileName() + ".json";
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
        var fileName = Path.GetRandomFileName() + ".json";
        File.WriteAllText(fileName, invalidJson);
        _createdFiles.Add(fileName);

        Action act = () => _provider.LoadConfiguration(fileName);
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    public void Dispose()
    {
        foreach (var file in _createdFiles)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"警告: 无法删除测试文件 {file}: {ex.Message}");
                }
            }
        }
    }
}
