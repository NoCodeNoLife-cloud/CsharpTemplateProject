using CommonFramework.Configuration.Interfaces;
using CommonFramework.Configuration.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.ConfigurationTests;

public class ConfigurationServiceTests
{
    private readonly ConfigurationServiceImpl _configService = ConfigurationServiceImpl.InstanceVal;
    private readonly Mock<IConfigurationProvider> _mockProvider = new();

    [Fact]
    public void Singleton_Instance_Should_Return_Same_Instance()
    {
        // Arrange & Act
        var instance1 = ConfigurationServiceImpl.InstanceVal;
        var instance2 = ConfigurationServiceImpl.InstanceVal;

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void RegisterProvider_With_Valid_Provider_Should_Register_Successfully()
    {
        // Arrange
        _mockProvider.Setup(p => p.Name).Returns("TestProvider");

        // Act
        _configService.RegisterProvider(_mockProvider.Object);

        // Assert
        // Registration should not throw exception and Name should be accessed
        _mockProvider.Verify(p => p.Name, Times.AtLeastOnce);
    }

    [Fact]
    public void RegisterProvider_With_Null_Provider_Should_Throw_ArgumentNullException()
    {
        // Act
        Action act = () => _configService.RegisterProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Fact]
    public void ContainsKey_With_Existing_Key_Should_Return_True()
    {
        // Arrange
        _configService.SetValue("TestKey", "TestValue");

        // Act
        var result = _configService.ContainsKey("TestKey");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsKey_With_NonExisting_Key_Should_Return_False()
    {
        // Act
        var result = _configService.ContainsKey("NonExistentKey");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsKey_With_Empty_Key_Should_Return_False()
    {
        // Act
        var result = _configService.ContainsKey("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAllKeys_Should_Return_All_Configuration_Keys()
    {
        // Arrange
        _configService.SetValue("Key1", "Value1");
        _configService.SetValue("Key2", "Value2");

        // Act
        var keys = _configService.GetAllKeys().ToList();

        // Assert
        keys.Should().Contain("Key1");
        keys.Should().Contain("Key2");
    }

    [Fact]
    public void SetValue_With_Valid_Key_And_Value_Should_Set_Successfully()
    {
        // Arrange
        const string key = "TestKey";
        const string value = "TestValue";

        // Act
        _configService.SetValue(key, value);

        // Assert
        _configService.GetValue<string>(key).Should().Be(value);
    }

    [Fact]
    public void SetValue_With_Empty_Key_Should_Throw_ArgumentException()
    {
        // Act
        Action act = () => _configService.SetValue("", "value");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("key");
    }

    [Fact]
    public void GetValue_With_Existing_Key_Should_Return_Correct_Value()
    {
        // Arrange
        const string key = "TestKey";
        const string expectedValue = "TestValue";
        _configService.SetValue(key, expectedValue);

        // Act
        var result = _configService.GetValue<string>(key);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetValue_With_NonExisting_Key_Should_Return_Default_Value()
    {
        // Act
        var result = _configService.GetValue<string>("NonExistentKey", "DefaultValue");

        // Assert
        result.Should().Be("DefaultValue");
    }

    [Fact]
    public void GetValue_With_Type_Conversion_Should_Work_Correctly()
    {
        // Arrange
        _configService.SetValue("IntKey", "42");
        _configService.SetValue("BoolKey", "true");

        // Act
        var intValue = _configService.GetValue<int>("IntKey");
        var boolValue = _configService.GetValue<bool>("BoolKey");

        // Assert
        intValue.Should().Be(42);
        boolValue.Should().BeTrue();
    }

    [Fact]
    public void Refresh_Should_Clear_Configuration_Cache()
    {
        // Arrange
        _configService.SetValue("TestKey", "TestValue");

        // Act
        _configService.Refresh();

        // Assert
        _configService.ContainsKey("TestKey").Should().BeFalse();
    }

    [Fact]
    public void GetValue_With_Empty_Key_Should_Throw_ArgumentException()
    {
        // Act
        Action act = () => _configService.GetValue<string>("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("key");
    }
}