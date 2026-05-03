using System;

namespace Microsoft.Maui;

/// <summary>
/// Marks a public instance method as invokable from JavaScript via HybridWebView.
/// Only methods decorated with this attribute can be called from JavaScript using <c>HybridWebView.InvokeDotNet()</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class HybridJSInvokableAttribute : Attribute
{
}
