#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.Content;
using AndroidX.Credentials;
using Java.Util.Concurrent;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication;

partial class PasskeysImplementation : IPasskeys
{
	// Passkey credentials are handled natively by the platform Credential Manager on Android 14+ (API 34).
	public bool IsSupported => OperatingSystem.IsAndroidVersionAtLeast(34);

	public async Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		EnsureSupported();

		var activity = Platform.CurrentActivity
			?? throw new InvalidOperationException("Passkeys require a current Activity.");

		var manager = CredentialManager.Create(activity);
		var executor = ContextCompat.GetMainExecutor(activity)
			?? throw new InvalidOperationException("Unable to acquire the Android main-thread executor.");
		var request = new CreatePublicKeyCredentialRequest(
			options.ToString(),
			clientDataHash: null,
			preferImmediatelyAvailableCredentials: options.PreferImmediatelyAvailable);

		var result = await InvokeAsync(
			(signal, executor, callback) => manager.CreateCredentialAsync(activity, request, signal, executor, callback),
			executor,
			cancellationToken);

		var response = result.JavaCast<CreatePublicKeyCredentialResponse>()
			?? throw new InvalidOperationException("The credential provider did not return a passkey registration response.");

		return new PasskeyCreationResponse(response.RegistrationResponseJson!);
	}

	public async Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		EnsureSupported();

		var activity = Platform.CurrentActivity
			?? throw new InvalidOperationException("Passkeys require a current Activity.");

		var manager = CredentialManager.Create(activity);
		var executor = ContextCompat.GetMainExecutor(activity)
			?? throw new InvalidOperationException("Unable to acquire the Android main-thread executor.");
		var option = new GetPublicKeyCredentialOption(options.ToString());
		var request = new GetCredentialRequest.Builder()
			.AddCredentialOption(option)
			.SetPreferImmediatelyAvailableCredentials(options.PreferImmediatelyAvailable)
			.Build();

		var result = await InvokeAsync(
			(signal, executor, callback) => manager.GetCredentialAsync(activity, request, signal, executor, callback),
			executor,
			cancellationToken);

		var response = result.JavaCast<GetCredentialResponse>()
			?? throw new InvalidOperationException("The credential provider did not return a sign-in response.");

		var credential = response.Credential.JavaCast<PublicKeyCredential>()
			?? throw new InvalidOperationException("The returned credential was not a passkey.");

		return new PasskeyAssertionResponse(credential.AuthenticationResponseJson!);
	}

	void EnsureSupported()
	{
		if (!IsSupported)
			throw new FeatureNotSupportedException("Passkeys require Android 14 (API 34) or later.");
	}

	static async Task<Java.Lang.Object> InvokeAsync(
		Action<CancellationSignal, IExecutor, ICredentialManagerCallback> start,
		IExecutor executor,
		CancellationToken cancellationToken)
	{
		var tcs = new TaskCompletionSource<Java.Lang.Object>(TaskCreationOptions.RunContinuationsAsynchronously);
		var signal = new CancellationSignal();
		var callback = new CredentialManagerCallback(tcs);

		using var registration = cancellationToken.Register(() =>
		{
			signal.Cancel();
			tcs.TrySetCanceled(cancellationToken);
		});

		try
		{
			start(signal, executor, callback);
		}
		catch (Exception ex) when (IsCancellation(ex))
		{
			tcs.TrySetCanceled();
		}
		catch (Exception ex)
		{
			tcs.TrySetException(new InvalidOperationException(ex.Message, ex));
		}

		return await tcs.Task;
	}

	// A synchronous failure from the Credential Manager call is a .NET exception; the Java
	// *CancellationException surfaces here as an OperationCanceledException or a type whose name
	// contains "Cancellation". Fully qualify to avoid the Android.OS.OperationCanceledException clash.
	static bool IsCancellation(Exception ex)
		=> ex is System.OperationCanceledException || IsCancellation(ex.GetType().Name);

	static bool IsCancellation(string typeName)
		=> typeName.Contains("Cancellation", StringComparison.Ordinal);

	sealed class CredentialManagerCallback : Java.Lang.Object, ICredentialManagerCallback
	{
		readonly TaskCompletionSource<Java.Lang.Object> _tcs;

		public CredentialManagerCallback(TaskCompletionSource<Java.Lang.Object> tcs) => _tcs = tcs;

		public void OnResult(Java.Lang.Object? result)
		{
			if (result is null)
				_tcs.TrySetException(new InvalidOperationException("The credential provider returned no result."));
			else
				_tcs.TrySetResult(result);
		}

		public void OnError(Java.Lang.Object? e)
		{
			// e is a bound AndroidX.Credentials.Exceptions.* object (a Java throwable), not a .NET Exception.
			var typeName = e?.GetType().Name ?? string.Empty;

			if (IsCancellation(typeName))
				_tcs.TrySetCanceled();
			else
				_tcs.TrySetException(new InvalidOperationException(e?.ToString() ?? "The passkey operation failed."));
		}
	}
}
