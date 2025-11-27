using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class BindablePropertyConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["BindableProperty", "Microsoft.Maui.Controls.BindableProperty"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		if (node is not ValueNode vNode)
			return "default";
		return NodeSGExtensions.GetBindableProperty(vNode, context).ToFQDisplayString();
	}
}