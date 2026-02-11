using System.Text.Json;
using System.Text.Json.Serialization;
using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Interfaces;

namespace CommonFramework.Configuration.Providers;

/// <summary>
/// JSON configuration provider implementation
/// </summary>
public sealed class JsonConfigurationProvider : IConfigurationProvider
{
    /// <summary>
    /// Gets the name of the configuration provider
    /// </summary>
    public string Name => ConfigurationConstants.JsonProviderName;

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    public bool CanHandleSource(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.EndsWith(ConfigurationConstants.JsonExtension, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Load configuration data from JSON file synchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateInput(source);
        ValidateFileExists(source);

        try
        {
            using var fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fileStream);
            var jsonContent = reader.ReadToEnd();

            return DeserializeAndFlattenJson(jsonContent);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigurationException($"Access denied to configuration file: {source}", ex);
        }
        catch (IOException ex)
        {
            throw new ConfigurationException($"Failed to read configuration file: {source}", ex);
        }
    }

    /// <summary>
    /// Load configuration data from JSON file asynchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
    public async Task<Dictionary<string, object>> LoadConfigurationAsync(string source, CancellationToken cancellationToken = default)
    {
        ValidateInput(source);
        ValidateFileExists(source);

        try
        {
            using var fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fileStream);
            var jsonContent = await reader.ReadToEndAsync(cancellationToken);

            return DeserializeAndFlattenJson(jsonContent);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigurationException($"Access denied to configuration file: {source}", ex);
        }
        catch (IOException ex)
        {
            throw new ConfigurationException($"Failed to read configuration file: {source}", ex);
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
    /// Deserialize JSON content and flatten the object structure
    /// </summary>
    /// <param name="jsonContent">JSON content string</param>
    /// <returns>Flattened configuration dictionary</returns>
    /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
    private static Dictionary<string, object> DeserializeAndFlattenJson(string jsonContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new ObjectToInferredTypesConverter() },
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        try
        {
            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, options);
            return FlattenJsonObject(jsonObject ?? []);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to parse JSON configuration. Please check the file format.", ex);
        }
    }

    /// <summary>
    /// Recursively flatten JSON object
    /// </summary>
    /// <param name="jsonObject">JSON object to flatten</param>
    /// <param name="prefix">Key prefix for nested elements</param>
    /// <returns>Flattened dictionary</returns>
    private static Dictionary<string, object> FlattenJsonObject(Dictionary<string, object> jsonObject, string prefix = "")
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in jsonObject)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            switch (kvp.Value)
            {
                case Dictionary<string, object> nestedDict:
                {
                    var flattened = FlattenJsonObject(nestedDict, key);
                    foreach (var nestedKvp in flattened)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }

                    break;
                }
                case IEnumerable<object> enumerable when kvp.Value is not string:
                {
                    var list = enumerable.ToList();
                    for (var i = 0; i < list.Count; i++)
                    {
                        var arrayKey = $"{key}[{i}]";
                        if (list[i] is Dictionary<string, object> listItemDict)
                        {
                            var flattened = FlattenJsonObject(listItemDict, arrayKey);
                            foreach (var nestedKvp in flattened)
                            {
                                result[nestedKvp.Key] = nestedKvp.Value;
                            }
                        }
                        else
                        {
                            result[arrayKey] = list[i];
                        }
                    }

                    break;
                }
                default:
                    result[key] = kvp.Value;
                    break;
            }
        }

        return result;
    }
}

/// <summary>
/// Custom converter to deserialize JSON to object types instead of JsonElement
/// </summary>
public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt32(out var intValue) => intValue,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.StartObject => JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options),
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<object>>(ref reader, options),
            JsonTokenType.Null => null,
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}