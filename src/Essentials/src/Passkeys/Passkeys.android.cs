#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using AndroidX.Credentials;
using Java.Util.Concurrent;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
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
			var request = new CreatePublicKeyCredentialRequest(
				options.ToString(),
				clientDataHash: null,
				preferImmediatelyAvailableCredentials: options.PreferImmediatelyAvailable);

			var result = await InvokeAsync(
				(signal, executor, callback) => manager.CreateCredentialAsync(activity, request, signal, executor, callback),
				cancellationToken).ConfigureAwait(false);

			var response = result.JavaCast<CreatePublicKeyCredentialResponse>()
				?? throw new PasskeyException("The credential provider did not return a passkey registration response.");

			return new PasskeyCreationResponse(response.RegistrationResponseJson!);
		}

		public async Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(options);
			EnsureSupported();

			var activity = Platform.CurrentActivity
				?? throw new InvalidOperationException("Passkeys require a current Activity.");

			var manager = CredentialManager.Create(activity);
			var option = new GetPublicKeyCredentialOption(options.ToString());
			var request = new GetCredentialRequest.Builder()
				.AddCredentialOption(option)
				.SetPreferImmediatelyAvailableCredentials(options.PreferImmediatelyAvailable)
				.Build();

			var result = await InvokeAsync(
				(signal, executor, callback) => manager.GetCredentialAsync(activity, request, signal, executor, callback),
				cancellationToken).ConfigureAwait(false);

			var response = result.JavaCast<GetCredentialResponse>()
				?? throw new PasskeyException("The credential provider did not return a sign-in response.");

			var credential = response.Credential.JavaCast<PublicKeyCredential>()
				?? throw new PasskeyException("The returned credential was not a passkey.");

			return new PasskeyAssertionResponse(credential.AuthenticationResponseJson!);
		}

		void EnsureSupported()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException("Passkeys require Android 14 (API 34) or later.");
		}

		static Task<Java.Lang.Object> InvokeAsync(
			Action<CancellationSignal, IExecutor, ICredentialManagerCallback> start,
			CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<Java.Lang.Object>();
			var signal = new CancellationSignal();
			var executor = Executors.NewSingleThreadExecutor()!;
			var callback = new CredentialManagerCallback(tcs);

			var registration = cancellationToken.Register(() =>
			{
				signal.Cancel();
				tcs.TrySetCanceled(cancellationToken);
			});

			try
			{
				start(signal, executor, callback);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(MapException(ex));
			}

			return tcs.Task.ContinueWith(t =>
			{
				registration.Dispose();
				executor.Shutdown();
				return t;
			}, TaskScheduler.Default).Unwrap();
		}

		internal static Exception MapException(Exception ex)
		{
			// A synchronous failure from the Credential Manager call.
			if (ex is System.OperationCanceledException)
				return new TaskCanceledException();

			if (ex.GetType().Name.Contains("Cancellation", StringComparison.Ordinal))
				return new TaskCanceledException();

			return new PasskeyException(ex.Message, ex);
		}

		static bool IsCancellation(string typeName)
			=> typeName.Contains("Cancellation", StringComparison.Ordinal);

		sealed class CredentialManagerCallback : Java.Lang.Object, ICredentialManagerCallback
		{
			readonly TaskCompletionSource<Java.Lang.Object> _tcs;

			public CredentialManagerCallback(TaskCompletionSource<Java.Lang.Object> tcs) => _tcs = tcs;

			public void OnResult(Java.Lang.Object? result)
			{
				if (result is null)
					_tcs.TrySetException(new PasskeyException("The credential provider returned no result."));
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
					_tcs.TrySetException(new PasskeyException(e?.ToString() ?? "The passkey operation failed."));
			}
		}
	}
}
