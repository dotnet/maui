using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen;

class DependencyFirstInflator
{
	public static readonly IList<XmlName> Skips = [
		XmlName.xArguments,
		XmlName.xClass,
		XmlName.xClassModifier,
		XmlName.xDataType,
		XmlName.xFactoryMethod,
		XmlName.xFieldModifier,
		XmlName.xKey,
		XmlName.xName,
		XmlName.xTypeArguments,
		XmlName.mcIgnorable,
	];

	public static void Inflate(SourceGenContext context, ElementNode root, IndentedTextWriter writer)
	{
		var rootScope = new ScopeInfo(context.RootType.Name, "InitializeComponent", "this", "inflator", null);
		var inflatorScope = new ScopeInfo($"{context.RootType.Name}Inflator", null, "this", "inflator", "__root");
		ImmutableArray<ScopeInfo> scopes = [rootScope, inflatorScope];

		var thisVar = new LocalVariable((INamedTypeSymbol)context.RootType, "this") { Scope = rootScope };
		PreserveNodeValue(root, new InflatorScopedVar(context.RootType, "__root") { Scope = inflatorScope }, context);
		writer.WriteLine($"var inflator = new {context.RootType.Name}Inflator() {{ __root = {thisVar.ValueAccessor} }};");

		//a writer for writing calls that have to happen in InitializeCompnent, like setting xnames, connecting event handlers, ...
		var icWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writer.Indent };

		//set the properties on the root control, and create all the derred objects on the refstructwriter
		var refStructWriter = context.RefStructWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t");
		using (PrePost.NewBlock(refStructWriter, $"file ref struct {context.RootType.Name}Inflator() {{", "}"))
		{
			refStructWriter.WriteLine($"public required {context.RootType.ToFQDisplayString()} __root {{ get; init; }}");
			SetNamescope(root, (refStructWriter, writer), context, topLevel: true);
			refStructWriter.WriteLineNoTabs("");
			SetValuesForNode(root, (writer, refStructWriter, icWriter), thisVar, thisVar, scopes, context);
		}

