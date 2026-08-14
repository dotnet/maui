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
		public void RegistrationSetValidatesDisabledRegistrations()
		{
			using var registrations = new NativeElementRegistrationSet();

			Assert.Throws<ArgumentNullException>(() => registrations.Register(
				null!,
				new object(),
				NativeElementRoles.ToolbarItem));
			Assert.Throws<ArgumentNullException>(() => registrations.Register(
				new object(),
				null!,
				NativeElementRoles.ToolbarItem));
			Assert.Throws<ArgumentException>(() => registrations.Register(
				new object(),
				new object(),
				" "));
		}

		[Fact]
		public void RegistrationSetInvalidReplacementDoesNotMutateExistingRegistration()
		{
			var owner = new object();
			var nativeElement = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));
			using var registrations = new NativeElementRegistrationSet();
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			Assert.Throws<ArgumentException>(() => registrations.Register(owner, nativeElement, " "));
			Assert.Single(events);

			registrations.Clear();

			Assert.Equal(2, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[1].Key);
		}

		[Fact]
		public void RegistrationSetInvalidExclusiveReplacementDoesNotMutateExistingRegistrations()
		{
			var owner = new object();
			var firstElement = new object();
			var secondElement = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));
			using var registrations = new NativeElementRegistrationSet();
			registrations.Register(
				owner,
				firstElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);
			registrations.Register(
				owner,
				secondElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);

			Assert.Throws<ArgumentException>(() => registrations.RegisterExclusive(
				owner,
				new object(),
				" ",
				NativeElementDiscriminators.LogicalModel));
			Assert.Equal(2, events.Count);

			registrations.Clear();

			Assert.Equal(4, events.Count);
		}

		[Fact]
		public void RegistrationSetPromotesDisabledRegistrationWhenListenerAttaches()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			registrations.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));

			registrations.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);

			var registered = Assert.Single(events);
			Assert.Equal(NativeElementDiagnostics.RegisteredEventName, registered.Key);

			registrations.Clear();

			Assert.Equal(2, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[1].Key);
		}

		[Fact]
		public void RegistrationSetPromotesLatestDisabledMetadataWhenListenerAttaches()
		{
			var owner = new object();
			var nativeElement = new object();
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
				NativeElementDiscriminators.RealizedView);
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));

			registrations.Register(
				owner,
				nativeElement,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.RealizedView);

			var registered = Assert.Single(events);
			var payload = Assert.IsType<object[]>(registered.Value);
			Assert.Equal(NativeElementDiscriminators.RealizedView, payload[4]);
		}

		[Fact]
		public void RegistrationSetReplaysAfterDisabledRefresh()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			var firstEvents = new List<KeyValuePair<string, object>>();
			using (NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(firstEvents)))
			{
				registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);
			}

			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			var secondEvents = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(secondEvents));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			Assert.Single(firstEvents);
			Assert.Single(secondEvents);
			Assert.Equal(NativeElementDiagnostics.RegisteredEventName, secondEvents[0].Key);
		}

		[Fact]
		public void RegistrationSetReplaysForReplacementObserver()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			var firstEvents = new List<KeyValuePair<string, object>>();
			using (NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(firstEvents)))
			{
				registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);
			}

			var secondEvents = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(secondEvents));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			var registered = Assert.Single(secondEvents);
			Assert.Equal(NativeElementDiagnostics.RegisteredEventName, registered.Key);
		}

		[Fact]
		public void RegistrationSetReplaysOnlyForOverlappingNewObserver()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			var firstEvents = new List<KeyValuePair<string, object>>();
			using var firstSubscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(firstEvents));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			var secondEvents = new List<KeyValuePair<string, object>>();
			using var secondSubscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(secondEvents));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			Assert.Single(firstEvents);
			Assert.Single(secondEvents);
			Assert.Equal(NativeElementDiagnostics.RegisteredEventName, secondEvents[0].Key);

			registrations.Clear();

			Assert.Equal(2, firstEvents.Count);
			Assert.Equal(2, secondEvents.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, firstEvents[1].Key);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, secondEvents[1].Key);
		}

		[Fact]
		public void RegistrationSetReplayMatchesWriteSemanticsForFilteredObserver()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			var firstEvents = new List<KeyValuePair<string, object>>();
			using var firstSubscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(firstEvents));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			var filteredEvents = new List<KeyValuePair<string, object>>();
			using var filteredSubscription = NativeElementDiagnostics.Listener.Subscribe(
				new RecordingObserver(filteredEvents),
				(_, _, _) => false);
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			Assert.Single(filteredEvents);
			Assert.Equal(NativeElementDiagnostics.RegisteredEventName, filteredEvents[0].Key);
		}

		[Fact]
		public void RegistrationSetDoesNotReplayToDisposedObserver()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			var firstEvents = new List<KeyValuePair<string, object>>();
			using var firstSubscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(firstEvents));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			var disposedEvents = new List<KeyValuePair<string, object>>();
			NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(disposedEvents)).Dispose();
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			Assert.Empty(disposedEvents);
		}

		[Fact]
		public void RegistrationSetDrainsObserversAddedDuringReplay()
		{
			var owner = new object();
			var nativeElement = new object();
			using var registrations = new NativeElementRegistrationSet();
			using var firstSubscription = NativeElementDiagnostics.Listener.Subscribe(
				new RecordingObserver(new List<KeyValuePair<string, object>>()));
			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			var secondEvents = new List<KeyValuePair<string, object>>();
			var thirdEvents = new List<KeyValuePair<string, object>>();
			IDisposable thirdSubscription = null;
			using var secondSubscription = NativeElementDiagnostics.Listener.Subscribe(
				new CallbackObserver(diagnosticEvent =>
				{
					secondEvents.Add(diagnosticEvent);
					if (diagnosticEvent.Key == NativeElementDiagnostics.RegisteredEventName &&
						thirdSubscription is null)
					{
						thirdSubscription = NativeElementDiagnostics.Listener.Subscribe(
							new RecordingObserver(thirdEvents));
					}
				}));

			registrations.Register(owner, nativeElement, NativeElementRoles.ToolbarItem);

			Assert.Single(secondEvents);
			Assert.Single(thirdEvents);
			Assert.Equal(NativeElementDiagnostics.RegisteredEventName, thirdEvents[0].Key);

			registrations.Clear();

			Assert.Equal(2, secondEvents.Count);
			Assert.Equal(2, thirdEvents.Count);
			thirdSubscription?.Dispose();
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
		public void RegistrationSetUnregistersAllMatchingDiscriminators()
		{
			var firstOwner = new object();
			var secondOwner = new object();
			var firstRealized = new object();
			var secondRealized = new object();
			var logical = new object();
			var events = new List<KeyValuePair<string, object>>();
			using var subscription = NativeElementDiagnostics.Listener.Subscribe(new RecordingObserver(events));
			using var registrations = new NativeElementRegistrationSet();
			registrations.Register(
				firstOwner,
				firstRealized,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.RealizedView);
			registrations.Register(
				secondOwner,
				secondRealized,
				NativeElementRoles.ToolbarOverflow,
				NativeElementDiscriminators.RealizedView);
			registrations.Register(
				firstOwner,
				logical,
				NativeElementRoles.ToolbarItem,
				NativeElementDiscriminators.LogicalModel);

			registrations.UnregisterDiscriminator(NativeElementDiscriminators.RealizedView);

			Assert.Equal(5, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[3].Key);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[4].Key);

			registrations.Clear();

			Assert.Equal(6, events.Count);
			Assert.Equal(NativeElementDiagnostics.UnregisteredEventName, events[5].Key);
			var payload = Assert.IsType<object[]>(events[5].Value);
			Assert.Same(logical, payload[1]);
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

		sealed class CallbackObserver : IObserver<KeyValuePair<string, object>>
		{
			readonly Action<KeyValuePair<string, object>> _onNext;

			public CallbackObserver(Action<KeyValuePair<string, object>> onNext)
			{
				_onNext = onNext;
			}

			public void OnCompleted()
			{
			}

			public void OnError(Exception error)
			{
			}

			public void OnNext(KeyValuePair<string, object> value)
			{
				_onNext(value);
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
