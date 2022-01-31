using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsPrefixAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.XmlnsPrefixAttribute']/Docs" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class XmlnsPrefixAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsPrefixAttribute.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
		{
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsPrefixAttribute.xml" path="//Member[@MemberName='XmlNamespace']/Docs" />
		public string XmlNamespace { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/XmlnsPrefixAttribute.xml" path="//Member[@MemberName='Prefix']/Docs" />
		public string Prefix { get; }
	}
}