using System;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/TextDecorations.xml" path="Type[@FullName='Microsoft.Maui.TextDecorations']/Docs/*" />
	[Flags]
	public enum TextDecorations
	{
		/// <include file="../../docs/Microsoft.Maui/TextDecorations.xml" path="//Member[@MemberName='None']/Docs/*" />
		None = 0,
		/// <include file="../../docs/Microsoft.Maui/TextDecorations.xml" path="//Member[@MemberName='Underline']/Docs/*" />
		Underline = 1 << 0,
		/// <include file="../../docs/Microsoft.Maui/TextDecorations.xml" path="//Member[@MemberName='Strikethrough']/Docs/*" />
		Strikethrough = 1 << 1,
	}
}
