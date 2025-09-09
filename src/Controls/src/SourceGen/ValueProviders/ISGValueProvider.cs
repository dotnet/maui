using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.Maui.Controls.Xaml;

using static Microsoft.Maui.Controls.SourceGen.NodeSGExtensions;

namespace Microsoft.Maui.Controls.SourceGen;

interface ISGValueProvider
{
	bool CanProvideValue(ElementNode node, SourceGenContext context, TryGetNodeValueDelegate tryGetNodeValue);

	DirectValue ProvideDirectValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, TryGetNodeValueDelegate tryGetNodeValue);

	ILocalValue ProvideValue(ElementNode elementNode, (IndentedTextWriter declarationWriter, IndentedTextWriter? methodWriter) writers, ImmutableArray<Scope> scopes, SourceGenContext context, TryGetNodeValueDelegate tryGetNodeValue);
}
