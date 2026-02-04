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
    public string Name => "XML";

    /// <summary>
    /// Check if the provider can handle the specified configuration source
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Whether it can be handled</returns>
    public bool CanHandleSource(string source)
    {
        return source.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Load configuration data from XML file
    /// </summary>
    /// <param name="source">Configuration source path</param>
    /// <returns>Configuration dictionary</returns>
    /// <exception cref="ConfigurationFileNotFoundException">Thrown when configuration file is not found</exception>
    /// <exception cref="XmlException">Thrown when XML parsing fails</exception>
    public Dictionary<string, object> LoadConfiguration(string source)
    {
        ValidateFileExists(source);

        var doc = new XmlDocument();
        doc.Load(source);

        var configDict = new Dictionary<string, object>();
        if (doc.DocumentElement != null) ParseXmlNode(doc.DocumentElement, configDict, "");

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
            var key = string.IsNullOrEmpty(prefix) ? childNode.Name : $"{prefix}.{childNode.Name}";

            if (childNode is { HasChildNodes: true, FirstChild.NodeType: XmlNodeType.Text })
            {
                configDict[key] = childNode.InnerText;
            }
            else if (childNode.HasChildNodes)
            {
                ParseXmlNode(childNode, configDict, key);
            }
            else
            {
                configDict[key] = "";
            }
        }
    }
}