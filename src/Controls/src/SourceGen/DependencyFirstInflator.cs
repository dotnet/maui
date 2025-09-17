using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen;

class DependencyFirstInflator
{
	record InflatorProperty(ITypeSymbol Type, string name) : ILocalVariable
	{
		public string Name => $"inflator.{name}";
	}

	static readonly IList<XmlName> skips = [
		XmlName.xArguments,
		XmlName.xClass,
		XmlName.xDataType,
		XmlName.xFactoryMethod,
		XmlName.xFieldModifier,
		XmlName.xKey,
		XmlName.xName,
		XmlName.xTypeArguments,
	];

	public void Inflate(SourceGenContext context, ElementNode root, IndentedTextWriter writer)
	{
		var thisVar = new LocalVariable((INamedTypeSymbol)context.RootType, "this");
		writer.WriteLine($"var inflator = new {context.RootType.Name}Inflator();");

		//set the properties on the root control, and create all the derred objects on the refstructwriter
		var refStructWriter = context.RefStructWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = 1 };
		Dictionary<ElementNode, string> xNameElements = [];
		SetValuesForNode(root, (writer, refStructWriter), thisVar, context, xNameElements);
		foreach (var kvp in xNameElements)
		{
			writer.WriteLine($"{thisVar.Name}.{EscapeIdentifier(kvp.Value)} = {context.Variables[kvp.Key].Name};");
		}
	}

	void SetValuesForNode(ElementNode node, (IndentedTextWriter SetValue, IndentedTextWriter PropertiesWriter) writers, ILocalVariable parentVar, SourceGenContext context, Dictionary<ElementNode, string> xNameElements)
	{
		foreach (var prop in node.Properties)
		{
			if (prop.Key == XmlName.xName && prop.Value is ValueNode valueNode && valueNode.Value is string xName)
			{
				xNameElements[node] = xName;
				continue;
			}
			if (skips.Contains(prop.Key))
				continue;

			SetPropertyValue(prop, writers, parentVar, context, xNameElements);
		}

		var contentPropertyName = node.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)?.GetContentPropertyName(context);
		if (node.CollectionItems != null && node.CollectionItems.Count > 0 && contentPropertyName != null)
		{
			foreach (var child in node.CollectionItems)
			{
				var prop = new KeyValuePair<XmlName, INode>(new XmlName(null, contentPropertyName), child);
				SetPropertyValue(prop, writers, parentVar, context, xNameElements);
			}
		}
	}

	void SetPropertyValue(KeyValuePair<XmlName, INode> prop, (IndentedTextWriter SetValue, IndentedTextWriter PropertiesWriter) writers, ILocalVariable parentVar, SourceGenContext context, Dictionary<ElementNode, string> xNameElements)
	{
		if (!CanBeSetDirectly(prop, context))
		{
			if (prop.Value is not ElementNode elementNode)
				throw new NotImplementedException();
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol type)
				throw new NotImplementedException();

			//let's create this code
			//
			// 	public global::Microsoft.Maui.Controls.Button Button  {
			// 		get {
			//	 		if (field != null)
			// 				return field;
			// 			field = Create(this);
			// 			SetProperties(this);
			// 			return field;

			// 			static global::Microsoft.Maui.Controls.Button Create(TestPageInflator inflator) {
			// 				var local = new global::Microsoft.Maui.Controls.Button();
			// 				global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(local!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 8, 5);
			// 				return local;
			// 			}

			// 			static void SetProperties(Button @field, TestPageInflator inflator) {
			// 				@field.SetValue(global::Microsoft.Maui.Controls.Button.TextProperty, "Hello MAUI!");
			// 			}
			//		}
			// }

			var property = new InflatorProperty(type, NamingHelpers.CreateUniqueVariableName(context, type));
			context.Variables.Add(elementNode, property);

			//create one write per property, to flush them at once
			var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.PropertiesWriter.Indent };

			using (PrePost.NewBlock(writer, $"public {type.ToFQDisplayString()} {property.name}  {{", "}"))
			using (PrePost.NewBlock(writer, "get {", "}"))
			{
				writer.WriteLine($"if (field != null)");
				writer.Indent++;
				writer.WriteLine($"return field;");
				writer.Indent--;
				writer.WriteLine($"field = Create(this);");
				writer.WriteLine($"SetProperties(field, this);");
				writer.WriteLine($"return field;");
				writer.WriteLine();

				using (PrePost.NewBlock(writer, $"static {type.ToFQDisplayString()} Create({context.RootType.Name}Inflator inflator) {{", "}"))
				{
					CreateValuesVisitor.CreateValue((ElementNode)elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context);
					var localVar = context.Variables[elementNode];
					writer.WriteLine($"return {localVar.Name};");
				}
				context.Variables[elementNode] = property; //replace the variable with the property
				writer.WriteLine();

				using (PrePost.NewBlock(writer, $"static void SetProperties({type.ToFQDisplayString()} @field, {context.RootType.Name}Inflator inflator) {{", "}"))
				{
					//replace the variable with a local var, to avoid using the property accessor
					var localVar = context.Variables[elementNode];
					SetValuesForNode(elementNode, (writer, writers.PropertiesWriter), context.Variables[elementNode] = new LocalVariable(type, "@field"), context, xNameElements);
					context.Variables[elementNode] = localVar;
				}
			}
			writers.PropertiesWriter.Append(writer, noTabs: true);
		}
		SetPropertyHelpers.SetPropertyValue(writers.SetValue, parentVar, prop.Key, prop.Value, context);
	}

	bool CanBeSetDirectly(KeyValuePair<XmlName, INode> prop, SourceGenContext context)
	{
		if (prop.Value is ValueNode)
			return true;

		return false;
	}
}