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

		// Batch dispatch — collect ALL (instance, method, type) tuples across every updated type whose
		// XAML actually changed in this delta, then issue a single MainThread.BeginInvokeOnMainThread.
		//
		// handledTypes is the synchronous, pre-dispatch "this is a XAML change" signal tooling reads.
		// UpdateComponent() is now emitted on EVERY XAML type (member stability), so its mere presence no
		// longer distinguishes a XAML edit from a pure C#/code-behind edit. Instead we look at whether the
		// method's BODY is non-empty: the generator emits an EMPTY UpdateComponent() when a generation
		// carries no XAML change, and a patched (non-empty) one when the XAML changed. A pure C# edit does
		// not regenerate the XAML code, so UpdateComponent() stays empty for that page — correctly read as
		// "not a XAML change" — while the method still never appears/disappears across generations.
		var dispatchBatch = new List<(object Instance, MethodInfo Method, Type Type)>();
		var handledTypes = new List<Type>();

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
				continue; // not an incremental-XAML type at all

			if (IsEmptyUpdateComponent(ucMethod))
				continue; // XAML unchanged in this delta (e.g. a pure C# edit) — not a XAML change

			handledTypes.Add(type);

			var instances = XamlComponentRegistry.GetInstances(type);
			foreach (var instance in instances)
				dispatchBatch.Add((instance, ucMethod, type));
		}

		// XamlTools contract: raised SYNCHRONOUSLY, before any dispatch, carrying the recognized subset
		// (HandledTypes) so tooling can classify the update type (XAML Incremental Hot Reload vs. non-XAML)
		// inline, without awaiting the async apply. Keep this call synchronous and pre-dispatch.
		HotReloadDiagnostics.OnUpdateRequested(typesArray, handledTypes);

		if (dispatchBatch.Count == 0)
		{
			// XamlTools contract: terminal event when nothing is dispatched (no live instances) —
			// UpdateApplied never fires on this path, so XamlTools uses UpdateSkipped as the definitive
			// "no apply coming" signal and stops waiting. No version is allocated here, keeping the
			// diagnostic version stream gap-free (every increment is paired with an UpdateApplied).
			HotReloadDiagnostics.OnUpdateSkipped(typesArray, handledTypes);
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
					HotReloadDiagnostics.OnUpdateFailed(capturedType, capturedInstance, inner, toVersion);
				}
#pragma warning restore CA1031
			}

			sw.Stop();
			// XamlTools contract: UpdateApplied is the TERMINAL event of a dispatched batch and must
			// ALWAYS be raised here, after every per-instance OnUpdateFailed above — even if all failed
			// (instanceCount == 0). XamlTools blocks briefly awaiting it to fold the stats into its
			// report; never returning it strands that wait until timeout.
			HotReloadDiagnostics.OnUpdateApplied(typesArray, instanceCount, fromVersion, toVersion, sw.Elapsed);
		});
	}

	// The generator emits an EMPTY UpdateComponent() body when a generation carries no XAML change, and a
	// patched (non-empty) one when the XAML changed. An empty method compiles to a trivial body (just a
	// return / a couple of nops), so its IL is only a few bytes; any real patch is far larger. This lets
	// the handler distinguish a XAML change from a pure C#/code-behind edit without adding or removing the
	// method across generations. Unlike reflecting a generated method's return value, UpdateComponent() is
	// always part of a XAML-change delta (it is what applies Hot Reload), so its body is reliably current.
	const int EmptyUpdateComponentMaxIL = 8;

	// GetMethodBody() is [RequiresUnreferencedCode] (IL2026). The trimmer/ILC honors
	// UnconditionalSuppressMessage (a #pragma only silences the Roslyn analyzer, not the publish-time
	// trim/AOT warning). This is safe: XIHR is a dev-time (Hot Reload) feature gated by
	// RuntimeFeature.IsIncrementalHotReloadEnabled, which is off for Release/publish, so this method is
	// never reached under trimming/AOT — the "trimming may change method bodies" caveat cannot apply.
	[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
		Justification = "XIHR is a dev-time feature disabled under trimming/AOT (Release); IsEmptyUpdateComponent is never reached there.")]
	static bool IsEmptyUpdateComponent(MethodInfo ucMethod)
	{
		try
		{
			var body = ucMethod.GetMethodBody();
			var il = body?.GetILAsByteArray();
			return il is null || il.Length <= EmptyUpdateComponentMaxIL;
		}
#pragma warning disable CA1031
		catch
		{
			// If the IL cannot be inspected on this runtime, fall back to treating the update as a XAML
			// change (the pre-regression behavior) rather than silently dropping it.
			return false;
		}
#pragma warning restore CA1031
	}
}
#endif
