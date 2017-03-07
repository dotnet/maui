using System;
using System.Diagnostics;


namespace Xamarin.Forms.Internals
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[DebuggerDisplay("{XmlNamespace}, {ClrNamespace}, {AssemblyName}")]
	sealed class XmlnsDefinitionAttribute : Attribute
	{
		public string XmlNamespace { get; }
		public string ClrNamespace { get; }
		public string AssemblyName { get; set; }

		public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
		{
			if (xmlNamespace == null)
				throw new ArgumentNullException(nameof(xmlNamespace));
			if (clrNamespace == null)
				throw new ArgumentNullException(nameof(clrNamespace));

			ClrNamespace = clrNamespace;
			XmlNamespace = xmlNamespace;
		}
	}
}