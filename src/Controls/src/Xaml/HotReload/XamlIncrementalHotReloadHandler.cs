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

		// Start timing at the moment the request is received so Duration includes batch
		// building and main-thread dispatch latency, not just the UI-thread invoke loop.
		var sw = Stopwatch.StartNew();

		// Batch dispatch — collect ALL (instance, method, type, __version field) tuples across every
		// updated type carrying a generated UpdateComponent(), then issue a single
		// MainThread.BeginInvokeOnMainThread that iterates them.
		//
		// candidateTypes is a COARSE, synchronous, pre-dispatch hint: every updated type that has an
		// UpdateComponent(). Because UC is now emitted on every XAML type from the first compile, its
		// presence no longer means "this delta changed the XAML" — a pure C#/code-behind edit to a XAML
		// page also lands here. The AUTHORITATIVE "which types actually changed their XAML" set is
		// computed POST-dispatch below (see the dispatch loop): running UpdateComponent() — the same call
		// that applies Hot Reload — stamps the instance's __version with the new content hash only when
		// the XAML changed, so a moved __version is a reliable per-instance "XAML changed" signal.
		var dispatchBatch = new List<(object Instance, MethodInfo Method, Type Type, FieldInfo? VersionField)>();
		var candidateTypes = new List<Type>();

		foreach (var type in updatedTypes)
		{
#pragma warning disable IL2070, IL2075
			var ucMethod = type.GetMethod(
				"UpdateComponent",
				BindingFlags.NonPublic | BindingFlags.Instance,
				binder: null,
				types: Type.EmptyTypes,
				modifiers: null);
			var versionField = type.GetField("__version", BindingFlags.NonPublic | BindingFlags.Instance);
#pragma warning restore IL2070, IL2075

			if (ucMethod is null)
				continue; // not an incremental-XAML type at all

			candidateTypes.Add(type);

			var instances = XamlComponentRegistry.GetInstances(type);
			foreach (var instance in instances)
				dispatchBatch.Add((instance, ucMethod, type, versionField));
		}

		// XamlTools contract: raised SYNCHRONOUSLY, before any dispatch. HandledTypes here is the COARSE
		// candidate set (types carrying a UC). The precise XAML-vs-non-XAML classification is now on
		// UpdateApplied.HandledTypes (post-apply); tooling should prefer that. Kept for the pre-dispatch
		// "an update is happening" signal and back-compat.
		HotReloadDiagnostics.OnUpdateRequested(typesArray, candidateTypes);

		if (dispatchBatch.Count == 0)
		{
			// XamlTools contract: terminal event when nothing is dispatched (no live instances) —
			// UpdateApplied never fires on this path, so XamlTools uses UpdateSkipped as the definitive
			// "no apply coming" signal and stops waiting. With no live instance we cannot observe a
			// __version transition, so this path can only surface the coarse candidate set. No version is
			// allocated here, keeping the diagnostic version stream gap-free.
			HotReloadDiagnostics.OnUpdateSkipped(typesArray, candidateTypes);
			return;
		}

		// Allocate the version range atomically once we know work will happen: toVersion is the
		// new generation, fromVersion the one before it. Reserving the number only for non-empty
		// batches keeps the diagnostic version stream gap-free (every increment has a paired
		// UpdateApplied).
		var toVersion = HotReloadDiagnostics.IncrementVersion();
		var fromVersion = toVersion - 1;

		global::Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
		{
			int instanceCount = 0;
			// Types whose XAML actually changed in this apply — the authoritative classification.
			var changedTypes = new List<Type>();

			foreach (var (capturedInstance, capturedMethod, capturedType, versionField) in dispatchBatch)
			{
				try
				{
					// Observe the content identity before/after running UpdateComponent(). UC — the same
					// call that applies Hot Reload — stamps __version with the new content hash ONLY when
					// the XAML changed (a pure C# delta leaves UC's body, and the stamp, untouched). A
					// field read after the call is reliable even where reflecting the generated static
					// content-hash accessor is not (which is why the pre-dispatch accessor approach failed).
					var before = ReadVersion(versionField, capturedInstance);
					capturedMethod.Invoke(capturedInstance, null);
					instanceCount++;
					var after = ReadVersion(versionField, capturedInstance);

					if (before is int b && after is int a && b != a && !changedTypes.Contains(capturedType))
						changedTypes.Add(capturedType);
				}
#pragma warning disable CA1031
				catch (Exception ex)
				{
					var inner = ex.InnerException ?? ex;
					System.Diagnostics.Debug.WriteLine(
						$"[XIHR] UpdateComponent failed for {capturedType.Name}: {inner.Message}");
					HotReloadDiagnostics.OnUpdateFailed(capturedType, capturedInstance, inner, toVersion);
				}
#pragma warning restore CA1031
			}

			sw.Stop();
			// XamlTools contract: UpdateApplied is the TERMINAL event of a dispatched batch and must
			// ALWAYS be raised here, after every per-instance OnUpdateFailed above — even if all failed
			// (instanceCount == 0). Its HandledTypes is the AUTHORITATIVE set of types whose XAML changed
			// (empty for a pure C# delta), which tooling should use to classify the cycle.
			HotReloadDiagnostics.OnUpdateApplied(typesArray, changedTypes, instanceCount, fromVersion, toVersion, sw.Elapsed);
		});
	}

	static int? ReadVersion(FieldInfo? versionField, object instance)
		=> versionField?.GetValue(instance) is int version ? version : (int?)null;
}
#endif
