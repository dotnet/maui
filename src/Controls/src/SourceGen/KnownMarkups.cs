using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

using static System.String;

namespace Microsoft.Maui.Controls.SourceGen;

internal class KnownMarkups
{
	public static bool ProvideValueForStaticExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.ObjectType;
		if (!markupNode.Properties.TryGetValue(new XmlName("", "Member"), out INode ntype)
			&& !markupNode.Properties.TryGetValue(new XmlName(null, "Member"), out ntype))
			ntype = markupNode.CollectionItems[0];
		var member = ((ValueNode)ntype).Value as string;

		if (IsNullOrEmpty(member) || !member!.Contains("."))
		{
			//FIXME
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null));
			value = string.Empty;
			return false;
		}

		var dotIdx = member.LastIndexOf('.');
		var typename = member.Substring(0, dotIdx);
		var membername = member.Substring(dotIdx + 1);

		var typeSymbol = typename.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache, markupNode);
		if (typeSymbol == null)
		{
			//FIXME
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, $"Type not found {typename}"));
			value = string.Empty;
			return false;
		}
		var field = typeSymbol.GetAllFields(membername, context).FirstOrDefault(f => f.IsStatic);
		var property = typeSymbol.GetAllProperties(membername, context).FirstOrDefault(p => p.IsStatic);

		//TODO handle enums, or contstants
		if (field == null && property == null)
		{
			//FIXME
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, "Member not found"));
			value = string.Empty;
			return false;
		}
		if (field != null)
		{
			returnType = field.Type;
			value = field.ToFQDisplayString();
			return true;
		}
		if (property != null)
		{
			returnType = property.Type;
			value = property.ToFQDisplayString();
			return true;
		}

		//should never happen
		value = string.Empty;
		return false;
	}

	public static ITypeSymbol? GetTypeFromTypeExtension(ElementNode xTypeNode, SourceGenContext context)
	{
		if (!xTypeNode.Properties.TryGetValue(new XmlName("", "TypeName"), out INode? typeNameNode)
			&& !xTypeNode.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "TypeName"), out typeNameNode)
			&& xTypeNode.CollectionItems.Count == 1)
			typeNameNode = xTypeNode.CollectionItems[0];

		if (typeNameNode is not ValueNode vn)
		{
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, $"invalid x:Type"));
			return null;
		}

		var typeName = vn.Value as string;
		if (IsNullOrEmpty(typeName))
		{
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, $"invalid x:Type"));
			return null;
		}

		var typeSymbol = typeName!.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache, xTypeNode);
		if (typeSymbol == null)
		{
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, $"Type not found {typeSymbol}"));
			return null;
		}

		return typeSymbol;
	}

	public static bool ProvideValueForTypeExtension(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.GetTypeByMetadataName("System.Type")!;
		var typeSymbol = GetTypeFromTypeExtension(node, context);
		if (typeSymbol == null)
		{
			value = "null";
			return false;
		}

		context.Types[node] = typeSymbol;
		value = $"typeof({typeSymbol.ToFQDisplayString()})";
		return true;
	}

	public static bool ProvideValueForSetter(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!;

		if (!node.Properties.TryGetValue(new XmlName("", "Value"), out INode? valueNode) &&
			!node.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Value"), out valueNode) &&
			node.CollectionItems.Count == 1)
			valueNode = node.CollectionItems[0];

		var bpNode = (ValueNode)node.Properties[new XmlName("", "Property")];
		var bpRef = bpNode.GetBindableProperty(context);

		string targetsetter;
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode))
			targetsetter = $"TargetName = \"{((ValueNode)targetNode).Value}\", ";
		else
			targetsetter = "";

		if (valueNode is ValueNode vn)
		{
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {vn.ConvertTo(bpRef, writer, context)}}}";
			return true;
		}
		else if (getNodeValue != null)
		{
			var lvalue = getNodeValue(valueNode, bpRef.Type);
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {lvalue.ValueAccessor}}}";
			return true;
		}
		else if (context.Variables.TryGetValue(valueNode, out var variable))
		{
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {variable.ValueAccessor}}}";
			return true;
		}

		value = string.Empty;
		//FIXME context.ReportDiagnostic
		return false;
	}

	public static bool ProvideValueForDynamicResourceExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.DynamicResource")!;
		string? key = null;
		if (markupNode.CollectionItems.Count == 1)
			key = ((ValueNode)markupNode.CollectionItems[0]).Value as string;
		else if (markupNode.Properties.TryGetValue(new XmlName("", "Key"), out var keyNode))
			key = ((ValueNode)keyNode).Value as string;

		if (key is null)
			throw new Exception();
		value = $"new global::Microsoft.Maui.Controls.Internals.DynamicResource(\"{key}\")";
		return true;
	}

	internal static bool ProvideValueForStyleSheetExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.StyleSheets.StyleSheet")!;

		markupNode.Properties.TryGetValue(new XmlName("", "Source"), out INode? sourceNode);
		if (sourceNode == null)
			markupNode.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Source"), out sourceNode);

		if (!markupNode.Properties.TryGetValue(new XmlName("", "Style"), out INode? styleNode) &&
			!markupNode.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Style"), out styleNode) &&
			markupNode.CollectionItems.Count == 1)
			styleNode = markupNode.CollectionItems[0];

		if (sourceNode != null && styleNode != null)
			throw new Exception(); //FIXME report diagnostic
								   // throw new BuildException(BuildExceptionCode.StyleSheetSourceOrContent, node, null);

		if (sourceNode == null && styleNode == null)
			throw new Exception(); //FIXME report diagnostic
								   // throw new BuildException(BuildExceptionCode.StyleSheetNoSourceOrContent, node, null);

		if (styleNode != null && styleNode is not ValueNode)
			throw new Exception(); //FIXME report diagnostic
								   // throw new BuildException(BuildExceptionCode.StyleSheetStyleNotALiteral, node, null);

		if (sourceNode != null && sourceNode is not ValueNode)
			throw new Exception(); //FIXME report diagnostic
								   // throw new BuildException(BuildExceptionCode.StyleSheetSourceNotALiteral, node, null);

		if (styleNode != null)
		{
			value = $"global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromString(@\"{(styleNode as ValueNode)!.Value as string}\")";
			return true;
		}
		else // sourceNode != null
		{
			var source = (string)(sourceNode as ValueNode)!.Value;
			var uri = $"new global::System.Uri(\"{source}\", global::System.UriKind.Relative)";
			var rootTargetPath = $"global::Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute.GetPathForType(typeof({context.RootType.ToFQDisplayString()}))";

			var resourcePath = $"global::Microsoft.Maui.Controls.ResourceDictionary.RDSourceTypeConverter.GetResourcePath({uri}, {rootTargetPath})";
			var assembly = $"typeof({context.RootType.ToFQDisplayString()}).Assembly";

			var lineInfo = $"new global::Microsoft.Maui.Controls.Xaml.XmlLineInfo({((IXmlLineInfo)markupNode).LineNumber}, {((IXmlLineInfo)markupNode).LinePosition})";
			value = $"global::Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromResource({resourcePath}, {assembly}, {lineInfo})";
			return true;
		}
	}

	internal static bool ProvideValueForTemplateBindingExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		return ProvideValueForBindingExtension(markupNode, writer, context, isTemplateBinding: true, getNodeValue, out returnType, out value);
	}

	internal static bool ProvideValueForBindingExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		return ProvideValueForBindingExtension(markupNode, writer, context, isTemplateBinding: false, getNodeValue, out returnType, out value);
	}

	private static bool ProvideValueForBindingExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, bool isTemplateBinding, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingBase")!;
		ITypeSymbol? dataTypeSymbol = null;
		if (context.Variables.TryGetValue(markupNode, out ILocalValue extVariable)
			&& TryGetXDataType(markupNode, context, out dataTypeSymbol)
			&& dataTypeSymbol is not null)
		{
			var compiledBindingMarkup = new CompiledBindingMarkup(markupNode, GetBindingPath(markupNode), extVariable, context);
			if (compiledBindingMarkup.TryCompileBinding(dataTypeSymbol, isTemplateBinding, out string? newBindingExpression) && newBindingExpression is not null)
			{
				value = newBindingExpression;
				return true;
			}
		}

		// fallback to the string-based binding
		var dataTypeExpression = dataTypeSymbol is not null
			? $"typeof({dataTypeSymbol.ToFQDisplayString()})"
			: "null";

		string expression;
		if (extVariable != null)
		{
			var source = isTemplateBinding
			? "global::Microsoft.Maui.Controls.RelativeBindingSource.TemplatedParent"
			: $"{extVariable.ValueAccessor}.Source";

			expression = $"new global::Microsoft.Maui.Controls.Binding(" +
			   $"{extVariable.ValueAccessor}.Path, " +
			   $"{extVariable.ValueAccessor}.Mode, " +
			   $"{extVariable.ValueAccessor}.Converter, " +
			   $"{extVariable.ValueAccessor}.ConverterParameter, " +
			   $"{extVariable.ValueAccessor}.StringFormat, " +
			   $"{source}) ";
			if (isTemplateBinding)
			{
				// TemplateBindingExtension doesn't have all the same properties as BindingExtension
				value = expression;
				return true;
			}
			else
			{
				value = expression +
					$"{{ UpdateSourceEventName = {extVariable.ValueAccessor}.UpdateSourceEventName, " +
					$"FallbackValue = {extVariable.ValueAccessor}.FallbackValue, " +
					$"TargetNullValue = {extVariable.ValueAccessor}.TargetNullValue }}";
				return true;
			}
		}
		else //this is newer sourcegen, dependency first
		{
			if (getNodeValue is null)
				throw new ArgumentNullException(nameof(getNodeValue));

			string path = "\".\"";
			if (markupNode.Properties.TryGetValue(new XmlName(null, "Path"), out var pnode))
				path = getNodeValue(pnode, context.Compilation.GetTypeByMetadataName("System.String")!).ValueAccessor;
			else if (markupNode.CollectionItems.Count == 1)
				path = getNodeValue(markupNode.CollectionItems[0], context.Compilation.GetTypeByMetadataName("System.String")!).ValueAccessor;
			expression = $"new global::Microsoft.Maui.Controls.Binding({path}";
			if (markupNode.Properties.TryGetValue(new XmlName(null, "Mode"), out var modeNode))
				expression += $", mode: {getNodeValue(modeNode, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingMode")!).ValueAccessor}"; //FIXME
			if (markupNode.Properties.TryGetValue(new XmlName(null, "Converter"), out var converterNode))
				expression += $", converter: {getNodeValue(converterNode, context.Compilation.GetTypeByMetadataName("System.Object")!).ValueAccessor}";
			if (markupNode.Properties.TryGetValue(new XmlName(null, "ConverterParameter"), out var converterParameterNode))
				expression += $", converterParameter: {getNodeValue(converterParameterNode, context.Compilation.GetTypeByMetadataName("System.Object")!).ValueAccessor}";
			if (markupNode.Properties.TryGetValue(new XmlName(null, "StringFormat"), out var stringFormatNode))
				expression += $", stringFormat:{getNodeValue(stringFormatNode, context.Compilation.GetTypeByMetadataName("System.String")!).ValueAccessor}";
			if (isTemplateBinding)
			{
				expression += $", source:global::Microsoft.Maui.Controls.RelativeBindingSource.TemplatedParent";
			}
			else
			{
				if (markupNode.Properties.TryGetValue(new XmlName(null, "Source"), out var sourceNode))
					expression += $", source: {getNodeValue(sourceNode, context.Compilation.GetTypeByMetadataName("System.String")!).ValueAccessor}";
				expression += ") {";
				if (markupNode.Properties.TryGetValue(new XmlName(null, "UpdateSourceEventName"), out var updateSourceEventNameNode))
					expression += $"UpdateSourceEventName = {getNodeValue(updateSourceEventNameNode, context.Compilation.GetTypeByMetadataName("System.String")!).ValueAccessor}, ";
				if (markupNode.Properties.TryGetValue(new XmlName(null, "FallbackValue"), out var fallbackValueNode))
					expression += $"FallbackValue = {getNodeValue(fallbackValueNode, context.Compilation.GetTypeByMetadataName("System.Object")!).ValueAccessor}, ";
				if (markupNode.Properties.TryGetValue(new XmlName(null, "TargetNullValue"), out var targetNullValueNode))
					expression += $"TargetNullValue = {getNodeValue(targetNullValueNode, context.Compilation.GetTypeByMetadataName("System.Object")!).ValueAccessor}, ";
			}

			expression += "}";
			value = expression;
			return true;
		}


		static string GetBindingPath(ElementNode node)
		{
			if (node.Properties.TryGetValue(new XmlName("", "Path"), out var pathNode)
				&& pathNode is ValueNode { Value: string pathValue })
			{
				return pathValue;
			}
			else if (node.CollectionItems.Count == 1
				&& node.CollectionItems[0] is ValueNode { Value: string singleCollectionItemValue })
			{
				return singleCollectionItemValue;
			}
			else
			{
				// no path
				// TODO report diagnostic
				return ".";
			}
		}

		static bool TryGetXDataType(ElementNode node, SourceGenContext context, out ITypeSymbol? dataTypeSymbol)
		{
			dataTypeSymbol = null;

			if (!TryFindXDataTypeNode(node, context, out INode? dataTypeNode, out bool xDataTypeIsInOuterScope)
				|| dataTypeNode is null)
			{
				return false;
			}

			var location = LocationHelpers.LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, "x:DataType");

			if (xDataTypeIsInOuterScope)
			{
				// TODO
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, "Binding with x:DataType from outer scope"));
				// _context.LoggingHelper.LogWarningOrError(BuildExceptionCode.BindingWithXDataTypeFromOuterScope, context.XamlFilePath, node.LineNumber, node.LinePosition, 0, 0, null);
				// continue compilation
			}

			if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "NullExtension"))
			{
				// TODO report warning
				// context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, "Binding with x:DataType NullExtension"));
				// context.LoggingHelper.LogWarningOrError(BuildExceptionCode.BindingWithNullDataType, context.XamlFilePath, node.LineNumber, node.LinePosition, 0, 0, null);
				return false;
			}

			// TypeExtension would already provide the type value, so we can just grab it from the context
			if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "TypeExtension"))
			{
				// it is possible that the dataTypeNode belongs to the parent context
				// this is the case for example in this scenario:
				//
				//    <DataTemplate x:DataType="local:ItemViewModel">
				//        <Label Text="{Binding ItemTitle}" />
				//    </DataTemplate>
				//
				SourceGenContext? ctx = context;
				while (ctx is not null)
				{
					if (ctx.Types.TryGetValue(dataTypeNode, out dataTypeSymbol))
					{
						return true;
					}

					ctx = ctx.ParentContext;
				}

				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, "Binding with x:DataType TypeExtension which cannot be resolved"));
				return false;
			}

			string? dataTypeName = (dataTypeNode as ValueNode)?.Value as string;
			if (dataTypeName is null)
			{
				// TODO
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, $"dataTypeNode is not a value node, got {dataTypeNode} instead"));
				// throw new BuildException(XDataTypeSyntax, dataTypeNode as IXmlLineInfo, null);
				// throw new Exception($"dataTypeNode {dataTypeNode} is not a value node"); // TODO
				return false;
			}

			XmlType? dataType = null;
			try
			{
				dataType = TypeArgumentsParser.ParseSingle(dataTypeName, node.NamespaceResolver, dataTypeNode as IXmlLineInfo);
			}
			catch (XamlParseException)
			{
				var prefix = dataTypeName.Contains(":") ? dataTypeName.Substring(0, dataTypeName.IndexOf(":", StringComparison.Ordinal)) : "";
				// throw new BuildException(XmlnsUndeclared, dataTypeNode as IXmlLineInfo, null, prefix);
				throw new Exception($"XmlnsUndeclared {prefix}"); // TODO
			}

			if (dataType is null)
			{
				// TODO
				// throw new BuildException(XDataTypeSyntax, dataTypeNode as IXmlLineInfo, null);
				// throw new Exception($"cannot parse {dataTypeName}"); // TODO
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, "Cannot parse x:DataType value"));
				return false;
			}

			if (!dataType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, out INamedTypeSymbol? symbol) && symbol is not null)
			{
				// TODO report the right diagnostic
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, "Cannot resolve x:DataType type"));
				return false;
			}

			dataTypeSymbol = symbol;
			return true;
		}

		static bool TryFindXDataTypeNode(ElementNode elementNode, SourceGenContext context, out INode? dataTypeNode, out bool isInOuterScope)
		{
			isInOuterScope = false;
			dataTypeNode = null;

			// Special handling for BindingContext={Binding ...}
			// The order of checks is:
			// - x:DataType on the binding itself
			// - SKIP looking for x:DataType on the parent
			// - continue looking for x:DataType on the parent's parent...
			ElementNode? skipNode = null;
			if (IsBindingContextBinding(elementNode))
			{
				skipNode = GetParent(elementNode);
			}

			ElementNode? node = elementNode;
			while (node is not null)
			{
				if (node != skipNode && node.Properties.TryGetValue(XmlName.xDataType, out dataTypeNode))
				{
					return true;
				}

				if (DoesNotInheritDataType(node, context))
				{
					return false;
				}

				// When the binding is inside of a DataTemplate and the x:DataType is in the parent scope,
				// it is usually a bug.
				if (node.RepresentsType(XamlParser.MauiUri, "DataTemplate"))
				{
					isInOuterScope = true;
				}

				node = GetParent(node);
			}

			return false;
		}

		static bool DoesNotInheritDataType(ElementNode node, SourceGenContext context)
		{
			return GetParent(node) is ElementNode parentNode
				&& parentNode.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, out INamedTypeSymbol? parentTypeSymbol)
				&& parentTypeSymbol is not null
				&& node.TryGetPropertyName(parentNode, out XmlName propertyName)
				&& parentTypeSymbol.GetAllProperties(propertyName.LocalName, context).FirstOrDefault() is IPropertySymbol propertySymbol
				&& propertySymbol.GetAttributes().Any(a => a.AttributeClass?.ToFQDisplayString() == "global::Microsoft.Maui.Controls.Xaml.DoesNotInheritDataTypeAttribute");
		}

		static ElementNode? GetParent(ElementNode node)
		{
			return node switch
			{
				{ Parent: ListNode { Parent: ElementNode parentNode } } => parentNode,
				{ Parent: ElementNode parentNode } => parentNode,
				_ => null,
			};
		}

		static bool IsBindingContextBinding(ElementNode node)
		{
			// looking for BindingContext="{Binding ...}"
			return GetParent(node) is ElementNode parentNode
				&& node.TryGetPropertyName(parentNode, out var propertyName)
				&& propertyName.NamespaceURI == ""
				&& propertyName.LocalName == "BindingContext";
		}
	}

	internal static bool ProvideValueForReferenceExtension(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		// should be possible to return the right value, as soon as we no longer use the namescope
		returnType = context.Compilation.ObjectType;
		if (!markupNode.Properties.TryGetValue(new XmlName("", "Name"), out INode refNameNode)
			&& !markupNode.Properties.TryGetValue(new XmlName(null, "Name"), out refNameNode))
			refNameNode = markupNode.CollectionItems[0];
		var name = ((ValueNode)refNameNode).Value as string;

		var node = markupNode;
		var currentcontext = context;
		while (currentcontext is not null && node is not null)
		{
			while (currentcontext is not null && !currentcontext.Scopes.ContainsKey(node))
				currentcontext = context.ParentContext;
			if (currentcontext is null)
				break;
			var namescope = currentcontext.Scopes[node];
			if (namescope.namesInScope != null && namescope.namesInScope.ContainsKey(name!))
			{
				returnType = namescope.namesInScope[name!].Type;
				value = $"{namescope.namesInScope[name!].ValueAccessor}";
				return true;
			}
			INode n = node;
			while (n.Parent is ListNode ln)
				n = ln.Parent;
			node = n.Parent as ElementNode;
		}

		//TODO report diagnostic
		context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, null, $"ReferenceExtension: Name '{name}' not found in any NameScope"));
		value = string.Empty;
		return false; // or throw an exception?
	}

	/// <summary>
	/// Provides value for AppThemeBindingExtension by generating an AppThemeBinding instance
	/// with Light, Dark, and Default properties set based on the markup extension's properties.
	/// </summary>
	internal static bool ProvideValueForAppThemeBindingExtension(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.AppThemeBinding")!;

		if (getNodeValue is null)
		{
			// Fallback when we don't have getNodeValue - shouldn't happen in normal flow
			value = string.Empty;
			return false;
		}

		// Get the target property type for type conversion
		ITypeSymbol? propertyType = null;
		if (node.TryGetPropertyName(node.Parent, out XmlName propertyName) && context.Variables.TryGetValue(node.Parent, out ILocalValue parentVar))
		{
			var localName = propertyName.LocalName;
			var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, node as IXmlLineInfo);
			var propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
			var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(context);
			propertyType = typeandconverter?.type ?? propertySymbol?.Type;
		}

		// Default to object if we can't determine property type
		if (propertyType is null)
			propertyType = context.Compilation.ObjectType;

		// Extract Light, Dark, and Default values from the markup extension
		string? lightValue = null;
		string? darkValue = null;
		string? defaultValue = null;

		// Check for Default property (can be in CollectionItems as it's the ContentProperty)
		if (node.Properties.TryGetValue(new XmlName("", "Default"), out INode? defaultNode)
			|| node.Properties.TryGetValue(new XmlName(null, "Default"), out defaultNode)
			|| (node.CollectionItems.Count == 1 && (defaultNode = node.CollectionItems[0]) != null))
		{
			var defaultLocal = getNodeValue(defaultNode, propertyType);
			defaultValue = defaultLocal.ValueAccessor;
		}

		// Check for Light property
		if (node.Properties.TryGetValue(new XmlName("", "Light"), out INode? lightNode)
			|| node.Properties.TryGetValue(new XmlName(null, "Light"), out lightNode))
		{
			var lightLocal = getNodeValue(lightNode, propertyType);
			lightValue = lightLocal.ValueAccessor;
		}

		// Check for Dark property
		if (node.Properties.TryGetValue(new XmlName("", "Dark"), out INode? darkNode)
			|| node.Properties.TryGetValue(new XmlName(null, "Dark"), out darkNode))
		{
			var darkLocal = getNodeValue(darkNode, propertyType);
			darkValue = darkLocal.ValueAccessor;
		}

		// At least one value must be set
		if (lightValue is null && darkValue is null && defaultValue is null)
		{
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError,
				LocationHelpers.LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ""),
				"AppThemeBindingExtension requires a non-null value to be specified for at least one theme or Default"));
			value = string.Empty;
			return false;
		}

		// Build the AppThemeBinding initialization expression
		var parts = new List<string>();

		if (lightValue is not null)
			parts.Add($"Light = {lightValue}");

		if (darkValue is not null)
			parts.Add($"Dark = {darkValue}");

		if (defaultValue is not null)
			parts.Add($"Default = {defaultValue}");

		value = $"new global::Microsoft.Maui.Controls.AppThemeBinding {{ {string.Join(", ", parts)} }}";
		return true;
	}

	//all of this could/should be better, but is already slightly better than XamlC
	internal static bool ProvideValueForStaticResourceExtension(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		// If the resource is defined locally, we can return the value directly
		var eNode = (node as ElementNode)!;

		if (!eNode.Properties.TryGetValue(new XmlName(null, "Key"), out INode keyNode) && eNode.CollectionItems.Count != 0)
			keyNode = eNode.CollectionItems[0];
		if (keyNode == null && !eNode.Properties.TryGetValue(new XmlName("", "Key"), out keyNode) && eNode.CollectionItems.Count != 0)
			keyNode = eNode.CollectionItems[0];

		if (keyNode is not ValueNode keyValueNode)
		{
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationHelpers.LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ""), "StaticResourceExtension: Key must be a string literal"));
			returnType = context.Compilation.ObjectType;
			value = string.Empty;
			return false;
		}

		var resource = GetResourceNode(eNode, context, (string)keyValueNode.Value);
		if (resource != null && getNodeValue != null)
		{
			var lvalue = getNodeValue(resource, context.Compilation.ObjectType);
			value = lvalue.ValueAccessor;
			returnType = lvalue.Type;
			return true;
		}

		if (resource is null || !context.Variables.TryGetValue(resource, out var variable))
		{
			returnType = context.Compilation.ObjectType;
			value = string.Empty;
			return false;
		}


		//if the resource is a string, try to convert it
		if (resource.CollectionItems.Count == 1 && resource.CollectionItems[0] is ValueNode vn && vn.Value is string)
		{
			if (node.TryGetPropertyName(node.Parent, out XmlName propertyName) && context.Variables.TryGetValue(node.Parent, out ILocalValue parentVar))
			{
				var localName = propertyName.LocalName;
				var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, node as IXmlLineInfo);
				var propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
				var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(context);

				var propertyType = typeandconverter?.type ?? propertySymbol?.Type;

				if (propertyType!.Equals(context.Compilation.GetSpecialType(SpecialType.System_String), SymbolEqualityComparer.Default))
				{
					value = $"\"{vn.Value}\"";
					returnType = context.Compilation.GetSpecialType(SpecialType.System_String);
					return true;
				}
				try
				{
					value = vn.ConvertTo(propertyType!, typeandconverter?.converter, writer, context, parentVar);
				}
				catch (Exception)
				{
					//shouldn't happen, but does
					value = string.Empty;
					returnType = context.Compilation.ObjectType;
					return false;
				}
				returnType = propertyType;
				return true;
			}
		}

		//Fallback to runtime resolution of StaticResource
		returnType = context.Compilation.ObjectType;
		value = string.Empty;
		return false;
	}

	//FIXME this could be smarter and look into merged RDs
	static ElementNode? GetResourceNode(ElementNode en, SourceGenContext context, string key)
	{
		var n = en;
		while (n != null)
		{
			if (!n.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Resources"), out var resourcesNode))
			{
				var np = n.Parent;
				if (np is ElementNode pen)
					n = pen;
				else if (np is ListNode lnp && lnp.Parent is ElementNode elnp)
					n = elnp;
				else
					n = null;
				continue;
			}
			//single resource in <Resources>
			if (resourcesNode is ElementNode irn
				&& irn.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode)
				&& context.Variables.ContainsKey(irn)
				&& xKeyNode is ValueNode xKeyValueNode
				&& xKeyValueNode.Value as string == key)
			{
				return irn as ElementNode;
			}
			//multiple resources in <Resources>
			else if (resourcesNode is ListNode lr)
			{
				foreach (var rn in lr.CollectionItems)
				{
					if (rn is ElementNode irn2
						&& irn2.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode2)
						&& context.Variables.ContainsKey(irn2)
						&& xKeyNode2 is ValueNode xKeyValueNode2
						&& xKeyValueNode2.Value as string == key)
					{
						return irn2 as ElementNode;
					}
				}
			}
			//explicit ResourceDictionary in Resources
			else if (resourcesNode is ElementNode resourceDictionary
					&& resourceDictionary.XmlType.Name == "ResourceDictionary")
			{
				foreach (var rn in resourceDictionary.CollectionItems)
				{
					if (rn is ElementNode irn3
						&& irn3.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode3)
						&& irn3.XmlType.Name != "OnPlatform"
						&& context.Variables.ContainsKey(irn3)
						&& xKeyNode3 is ValueNode xKeyValueNode3
						&& xKeyValueNode3.Value as string == key)
					{
						return irn3 as ElementNode;
					}
				}
			}

			n = n.Parent as ElementNode;
		}
		return null;
	}
}