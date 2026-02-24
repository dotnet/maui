#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Controls;

/// <summary>Specifies the mapping between an XML namespace and a CLR namespace.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[DebuggerDisplay("{XmlNamespace}, {Target}, {AssemblyName}")]
public sealed class XmlnsDefinitionAttribute : Attribute
{
	/// <summary>Gets the XML namespace being mapped.</summary>
	public string XmlNamespace { get; }

	/// <summary>Gets the target CLR namespace or xmlns.</summary>
	public string Target { get; }

	/// <summary>Gets the CLR namespace.</summary>
	[Obsolete("Use Target for ClrNamespace or other xmlns")]
	public string ClrNamespace => Target;

	/// <summary>Gets or sets the assembly name containing the target namespace.</summary>
	public string AssemblyName { get; set; }

	/// <summary>Creates a new <see cref="XmlnsDefinitionAttribute"/>.</summary>
	/// <param name="xmlNamespace">The XML namespace.</param>
	/// <param name="target">The target CLR namespace or xmlns.</param>
	public XmlnsDefinitionAttribute(string xmlNamespace, string target)
	{
		//TODO we need an analyzer to check before runtime
		if (target == "http://schemas.microsoft.com/winfx/2009/xaml" || target == "http://schemas.microsoft.com/winfx/2006/xaml")
			throw new ArgumentException($"Target cannot be {target}. That namespace can't be aggregated", nameof(target));

		if (target.StartsWith("http", StringComparison.Ordinal) && xmlNamespace != "http://schemas.microsoft.com/dotnet/maui/global")
			throw new ArgumentException($"We only support xmlns aggregation in http://schemas.microsoft.com/dotnet/maui/global", nameof(target));
		Target = target ?? throw new ArgumentNullException(nameof(target));
		XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
	}
}
