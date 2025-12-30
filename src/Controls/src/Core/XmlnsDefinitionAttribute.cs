#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Controls;

/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.XmlnsDefinitionAttribute']/Docs/*" />
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[DebuggerDisplay("{XmlNamespace}, {Target}, {AssemblyName}")]
public sealed class XmlnsDefinitionAttribute : Attribute
{
	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='XmlNamespace']/Docs/*" />
	public string XmlNamespace { get; }

	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='Target']/Docs/*" />
	public string Target { get; }
	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='ClrNamespace']/Docs/*" />
	[Obsolete("Use Target for ClrNamespace or other xmlns")]
	public string ClrNamespace => Target;
	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='AssemblyName']/Docs/*" />
	public string AssemblyName { get; set; }

	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
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
