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

            devTunnelResourceBuilder
                .WithParentRelationship(resource)
                .WithExplicitStart()
                .WithArgs(["host"]);

            devTunnelResourceBuilder.WithInitialState(new()
            {
                ResourceType = "dev tunnel",
                IsHidden = true,
                Properties = []
            });

            // TODO: This should be handled by native start. May be missing a hook somewhere from orchestration to enable.
            devTunnelResourceBuilder
               .WithCommand("Start", "Start Dev Tunnel", async (context) =>
               {

                   await notificationService.PublishUpdateAsync(devTunnelResourceBuilder.Resource, snapshot => snapshot with { State = KnownResourceStates.Starting }).ConfigureAwait(false);

                   await Task.Delay(3000).ConfigureAwait(false);

                   await notificationService.PublishUpdateAsync(devTunnelResourceBuilder.Resource, snapshot => snapshot with { State = KnownResourceStates.Running }).ConfigureAwait(false);

                   //await notificationService.PublishUpdateAsync(resource, snapshot =>
                   //    snapshot with { Urls = snapshot.Urls.Add(new(null, "https://some-dev-tunnel.azure.com:1234", false)) }).ConfigureAwait(false);

                   return new ExecuteCommandResult { Success = true };

               }, commandOptions: new CommandOptions
               {
                   IconName = "Play",
                   UpdateState = (updateState) =>
                   {
                       return updateState.ResourceSnapshot.State?.Text == "Running" ? ResourceCommandState.Disabled : ResourceCommandState.Enabled;
                   },
               });

            appModel.Resources.Add(devTunnelResourceBuilder.Resource);

            var resourceBuilder = distributedApplicationBuilder.CreateResourceBuilder(resource);

            resourceBuilder.WithCommand("show-devtunnel", "Show DevTunnel", async context =>
            {
                await notificationService.PublishUpdateAsync(devTunnelResourceBuilder.Resource, snapshot => snapshot with { State = KnownResourceStates.NotStarted, IsHidden = false }).ConfigureAwait(false);

                // TODO: Fix child resource showing underneath parent resource in UI

                return new() { Success = true };
            }, commandOptions: new CommandOptions
            {
                IconName = "Apps"
            });

            // Push an update to force the UI to refresh
            await notificationService.PublishUpdateAsync(resource, snapshot => snapshot).ConfigureAwait(false);
        }
    }
}
