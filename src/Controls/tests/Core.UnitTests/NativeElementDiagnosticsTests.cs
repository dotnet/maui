using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[CollectionDefinition(CollectionName, DisableParallelization = true)]
	public sealed class NativeElementDiagnosticsCollection
	{
		public const string CollectionName = nameof(NativeElementDiagnosticsCollection);
	}

	[Collection(NativeElementDiagnosticsCollection.CollectionName)]
	public class NativeElementDiagnosticsTests
	{
		[Fact]
		public void EventNamesUseContractVersion()
		{
			var versionSuffix = $".v{NativeElementDiagnostics.ContractVersion}";

			Assert.EndsWith(versionSuffix, NativeElementDiagnostics.RegisteredEventName, StringComparison.Ordinal);
			Assert.EndsWith(versionSuffix, NativeElementDiagnostics.UnregisteredEventName, StringComparison.Ordinal);
		}

		[Fact]
		public void RegisterEmitsVersionedLifecycleWithReferenceIdentity()
		{
			var owner = new object();
			var nativeElement = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));

			using (NativeElementDiagnostics.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel))
			{
				var registered = Assert.Single(events);
				Assert.Equal(NativeElementDiagnostics.RegisteredEventName, registered.Key);
				var payload = Assert.IsType<object[]>(registered.Value);
				Assert.Equal(NativeElementDiagnostics.ContractVersion, payload[0]);
				Assert.Same(owner, payload[1]);
				Assert.Same(nativeElement, payload[2]);
				Assert.Equal(NativeElementRoles.ToolbarItem, payload[3]);
				Assert.Equal(NativeElementDiscriminators.LogicalModel, payload[4]);
			}

			Assert.Equal(2, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[1].Key);
			var unregisteredPayload = Assert.IsType<object[]>(events[1].Value);
			Assert.Equal(NativeElementDiagnostics.ContractVersion, unregisteredPayload[0]);
			Assert.Same(nativeElement, unregisteredPayload[1]);
		}

		[Fact]
		public void RegistrationCreatedWithoutListenerDoesNotEmitLateUnregister()
		{
			var registration = NativeElementDiagnostics.Register(
				new object(),
				new object(),
				NativeElementRoles.Toolbar);
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));

			registration.Dispose();

			Assert.Empty(events);
		}

		[Fact]
		public void ObserverExceptionsDoNotBreakRegistrationLifecycle()
		{
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new ThrowingObserver());

			var exception = Record.Exception(() =>
			{
				using var registration = NativeElementDiagnostics.Register(
					new object(),
					new object(),
					NativeElementRoles.Toolbar);
			});

			Assert.Null(exception);
		}

		[Fact]
		public void RegistrationSetReplacesMetadataWithSymmetricLifecycle()
		{
			var owner = new object();
			var nativeElement = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));
			using var registrations = new NativeElementRegistrationSet();

			registrations.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);
			registrations.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);
			registrations.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.RealizedView);

			Assert.Collection(
				events,
				diagnosticEvent => Assert.Equal(NativeElementDiagnostics.RegisteredEventName, diagnosticEvent.Key),
				diagnosticEvent => Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, diagnosticEvent.Key),
				diagnosticEvent => Assert.Equal(NativeElementDiagnostics.RegisteredEventName, diagnosticEvent.Key));

			registrations.Clear();
			registrations.Clear();

			Assert.Equal(4, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[3].Key);
		}

		[Fact]
		public void RegistrationSetRetainsOnlyExactNativeObjects()
		{
			var owner = new object();
			var retained = new object();
			var removed = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));
			using var registrations = new NativeElementRegistrationSet();
			registrations.Register(owner, retained, NativeElementRoles.ToolbarItem);
			registrations.Register(owner, removed, NativeElementRoles.ToolbarItem);

			registrations.Retain(new[] { retained });

			Assert.Equal(3, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[2].Key);
			var payload = Assert.IsType<object[]>(events[2].Value);
			Assert.Same(removed, payload[1]);
		}

		[Fact]
		public void RegistrationSetClearInvalidatesPendingLifecycle()
		{
			using var registrations = new NativeElementRegistrationSet();
			var lifecycleEpoch = registrations.LifecycleEpoch;

			registrations.Clear();

			Assert.False(registrations.IsCurrent(lifecycleEpoch));
		}

		[Fact]
		public void RegistrationSetAdvanceLifecyclePreservesRegistrations()
		{
			var owner = new object();
			var nativeElement = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));
			using var registrations = new NativeElementRegistrationSet();
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);
			var lifecycleEpoch = registrations.LifecycleEpoch;

			registrations.AdvanceLifecycle();

			Assert.False(registrations.IsCurrent(lifecycleEpoch));
			Assert.Single(events);

			registrations.Clear();

			Assert.Equal(2, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[1].Key);
		}

		sealed class RecordingObserver : IObserver<KeyValuePair<string, object>>
		{
			readonly List<KeyValuePair<string, object>> _events;

			public RecordingObserver(List<KeyValuePair<string, object>> events)
			{
				_events = events;
			}

			public void OnCompleted()
			{
			}

			public void OnError(Exception error)
			{
			}

			public void OnNext(KeyValuePair<string, object> value)
			{
				_events.Add(value);
			}
		}

		sealed class ThrowingObserver : IObserver<KeyValuePair<string, object>>
		{
			public void OnCompleted()
			{
			}

			public void OnError(Exception error)
			{
			}

			public void OnNext(KeyValuePair<string, object> value)
			{
				throw new InvalidOperationException("Observer failure");
			}
		}
	}
}
