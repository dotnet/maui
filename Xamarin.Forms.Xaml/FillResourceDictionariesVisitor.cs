using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	internal class FillResourceDictionariesVisitor : IXamlNodeVisitor
	{
		public FillResourceDictionariesVisitor(HydratationContext context)
		{
			Context = context;
		}

		HydratationContext Context { get; }

		Dictionary<INode, object> Values
		{
			get { return Context.Values; }
		}

		public bool VisitChildrenFirst
		{
			get { return false; }
		}

		public bool StopOnDataTemplate
		{
			get { return true; }
		}

		public bool StopOnResourceDictionary
		{
			get { return false; }
		}

		public void Visit(ValueNode node, INode parentNode)
		{
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			if (node.SkipPrefix(node.NamespaceResolver.LookupPrefix(node.NamespaceURI)))
				return;

			var value = Values[node];
			var parentElement = parentNode as IElementNode;
			var markupExtension = value as IMarkupExtension;
			var valueProvider = value as IValueProvider;

			//Set Resources in ResourcesDictionaries
			if (IsCollectionItem(node, parentNode) && parentNode is IElementNode)
			{
				if (typeof (IEnumerable).GetTypeInfo().IsAssignableFrom(Context.Types[parentElement].GetTypeInfo()))
				{
					var source = Values[parentNode];
					if (Context.Types[parentElement] == typeof (ResourceDictionary) && value is Style &&
					    !node.Properties.ContainsKey(XmlName.xKey))
					{
						node.Accept(new ApplyPropertiesVisitor(Context), parentNode);
						if (markupExtension != null)
						{
							var serviceProvider = new XamlServiceProvider(node, Context);
							value = markupExtension.ProvideValue(serviceProvider);
						}
						if (valueProvider != null)
						{
							var serviceProvider = new XamlServiceProvider(node, Context);
							value = valueProvider.ProvideValue(serviceProvider);
						}
						((ResourceDictionary)source).Add(value as Style);
					}
					else if (Context.Types[parentElement] == typeof (ResourceDictionary) && !node.Properties.ContainsKey(XmlName.xKey))
						throw new XamlParseException("resources in ResourceDictionary require a x:Key attribute", node);
					else if (Context.Types[parentElement] == typeof (ResourceDictionary) && node.Properties.ContainsKey(XmlName.xKey))
					{
						node.Accept(new ApplyPropertiesVisitor(Context), parentNode);
						if (markupExtension != null)
						{
							var serviceProvider = new XamlServiceProvider(node, Context);
							value = markupExtension.ProvideValue(serviceProvider);
						}
						if (valueProvider != null)
						{
							var serviceProvider = new XamlServiceProvider(node, Context);
							value = valueProvider.ProvideValue(serviceProvider);
						}
						((ResourceDictionary)source).Add((string)(((ValueNode)node.Properties[XmlName.xKey]).Value), value);
					}
				}
			}

			//Set RD to VE
			XmlName propertyName;
			if (ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out propertyName))
			{
				if ((propertyName.LocalName == "Resources" ||
				     propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal)) && value is ResourceDictionary)
				{
					var source = Values[parentNode];
					ApplyPropertiesVisitor.SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
				}
			}
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		static bool IsCollectionItem(INode node, INode parentNode)
		{
			var parentList = parentNode as IListNode;
			if (parentList == null)
				return false;
			return parentList.CollectionItems.Contains(node);
		}
	}
}