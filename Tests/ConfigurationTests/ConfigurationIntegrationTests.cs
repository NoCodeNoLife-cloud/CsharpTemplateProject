using CommonFramework.Configuration;
using CommonFramework.Configuration.Interfaces;
using FluentAssertions;
using Xunit;
using System.Threading;

namespace Tests.ConfigurationTests;

public class ConfigurationIntegrationTests : IDisposable
{
    private readonly List<string> _createdFiles;
    private readonly string _testFilePrefix;

    public ConfigurationIntegrationTests()
    {
        _createdFiles = new List<string>();
        _testFilePrefix = $"integration-test-{Guid.NewGuid():N}-";
        
        // Ensure complete test isolation by refreshing configuration cache
        ForceConfigurationRefresh();
        
        CreateTestConfigurationFiles();
    }

    [Fact]
    public void Integration_Test_Json_Configuration_Loading()
    {
        // Use pre-created test file with prefix
        var fileName = $"{_testFilePrefix}integration-test.json";

        var configService = ConfigurationBuilder.CreateDefault()
            .LoadFrom(fileName)
            .Build();

        configService.GetValue<string>("AppName").Should().Be("IntegrationTestApp");
        configService.GetValue<int>("Port").Should().Be(9090);
        configService.GetValue<bool>("Debug").Should().BeTrue();
        configService.GetValue<string>("Database.ConnectionString").Should().Be("Server=integration;Database=testdb");
        configService.ContainsKey("AppName").Should().BeTrue();
        configService.ContainsKey("NonExistent").Should().BeFalse();
    }

    [Fact]
    public void Integration_Test_Xml_Configuration_Loading()
    {
        // Ensure clean state for this specific test
        ForceConfigurationRefresh();
        
        // Use pre-created test file with prefix
        var fileName = $"{_testFilePrefix}integration-test.xml";

        var configService = ConfigurationBuilder.CreateDefault()
            .LoadFrom(fileName)
            .Build();

        configService.GetAllKeys().Should().NotBeEmpty();
        configService.ContainsKey("appSettings.Environment").Should().BeTrue();
        configService.ContainsKey("appSettings.MaxRetries").Should().BeTrue();
        configService.ContainsKey("appSettings.EnableLogging").Should().BeTrue();

        // Verify values are correct
        configService.GetValue<string>("appSettings.Environment").Should().Be("Integration");
        configService.GetValue<string>("appSettings.MaxRetries").Should().Be("5");
        configService.GetValue<bool>("appSettings.EnableLogging").Should().BeFalse();
    }

    [Fact]
    public void Integration_Test_Multiple_Sources_Merging()
    {
        // Ensure clean state for this specific test
        ForceConfigurationRefresh();
        
        // Use pre-created test files with prefix
        var jsonFile = $"{_testFilePrefix}multi-integration.json";
        var xmlFile = $"{_testFilePrefix}multi-integration.xml";

        var configService = ConfigurationBuilder.CreateDefault()
            .LoadFrom(jsonFile, xmlFile)
            .Build();

        // XML configuration should override JSON configuration
        configService.GetValue<string>("SharedSetting").Should().Be("from-xml");
        configService.GetValue<string>("JsonOnly").Should().Be("json-value");
        configService.GetValue<string>("XmlOnly").Should().Be("xml-value");

        var allKeys = configService.GetAllKeys().ToList();
        allKeys.Should().Contain("SharedSetting");
        allKeys.Should().Contain("JsonOnly");
        allKeys.Should().Contain("XmlOnly");
    }

    [Fact]
    public void Integration_Test_Type_Conversion()
    {
        // Ensure clean state for this specific test
        ForceConfigurationRefresh();
        
        // Use pre-created test file with prefix
        var fileName = $"{_testFilePrefix}conversion-integration.json";

        var configService = ConfigurationBuilder.CreateDefault()
            .LoadFrom(fileName)
            .Build();

        configService.GetValue<string>("StringValue").Should().Be("hello");
        configService.GetValue<int>("IntValue").Should().Be(42);
        configService.GetValue<bool>("BoolValue").Should().BeTrue();
        configService.GetValue<double>("DoubleValue").Should().Be(3.14);
        configService.GetValue<string>("NonExistent", "default-value").Should().Be("default-value");
    }

    [Fact]
    public void Integration_Test_Custom_Provider()
    {
        // Ensure clean state for this specific test
        ForceConfigurationRefresh();
        
        var customProvider = new InMemoryConfigurationProvider();
        customProvider.SetConfig("Custom.Key1", "custom-value-1");
        customProvider.SetConfig("Custom.Key2", 123);

        var configService = ConfigurationBuilder.CreateDefault()
            .AddProvider(customProvider)
            .LoadFrom("memory://test")
            .Build();

        configService.GetValue<string>("Custom.Key1").Should().Be("custom-value-1");
        configService.GetValue<int>("Custom.Key2").Should().Be(123);

        configService.SetValue("Runtime.Key", "runtime-value");
        configService.GetValue<string>("Runtime.Key").Should().Be("runtime-value");
    }

