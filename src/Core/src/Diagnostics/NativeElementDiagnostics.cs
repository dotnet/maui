using System;
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

		static readonly DiagnosticListener s_listener = new DiagnosticListener(ListenerName);

		internal static DiagnosticListener Listener => s_listener;

		internal static bool IsRegistrationEnabled
		{
			get
			{
				try
				{
					return s_listener.IsEnabled(RegisteredEventName);
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
			ValidateRegistrationArguments(owner, nativeElement, role);

			if (!IsRegistrationEnabled)
			{
				registration = null;
				return false;
			}

			registration = new Registration(nativeElement);
			try
			{
				s_listener.Write(
					RegisteredEventName,
					new object?[] { ContractVersion, owner, nativeElement, role, discriminator });
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Native element registration observer failed: {ex}");
			}

			return true;
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
				if (s_listener.IsEnabled(UnregisteredEventName))
					s_listener.Write(
						UnregisteredEventName,
						new object?[] { ContractVersion, nativeElement });
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Native element unregistration observer failed: {ex}");
			}
		}

		sealed class Registration : IDisposable
		{
			object? _nativeElement;

			public Registration(object nativeElement)
			{
				_nativeElement = nativeElement;
			}

			public void Dispose()
			{
				Unregister(Interlocked.Exchange(ref _nativeElement, null));
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
