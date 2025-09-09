using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

internal interface IKnownMarkupValueProvider
{
	bool CanProvideValue(ElementNode node, SourceGenContext context);

	bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate? tryGetNodeValue, out ITypeSymbol? returnType, out string value);
}
