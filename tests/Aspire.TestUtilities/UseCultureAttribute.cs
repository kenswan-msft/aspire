// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// The original code was borrowed from https://github.com/xunit/samples.xunit/blob/main/v3/UseCultureExample/UseCultureAttributeTests.cs
// Licensed under http://www.apache.org/licenses/LICENSE-2.0.

using System.Globalization;
using System.Reflection;

namespace Aspire.TestUtilities;

/// <summary>
/// Apply this attribute to your test method to replace the
/// <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
/// <see cref="CultureInfo.CurrentUICulture" /> with another culture.
/// </summary>
/// <remarks>
/// Replaces the culture and UI culture of the current thread with
/// <paramref name="culture" /> and <paramref name="uiCulture" />
/// </remarks>
/// <param name="culture">The name of the culture.</param>
/// <param name="uiCulture">The name of the UI culture.</param>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class UseCultureAttribute(string culture, string uiCulture) : BeforeAfterTestAttribute
{
    private readonly Lazy<CultureInfo> _culture = new(() => new(culture, useUserOverride: false));
    private readonly Lazy<CultureInfo> _uiCulture = new(() => new(uiCulture, useUserOverride: false));

    private CultureInfo? _originalCulture;
    private CultureInfo? _originalUICulture;

    /// <summary>
    /// Gets the culture.
    /// </summary>
    public CultureInfo Culture => _culture.Value;

    /// <summary>
    /// Gets the UI culture.
    /// </summary>
    public CultureInfo UICulture => _uiCulture.Value;

    /// <summary>
    /// Replaces the culture and UI culture of the current thread with
    /// <paramref name="culture" />
    /// </summary>
    /// <param name="culture">The name of the culture.</param>
    /// <remarks>
    /// This constructor overload uses <paramref name="culture" /> for both
    /// <see cref="Culture" /> and <see cref="UICulture" />.
    /// </remarks>
    public UseCultureAttribute(string culture)
        : this(culture, culture)
    {
    }

    /// <summary>
    /// Stores the current <see cref="Thread.CurrentPrincipal" />
    /// <see cref="CultureInfo.CurrentCulture" /> and <see cref="CultureInfo.CurrentUICulture" />
    /// and replaces them with the new cultures defined in the constructor.
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        _originalCulture = Thread.CurrentThread.CurrentCulture;
        _originalUICulture = Thread.CurrentThread.CurrentUICulture;

        CultureInfo.DefaultThreadCurrentCulture = Culture;
        CultureInfo.DefaultThreadCurrentUICulture = UICulture;

        Thread.CurrentThread.CurrentCulture = Culture;
        Thread.CurrentThread.CurrentUICulture = UICulture;

        CultureInfo.CurrentCulture.ClearCachedData();
        CultureInfo.CurrentUICulture.ClearCachedData();

        base.Before(methodUnderTest, test);
    }

    /// <summary>
    /// Restores the original <see cref="CultureInfo.CurrentCulture" /> and
    /// <see cref="CultureInfo.CurrentUICulture" /> to <see cref="Thread.CurrentPrincipal" />
    /// </summary>
    /// <param name="methodUnderTest">The method under test</param>
    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        if(_originalCulture is not null)
        {
            Thread.CurrentThread.CurrentCulture = _originalCulture;
        }

        if (_originalUICulture is not null)
        {
            Thread.CurrentThread.CurrentUICulture = _originalUICulture;
        }

        CultureInfo.CurrentCulture.ClearCachedData();
        CultureInfo.CurrentUICulture.ClearCachedData();

        base.After(methodUnderTest, test);
    }
}
