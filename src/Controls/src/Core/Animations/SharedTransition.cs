#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Shared-element ("hero") transitions: an element on the outgoing page morphs into the element with the
	/// same <c>Name</c> on the incoming page while the pages transition.
	/// </summary>
	/// <remarks>
	/// <para><b>Pairing.</b> During a navigation that runs one of MAUI's transitions, elements with the same
	/// <c>Name</c> on the outgoing and incoming page are paired, going forward and back alike. When several
	/// elements on one page share a name (a list), the one whose element-level <c>Group</c> equals the other
	/// page's page-level <c>Group</c> is used; the detail page usually binds its <c>Group</c> to the selected
	/// item's id.</para>
	/// <para><b>Contract.</b> With no unique match, that pair doesn't morph and the page transition runs alone.
	/// A cancelled back gesture returns the element to its source along with the page. Under reduced motion
	/// nothing morphs. During the morph the element is a snapshot: it doesn't take input or update until it
	/// lands. Only elements that are showing take part.</para>
	/// </remarks>
	public static class SharedTransition
	{
		/// <summary>Bindable property for the attached <c>Name</c> property.</summary>
		public static readonly BindableProperty NameProperty =
			BindableProperty.CreateAttached("Name", typeof(string), typeof(SharedTransition), null);

		/// <summary>Bindable property for the attached <c>Group</c> property, set on an element or a page.</summary>
		public static readonly BindableProperty GroupProperty =
			BindableProperty.CreateAttached("Group", typeof(string), typeof(SharedTransition), null);

		/// <summary>Gets the shared-element name of <paramref name="bindable"/>.</summary>
		public static string? GetName(BindableObject bindable)
			=> (string?)(bindable ?? throw new ArgumentNullException(nameof(bindable))).GetValue(NameProperty);

		/// <summary>Sets the shared-element name of <paramref name="bindable"/>.</summary>
		public static void SetName(BindableObject bindable, string? value)
			=> (bindable ?? throw new ArgumentNullException(nameof(bindable))).SetValue(NameProperty, value);

		/// <summary>Gets the shared-element group of <paramref name="bindable"/>.</summary>
		public static string? GetGroup(BindableObject bindable)
			=> (string?)(bindable ?? throw new ArgumentNullException(nameof(bindable))).GetValue(GroupProperty);

		/// <summary>Sets the shared-element group of <paramref name="bindable"/>.</summary>
		public static void SetGroup(BindableObject bindable, string? value)
			=> (bindable ?? throw new ArgumentNullException(nameof(bindable))).SetValue(GroupProperty, value);

		/// <summary>
		/// The pairs of elements that morph between <paramref name="from"/> and <paramref name="to"/>, by the rules above.
		/// </summary>
		internal static List<(VisualElement From, VisualElement To)> FindPairs(Page? from, Page? to)
		{
			var pairs = new List<(VisualElement From, VisualElement To)>();
			if (from is null || to is null || ReferenceEquals(from, to))
			{
				return pairs;
			}

			var outgoing = Collect(from);
			if (outgoing.Count == 0)
			{
				return pairs;
			}

			var incoming = Collect(to);
			foreach (var entry in incoming)
			{
				if (outgoing.TryGetValue(entry.Key, out var sources))
				{
					var source = Pick(sources, entry.Value, to);
					var target = Pick(entry.Value, sources, from);
					if (source is not null && target is not null)
					{
						pairs.Add((source, target));
					}
				}
			}

			return pairs;
		}

		// One side's element for a name: the only one, or, when the side is a list, the one whose element
		// Group matches the other page's Group. Both sides being lists is ambiguous.
		static VisualElement? Pick(List<VisualElement> side, List<VisualElement> other, Page otherPage)
		{
			if (side.Count == 1)
			{
				return side[0];
			}

			if (other.Count != 1)
			{
				return null;
			}

			var group = GetGroup(otherPage);
			if (group is null)
			{
				return null;
			}

			VisualElement? match = null;
			for (int i = 0; i < side.Count; i++)
			{
				if (string.Equals(GetGroup(side[i]), group, StringComparison.Ordinal))
				{
					if (match is not null)
					{
						return null;
					}

					match = side[i];
				}
			}

			return match;
		}

		static Dictionary<string, List<VisualElement>> Collect(Page page)
		{
			var result = new Dictionary<string, List<VisualElement>>(StringComparer.Ordinal);
			Walk(page, result);
			return result;
		}

		static void Walk(IVisualTreeElement element, Dictionary<string, List<VisualElement>> result)
		{
			if (element is VisualElement visual)
			{
				if (!visual.IsVisible)
				{
					return;
				}

				if (visual is not Page && GetName(visual) is { Length: > 0 } name)
				{
					if (!result.TryGetValue(name, out var list))
					{
						list = new List<VisualElement>();
						result[name] = list;
					}

					list.Add(visual);
				}
			}

			var children = element.GetVisualChildren();
			for (int i = 0; i < children.Count; i++)
			{
				Walk(children[i], result);
			}
		}
	}
}
