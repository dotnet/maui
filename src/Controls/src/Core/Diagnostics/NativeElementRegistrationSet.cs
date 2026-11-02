using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Maui.Diagnostics;

namespace Microsoft.Maui.Controls.Diagnostics
{
	internal sealed class NativeElementRegistrationSet : IDisposable
	{
		readonly Dictionary<object, Registration> _registrations =
			new Dictionary<object, Registration>(ReferenceComparer.Instance);
		long _lifecycleEpoch;

		public long LifecycleEpoch => Interlocked.Read(ref _lifecycleEpoch);

		public bool HasRegistrations => _registrations.Count > 0;

		public bool IsCurrent(long lifecycleEpoch) => LifecycleEpoch == lifecycleEpoch;

		public long AdvanceLifecycle() => Interlocked.Increment(ref _lifecycleEpoch);

		public void Register(
			object owner,
			object nativeElement,
			string role,
			string? discriminator = null)
		{
			NativeElementDiagnostics.ValidateRegistrationArguments(owner, nativeElement, role);

			if (_registrations.TryGetValue(nativeElement, out var existing))
			{
				if (ReferenceEquals(existing.Owner, owner) &&
					string.Equals(existing.Role, role, StringComparison.Ordinal) &&
					string.Equals(existing.Discriminator, discriminator, StringComparison.Ordinal))
				{
					existing.Refresh();
					return;
				}

				existing.Dispose();
			}

			var registration = new Registration(
				owner,
				nativeElement,
				role,
				discriminator);
			_registrations[nativeElement] = registration;
			registration.Refresh();
		}

		public void RegisterExclusive(
			object owner,
			object nativeElement,
			string role,
			string? discriminator = null)
		{
			NativeElementDiagnostics.ValidateRegistrationArguments(owner, nativeElement, role);

			List<object>? replacedElements = null;
			foreach (var registration in _registrations)
			{
				if (!ReferenceEquals(registration.Key, nativeElement) &&
					ReferenceEquals(registration.Value.Owner, owner) &&
					string.Equals(registration.Value.Discriminator, discriminator, StringComparison.Ordinal))
				{
					(replacedElements ??= new List<object>()).Add(registration.Key);
				}
			}

			if (replacedElements is not null)
			{
				foreach (var replacedElement in replacedElements)
					Unregister(replacedElement);
			}

			Register(owner, nativeElement, role, discriminator);
		}

		public void Unregister(object? nativeElement)
		{
			if (nativeElement is null || !_registrations.TryGetValue(nativeElement, out var registration))
				return;

			_registrations.Remove(nativeElement);
			registration.Dispose();
		}

		public void UnregisterOwner(object? owner)
		{
			UnregisterOwner(owner, null, matchDiscriminator: false);
		}

		public void UnregisterOwner(object? owner, string discriminator)
		{
			UnregisterOwner(owner, discriminator, matchDiscriminator: true);
		}

		public void UnregisterDiscriminator(string discriminator)
		{
			if (_registrations.Count == 0)
				return;

			List<object>? nativeElements = null;
			foreach (var registration in _registrations)
			{
				if (string.Equals(
					registration.Value.Discriminator,
					discriminator,
					StringComparison.Ordinal))
				{
					(nativeElements ??= new List<object>()).Add(registration.Key);
				}
			}

			if (nativeElements is null)
				return;

			foreach (var nativeElement in nativeElements)
				Unregister(nativeElement);
		}

		public void Retain(IEnumerable<object> nativeElements)
		{
			var retainedElements = new HashSet<object>(nativeElements, ReferenceComparer.Instance);
			List<object>? removedElements = null;
			foreach (var nativeElement in _registrations.Keys)
			{
				if (!retainedElements.Contains(nativeElement))
					(removedElements ??= new List<object>()).Add(nativeElement);
			}

			if (removedElements is null)
				return;

			foreach (var removedElement in removedElements)
				Unregister(removedElement);
		}

		void UnregisterOwner(object? owner, string? discriminator, bool matchDiscriminator)
		{
			if (owner is null || _registrations.Count == 0)
				return;

			List<object>? nativeElements = null;
			foreach (var registration in _registrations)
			{
				if (ReferenceEquals(registration.Value.Owner, owner) &&
					(!matchDiscriminator ||
						string.Equals(registration.Value.Discriminator, discriminator, StringComparison.Ordinal)))
				{
					(nativeElements ??= new List<object>()).Add(registration.Key);
				}
			}

			if (nativeElements is null)
				return;

			foreach (var nativeElement in nativeElements)
				Unregister(nativeElement);
		}

		public void Clear()
		{
			AdvanceLifecycle();
			var registrations = new List<Registration>(_registrations.Values);
			_registrations.Clear();
			foreach (var registration in registrations)
				registration.Dispose();
		}

		public void Dispose()
		{
			Clear();
		}

		sealed class Registration
		{
			readonly object _gate = new object();
			IDisposable? _token;
			long _subscriptionEpoch;
			bool _disposed;
			bool _disposeRequested;
			bool _refreshing;
			bool _refreshRequested;
			int _operationThreadId;

			public Registration(
				object owner,
				object nativeElement,
				string role,
				string? discriminator)
			{
				Owner = owner;
				NativeElement = nativeElement;
				Role = role;
				Discriminator = discriminator;
				_subscriptionEpoch = NativeElementDiagnostics.SubscriptionEpoch;
				NativeElementRegistrationTracker.Track(this);
			}

			public object Owner { get; }
			public object NativeElement { get; }
			public string Role { get; }
			public string? Discriminator { get; }

