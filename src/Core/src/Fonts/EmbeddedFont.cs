#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/EmbeddedFont.xml" path="Type[@FullName='Microsoft.Maui.EmbeddedFont']/Docs" />
	public class EmbeddedFont
	{
		/// <include file="../../docs/Microsoft.Maui/EmbeddedFont.xml" path="//Member[@MemberName='FontName']/Docs" />
		public string? FontName { get; set; }
		/// <include file="../../docs/Microsoft.Maui/EmbeddedFont.xml" path="//Member[@MemberName='ResourceStream']/Docs" />
		public Stream? ResourceStream { get; set; }
	}
}
