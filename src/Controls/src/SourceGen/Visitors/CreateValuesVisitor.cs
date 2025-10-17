using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;

class CreateValuesVisitor : IXamlNodeVisitor
{
	public CreateValuesVisitor(SourceGenContext context) => Context = context;

	SourceGenContext Context { get; }
	IndentedTextWriter Writer => Context.Writer;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		Context.Values[node] = node.Value;
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
		//At this point, all MarkupNodes are expanded to ElementNodes
	}

	public static void CreateValue(ElementNode node, IndentedTextWriter writer, IDictionary<INode, ILocalValue> variables, Compilation compilation, AssemblyCaches xmlnsCache, SourceGenContext Context, Func<INode, ITypeSymbol, ILocalValue>? getNodeValue = null)
	{
		if (!node.XmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, out var type) || type is null)
			return;

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