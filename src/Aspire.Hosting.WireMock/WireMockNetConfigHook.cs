using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using RestEase;
using WireMock.Client;
using WireMock.Client.Extensions;

namespace Aspire.Hosting.WireMock;
internal class WireMockNetConfigHook : IDistributedApplicationLifecycleHook
{
    public Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
    {
        var wireMockInstances = appModel.Resources.OfType<WireMockNetResource>();

        if (!wireMockInstances.Any())
        {
            return Task.CompletedTask;
        }

        foreach (var wireMockInstance in wireMockInstances)
        {
            if (wireMockInstance.PrimaryEndpoint.IsAllocated)
            {
                var _wireMockAdminApi =
                    RestClient.For<IWireMockAdminApi>(new Uri(wireMockInstance.PrimaryEndpoint.Url, UriKind.Absolute));
                var mappingBuilder = _wireMockAdminApi.GetMappingBuilder();
                wireMockInstance.ApiMappingBuilder?.Invoke(mappingBuilder);
            }
        }

        return Task.CompletedTask;
    }
}
