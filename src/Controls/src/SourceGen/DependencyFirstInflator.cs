using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen;

class DependencyFirstInflator
{
	static readonly IList<XmlName> skips = [
		XmlName.xArguments,
		XmlName.xClass,
		XmlName.xDataType,
		XmlName.xFactoryMethod,
		XmlName.xFieldModifier,
		XmlName.xKey,
		XmlName.xName,
		XmlName.xTypeArguments,
		XmlName.mcIgnorable,
	];

	public void Inflate(SourceGenContext context, ElementNode root, IndentedTextWriter writer)
	{
		var thisVar = new LocalVariable((INamedTypeSymbol)context.RootType, "this");
		context.Variables.Add(root, new InflatorProperty(context.RootType, "__root"));
		writer.WriteLine($"var inflator = new {context.RootType.Name}Inflator() {{ __root = {thisVar.ValueAccessor} }};");

		//a writer for writing calls that have to happen in InitializeCompnent, like setting xnames, connecting event handlers, ...
		var icWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writer.Indent };

		//set the properties on the root control, and create all the derred objects on the refstructwriter
		var refStructWriter = context.RefStructWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t");
		using (PrePost.NewBlock(refStructWriter, $"file ref struct {context.RootType.Name}Inflator() {{", "}"))
		{
			refStructWriter.WriteLine($"public required {context.RootType.ToFQDisplayString()} __root {{ get; init; }}");
			refStructWriter.WriteLineNoTabs("");
			SetValuesForNode(root, (writer, refStructWriter, icWriter), thisVar, thisVar, context, []);
		}

