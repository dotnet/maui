#nullable enable
#if !NETSTANDARD
using System;
using System.Globalization;
using System.Reflection;

// Single assembly-level MetadataUpdateHandler for all XAML page types in user assemblies.
// The .NET hot-reload infrastructure passes the list of updated types to UpdateApplication();
// this single SDK handler iterates over them and dispatches the generated UpdateComponent_vNtoM()
// methods, so no per-page generated handler class is needed.
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
/// this handler reads the <c>__version</c> field (written by the generated
/// <c>InitializeComponent</c>) and invokes the matching
/// <c>UpdateComponent_v{N}to{N+1}()</c> partial method (emitted by the XAML Source
/// Generator) to apply only the changed property assignments.
/// </para>
/// <para>
/// If no <c>__version</c> field exists on a type (i.e. the page was compiled without
/// <c>EnableMauiIncrementalHotReload</c>) the update is silently skipped and the
/// existing full-reload path handles it.
/// </para>
/// <para>
/// <b>Trim / Native AOT:</b> Dispatch uses reflection; in trimmed / AOT builds these
/// calls no-op because <c>UpdateComponent_vNtoM</c> methods may be trimmed away.
/// This is intentional — the feature is only active in Debug builds where trimming
/// is not applied.
/// </para>
/// </remarks>
internal static class XamlIncrementalHotReloadHandler
{
	// Cache FieldInfo per type to avoid repeated GetField() on every hot-reload cycle.
	static readonly System.Collections.Generic.Dictionary<Type, FieldInfo?> s_versionFieldCache =
		new System.Collections.Generic.Dictionary<Type, FieldInfo?>();

	static readonly object s_cacheLock = new object();

	/// <summary>
	/// Called by the .NET hot-reload infrastructure <em>before</em> metadata is applied.
	/// Clears the cached <c>FieldInfo</c> entries so stale reflection results from a
	/// previous hot-reload cycle do not survive across metadata updates.
	/// </summary>
	public static void ClearCache(Type[]? updatedTypes)
	{
		if (updatedTypes is null)
			return;

		lock (s_cacheLock)
		{
			foreach (var t in updatedTypes)
				s_versionFieldCache.Remove(t);
		}
	}

	/// <summary>
	/// Called by the .NET hot-reload infrastructure after every
	/// <c>MetadataUpdater.ApplyUpdate</c> to push property changes to live page instances.
	/// </summary>
	/// <param name="updatedTypes">
	/// The types whose metadata was updated. May be <see langword="null"/> if the
	/// runtime cannot determine the exact set, in which case the handler is a no-op.
	/// </param>
	public static void UpdateApplication(Type[]? updatedTypes)
	{
		if (updatedTypes is null)
			return;

		foreach (var type in updatedTypes)
		{
			var instances = XamlComponentRegistry.GetInstances(type);
			if (instances.Count == 0)
				continue;

			var versionField = GetVersionField(type);
			if (versionField is null)
				continue; // Type not compiled with IHR; skip.

			foreach (var instance in instances)
			{
				try
				{
					var version = (int)(versionField.GetValue(instance) ?? 0);

					// Loop to chain multiple version steps (e.g., v1→v2→v3 if several saves occurred).
					while (true)
					{
						var methodName = string.Concat(
							"UpdateComponent_v",
							version.ToString(CultureInfo.InvariantCulture),
							"to",
							(version + 1).ToString(CultureInfo.InvariantCulture));

#pragma warning disable IL2075 // type comes from updatedTypes at runtime; DynamicallyAccessedMembers cannot annotate loop variables
						var method = type.GetMethod(
							methodName,
							BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore IL2075

						// method is null when no UC was generated for this version step.
						if (method is null)
							break;

						method.Invoke(instance, null);

						// Re-read version — the UC method updates __version on success.
						var newVersion = (int)(versionField.GetValue(instance) ?? 0);
						if (newVersion <= version)
							break; // Guard against infinite loop if __version didn't advance.
						version = newVersion;
					}
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

	static FieldInfo? GetVersionField(Type type)
	{
		lock (s_cacheLock)
		{
			if (s_versionFieldCache.TryGetValue(type, out var cached))
				return cached;
		}

#pragma warning disable IL2070 // type comes from updatedTypes at runtime; DynamicallyAccessedMembers cannot annotate loop variables
		// Intentional: reflection outside the lock (expensive but thread-safe).
		var field = type.GetField(
			"__version",
			BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore IL2070

		lock (s_cacheLock)
		{
			// Another thread may have raced; last write wins — both values are the same field.
			s_versionFieldCache[type] = field;
			return field;
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

		initMethod?.Invoke(instance, null);

		// Restore BindingContext
		if (bindableObj != null)
		{
			bindableObj.BindingContext = savedBindingContext;
		}
	}
}
#endif
