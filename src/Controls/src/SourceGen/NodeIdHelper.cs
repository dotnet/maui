#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Assigns unique string IDs to every <see cref="ElementNode"/> in a parsed XAML tree via a
/// depth-first walk. IDs are simple incrementing integers — fast to generate, unique within a tree,
/// and identity-based (not position-based) so reordering children does not change any node's ID.
/// The root always receives <c>""</c> (accessed as <c>this</c> at runtime).
/// </summary>
internal static class NodeIdHelper
{
	/// <summary>
	/// Walks <paramref name="root"/> depth-first and returns a dictionary mapping every
	/// <see cref="ElementNode"/> to its unique ID. The root maps to <c>""</c>; children
	/// are numbered <c>"0"</c>, <c>"1"</c>, etc. in depth-first order.
	/// </summary>
	public static Dictionary<ElementNode, string> AssignIds(ElementNode root)
	{
		var ids = new Dictionary<ElementNode, string>();
		ids[root] = "";
		int counter = 0;
		AssignChildrenRecursive(root, ids, ref counter);
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

	static void AssignChildrenRecursive(ElementNode parent, Dictionary<ElementNode, string> ids, ref int counter)
	{
		foreach (var item in parent.CollectionItems)
		{
			if (item is ElementNode child)
			{
				ids[child] = counter.ToString();
				counter++;
				AssignChildrenRecursive(child, ids, ref counter);
			}
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
	}

	static bool XmlTypeEquals(XmlType a, XmlType b) =>
		string.Equals(a.Name, b.Name, System.StringComparison.Ordinal)
		&& string.Equals(a.NamespaceUri, b.NamespaceUri, System.StringComparison.Ordinal);
}
