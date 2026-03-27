#nullable enable
#if !NETSTANDARD
using System;
using System.Collections.Concurrent;
using System.Reflection;

// Single assembly-level MetadataUpdateHandler for all XAML page types in user assemblies.
// The .NET hot-reload infrastructure passes the list of updated types to UpdateApplication();
// this handler calls the generated UpdateComponent() method on each live instance.
// The handler is a no-op for types compiled without EnableMauiIncrementalHotReload
// (no UpdateComponent method exists, so the type is silently skipped).
#pragma warning disable IL2026
[assembly: global::System.Reflection.Metadata.MetadataUpdateHandler(
    typeof(global::Microsoft.Maui.Controls.Xaml.XamlIncrementalHotReloadHandler))]
#pragma warning restore IL2026

namespace Microsoft.Maui.Controls.Xaml;

/// <summary>
/// SDK-level <c>[MetadataUpdateHandler]</c> for XAML Incremental Hot Reload.
/// Registered once in <c>Microsoft.Maui.Controls</c>; handles hot-reload notifications
/// for <em>all</em> XAML pages across all user assemblies.
/// </summary>
/// <remarks>
/// <para>
/// When the .NET hot-reload infrastructure applies a metadata update it calls
/// <see cref="UpdateApplication"/> with the set of updated types.
/// For each type that has live instances tracked in <see cref="XamlComponentRegistry"/>,
/// this handler invokes the generated <c>UpdateComponent()</c> method which contains
/// sequential <c>if (__version == N)</c> blocks that chain the instance forward
/// from its current version to the latest.
/// </para>
/// <para>
/// If no <c>UpdateComponent</c> method exists on a type (i.e. the page was compiled without
/// <c>EnableMauiIncrementalHotReload</c>) the update is silently skipped and the
/// existing full-reload path handles it.
/// </para>
/// <para>
/// <b>Trim / Native AOT:</b> Dispatch uses reflection; in trimmed / AOT builds these
/// calls no-op because <c>UpdateComponent</c> may be trimmed away.
/// This is intentional — the feature is only active in Debug builds where trimming
/// is not applied.
/// </para>
/// </remarks>
internal static class XamlIncrementalHotReloadHandler
{
	// ConcurrentDictionary eliminates lock contention and race conditions in the cache.
	static readonly ConcurrentDictionary<Type, MethodInfo?> s_methodCache = new();

	/// <summary>
	/// Called by the .NET hot-reload infrastructure <em>before</em> metadata is applied.
	/// Clears the cached <c>MethodInfo</c> entries so stale reflection results from a
	/// previous hot-reload cycle do not survive across metadata updates.
	/// </summary>
	public static void ClearCache(Type[]? updatedTypes)
	{
		if (updatedTypes is null)
			return;

		foreach (var t in updatedTypes)
			s_methodCache.TryRemove(t, out _);
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
			var instances = XamlComponentRegistry.GetInstances(type);
			if (instances.Count == 0)
				continue;

			var ucMethod = GetUpdateComponentMethod(type);
			if (ucMethod is null)
				continue; // Type not compiled with IHR; skip.

			foreach (var instance in instances)
			{
				var capturedInstance = instance;
				var capturedMethod = ucMethod;
				var capturedType = type;

				// UI mutations must run on the main thread.
				global::Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
				{
					try
					{
						System.Diagnostics.Debug.WriteLine(
							$"[XamlIncrementalHotReload] Calling UpdateComponent() on {capturedType.Name} (instance {capturedInstance.GetHashCode():X8})");

						capturedMethod.Invoke(capturedInstance, null);
					}
#pragma warning disable CA1031 // Do not catch general exception types
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(
							$"[XamlIncrementalHotReload] UpdateComponent failed for {capturedType.Name}: {ex.InnerException?.Message ?? ex.Message}. Falling back to full reload.");
						try
						{
							FallbackReload(capturedInstance, capturedType);
						}
						catch (Exception fallbackEx)
						{
							System.Diagnostics.Debug.WriteLine(
								$"[XamlIncrementalHotReload] Fallback reload also failed for {capturedType.Name}: {fallbackEx.Message}");
						}
					}
#pragma warning restore CA1031
				});
			}
		}
	}

	static MethodInfo? GetUpdateComponentMethod(Type type)
	{
		return s_methodCache.GetOrAdd(type, static t =>
		{
#pragma warning disable IL2070, IL2075 // type comes from updatedTypes at runtime
			return t.GetMethod(
				"UpdateComponent",
				BindingFlags.NonPublic | BindingFlags.Instance,
				binder: null,
				types: Type.EmptyTypes,
				modifiers: null);
#pragma warning restore IL2070, IL2075
		});
	}

	/// <summary>
	/// Fallback: unregister components, save BindingContext, re-run InitializeComponent, restore BindingContext.
	/// </summary>
	static void FallbackReload(object instance, Type type)
	{
		// Save and clear BindingContext if instance is a BindableObject
		object? savedBindingContext = null;
		var bindableObj = instance as global::Microsoft.Maui.Controls.BindableObject;
		if (bindableObj != null)
		{
			savedBindingContext = bindableObj.BindingContext;
			bindableObj.BindingContext = null;
		}

		// Clean up stale registry entries before re-init
		XamlComponentRegistry.Unregister(instance);

#pragma warning disable IL2070, IL2075
		var initMethod = type.GetMethod(
			"InitializeComponent",
			BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
			null, Type.EmptyTypes, null);
#pragma warning restore IL2070, IL2075

		try
		{
			initMethod?.Invoke(instance, null);
		}
		finally
		{
			// Always restore BindingContext, even if InitializeComponent throws
			if (bindableObj != null)
			{
				bindableObj.BindingContext = savedBindingContext;
			}
		}
	}
}
#endif
