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

        // Load XML with proper encoding handling for auto-generated files
        using (var reader = new StreamReader(source, Encoding.UTF8, true))
        {
            doc.Load(reader);
        }

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

            // Handle attribute-based configuration elements (<setting name="key" value="value" />)
            if (childNode.Attributes != null && childNode.Attributes.Count >= 2)
            {
                var nameAttr = childNode.Attributes["name"];
                var valueAttr = childNode.Attributes["value"];
                
                if (nameAttr != null && valueAttr != null)
                {
                    // Create the full key with proper hierarchy
                    // For <appSettings><setting name="Environment" value="Integration" />, 
                    // this should create "appSettings.Environment"
                    var attributeName = nameAttr.Value;
                    var attributeValue = valueAttr.Value;
                    var fullKey = string.IsNullOrEmpty(prefix) ? attributeName : $"{prefix}.{attributeName}";
                    configDict[fullKey] = attributeValue;
                    
                    // Skip further processing of this node since we've handled the attributes
                    continue;
                }
            }

            // For regular elements, create the hierarchical key
            var elementKey = string.IsNullOrEmpty(prefix) ? childNode.Name : $"{prefix}.{childNode.Name}";

            // Process the node based on its content type
            if (childNode.HasChildNodes)
            {
                // Text content node (<element>value</element>)
                if (childNode.ChildNodes.Count == 1 && childNode.FirstChild.NodeType == XmlNodeType.Text)
                {
                    configDict[elementKey] = childNode.InnerText.Trim();
                }
                // Element with child nodes - recurse deeper
                else
                {
                    ParseXmlNode(childNode, configDict, elementKey);
                }
            }
            else
            {
                // Empty element (<element />)
                configDict[elementKey] = "";
            }
        }
    }
}