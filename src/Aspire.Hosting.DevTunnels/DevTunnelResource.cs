// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.DevTunnels;

internal class DevTunnelResource(string name) : ExecutableResource(name, "devtunnel", "")
{
    /// <summary>
    /// Reports DevTunnel resource initialization status.
    /// </summary>
    public bool IsInitialized { get; internal set; }

    /// <summary>
    /// Reports DevTunnel resource public access status.
    /// </summary>
    public bool IsPublic { get; internal set; }

    /// <summary>
    /// Parent resource of the attached DevTunnel resource.
    /// </summary>
    public required IResourceWithEndpoints Parent { get; init; }
}
