using System;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="Type[@FullName='Microsoft.Maui.KeyboardFlags']/Docs/*" />
	[Flags]
	public enum KeyboardFlags
	{
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='None']/Docs/*" />
		None = 0,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='CapitalizeSentence']/Docs/*" />
		CapitalizeSentence = 1,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='Spellcheck']/Docs/*" />
		Spellcheck = 1 << 1,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='Suggestions']/Docs/*" />
		Suggestions = 1 << 2,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='CapitalizeWord']/Docs/*" />
		CapitalizeWord = 1 << 3,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='CapitalizeCharacter']/Docs/*" />
		CapitalizeCharacter = 1 << 4,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='CapitalizeNone']/Docs/*" />
		CapitalizeNone = 1 << 5,
		/// <include file="../../docs/Microsoft.Maui/KeyboardFlags.xml" path="//Member[@MemberName='All']/Docs/*" />
		All = ~0
	}
}