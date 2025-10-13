using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
		var rootScope = new TypeScope(null!, (INamedTypeSymbol)context.RootType);
		var inflatorScope = new InflatorScope(context.RefStructWriter = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t"), $"{context.RootType.Name}Inflator");

		var icScope = new InitializeComponentScope(new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writer.Indent }, ("inflator", inflatorScope));
		var ic = ImmutableArray.Create<Scope>(rootScope, icScope);
		inflatorScope.InitializeComponentScope = ("__root", ic);
		var inf = ImmutableArray.Create<Scope>(inflatorScope);

		//root object (usually the page) can be accessed from both scopes
		PreserveNodeValue(root, new ThisValue(context.RootType, rootScope), context);
		PreserveNodeValue(root, new ScopedVariable(context.RootType, "__root", inflatorScope), context);

		icScope.Writer.WriteLine($"var inflator = new {context.RootType.Name}Inflator() {{ __root = this }};");

		using (PrePost.NewBlock(inflatorScope.Writer, $"file ref struct {context.RootType.Name}Inflator() {{", "}"))
		{
			inflatorScope.Writer.WriteLine($"public required {context.RootType.ToFQDisplayString()} __root {{ get; init; }}");
			SetNamescope(root, (inf, ic), context, topLevel: true);
			SetValuesForNode(root, (inf, ic), context);
		}

		writer.Append(icScope.Writer, noTabs: true);
	}

	static void SetValuesForNode(ElementNode node, ImmutableArray<Scope> scopes, SourceGenContext context)
    {
		Debug.Assert(scopes.Last() is StaticMethodScope);
		var declareScope = scopes.RemoveRange(scopes.Length - 2, 2);
		SetValuesForNode(node, (declareScope, scopes), context);
    }

	static void SetValuesForNode(ElementNode node, (ImmutableArray<Scope> declarationScope, ImmutableArray<Scope> methodScope) scopes, SourceGenContext context)
	{
		var declarationWriter = scopes.declarationScope.Last().Writer;  //inflator writer in which we can declare new properties
		var methodWriter = scopes.methodScope.Last().Writer;            //current method writer in which we set values. Could be InitComp for top level, or a static method of rinflator properties

		var properties = new List<KeyValuePair<XmlName, INode>>();
		properties.AddRange(node.Properties.Where(p => !Skips.Contains(p.Key) && !node.SkipProperties.Contains(p.Key)));

		var localVar =  GetNodeValue(node, scopes.methodScope, context, context.Compilation.ObjectType);;
		if (localVar is ScopedVariable lv)
			localVar = lv.AccessedFrom(scopes.methodScope);

		if (node.HasXName(out var xName))
		{
			//set runtime name (VSG)
			var runtimeNameAttr = localVar.Type.GetAllAttributes(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RuntimeNamePropertyAttribute")!, context).FirstOrDefault();
			if (runtimeNameAttr?.ConstructorArguments[0].Value is string name)
				methodWriter.WriteLine($"{localVar.ValueAccessor}.{name} = \"{xName}\";");

			//set fields in InitializeComponent
			var icScopes = scopes.methodScope.Last() is InitializeComponentScope ? scopes.methodScope : scopes.declarationScope.Last() is InflatorScope inf ? inf.InitializeComponentScope.scopes : throw new InvalidOperationException();
			var methodScopes = scopes.methodScope;
			if (methodScopes.Length == 3)
				methodScopes = methodScopes.Slice(0, 1);
			var parentVar = GetNodeValue(node, methodScopes, context, context.Compilation.ObjectType);
			if (parentVar is ScopedVariable sv)
				parentVar = sv.AccessedFrom(icScopes);
			if (ShouldSetFieldsInICForXName(node, context))			
				icScopes.Last().Writer.WriteLine($"this.{EscapeIdentifier(xName!)} = {parentVar.ValueAccessor};");
			
			//register in namescope
			var namescope = context.Scopes[node];
			if (namescope.namesInScope.ContainsKey(xName!))
				//TODO send diagnostic instead
				throw new Exception("dup x:Name");
			namescope.namesInScope.Add(xName!, parentVar);
			using (PrePost.NewConditional(methodWriter, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))			
				methodWriter.WriteLine($"{namescope.namescope.ValueAccessor}.RegisterName(\"{xName}\", {localVar.ValueAccessor});");
			
			// SetStyleId((string)node.Value, Context.Variables[(ElementNode)parentNode]);
		}

		var contentPropertyName = node.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)?.GetContentPropertyName(context);
		if (   node.CollectionItems != null
			&& node.CollectionItems.Count > 0
			&& contentPropertyName != null)
			properties.AddRange(node.CollectionItems.Select(child => new KeyValuePair<XmlName, INode>(new XmlName(null, contentPropertyName), child)));

		foreach (var prop in properties)
			SetPropertyValue(node, prop, scopes, context);

		if (   node.CollectionItems != null
			&& node.CollectionItems.Count > 0
			&& contentPropertyName == null
			&& localVar.Type.CanAdd(context))
		{
			foreach (var item in node.CollectionItems)
				SetPropertyValue(node, new KeyValuePair<XmlName, INode>(XmlName.Empty, item), scopes, context, asCollectionItem: true);
		}
	}

	static void SetPropertyValue(INode node, KeyValuePair<XmlName, INode> prop, ImmutableArray<Scope> scopes, SourceGenContext context, bool asCollectionItem = false)
	{
		Debug.Assert(scopes.Last() is StaticMethodScope);
		var declareScope = scopes.RemoveRange(scopes.Length - 2, 2);
		SetPropertyValue(node, prop, (declareScope, scopes), context, asCollectionItem);
	}
	
	static void SetPropertyValue(INode node, KeyValuePair<XmlName, INode> prop, (ImmutableArray<Scope> declarationScope, ImmutableArray<Scope> methodScope) scopes, SourceGenContext context, bool asCollectionItem = false)
	{
		Debug.Assert(scopes.declarationScope.Last() is InflatorScope);
		Debug.Assert(scopes.methodScope.Last() is InitializeComponentScope || scopes.methodScope.Last() is StaticMethodScope);

		var declarationWriter = scopes.declarationScope.Last().Writer;	//inflator writer in which we can declare new properties
		var methodWriter = scopes.methodScope.Last().Writer;			//current method writer in which we set values. Could be InitComp for top level, or a static method of rinflator properties

		if (!CanBeSetDirectly(prop.Value, declarationWriter, context)) 
		{
			if (prop.Value is ListNode listNode)
			{
				//if there's a single item, treat it as a property
				if (listNode.CollectionItems.Count == 1)
				{
					SetPropertyValue(node, new KeyValuePair<XmlName, INode>(prop.Key, listNode.CollectionItems[0]), scopes, context);
					return;
				}
				foreach (var item in listNode.CollectionItems)
				{
					var itemProp = new KeyValuePair<XmlName, INode>(prop.Key, item);
					SetPropertyValue(node, itemProp, scopes, context, asCollectionItem: true);
				}
				return;
			}
			if (prop.Value is not ElementNode elementNode)
				throw new NotImplementedException();
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol type)
				throw new NotImplementedException();

			// if (prop.Key == XmlName._CreateContent)
			// {
			// 	var template = LoadTemplate(prop.Value, type, writers, scopes, context);
			// 	//create a nested ref struc
			// 	// call ValueCreator on the elementNode
			// 	// this.LoadTemplate => new innerinflaotr().label;
			// }
			//xArray need a special syntax
			/*else*/ if (type.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
				ArrayCreator(elementNode, scopes.methodScope, context);
			else if (elementNode.IsValueProvider(context, out _, out _))
				ValueProvider(elementNode, scopes.methodScope, context);
			else
				ValueCreator(elementNode, type, scopes.declarationScope, context);
		}
		bool tryGetNodeValue(INode node, ITypeSymbol toType, out ILocalValue? localVar) {
			localVar = GetNodeValue(node, scopes.methodScope, context, toType!);
			if (localVar is ScopedVariable sv)
				localVar = sv.AccessedFrom(scopes.methodScope);
			return true;
		};

		var icScopes = scopes.methodScope.Last() is InitializeComponentScope  ? scopes.methodScope : scopes.declarationScope.Last() is InflatorScope inf ? inf.InitializeComponentScope.scopes : throw new InvalidOperationException();
		var icScope = icScopes.Last() as InitializeComponentScope ?? throw new InvalidOperationException();
		var targetVar = GetNodeValue(node, scopes.methodScope, context, context.Compilation.ObjectType).Descope();
		var inflatorVar = GetNodeValue(node, scopes.declarationScope, context, context.Compilation.ObjectType).AccessedFrom(icScopes);

		SetPropertyHelpers.SetPropertyValue(methodWriter, targetVar, prop.Key, prop.Value, context, tryGetNodeValue: tryGetNodeValue, treeOrder: true, icWriter: icScope.Writer, inflatorVar: inflatorVar, asCollectionItem: asCollectionItem, scopes.methodScope);
	}

	// static ILocalValue LoadTemplate(INode node, INamedTypeSymbol type, (IndentedTextWriter localMethodWriter, IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context)
	// {
	// 	ILocalValue templateContent;
	// 	var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = writers.declarationWriter.Indent };
	// 	var inflatorName = NamingHelpers.CreateUniqueTypeName(context, "TemplateInflator");
	// 	var scopeInfo = new ScopeInfo(inflatorName, null, "this", "inflator", null);
	// 	using (PrePost.NewBlock(writer, $"ref struct {inflatorName} {{", "}"))
	// 	{
	// 		writer.WriteLine($"public required {context.RootType.ToFQDisplayString()} __root {{ get; init; }}");
	// 		writer.WriteLine($"public {inflatorName}() {{ }}");
	// 		templateContent = ValueCreator(type, (ElementNode)node, (writer, null /*template shouldn't register anything at class level*/), scopes.Add(scopeInfo), context).Descoped();
	// 	}
	// 	writers.declarationWriter.Append(writer, noTabs: true);
	// 	return context.Variables[node] = templateContent.AsInflatorScoped(scopeInfo);
	// }

	static Dictionary<SourceGenContext, Dictionary<Scope, Dictionary<INode, ScopedVariable>>> scopedVariables = [];
	static Dictionary<SourceGenContext, Dictionary<INode, DirectValue>> directValues = [];
	public static T PreserveNodeValue<T>(INode node, T value, SourceGenContext context) where T : ILocalValue
	{
		if (value is DirectValue dv)
		{
			if (!directValues.TryGetValue(context, out var dict))
				directValues[context] = dict = [];
			return (T)(context.Variables[node] = dict[node] = dv);
		}
		else if (value is ScopedVariable sv)
		{
			if (!scopedVariables.TryGetValue(context, out var dict))
				scopedVariables[context] = dict = [];
			if (!dict.TryGetValue(sv.Scope, out var dict2))
				dict[sv.Scope] = dict2 = [];
			return (T)(context.Variables[node] = dict2[node] = sv);
		}
		throw new NotImplementedException();
	}

	public static ILocalValue GetNodeValue(INode node, ImmutableArray<Scope> scopes, SourceGenContext context, ITypeSymbol toType)
	{
		if (directValues.TryGetValue(context, out var dict) && dict.TryGetValue(node, out var directVar))
			return directVar;

		if (scopedVariables.TryGetValue(context, out var scopedVar))
		{
			//check if the node as a variable in the current scope or in a parent
			foreach (var searchscope in scopes.Reverse())
				if (scopedVar.TryGetValue(searchscope, out var dict2) && dict2.TryGetValue(node, out var scopedVariable))
					return scopedVariable;

			//check if the node has a variable in any linked scope
			var searchScope = scopes.Last() is StaticMethodScope sme ? sme.InflatorScope.scope : scopes.Last() is InitializeComponentScope ic ? ic.InflatorScope.scope : null;
			if (searchScope != null && scopedVar.TryGetValue(searchScope, out var dict3) && dict3.TryGetValue(node, out var scopedVariable3))
				return scopedVariable3;
		}

		if (context.Variables.TryGetValue(node, out var localVar))
			throw new Exception("Variable found but not in scope");

		var declarationScope = scopes[0]; //the inflator scope

		if (CanBeSetDirectly(node, declarationScope.Writer, context))
		{
			if (context.Variables.TryGetValue(node, out var localVar1))
				return localVar1;
			if (node is ValueNode valueNode && valueNode.Value is string strValue)
				return new DirectValue(toType, valueNode.ConvertTo(toType, declarationScope.Writer, context, null));
			throw new NotImplementedException();
		}

		if (node is ElementNode elementNode)
		{
			if (elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache) is not INamedTypeSymbol elementType)
				throw new NotImplementedException();
			//xArray need a special syntax
			if (elementType.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
				return ArrayCreator(elementNode, scopes, context);
			else if (elementNode.IsValueProvider(context, out _, out _))
				return ValueProvider(elementNode, scopes, context);
			else
				return ValueCreator(elementNode, (INamedTypeSymbol)toType, scopes, context);
		}

		throw new NotImplementedException();
	}

	static ILocalValue ValueProvider(ElementNode elementNode, ImmutableArray<Scope> scopes, SourceGenContext context)
	{
		var scope = scopes.Last();
		Debug.Assert(scope is StaticMethodScope || scope is InitializeComponentScope || scope is PropertyScope);

		var inflatorScope = scope is InitializeComponentScope ic ? ic.InflatorScope.scope! : (InflatorScope)scopes.First();
		var declarationWriter = inflatorScope.Writer;
		var methodWriter = scope.Writer;
		
		var type = elementNode.XmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache)!;
		var hasXName = elementNode.HasXName(out _); //if there's a x:Name, we need to create a var for future references
		bool hasPropertiesRequiringParentObject = elementNode.HasPropertiesRequiringParentObject(context, out _);
		bool tryGetNodeValue(INode node, ITypeSymbol toType, out ILocalValue localVar)
		{
			localVar = GetNodeValue(node, scopes, context, toType!);
			if (localVar is ScopedVariable sv)
				localVar = sv.AccessedFrom(scopes);
			return true;
		}

		//new thing
		if (   KnownMarkups.TryGetValueProvider(type, context.Compilation, out var valueProvider)
			&& valueProvider!.CanProvideValue(elementNode, context, tryGetNodeValue))
	    {
			if (!hasXName && !hasPropertiesRequiringParentObject)
				return PreserveNodeValue(elementNode, valueProvider.ProvideDirectValue(elementNode, methodWriter, context, tryGetNodeValue), context);
			else
				return PreserveNodeValue(elementNode, valueProvider.ProvideValue(elementNode, (declarationWriter, null), scopes, context, tryGetNodeValue), context);
	    }


		//FIXME: move all markup and value provider to the new registry
		//old-ish one
		if (NodeSGExtensions.GetKnownLateMarkupExtensions(context).TryGetValue(type, out var provideValue)
			&& provideValue.Invoke(elementNode, declarationWriter, context, tryGetNodeValue, out var returnType0, out var value))
			return PreserveNodeValue(elementNode, new DirectValue(returnType0!, value), context);

		if (NodeSGExtensions.GetKnownValueProviders(context).TryGetValue(type, out provideValue)
			&& provideValue.Invoke(elementNode, declarationWriter, context, tryGetNodeValue, out returnType0, out value))
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
		scopes = scopes.Slice(0, 1);	//only keep the Inflator scope
		var property = new ScopedVariable(returnType, NamingHelpers.CreateUniqueVariableName(context, returnType), inflatorScope);
		PreserveNodeValue(elementNode, property, context);

		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = declarationWriter.Indent };			writer.WriteLineNoTabs();

		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		using (PrePost.NewBlock(writer, $"public {returnType.ToFQDisplayString()} {property.Name}  {{", "}"))
		using (PrePost.NewBlock(writer, "get {", "}"))
		{
			writer.WriteLine($"if (field != null)");
			writer.Indent++;
			writer.WriteLine($"return field;");
			writer.Indent--;
			writer.WriteLine($"return field = ProvideValue(ref this);");
			writer.WriteLineNoTabs();
			using (PrePost.NewBlock(writer, $"static {returnType.ToFQDisplayString()} ProvideValue(ref {context.RootType.Name}Inflator inflator) {{", "}"))
			{
				var propertyScope = scopes.Add(new PropertyScope(writer, type, property.Name));
				var sms = new StaticMethodScope(writer, ("inflator", inflatorScope));
				var provideValueScope = propertyScope.Add(sms);

				CreateValuesVisitor.CreateValue(elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context, tryGetNodeValue);
				var localVar = context.Variables[elementNode];
				localVar = PreserveNodeValue(elementNode, new ScopedVariable(localVar.Type, localVar.ValueAccessor, sms), context);
				SetValuesForNode(elementNode, provideValueScope, context);

				var serviceProviderVar = elementNode.GetOrCreateServiceProvider(writer, context, requiredServices, provideValueScope);
				writer.WriteLine($"return ({returnType.ToFQDisplayString()}){localVar.AccessedFrom(provideValueScope).ValueAccessor}.ProvideValue({serviceProviderVar.ValueAccessor});");
			}
		}
		declarationWriter.Append(writer, noTabs: true);

		return PreserveNodeValue(elementNode, property, context);
	}

	static ScopedVariable ArrayCreator(ElementNode arrayNode, ImmutableArray<Scope> scopes, SourceGenContext context)
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
		var inflatorScope = scopes.LastOrDefault(s => s is InflatorScope) as InflatorScope?? throw new InvalidOperationException();
		var declarationWriter = inflatorScope.Writer;
		var property = new ScopedVariable(arrayType, NamingHelpers.CreateUniqueVariableName(context, arrayType), inflatorScope);
		PreserveNodeValue(arrayNode, property, context);

		//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = inflatorScope.Writer.Indent };
		writer.WriteLineNoTabs();

		scopes = scopes.Slice(0, 1);	//only keep the Inflator scope
		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		var propertyScope = scopes.Add(new PropertyScope(writer, arrayType, property.Name));
		using (PrePost.NewBlock(writer, $"public {arrayType.ToFQDisplayString()} {property.Name}  {{", "}"))
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
						writer.WriteLine($"{GetNodeValue(item, propertyScope, context, typeSymbol).ValueAccessor},");
			}
		}
		declarationWriter.Append(writer, noTabs: true);
		return property;
	}

	static ScopedVariable ValueCreator(ElementNode elementNode, INamedTypeSymbol type, ImmutableArray<Scope> scopes, SourceGenContext context)
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

		var inflatorScope = scopes.LastOrDefault(s => s is InflatorScope) as InflatorScope?? throw new InvalidOperationException();
		var declarationWriter = inflatorScope.Writer;
		scopes = scopes.Slice(0, 1);	//only keep the Inflator scope

		var property = new ScopedVariable(type, NamingHelpers.CreateUniqueVariableName(context, type), inflatorScope);
		PreserveNodeValue(elementNode, property, context);

		//create one writer per property, to flush them at once
		var writer = new IndentedTextWriter(new StringWriter(CultureInfo.InvariantCulture), "\t") { Indent = inflatorScope.Writer.Indent };
		writer.WriteLineNoTabs();

		writer.WriteLine("[field: global::System.Diagnostics.CodeAnalysis.MaybeNull, global::System.Diagnostics.CodeAnalysis.AllowNull]");
		using (PrePost.NewBlock(writer, $"public {type.ToFQDisplayString()} {property.Name}  {{", "}"))
		using (PrePost.NewBlock(writer, "get {", "}"))
		{
			writer.WriteLine($"if (field != null)");
			writer.Indent++;
			writer.WriteLine($"return field;");
			writer.Indent--;
			writer.WriteLine($"field = Create(ref this);");
			writer.WriteLine($"SetProperties(field, ref this);");
			writer.WriteLineNoTabs();

			var propertyScope = scopes.Add(new PropertyScope(writer, type, property.Name));
			var createValueScope = propertyScope.Add(new StaticMethodScope(writer, ("inflator", inflatorScope)));
			using (PrePost.NewBlock(writer, $"static {type.ToFQDisplayString()} Create(ref {inflatorScope.Type} inflator) {{", "}"))
			{
				bool tryGetNodeValue(INode node, ITypeSymbol toType, out ILocalValue? localVar) {
					localVar = GetNodeValue(node, createValueScope, context, toType!);
					return true;
				};
				CreateValuesVisitor.CreateValue(elementNode, writer, context.Variables, context.Compilation, context.XmlnsCache, context, tryGetNodeValue, createValueScope);
				var (namescope, namesInNamescope) = SetNamescope(elementNode, createValueScope, context);
				writer.WriteLine($"return {context.Variables[elementNode].ValueAccessor};");
			}
			writer.WriteLineNoTabs();

			var staticMethodScope = new StaticMethodScope(writer, ("inflator", inflatorScope));
			var setPropertiesScope = propertyScope.Add(staticMethodScope);
			var localVar = PreserveNodeValue(elementNode, new ScopedVariable(type, "local", staticMethodScope), context);
			PreserveNodeValue(elementNode, property, context);
			using (PrePost.NewBlock(writer, $"static void SetProperties({type.ToFQDisplayString()} {localVar.Name}, ref {inflatorScope.Type} inflator) {{", "}"))
			{
				SetValuesForNode(elementNode, setPropertiesScope, context);
			}
			writer.WriteLineNoTabs();

			// if (type.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.DataTemplate"), SymbolEqualityComparer.Default))
			// {
			// 	var v = context.Variables[elementNode.Properties[XmlName._CreateContent]] as InflatorScopedVar;
			// 	writer.WriteLine($"var __root = this.__root;");
			// 	using (PrePost.NewBlock(writer, $"field.LoadTemplate = () => new {v!.Scope?.type}() {{", $"}}.{v!.Descoped().ValueAccessor};"))
			// 	{
			// 		writer.WriteLine($"__root = __root,");
			//         // writer.WriteLine("//copy over the var from upper scope");
			//     }
			// 	writer.WriteLineNoTabs();
			// }

			writer.WriteLine($"return field;");
		}
		declarationWriter.Append(writer, noTabs: true);
		return property;
	}

	static (ILocalValue, IDictionary<string, ILocalValue>) SetNamescope(ElementNode elementNode, ImmutableArray<Scope> scopes, SourceGenContext context)
	{
		Debug.Assert(scopes.Last() is StaticMethodScope);
		var declareScope = scopes.RemoveRange(scopes.Length - 2, 2);
		return SetNamescope(elementNode, (declareScope, scopes), context);
	}
	
	static (ILocalValue, IDictionary<string, ILocalValue>) SetNamescope(ElementNode elementNode, (ImmutableArray<Scope> declareScope, ImmutableArray<Scope> setNSScope) scopes, SourceGenContext context, bool topLevel = false)
	{
		ILocalValue namescope;
		IDictionary<string, ILocalValue> namesInNamescope;
		var setNameScope = false;

		if (elementNode.Parent is null
			|| SetNamescopesAndRegisterNamesVisitor.IsDataTemplate(elementNode, elementNode.Parent)
			|| SetNamescopesAndRegisterNamesVisitor.IsStyle(elementNode, elementNode.Parent)
			|| SetNamescopesAndRegisterNamesVisitor.IsVisualStateGroupList(elementNode))
		{
			//FIXME the public/private shouldn't be based on toplevel, but on scope visibility
			//FIXME for toplevel, we should call GetNameScope on the root, if there's a namescope set on it
			namescope = SetNamescopesAndRegisterNamesVisitor.CreateNamescope(scopes.declareScope[scopes.declareScope.Length - 1].Writer, context, accessor: topLevel ? "public" : null);
			if (namescope is LocalVariable lv)	//workaround til SNARNV retunrs a ScopedVariable
				namescope = new ScopedVariable(lv.Type, lv.Name, scopes.declareScope[scopes.declareScope.Length - 1]);
			if (namescope is ScopedVariable sv)
				namescope = sv.AccessedFrom(scopes.setNSScope);
			
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
			using (PrePost.NewConditional(scopes.setNSScope[scopes.setNSScope.Length - 1].Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				var target = GetNodeValue(elementNode, scopes.setNSScope, context, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!);
				if (target is ScopedVariable sv)
					target = sv.AccessedFrom(scopes.setNSScope);
				scopes.setNSScope[scopes.setNSScope.Length - 1].Writer.WriteLine($"global::Microsoft.Maui.Controls.Internals.NameScope.SetNameScope({target.ValueAccessor}, {namescope.ValueAccessor});");
			}
		//workaround when VSM tries to apply state before parenting (https://github.com/dotnet/maui/issues/16208)
		else if (context.Variables.TryGetValue(elementNode, out var variable) && variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Element")!, context))
			using (PrePost.NewConditional(scopes.setNSScope[scopes.setNSScope.Length - 1].Writer, "!_MAUIXAML_SG_NAMESCOPE_DISABLE"))
			{
				scopes.setNSScope[scopes.setNSScope.Length - 1].Writer.WriteLine($"{context.Variables[elementNode].ValueAccessor}.transientNamescope = {namescope.ValueAccessor};");
			}
		return context.Scopes[elementNode] = (namescope, namesInNamescope);
	}

	//check if we can use the value directly, or if we hhave to create a variable accessor for it
	static bool CanBeSetDirectly(INode propValue, IndentedTextWriter writer, SourceGenContext context)
	{
		if (   propValue is ValueNode valueNode
			&& valueNode.Value is string strValue)
			return true;

		//x2009 language primitives
		if (   propValue is ElementNode elementNode1
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
	
	static bool ShouldSetFieldsInICForXName(ElementNode node, SourceGenContext context)
	{
		if (node.Parent is null)
			return true;
		if (   SetNamescopesAndRegisterNamesVisitor.IsDataTemplate(node, node.Parent)
			|| SetNamescopesAndRegisterNamesVisitor.IsStyle(node, node.Parent)
			|| SetNamescopesAndRegisterNamesVisitor.IsVisualStateGroupList(node))
			return false;
		if (node.Parent is ListNode listNode)
			return ShouldSetFieldsInICForXName(listNode.Parent as ElementNode ?? throw new InvalidOperationException(), context);
		if (node.Parent is ElementNode parent)
			return ShouldSetFieldsInICForXName(parent, context);	
		throw new NotImplementedException();
	}
}