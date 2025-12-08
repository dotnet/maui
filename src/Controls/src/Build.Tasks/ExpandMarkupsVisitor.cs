using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class ExpandMarkupsVisitor(ILContext context) : IXamlNodeVisitor
	{
		readonly IList<XmlName> _skips =
		[
			XmlName.xKey,
			XmlName.xTypeArguments,
			XmlName.xFactoryMethod,
			XmlName.xName,
		];

		ILContext Context { get; } = context;

		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => false;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => true;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public bool IsResourceDictionary(ElementNode node)
		{
			var parentVar = Context.Variables[node];
			return parentVar.VariableType.FullName == "Microsoft.Maui.Controls.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "Microsoft.Maui.Controls.ResourceDictionary";
		}

		public void Visit(ValueNode node, INode parentNode)
		{
		}

		public void Visit(MarkupNode markupnode, INode parentNode)
		{
			if (!TryGetProperyName(markupnode, parentNode, out XmlName propertyName))
				return;
			if (_skips.Contains(propertyName))
				return;
			if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(propertyName))
				return;
			var markupString = markupnode.MarkupString;
			if (ParseExpression(ref markupString, Context, markupnode.NamespaceResolver, markupnode) is ElementNode node)
			{
				((ElementNode)parentNode).Properties[propertyName] = node;
				node.Accept(new XamlNodeVisitor((n, parent) => n.Parent = parent), parentNode);
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

		public static bool TryGetProperyName(INode node, INode parentNode, out XmlName name)
		{
			name = default(XmlName);
			if (parentNode is not ElementNode parentElement)
				return false;
			foreach (var kvp in parentElement.Properties)
			{
				if (kvp.Value != node)
					continue;
				name = kvp.Key;
				return true;
			}
			return false;
		}

		static INode ParseExpression(ref string expression, ILContext context, IXmlNamespaceResolver nsResolver,
			IXmlLineInfo xmlLineInfo)
		{
			if (expression.StartsWith("{}", StringComparison.Ordinal))
				return new ValueNode(expression.Substring(2), null);

			if (expression[expression.Length - 1] != '}')
				throw new BuildException(BuildExceptionCode.MarkupNotClosed, xmlLineInfo, null);

			if (!MarkupExpressionParser.MatchMarkup(out var match, expression, out var len))
				throw new BuildException(BuildExceptionCode.MarkupParsingFailed, xmlLineInfo, null);
			expression = expression.Substring(len).TrimStart();
			if (expression.Length == 0)
				throw new BuildException(BuildExceptionCode.MarkupNotClosed, xmlLineInfo, null);

			var provider = new XamlServiceProvider(null, null);
			provider.Add(typeof(ILContextProvider), new ILContextProvider(context));
			provider.Add(typeof(IXmlNamespaceResolver), nsResolver);
			provider.Add(typeof(IXmlLineInfoProvider), new XmlLineInfoProvider(xmlLineInfo));

			return new MarkupExpansionParser().Parse(match, ref expression, provider);
		}

		class ILContextProvider(ILContext context)
		{
			public ILContext Context { get; } = context;
		}

		class MarkupExpansionParser : MarkupExpressionParser, IExpressionParser<INode>
		{
			ElementNode _node;

			object IExpressionParser.Parse(string match, ref string remaining, IServiceProvider serviceProvider) => Parse(match, ref remaining, serviceProvider);

			public INode Parse(string match, ref string remaining, IServiceProvider serviceProvider)
			{
				if (!(serviceProvider.GetService(typeof(IXmlNamespaceResolver)) is IXmlNamespaceResolver nsResolver))
					throw new ArgumentException();
				IXmlLineInfo xmlLineInfo = null;
				if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider)
					xmlLineInfo = xmlLineInfoProvider.XmlLineInfo;
				var contextProvider = serviceProvider.GetService(typeof(ILContextProvider)) as ILContextProvider;

				var split = match.Split(':');
				if (split.Length > 2)
					throw new ArgumentException();

				var (prefix, name) = ParseName(match);

				var namespaceuri = nsResolver.LookupNamespace(prefix) ?? "";
				if (!string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(namespaceuri))
					throw new BuildException(BuildExceptionCode.XmlnsUndeclared, xmlLineInfo, null, prefix);

				IList<XmlType> typeArguments = null;
				var childnodes = new List<(XmlName, INode)>();
				var contentname = new XmlName(null, null);

				if (remaining.StartsWith("}", StringComparison.Ordinal))
				{
					remaining = remaining.Substring(1);
				}
				else
				{
					Property parsed;
					do
					{
						try
						{
							parsed = ParseProperty(serviceProvider, ref remaining);
						}
						catch (XamlParseException xpe)
						{
							throw new BuildException(BuildExceptionCode.MarkupParsingFailed, xmlLineInfo, xpe);
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
								propertyPrefix == "" ? "" : nsResolver.LookupNamespace(propertyPrefix),
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

				//The order of lookup is to look for the Extension-suffixed class name first and then look for the class name without the Extension suffix.
				XmlType type = new XmlType(namespaceuri, name + "Extension", typeArguments);
				if (!type.TryGetTypeReference(contextProvider.Context.Cache, contextProvider.Context.Module, null, expandToExtension: true, out _))
					type = new XmlType(namespaceuri, name, typeArguments);

				if (type == null)
					throw new NotSupportedException();

				_node = xmlLineInfo == null
					? new ElementNode(type, "", nsResolver)
					: new ElementNode(type, "", nsResolver, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

				foreach (var (childname, childnode) in childnodes)
				{
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
}
