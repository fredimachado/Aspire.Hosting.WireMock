using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.WireMock;
using Microsoft.Extensions.DependencyInjection;
using RestEase;
using System.Data.Common;
using WireMock.Client;
using WireMock.Client.Builders;
using WireMock.Client.Extensions;

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
            .WithImageRegistry(WireMockNetContainerImageTags.Registry)
            .WithHttpHealthCheck("/__admin/mappings");
    }

    /// <summary>
    /// Use WireMock Client's AdminApiMappingBuilder to configure the WireMock.Net resource.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{WireMockNetResource}"/>.</param>
    /// <param name="configure">Delegate that will be invoked to configure the WireMock.Net resource.</param>
    /// <returns></returns>
    public static IResourceBuilder<WireMockNetResource> WithApiMappingBuilder(this IResourceBuilder<WireMockNetResource> builder, Func<AdminApiMappingBuilder, Task> configure)
    {
        builder.Resource.ApiMappingBuilder = configure;

        builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(builder.Resource, async (@event, cancellationToken) =>
        {
            if (builder.Resource is not WireMockNetResource resource)
            {
                return;
            }

            var notificationService = @event.Services.GetRequiredService<ResourceNotificationService>();

            try
            {
                var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot("No connection string", KnownResourceStateStyles.Error) });
                    return;
                }

                if (!Uri.TryCreate(connectionString, UriKind.Absolute, out _))
                {
                    var connectionBuilder = new DbConnectionStringBuilder
                    {
                        ConnectionString = connectionString
                    };

                    if (connectionBuilder.ContainsKey("Endpoint") && Uri.TryCreate(connectionBuilder["Endpoint"].ToString(), UriKind.Absolute, out var endpoint))
                    {
                        connectionString = endpoint.ToString();
                    }
                }

                var _wireMockAdminApi = RestClient.For<IWireMockAdminApi>(new Uri(connectionString));

                var mappingBuilder = _wireMockAdminApi.GetMappingBuilder();
                resource.ApiMappingBuilder?.Invoke(mappingBuilder);

                await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success) });
            }
            catch (Exception ex)
            {
                await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot(ex.Message, KnownResourceStateStyles.Error) });
            }
            
        });

        return builder;
    }
}
