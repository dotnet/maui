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
	static readonly List<WeakReference> s_instances = new();

	/// <summary>
	/// Call from page constructors (after InitializeComponent) to register the
	/// instance for incremental hot-reload updates.
	/// </summary>
	public static void Track(object instance)
	{
		if (!global::Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled)
			return;

		lock (s_instances)
		{
			// InitializeComponent may run multiple times; avoid duplicate tracking
			for (int i = 0; i < s_instances.Count; i++)
			{
				if (ReferenceEquals(s_instances[i].Target, instance))
					return;
			}
			s_instances.Add(new WeakReference(instance));
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
			lock (s_instances)
			{
				snapshot = new List<WeakReference>(s_instances);
			}

			foreach (var weakRef in snapshot)
			{
				if (!weakRef.IsAlive)
					continue;

				var instance = weakRef.Target;
				if (instance is null || instance.GetType() != type)
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

		// Cleanup dead refs
		lock (s_instances)
		{
			s_instances.RemoveAll(w => !w.IsAlive);
		}
	}
}
#endif
