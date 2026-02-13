#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.StyleSheets
{
	interface IStyleSelectable
	{
		string[] NameAndBases { get; }
		string Id { get; }
		IStyleSelectable Parent { get; }
		IList<string> Classes { get; }
		IEnumerable<IStyleSelectable> Children { get; }
	}

	interface IStylable
	{
		[RequiresUnreferencedCode(StyleSheet.StyleSheetTrimmerWarning)]
		BindableProperty GetProperty(string key, bool inheriting);
	}
}