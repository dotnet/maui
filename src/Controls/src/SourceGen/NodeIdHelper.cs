#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Assigns unique string IDs to every <see cref="ElementNode"/> in a parsed XAML tree, including
/// elements stored as property values such as <c>CollectionView.ItemTemplate</c>. IDs are simple
/// incrementing integers — fast to generate, unique within a tree, and identity-based
/// (not position-based) so reordering children does not change any node's ID.
/// The root always receives <c>""</c> (accessed as <c>this</c> at runtime).
/// </summary>
internal static class NodeIdHelper
{
	/// <summary>
	/// Walks <paramref name="root"/> and returns a dictionary mapping every
	/// <see cref="ElementNode"/> to its unique ID. The root maps to <c>""</c>; nodes
	/// are numbered <c>"0"</c>, <c>"1"</c>, etc.
	/// </summary>
	public static Dictionary<ElementNode, string> AssignIds(ElementNode root)
	{
		var ids = new Dictionary<ElementNode, string>();
		ids[root] = "";
		int counter = 0;
		AssignChildrenRecursive(root, ids, ref counter);
		AssignPropertyNodesRecursive(root, ids, ref counter);
		return ids;
	}

	/// <summary>
	/// Same as <see cref="AssignIds(ElementNode)"/> but starts the counter at
	/// <paramref name="startId"/>, allowing callers to generate IDs that don't collide
	/// with a previous set. Returns the next available counter value.
	/// </summary>
	public static Dictionary<ElementNode, string> AssignIds(ElementNode root, int startId, out int nextId)
	{
		var ids = new Dictionary<ElementNode, string>();
		ids[root] = "";
		int counter = startId;
		AssignChildrenRecursive(root, ids, ref counter);
		AssignPropertyNodesRecursive(root, ids, ref counter);
		nextId = counter;
		return ids;
	}

	/// <summary>
	/// Walks <paramref name="targetRoot"/> and <paramref name="sourceRoot"/> in parallel (depth-first)
	/// and copies IDs from <paramref name="sourceIds"/> onto <paramref name="targetRoot"/>'s nodes.
	/// Both trees must have the same structure (parsed from the same XAML).
	/// </summary>
	public static Dictionary<ElementNode, string> TransferIds(
		ElementNode targetRoot,
		Dictionary<ElementNode, string> sourceIds,
		ElementNode sourceRoot)
	{
		var result = new Dictionary<ElementNode, string>();
		TransferRecursive(targetRoot, sourceRoot, sourceIds, result);
		return result;
	}

	public static HashSet<string> GetTemplateNodeIds(
		ElementNode root,
		IReadOnlyDictionary<ElementNode, string> nodeIds)
	{
		var result = new HashSet<string>();
		CollectTemplateNodeIds(root, nodeIds, result, new HashSet<ElementNode>(), insideTemplate: false);
		return result;
	}

	static void AssignChildrenRecursive(ElementNode parent, Dictionary<ElementNode, string> ids, ref int counter)
	{
		foreach (var item in parent.CollectionItems)
		{
			if (item is ElementNode child && !ids.ContainsKey(child))
			{
				ids[child] = counter.ToString();
				counter++;
				AssignChildrenRecursive(child, ids, ref counter);
			}
		}
	}

	static void AssignPropertyNodesRecursive(ElementNode node, Dictionary<ElementNode, string> ids, ref int counter)
	{
		foreach (var property in node.Properties)
			AssignPropertyValue(property.Value, ids, ref counter);

		foreach (var item in node.CollectionItems)
		{
			if (item is ElementNode child)
				AssignPropertyNodesRecursive(child, ids, ref counter);
		}
	}

