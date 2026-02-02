#nullable enable
using System;

namespace Microsoft.Maui.Controls.Xaml.Internals;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class AllowImplicitXmlnsDeclarationAttribute(bool allow = true) : Attribute
{
	public bool Allow { get; } = allow;
}