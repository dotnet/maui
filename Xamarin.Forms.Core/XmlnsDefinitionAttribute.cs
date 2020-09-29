using System;
using System.Diagnostics;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[DebuggerDisplay("{XmlNamespace}, {ClrNamespace}, {AssemblyName}")]
#if NETSTANDARD2_0
	public
#else
	// Certain needed reflection methods are not possible in .NET Standard 1.X
	// so the attribute needs to remain internal in that case.
	internal
#endif
		sealed class XmlnsDefinitionAttribute : Attribute
	{
		public string XmlNamespace { get; }
		public string ClrNamespace { get; }
		public string AssemblyName { get; set; }

		public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
		{
			ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
			XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
		}
	}
}