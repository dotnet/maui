using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Maui.Diagnostics
{
	internal static class NativeElementDiagnostics
	{
		internal const int ContractVersion = 1;
		internal const string ListenerName = "Microsoft.Maui.NativeElements";
		internal static readonly string RegisteredEventName = $"{ListenerName}.Registered.v{ContractVersion}";
		internal static readonly string UnregisteredEventName = $"{ListenerName}.Unregistered.v{ContractVersion}";

		static readonly NativeElementDiagnosticListener s_listener = new NativeElementDiagnosticListener(ListenerName);
		static int s_subscriptionNotificationRequested;
		static int s_subscriptionNotificationWorkerScheduled;

		internal static DiagnosticListener Listener => s_listener;
		internal static long SubscriptionEpoch => s_listener.SubscriptionEpoch;
		internal static event Action? SubscriptionAdded;

		internal static bool IsRegistrationEnabled
		{
			get
			{
				try
				{
					return s_listener.IsRegistrationEnabled;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Native element diagnostics listener check failed: {ex}");
					return false;
				}
			}
		}

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
			Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
		internal static IDisposable Register(
			object owner,
			object nativeElement,
			string role,
			string? discriminator = null)
		{
			return TryRegister(owner, nativeElement, role, discriminator, out var registration)
				? registration
				: EmptyRegistration.Instance;
		}

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
			Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
		internal static bool TryRegister(
			object owner,
			object nativeElement,
			string role,
			string? discriminator,
			[NotNullWhen(true)] out IDisposable? registration)
		{
			return TryRegister(owner, nativeElement, role, discriminator, out registration, out _);
		}

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
			Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
		internal static bool TryRegister(
			object owner,
			object nativeElement,
			string role,
			string? discriminator,
			[NotNullWhen(true)] out IDisposable? registration,
			out long subscriptionEpoch)
		{
			ValidateRegistrationArguments(owner, nativeElement, role);
			if (s_listener.TryWriteRegistered(
				owner,
				nativeElement,
				role,
				discriminator,
				out subscriptionEpoch))
			{
				registration = new Registration(
					owner,
					nativeElement,
					role,
					discriminator,
					subscriptionEpoch);
				return true;
			}

			registration = null;
			return false;
		}

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
			Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
		internal static long ReplayRegistered(
			object owner,
			object nativeElement,
			string role,
			string? discriminator,
			long afterSubscriptionEpoch)
		{
			return s_listener.ReplayRegistered(
				owner,
				nativeElement,
				role,
				discriminator,
				afterSubscriptionEpoch);
		}

		internal static long ReplayRegisteredAndDispose(
			object owner,
			object nativeElement,
			string role,
			string? discriminator,
			long afterSubscriptionEpoch,
			IDisposable registration)
		{
			return s_listener.ReplayRegisteredAndDispose(
				owner,
				nativeElement,
				role,
				discriminator,
				afterSubscriptionEpoch,
				registration);
		}

		internal static void ReplayRegisteredAndUnregister(
			object owner,
			object nativeElement,
			string role,
			string? discriminator,
			long afterSubscriptionEpoch)
		{
			try
			{
				s_listener.ReplayRegisteredAndUnregister(
					owner,
					nativeElement,
					role,
					discriminator,
					afterSubscriptionEpoch);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Native element unregistration observer failed: {ex}");
			}
		}

		internal static void ValidateRegistrationArguments(
			object owner,
			object nativeElement,
			string role)
		{
			if (owner is null)
				throw new ArgumentNullException(nameof(owner));
			if (nativeElement is null)
				throw new ArgumentNullException(nameof(nativeElement));
			if (string.IsNullOrWhiteSpace(role))
				throw new ArgumentException("A native element role is required.", nameof(role));
		}

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
			Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
		internal static void Unregister(object? nativeElement)
		{
			if (nativeElement is null)
				return;

			try
			{
				s_listener.WriteUnregistered(nativeElement);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Native element unregistration observer failed: {ex}");
			}
		}

		sealed class Registration : IDisposable
		{
			object? _owner;
			object? _nativeElement;
			string? _role;
			string? _discriminator;
			readonly long _subscriptionEpoch;

			public Registration(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				long subscriptionEpoch)
			{
				_owner = owner;
				_nativeElement = nativeElement;
				_role = role;
				_discriminator = discriminator;
				_subscriptionEpoch = subscriptionEpoch;
			}

			public void Dispose()
			{
				var nativeElement = Interlocked.Exchange(ref _nativeElement, null);
				if (nativeElement is null)
					return;

				var owner = Interlocked.Exchange(ref _owner, null);
				var role = Interlocked.Exchange(ref _role, null);
				var discriminator = Interlocked.Exchange(ref _discriminator, null);
				if (owner is null || role is null)
					return;

				ReplayRegisteredAndUnregister(
					owner,
					nativeElement,
					role,
					discriminator,
					_subscriptionEpoch);
			}

			public void DisposeWithoutReplay()
			{
				Interlocked.Exchange(ref _nativeElement, null);
				Interlocked.Exchange(ref _owner, null);
				Interlocked.Exchange(ref _role, null);
				Interlocked.Exchange(ref _discriminator, null);
			}
		}

		static void NotifySubscriptionAdded()
		{
			var handlers = SubscriptionAdded;
			if (handlers is null)
				return;

			foreach (Action handler in handlers.GetInvocationList())
			{
				try
				{
					handler();
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Native element subscription callback failed: {ex}");
				}
			}
		}

		static void QueueSubscriptionAddedNotification()
		{
			Volatile.Write(ref s_subscriptionNotificationRequested, 1);
			if (Interlocked.CompareExchange(ref s_subscriptionNotificationWorkerScheduled, 1, 0) != 0)
				return;

			ThreadPool.QueueUserWorkItem(static _ =>
			{
				while (true)
				{
					while (Interlocked.Exchange(ref s_subscriptionNotificationRequested, 0) != 0)
						NotifySubscriptionAdded();

					Interlocked.Exchange(ref s_subscriptionNotificationWorkerScheduled, 0);
					if (Volatile.Read(ref s_subscriptionNotificationRequested) == 0 ||
						Interlocked.CompareExchange(ref s_subscriptionNotificationWorkerScheduled, 1, 0) != 0)
					{
						return;
					}
				}
			});
		}

		sealed class NativeElementDiagnosticListener : DiagnosticListener
		{
			readonly object _subscriptionsLock = new object();
			readonly List<TrackedSubscription> _subscriptions = new List<TrackedSubscription>();
			long _subscriptionEpoch;

			public NativeElementDiagnosticListener(string name)
				: base(name)
			{
			}

			public long SubscriptionEpoch => Interlocked.Read(ref _subscriptionEpoch);
			public bool IsRegistrationEnabled
			{
				get
				{
					return IsEnabledSafely(RegisteredEventName);
				}
			}

			public override IDisposable Subscribe(IObserver<KeyValuePair<string, object?>> observer)
			{
				return Track(observer, () => base.Subscribe(observer));
			}

			public override IDisposable Subscribe(
				IObserver<KeyValuePair<string, object?>> observer,
				Predicate<string>? isEnabled)
			{
				return Track(observer, () => base.Subscribe(observer, isEnabled));
			}

			public override IDisposable Subscribe(
				IObserver<KeyValuePair<string, object?>> observer,
				Func<string, object?, object?, bool>? isEnabled)
			{
				return Track(observer, () => base.Subscribe(observer, isEnabled));
			}

			public override IDisposable Subscribe(
				IObserver<KeyValuePair<string, object?>> observer,
				Func<string, object?, object?, bool>? isEnabled,
				Action<Activity, object?>? onActivityImport,
				Action<Activity, object?>? onActivityExport)
			{
				return Track(
					observer,
					() => base.Subscribe(observer, isEnabled, onActivityImport, onActivityExport));
			}

			TrackedSubscription Track(
				IObserver<KeyValuePair<string, object?>> observer,
				Func<IDisposable> subscribe)
			{
				var deferNotification = Monitor.IsEntered(_subscriptionsLock);
				TrackedSubscription trackedSubscription;
				lock (_subscriptionsLock)
				{
					var subscription = subscribe();
					trackedSubscription = new TrackedSubscription(
						this,
						observer,
						Interlocked.Increment(ref _subscriptionEpoch));
					trackedSubscription.Attach(subscription);
					_subscriptions.Add(trackedSubscription);
				}

				if (deferNotification)
					QueueSubscriptionAddedNotification();
				else
					NotifySubscriptionAdded();

				return trackedSubscription;
			}

			[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
				Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
			public bool TryWriteRegistered(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				out long subscriptionEpoch)
			{
				TrackedSubscription[] subscriptions;
				lock (_subscriptionsLock)
				{
					if (!IsEnabledSafely(RegisteredEventName))
					{
						subscriptionEpoch = _subscriptionEpoch;
						return false;
					}

					subscriptions = _subscriptions.ToArray();
					subscriptionEpoch = _subscriptionEpoch;
				}

				WriteCore(
					subscriptions,
					RegisteredEventName,
					new object?[] { ContractVersion, owner, nativeElement, role, discriminator });
				return true;
			}

			public long ReplayRegistered(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				long afterSubscriptionEpoch)
			{
				return ReplayRegisteredCore(
					owner,
					nativeElement,
					role,
					discriminator,
					afterSubscriptionEpoch);
			}

			public long ReplayRegisteredAndDispose(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				long afterSubscriptionEpoch,
				IDisposable registration)
			{
				var subscriptionEpoch = ReplayRegisteredAndUnregisterCore(
					owner,
					nativeElement,
					role,
					discriminator,
					afterSubscriptionEpoch);
				if (registration is Registration nativeRegistration)
					nativeRegistration.DisposeWithoutReplay();
				else
					registration.Dispose();
				return subscriptionEpoch;
			}

			[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
				Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
			public void ReplayRegisteredAndUnregister(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				long afterSubscriptionEpoch)
			{
				ReplayRegisteredAndUnregisterCore(
					owner,
					nativeElement,
					role,
					discriminator,
					afterSubscriptionEpoch);
			}

			[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
				Justification = "The fixed object-array payload is consumed by index and does not require reflected members.")]
			public void WriteUnregistered(object nativeElement)
			{
				TrackedSubscription[] subscriptions;
				lock (_subscriptionsLock)
					subscriptions = _subscriptions.ToArray();
				WriteCore(
					subscriptions,
					UnregisteredEventName,
					new object?[] { ContractVersion, nativeElement });
			}

			static void WriteCore(
				IReadOnlyList<TrackedSubscription> subscriptions,
				string eventName,
				object?[] payload)
			{
				if (subscriptions.Count == 0)
					return;

				var diagnosticEvent = new KeyValuePair<string, object?>(eventName, payload);
				foreach (var subscription in subscriptions)
					subscription.Replay(diagnosticEvent);
			}

			long ReplayRegisteredCore(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				long afterSubscriptionEpoch)
			{
				var replayedThroughEpoch = afterSubscriptionEpoch;
				var payload = new object?[] { ContractVersion, owner, nativeElement, role, discriminator };
				var diagnosticEvent = new KeyValuePair<string, object?>(RegisteredEventName, payload);
				while (true)
				{
					long subscriptionEpoch;
					TrackedSubscription[] subscriptions;
					lock (_subscriptionsLock)
					{
						subscriptionEpoch = _subscriptionEpoch;
						if (subscriptionEpoch == replayedThroughEpoch)
							return subscriptionEpoch;

						subscriptions = _subscriptions.FindAll(
							subscription =>
								subscription.Epoch > replayedThroughEpoch &&
								subscription.Epoch <= subscriptionEpoch).ToArray();
					}

					foreach (var subscription in subscriptions)
						subscription.Replay(diagnosticEvent);

					replayedThroughEpoch = subscriptionEpoch;
				}
			}

			long ReplayRegisteredAndUnregisterCore(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				long afterSubscriptionEpoch)
			{
				long subscriptionEpoch;
				TrackedSubscription[] subscriptions;
				TrackedSubscription[] replaySubscriptions;
				lock (_subscriptionsLock)
				{
					subscriptionEpoch = _subscriptionEpoch;
					subscriptions = _subscriptions.ToArray();
					replaySubscriptions = Array.FindAll(
						subscriptions,
						subscription => subscription.Epoch > afterSubscriptionEpoch);
				}

				WriteCore(
					replaySubscriptions,
					RegisteredEventName,
					new object?[] { ContractVersion, owner, nativeElement, role, discriminator });
				WriteCore(
					subscriptions,
					UnregisteredEventName,
					new object?[] { ContractVersion, nativeElement });
				return subscriptionEpoch;
			}

			bool IsEnabledSafely(string eventName)
			{
				try
				{
					return base.IsEnabled(eventName);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Native element diagnostics listener check failed: {ex}");
					return false;
				}
			}

			void SubscriptionDisposed(TrackedSubscription subscription)
			{
				lock (_subscriptionsLock)
					_subscriptions.Remove(subscription);
			}

			sealed class TrackedSubscription : IDisposable
			{
				NativeElementDiagnosticListener? _listener;
				IDisposable? _subscription;
				readonly IObserver<KeyValuePair<string, object?>> _observer;
				readonly object _gate = new object();
				bool _disposed;

				public TrackedSubscription(
					NativeElementDiagnosticListener listener,
					IObserver<KeyValuePair<string, object?>> observer,
					long epoch)
				{
					_listener = listener;
					_observer = observer;
					Epoch = epoch;
				}

				public long Epoch { get; }

				public void Attach(IDisposable subscription)
				{
					lock (_gate)
					{
						if (!_disposed)
						{
							_subscription = subscription;
							return;
						}
					}

					subscription.Dispose();
				}

				public void Replay(KeyValuePair<string, object?> diagnosticEvent)
				{
					lock (_gate)
					{
						if (_disposed)
							return;

						try
						{
							_observer.OnNext(diagnosticEvent);
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"Native element registration observer failed: {ex}");
						}
					}
				}

				public void Dispose()
				{
					NativeElementDiagnosticListener? listener;
					IDisposable? subscription;
					lock (_gate)
					{
						if (_disposed)
							return;

						_disposed = true;
						listener = _listener;
						_listener = null;
						subscription = _subscription;
						_subscription = null;
					}

					listener?.SubscriptionDisposed(this);
					subscription?.Dispose();
				}
			}
		}

		sealed class EmptyRegistration : IDisposable
		{
			internal static EmptyRegistration Instance { get; } = new EmptyRegistration();

			public void Dispose()
			{
			}
		}
	}
}
