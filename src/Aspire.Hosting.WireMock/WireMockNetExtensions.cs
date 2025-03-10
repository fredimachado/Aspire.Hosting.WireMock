using Aspire.Hosting.WireMock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RestEase;
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
            .WithHttpHealthCheck("/__admin/mappings")
            .WithOpenInspectorCommand();
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
                if (!resource.PrimaryEndpoint.IsAllocated)
                {
                    await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot("Endpoint is not allocated.", KnownResourceStateStyles.Error) });
                    return;
                }

                await notificationService.WaitForResourceHealthyAsync(resource.Name, cancellationToken);

                var wireMockAdminApi = RestClient.For<IWireMockAdminApi>(new Uri(resource.PrimaryEndpoint.Url, UriKind.Absolute));
                var mappingBuilder = wireMockAdminApi.GetMappingBuilder();
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

    /// <summary>
    /// Enables the WireMockInspect, a cross-platform UI app that facilitates WireMock troubleshooting.
    /// This requires installation of the WireMockInspector tool.
    /// <code>
    /// dotnet tool install WireMockInspector --global --no-cache --ignore-failed-sources
    /// </code>
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{WireMockNetResource}"/>.</param>
    /// <returns></returns>
    private static IResourceBuilder<WireMockNetResource> WithOpenInspectorCommand(this IResourceBuilder<WireMockNetResource> builder)
    {
        builder.WithCommand(
            name: "open-inspector",
            displayName: "Open Inspector",
            displayDescription: "Requires installation of the WireMockInspector tool:\ndotnet tool install WireMockInspector --global --no-cache --ignore-failed-sources",
            executeCommand: context => OnRunOpenInspectorCommandAsync(builder),
            updateState: OnUpdateResourceState,
            iconName: "BoxSearch",
            iconVariant: IconVariant.Filled);

        return builder;
    }

    private static Task<ExecuteCommandResult> OnRunOpenInspectorCommandAsync(IResourceBuilder<WireMockNetResource> builder)
    {
        WireMockInspector.Inspect(builder.Resource.PrimaryEndpoint.Url);

        return Task.FromResult(CommandResults.Success());
    }

    private static ResourceCommandState OnUpdateResourceState(UpdateCommandStateContext context)
    {
        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }
}
