#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.XmlnsDefinitionAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[DebuggerDisplay("{XmlNamespace}, {ClrNamespace}, {AssemblyName}")]
	public sealed class XmlnsDefinitionAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='XmlNamespace']/Docs/*" />
		public string XmlNamespace { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='ClrNamespace']/Docs/*" />
		public string ClrNamespace { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='AssemblyName']/Docs/*" />
		public string AssemblyName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsDefinitionAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
		{
			ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
		}
	}
}
