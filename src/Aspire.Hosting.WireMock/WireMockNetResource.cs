using Aspire.Hosting.ApplicationModel;
using WireMock.Client.Builders;

namespace Aspire.Hosting;

/// <summary>
/// A resource that represents a WireMock.Net resource.
/// </summary>
/// <param name="name">The resource name.</param>
/// <param name="enableWireMockInspector">Enable WireMock Inspector</param>
public class WireMockNetResource(string name, bool enableWireMockInspector) : ContainerResource(name), IResourceWithServiceDiscovery
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    internal Func<AdminApiMappingBuilder, Task>? ApiMappingBuilder { get; set; }

    /// <summary>
    /// Gets the primary endpoint for the server.
    /// </summary>
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
    
    
     public bool EnabledWireMockInspector => enableWireMockInspector;
}