			public void Refresh()
			{
				var currentThreadId = Environment.CurrentManagedThreadId;
				lock (_gate)
				{
					if (_disposed || _disposeRequested)
						return;

					if (_refreshing)
					{
						if (_operationThreadId == currentThreadId)
						{
							_refreshRequested = true;
							return;
						}

						do
						{
							Monitor.Wait(_gate);
						}
						while (_refreshing);

						if (_disposed || _disposeRequested)
							return;
					}

					_refreshing = true;
					_operationThreadId = currentThreadId;
				}

				ProcessPendingOperations();
			}

			void ProcessPendingOperations()
			{
				var disposed = false;
				try
				{
					while (true)
					{
						bool dispose;
						lock (_gate)
						{
							dispose = _disposeRequested;
							_refreshRequested = false;
						}

						if (dispose)
						{
							DisposeToken();
							disposed = true;
							return;
						}

						RefreshCore();
						lock (_gate)
						{
							if (_disposeRequested || _refreshRequested)
								continue;

							return;
						}
					}
				}
				finally
				{
					lock (_gate)
					{
						_disposed |= disposed;
						_refreshing = false;
						_operationThreadId = 0;
						Monitor.PulseAll(_gate);
					}
				}
			}

			void RefreshCore()
			{
				var registrationEnabled = NativeElementDiagnostics.IsRegistrationEnabled;
				if (_token is not null && registrationEnabled)
				{
					_subscriptionEpoch = NativeElementDiagnostics.ReplayRegistered(
						Owner,
						NativeElement,
						Role,
						Discriminator,
						_subscriptionEpoch);
					return;
				}

				if ((_token is not null) == registrationEnabled)
					return;

				DisposeToken();
				if (!registrationEnabled)
					return;

				NativeElementDiagnostics.TryRegister(
					Owner,
					NativeElement,
					Role,
					Discriminator,
					out var token,
					out var subscriptionEpoch);
				_subscriptionEpoch = subscriptionEpoch;
				if (token is null)
					return;

				_token = token;
			}

			void DisposeToken()
			{
				var token = _token;
				_token = null;
				if (token is null)
					return;

				_subscriptionEpoch = NativeElementDiagnostics.ReplayRegisteredAndDispose(
					Owner,
					NativeElement,
					Role,
					Discriminator,
					_subscriptionEpoch,
					token);
			}

			public void Dispose()
			{
				var currentThreadId = Environment.CurrentManagedThreadId;
				lock (_gate)
				{
					if (_disposed || _disposeRequested)
						return;

					_disposeRequested = true;
					if (_refreshing)
					{
						if (_operationThreadId == currentThreadId)
							return;

						do
						{
							Monitor.Wait(_gate);
						}
						while (_refreshing);

						if (_disposed)
							return;
					}

					_refreshing = true;
					_operationThreadId = currentThreadId;
				}

				ProcessPendingOperations();
			}
		}

		static class NativeElementRegistrationTracker
		{
			static readonly object s_gate = new object();
			static readonly List<WeakReference<Registration>> s_registrations =
				new List<WeakReference<Registration>>();
			static int s_registrationsSinceSweep;

			static NativeElementRegistrationTracker()
			{
				NativeElementDiagnostics.SubscriptionAdded += ReplayRegistrations;
			}

			public static void Track(Registration registration)
			{
				lock (s_gate)
				{
					s_registrations.Add(new WeakReference<Registration>(registration));
					if (++s_registrationsSinceSweep < 64)
						return;

					RemoveExpiredRegistrations();
					s_registrationsSinceSweep = 0;
				}
			}

			static void ReplayRegistrations()
			{
				List<Registration> registrations;
				lock (s_gate)
				{
					registrations = new List<Registration>(s_registrations.Count);
					for (var index = 0; index < s_registrations.Count; index++)
					{
						if (s_registrations[index].TryGetTarget(out var registration))
							registrations.Add(registration);
					}

					RemoveExpiredRegistrations();
					s_registrationsSinceSweep = 0;
				}

				foreach (var registration in registrations)
					registration.Refresh();
			}

			static void RemoveExpiredRegistrations()
			{
				for (var index = s_registrations.Count - 1; index >= 0; index--)
				{
					if (!s_registrations[index].TryGetTarget(out _))
						s_registrations.RemoveAt(index);
				}
			}
		}

		sealed class ReferenceComparer : IEqualityComparer<object>
		{
			internal static ReferenceComparer Instance { get; } = new ReferenceComparer();

			public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

			public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
		}
	}

	internal static class NativeElementRoles
	{
		public const string Toolbar = "Toolbar";
		public const string ToolbarTitle = "ToolbarTitle";
		public const string ToolbarItem = "ToolbarItem";
		public const string ToolbarOverflow = "ToolbarOverflow";
		public const string ShellTab = "ShellTab";
		public const string ShellTabOverflow = "ShellTabOverflow";
		public const string ShellFlyout = "ShellFlyout";
		public const string ShellFlyoutToggle = "ShellFlyoutToggle";
		public const string SearchHandler = "SearchHandler";
		public const string BackButton = "BackButton";
		public const string Dialog = "Dialog";
		public const string DialogAction = "DialogAction";
	}

	internal static class NativeElementDiscriminators
	{
		public const string LogicalModel = "LogicalModel";
		public const string RealizedView = "RealizedView";
		public const string ToolbarContainer = "ToolbarContainer";
		public const string TitleView = "TitleView";
		public const string TabBar = "TabBar";
		public const string TabBarItem = "TabBarItem";
		public const string OverflowRow = "OverflowRow";
	}
}
