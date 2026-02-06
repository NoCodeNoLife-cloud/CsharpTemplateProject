using CommonFramework.Configuration;
using CommonFramework.Configuration.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.ConfigurationTests;

public class ConfigurationBuilderTests : IDisposable
{
    private readonly Mock<IConfigurationProvider> _mockProvider;
    private readonly List<string> _createdFiles;
    private readonly string _testFilePrefix;

    public ConfigurationBuilderTests()
    {
        _mockProvider = new Mock<IConfigurationProvider>();
        _mockProvider.Setup(p => p.Name).Returns("TestProvider");
        _createdFiles = new List<string>();
        _testFilePrefix = $"config-test-{Guid.NewGuid():N}-";

        // Clear configuration cache before each test to ensure isolation
        CommonFramework.ConfigurationServiceImpl.InstanceVal.Refresh();

        // Create necessary configuration files before tests begin
        CreateTestConfigurationFiles();
    }

    [Fact]
    public void CreateDefault_Should_Return_ConfigurationBuilder_Instance()
    {
        // Act
        var builder = ConfigurationBuilder.CreateDefault();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void AddProvider_With_Valid_Provider_Should_Return_Builder_Instance()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();

        // Act
        var result = builder.AddProvider(_mockProvider.Object);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddProvider_With_Null_Provider_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();

        // Act
        Action act = () => builder.AddProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LoadFrom_With_Valid_Source_Should_Return_Builder_Instance()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();
        var source = $"{_testFilePrefix}test.json";

        // Act
        var result = builder.LoadFrom(source);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void LoadFrom_With_Empty_Source_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();

        // Act
        Action act = () => builder.LoadFrom("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LoadFrom_With_Empty_Source_In_Array_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();

        // Act & Assert - Verify that empty string source throws ArgumentException
        Action act = () => builder.LoadFrom("", "valid-source.json");

        // Should throw ArgumentException immediately when processing empty string
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration source path cannot be empty*")
            .Where(ex => ex.ParamName == "source");
    }

    [Fact]
    public void LoadFrom_With_Valid_Sources_Should_Work_Correctly()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();

        // Act & Assert - Verify that valid sources can be processed normally
        var result = builder.LoadFrom([$"{_testFilePrefix}test.json"]);

        // Should return builder instance
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void LoadFrom_With_Multiple_Sources_Should_Return_Builder_Instance()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();
        var sources = new[]
        {
            $"{_testFilePrefix}config1.json",
            $"{_testFilePrefix}config2.xml",
            $"{_testFilePrefix}config3.yaml"
        };

        // Act
        var result = builder.LoadFrom(sources);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Build_Should_Return_ConfigurationService_Instance()
    {
        // Arrange
        var builder = ConfigurationBuilder.CreateDefault();

        // Act
        var result = builder.Build();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IConfigurationService>();
    }

    [Fact]
    public void Fluent_Api_Chain_Should_Work_Correctly()
    {
        // Arrange
        var source = $"{_testFilePrefix}test.json";

        // Act
        var result = ConfigurationBuilder.CreateDefault()
            .AddProvider(_mockProvider.Object)
            .LoadFrom(source)
            .Build();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IConfigurationService>();
    }

    [Fact]
    public void Multiple_LoadFrom_Calls_Should_Accumulate_Sources()
    {
        // Arrange
        // Use new configuration service instance instead of shared instance
        var configService = ConfigurationBuilder.CreateDefault()
            .LoadFrom($"{_testFilePrefix}config1.json")
            .LoadFrom($"{_testFilePrefix}config2.xml")
            .Build();

        // Assert
        configService.ContainsKey("AppName").Should().BeTrue();
        configService.ContainsKey("appSettings.Environment").Should().BeTrue();
        configService.ContainsKey("appSettings.MaxRetries").Should().BeTrue();
    }

    [Fact]
    public void Xml_Configuration_Should_Generate_Correct_Key_Structure()
    {
        // Arrange
        var xmlFile = $"{_testFilePrefix}config2.xml";

        // Act
        var result = ConfigurationBuilder.CreateDefault()
            .LoadFrom(xmlFile)
            .Build();

        // Assert - Verify correct key structure generated by XML configuration
        result.ContainsKey("appSettings.Environment").Should().BeTrue();
        result.ContainsKey("appSettings.MaxRetries").Should().BeTrue();

        // Verify values are correct
        result.GetValue<string>("appSettings.Environment").Should().Be("Development");
        result.GetValue<string>("appSettings.MaxRetries").Should().Be("3");
    }

    /// <summary>
    /// Creates necessary configuration files before tests begin
    /// </summary>
    private void CreateTestConfigurationFiles()
    {
        Console.WriteLine($"Starting to create test configuration files, prefix: {_testFilePrefix}");

        // Create test.json file for Fluent_Api_Chain test
        const string testJsonContent = @"{
            ""AppName"": ""TestApp"",
            ""Version"": ""1.0.0"",
            ""Port"": 8080
        }";
        var testJsonFile = $"{_testFilePrefix}test.json";
        File.WriteAllText(testJsonFile, testJsonContent);
        _createdFiles.Add(testJsonFile);
        Console.WriteLine($"Created test file: {testJsonFile}");

        // Create JSON test file
        const string jsonContent = @"{
            ""AppName"": ""TestApp"",
            ""Version"": ""1.0.0"",
            ""Port"": 8080
        }";
        var jsonFile = $"{_testFilePrefix}config1.json";
        File.WriteAllText(jsonFile, jsonContent);
        _createdFiles.Add(jsonFile);
        Console.WriteLine($"Created test file: {jsonFile}");

        // Create XML test file
        const string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <appSettings>
        <setting name=""Environment"" value=""Development"" />
        <setting name=""MaxRetries"" value=""3"" />
    </appSettings>
</configuration>";
        var xmlFile = $"{_testFilePrefix}config2.xml";
        File.WriteAllText(xmlFile, xmlContent);
        _createdFiles.Add(xmlFile);
        Console.WriteLine($"Created test file: {xmlFile}");

        // Create YAML test file
        const string yamlContent = @"
Database:
  ConnectionString: Server=localhost;Database=testdb
  Timeout: 30
";
        var yamlFile = $"{_testFilePrefix}config3.yaml";
        File.WriteAllText(yamlFile, yamlContent);
        _createdFiles.Add(yamlFile);
        Console.WriteLine($"Created test file: {yamlFile}");

        Console.WriteLine($"Total {_createdFiles.Count} test files created");
    }

    /// <summary>
    /// Implements IDisposable interface to automatically clean up created files after tests
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine($"Starting to clean up test files...");
        var deletedCount = 0;
        var failedCount = 0;

        // Clean up all created test files
        foreach (var file in _createdFiles.ToList()) // Use ToList to avoid collection modification exception
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    _createdFiles.Remove(file);
                    deletedCount++;
                    Console.WriteLine($"Deleted test file: {file}");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    Console.WriteLine($"Warning: Unable to delete test file {file}: {ex.Message}");
                }
            }
            else
            {
                // File does not exist, but still remove from list
                _createdFiles.Remove(file);
                Console.WriteLine($"File does not exist, removed from tracking list: {file}");
            }
        }

        Console.WriteLine($"Cleanup completed: Successfully deleted {deletedCount} files, failed {failedCount}, {_createdFiles.Count} files remaining unprocessed");

        // Verify cleanup results
        Console.WriteLine(_createdFiles.Count == 0 ? "All test files cleaned up successfully" : $"Note: {_createdFiles.Count} files still not cleaned up");
    }
}