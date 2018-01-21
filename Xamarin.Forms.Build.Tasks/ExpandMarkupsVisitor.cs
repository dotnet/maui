using System;
using System.Collections.Generic;
using System.Xml;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Build.Tasks
{
	class ExpandMarkupsVisitor : IXamlNodeVisitor
	{
		readonly IList<XmlName> skips = new List<XmlName>
		{
			XmlName.xKey,
			XmlName.xTypeArguments,
			XmlName.xFactoryMethod,
			XmlName.xName,
			XmlName.xDataType
		};

		public ExpandMarkupsVisitor(ILContext context)
		{
			Context = context;
		}

		ILContext Context { get; }

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
			XmlName propertyName;
			if (!TryGetProperyName(markupnode, parentNode, out propertyName))
				return;
			if (skips.Contains(propertyName))
				return;
			if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
				return;
			var markupString = markupnode.MarkupString;
			var node = ParseExpression(ref markupString, Context, markupnode.NamespaceResolver, markupnode) as IElementNode;
			if (node != null)
			{
				((IElementNode)parentNode).Properties[propertyName] = node;
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
			var parentElement = parentNode as IElementNode;
			if (parentElement == null)
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
				throw new XamlParseException("Markup expression missing its closing tag", xmlLineInfo);

			int len;
			string match;
			if (!MarkupExpressionParser.MatchMarkup(out match, expression, out len))
				throw new XamlParseException("Error while parsing markup expression", xmlLineInfo);
			expression = expression.Substring(len).TrimStart();
			if (expression.Length == 0)
				throw new XamlParseException("Markup expression not closed", xmlLineInfo);

			var provider = new XamlServiceProvider(null, null);
			provider.Add(typeof (ILContextProvider), new ILContextProvider(context));
			provider.Add(typeof (IXmlNamespaceResolver), nsResolver);
			provider.Add(typeof (IXmlLineInfoProvider), new XmlLineInfoProvider(xmlLineInfo));

			return new MarkupExpansionParser().Parse(match, ref expression, provider);
		}

		class ILContextProvider
		{
			public ILContextProvider(ILContext context)
			{
				Context = context;
			}

			public ILContext Context { get; }
		}

		class MarkupExpansionParser : MarkupExpressionParser, IExpressionParser<INode>
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
				var contextProvider = serviceProvider.GetService(typeof (ILContextProvider)) as ILContextProvider;

				var split = match.Split(':');
				if (split.Length > 2)
					throw new ArgumentException();

				string prefix, name;
				if (split.Length == 2)
				{
					prefix = split[0];
					name = split[1];
				}
				else
				{
					prefix = "";
					name = split[0];
				}

				var namespaceuri = nsResolver.LookupNamespace(prefix) ?? "";
				//The order of lookup is to look for the Extension-suffixed class name first and then look for the class name without the Extension suffix.
				XmlType type;
				try
				{
					type = new XmlType(namespaceuri, name + "Extension", null);
					type.GetTypeReference(contextProvider.Context.Module, null);
				}
				catch (XamlParseException)
				{
					type = new XmlType(namespaceuri, name, null);
				}

				if (type == null)
					throw new NotSupportedException();

				node = xmlLineInfo == null
					? new ElementNode(type, "", nsResolver)
					: new ElementNode(type, "", nsResolver, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);

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
				if (prop != null)
				{
					var name = new XmlName(node.NamespaceURI, prop);
					node.Properties[name] = value as INode ?? new ValueNode(strValue, null);
				}
				else //ContentProperty
					node.CollectionItems.Add(value as INode ?? new ValueNode(strValue, null));
			}
		}
	}
}