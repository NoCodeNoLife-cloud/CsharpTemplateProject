using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Interfaces;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace CommonFramework.Configuration.Providers;

/// <summary>
/// YAML configuration provider implementation
/// </summary>
public sealed class YamlConfigurationProvider : IConfigurationProvider
{
    /// <summary>
    /// Gets the name of the configuration provider
    /// </summary>
    public string Name => "YAML";

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    public bool CanHandleSource(string source)
    {
        return source.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
               source.EndsWith(".yml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Load configuration data from YAML file
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="YamlException">Thrown when YAML parsing fails</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateFileExists(source);

        var yamlContent = File.ReadAllText(source);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<object>(yamlContent);

        var configDict = new Dictionary<string, object>();
        FlattenObject(yamlObject, configDict, "");

        return configDict;
    }

    /// <summary>
    /// Validates that the configuration file exists
    /// </summary>
    /// <param name="source">The file path to validate</param>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when file does not exist</exception>
    private static void ValidateFileExists(string source)
    {
        if (!File.Exists(source))
            throw new ConfigurationFileNotFoundException(source);
    }

    /// <summary>
    /// Recursively flatten YAML object into configuration dictionary
    /// </summary>
    /// <param name="obj">The YAML object to flatten</param>
    /// <param name="configDict">The configuration dictionary to populate</param>
    /// <param name="prefix">The key prefix for nested elements</param>
    private static void FlattenObject(object obj, Dictionary<string, object> configDict, string prefix)
    {
        switch (obj)
        {
            case Dictionary<object, object> dict:
            {
                foreach (var kvp in dict)
                {
                    var key = string.IsNullOrEmpty(prefix) ? kvp.Key.ToString() : $"{prefix}.{kvp.Key}";
                    if (key != null) FlattenObject(kvp.Value, configDict, key);
                }

                break;
            }
            case IList<object> list:
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var key = $"{prefix}[{i}]";
                    FlattenObject(list[i], configDict, key);
                }

                break;
            }
            default:
                configDict[prefix] = obj;
                break;
        }
    }
}