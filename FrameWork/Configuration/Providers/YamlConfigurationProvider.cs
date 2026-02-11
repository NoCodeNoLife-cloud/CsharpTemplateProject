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
    public string Name => ConfigurationConstants.YamlProviderName;

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    public bool CanHandleSource(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return ConfigurationConstants.YamlExtensions.Any(ext => 
            source.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Load configuration data from YAML file synchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="YamlException">Thrown when YAML parsing fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateInput(source);
        ValidateFileExists(source);

        try
        {
            string yamlContent;
            using (var fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fileStream))
            {
                yamlContent = reader.ReadToEnd();
            }

            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<object>(yamlContent);

            var configDict = new Dictionary<string, object>();
            FlattenObject(yamlObject, configDict, "");
            return configDict;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigurationException($"Access denied to configuration file: {source}", ex);
        }
        catch (IOException ex)
        {
            throw new ConfigurationException($"Failed to read configuration file: {source}", ex);
        }
        catch (YamlException ex)
        {
            throw new ConfigurationException($"Failed to parse YAML configuration: {source}", ex);
        }
    }

    /// <summary>
    /// Load configuration data from YAML file asynchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="YamlException">Thrown when YAML parsing fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
    public async Task<Dictionary<string, object>> LoadConfigurationAsync(string source, CancellationToken cancellationToken = default)
    {
        ValidateInput(source);
        ValidateFileExists(source);

        try
        {
            string yamlContent;
            using (var fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fileStream))
            {
                yamlContent = await reader.ReadToEndAsync(cancellationToken);
            }

            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<object>(yamlContent);

            var configDict = new Dictionary<string, object>();
            FlattenObject(yamlObject, configDict, "");
            return configDict;
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigurationException($"Access denied to configuration file: {source}", ex);
        }
        catch (IOException ex)
        {
            throw new ConfigurationException($"Failed to read configuration file: {source}", ex);
        }
        catch (YamlException ex)
        {
            throw new ConfigurationException($"Failed to parse YAML configuration: {source}", ex);
        }
    }

    /// <summary>
    /// Validates input parameters
    /// </summary>
    /// <param name="source">The source path to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    private static void ValidateInput(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Configuration source path cannot be empty or whitespace", nameof(source));
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
                FlattenDictionary(dict, configDict, prefix);
                break;
                
            case IList<object> list:
                FlattenList(list, configDict, prefix);
                break;
                
            default:
                if (!string.IsNullOrEmpty(prefix))
                {
                    configDict[prefix] = obj;
                }
                break;
        }
    }

    /// <summary>
    /// Flattens a dictionary object
    /// </summary>
    /// <param name="dict">The dictionary to flatten</param>
    /// <param name="configDict">The configuration dictionary to populate</param>
    /// <param name="prefix">The key prefix for nested elements</param>
    private static void FlattenDictionary(Dictionary<object, object> dict, Dictionary<string, object> configDict, string prefix)
    {
        foreach (var kvp in dict)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key.ToString() : $"{prefix}.{kvp.Key}";
            if (key != null)
            {
                FlattenObject(kvp.Value, configDict, key);
            }
        }
    }

    /// <summary>
    /// Flattens a list object
    /// </summary>
    /// <param name="list">The list to flatten</param>
    /// <param name="configDict">The configuration dictionary to populate</param>
    /// <param name="prefix">The key prefix for nested elements</param>
    private static void FlattenList(IList<object> list, Dictionary<string, object> configDict, string prefix)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var key = $"{prefix}[{i}]";
            FlattenObject(list[i], configDict, key);
        }
    }
}
