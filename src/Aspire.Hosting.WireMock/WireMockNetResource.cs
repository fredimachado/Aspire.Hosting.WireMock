using Aspire.Hosting.ApplicationModel;
using WireMock.Client.Builders;

namespace Aspire.Hosting;

/// <summary>
/// A resource that represents a WireMock.Net resource.
/// </summary>
/// <param name="name">The resource name.</param>
public class WireMockNetResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    internal Func<AdminApiMappingBuilder, Task>? ApiMappingBuilder { get; set; }

    /// <summary>
    /// Gets the primary endpoint for the WireMock server.
    /// </summary>
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);
}
