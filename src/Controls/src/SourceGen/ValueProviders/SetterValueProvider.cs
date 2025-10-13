using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System;
using Microsoft.Maui.Controls.Xaml;

using static Microsoft.Maui.Controls.SourceGen.DependencyFirstInflator;
using static Microsoft.Maui.Controls.SourceGen.NodeSGExtensions;
using System.Linq;

namespace Microsoft.Maui.Controls.SourceGen.ValueProviders;

class SetterValueProvider : ISGValueProvider
{
	public bool CanProvideValue(ElementNode node, SourceGenContext context, TryGetNodeValueDelegate getNodeValue)
		=> true;

	public DirectValue ProvideDirectValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, TryGetNodeValueDelegate tryGetNodeValue)
	{
		var returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!;

		if (!node.Properties.TryGetValue(new XmlName("", "Value"), out INode? valueNode) &&
			!node.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Value"), out valueNode) &&
			node.CollectionItems.Count == 1)
			valueNode = node.CollectionItems[0];

		var bpNode = (ValueNode)node.Properties[new XmlName("", "Property")];
		var bpRef = bpNode.GetBindableProperty(context);
		var bprefType = bpRef.GetBPTypeAndConverter(context)?.type;

		string targetsetter;
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode))
			targetsetter = $"TargetName = \"{((ValueNode)targetNode).Value}\", ";
		else
			targetsetter = "";

		if (valueNode is ValueNode vn)
			return new DirectValue(returnType, $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {vn.ConvertTo(bpRef, writer, context)}}}");

		tryGetNodeValue(valueNode, bprefType!, out var lvalue);
		return new DirectValue(returnType, $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {lvalue!.ValueAccessor}}}");
	}

	public ILocalValue ProvideValue(ElementNode node, (IndentedTextWriter declarationWriter, IndentedTextWriter? methodWriter) writers, ImmutableArray<Scope> scopes, SourceGenContext context, TryGetNodeValueDelegate tryGetNodeValue)
	{
		var type = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!;
		if (!node.Properties.TryGetValue(new XmlName("", "Value"), out INode? valueNode) &&
			!node.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Value"), out valueNode) &&
			node.CollectionItems.Count == 1)
			valueNode = node.CollectionItems[0];

		var bpNode = (ValueNode)node.Properties[new XmlName("", "Property")];
		var bpRef = bpNode.GetBindableProperty(context);
		var bprefType = bpRef.GetBPTypeAndConverter(context)?.type;

		string targetsetter;
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode))
			targetsetter = $"TargetName = \"{((ValueNode)targetNode).Value}\", ";
		else
			targetsetter = "";

		var inflatorscope = scopes.GetInflatorScope();

		var property = new ScopedVariable(type, NamingHelpers.CreateUniqueVariableName(context, type), inflatorscope);
		PreserveNodeValue(node, property, context);
				//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.declarationWriter.Indent };
		if (scopes.Last() is StaticMethodScope sms)
			scopes = scopes.RemoveRange(scopes.Length - 2, 2);
		scopes = scopes.Add(new PropertyScope(writer, type, property.Name));
		writer.WriteLineNoTabs();

		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		using (PrePost.NewBlock(writer, $"public {type.ToFQDisplayString()} {property.Name}  {{", "}"))
		using (PrePost.NewBlock(writer, "get {", "}"))
		{
			writer.WriteLine($"if (field != null)");
			writer.Indent++;
			writer.WriteLine($"return field;");
			writer.Indent--;
			writer.WriteLine($"field = new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}}};");
			var localVar = GetNodeValue(valueNode, scopes, context, bprefType!);
			if (localVar is ScopedVariable sv)
				localVar = sv.AccessedFrom(scopes);
			writer.WriteLine($"field.Value = {localVar?.ValueAccessor};");

			writer.WriteLine($"return field;");
		}
		writers.declarationWriter.Append(writer, noTabs: true);
		return property;
    }
}