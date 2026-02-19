#nullable enable
using System;

namespace Microsoft.Maui.Controls.Xaml.Internals;

#if !NET12_0_OR_GREATER
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
[Obsolete("Implicit xmlns declarations are now always enabled. This attribute is no longer needed and will be removed in .NET 12.")]
public sealed class AllowImplicitXmlnsDeclarationAttribute(bool allow = true) : Attribute
{
	public bool Allow { get; } = allow;
}
#endif