		writer.WriteLineNoTabs("");
		writer.Append(icWriter, noTabs: true);
	}

	static void SetValuesForNode(ElementNode node, (IndentedTextWriter SetValue, IndentedTextWriter PropertiesWriter, IndentedTextWriter ICWriter) writers, ILocalValue parentVar, ILocalValue localVar, SourceGenContext context, Dictionary<ElementNode, string> xNameElements)
	{
		var properties = new List<KeyValuePair<XmlName, INode>>();
		properties.AddRange(node.Properties.Where(p => !skips.Contains(p.Key)));

		var xNameProp = node.Properties.FirstOrDefault(p => p.Key == XmlName.xName);
		if (xNameProp.Value is ValueNode valueNode && valueNode.Value is string xName)
			writers.ICWriter.WriteLine($"this.{EscapeIdentifier(xName)} = {parentVar.ValueAccessor};");

		var contentPropertyName = node.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)?.GetContentPropertyName(context);
		if (   node.CollectionItems != null
			&& node.CollectionItems.Count > 0
			&& contentPropertyName != null)
			properties.AddRange(node.CollectionItems.Select(child => new KeyValuePair<XmlName, INode>(new XmlName(null, contentPropertyName), child)));

		foreach (var prop in properties)
			SetPropertyValue(prop, writers, parentVar, localVar, context, xNameElements, parentVar);

		if (   node.CollectionItems != null
			&& node.CollectionItems.Count > 0
			&& contentPropertyName == null
			&& parentVar.Type.CanAdd(context))
        {
			foreach (var item in node.CollectionItems)
			SetPropertyValue(new KeyValuePair<XmlName, INode>(XmlName.Empty, item), writers, parentVar, localVar, context, xNameElements, parentVar, asCollectionItem: true);
        }
	}

	static void SetPropertyValue(KeyValuePair<XmlName, INode> prop, (IndentedTextWriter SetValue, IndentedTextWriter PropertiesWriter, IndentedTextWriter ICWriter) writers, ILocalValue parentVar, ILocalValue localVar, SourceGenContext context, Dictionary<ElementNode, string> xNameElements, ILocalValue inflatorVar, bool asCollectionItem = false)
	{
		if (!CanBeSetDirectly(prop, writers.PropertiesWriter, context)) //we probably could inline the content of CanBeSetDirectly here
		{
			if (prop.Value is ListNode listNode)
			{
				//if there's a single item, treat it as a property
				if (listNode.CollectionItems.Count == 1)
				{
					SetPropertyValue(new KeyValuePair<XmlName, INode>(prop.Key, listNode.CollectionItems[0]), writers, parentVar, localVar, context, xNameElements, inflatorVar);
					return;
				}
				foreach (var item in listNode.CollectionItems)
				{
					var itemProp = new KeyValuePair<XmlName, INode>(prop.Key, item);
					SetPropertyValue(itemProp, writers, parentVar, localVar, context, xNameElements, inflatorVar, asCollectionItem: true);
				}
				return;
			}
			if (prop.Value is not ElementNode elementNode)
				throw new NotImplementedException();
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol type)
				throw new NotImplementedException();

			//xArray need a special syntax
			if (type.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
				ArrayCreator(elementNode, (writers.PropertiesWriter, writers.ICWriter), context, xNameElements);
			else if (IsValueProvider(elementNode, context))
				ValueProvider(elementNode, (writers.PropertiesWriter, writers.ICWriter), context, xNameElements);
			else
				ValueCreator((INamedTypeSymbol)type, elementNode, (writers.PropertiesWriter, writers.ICWriter), context, xNameElements);
		}
		SetPropertyHelpers.SetPropertyValue(writers.SetValue, localVar, prop.Key, prop.Value, context, treeOrder: true, icWriter: writers.ICWriter, inflatorVar: inflatorVar, asCollectionItem: asCollectionItem);
	}

	static ILocalValue GetNodeValue(INode node, (IndentedTextWriter PropertiesWriter, IndentedTextWriter ICWriter) writers, SourceGenContext context, ITypeSymbol toType, Dictionary<ElementNode, string> xNameElements)
	{
		if (context.Variables.TryGetValue(node, out var localVar))
			return localVar;

		if (CanBeSetDirectly(new KeyValuePair<XmlName, INode>(new XmlName(), node), writers.PropertiesWriter, context))
		{
			if (context.Variables.TryGetValue(node, out var localVar1))
				return localVar1;
			if (node is ValueNode valueNode && valueNode.Value is string strValue)
				return new DirectValue(toType, valueNode.ConvertTo(toType, writers.PropertiesWriter, context, null));
			throw new NotImplementedException();
		}

		if (node is ElementNode elementNode)
		{
			//xArray need a special syntax
			if (toType.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
				return ArrayCreator(elementNode, writers, context, xNameElements);
			else if (IsValueProvider(elementNode, context))
				return ValueProvider(elementNode, writers, context, xNameElements);
			else
				return ValueCreator((INamedTypeSymbol)toType, elementNode, writers, context, xNameElements);
		}

		throw new NotImplementedException();
	}

	static ILocalValue ValueProvider(ElementNode elementNode, (IndentedTextWriter PropertiesWriter, IndentedTextWriter ICWriter) writers, SourceGenContext context, Dictionary<ElementNode, string> xNameElements)
	{
		var type = elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)!;

		ILocalValue getNodeValue(INode node, ITypeSymbol toType) => GetNodeValue(node, writers, context, toType!, xNameElements);

		if (   NodeSGExtensions.GetKnownLateMarkupExtensions(context).TryGetValue(type, out var provideValue)
			&& provideValue.Invoke(elementNode, writers.PropertiesWriter, context, getNodeValue, out var returnType0, out var value))
			return context.Variables[elementNode] = new DirectValue(returnType0!, value);

		if (   NodeSGExtensions.GetKnownValueProviders(context).TryGetValue(type, out provideValue)
			&& provideValue.Invoke(elementNode, writers.PropertiesWriter, context, getNodeValue, out returnType0, out value))
			return context.Variables[elementNode] = new DirectValue(returnType0!, value);

		type.IsValueProvider(context, out var returnType, out var iface, out var acceptEmptyServiceProvider, out var requiredServices);

		if (acceptEmptyServiceProvider)
			return context.Variables[elementNode] = new DirectValue(returnType!, $"new {type.ToFQDisplayString()}().ProvideValue(null)");


		//let's create something like this
		// 	public global::Microsoft.Maui.Controls.Binding binding  {
		// 		get {
		//	 		if (field != null)
		// 				return field;
		// 			var bindingExtension = Create(this);
		// 			SetProperties(bindingExtension, this);
		// 			return field = ProvideValue(bindingExtension, this);

		// 			static global::Microsoft.Maui.Controls.Xaml.BindingExtension Create(TestPageInflator inflator) {
		// 				var local = new global::Microsoft.Maui.Controls.Xaml.BindingExtension();
		// 				return local;
		// 			}
		//
		// 			static void SetProperties(global::Microsoft.Maui.Controls.Xaml.BindingExtension local, TestPageInflator inflator) {
		// 				local.Path = "Name";
		// 			}
		//
		// 			static global::Microsoft.Maui.Controls.Binding ProvideValue(global::Microsoft.Maui.Controls.Xaml.BindingExtension local, TestPageInflator inflator) {
		// 				return (global::Microsoft.Maui.Controls.Binding)local.ProvideValue(null);
		// 			}
		// 		}
		// 	}
		//

		var property = new InflatorProperty(returnType, NamingHelpers.CreateUniqueVariableName(context, returnType));
		context.Variables.Add(elementNode, property);

		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.PropertiesWriter.Indent };

		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		using (PrePost.NewBlock(writer, $"public {returnType.ToFQDisplayString()} {property.name}  {{", "}"))
		using (PrePost.NewBlock(writer, "get {", "}"))
		{
			writer.WriteLine($"if (field != null)");
			writer.Indent++;
			writer.WriteLine($"return field;");
			writer.Indent--;
			writer.WriteLine($"return field = ProvideValue(ref this);");
			writer.WriteLineNoTabs("");
			using (PrePost.NewBlock(writer, $"static {returnType.ToFQDisplayString()} ProvideValue(ref {context.RootType.Name}Inflator inflator) {{", "}"))
			{
				CreateValuesVisitor.CreateValue(elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context);
				var localVar = context.Variables[elementNode];
				SetValuesForNode(elementNode, (writer, writers.PropertiesWriter, writers.ICWriter), property, localVar, context, xNameElements);

				var serviceProviderVar = elementNode.GetOrCreateServiceProvider(writer, context, requiredServices);
				writer.WriteLine($"return ({returnType.ToFQDisplayString()}){localVar.ValueAccessor}.ProvideValue({serviceProviderVar.ValueAccessor});");
			}
		}
		writers.PropertiesWriter.Append(writer, noTabs: true);

		return context.Variables[elementNode] = property;
    }
	
	static InflatorProperty ArrayCreator(ElementNode arrayNode, (IndentedTextWriter PropertiesWriter, IndentedTextWriter ICWriter) writers, SourceGenContext context, Dictionary<ElementNode, string> xNameElements)
	{
		//let's create this code
		//
		// 	public global::System.String[] array  {
		// 		get {
		//	 		if (field != null)
		// 				return field;
		// 			field = Create(this);
		// 			return field;
		// 			static global::System.String[] Create(TestPageInflator inflator) {
		// 				var local = new global::System.String[] {
		// 					"foo",
		// 					"bar"
		// 				};
		// 				return local;
		// 			}
		//
		if (   arrayNode.Properties[new XmlName("", "Type")] is not ElementNode typeNode
			|| KnownMarkups.GetTypeFromTypeExtension(typeNode, context) is not ITypeSymbol typeSymbol)
			throw new Exception("ArrayExtension Type not found");

		var arrayType = context.Compilation.CreateArrayTypeSymbol(typeSymbol);
		var property = new InflatorProperty(arrayType, NamingHelpers.CreateUniqueVariableName(context, arrayType));
		context.Variables.Add(arrayNode, property);

		//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.PropertiesWriter.Indent };

		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		using (PrePost.NewBlock(writer, $"public {arrayType.ToFQDisplayString()} {property.name}  {{", "}"))
		using (PrePost.NewBlock(writer, "get {", "}"))
		{
			writer.WriteLine($"if (field != null)");
			writer.Indent++;
			writer.WriteLine($"return field;");
			writer.Indent--;
			using (PrePost.NewBlock(writer, $"return field = new {arrayType.ToFQDisplayString()} {{", "};"))
			{
				if (arrayNode.CollectionItems != null)		
					foreach (var item in arrayNode.CollectionItems)
						writer.WriteLine($"{GetNodeValue(item, writers, context, typeSymbol, xNameElements).ValueAccessor},");
			}
		}
		writers.PropertiesWriter.Append(writer, noTabs: true);
		return property;
	}

	static InflatorProperty ValueCreator(INamedTypeSymbol type, ElementNode elementNode, (IndentedTextWriter PropertiesWriter, IndentedTextWriter ICWriter) writers, SourceGenContext context, Dictionary<ElementNode, string> xNameElements)
	{
		//let's create this code
		//
		// 	public global::Microsoft.Maui.Controls.Button Button  {
		// 		get {
		//	 		if (field != null)
		// 				return field;
		// 			field = Create(this);
		// 			SetProperties(field, this);
		// 			return field;

		// 			static global::Microsoft.Maui.Controls.Button Create(TestPageInflator inflator) {
		// 				var local = new global::Microsoft.Maui.Controls.Button();
		// 				global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo(local!, new global::System.Uri(@"Test.xaml;assembly=SourceGeneratorDriver.Generated", global::System.UriKind.Relative), 8, 5);
		// 				return local;
		// 			}

		// 			static void SetProperties(Button local, TestPageInflator inflator) {
		// 				local.SetValue(global::Microsoft.Maui.Controls.Button.TextProperty, "Hello MAUI!");
		// 			}
		//		}
		// }

		var property = new InflatorProperty(type, NamingHelpers.CreateUniqueVariableName(context, type));
		context.Variables.Add(elementNode, property);

		//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.PropertiesWriter.Indent };
		writer.WriteLine();

		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		using (PrePost.NewBlock(writer, $"public {type.ToFQDisplayString()} {property.name}  {{", "}"))
		using (PrePost.NewBlock(writer, "get {", "}"))
		{
			writer.WriteLine($"if (field != null)");
			writer.Indent++;
			writer.WriteLine($"return field;");
			writer.Indent--;
			writer.WriteLine($"field = Create(ref this);");
			writer.WriteLine($"SetProperties(field, ref this);");
			writer.WriteLine($"return field;");
			writer.WriteLineNoTabs("");

			using (PrePost.NewBlock(writer, $"static {type.ToFQDisplayString()} Create(ref {context.RootType.Name}Inflator inflator) {{", "}"))
			{
				CreateValuesVisitor.CreateValue(elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context);
				writer.WriteLine($"return {context.Variables[elementNode].ValueAccessor};");
			}
			writer.WriteLine();

			var currentVar = context.Variables[elementNode] = property; //replace the variable with the property
			var localVar = new LocalVariable(type, "local");

			using (PrePost.NewBlock(writer, $"static void SetProperties({type.ToFQDisplayString()} {localVar.ValueAccessor}, ref {context.RootType.Name}Inflator inflator) {{", "}"))
			{
				SetValuesForNode(elementNode, (writer, writers.PropertiesWriter, writers.ICWriter), currentVar, localVar, context, xNameElements);
			}
			context.Variables[elementNode] = currentVar; //restore the variable to the property
		}
		writers.PropertiesWriter.Append(writer, noTabs: true);
		return property;
	}

	//check if we can use the value directly, or if we hhave to create a variable accessor for it
	static bool CanBeSetDirectly(KeyValuePair<XmlName, INode> prop, IndentedTextWriter writer, SourceGenContext context)
	{
		if (prop.Value is ValueNode valueNode
			&& valueNode.Value is string strValue)
			return true;

		//x2009 language primitives
		if (prop.Value is ElementNode elementNode1
			&& CreateValuesVisitor.IsXaml2009LanguagePrimitive(elementNode1))
		{
			if (!(elementNode1.CollectionItems.Count == 1
				&& elementNode1.CollectionItems[0] is ValueNode valueNode1
				&& valueNode1.Value is string strValue1))
				strValue1 = string.Empty;

			var type1 = elementNode1.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache);
			context.Variables[elementNode1] = new DirectValue(type1!, NodeSGExtensions.ValueForLanguagePrimitive(strValue1, type1!, context, elementNode1));
			return true;
		}

		//known early bound MarkupExtensions
		if (prop.Value is ElementNode elementNode
			&& elementNode.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, out var type) && type is not null
			&& NodeSGExtensions.GetKnownEarlyMarkupExtensions(context).TryGetValue(type, out var provideValue)
			&& provideValue(elementNode, writer, context, null, out var returnType, out strValue))
		{
			context.Variables[elementNode] = new DirectValue(returnType!, strValue);
			return true;
		}

		//elementnode with a single value, and has a typeconverter, like <Color>Red</Color>
		//TODO should we restrict this to value types? or ref types without x:Names?
		if (prop.Value is ElementNode elementNode2
			&& elementNode2.CollectionItems.Count == 1
			&& elementNode2.CollectionItems[0] is ValueNode valueNode2
			&& valueNode2.Value is string strValue2
			&& elementNode2.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, out var type2) && type2 is not null
			&& type2.GetAttributes(context.Compilation.GetTypeByMetadataName("System.ComponentModel.TypeConverterAttribute")!).FirstOrDefault(ad => ad.ConstructorArguments[0].Value is ITypeSymbol) is var attribute && attribute is not null
			&& NodeSGExtensions.GetKnownSGTypeConverters(context).TryGetValue((ITypeSymbol)attribute.ConstructorArguments[0].Value!, out var converterInfo))
		{
			var converted = converterInfo.converter(strValue2, valueNode2, type2, context, null);
			context.Variables[elementNode2] = new DirectValue(converterInfo.returnType, converted);
			return true;
		}

		return false;
	}

	static bool IsValueProvider(ElementNode elementNode, SourceGenContext context)
	{
		var type = elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache);
		if (type is null)
			return false;

		if (NodeSGExtensions.GetKnownLateMarkupExtensions(context).TryGetValue(type, out var provideValue))
			return true;

		if (NodeSGExtensions.GetKnownValueProviders(context).TryGetValue(type, out provideValue))
			return true;

		if (type.IsValueProvider(context, out _, out _, out _, out _))
			return true;

		return false;
	}
}