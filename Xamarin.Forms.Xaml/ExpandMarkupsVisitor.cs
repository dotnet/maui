using System;
using System.Collections.Generic;
using System.Xml;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	class ExpandMarkupsVisitor : IXamlNodeVisitor
	{
		public ExpandMarkupsVisitor(HydrationContext context)
		{
			Context = context;
		}

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
				throw new Exception("Expression must end with '}'");

			int len;
			string match;
			if (!MarkupExpressionParser.MatchMarkup(out match, expression, out len))
				throw new Exception();
			expression = expression.Substring(len).TrimStart();
			if (expression.Length == 0)
				throw new Exception("Expression did not end in '}'");

			var serviceProvider = new XamlServiceProvider(node, Context);
			serviceProvider.Add(typeof (IXmlNamespaceResolver), nsResolver);

			return new MarkupExpansionParser().Parse(match, ref expression, serviceProvider);
		}

		public class MarkupExpansionParser : MarkupExpressionParser, IExpressionParser<INode>
		{
			IElementNode node;

			object IExpressionParser.Parse(string match, ref string remaining, IServiceProvider serviceProvider)
			{
				return Parse(match, ref remaining, serviceProvider);
			}

			public INode Parse(string match, ref string remaining, IServiceProvider serviceProvider)
			{
				var nsResolver = serviceProvider.GetService(typeof (IXmlNamespaceResolver)) as IXmlNamespaceResolver;
				if (nsResolver == null)
					throw new ArgumentException();
				IXmlLineInfo xmlLineInfo = null;
				var xmlLineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				if (xmlLineInfoProvider != null)
					xmlLineInfo = xmlLineInfoProvider.XmlLineInfo;

				var split = match.Split(':');
				if (split.Length > 2)
					throw new ArgumentException();

				string prefix; //, name;
				if (split.Length == 2)
				{
					prefix = split[0];
					//					name = split [1];
				}
				else
				{
					prefix = "";
					//					name = split [0];
				}

				Type type;
				var typeResolver = serviceProvider.GetService(typeof (IXamlTypeResolver)) as IXamlTypeResolver;
				if (typeResolver == null)
					type = null;
				else
				{
					//The order of lookup is to look for the Extension-suffixed class name first and then look for the class name without the Extension suffix.
					if (!typeResolver.TryResolve(match + "Extension", out type) && !typeResolver.TryResolve(match, out type))
					{
						var lineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
						var lineInfo = (lineInfoProvider != null) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
						throw new XamlParseException(String.Format("MarkupExtension not found for {0}", match), lineInfo);
					}
				}

				var namespaceuri = nsResolver.LookupNamespace(prefix) ?? "";
				var xmltype = new XmlType(namespaceuri, type.Name, null);

				if (type == null)
					throw new NotSupportedException();

				node = xmlLineInfo == null
					? new ElementNode(xmltype, null, nsResolver)
					: new ElementNode(xmltype, null, nsResolver, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

				if (remaining.StartsWith("}", StringComparison.Ordinal))
				{
					remaining = remaining.Substring(1);
					return node;
				}

				char next;
				string piece;
				while ((piece = GetNextPiece(ref remaining, out next)) != null)
					HandleProperty(piece, serviceProvider, ref remaining, next != '=');

				return node;
			}

			protected override void SetPropertyValue(string prop, string strValue, object value, IServiceProvider serviceProvider)
			{
				var nsResolver = serviceProvider.GetService(typeof (IXmlNamespaceResolver)) as IXmlNamespaceResolver;

				var childnode = value as INode ?? new ValueNode(strValue, nsResolver);
				childnode.Parent = node;
				if (prop != null)
				{
					var name = new XmlName(node.NamespaceURI, prop);
					node.Properties[name] = childnode;
				}
				else //ContentProperty
					node.CollectionItems.Add(childnode);
			}
		}
	}
}