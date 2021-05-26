using System;

namespace Microsoft.Maui
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class XmlnsPrefixAttribute : Attribute
	{
		public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
		{
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
		}

		public string XmlNamespace { get; }
		public string Prefix { get; }
	}
}
