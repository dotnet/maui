#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies a prefix for an XML namespace when serializing XAML.</summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class XmlnsPrefixAttribute : Attribute
	{
		/// <summary>Creates a new <see cref="XmlnsPrefixAttribute"/>.</summary>
		/// <param name="xmlNamespace">The XML namespace.</param>
		/// <param name="prefix">The prefix to use for the namespace.</param>
		public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
		{
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
		}

		/// <summary>Gets the XML namespace.</summary>
		public string XmlNamespace { get; }

		/// <summary>Gets the prefix for the XML namespace.</summary>
		public string Prefix { get; }
	}
}