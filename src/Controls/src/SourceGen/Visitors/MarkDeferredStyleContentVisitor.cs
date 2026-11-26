using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Marks styles whose Setters, Behaviors and Triggers are emitted by a lazy initializer instead of the
/// enclosing scope. This must run before any value is created so that no later pass visits the deferred
/// content. A TargetType given as a runtime Type value (e.g. <c>{x:Static}</c>) keeps eager content.
/// </summary>
class MarkDeferredStyleContentVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	static readonly XmlName TargetTypeName = new("", "TargetType");

	SourceGenContext Context => context;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => false;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool StopOnStyle => false;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => false;
	public bool IsStyle(ElementNode node) => false;

	internal static bool IsContentProperty(XmlName name)
		=> name.LocalName is "Setters" or "Behaviors" or "Triggers";

	public void Visit(ValueNode node, INode parentNode)
	{
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		if (!Context.ProjectItem.TrimmableStyles || !node.IsStyle(Context))
			return;

		if (node.Properties.TryGetValue(TargetTypeName, out var targetType)
			&& targetType is ElementNode targetTypeElement
			&& !IsTypeExtension(targetTypeElement))
			return;

		var contentProperties = node.Properties.Keys.Where(IsContentProperty).ToList();
		if (contentProperties.Count == 0 && node.CollectionItems.Count == 0)
			return;

		foreach (var contentProperty in contentProperties)
		{
			if (!node.SkipProperties.Contains(contentProperty))
				node.SkipProperties.Add(contentProperty);
		}

		node.Properties[XmlName._StyleContent] = new ValueNode("true", node.NamespaceResolver);
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}

	internal static bool IsTypeExtension(ElementNode node)
		=> node.XmlType.Name == "TypeExtension" && node.XmlType.NamespaceUri == XamlParser.X2009Uri;
}
