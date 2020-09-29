using System;
using System.Collections.Generic;
using System.Xml;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	class ExpandMarkupsVisitor : IXamlNodeVisitor
	{
		public ExpandMarkupsVisitor(HydrationContext context) => Context = context;

		public static readonly IList<XmlName> Skips = new List<XmlName>
		{
			XmlName.xKey,
			XmlName.xTypeArguments,
			XmlName.xFactoryMethod,
			XmlName.xName,
			XmlName.xDataType
		};

		Dictionary<INode, object> Values
		{
			get { return Context.Values; }
		}

		HydrationContext Context { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => false;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => true;
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => false;

		public void Visit(ValueNode node, INode parentNode)
		{
		}

		public void Visit(MarkupNode markupnode, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			XmlName propertyName;
			if (!ApplyPropertiesVisitor.TryGetPropertyName(markupnode, parentNode, out propertyName))
				return;
			if (Skips.Contains(propertyName))
				return;
			if (parentElement.SkipProperties.Contains(propertyName))
				return;

			var markupString = markupnode.MarkupString;
			var node =
				ParseExpression(ref markupString, markupnode.NamespaceResolver, markupnode, markupnode, parentNode) as IElementNode;
			if (node != null)
			{
				((IElementNode)parentNode).Properties[propertyName] = node;
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

		INode ParseExpression(ref string expression, IXmlNamespaceResolver nsResolver, IXmlLineInfo xmlLineInfo, INode node,
			INode parentNode)
		{
			if (expression.StartsWith("{}", StringComparison.Ordinal))
				return new ValueNode(expression.Substring(2), null);

			if (expression[expression.Length - 1] != '}')
			{
				var ex = new XamlParseException("Expression must end with '}'", xmlLineInfo);
				if (Context.ExceptionHandler != null)
				{
					Context.ExceptionHandler(ex);
					return null;
				}
				throw ex;
			}

			if (!MarkupExpressionParser.MatchMarkup(out var match, expression, out var len))
				throw new Exception();

			expression = expression.Substring(len).TrimStart();
			if (expression.Length == 0)
			{
				var ex = new XamlParseException("Expression did not end in '}'", xmlLineInfo);
				if (Context.ExceptionHandler != null)
				{
					Context.ExceptionHandler(ex);
					return null;
				}
				throw ex;
			}
			var serviceProvider = new XamlServiceProvider(node, Context);
			serviceProvider.Add(typeof(IXmlNamespaceResolver), nsResolver);

			return new MarkupExpansionParser { ExceptionHandler = Context.ExceptionHandler }.Parse(match, ref expression, serviceProvider);
		}

		public class MarkupExpansionParser : MarkupExpressionParser, IExpressionParser<INode>
		{
			IElementNode _node;
			internal Action<Exception> ExceptionHandler { get; set; }
			object IExpressionParser.Parse(string match, ref string remaining, IServiceProvider serviceProvider)
			{
				return Parse(match, ref remaining, serviceProvider);
			}

			public INode Parse(string match, ref string remaining, IServiceProvider serviceProvider)
			{
				if (!(serviceProvider.GetService(typeof(IXmlNamespaceResolver)) is IXmlNamespaceResolver nsResolver))
					throw new ArgumentException();
				IXmlLineInfo xmlLineInfo = null;
				if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider)
					xmlLineInfo = xmlLineInfoProvider.XmlLineInfo;

				var (prefix, name) = ParseName(match);

				var namespaceuri = nsResolver.LookupNamespace(prefix) ?? "";

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
						parsed = ParseProperty(serviceProvider, ref remaining);
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


				if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is XamlTypeResolver typeResolver))
					throw new NotSupportedException();

				var xmltype = new XmlType(namespaceuri, name + "Extension", typeArguments);

				//The order of lookup is to look for the Extension-suffixed class name first and then look for the class name without the Extension suffix.
				if (!typeResolver.TryResolve(xmltype, out _))
				{
					xmltype = new XmlType(namespaceuri, name, typeArguments);
					if (!typeResolver.TryResolve(xmltype, out _))
					{
						var ex = new XamlParseException($"MarkupExtension not found for {match}", serviceProvider);
						if (ExceptionHandler != null)
						{
							ExceptionHandler(ex);
							return null;
						}
						throw ex;
					}
				}

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
}