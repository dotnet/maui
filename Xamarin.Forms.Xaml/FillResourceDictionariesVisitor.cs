using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
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

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;

		public void Visit(ValueNode node, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			var value = Values [node];
			var source = Values [parentNode];

			XmlName propertyName;
			if (ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out propertyName)) {
				if (parentElement.SkipProperties.Contains(propertyName))
					return;
				if (parentElement.SkipPrefix(node.NamespaceResolver.LookupPrefix(propertyName.NamespaceURI)))
					return;
				if (propertyName.NamespaceURI == "http://schemas.openxmlformats.org/markup-compatibility/2006" &&
					propertyName.LocalName == "Ignorable") 
					return;
				if (propertyName.LocalName != "MergedWith")
					return;
				ApplyPropertiesVisitor.SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			}

		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			var value = Values[node];
			var parentElement = parentNode as IElementNode;
			var markupExtension = value as IMarkupExtension;
			var valueProvider = value as IValueProvider;

			//Set Resources in ResourcesDictionaries
			if (IsCollectionItem(node, parentNode) && parentNode is IElementNode)
			{
				if (typeof (IEnumerable).IsAssignableFrom(Context.Types[parentElement]))
				{
					var source = Values[parentNode];
					if (typeof (ResourceDictionary).IsAssignableFrom(Context.Types[parentElement]) && value is Style &&
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
					else if (typeof (ResourceDictionary).IsAssignableFrom(Context.Types[parentElement]) && !node.Properties.ContainsKey(XmlName.xKey))
						throw new XamlParseException("resources in ResourceDictionary require a x:Key attribute", node);
					else if (typeof (ResourceDictionary).IsAssignableFrom(Context.Types[parentElement]) && node.Properties.ContainsKey(XmlName.xKey))
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
					 propertyName.LocalName == "MergedDictionaries" ||
					 propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal)) && value is ResourceDictionary)
				{
					var source = Values[parentNode];
					ApplyPropertiesVisitor.SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
				}
			}

			//Add ResourceDictionary into MergedDictionaries
			XmlName parentPropertyName;
			if (parentNode is IListNode && ApplyPropertiesVisitor.TryGetPropertyName(parentNode, parentNode.Parent, out parentPropertyName))
			{
				if (parentPropertyName.LocalName == "MergedDictionaries")
				{
					var source = Values[parentNode.Parent];
					node.Parent = node.Parent.Parent;
					ApplyPropertiesVisitor.SetPropertyValue(source, parentPropertyName, value, Context.RootElement, node, Context, node);
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