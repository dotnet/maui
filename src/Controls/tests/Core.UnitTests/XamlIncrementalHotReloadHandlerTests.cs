#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// Runtime tests for XamlIncrementalHotReloadHandler.UpdateApplication and the HotReloadDiagnostics
	// events it raises: the synchronous HandledTypes classification signal on UpdateRequested, the
	// terminal UpdateSkipped event, InstanceCount/version reporting on UpdateApplied, and the Version
	// correlation carried on UpdateFailed.
	public class XamlIncrementalHotReloadHandlerTests : BaseTestFixture
	{
		const string IncrementalHotReloadSwitch = "Microsoft.Maui.RuntimeFeature.IsIncrementalHotReloadEnabled";

		readonly List<object> _registeredPages = new();

		public XamlIncrementalHotReloadHandlerTests()
		{
			// The XIHR handler dispatches the UI-thread patch through MainThread.BeginInvokeOnMainThread.
			// In the unit-test (netstandard Essentials) harness MainThread has no platform backend, so
			// install a synchronous one that reports "on main thread" and runs callbacks inline.
			MainThread.SetCustomImplementation(() => true, action => action());
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var page in _registeredPages)
					XamlComponentRegistry.Unregister(page);
				_registeredPages.Clear();
				AppContext.SetSwitch(IncrementalHotReloadSwitch, false);
				MainThread.ClearCustomImplementation();
			}

			base.Dispose(disposing);
		}

		// -----------------------------------------------------------------------
		// Fakes. A source-generated XAML page carries a non-public parameterless UpdateComponent();
		// that is exactly what XamlIncrementalHotReloadHandler reflects for, so these stand in for
		// generated pages without needing the source generator.
		// -----------------------------------------------------------------------
#pragma warning disable IDE0051 // invoked by the handler via reflection, not directly
		sealed class HotReloadablePage
		{
			public int UpdateComponentCalls;

			void UpdateComponent() => UpdateComponentCalls++;
		}

		sealed class ThrowingHotReloadablePage
		{
			public int Attempts;

			void UpdateComponent()
			{
				Attempts++;
				throw new InvalidOperationException("update failed");
			}
		}
