using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Distinguishes between different states of x:DataType resolution on a node.
/// </summary>
enum BindingContextDataTypeKind
{
	/// <summary>x:DataType resolved to a known type.</summary>
	Resolved,

	/// <summary>x:DataType="{x:Null}" — explicit opt-out of compiled bindings for this scope.</summary>
	ExplicitNull,

	/// <summary>x:DataType is present but couldn't be resolved (diagnostic already reported).</summary>
	Unresolved,
}

/// <summary>
/// Represents the resolved x:DataType for a node in the XAML tree.
/// Pre-computed by <see cref="PropagateDataTypeVisitor"/> and stored in
/// <see cref="SourceGenContext.BindingContextDataTypes"/>.
/// </summary>
/// <param name="Kind">The resolution state.</param>
/// <param name="Symbol">The resolved type symbol. Non-null only when <paramref name="Kind"/> is <see cref="BindingContextDataTypeKind.Resolved"/>.</param>
/// <param name="Origin">The node where x:DataType was declared (for diagnostics). Null when inherited from a parent context.</param>
/// <param name="IsInherited">True if the data type was inherited from an ancestor, false if declared on this node.</param>
/// <param name="CrossedTemplateBoundary">True if the data type was inherited across a DataTemplate boundary (likely a bug).</param>
record BindingContextDataType(
	BindingContextDataTypeKind Kind,
	ITypeSymbol? Symbol,
	INode? Origin,
	bool IsInherited,
	bool CrossedTemplateBoundary);
