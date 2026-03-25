using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Interface for known markup types that can provide inline value initialization.
/// Separates the "can we inline?" check from the actual inlining logic.
/// </summary>
internal interface IKnownMarkupValueProvider
{
	/// <summary>
	/// Determines if this element can be fully inlined without requiring
	/// property assignments or service provider infrastructure.
	/// </summary>
	/// <param name="node">The element node to check</param>
	/// <param name="context">The source generation context</param>
	/// <returns>True if the element can be inlined, false otherwise</returns>
	bool CanProvideValue(ElementNode node, SourceGenContext context);

	/// <summary>
	/// Provides the inline value initialization code.
	/// </summary>
	/// <param name="node">The element node</param>
	/// <param name="writer">The code writer</param>
	/// <param name="context">The source generation context</param>
	/// <param name="getNodeValue">Delegate to get node values</param>
	/// <param name="returnType">The return type of the value</param>
	/// <param name="value">The generated value code</param>
	/// <returns>True if value was provided, false otherwise</returns>
	bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value);
}