		writer.WriteLineNoTabs("");
		writer.Append(icWriter, noTabs: true);
	}

	static void SetValuesForNode(ElementNode node, (IndentedTextWriter localMethodWriter, IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ILocalValue parentVar, ILocalValue localVar, ImmutableArray<ScopeInfo> scopes, SourceGenContext context)
	{
		var properties = new List<KeyValuePair<XmlName, INode>>();
		properties.AddRange(node.Properties.Where(p => !Skips.Contains(p.Key) && !node.SkipProperties.Contains(p.Key)));

		if (node.HasXName(out var xName))
		{
			//set runtime name (VSG)
			var runtimeNameAttr = localVar.Type.GetAllAttributes(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RuntimeNamePropertyAttribute")!, context).FirstOrDefault();
			if (runtimeNameAttr?.ConstructorArguments[0].Value is string name)
				writers.localMethodWriter.WriteLine($"{localVar.ValueAccessor}.{name} = \"{xName}\";");

			//set fields in InitializeComponent
			writers.ICWriter?.WriteLine($"this.{EscapeIdentifier(xName!)} = {parentVar.ValueAccessor};");

			//register in namescope
			var namescope = context.Scopes[node];
			if (namescope.namesInScope.ContainsKey(xName!))
				//TODO send diagnostic instead
				throw new Exception("dup x:Name");
			namescope.namesInScope.Add(xName!, parentVar);
			using (PrePost.NewConditional(writers.localMethodWriter, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				writers.localMethodWriter.WriteLine($"{namescope.namescope.ValueAccessor}.RegisterName(\"{xName}\", {parentVar.ValueAccessor});");
			}
			// SetStyleId((string)node.Value, Context.Variables[(ElementNode)parentNode]);
		}

		var contentPropertyName = node.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)?.GetContentPropertyName(context);
		if (   node.CollectionItems != null
			&& node.CollectionItems.Count > 0
			&& contentPropertyName != null)
			properties.AddRange(node.CollectionItems.Select(child => new KeyValuePair<XmlName, INode>(new XmlName(null, contentPropertyName), child)));

		foreach (var prop in properties)
			SetPropertyValue(prop, writers, parentVar, localVar, scopes, context, parentVar);

		if (   node.CollectionItems != null
			&& node.CollectionItems.Count > 0
			&& contentPropertyName == null
			&& parentVar.Type.CanAdd(context))
        {
			foreach (var item in node.CollectionItems)
				SetPropertyValue(new KeyValuePair<XmlName, INode>(XmlName.Empty, item), writers, parentVar, localVar, scopes, context, parentVar, asCollectionItem: true);
        }
	}

	static void SetPropertyValue(KeyValuePair<XmlName, INode> prop, (IndentedTextWriter localMethodWriter, IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ILocalValue parentVar, ILocalValue localVar, ImmutableArray<ScopeInfo> scopes, SourceGenContext context, ILocalValue inflatorVar, bool asCollectionItem = false)
	{
		if (!CanBeSetDirectly(prop.Value, writers.declarationWriter, context)) 
		{
			if (prop.Value is ListNode listNode)
			{
				//if there's a single item, treat it as a property
				if (listNode.CollectionItems.Count == 1)
				{
					SetPropertyValue(new KeyValuePair<XmlName, INode>(prop.Key, listNode.CollectionItems[0]), writers, parentVar, localVar, scopes, context, inflatorVar);
					return;
				}
				foreach (var item in listNode.CollectionItems)
				{
					var itemProp = new KeyValuePair<XmlName, INode>(prop.Key, item);
					SetPropertyValue(itemProp, writers, parentVar, localVar, scopes, context, inflatorVar, asCollectionItem: true);
				}
				return;
			}
			if (prop.Value is not ElementNode elementNode)
				throw new NotImplementedException();
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol type)
				throw new NotImplementedException();

			if (prop.Key == XmlName._CreateContent)
			{
				var template = LoadTemplate(prop.Value, type, writers, scopes, context);
				//create a nested ref struc
				// call ValueCreator on the elementNode
				// this.LoadTemplate => new innerinflaotr().label;
			}
			//xArray need a special syntax
			else if (type.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
				ArrayCreator(elementNode, (writers.declarationWriter, writers.ICWriter), scopes, context);
			else if (elementNode.IsValueProvider(context, out _, out _))
				ValueProvider(elementNode, (writers.declarationWriter, writers.ICWriter), scopes, context);
			else
				ValueCreator(type, elementNode, (writers.declarationWriter, writers.ICWriter), scopes, context);
		}
		bool tryGetNodeValue(INode node, ITypeSymbol toType, out ILocalValue? localVar) {
			localVar = GetNodeValue(node, (writers.declarationWriter, writers.ICWriter), scopes, context, toType!);
			return true;
		};

		SetPropertyHelpers.SetPropertyValue(writers.localMethodWriter, localVar, prop.Key, prop.Value, context, tryGetNodeValue: tryGetNodeValue, treeOrder: true, icWriter: writers.ICWriter, inflatorVar: inflatorVar, asCollectionItem: asCollectionItem);
	}

	static ILocalValue LoadTemplate(INode node, INamedTypeSymbol type, (IndentedTextWriter localMethodWriter, IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context)
	{
		ILocalValue templateContent;
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.declarationWriter.Indent };
		var inflatorName = NamingHelpers.CreateUniqueTypeName(context, "TemplateInflator");
		var scopeInfo = new ScopeInfo(inflatorName, null, "this", "inflator", null);
		using (PrePost.NewBlock(writer, $"ref struct {inflatorName} {{", "}"))
		{
			writer.WriteLine($"public required {context.RootType.ToFQDisplayString()} __root {{ get; init; }}");
			writer.WriteLine($"public {inflatorName}() {{ }}");
			templateContent = ValueCreator(type, (ElementNode)node, (writer, null /*template shouldn't register anything at class level*/), scopes.Add(scopeInfo), context).Descoped();
		}
		writers.declarationWriter.Append(writer, noTabs: true);
		return context.Variables[node] = templateContent.AsInflatorScoped(scopeInfo);
	}

	public static ILocalValue PreserveNodeValue(INode node, ILocalValue value, SourceGenContext context)
		=> context.Variables[node] = value;

	public static ILocalValue GetNodeValue(INode node, (IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context, ITypeSymbol toType)
	{
		// writers.declarationWriter.WriteLine($"//GetNodeValue {node} as {toType.ToFQDisplayString()}");
		if (context.Variables.TryGetValue(node, out var localVar))
			return localVar;

		if (CanBeSetDirectly(node, writers.declarationWriter, context))
		{
			if (context.Variables.TryGetValue(node, out var localVar1))
				return localVar1;
			if (node is ValueNode valueNode && valueNode.Value is string strValue)
				return new DirectValue(toType, valueNode.ConvertTo(toType, writers.declarationWriter, context, null));
			throw new NotImplementedException();
		}

		if (node is ElementNode elementNode)
		{
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol elementType)
				throw new NotImplementedException();
			//xArray need a special syntax
			if (elementType.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
				return ArrayCreator(elementNode, writers, scopes, context);
			else if (elementNode.IsValueProvider(context, out _, out _))
				return ValueProvider(elementNode, writers, scopes, context);
			else
				return ValueCreator((INamedTypeSymbol)toType, elementNode, writers, scopes, context);
		}

		throw new NotImplementedException();
	}

	static ILocalValue ValueProvider(ElementNode elementNode, (IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context)
	{
		var type = elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)!;
		var hasXName = elementNode.HasXName(out _); //if there's a x:Name, we need to create a var for future references
		bool hasPropertiesRequiringParentObject = elementNode.HasPropertiesRequiringParentObject(context, out _);
		bool tryGetNodeValue(INode node, ITypeSymbol toType, out ILocalValue localVar)
		{
			localVar = GetNodeValue(node, writers, scopes, context, toType!);
			return true;
		}

		//new thing
		if (   KnownMarkups.TryGetValueProvider(type, context.Compilation, out var valueProvider)
			&& valueProvider!.CanProvideValue(elementNode, context, tryGetNodeValue))
        {
			if (!hasXName && !hasPropertiesRequiringParentObject)
				return PreserveNodeValue(elementNode, valueProvider.ProvideDirectValue(elementNode, writers.declarationWriter, context, tryGetNodeValue), context);
			else
				return PreserveNodeValue(elementNode, valueProvider.ProvideValue(elementNode, writers, scopes, context, tryGetNodeValue), context);
        }


		//FIXME: move all markup and value provider to the new registry
		//old-ish one
		if (NodeSGExtensions.GetKnownLateMarkupExtensions(context).TryGetValue(type, out var provideValue)
			&& provideValue.Invoke(elementNode, writers.declarationWriter, context, tryGetNodeValue, out var returnType0, out var value))
			return PreserveNodeValue(elementNode, new DirectValue(returnType0!, value), context);

		if (NodeSGExtensions.GetKnownValueProviders(context).TryGetValue(type, out provideValue)
			&& provideValue.Invoke(elementNode, writers.declarationWriter, context, tryGetNodeValue, out returnType0, out value))
			return PreserveNodeValue(elementNode, new DirectValue(returnType0!, value), context);

		type.IsValueProvider(context, out var returnType, out var iface, out var acceptEmptyServiceProvider, out var requiredServices);


		//it's either a custom markup extnesion (or value provider) or we haven't implemented it yet. fallback to instanciating the extension, nd calling ProvideValue

		if (acceptEmptyServiceProvider)
			return PreserveNodeValue(elementNode, new DirectValue(returnType!, $"new {type.ToFQDisplayString()}().ProvideValue(null)"), context);
		

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

		var property = new InflatorScopedVar(returnType, NamingHelpers.CreateUniqueVariableName(context, returnType));
		PreserveNodeValue(elementNode, property, context);

		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.declarationWriter.Indent };

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
				CreateValuesVisitor.CreateValue(elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context, tryGetNodeValue);
				var localVar = context.Variables[elementNode];
				SetValuesForNode(elementNode, (writer, writers.declarationWriter, writers.ICWriter), property, localVar, scopes, context);

				var serviceProviderVar = elementNode.GetOrCreateServiceProvider(writer, context, requiredServices);
				writer.WriteLine($"return ({returnType.ToFQDisplayString()}){localVar.ValueAccessor}.ProvideValue({serviceProviderVar.ValueAccessor});");
			}
		}
		writers.declarationWriter.Append(writer, noTabs: true);

		return PreserveNodeValue(elementNode, property, context);
    }

	static InflatorScopedVar ArrayCreator(ElementNode arrayNode, (IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context)
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
		var property = new InflatorScopedVar(arrayType, NamingHelpers.CreateUniqueVariableName(context, arrayType));
		PreserveNodeValue(arrayNode, property, context);

		//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.declarationWriter.Indent };

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
						writer.WriteLine($"{GetNodeValue(item, writers, scopes, context, typeSymbol).ValueAccessor},");
			}
		}
		writers.declarationWriter.Append(writer, noTabs: true);
		return property;
	}

	static InflatorScopedVar ValueCreator(INamedTypeSymbol type, ElementNode elementNode, (IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context)
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

		var property = new InflatorScopedVar(type, NamingHelpers.CreateUniqueVariableName(context, type));
		PreserveNodeValue(elementNode, property, context);

		//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.declarationWriter.Indent };
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
			writer.WriteLineNoTabs();

			var currentScope = scopes.Last();
			using (PrePost.NewBlock(writer, $"static {type.ToFQDisplayString()} Create(ref {currentScope.type} inflator) {{", "}"))
			{
				bool tryGetNodeValue(INode node, ITypeSymbol toType, out ILocalValue? localVar) {
					localVar = GetNodeValue(node, writers, scopes, context, toType!);
					return true;
				};
				CreateValuesVisitor.CreateValue(elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context, tryGetNodeValue);
				var (namescope, namesInNamescope) = SetNamescope(elementNode, (writers.declarationWriter, writer), context);
				writer.WriteLine($"return {context.Variables[elementNode].ValueAccessor};");
			}
			writer.WriteLineNoTabs();

			var currentVar = PreserveNodeValue(elementNode, property, context); //replace the variable with the property
			var localVar = new LocalVariable(type, "local");

			using (PrePost.NewBlock(writer, $"static void SetProperties({type.ToFQDisplayString()} {localVar.ValueAccessor}, ref {currentScope.type} inflator) {{", "}"))
			{
				var icWriter = SetNamescopesAndRegisterNamesVisitor.IsDataTemplate(elementNode, elementNode.Parent)
							|| SetNamescopesAndRegisterNamesVisitor.IsStyle(elementNode, elementNode.Parent)
							|| SetNamescopesAndRegisterNamesVisitor.IsVisualStateGroupList(elementNode)
					? null : writers.ICWriter;
				SetValuesForNode(elementNode, (writer, writers.declarationWriter, icWriter), currentVar, localVar, scopes, context);
			}
			writer.WriteLineNoTabs();

			if (type.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.DataTemplate"), SymbolEqualityComparer.Default))
			{
				var v = context.Variables[elementNode.Properties[XmlName._CreateContent]] as InflatorScopedVar;
				writer.WriteLine($"var __root = this.__root;");
				using (PrePost.NewBlock(writer, $"field.LoadTemplate = () => new {v!.Scope?.type}() {{", $"}}.{v!.Descoped().ValueAccessor};"))
				{
					writer.WriteLine($"__root = __root,");
                    // writer.WriteLine("//copy over the var from upper scope");
                }
				writer.WriteLineNoTabs();
			}
			
			writer.WriteLine($"return field;");
			PreserveNodeValue(elementNode, currentVar, context); //restore the variable to the property
		}
		writers.declarationWriter.Append(writer, noTabs: true);
		return property;
	}

	static (ILocalValue, IDictionary<string, ILocalValue>) SetNamescope(ElementNode elementNode, (IndentedTextWriter declarationWriter, IndentedTextWriter localmethodwriter) writers, SourceGenContext context, bool topLevel = false)
	{
		ILocalValue namescope;
		IDictionary<string, ILocalValue> namesInNamescope;
		var setNameScope = false;

		if (   elementNode.Parent is null
			|| SetNamescopesAndRegisterNamesVisitor.IsDataTemplate(elementNode, elementNode.Parent)
			|| SetNamescopesAndRegisterNamesVisitor.IsStyle(elementNode, elementNode.Parent)
			|| SetNamescopesAndRegisterNamesVisitor.IsVisualStateGroupList(elementNode))
		{
			//FIXME for toplevel, we should call GetNameScope on the root, if there's a namescope set on it
			namescope = SetNamescopesAndRegisterNamesVisitor.CreateNamescope(writers.declarationWriter, context, accessor: topLevel ? "public" : null).AsInflatorScoped();
			namesInNamescope = new Dictionary<string, ILocalValue>();
			setNameScope = true;
		}
		else
		{
			var parent = elementNode.Parent is ListNode listNode ? listNode.Parent : elementNode.Parent;
			namescope = context.Scopes[parent].namescope;
			namesInNamescope = context.Scopes[parent].namesInScope;
		}

		if (setNameScope && context.Variables[elementNode].Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!, context))
			using (PrePost.NewConditional(writers.localmethodwriter, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				writers.localmethodwriter.WriteLine($"global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope({context.Variables[elementNode].ValueAccessor}, {namescope.ValueAccessor});");
			}
		//workaround when VSM tries to apply state before parenting (https://github.com/dotnet/maui/issues/16208)
		else if (context.Variables.TryGetValue(elementNode, out var variable) && variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Element")!, context))
			using (PrePost.NewConditional(writers.localmethodwriter, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				writers.localmethodwriter.WriteLine($"{context.Variables[elementNode].ValueAccessor}.transientNamescope = {namescope.ValueAccessor};");
			}
		return context.Scopes[elementNode] = (namescope, namesInNamescope);
	}

	//check if we can use the value directly, or if we hhave to create a variable accessor for it
	static bool CanBeSetDirectly(INode propValue, IndentedTextWriter writer, SourceGenContext context)
	{
		if (propValue is ValueNode valueNode
			&& valueNode.Value is string strValue)
			return true;

		//x2009 language primitives
		if (propValue is ElementNode elementNode1
			&& CreateValuesVisitor.IsXaml2009LanguagePrimitive(elementNode1))
		{
			if (   !(elementNode1.CollectionItems.Count == 1
				&& elementNode1.CollectionItems[0] is ValueNode valueNode1
				&& valueNode1.Value is string strValue1))
				strValue1 = string.Empty;

			var type1 = elementNode1.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache);
			PreserveNodeValue(elementNode1, new DirectValue(type1!, NodeSGExtensions.ValueForLanguagePrimitive(strValue1, type1!, context, elementNode1)), context);
			return true;
		}

		//known early bound MarkupExtensions
		if (   propValue is ElementNode elementNode
			&& elementNode.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, out var type) && type is not null
			&& NodeSGExtensions.GetKnownEarlyMarkupExtensions(context).TryGetValue(type, out var provideValue)
			&& provideValue(elementNode, writer, context, null, out var returnType, out strValue))
		{
			PreserveNodeValue(elementNode, new DirectValue(returnType!, strValue), context);
			return true;
		}

		//elementnode with a single value, and has a typeconverter, like <Color>Red</Color>
		if (   propValue is ElementNode elementNode2
			&& !propValue.HasXName(out _)				//x:Name would mean we need a variable
			&& elementNode2.CollectionItems.Count == 1
			&& elementNode2.CollectionItems[0] is ValueNode valueNode2
			&& valueNode2.Value is string strValue2
			&& elementNode2.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, out var type2) && type2 is not null
			&& type2.GetAttributes(context.Compilation.GetTypeByMetadataName("System.ComponentModel.TypeConverterAttribute")!).FirstOrDefault(ad => ad.ConstructorArguments[0].Value is ITypeSymbol) is var attribute && attribute is not null
			&& NodeSGExtensions.GetKnownSGTypeConverters(context).TryGetValue((ITypeSymbol)attribute.ConstructorArguments[0].Value!, out var converterInfo))
		{
			var converted = converterInfo.converter(strValue2, valueNode2, type2, writer, context, null);
			PreserveNodeValue(elementNode2, new DirectValue(converterInfo.returnType, converted), context);
			return true;
		}

		return false;
	}
}