    /// <summary>
    /// Creates necessary configuration files before tests begin
    /// </summary>
    private void CreateTestConfigurationFiles()
    {
        Console.WriteLine($"Starting to create integration test configuration files, prefix: {_testFilePrefix}");

        // Create JSON file for integration testing
        const string jsonContent = @"{
            ""AppName"": ""IntegrationTestApp"",
            ""Version"": ""2.0.0"",
            ""Port"": 9090,
            ""Debug"": true,
            ""Database"": {
                ""ConnectionString"": ""Server=integration;Database=testdb"",
                ""Timeout"": 60
            }
        }";
        var jsonFile = $"{_testFilePrefix}integration-test.json";
        File.WriteAllText(jsonFile, jsonContent);
        _createdFiles.Add(jsonFile);
        Console.WriteLine($"Created integration test file: {jsonFile}");

        // Create XML file for integration testing
        const string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <appSettings>
        <setting name=""Environment"" value=""Integration"" />
        <setting name=""MaxRetries"" value=""5"" />
        <setting name=""EnableLogging"" value=""false"" />
    </appSettings>
</configuration>";
        var xmlFile = $"{_testFilePrefix}integration-test.xml";
        File.WriteAllText(xmlFile, xmlContent);
        _createdFiles.Add(xmlFile);
        Console.WriteLine($"Created integration test file: {xmlFile}");

        // Create multi-source merge test files
        const string multiJsonContent = @"{""SharedSetting"": ""from-json"", ""JsonOnly"": ""json-value""}";
        var multiJsonFile = $"{_testFilePrefix}multi-integration.json";
        File.WriteAllText(multiJsonFile, multiJsonContent);
        _createdFiles.Add(multiJsonFile);
        Console.WriteLine($"Created multi-source test file: {multiJsonFile}");

        const string multiXmlContent = @"<config><SharedSetting>from-xml</SharedSetting><XmlOnly>xml-value</XmlOnly></config>";
        var multiXmlFile = $"{_testFilePrefix}multi-integration.xml";
        File.WriteAllText(multiXmlFile, multiXmlContent);
        _createdFiles.Add(multiXmlFile);
        Console.WriteLine($"Created multi-source test file: {multiXmlFile}");

        // Create type conversion test file
        const string conversionContent = @"{
            ""StringValue"": ""hello"",
            ""IntValue"": ""42"",
            ""BoolValue"": ""true"",
            ""DoubleValue"": ""3.14""
        }";
        var conversionFile = $"{_testFilePrefix}conversion-integration.json";
        File.WriteAllText(conversionFile, conversionContent);
        _createdFiles.Add(conversionFile);
        Console.WriteLine($"Created conversion test file: {conversionFile}");

        Console.WriteLine($"Total {_createdFiles.Count} integration test files created");
    }

    /// <summary>
    /// Forces complete configuration refresh with retry mechanism
    /// </summary>
    private static void ForceConfigurationRefresh()
    {
        const int maxRetries = 3;
        const int delayMs = 100;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                CommonFramework.ConfigurationServiceImpl.InstanceVal.Refresh();
                
                // Verify cache is actually cleared
                var keys = CommonFramework.ConfigurationServiceImpl.InstanceVal.GetAllKeys().ToList();
                if (keys.Count == 0)
                {
                    Console.WriteLine($"Configuration cache successfully cleared on attempt {i + 1}");
                    return;
                }
                
                Console.WriteLine($"Attempt {i + 1}: Cache still contains {keys.Count} keys, retrying...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {i + 1}: Failed to refresh configuration: {ex.Message}");
            }
            
            if (i < maxRetries - 1)
            {
                Thread.Sleep(delayMs);
            }
        }
        
        Console.WriteLine("Warning: Configuration refresh may not have completed successfully");
    }

    /// <summary>
    /// Implements IDisposable interface to automatically clean up created files after tests
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine($"Starting to clean up integration test files...");
        var deletedCount = 0;
        var failedCount = 0;

        // Clean up all created test files
        foreach (var file in _createdFiles.ToList())
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    _createdFiles.Remove(file);
                    deletedCount++;
                    Console.WriteLine($"Deleted integration test file: {file}");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    Console.WriteLine($"Warning: Unable to delete integration test file {file}: {ex.Message}");
                }
            }
            else
            {
                _createdFiles.Remove(file);
                Console.WriteLine($"Integration test file does not exist, removed from tracking list: {file}");
            }
        }

        Console.WriteLine($"Integration test cleanup completed: Successfully deleted {deletedCount} files, failed {failedCount}, {_createdFiles.Count} files remaining unprocessed");

        Console.WriteLine(_createdFiles.Count == 0 ? "All integration test files cleaned up successfully" : $"Note: {_createdFiles.Count} integration test files still not cleaned up");
    }
}

public class InMemoryConfigurationProvider : IConfigurationProvider
{
    private readonly Dictionary<string, object> _configData = new();

    public string Name => "InMemory";

    public Dictionary<string, object> LoadConfiguration(string source)
    {
        if (source.StartsWith("memory://"))
        {
            return new Dictionary<string, object>(_configData);
        }

        return new Dictionary<string, object>();
    }

    public bool CanHandleSource(string source)
    {
        return source.StartsWith("memory://");
    }

    public void SetConfig(string key, object value)
    {
        _configData[key] = value;
    }
}