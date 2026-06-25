#nullable enable
#if !NETSTANDARD
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

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
/// Looks up live page instances via <see cref="XamlComponentRegistry.GetInstances"/> and
/// invokes the generated <c>UpdateComponent()</c> method on the main thread when metadata
/// is updated.
/// </summary>
/// <remarks>This type is public for source-generator access only. It is not intended to be used directly.</remarks>
[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
public static class XamlIncrementalHotReloadHandler
{
	/// <summary>
	/// Generator-only entry point retained for backward compatibility with previously
	/// compiled <c>InitializeComponent()</c> bodies. <see cref="XamlComponentRegistry.Register"/>
	/// already records every page instance, so live-instance enumeration is sourced from
	/// <see cref="XamlComponentRegistry.GetInstances"/>. This method only validates its
	/// argument and respects the <c>IsIncrementalHotReloadEnabled</c> feature switch.
	/// </summary>
	public static void Track(object instance)
	{
		if (instance is null)
			throw new ArgumentNullException(nameof(instance));

		if (!global::Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled)
			return;

		// No-op: XamlComponentRegistry.Register has already tracked this instance via
		// its secondary type-indexed weak-reference list.
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

		var typesArray = (IReadOnlyList<Type>)updatedTypes;

		HotReloadDiagnostics.OnUpdateRequested(typesArray);

		// Start timing at the moment the request is received so Duration includes batch
		// building and main-thread dispatch latency, not just the UI-thread invoke loop.
		var sw = Stopwatch.StartNew();

		// Batch dispatch — collect ALL (instance, method, type) tuples across every updated
		// type, then issue a single MainThread.BeginInvokeOnMainThread that iterates them.
		var dispatchBatch = new List<(object Instance, MethodInfo Method, Type Type)>();

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

			var instances = XamlComponentRegistry.GetInstances(type);
			if (instances.Count == 0)
				continue;

			foreach (var instance in instances)
				dispatchBatch.Add((instance, ucMethod, type));
		}

		if (dispatchBatch.Count == 0)
			return;

		// Allocate the version range atomically once we know work will happen: toVersion is the
		// new generation, fromVersion the one before it. Reserving the number only for non-empty
		// batches keeps the diagnostic version stream gap-free (every increment has a paired
		// UpdateApplied).
		var toVersion = HotReloadDiagnostics.IncrementVersion();
		var fromVersion = toVersion - 1;

		global::Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
		{
			int instanceCount = 0;

			foreach (var (capturedInstance, capturedMethod, capturedType) in dispatchBatch)
			{
				try
				{
					capturedMethod.Invoke(capturedInstance, null);
					instanceCount++;
				}
#pragma warning disable CA1031
				catch (Exception ex)
				{
					var inner = ex.InnerException ?? ex;
					System.Diagnostics.Debug.WriteLine(
						$"[XIHR] UpdateComponent failed for {capturedType.Name}: {inner.Message}");
					HotReloadDiagnostics.OnUpdateFailed(capturedType, capturedInstance, inner);
				}
#pragma warning restore CA1031
			}

			sw.Stop();
			HotReloadDiagnostics.OnUpdateApplied(typesArray, instanceCount, fromVersion, toVersion, sw.Elapsed);
		});
	}
}
#endif
