using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;

class CreateValuesVisitor : IXamlNodeVisitor
{
	public CreateValuesVisitor(SourceGenContext context, bool stopOnStyle = true)
	{
		Context = context;
		StopOnStyle = stopOnStyle;
	}

	SourceGenContext Context { get; }
	IndentedTextWriter Writer => Context.Writer;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;
	public bool StopOnStyle { get; }
	public bool VisitNodeOnStyle => true; // We need to visit the Style node itself, just not its children
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);
	public bool IsStyle(ElementNode node) => node.IsStyle(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		Context.Values[node] = node.Value;
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
		//At this point, all MarkupNodes are expanded to ElementNodes
	}

	public static void CreateValue(ElementNode node, IndentedTextWriter writer, IDictionary<INode, ILocalValue> variables, Compilation compilation, AssemblyAttributes xmlnsCache, SourceGenContext Context, Func<INode, ITypeSymbol, ILocalValue>? getNodeValue = null)
	{
		// Style initializer may pre-register the Style variable; skip re-creating it.
		if (node.IsStyle(Context) && variables.ContainsKey(node))
			return;

		if (!node.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, Context.TypeCache, out var type) || type is null)
			return;

		// Handle OnPlatform default value nodes - generate default(T) instead of instantiation
		// This is used when OnPlatform has no matching platform and no Default specified
		if (node.IsOnPlatformDefaultValue)
		{
			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
			writer.WriteLine($"{type.ToFQDisplayString()} {variableName} = default;");
			variables[node] = new LocalVariable(type, variableName);
			node.RegisterSourceInfo(Context, writer);
			return;
		}

		// Handle Style with trimmable constructor
		if (node.IsStyle(Context) && TryCreateTrimmableStyle(node, writer, variables, compilation, xmlnsCache, Context))
		{
			return;
		}

		//x:Array
		if (type.Equals(compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
		{
			//we might want to move this to a separate method
			var visitor = new SetPropertiesVisitor(Context);
			// var children = node.Properties.Values.ToList();
			// children.AddRange(node.CollectionItems);
			foreach (var cn in node.CollectionItems)
			{
				if (cn is not ElementNode en)
					continue;
				foreach (var n in en.Properties.Values.ToList())
					n.Accept(visitor, cn);
				foreach (var n in en.CollectionItems)
					n.Accept(visitor, cn);
			}

			var typeNode = node.Properties[new XmlName("", "Type")];
			if (!Context.Types.TryGetValue(typeNode, out var arrayType))
				throw new Exception("ArrayExtension Type not found");

			var variableName = NamingHelpers.CreateUniqueVariableName(Context, compilation.CreateArrayTypeSymbol(arrayType));

			//Provide value for Providers
			foreach (var cn in node.CollectionItems)
			{
				if (cn is not ElementNode en)
					continue;

				en.TryProvideValue(writer, Context);
			}

			variables[node] = new LocalVariable(compilation.CreateArrayTypeSymbol(arrayType), variableName);
			writer.WriteLine($"var {variableName} = new {arrayType.ToFQDisplayString()}[]");
			using (PrePost.NewBlock(writer, begin: "{", end: "};"))
			{
				foreach (var cn in node.CollectionItems)
				{
					if (cn is not ElementNode en)
						continue;

					var enVariable = variables[en];
					writer.WriteLine($"({arrayType.ToFQDisplayString()}){enVariable.ValueAccessor},");
				}
			}

			//clean the node as it has been fully exhausted
			foreach (var prop in node.Properties)
				if (!node.SkipProperties.Contains(prop.Key))
					node.SkipProperties.Add(prop.Key);
			node.CollectionItems.Clear();
			return;
		}

		//if type is Xaml2009Primitive
		if (IsXaml2009LanguagePrimitive(node))
		{
			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
			writer.WriteLine($"{type.ToFQDisplayString()} {variableName} = {ValueForLanguagePrimitive(type, node, Context)};");
			variables[node] = new LocalVariable(type, variableName);
			node.RegisterSourceInfo(Context, writer);
			return;
		}

		if (NodeSGExtensions.GetKnownEarlyMarkupExtensions(Context).TryGetValue(type, out var provideValue)
			&& provideValue(node, writer, Context, null, out var returnType, out var value))
		{
			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
			writer.WriteLine($"var {variableName} = {value};");
			variables[node] = new LocalVariable(returnType!, variableName);
			node.RegisterSourceInfo(Context, writer);

			//skip the node as it has been fully exhausted
			foreach (var prop in node.Properties)
				if (!node.SkipProperties.Contains(prop.Key))
					node.SkipProperties.Add(prop.Key);
			node.CollectionItems.Clear();

			return;
		}

		IMethodSymbol? ctor = null, factoryMethod = null;
		IList<(INode node, ITypeSymbol type, ITypeSymbol? converter)>? parameters = null;
		//ctor with x:Arguments
		if (node.Properties.ContainsKey(XmlName.xArguments) && !node.Properties.ContainsKey(XmlName.xFactoryMethod))
		{
			ctor = type.InstanceConstructors.FirstOrDefault(c
				=> c.MatchXArguments(node, Context, getNodeValue, out parameters));
			if (ctor is null)
#pragma warning disable RS0030 // Do not use banned APIs
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodResolution, LocationCreate(Context.ProjectItem.RelativePath!, node, type.Name), type.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs
		}
		//x:FactoryMethod
		else if (node.Properties.ContainsKey(XmlName.xFactoryMethod))
		{
			var factoryMehodName = (node.Properties[XmlName.xFactoryMethod] as ValueNode)!.Value as string;
			//TODO report diagnostic if factoryMethod is null/not a string

			factoryMethod = type.GetAllMethods(factoryMehodName!, Context).FirstOrDefault(m =>
					   m.IsStatic
					&& m.MatchXArguments(node, Context, getNodeValue, out parameters));
			if (factoryMethod is null)
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodResolution, LocationCreate(Context.ProjectItem.RelativePath!, node, factoryMehodName!), factoryMehodName));
		}

		if (ctor is null && factoryMethod is null)
		{
			//TODO we might need an extension method for that and cache the result. it happens eveytime we have a Style
			ctor = type.InstanceConstructors.FirstOrDefault(c
				=> c.Parameters.Length > 0
				&& c.Parameters.All(p =>
				{
					var parameterattribute = p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Equals(compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ParameterAttribute")!, SymbolEqualityComparer.Default));
					if (parameterattribute is null)
						return false;
					var parametername = parameterattribute.ConstructorArguments[0].Value as string;
					return node.Properties.ContainsKey(new XmlName("", parametername));
				}));

			if (ctor is not null)
			{
				var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
				var paramsTuple = ctor.Parameters
					.Select(p => (p.Type,
									p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Equals(compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ParameterAttribute")!, SymbolEqualityComparer.Default))?.ConstructorArguments[0].Value as string,
									p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Equals(compilation.GetTypeByMetadataName("System.ComponentModel.TypeConverterAttribute")!, SymbolEqualityComparer.Default))?.ConstructorArguments[0].Value as ITypeSymbol
									))
					.Select(p => (p.Type, new XmlName("", p.Item2), node.Properties[new XmlName("", p.Item2)], p.Item3)).ToList();

				parameters = [.. paramsTuple.Select(p => (p.Item3, p.Type, p.Item4))];

				foreach (var n in paramsTuple.Select(p => p.Item2))
					if (!node.SkipProperties.Contains(n))
						node.SkipProperties.Add(n);
			}
		}

		//does it has a default parameterless ctor ?
		ctor ??= type.InstanceConstructors.FirstOrDefault(c => c.Parameters.Length == 0);

		//is there an implicit operator from a string ? (XamlC only supports from string, but this could be extended)
		var implicitOperator = type.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion && m.Parameters.Length == 1 && m.Parameters[0].Type.SpecialType == SpecialType.System_String).FirstOrDefault();

		bool isColor = type.Equals(compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color"), SymbolEqualityComparer.Default);

		//are there any `required` properties ?
		var requiredPropAndFields = type.GetMembers()
									.Where(p => p is IPropertySymbol prop && prop.IsRequired || p is IFieldSymbol field && field.IsRequired)
									.ToList();
		List<(string name, ISymbol propOrField, ITypeSymbol propType, string propValue)> requiredPropertiesAndFields = [];
		if (requiredPropAndFields.Count > 0)
		{
			var contentPropertyName = type.GetContentPropertyName(Context);
			foreach (var req in requiredPropAndFields)
			{
				XmlName propXmlName;
				INode propNode;
				if (req.Name == contentPropertyName && node.CollectionItems.Count == 1)
				{
					propNode = node.CollectionItems[0];
					propXmlName = new XmlName("", contentPropertyName);
				}
				else if (!node.Properties.TryGetValue(req.Name, out propNode, out propXmlName))
					Context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationCreate(Context.ProjectItem.RelativePath!, node, req.Name), $"Required field '{req.Name}' not found in {type}"));

				string propValue = "null";
				var pType = req is IPropertySymbol prop ? prop.Type : ((IFieldSymbol)req).Type;
				var pConverter = req.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Equals(compilation.GetTypeByMetadataName("System.ComponentModel.TypeConverterAttribute")!, SymbolEqualityComparer.Default))?.ConstructorArguments[0].Value as ITypeSymbol;

				var visitor = new SetPropertiesVisitor(Context);
				var children = node.Properties.Values.ToList();
				children.AddRange(node.CollectionItems);
				foreach (var cn in children)
				{
					if (cn is not ElementNode en)
						continue;
					foreach (var n in en.Properties.Values.ToList())
						n.Accept(visitor, cn);
					foreach (var n in en.CollectionItems)
						n.Accept(visitor, cn);
				}
				if (propNode is ValueNode vn)
					propValue = vn.ConvertTo(pType, pConverter, writer, Context);
				else if (propNode is ElementNode en)
				{
					Context.ReportDiagnostic(Diagnostic.Create(Descriptors.RequiredProperty, LocationCreate(Context.ProjectItem.RelativePath!, node, type.Name), $"'{type.ToFQDisplayString()}.{propXmlName.LocalName}'"));
					en.TryProvideValue(writer, Context);
					propValue = variables[en].ValueAccessor;
				}

				requiredPropertiesAndFields.Add((req.Name, req, pType, propValue));
				if (!node.SkipProperties.Contains(new XmlName("", req.Name)))
					node.SkipProperties.Add(new XmlName("", req.Name));
			}
		}

		if (node.CollectionItems.Count == 1
				&& node.CollectionItems[0] is ValueNode valueNode
				&& (isColor || type.IsValueType))
		{ //<Color>HotPink</Color>
			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
			var valueString = valueNode.ConvertTo(type, writer, Context);

			writer.WriteLine($"var {variableName} = {valueString};");
			variables[node] = new LocalVariable(type, variableName);
			node.RegisterSourceInfo(Context, writer);
			return;
		}
		//TODO we could also check if there's a TypeConverter, but we have no test (so no need?) for it
		else if (node.CollectionItems.Count == 1
				&& node.CollectionItems[0] is ValueNode valueNode1
				&& implicitOperator != null)
		{ //<FileImageSource>path.png</FileImageSource>
			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
			var valueString = valueNode1.Value as string;
			writer.WriteLine($"var {variableName} = ({type.ToFQDisplayString()})\"{valueString}\";");
			variables[node] = new LocalVariable(type, variableName);
			node.RegisterSourceInfo(Context, writer);
			return;
		}
		else if (factoryMethod != null)
		{
			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
			writer.WriteLine($"var {variableName} = {factoryMethod.ToFQDisplayString()}({string.Join(", ", parameters?.ToMethodParameters(writer, Context) ?? [])});");
			variables[node] = new LocalVariable(factoryMethod.ReturnType, variableName);
			node.RegisterSourceInfo(Context, writer);
			return;
		}
		else if (ctor != null)
		{
			// Check if this type is a known value provider that can be inlined.
			// Use CanProvideValue to verify that ALL properties can be inlined (no markup extensions).
			if (NodeSGExtensions.GetKnownValueProviders(Context).TryGetValue(type, out var valueProvider) &&
				valueProvider.CanProvideValue(node, Context))
			{
				// This element can be fully inlined without property assignments or variable creation.
				// Skip setting all simple value properties since they'll be handled
				// by inline initialization in TryProvideValue.
				// 
				// This eliminates the dead code that creates:
				// 1. Empty variable instantiation (var setter = new Setter();)
				// 2. Service providers (XamlServiceProvider, SimpleValueTargetProvider, 
				//    XmlNamespaceResolver, XamlTypeResolver) for property assignments
				// 
				// These are not AOT-compatible and were completely dead code.
				//
				// Register a placeholder variable entry so TryProvideValue can replace it
				// with the actual inline instantiation later.
				foreach (var prop in node.Properties)
				{
					if (prop.Value is ValueNode && !node.SkipProperties.Contains(prop.Key))
						node.SkipProperties.Add(prop.Key);
				}

				// Also skip the content property when the value is specified as a collection item.
				// This handles the content property syntax like:
				//   <Setter Property="...">10,10,20,20</Setter>
				// where the value is a collection item instead of a property.
				// Without skipping this, SetPropertiesVisitor would try to process the
				// collection item and generate invalid code like ".Value = ..." 
				// (with an empty variable name).
				if (node.CollectionItems.Count == 1 && node.CollectionItems[0] is ValueNode)
				{
					var contentPropertyName = type.GetContentPropertyName(Context);
					if (!string.IsNullOrEmpty(contentPropertyName))
					{
						var contentPropertyXmlName = new XmlName(node.NamespaceURI, contentPropertyName);
						if (!node.SkipProperties.Contains(contentPropertyXmlName))
							node.SkipProperties.Add(contentPropertyXmlName);
					}
				}

				// Register placeholder - TryProvideValue will create the actual variable
				// Use empty string as placeholder name since it will be replaced
				variables[node] = new LocalVariable(type, string.Empty);
				return;
			}

			var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);

			if (requiredPropAndFields.Any())
			{
				writer.WriteLine($"var {variableName} = new {type.ToFQDisplayString()}({string.Join(", ", parameters?.ToMethodParameters(writer, Context) ?? [])})");
				using (PrePost.NewBlock(writer, begin: "{", end: "};"))
				{
					foreach (var (name, propOrField, propType, propValue) in requiredPropertiesAndFields)
						writer.WriteLine($"{name} = ({propType.ToFQDisplayString()}){propValue},");
				}
			}
			else
				writer.WriteLine($"var {variableName} = new {type.ToFQDisplayString()}({string.Join(", ", parameters?.ToMethodParameters(writer, Context) ?? [])});");
			variables[node] = new LocalVariable(type, variableName);
			node.RegisterSourceInfo(Context, writer);

			return;
		}
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		CreateValue(node, Writer, Context.Variables, Context.Compilation, Context.XmlnsCache, Context);
	}

	public void Visit(ListNode node, INode parentNode)
	{
		if (node.TryGetPropertyName(parentNode, out XmlName name))
			node.XmlName = name;
	}

	public void Visit(RootNode node, INode parentNode)
	{
		var variableName = "__root";

		Writer.WriteLine($"var {variableName} = this;");
		Context.Variables[node] = new LocalVariable(Context.RootType, variableName);

		node.RegisterSourceInfo(Context, Writer);
	}

	/// <summary>
	/// Creates a trimmable Style using the new constructor that takes an assembly-qualified type name string.
	/// This allows the trimmer to remove the Style's target type if it's not used elsewhere.
	/// The Style's Setters/Behaviors/Triggers are marked to be skipped during normal traversal,
	/// and SetPropertiesVisitor will generate the initializer lambda when visiting the Style node.
	/// </summary>
	static bool TryCreateTrimmableStyle(ElementNode node, IndentedTextWriter writer, IDictionary<INode, ILocalValue> variables, Compilation compilation, AssemblyAttributes xmlnsCache, SourceGenContext context)
	{
		var styleType = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Style");
		if (styleType is null)
			return false;

		// Get the TargetType property
		var targetTypeXmlName = new XmlName("", "TargetType");
		if (!node.Properties.TryGetValue(targetTypeXmlName, out var targetTypeNode))
			return false;

		// Get the target type from the node
		INamedTypeSymbol? targetType = null;
		if (targetTypeNode is ValueNode targetTypeValueNode && targetTypeValueNode.Value is string targetTypeString)
		{
			// TargetType="Label" - resolve the type from string
			var xmlType = new XmlType(node.NamespaceURI, targetTypeString, null);
			if (!xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, context.TypeCache, out targetType) || targetType is null)
			{
				// Try to resolve from default namespace
				xmlType = new XmlType("http://schemas.microsoft.com/dotnet/2021/maui", targetTypeString, null);
				xmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, context.TypeCache, out targetType);
			}
		}
		else if (targetTypeNode is ElementNode targetTypeElementNode)
		{
			// TargetType="{x:Type Label}" - the type should already be resolved
			if (context.Types.TryGetValue(targetTypeElementNode, out var resolvedType))
				targetType = resolvedType as INamedTypeSymbol;
		}

		if (targetType is null)
			return false;

		// Generate the assembly-qualified name (without global:: prefix, with assembly name)
		// Type.GetType() expects format: "Namespace.TypeName, AssemblyName"