#pragma warning restore IDE0051

		// A plain type with no UpdateComponent() — represents a pure-C# (non-XAML) metadata delta.
		sealed class PlainType
		{
		}

		// -----------------------------------------------------------------------
		// Helpers
		// -----------------------------------------------------------------------
		static IDisposable IncrementalHotReloadEnabled()
		{
			var previous = AppContext.TryGetSwitch(IncrementalHotReloadSwitch, out var value) && value;
			AppContext.SetSwitch(IncrementalHotReloadSwitch, true);
			return new Restore(() => AppContext.SetSwitch(IncrementalHotReloadSwitch, previous));
		}

		T RegisterLiveInstance<T>() where T : class, new()
		{
			var page = new T();
			XamlComponentRegistry.Register(page, "0", page);
			_registeredPages.Add(page);
			return page;
		}

		sealed class Restore : IDisposable
		{
			readonly Action _onDispose;
			public Restore(Action onDispose) => _onDispose = onDispose;
			public void Dispose() => _onDispose();
		}

		sealed class DiagnosticsRecorder : IDisposable
		{
			public readonly List<HotReloadRequestedEventArgs> Requested = new();
			public readonly List<HotReloadAppliedEventArgs> Applied = new();
			public readonly List<HotReloadErrorEventArgs> Failed = new();
			public readonly List<HotReloadSkippedEventArgs> Skipped = new();

			readonly EventHandler<HotReloadRequestedEventArgs> _onRequested;
			readonly EventHandler<HotReloadAppliedEventArgs> _onApplied;
			readonly EventHandler<HotReloadErrorEventArgs> _onFailed;
			readonly EventHandler<HotReloadSkippedEventArgs> _onSkipped;

			public DiagnosticsRecorder()
			{
				_onRequested = (_, e) => Requested.Add(e);
				_onApplied = (_, e) => Applied.Add(e);
				_onFailed = (_, e) => Failed.Add(e);
				_onSkipped = (_, e) => Skipped.Add(e);

				HotReloadDiagnostics.UpdateRequested += _onRequested;
				HotReloadDiagnostics.UpdateApplied += _onApplied;
				HotReloadDiagnostics.UpdateFailed += _onFailed;
				HotReloadDiagnostics.UpdateSkipped += _onSkipped;
			}

			public void Dispose()
			{
				HotReloadDiagnostics.UpdateRequested -= _onRequested;
				HotReloadDiagnostics.UpdateApplied -= _onApplied;
				HotReloadDiagnostics.UpdateFailed -= _onFailed;
				HotReloadDiagnostics.UpdateSkipped -= _onSkipped;
			}
		}

		// -----------------------------------------------------------------------
		// Tests
		// -----------------------------------------------------------------------

		[Fact]
		public void FeatureDisabled_RaisesNoEvents()
		{
			// switch defaults to false; do not enable it
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(HotReloadablePage) });

			Assert.Empty(recorder.Requested);
			Assert.Empty(recorder.Applied);
			Assert.Empty(recorder.Failed);
			Assert.Empty(recorder.Skipped);
		}

		[Fact]
		public void XamlType_IsReportedInHandledTypes()
		{
			using var _ = IncrementalHotReloadEnabled();
			RegisterLiveInstance<HotReloadablePage>();
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(HotReloadablePage) });

			var requested = Assert.Single(recorder.Requested);
			Assert.Contains(typeof(HotReloadablePage), requested.UpdatedTypes);
			Assert.Contains(typeof(HotReloadablePage), requested.HandledTypes);
		}

		[Fact]
		public void MixedDelta_HandledTypesContainsOnlyTheXamlType()
		{
			using var _ = IncrementalHotReloadEnabled();
			RegisterLiveInstance<HotReloadablePage>();
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(PlainType), typeof(HotReloadablePage) });

			var requested = Assert.Single(recorder.Requested);
			Assert.Equal(2, requested.UpdatedTypes.Count);
			Assert.Equal(new[] { typeof(HotReloadablePage) }, requested.HandledTypes.ToArray());
		}

		[Fact]
		public void PureCSharpDelta_HasEmptyHandledTypes_AndIsSkipped()
		{
			using var _ = IncrementalHotReloadEnabled();
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(PlainType) });

			var requested = Assert.Single(recorder.Requested);
			Assert.Empty(requested.HandledTypes);

			var skipped = Assert.Single(recorder.Skipped);
			Assert.Empty(skipped.HandledTypes);
			Assert.Empty(recorder.Applied);
		}

		[Fact]
		public void RecognizedTypeWithoutLiveInstances_RaisesSkipped_NotApplied()
		{
			using var _ = IncrementalHotReloadEnabled();
			// note: no RegisterLiveInstance — the type is recognized but has nothing to patch
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(HotReloadablePage) });

			var requested = Assert.Single(recorder.Requested);
			Assert.Contains(typeof(HotReloadablePage), requested.HandledTypes);

			var skipped = Assert.Single(recorder.Skipped);
			Assert.Contains(typeof(HotReloadablePage), skipped.HandledTypes);
			Assert.Empty(recorder.Applied);
		}

		[Fact]
		public void LiveInstance_IsPatched_AndAppliedReportsInstanceCountAndVersionRange()
		{
			using var _ = IncrementalHotReloadEnabled();
			var page = RegisterLiveInstance<HotReloadablePage>();
			var versionBefore = HotReloadDiagnostics.CurrentVersion;
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(HotReloadablePage) });

			Assert.Equal(1, page.UpdateComponentCalls);
			Assert.Empty(recorder.Skipped);

			var applied = Assert.Single(recorder.Applied);
			Assert.Equal(1, applied.InstanceCount);
			Assert.Equal(versionBefore, applied.FromVersion);
			Assert.Equal(versionBefore + 1, applied.ToVersion);
			Assert.Contains(typeof(HotReloadablePage), applied.UpdatedTypes);
		}

		[Fact]
		public void FailingUpdateComponent_RaisesUpdateFailed_CorrelatedByVersion()
		{
			using var _ = IncrementalHotReloadEnabled();
			RegisterLiveInstance<ThrowingHotReloadablePage>();
			var versionBefore = HotReloadDiagnostics.CurrentVersion;
			using var recorder = new DiagnosticsRecorder();

			XamlIncrementalHotReloadHandler.UpdateApplication(new[] { typeof(ThrowingHotReloadablePage) });

			var failed = Assert.Single(recorder.Failed);
			Assert.Equal(typeof(ThrowingHotReloadablePage), failed.UpdatedType);
			Assert.IsType<InvalidOperationException>(failed.Exception);
			Assert.Equal(versionBefore + 1, failed.Version);

			// UpdateApplied still fires as the terminal event; the failed instance is not counted.
			var applied = Assert.Single(recorder.Applied);
			Assert.Equal(0, applied.InstanceCount);
			Assert.Equal(failed.Version, applied.ToVersion);
		}
	}
}
