using System;
using System.Linq;

#nullable disable

namespace Microsoft.Maui.Controls.Xaml;

class SimplifyOnPlatformVisitor : IXamlNodeVisitor
{
	public SimplifyOnPlatformVisitor(string targetFramework)
	{

		if (string.IsNullOrEmpty(targetFramework))
			return;

		if (targetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
			Target = "Android";
		if (targetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
			Target = "iOS";
		if (targetFramework.IndexOf("-macos", StringComparison.OrdinalIgnoreCase) != -1)
			Target = "macOS";
		if (targetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
			Target = "MacCatalyst";
	}

	public string Target { get; }

	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool IsResourceDictionary(ElementNode node) => false;
	public bool SkipChildren(INode node, INode parentNode) => false;

	public void Visit(ValueNode node, INode parentNode)
	{
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
		//markup was already expanded to element
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		if (Target is null)
			return;

		//`{OnPlatform}` markup extension
		if (node.XmlType.IsOfAnyType("OnPlatformExtension"))
		{
			if (node.Properties.TryGetValue(new XmlName("", Target), out INode targetNode)
				|| node.Properties.TryGetValue(new XmlName(null, Target), out targetNode)
				|| node.Properties.TryGetValue(new XmlName("", "Default"), out targetNode)
				|| node.Properties.TryGetValue(new XmlName(null, "Default"), out targetNode))
			{
				if (!node.TryGetPropertyName(parentNode, out XmlName name))
					return;
				if (parentNode is ElementNode parentEnode)
					parentEnode.Properties[name] = targetNode;
			}
			else if (node.CollectionItems.Count > 0) // syntax like {OnPlatform foo, iOS=bar}
			{
				if (!node.TryGetPropertyName(parentNode, out XmlName name))
					return;
				if (parentNode is ElementNode parentEnode)
					parentEnode.Properties[name] = node.CollectionItems[0];
			}
			else //no prop for target and no Default set
			{
				if (!node.TryGetPropertyName(parentNode, out XmlName name))
					return;
				//if there's no value for the targetPlatform, ignore the node.
				//this is slightly different than what OnPlatform does (return default(T))
				if (parentNode is ElementNode parentEnode)
					parentEnode.Properties.Remove(name);
			}
		}

		//`<OnPlatform>` elements
		//if (node.XmlType.Name == "OnPlatform" && node.XmlType.NamespaceUri == XamlParser.MauiUri)
		//{
		//	var onNode = GetOnNode(node, Target) ?? GetDefault(node);

		//	//Property node
		//	if (node.TryGetPropertyName(parentNode, out XmlName name)
		//		&& parentNode is IElementNode parentEnode)
		//	{
		//		if (onNode != null)
		//			parentEnode.Properties[name] = onNode;
		//		else
		//			parentEnode.Properties.Remove(name);
		//		return;
		//	}

		//	//Collection item
		//	if (onNode != null && parentNode is IElementNode parentEnode2)
		//		parentEnode2.CollectionItems[parentEnode2.CollectionItems.IndexOf(node)] = onNode;

		//}

		//INode GetOnNode(ElementNode onPlatform, string target)
		//{
		//	foreach (var onNode in onPlatform.CollectionItems)
		//	{
		//		if ((onNode as ElementNode).Properties.TryGetValue(new XmlName("", "Platform"), out var platform))
		//		{
		//			var splits = ((platform as ValueNode).Value as string).Split(',');
		//			foreach (var split in splits)
		//			{
		//				if (string.IsNullOrWhiteSpace(split))
		//					continue;
		//				if (split.Trim() == target)
		//				{
		//					if ((onNode as ElementNode).Properties.TryGetValue(new XmlName("", "Value"), out var node))
		//						return node;

		//					return (onNode as ElementNode).CollectionItems.FirstOrDefault();
		//				}
		//			}
		//		}
		//	}
		//	return null;
		//}

		//INode GetDefault(ElementNode onPlatform)
		//{
		//	if (node.Properties.TryGetValue(new XmlName("", "Default"), out INode defaultNode))
		//		return defaultNode;
		//	return null;
		//}

	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}
}
