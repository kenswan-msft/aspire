// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace Aspire.Hosting.DevTunnels;

internal class DevTunnelLifecycleHook(
    IDistributedApplicationBuilder distributedApplicationBuilder,
    ResourceNotificationService notificationService) : IDistributedApplicationLifecycleHook
{
    public async Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        foreach (var resource in appModel.Resources.OfType<IResourceWithEndpoints>().ToList())
        {
            var devTunnelResourceBuilder = distributedApplicationBuilder.CreateResourceBuilder(new DevTunnelResource($"{resource.Name}-devtunnel")
            {
                Parent = resource
            });

            devTunnelResourceBuilder.WithParentRelationship(resource);
            devTunnelResourceBuilder.WithInitialState(new()
            {
                ResourceType = "DevTunnel",
                // State = KnownResourceStates.Hidden,
                State = KnownResourceStates.NotStarted,
                Properties = []
            });

            appModel.Resources.Add(devTunnelResourceBuilder.Resource);

            var resourceBuilder = distributedApplicationBuilder.CreateResourceBuilder(resource);

            resourceBuilder.WithCommand("devtunnel", "Start dev tunnel", async context =>
            {
                await notificationService.PublishUpdateAsync(devTunnelResourceBuilder.Resource, snapshot => snapshot with { State = KnownResourceStates.Starting }).ConfigureAwait(false);

                // TODO: Add Start Logic here

                await notificationService.PublishUpdateAsync(devTunnelResourceBuilder.Resource, snapshot => snapshot with { State = KnownResourceStates.Running }).ConfigureAwait(false);
                await notificationService.PublishUpdateAsync(resource, snapshot =>
                    snapshot with { Urls = snapshot.Urls.Add(new(null, "https://some-dev-tunnel.azure.com:1234", false)) }).ConfigureAwait(false);

                return new() { Success = true };
            }, new());

            // Pump an update to force the UI to refresh
            await notificationService.PublishUpdateAsync(resource, snapshot => snapshot).ConfigureAwait(false);
        }
    }
}
