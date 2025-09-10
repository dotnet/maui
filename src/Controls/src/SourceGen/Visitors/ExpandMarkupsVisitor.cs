using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class ExpandMarkupsVisitor : IXamlNodeVisitor
{
	public ExpandMarkupsVisitor(SourceGenContext context) => Context = context;

	public static readonly IList<XmlName> Skips =
	[
		XmlName.xKey,
		XmlName.xTypeArguments,
		XmlName.xFactoryMethod,
		XmlName.xName,
	];

	SourceGenContext Context { get; }
	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => false;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
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
		if (ParseExpression(ref markupString, markupnode.NamespaceResolver, markupnode, markupnode, parentNode) is ElementNode node)
		{
			((ElementNode)parentNode).Properties[propertyName] = node;
			node.Parent = parentNode;
		}
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
			return new ValueNode(expression.Substring(2), null);

		if (expression[expression.Length - 1] != '}')
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

		return new MarkupExpansionParser().Parse(match!, ref expression, serviceProvider);
	}

	class SGContextProvider
	{
		public SGContextProvider(SourceGenContext context) => Context = context;

		public SourceGenContext Context { get; }
	}

	public class MarkupExpansionParser : MarkupExpressionParser, IExpressionParser<INode>
	{
		ElementNode? _node;
		object IExpressionParser.Parse(string match, ref string remaining, IServiceProvider serviceProvider) => Parse(match, ref remaining, serviceProvider);

		public INode Parse(string match, ref string remaining, IServiceProvider serviceProvider)
		{
			if (!(serviceProvider.GetService(typeof(IXmlNamespaceResolver)) is IXmlNamespaceResolver nsResolver))
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
							contextProvider.Context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationHelpers.LocationCreate(contextProvider.Context.ProjectItem.RelativePath!, xmlLineInfo!, match), xpe.Message));
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
						childnodes.Add((childname, new ValueNode(typeArguments, nsResolver)));
					}
					else
					{
						var childnode = parsed.value as INode ?? new ValueNode(parsed.strValue, nsResolver);
						childnodes.Add((childname, childnode));
					}
				}
				while (!parsed.last);
			}


			// if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is XamlTypeResolver typeResolver))
			// 	throw new NotSupportedException();

			var xmltype = new XmlType(namespaceuri, name + "Extension", typeArguments);

			if (!xmltype.TryResolveTypeSymbol(null, contextProvider!.Context.Compilation, contextProvider!.Context.XmlnsCache, out _))
				xmltype = new XmlType(namespaceuri, name, typeArguments);

			if (xmltype == null)
				throw new NotSupportedException();


			_node = xmlLineInfo == null
				? new ElementNode(xmltype, null, nsResolver)
				: new ElementNode(xmltype, null, nsResolver, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

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
