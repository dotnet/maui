using System;
using System.Diagnostics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a mapping between an XML namespace and a CLR namespace.
	/// </summary>
	/// <remarks>
	/// This attribute is used to map XML namespaces to CLR namespaces for XAML processing.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[DebuggerDisplay("{XmlNamespace}, {ClrNamespace}")]
	internal sealed class XmlnsDefinitionAttribute : Attribute
	{
		/// <summary>
		/// Gets the XML namespace.
		/// </summary>
		public string XmlNamespace { get; }

		/// <summary>
		/// Gets the CLR namespace.
		/// </summary>
		public string ClrNamespace { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlnsDefinitionAttribute"/> class.
		/// </summary>
		/// <param name="xmlNamespace">The XML namespace.</param>
		/// <param name="clrNamespace">The CLR namespace.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="xmlNamespace"/> or <paramref name="clrNamespace"/> is null.</exception>
		public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
		{
			ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
		}
	}
}
