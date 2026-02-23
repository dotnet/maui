#nullable enable
using System;
using System.Runtime.Versioning;

namespace Microsoft.Maui.Controls.Xaml.Internals;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
#if !NETSTANDARD
[RequiresPreviewFeatures]
#endif
public sealed class AllowImplicitXmlnsDeclarationAttribute(bool allow = true) : Attribute
{
	public bool Allow { get; } = allow;
}