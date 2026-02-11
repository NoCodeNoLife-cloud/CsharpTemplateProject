using System.Text;
using System.Xml;
using CommonFramework.Configuration.Exceptions;
using CommonFramework.Configuration.Interfaces;

namespace CommonFramework.Configuration.Providers;

/// <summary>
/// XML configuration provider implementation
/// </summary>
public sealed class XmlConfigurationProvider : IConfigurationProvider
{
    /// <summary>
    /// Gets the name of the configuration provider
    /// </summary>
    public string Name => ConfigurationConstants.XmlProviderName;

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    public bool CanHandleSource(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.EndsWith(ConfigurationConstants.XmlExtension, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Load configuration data from XML file synchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="XmlException">Thrown when XML parsing fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateInput(source);
        ValidateFileExists(source);

        try
        {
            var doc = new XmlDocument();
            
            // Load XML with proper encoding handling for auto-generated files
            using var fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fileStream, Encoding.UTF8, true);
            doc.Load(reader);

            var configDict = new Dictionary<string, object>();
            if (doc.DocumentElement != null)
            {
                ParseXmlNode(doc.DocumentElement, configDict, "");
            }

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
    }

    /// <summary>
    /// Load configuration data from XML file asynchronously
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null</exception>
    /// <exception cref="ArgumentException">Thrown when source is empty or whitespace</exception>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="XmlException">Thrown when XML parsing fails</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
    public async Task<Dictionary<string, object>> LoadConfigurationAsync(string source, CancellationToken cancellationToken = default)
    {
        ValidateInput(source);
        ValidateFileExists(source);

        try
        {
            var doc = new XmlDocument();
            
            // Load XML with proper encoding handling for auto-generated files
            using var fileStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fileStream, Encoding.UTF8, true);
            
            // For XML loading, we need to do it synchronously as XmlDocument doesn't have async methods
            await Task.Run(() => doc.Load(reader), cancellationToken);

            var configDict = new Dictionary<string, object>();
            if (doc.DocumentElement != null)
            {
                ParseXmlNode(doc.DocumentElement, configDict, "");
            }

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
    /// Recursively parse XML nodes into configuration dictionary
    /// </summary>
    /// <param name="node">The XML node to parse</param>
    /// <param name="configDict">The configuration dictionary to populate</param>
    /// <param name="prefix">The key prefix for nested elements</param>
    private static void ParseXmlNode(XmlNode node, Dictionary<string, object> configDict, string prefix)
    {
        foreach (XmlNode childNode in node.ChildNodes)
        {
            if (childNode.NodeType != XmlNodeType.Element) continue;

            // Handle attribute-based configuration elements (<setting name="key" value="value" />)
            if (TryParseAttributeBasedElement(childNode, configDict, prefix))
            {
                continue;
            }

            // For regular elements, create the hierarchical key
            var elementKey = string.IsNullOrEmpty(prefix) ? childNode.Name : $"{prefix}.{childNode.Name}";

            // Process the node based on its content type
            ProcessElementContent(childNode, configDict, elementKey);
        }
    }

    /// <summary>
    /// Attempts to parse attribute-based configuration elements
    /// </summary>
    /// <param name="node">The XML node to parse</param>
    /// <param name="configDict">The configuration dictionary to populate</param>
    /// <param name="prefix">The key prefix for nested elements</param>
    /// <returns>True if the node was processed as attribute-based, false otherwise</returns>
    private static bool TryParseAttributeBasedElement(XmlNode node, Dictionary<string, object> configDict, string prefix)
    {
        if (node.Attributes is not { Count: >= 2 }) 
            return false;

        var nameAttr = node.Attributes["name"];
        var valueAttr = node.Attributes["value"];
        
        if (nameAttr?.Value is null || valueAttr?.Value is null)
            return false;

        // Create the full key with proper hierarchy
        var fullKey = string.IsNullOrEmpty(prefix) ? nameAttr.Value : $"{prefix}.{nameAttr.Value}";
        configDict[fullKey] = valueAttr.Value;
        
        return true;
    }

    /// <summary>
    /// Processes the content of an XML element
    /// </summary>
    /// <param name="node">The XML node to process</param>
    /// <param name="configDict">The configuration dictionary to populate</param>
    /// <param name="elementKey">The key for this element</param>
    private static void ProcessElementContent(XmlNode node, Dictionary<string, object> configDict, string elementKey)
    {
        if (!node.HasChildNodes)
        {
            // Empty element (<element />)
            configDict[elementKey] = "";
            return;
        }

        if (node.ChildNodes.Count == 1 && node.FirstChild is { NodeType: XmlNodeType.Text })
        {
            // Text content node (<element>value</element>)
            configDict[elementKey] = node.InnerText.Trim();
            return;
        }

        // Element with child nodes - recurse deeper
        ParseXmlNode(node, configDict, elementKey);
    }
}
