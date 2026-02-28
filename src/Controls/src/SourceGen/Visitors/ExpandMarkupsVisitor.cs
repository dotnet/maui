using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class ExpandMarkupsVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	record XmlLineInfoProvider(IXmlLineInfo XmlLineInfo) : IXmlLineInfoProvider
	{
	}

	record SGContextProvider(SourceGenContext Context)
	{
	}

	public static readonly IList<XmlName> Skips =
	[
		XmlName.xKey,
		XmlName.xTypeArguments,
		XmlName.xFactoryMethod,
		XmlName.xName,
	];

	SourceGenContext Context { get; } = context;
	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => false;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool StopOnStyle => false;
	public bool VisitNodeOnStyle => true;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);
	public bool IsStyle(ElementNode node) => node.IsStyle(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		// Skip escaped values (e.g., Text="{}{Foo}" - the {} prefix means "treat as literal")
		if (node.IsEscaped)
			return;

		// Handle C# expressions in element content (e.g., CDATA or plain text)
		// Example: <Label.IsVisible><![CDATA[{A && B}]]></Label.IsVisible>
		if (node.Value is not string valueString)
			return;

		var trimmed = valueString.Trim();
		if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
			return;

		// Check if this is in a property context
		if (!node.TryGetPropertyName(parentNode, out var propertyName))
			return;
		if (Skips.Contains(propertyName))
			return;

		// Check if it's a C# expression
		if (CSharpExpressionHelpers.IsExpression(trimmed, name => TryResolveMarkupExtensionType(name, node.NamespaceResolver)))
		{
			if (!Context.ProjectItem.EnablePreviewFeatures)
			{
				var location = LocationHelpers.LocationCreate(Context.ProjectItem.RelativePath!, node, trimmed);
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.CSharpExpressionsRequirePreviewFeatures, location));
				return;
			}

			// Async lambda event handlers are not supported
			if (CSharpExpressionHelpers.IsAsyncLambdaEventHandler(trimmed))
			{
				var location = LocationHelpers.LocationCreate(Context.ProjectItem.RelativePath!, node, trimmed);
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.AsyncLambdaNotSupported, location));
				return;
			}

			// Extract expression code - single quotes are always transformed to double quotes
			var expressionCode = CSharpExpressionHelpers.GetExpressionCode(trimmed);
			node.Value = new Expression(expressionCode);
		}
	}

	public void Visit(MarkupNode markupnode, INode parentNode)
	{
		if (!markupnode.TryGetPropertyName(parentNode, out var propertyName))
			return;
		if (Skips.Contains(propertyName))
			return;
		if (parentNode is not ElementNode parentElement || parentElement.SkipProperties.Contains(propertyName))
			return;

		var markupString = markupnode.MarkupString;
		INode? node = null;

		// Get thisType and dataType for ambiguity checking
		ITypeSymbol? thisType = null;
		ITypeSymbol? dataType = null;
		if (Context.ProjectItem.EnablePreviewFeatures)
		{
			// Try to get the page/view type (this)
			var rootElement = GetRootElement(parentElement);
			if (rootElement?.XmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out var resolvedThisType) == true)
				thisType = resolvedThisType;
			
			// Try to get x:DataType
			XDataTypeResolver.TryGetXDataType(parentElement, Context, out dataType);
		}

		// Check if this is a C# expression (consolidates all detection logic)
		var classification = CSharpExpressionHelpers.ClassifyExpression(
			markupString, 
			name => TryResolveMarkupExtensionType(name, markupnode.NamespaceResolver),
			name => CanResolveAsProperty(name, thisType, dataType));

		// Report ambiguity warning if both markup extension and property exist
		if (classification.IsAmbiguous && classification.Name != null && Context.ProjectItem.EnablePreviewFeatures)
		{
			var location = LocationHelpers.LocationCreate(Context.ProjectItem.RelativePath!, markupnode, markupString);
			Context.ReportDiagnostic(Diagnostic.Create(
				Descriptors.AmbiguousExpressionOrMarkup, 
				location, 
				classification.Name));
		}

		if (classification.IsExpression)
		{
			// C# expressions require EnablePreviewFeatures
			if (!Context.ProjectItem.EnablePreviewFeatures)
			{
				var location = LocationHelpers.LocationCreate(Context.ProjectItem.RelativePath!, markupnode, markupString);
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.CSharpExpressionsRequirePreviewFeatures, location));
				// Fall through to parse as markup extension (will likely fail, but gives better error context)
			}
			// Async lambda event handlers are not supported
			else if (CSharpExpressionHelpers.IsAsyncLambdaEventHandler(markupString))
			{
				var location = LocationHelpers.LocationCreate(Context.ProjectItem.RelativePath!, markupnode, markupString);
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.AsyncLambdaNotSupported, location));
				return;
			}
			else
			{
				// Extract expression code - single quotes are always transformed to double quotes
				var expressionCode = CSharpExpressionHelpers.GetExpressionCode(markupString);
				node = new ValueNode(new Expression(expressionCode), markupnode.NamespaceResolver, markupnode.LineNumber, markupnode.LinePosition);
			}
		}

		// If not an expression, parse as markup extension
		node ??= ParseExpression(ref markupString, markupnode.NamespaceResolver, markupnode, markupnode, parentNode);

		if (node != null)
		{
			parentElement.Properties[propertyName] = node;
			node.Parent = parentNode;
		}
	}

	/// <summary>
	/// Checks if a bare identifier can be resolved as a property on thisType or dataType.
	/// </summary>
	bool CanResolveAsProperty(string name, ITypeSymbol? thisType, ITypeSymbol? dataType)
	{
		if (thisType != null && HasMember(thisType, name))
			return true;
		if (dataType != null && HasMember(dataType, name))
			return true;
		return false;
	}

	/// <summary>
	/// Checks if a type has a property or field with the given name.
	/// </summary>
	static bool HasMember(ITypeSymbol type, string memberName)
	{
		var currentType = type;
		while (currentType != null)
		{
			foreach (var member in currentType.GetMembers(memberName))
			{
				if (member is IPropertySymbol || member is IFieldSymbol)
					return true;
			}
			currentType = currentType.BaseType;
		}
		return false;
	}

	/// <summary>
	/// Gets the root element (page/view) from an element node.
	/// </summary>
	static ElementNode? GetRootElement(ElementNode node)
	{
		ElementNode current = node;
		while (true)
		{
			var parent = current.Parent;
			if (parent is null)
				return current;
			if (parent is ElementNode parentElement)
				current = parentElement;
			else if (parent is ListNode listNode && listNode.Parent is ElementNode listParent)
				current = listParent;
			else
				return current;
		}
	}

	bool TryResolveMarkupExtensionType(string name, IXmlNamespaceResolver nsResolver)
	{
		// Try to resolve FooExtension first, then Foo
		var namespaceUri = nsResolver.LookupNamespace("") ?? "";
		
		var xmlTypeExt = new XmlType(namespaceUri, name + "Extension", null);
		if (xmlTypeExt.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out _))
			return true;

		var xmlType = new XmlType(namespaceUri, name, null);
		if (xmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out _))
			return true;

		return false;
	}

	public void Visit(ElementNode node, INode parentNode)
	{
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}

	INode? ParseExpression(ref string expression, IXmlNamespaceResolver nsResolver, IXmlLineInfo xmlLineInfo, INode node, INode parentNode)
	{
		if (expression.StartsWith("{}", StringComparison.Ordinal))
			return new ValueNode(expression.Substring(2), null, xmlLineInfo?.LineNumber ?? -1, xmlLineInfo?.LinePosition ?? -1);

		if (expression.Length == 0 || expression[expression.Length - 1] != '}')
		{
			//FIXME fix location
			var location = Context.ProjectItem.RelativePath is not null ? Location.Create(Context.ProjectItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
			Context.ReportDiagnostic(Diagnostic.Create(Descriptors.ExpressionNotClosed, location));
			return null;
		}

		if (!MarkupExpressionParser.MatchMarkup(out var match, expression, out var len))
		{
			//FIXME fix location
			var location = Context.ProjectItem.RelativePath is not null ? Location.Create(Context.ProjectItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
			Context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location));
			return null;
		}

		expression = expression.Substring(len).TrimStart();
		if (expression.Length == 0)
		{
			//FIXME fix location
			var location = Context.ProjectItem.RelativePath is not null ? Location.Create(Context.ProjectItem.RelativePath, new TextSpan(), new LinePositionSpan()) : null;
			Context.ReportDiagnostic(Diagnostic.Create(Descriptors.ExpressionNotClosed, location));
			return null;
		}
		var serviceProvider = new XamlServiceProvider(node, Context);
		serviceProvider.Add(typeof(IXmlNamespaceResolver), nsResolver);
		serviceProvider.Add(typeof(SGContextProvider), new SGContextProvider(Context));
		if (xmlLineInfo != null)
			serviceProvider.Add(typeof(IXmlLineInfoProvider), new XmlLineInfoProvider(xmlLineInfo));

		return new MarkupExpansionParser().Parse(match!, ref expression, serviceProvider);
	}


	public class MarkupExpansionParser : MarkupExpressionParser, IExpressionParser<INode>
	{
		ElementNode? _node;
		object IExpressionParser.Parse(string match, ref string remaining, IServiceProvider serviceProvider) => Parse(match, ref remaining, serviceProvider);

		public INode Parse(string match, ref string remaining, IServiceProvider serviceProvider)
		{
			if (serviceProvider.GetService(typeof(IXmlNamespaceResolver)) is not IXmlNamespaceResolver nsResolver)
				throw new ArgumentException();
			IXmlLineInfo? xmlLineInfo = null;
			if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider)
				xmlLineInfo = xmlLineInfoProvider.XmlLineInfo;
			var contextProvider = serviceProvider.GetService(typeof(SGContextProvider)) as SGContextProvider;

			var split = match.Split(':');
			if (split.Length > 2)
				throw new ArgumentException();

			var (prefix, name) = ParseName(match);

			var namespaceuri = nsResolver.LookupNamespace(prefix) ?? "";
			if (!string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(namespaceuri))
				//FIXME report error properly
				throw new Exception();
			//throw new BuildException(BuildExceptionCode.XmlnsUndeclared, xmlLineInfo, null, prefix);

			IList<XmlType>? typeArguments = null;
			var childnodes = new List<(XmlName, INode)>();
			var contentname = new XmlName(null, null);

			if (remaining.StartsWith("}", StringComparison.Ordinal))
			{
				remaining = remaining.Substring(1);
			}
			else
			{
				Property parsed = new Property();
				do
				{
					try
					{
						parsed = ParseProperty(serviceProvider, ref remaining);
					}
					catch (XamlParseException xpe)
					{
						if (contextProvider != null)
						{
							contextProvider.Context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationHelpers.LocationCreate(contextProvider.Context.ProjectItem.RelativePath!, xmlLineInfo!, match), xpe.Message));
							return null!;
						}
						else
							throw;
					}
					XmlName childname;

					if (parsed.name == null)
					{
						childname = contentname;
					}
					else
					{
						var (propertyPrefix, propertyName) = ParseName(parsed.name);

						childname = XamlParser.ParsePropertyName(new XmlName(
							propertyPrefix == "" ? null : nsResolver.LookupNamespace(propertyPrefix),
							propertyName));

						if (childname.NamespaceURI == null && childname.LocalName == null)
							continue;
					}

					if (childname == XmlName.xTypeArguments)
					{
						typeArguments = TypeArgumentsParser.ParseExpression(parsed.strValue, nsResolver, xmlLineInfo);
						childnodes.Add((childname, new ValueNode(typeArguments, nsResolver, xmlLineInfo?.LineNumber ?? -1, xmlLineInfo?.LinePosition ?? -1)));
					}
					else
					{
						var childnode = parsed.value as INode ?? new ValueNode(parsed.strValue, nsResolver, xmlLineInfo?.LineNumber ?? -1, xmlLineInfo?.LinePosition ?? -1);
						childnodes.Add((childname, childnode));
					}
				}
				while (!parsed.last);
			}


			// if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is XamlTypeResolver typeResolver))
			// 	throw new NotSupportedException();

			var xmltype = new XmlType(namespaceuri, name + "Extension", typeArguments);

			if (!xmltype.TryResolveTypeSymbol(null, contextProvider!.Context.Compilation, contextProvider!.Context.XmlnsCache, contextProvider!.Context.TypeCache, out _))
				xmltype = new XmlType(namespaceuri, name, typeArguments);

			if (xmltype == null)
				throw new NotSupportedException();


			_node = new ElementNode(xmltype, null, nsResolver, xmlLineInfo?.LineNumber ?? -1, xmlLineInfo?.LinePosition ?? -1);

			foreach (var (childname, childnode) in childnodes)
			{
				childnode.Parent = _node;

				if (childname == contentname)
				{
					//ContentProperty
					_node.CollectionItems.Add(childnode);
				}
				else
				{
					_node.Properties[childname] = childnode;
				}
			}

			return _node;
		}
	}
}
