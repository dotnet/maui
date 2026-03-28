#nullable enable
#if !NETSTANDARD
using System;
using System.Collections.Generic;
using System.Reflection;

// Single assembly-level MetadataUpdateHandler for all XAML page types in user assemblies.
// The .NET hot-reload infrastructure passes the list of updated types to UpdateApplication();
// this handler calls the generated UpdateComponent() method on each live instance.
#pragma warning disable IL2026
[assembly: global::System.Reflection.Metadata.MetadataUpdateHandler(
    typeof(global::Microsoft.Maui.Controls.Xaml.XamlIncrementalHotReloadHandler))]
#pragma warning restore IL2026

namespace Microsoft.Maui.Controls.Xaml;

/// <summary>
/// SDK-level <c>[MetadataUpdateHandler]</c> for XAML Incremental Hot Reload.
/// Tracks live page instances via weak references and invokes the generated
/// <c>UpdateComponent()</c> method on the main thread when metadata is updated.
/// </summary>
/// <remarks>This type is public for source-generator access only. It is not intended to be used directly.</remarks>
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
public static class XamlIncrementalHotReloadHandler
{
	// Two-level table: Type → list of weak references to instances of that type.
	// Track() is O(instances-of-type) instead of O(all-instances),
	// and UpdateApplication() skips types with no tracked instances entirely.
	static readonly Dictionary<Type, List<WeakReference>> s_instancesByType = new();

	/// <summary>
	/// Call from page constructors (after InitializeComponent) to register the
	/// instance for incremental hot-reload updates.
	/// </summary>
	public static void Track(object instance)
	{
		if (!global::Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled)
			return;

		var type = instance.GetType();
		lock (s_instancesByType)
		{
			if (!s_instancesByType.TryGetValue(type, out var list))
			{
				list = new List<WeakReference>();
				s_instancesByType[type] = list;
			}

			// InitializeComponent may run multiple times; avoid duplicate tracking
			for (int i = 0; i < list.Count; i++)
			{
				if (ReferenceEquals(list[i].Target, instance))
					return;
			}
			list.Add(new WeakReference(instance));
		}
	}

	/// <summary>
	/// Called by the .NET hot-reload infrastructure <em>before</em> metadata is applied.
	/// </summary>
	public static void ClearCache(Type[]? updatedTypes)
	{
	}

	/// <summary>
	/// Called by the .NET hot-reload infrastructure after every
	/// <c>MetadataUpdater.ApplyUpdate</c> to push property changes to live page instances.
	/// </summary>
	public static void UpdateApplication(Type[]? updatedTypes)
	{
		if (updatedTypes is null)
			return;

		if (!global::Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled)
			return;

		foreach (var type in updatedTypes)
		{
#pragma warning disable IL2070, IL2075
			var ucMethod = type.GetMethod(
				"UpdateComponent",
				BindingFlags.NonPublic | BindingFlags.Instance,
				binder: null,
				types: Type.EmptyTypes,
				modifiers: null);
#pragma warning restore IL2070, IL2075

			if (ucMethod is null)
				continue;

			List<WeakReference> snapshot;
			lock (s_instancesByType)
			{
				if (!s_instancesByType.TryGetValue(type, out var list) || list.Count == 0)
					continue;
				snapshot = new List<WeakReference>(list);
			}

			foreach (var weakRef in snapshot)
			{
				var instance = weakRef.Target;
				if (instance is null)
					continue;

				var capturedInstance = instance;
				var capturedMethod = ucMethod;
				var capturedType = type;

				global::Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
				{
					try
					{
						capturedMethod.Invoke(capturedInstance, null);
					}
#pragma warning disable CA1031
					catch (Exception ex)
					{
						var inner = ex.InnerException ?? ex;
						System.Diagnostics.Debug.WriteLine(
							$"[XIHR] UpdateComponent failed for {capturedType.Name}: {inner.Message}");
					}
#pragma warning restore CA1031
				});
			}
		}

		// Cleanup dead refs across all types
		lock (s_instancesByType)
		{
			var emptyTypes = new List<Type>();
			foreach (var kvp in s_instancesByType)
			{
				kvp.Value.RemoveAll(w => !w.IsAlive);
				if (kvp.Value.Count == 0)
					emptyTypes.Add(kvp.Key);
			}
			foreach (var t in emptyTypes)
				s_instancesByType.Remove(t);
		}
	}
}
#endif
