using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Providers;
using FluentAssertions;
using Xunit;

namespace Ci.Ut;

public class XmlConfigurationProviderTests : IDisposable
{
    private readonly XmlConfigurationProvider _provider;
    private readonly List<string> _createdFiles;

    public XmlConfigurationProviderTests()
    {
        _provider = new XmlConfigurationProvider();
        _createdFiles = new List<string>();
    }

    [Fact]
    public void Name_Should_Return_XML()
    {
        var name = _provider.Name;
        name.Should().Be("XML");
    }

    [Theory]
    [InlineData("config.xml")]
    [InlineData("settings.XML")]
    [InlineData("data.Xml")]
    public void CanHandleSource_With_Xml_File_Should_Return_True(string source)
    {
        var result = _provider.CanHandleSource(source);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("config.json")]
    [InlineData("settings.yaml")]
    [InlineData("data.txt")]
    public void CanHandleSource_With_NonXml_File_Should_Return_False(string source)
    {
        var result = _provider.CanHandleSource(source);
        result.Should().BeFalse();
    }

    [Fact]
    public void LoadConfiguration_With_Valid_Xml_Should_Return_Correct_Data()
    {
        const string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <appSettings>
        <setting name=""Environment"" value=""Development"" />
        <setting name=""MaxRetries"" value=""3"" />
        <setting name=""EnableLogging"" value=""true"" />
    </appSettings>
</configuration>";
        var fileName = Path.GetRandomFileName() + ".xml";
        File.WriteAllText(fileName, xmlContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void LoadConfiguration_With_Simple_Xml_Structure_Should_Parse_Correctly()
    {
        const string xmlContent = @"<config>
    <AppName>TestApp</AppName>
    <Version>1.0.0</Version>
    <Port>8080</Port>
</config>";
        var fileName = Path.GetRandomFileName() + ".xml";
        File.WriteAllText(fileName, xmlContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("AppName");
        result.Should().ContainKey("Version");
        result.Should().ContainKey("Port");
    }

    [Fact]
    public void LoadConfiguration_With_Nested_Xml_Should_Parse_Correctly()
    {
        const string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
    <database>
        <connection>
            <server>localhost</server>
            <database>testdb</database>
        </connection>
    </database>
</configuration>";
        var fileName = Path.GetRandomFileName() + ".xml";
        File.WriteAllText(fileName, xmlContent);
        _createdFiles.Add(fileName);

        var result = _provider.LoadConfiguration(fileName);
        result.Should().ContainKey("database.connection.server");
        result.Should().ContainKey("database.connection.database");
    }

    [Fact]
    public void LoadConfiguration_With_NonExistent_File_Should_Throw_ConfigurationFileNotFoundException()
    {
        const string nonExistentFile = "non-existent.xml";
        Action act = () => _provider.LoadConfiguration(nonExistentFile);
        act.Should().Throw<ConfigurationFileNotFoundException>().WithMessage($"*{nonExistentFile}*");
    }

    [Fact]
    public void LoadConfiguration_With_Invalid_Xml_Should_Throw_XmlException()
    {
        const string invalidXml = @"<invalid xml>";
        var fileName = Path.GetRandomFileName() + ".xml";
        File.WriteAllText(fileName, invalidXml);
        _createdFiles.Add(fileName);

        Action act = () => _provider.LoadConfiguration(fileName);
        act.Should().Throw<System.Xml.XmlException>();
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
