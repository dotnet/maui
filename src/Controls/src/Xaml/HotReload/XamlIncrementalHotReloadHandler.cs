#nullable enable
#if !NETSTANDARD
using System;
using System.Reflection;

// Single assembly-level MetadataUpdateHandler for all XAML page types in user assemblies.
// The .NET hot-reload infrastructure passes the list of updated types to UpdateApplication();
// this handler calls the generated UpdateComponent() method on each live instance.
#pragma warning disable IL2026 // RequiresUnreferencedCode: whole class is debug-time only; trimming never active
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
	// Cache MethodInfo per type to avoid repeated GetMethod() on every hot-reload cycle.
	static readonly System.Collections.Generic.Dictionary<Type, MethodInfo?> s_methodCache =
		new System.Collections.Generic.Dictionary<Type, MethodInfo?>();

	static readonly object s_cacheLock = new object();

	/// <summary>
	/// Called by the .NET hot-reload infrastructure <em>before</em> metadata is applied.
	/// Clears the cached <c>MethodInfo</c> entries so stale reflection results from a
	/// previous hot-reload cycle do not survive across metadata updates.
	/// </summary>
	public static void ClearCache(Type[]? updatedTypes)
	{
		if (updatedTypes is null)
			return;

		lock (s_cacheLock)
		{
			foreach (var t in updatedTypes)
				s_methodCache.Remove(t);
		}
	}

	/// <summary>
	/// Called by the .NET hot-reload infrastructure after every
	/// <c>MetadataUpdater.ApplyUpdate</c> to push property changes to live page instances.
	/// </summary>
	public static void UpdateApplication(Type[]? updatedTypes)
	{
		if (updatedTypes is null)
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
				try
				{
					System.Diagnostics.Debug.WriteLine(
						$"[XamlIncrementalHotReload] Calling UpdateComponent() on {type.Name} (instance {instance.GetHashCode():X8})");

					ucMethod.Invoke(instance, null);
				}
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception ex)
				{
					// Fallback: attempt full-page reload per spec (clear BindingContext → InitializeComponent → restore).
					System.Diagnostics.Debug.WriteLine(
						$"[XamlIncrementalHotReload] UpdateComponent failed for {type.Name}: {ex.InnerException?.Message ?? ex.Message}. Falling back to full reload.");
					try
					{
						FallbackReload(instance, type);
					}
					catch (Exception fallbackEx)
					{
						System.Diagnostics.Debug.WriteLine(
							$"[XamlIncrementalHotReload] Fallback reload also failed for {type.Name}: {fallbackEx.Message}");
					}
				}
#pragma warning restore CA1031
			}
		}
	}

	static MethodInfo? GetUpdateComponentMethod(Type type)
	{
		lock (s_cacheLock)
		{
			if (s_methodCache.TryGetValue(type, out var cached))
				return cached;
		}

#pragma warning disable IL2075 // type comes from updatedTypes at runtime
		var method = type.GetMethod(
			"UpdateComponent",
			BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore IL2075

		lock (s_cacheLock)
		{
			s_methodCache[type] = method;
			return method;
		}
	}

	/// <summary>
	/// Fallback: save BindingContext, re-run InitializeComponent, restore BindingContext.
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

#pragma warning disable IL2075
		var initMethod = type.GetMethod(
			"InitializeComponent",
			BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
			null, Type.EmptyTypes, null);
#pragma warning restore IL2075

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
