#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml;

/// <summary>
/// Runtime registry that maps live page/view instances to their child components by stable node ID.
/// Used by XSG-generated <c>InitializeComponent()</c> and <c>UpdateComponent()</c> methods
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
	/// Called from generated <c>UpdateComponent()</c>.
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
	/// Convenience alias matching the spec's <c>RegisterComponent(element, nodeId)</c> signature.
	/// Registers the <paramref name="element"/> both as page owner and component under <paramref name="nodeId"/>.
	/// Intended for root-level self-registration (e.g. <c>RegisterComponent(this, "")</c>).
	/// </summary>
	public static void RegisterComponent(object element, string nodeId) =>
		Register(element, nodeId, element);

	/// <summary>
	/// Convenience alias matching the spec's <c>GetComponent(nodeId)</c> signature.
	/// Retrieves the component registered under <paramref name="nodeId"/> for <paramref name="page"/>.
	/// Returns <see langword="null"/> if not found.
	/// </summary>
	public static object? GetComponent(object page, string nodeId)
	{
		TryGet(page, nodeId, out var component);
		return component;
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

	/// <summary>
	/// Renames all component registrations whose node ID starts with <paramref name="oldPrefix"/>
	/// so that the prefix is replaced by <paramref name="newPrefix"/>. Used by generated
	/// <c>UpdateComponent</c> methods after a same-parent child reorder to keep the registry
	/// in sync with the new position-based node IDs (including all descendants).
	/// </summary>
	public static void ReRoot(object page, string oldPrefix, string newPrefix)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));
		if (oldPrefix is null)
			throw new ArgumentNullException(nameof(oldPrefix));
		if (newPrefix is null)
			throw new ArgumentNullException(nameof(newPrefix));

		if (string.Equals(oldPrefix, newPrefix, StringComparison.Ordinal))
			return;

		if (s_table.TryGetValue(page, out var map))
		{
			lock (map)
			{
				map.ReRoot(oldPrefix, newPrefix);
			}
		}
	}

	/// <summary>
	/// Removes a single component registration for the given <paramref name="nodeId"/> under <paramref name="page"/>.
	/// Called from generated <c>UpdateComponent()</c> when a child element is removed during incremental hot reload.
	/// </summary>
	public static void Unregister(object page, string nodeId)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));
		if (nodeId is null)
			throw new ArgumentNullException(nameof(nodeId));

		if (s_table.TryGetValue(page, out var map))
		{
			lock (map)
			{
				map.Remove(nodeId);
			}
		}
	}

	/// <summary>
	/// Removes all component registrations whose node ID starts with <paramref name="nodeIdPrefix"/>
	/// (the node itself and all descendants). Used by generated <c>UpdateComponent</c> methods
	/// when a child element is removed during incremental hot reload.
	/// </summary>
	public static void UnregisterSubtree(object page, string nodeIdPrefix)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));
		if (nodeIdPrefix is null)
			throw new ArgumentNullException(nameof(nodeIdPrefix));

		if (s_table.TryGetValue(page, out var map))
		{
			lock (map)
			{
				map.RemoveSubtree(nodeIdPrefix);
			}
		}
	}

	// -------------------------------------------------------------------------
	// Resource key tracking
	// -------------------------------------------------------------------------

	static readonly ConditionalWeakTable<object, HashSet<string>> s_resourceKeys = new();

	/// <summary>
	/// Stores the set of resource dictionary keys registered by the page's <c>InitializeComponent</c>
	/// or <c>UpdateComponent</c>. Used during incremental hot reload to determine which keys
	/// were removed and need to be deleted from <c>Resources</c>.
	/// </summary>
	public static void RegisterResourceKeys(object page, string[] keys)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));

		s_resourceKeys.Remove(page);
		s_resourceKeys.Add(page, new HashSet<string>(keys, StringComparer.Ordinal));
	}

	/// <summary>
	/// Returns the previously registered resource keys for the given page, or an empty array.
	/// </summary>
	public static string[] GetResourceKeys(object page)
	{
		if (page is null)
			throw new ArgumentNullException(nameof(page));

		if (s_resourceKeys.TryGetValue(page, out var keys))
		{
			var result = new string[keys.Count];
			keys.CopyTo(result);
			return result;
		}
		return Array.Empty<string>();
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

			// Prune the Type key when no instances remain
			if (list.Count == 0)
				s_instancesByType.Remove(type);
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

		public void ReRoot(string oldPrefix, string newPrefix)
		{
			var keysToRename = new List<string>();
			foreach (var key in _entries.Keys)
			{
				if (IsNodeOrDescendant(key, oldPrefix))
					keysToRename.Add(key);
			}
			foreach (var oldKey in keysToRename)
			{
				var newKey = newPrefix + oldKey.Substring(oldPrefix.Length);
				if (_entries.TryGetValue(oldKey, out var value))
				{
					_entries.Remove(oldKey);
					_entries[newKey] = value;
				}
			}
		}

		public void Remove(string nodeId)
		{
			_entries.Remove(nodeId);
		}

		public void RemoveSubtree(string nodeIdPrefix)
		{
			var keysToRemove = new List<string>();
			foreach (var key in _entries.Keys)
			{
				if (IsNodeOrDescendant(key, nodeIdPrefix))
					keysToRemove.Add(key);
			}
			foreach (var key in keysToRemove)
				_entries.Remove(key);
		}

		/// <summary>
		/// Returns true if <paramref name="key"/> is exactly <paramref name="prefix"/>
		/// or is a descendant path (prefix followed by '/'). Prevents "Label_1" from
		/// matching "Label_10", "Label_11", etc.
		/// </summary>
		static bool IsNodeOrDescendant(string key, string prefix) =>
			key.Length == prefix.Length
				? string.Equals(key, prefix, StringComparison.Ordinal)
				: key.Length > prefix.Length && key.StartsWith(prefix, StringComparison.Ordinal) && key[prefix.Length] == '/';
	}
}
