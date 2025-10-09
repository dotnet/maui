using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.NodeSGExtensions;

namespace Microsoft.Maui.Controls.SourceGen.ValueProviders;

interface ISGValueProvider
{
	bool CanProvideValue(ElementNode node, SourceGenContext context, TryGetNodeValueDelegate getNodeValue);

	DirectValue ProvideDirectValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, TryGetNodeValueDelegate getNodeValue);

	ILocalValue ProvideValue(ElementNode elementNode, (IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context, TryGetNodeValueDelegate getNodeValue);
}
