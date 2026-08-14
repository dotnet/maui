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
					var registrationEnabled = NativeElementDiagnostics.IsRegistrationEnabled;
					if (existing.IsActive && registrationEnabled)
					{
						existing.SubscriptionEpoch = NativeElementDiagnostics.ReplayRegistered(
							owner,
							nativeElement,
							role,
							discriminator,
							existing.SubscriptionEpoch);
						return;
					}

					if (existing.IsActive == registrationEnabled)
						return;

					existing.Dispose();
					existing.Token = NativeElementDiagnostics.TryRegister(
						owner,
						nativeElement,
						role,
						discriminator,
						out var token,
						out var currentSubscriptionEpoch)
							? token
							: null;
					existing.SubscriptionEpoch = currentSubscriptionEpoch;
					return;
				}

				existing.Dispose();
			}

			_registrations[nativeElement] = new Registration(
				owner,
				nativeElement,
				role,
				discriminator,
				NativeElementDiagnostics.TryRegister(
					owner,
					nativeElement,
					role,
					discriminator,
					out var registration,
					out var subscriptionEpoch)
						? registration
						: null,
				subscriptionEpoch);
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
			foreach (var registration in _registrations.Values)
				registration.Dispose();

			_registrations.Clear();
		}

		public void Dispose()
		{
			Clear();
		}

		sealed class Registration
		{
			public Registration(
				object owner,
				object nativeElement,
				string role,
				string? discriminator,
				IDisposable? token,
				long subscriptionEpoch)
			{
				Owner = owner;
				NativeElement = nativeElement;
				Role = role;
				Discriminator = discriminator;
				Token = token;
				SubscriptionEpoch = subscriptionEpoch;
			}

			public object Owner { get; }
			public object NativeElement { get; }
			public string Role { get; }
			public string? Discriminator { get; }
			public bool IsActive => Token is not null;
			public IDisposable? Token { get; set; }
			public long SubscriptionEpoch { get; set; }

			public void Dispose()
			{
				if (Token is not null)
				{
					SubscriptionEpoch = NativeElementDiagnostics.ReplayRegisteredAndDispose(
						Owner,
						NativeElement,
						Role,
						Discriminator,
						SubscriptionEpoch,
						Token);
					Token = null;
					return;
				}

				Token = null;
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
