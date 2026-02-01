using AspectCore.DynamicProxy;

namespace CommonFramework.Aop;

/// <summary>
/// Provides global proxy generation services using AspectCore DynamicProxy
/// </summary>
public static class AopGlobalProxyGenerator
{
    private static readonly Lazy<IProxyGenerator> LazyProxyGenerator = new(CreateProxyGenerator);

    /// <summary>
    /// Gets the singleton instance of IProxyGenerator
    /// </summary>
    public static IProxyGenerator Instance => LazyProxyGenerator.Value;

    /// <summary>
    /// Creates a proxy generator instance
    /// </summary>
    /// <returns>New instance of ProxyGenerator</returns>
    private static IProxyGenerator CreateProxyGenerator()
    {
        var proxyGeneratorBuilder = new ProxyGeneratorBuilder();
        return proxyGeneratorBuilder.Build();
    }

    /// <summary>
    /// Creates a class proxy instance with default constructor
    /// </summary>
    /// <typeparam name="T">Type of class to create proxy for, must be class and have parameterless constructor</typeparam>
    /// <returns>Proxy instance of type T</returns>
    public static T CreateClassProxy<T>() where T : class, new()
    {
        return Instance.CreateClassProxy<T>();
    }

    /// <summary>
    /// Creates a class proxy instance with constructor arguments
    /// </summary>
    /// <typeparam name="T">Type of class to create proxy for, must be class and have appropriate constructor</typeparam>
    /// <param name="args">Constructor arguments to pass to the proxied class</param>
    /// <returns>Proxy instance of type T</returns>
    public static T CreateClassProxy<T>(params object[] args) where T : class, new()
    {
        return Instance.CreateClassProxy<T>(args);
    }
}