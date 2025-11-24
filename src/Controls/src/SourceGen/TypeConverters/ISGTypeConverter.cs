using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

/// <summary>
/// Interface for source generator type converters.
/// </summary>
interface ISGTypeConverter
{
	/// <summary>
	/// Converts a string value from XAML to the corresponding C# code generation string.
	/// </summary>
	/// <param name="value">The string value from XAML to convert</param>
	/// <param name="node">The XML node for diagnostic location information</param>
	/// <param name="toType">The target type symbol for the conversion</param>
	/// <param name="context">The source generation context</param>
	/// <param name="parentVar">Optional parent variable context</param>
	/// <returns>Generated C# code string, or "default" if conversion fails</returns>
	string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null);

	/// <summary>
	/// Gets the type names this converter can handle (for registration/lookup).
	/// </summary>
	IEnumerable<string> SupportedTypes { get; }
}