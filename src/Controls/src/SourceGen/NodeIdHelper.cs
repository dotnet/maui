#nullable enable
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Assigns stable string IDs to every <see cref="ElementNode"/> in a parsed XAML tree via a
/// depth-first walk. IDs encode the element type, depth, and sibling index so that structurally
/// equivalent positions in two different versions of the same XAML file receive the same ID —
/// enabling the diff engine to correlate nodes across edits.
/// </summary>
/// <remarks>
/// <para>
/// ID format: <c>"{TypeName}_{depth}_{siblingIndex}"</c> — e.g. <c>"Label_1_0"</c> is the first
/// Label at depth 1. The root element always receives the empty string <c>""</c> so that call
/// sites can address the root without a special-case.
/// </para>
/// <para>
/// Only <see cref="ElementNode"/> nodes in <see cref="ElementNode.CollectionItems"/> are assigned
/// IDs; property-element children (stored in <see cref="ElementNode.Properties"/>) are not
/// independently addressable at runtime.
/// </para>
/// </remarks>
internal static class NodeIdHelper
{
	/// <summary>
	/// Walks <paramref name="root"/> depth-first and returns a dictionary mapping every
	/// <see cref="ElementNode"/> to its stable ID.  The root itself maps to <c>""</c>.
	/// </summary>
	public static Dictionary<ElementNode, string> AssignIds(ElementNode root)
	{
		var ids = new Dictionary<ElementNode, string>();
		ids[root] = "";
		WalkChildren(root, depth: 1, ids);
		return ids;
	}

	// -------------------------------------------------------------------------
	// Recursive walk
	// -------------------------------------------------------------------------

	static void WalkChildren(ElementNode parent, int depth, Dictionary<ElementNode, string> ids)
	{
		int index = 0;
		foreach (var item in parent.CollectionItems)
		{
			if (item is not ElementNode child)
			{
				index++;
				continue;
			}

			string typeName = child.XmlType.Name;
			string id = $"{typeName}_{depth}_{index}";
			ids[child] = id;

			// Recurse into the child's collection items
			WalkChildren(child, depth + 1, ids);
			index++;
		}
	}
}
