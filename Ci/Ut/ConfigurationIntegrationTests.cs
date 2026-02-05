using CommonFramework.Configuration;
using CommonFramework.Configuration.Interfaces;
using FluentAssertions;
using Xunit;

namespace Ci.Ut;

public class ConfigurationIntegrationTests : IDisposable
{
    private readonly List<string> _createdFiles;

    public ConfigurationIntegrationTests()
    {
        _createdFiles = new List<string>();
        CreateTestConfigurationFiles();
    }

    [Fact]
    public async Task Integration_Test_Json_Configuration_Loading()
    {
        // Use pre-created test file
        const string fileName = "integration-test.json";

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
    public async Task Integration_Test_Xml_Configuration_Loading()
    {
        // Use pre-created test file
        const string fileName = "integration-test.xml";

        var configService = ConfigurationBuilder.CreateDefault()
            .LoadFrom(fileName)
            .Build();

        configService.GetAllKeys().Should().NotBeEmpty();
        // According to XML parsing logic, should check specific configuration keys instead of index form
        configService.ContainsKey("appSettings.Environment").Should().BeTrue();
        configService.ContainsKey("appSettings.MaxRetries").Should().BeTrue();
        configService.ContainsKey("appSettings.EnableLogging").Should().BeTrue();
        
        // Verify values are correct
        configService.GetValue<string>("appSettings.Environment").Should().Be("Integration");
        configService.GetValue<string>("appSettings.MaxRetries").Should().Be("5");
        configService.GetValue<bool>("appSettings.EnableLogging").Should().BeFalse();
    }

    [Fact]
    public async Task Integration_Test_Multiple_Sources_Merging()
    {
        // Use pre-created test files
        const string jsonFile = "multi-integration.json";
        const string xmlFile = "multi-integration.xml";

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
    public async Task Integration_Test_Type_Conversion()
    {
        // Use pre-created test file
        const string fileName = "conversion-integration.json";

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
        var customProvider = new InMemoryConfigurationProvider();
        customProvider.SetConfig("Custom.Key1", "custom-value-1");
        customProvider.SetConfig("Custom.Key2", 123);

        var configService = ConfigurationBuilder.CreateDefault()
            .AddProvider(customProvider)
            .LoadFrom("memory://test") // 添加这行来触发LoadConfiguration
            .Build();

        configService.GetValue<string>("Custom.Key1").Should().Be("custom-value-1");
        configService.GetValue<int>("Custom.Key2").Should().Be(123);

        configService.SetValue("Runtime.Key", "runtime-value");
        configService.GetValue<string>("Runtime.Key").Should().Be("runtime-value");
    }

    /// <summary>
    /// 在测试开始前创建必要的配置文件
    /// </summary>
    private void CreateTestConfigurationFiles()
    {
        // 创建集成测试用的JSON文件
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
        var jsonFile = "integration-test.json";
        File.WriteAllText(jsonFile, jsonContent);
        _createdFiles.Add(jsonFile);

        // 创建集成测试用的XML文件
        const string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <appSettings>
        <setting name=""Environment"" value=""Integration"" />
        <setting name=""MaxRetries"" value=""5"" />
        <setting name=""EnableLogging"" value=""false"" />
    </appSettings>
</configuration>";
        var xmlFile = "integration-test.xml";
        File.WriteAllText(xmlFile, xmlContent);
        _createdFiles.Add(xmlFile);

        // 创建多源合并测试文件
        const string multiJsonContent = @"{""SharedSetting"": ""from-json"", ""JsonOnly"": ""json-value""}";
        var multiJsonFile = "multi-integration.json";
        File.WriteAllText(multiJsonFile, multiJsonContent);
        _createdFiles.Add(multiJsonFile);

        const string multiXmlContent = @"<config><SharedSetting>from-xml</SharedSetting><XmlOnly>xml-value</XmlOnly></config>";
        var multiXmlFile = "multi-integration.xml";
        File.WriteAllText(multiXmlFile, multiXmlContent);
        _createdFiles.Add(multiXmlFile);

        // 创建类型转换测试文件
        const string conversionContent = @"{
            ""StringValue"": ""hello"",
            ""IntValue"": ""42"",
            ""BoolValue"": ""true"",
            ""DoubleValue"": ""3.14""
        }";
        var conversionFile = "conversion-integration.json";
        File.WriteAllText(conversionFile, conversionContent);
        _createdFiles.Add(conversionFile);
    }

    /// <summary>
    /// 实现IDisposable接口，在测试结束后自动清理创建的文件
    /// </summary>
    public void Dispose()
    {
        // 清理所有创建的测试文件
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
                    Console.WriteLine($"Warning: Unable to delete test file {file}: {ex.Message}");
                }
            }
        }
    }
}

public class InMemoryConfigurationProvider : IConfigurationProvider
{
    private readonly Dictionary<string, object> _configData = new();

    public string Name => "InMemory";

    public Dictionary<string, object> LoadConfiguration(string source)
    {
        // When handling memory protocol, return preset configuration data
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
