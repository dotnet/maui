#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml;

/// <summary>
/// Runtime registry that maps live page/view instances to their child components by stable node ID.
/// Used by XSG-generated <c>InitializeComponent()</c> and <c>UpdateComponent_vNtoM()</c> methods
/// to find live objects during incremental XAML Hot Reload without re-inflation.
/// </summary>
/// <remarks>
/// <para>
/// Registration is keyed on the <em>page instance</em> (not type), so multiple concurrent instances
/// of the same page type each have their own component map. Components are held via
/// <see cref="WeakReference{T}"/> so they do not prevent GC of abandoned instances.
/// </para>
/// <para>
/// This class is trim-safe: it stores <see cref="object"/> references with no reflection on user types.
/// </para>
/// </remarks>
public static class XamlComponentRegistry
{
	// ConditionalWeakTable allows the page instance to be GC'd; its entry is then automatically removed.
	static readonly ConditionalWeakTable<object, ComponentMap> s_table = new();

	// Secondary index for GetInstances. ConditionalWeakTable is not enumerable on netstandard2.0,
	// so we maintain a per-type list of weak references to page instances.
	static readonly object s_instancesLock = new();
	static readonly Dictionary<Type, List<WeakReference<object>>> s_instancesByType = new();

	/// <summary>
	/// Registers (or replaces) a component under <paramref name="nodeId"/> for the given <paramref name="page"/> instance.
	/// Called from generated <c>InitializeComponent()</c>.
	/// </summary>
	/// <param name="page">The root page/view instance that owns this component.</param>
	/// <param name="nodeId">Stable node ID assigned by the source generator (e.g. <c>"Label_1_0"</c>).</param>
	/// <param name="component">The live component object to register.</param>
	public static void Register(object page, string nodeId, object component)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));
		if (nodeId is null)
			throw new ArgumentNullException(nameof(nodeId));
		if (component is null)
			throw new ArgumentNullException(nameof(component));

		var map = s_table.GetOrCreateValue(page);
		bool firstRegistration;
		lock (map)
		{
			firstRegistration = map.IsNew;
			map.IsNew = false;
			map.Set(nodeId, component);
		}

		if (firstRegistration)
			TrackInstance(page);
	}

	/// <summary>
	/// Tries to retrieve the live component registered under <paramref name="nodeId"/> for <paramref name="page"/>.
	/// Called from generated <c>UpdateComponent_vNtoM()</c>.
	/// </summary>
	/// <returns><see langword="true"/> if found and the weak reference is still alive.</returns>
	public static bool TryGet(object page, string nodeId, out object? component)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));
		if (nodeId is null)
			throw new ArgumentNullException(nameof(nodeId));

		if (s_table.TryGetValue(page, out var map))
		{
			lock (map)
			{
				return map.TryGet(nodeId, out component);
			}
		}

		component = null;
		return false;
	}

	/// <summary>
	/// Returns all live page instances of the given <paramref name="pageType"/> that have registered components.
	/// Used by <c>UpdateApplication</c> in the generated <c>[MetadataUpdateHandler]</c> to enumerate
	/// live page instances that need updating.
	/// </summary>
	/// <remarks>
	/// This iterates a snapshot; entries for GC'd pages are automatically excluded.
	/// </remarks>
	public static IReadOnlyList<object> GetInstances(Type pageType)
	{
		if (pageType is null)
			throw new ArgumentNullException(nameof(pageType));

		List<WeakReference<object>>? weakList;
		lock (s_instancesLock)
		{
			if (!s_instancesByType.TryGetValue(pageType, out weakList))
				return Array.Empty<object>();

			// Snapshot for safe iteration outside the lock
			weakList = new List<WeakReference<object>>(weakList);
		}

		var result = new List<object>(weakList.Count);
		foreach (var weakRef in weakList)
		{
			if (weakRef.TryGetTarget(out var instance))
				result.Add(instance);
		}
		return result;
	}

	/// <summary>
	/// Removes all registrations for <paramref name="page"/>.
	/// Call this when a page instance is being destroyed to free weak references eagerly.
	/// </summary>
	public static void Unregister(object page)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));

		s_table.Remove(page);
		UntrackInstance(page);
	}

	// -------------------------------------------------------------------------
	// Instance tracking helpers
	// -------------------------------------------------------------------------

	static void TrackInstance(object page)
	{
		var type = page.GetType();
		lock (s_instancesLock)
		{
			if (!s_instancesByType.TryGetValue(type, out var list))
			{
				list = new List<WeakReference<object>>();
				s_instancesByType[type] = list;
			}

			// Prune dead entries while we have the lock
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!list[i].TryGetTarget(out _))
					list.RemoveAt(i);
			}

			list.Add(new WeakReference<object>(page));
		}
	}

	static void UntrackInstance(object page)
	{
		var type = page.GetType();
		lock (s_instancesLock)
		{
			if (!s_instancesByType.TryGetValue(type, out var list))
				return;

			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!list[i].TryGetTarget(out var target) || ReferenceEquals(target, page))
					list.RemoveAt(i);
			}
		}
	}

	// -------------------------------------------------------------------------
	// Inner type
	// -------------------------------------------------------------------------

	/// <summary>
	/// Per-page map from nodeId → weak reference to component.
	/// Separate type so it can be the value in <see cref="ConditionalWeakTable{TKey,TValue}"/>.
	/// </summary>
	sealed class ComponentMap
	{
		readonly Dictionary<string, WeakReference<object>> _entries = new(StringComparer.Ordinal);

		/// <summary>True until the first call to <see cref="Set"/>, used to detect first registration.</summary>
		public bool IsNew { get; set; } = true;

		public void Set(string nodeId, object component)
		{
			if (_entries.TryGetValue(nodeId, out var existing))
				existing.SetTarget(component);
			else
				_entries[nodeId] = new WeakReference<object>(component);
		}

		public bool TryGet(string nodeId, out object? component)
		{
			if (_entries.TryGetValue(nodeId, out var weakRef) && weakRef.TryGetTarget(out var target))
			{
				component = target;
				return true;
			}
			component = null;
			return false;
		}
	}
}