	static void AssignPropertyValue(INode node, Dictionary<ElementNode, string> ids, ref int counter)
	{
		if (node is ElementNode element)
		{
			if (!ids.ContainsKey(element))
			{
				ids[element] = counter.ToString();
				counter++;
				AssignChildrenRecursive(element, ids, ref counter);
			}
			AssignPropertyNodesRecursive(element, ids, ref counter);
		}
		else if (node is ListNode list)
		{
			foreach (var item in list.CollectionItems)
				AssignPropertyValue(item, ids, ref counter);
		}
	}

	static void TransferRecursive(
		ElementNode target, ElementNode source,
		Dictionary<ElementNode, string> sourceIds,
		Dictionary<ElementNode, string> result)
	{
		if (sourceIds.TryGetValue(source, out var id))
			result[target] = id;

		int ti = 0, si = 0;
		while (ti < target.CollectionItems.Count && si < source.CollectionItems.Count)
		{
			var tc = target.CollectionItems[ti];
			var sc = source.CollectionItems[si];
			if (tc is ElementNode targetChild && sc is ElementNode sourceChild
				&& XmlTypeEquals(targetChild.XmlType, sourceChild.XmlType))
			{
				TransferRecursive(targetChild, sourceChild, sourceIds, result);
			}
			ti++;
			si++;
		}

		foreach (var property in target.Properties)
		{
			if (source.Properties.TryGetValue(property.Key, out var sourceProperty))
				TransferPropertyValue(property.Value, sourceProperty, sourceIds, result);
		}
	}

	static void TransferPropertyValue(
		INode target,
		INode source,
		Dictionary<ElementNode, string> sourceIds,
		Dictionary<ElementNode, string> result)
	{
		if (target is ElementNode targetElement
			&& source is ElementNode sourceElement
			&& XmlTypeEquals(targetElement.XmlType, sourceElement.XmlType))
		{
			TransferRecursive(targetElement, sourceElement, sourceIds, result);
		}
		else if (target is ListNode targetList && source is ListNode sourceList)
		{
			var count = System.Math.Min(targetList.CollectionItems.Count, sourceList.CollectionItems.Count);
			for (int i = 0; i < count; i++)
				TransferPropertyValue(targetList.CollectionItems[i], sourceList.CollectionItems[i], sourceIds, result);
		}
	}

	static void CollectTemplateNodeIds(
		ElementNode node,
		IReadOnlyDictionary<ElementNode, string> nodeIds,
		HashSet<string> result,
		HashSet<ElementNode> visited,
		bool insideTemplate)
	{
		if (!visited.Add(node))
			return;

		if (insideTemplate && nodeIds.TryGetValue(node, out var nodeId) && !string.IsNullOrEmpty(nodeId))
			result.Add(nodeId);

		var childIsInsideTemplate = insideTemplate
			|| node.XmlType.RepresentsType(XamlParser.MauiUri, "DataTemplate")
			|| node.XmlType.RepresentsType(XamlParser.MauiUri, "ControlTemplate");

		foreach (var item in node.CollectionItems)
		{
			if (item is ElementNode child)
				CollectTemplateNodeIds(child, nodeIds, result, visited, childIsInsideTemplate);
		}

		foreach (var property in node.Properties)
			CollectTemplatePropertyNodeIds(property.Value, nodeIds, result, visited, childIsInsideTemplate);
	}

	static void CollectTemplatePropertyNodeIds(
		INode node,
		IReadOnlyDictionary<ElementNode, string> nodeIds,
		HashSet<string> result,
		HashSet<ElementNode> visited,
		bool insideTemplate)
	{
		if (node is ElementNode element)
			CollectTemplateNodeIds(element, nodeIds, result, visited, insideTemplate);
		else if (node is ListNode list)
		{
			foreach (var item in list.CollectionItems)
				CollectTemplatePropertyNodeIds(item, nodeIds, result, visited, insideTemplate);
		}
	}

	static bool XmlTypeEquals(XmlType a, XmlType b) =>
		string.Equals(a.Name, b.Name, System.StringComparison.Ordinal)
		&& string.Equals(a.NamespaceUri, b.NamespaceUri, System.StringComparison.Ordinal);
}