#pragma warning disable RS0030 // Use banned ToDisplayString to get name without global:: prefix
		var typeFullName = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
#pragma warning restore RS0030
		var assemblyQualifiedName = $"{typeFullName}, {targetType.ContainingAssembly.Name}";

		// Create the Style variable with the trimmable constructor
		var variableName = NamingHelpers.CreateUniqueVariableName(context, styleType);

		// Store the target type for later use by SetPropertiesVisitor
		context.Types[node] = targetType;

		// Mark TargetType as processed (it's embedded in the AQN)
		if (!node.SkipProperties.Contains(targetTypeXmlName))
			node.SkipProperties.Add(targetTypeXmlName);

		// Check if Style has content that needs to go in the initializer
		// Content properties: Setters, Behaviors, Triggers, or implicit Setters (CollectionItems)
		var contentPropertyNames = new[] { new XmlName("", "Setters"), new XmlName("", "Behaviors"), new XmlName("", "Triggers") };
		var hasExplicitContent = contentPropertyNames.Any(p => node.Properties.ContainsKey(p));
		var hasImplicitContent = node.CollectionItems.Count > 0;
		
		if (hasExplicitContent || hasImplicitContent)
		{
			// Mark content properties to be processed in the initializer (skip during normal traversal)
			foreach (var propName in contentPropertyNames)
			{
				if (node.Properties.ContainsKey(propName) && !node.SkipProperties.Contains(propName))
					node.SkipProperties.Add(propName);
			}
			
			// Mark that this Style needs an initializer
			node.Properties[XmlName._StyleContent] = new ValueNode("true", node.NamespaceResolver);
		}

		// Write the Style constructor
		writer.WriteLine($"var {variableName} = new {styleType.ToFQDisplayString()}(\"{assemblyQualifiedName}\");");

		// Register the Style variable
		variables[node] = new LocalVariable(styleType, variableName);
		node.RegisterSourceInfo(context, writer);

		return true;
	}


	internal static bool IsXaml2009LanguagePrimitive(ElementNode node)
	{
		// if (node.NamespaceURI == XamlParser.X2009Uri)
		// {
		//     return node.XmlType.Name != "Array";
		// }
		if (node.NamespaceURI != "clr-namespace:System;assembly=mscorlib" && node.NamespaceURI != XamlParser.X2009Uri)
			return false;
		var name = node.XmlType.Name;
		if (name == "SByte" ||
			name == "Int16" ||
			name == "Int32" ||
			name == "Int64" ||
			name == "Byte" ||
			name == "UInt16" ||
			name == "UInt32" ||
			name == "UInt64" ||
			name == "Single" ||
			name == "Double" ||
			name == "Boolean" ||
			name == "String" ||
			name == "Char" ||
			name == "Decimal" ||
			name == "TimeSpan" ||
			name == "Uri")
			return true;
		return false;
	}

	static string ValueForLanguagePrimitive(ITypeSymbol type, ElementNode node, SourceGenContext context)
	{
		if (!(node.CollectionItems.Count == 1
			&& node.CollectionItems[0] is ValueNode node1
			&& node1.Value is string valueString))
			valueString = string.Empty;

		return NodeSGExtensions.ValueForLanguagePrimitive(valueString, toType: type, context, node);
	}
}
