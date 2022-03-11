#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.XmlnsDefinitionAttribute']/Docs" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[DebuggerDisplay("{XmlNamespace}, {ClrNamespace}, {AssemblyName}")]
#if !NETSTANDARD1_0
	public
#else
	// Certain needed reflection methods are not possible in .NET Standard 1.X
	// so the attribute needs to remain internal in that case.
	internal
#endif
		sealed class XmlnsDefinitionAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='XmlNamespace']/Docs" />
		public string XmlNamespace { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='ClrNamespace']/Docs" />
		public string ClrNamespace { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='AssemblyName']/Docs" />
		public string AssemblyName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
		{
			ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
		}
	}
}
