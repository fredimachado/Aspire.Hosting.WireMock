using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.WireMock;
using WireMock.Client.Builders;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding the WireMock.Net resources to the application model.
/// </summary>
public static class WireMockNetExtensions
{
    /// <summary>
    /// Adds a WireMock.Net resource to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="port">External port</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{WireMockNetResource}"/>.</returns>
    public static IResourceBuilder<WireMockNetResource> AddWireMockNet(this IDistributedApplicationBuilder builder, string name, int? port = null)
    {
        var wireMockResource = new WireMockNetResource(name);

        return builder
            .AddResource(wireMockResource)
            .WithHttpEndpoint(port: port, targetPort: 80, name: WireMockNetResource.PrimaryEndpointName)
            .WithImage(WireMockNetContainerImageTags.Image, WireMockNetContainerImageTags.Tag)
            .WithImageRegistry(WireMockNetContainerImageTags.Registry);
    }

    /// <summary>
    /// Use WireMock Client's AdminApiMappingBuilder to configure the WireMock.Net resource.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{WireMockNetResource}"/>.</param>
    /// <param name="configure">Delegate that will be invoked to configure the WireMock.Net resource.</param>
    /// <returns></returns>
    public static IResourceBuilder<WireMockNetResource> WithApiMappingBuilder(this IResourceBuilder<WireMockNetResource> builder, Func<AdminApiMappingBuilder, Task> configure)
    {
        builder.ApplicationBuilder.Services.TryAddLifecycleHook<WireMockNetConfigHook>();

        builder.Resource.ApiMappingBuilder = configure;

        return builder;
    }
}
