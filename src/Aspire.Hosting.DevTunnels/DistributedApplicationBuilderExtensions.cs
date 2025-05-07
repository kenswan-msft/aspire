// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace Aspire.Hosting.DevTunnels;

/// <summary>
/// Adds support for dev tunnels in the distributed application builder.
/// </summary>
public static class DistributedApplicationBuilderExtensions
{
    /// <summary>
    /// Adds support for dev tunnels to the distributed application builder.
    /// </summary>
    /// <remarks>
    /// This method configures the necessary lifecycle hooks to enable development tunnels within the
    /// distributed application. It should be called during the application setup phase to ensure proper
    /// initialization.
    /// </remarks>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to which dev tunnel support will be added.</param>
    /// <returns>The <see cref="IDistributedApplicationBuilder"/> instance with dev tunnel support configured.</returns>
    public static IDistributedApplicationBuilder AddDevTunnels(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook(services =>
            new DevTunnelLifecycleHook(builder, services.GetRequiredService<ResourceNotificationService>()));

        return builder;
    }
}
