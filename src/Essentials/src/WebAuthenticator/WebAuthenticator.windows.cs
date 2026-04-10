#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Microsoft.Security.Authentication.OAuth;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
		// TCS for receiving protocol activation callbacks.
		// Used by both OAuth2Manager flows (where it races against RequestAuthWithParamsAsync)
		// and server-brokered flows (where it is the sole completion mechanism).
		TaskCompletionSource<WebAuthenticatorResult>? tcsResponse;
		Uri? currentRedirectUri;
		WebAuthenticatorOptions? currentOptions;

		/// <inheritdoc />
		public bool OnAppInstanceActivatedCallback(AppActivationArguments args)
		{
			if (args is null || args.Kind != ExtendedActivationKind.Protocol)
			{
				System.Diagnostics.Debug.WriteLine($"[WebAuthenticator] OnAppInstanceActivatedCallback: skipped (Kind={args?.Kind})");
				return false;
			}

			if (args.Data is not IProtocolActivatedEventArgs protocolArgs)
			{
				System.Diagnostics.Debug.WriteLine("[WebAuthenticator] OnAppInstanceActivatedCallback: not IProtocolActivatedEventArgs");
				return false;
			}

			var uri = protocolArgs.Uri;
			System.Diagnostics.Debug.WriteLine($"[WebAuthenticator] OnAppInstanceActivatedCallback: URI={uri}");

			// First, try OAuth2Manager — completes standard authorization code + PKCE flows.
			if (OAuth2Manager.CompleteAuthRequest(uri))
			{
				System.Diagnostics.Debug.WriteLine("[WebAuthenticator] CompleteAuthRequest succeeded (OAuth2Manager flow)");
				// Also complete TCS so the racing Task.WhenAny in AuthenticateAsync unblocks.
				// Use TrySetResult since it may already be completed or cancelled.
				var callbackUri = new Uri(uri.ToString());
				tcsResponse?.TrySetResult(new WebAuthenticatorResult(callbackUri, currentOptions?.ResponseDecoder));
				return true;
			}

			// Fallback: complete the TCS for server-brokered flows that return tokens directly.
			if (tcsResponse is not null && !(tcsResponse.Task?.IsCompleted ?? true))
			{
				try
				{
					var callbackUri = new Uri(uri.ToString());

					if (currentRedirectUri is not null && !WebUtils.CanHandleCallback(currentRedirectUri, callbackUri))
					{
						System.Diagnostics.Debug.WriteLine($"[WebAuthenticator] TCS fallback: scheme mismatch (expected {currentRedirectUri.Scheme})");
						return false;
					}

					System.Diagnostics.Debug.WriteLine("[WebAuthenticator] TCS fallback: completing auth result");
					tcsResponse.TrySetResult(new WebAuthenticatorResult(callbackUri, currentOptions?.ResponseDecoder));
					return true;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[WebAuthenticator] TCS fallback exception: {ex}");
					tcsResponse.TrySetException(ex);
					return true;
				}
			}

			System.Diagnostics.Debug.WriteLine("[WebAuthenticator] OnAppInstanceActivatedCallback: no handler matched");
			return false;
		}

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
		=> await AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None).ConfigureAwait(false);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			ArgumentNullException.ThrowIfNull(webAuthenticatorOptions);

			var url = webAuthenticatorOptions.Url ?? throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
			var callbackUrl = webAuthenticatorOptions.CallbackUrl ?? throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));

			if (AppInfoUtils.IsPackagedApp)
			{
				if (!IsUriProtocolDeclared(callbackUrl.Scheme))
				{
					throw new InvalidOperationException(
					$"You need to declare the windows.protocol usage of the " +
					$"protocol/scheme `{callbackUrl.Scheme}` in your AppxManifest.xml file");
				}
			}
			else
			{
				if (callbackUrl.Scheme is "http" or "https")
				{
					throw new InvalidOperationException(
					$"{callbackUrl.Scheme}:// schemes are not allowed for callbackUri. " +
					$"Use a custom scheme like 'myapp' instead.");
				}

				if (!IsRegistryDeclared(callbackUrl.Scheme))
				{
					throw new InvalidOperationException(
					$"The URI Scheme '{callbackUrl.Scheme}' is not registered. " +
					$"Call ActivationRegistrationManager.RegisterForProtocolActivation to register protocol activation.");
				}
			}

			// Set up TCS before launching the browser — protocol activation callbacks will complete it.
			if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
				tcsResponse.TrySetCanceled();

			tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
			currentRedirectUri = callbackUrl;
			currentOptions = webAuthenticatorOptions;

			using (cancellationToken.Register(() => tcsResponse.TrySetCanceled()))
			{
				// Use OAuth2Manager to open the browser and manage the auth flow.
				// OAuth2Manager handles PKCE for direct OAuth2 flows; for server-brokered flows
				// it will fail (the server returns tokens, not an auth code), but the TCS fallback
				// catches the callback via protocol activation.
				// Race both: whichever completes first wins.
				var windowId = WindowStateManager.Default.GetActiveAppWindow(false)?.Id;
				if (windowId.HasValue)
				{
					var authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest("", callbackUrl);
					var oauthTask = OAuth2Manager
					.RequestAuthWithParamsAsync(windowId.Value, url, authRequestParams)
					.AsTask(cancellationToken);

					// Wait for either OAuth2Manager or TCS (protocol activation callback) to complete.
					var completedTask = await Task.WhenAny(oauthTask, tcsResponse.Task).ConfigureAwait(false);

					if (completedTask == tcsResponse.Task)
					{
						// TCS won — server-brokered flow or OAuth2Manager completed it via the callback.
						return await tcsResponse.Task.ConfigureAwait(false);
					}

					// OAuth2Manager finished first — check its result.
					var authRequestResult = await oauthTask.ConfigureAwait(false);
					if (authRequestResult.Response is not null)
					{
						return new WebAuthenticatorResult(authRequestResult.ResponseUri, webAuthenticatorOptions.ResponseDecoder);
					}

					// OAuth2Manager failed — if TCS was completed while we checked, use it.
					if (tcsResponse.Task.IsCompleted)
					{
						return await tcsResponse.Task.ConfigureAwait(false);
					}

					// Report the failure.
					if (authRequestResult.Failure is not null)
					{
						var message = string.IsNullOrEmpty(authRequestResult.Failure.ErrorDescription)
						? authRequestResult.Failure.Error
						: $"{authRequestResult.Failure.Error}: {authRequestResult.Failure.ErrorDescription}";

						if (IsUserCancellation(authRequestResult.Failure.Error, authRequestResult.Failure.ErrorDescription))
							throw new TaskCanceledException(message);

						throw new InvalidOperationException(message);
					}
				}

				// No active window — fall back to Launcher + TCS only.
				var launched = await global::Windows.System.Launcher.LaunchUriAsync(url);
				if (!launched)
					throw new InvalidOperationException("Failed to launch the browser for authentication.");

				return await tcsResponse.Task.ConfigureAwait(false);
			}
		}

		static bool IsUserCancellation(string? error, string? errorDescription) =>
		string.Equals(error, "access_denied", StringComparison.OrdinalIgnoreCase) ||
		(error?.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0) ||
		(errorDescription?.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) >= 0);

		static bool IsUriProtocolDeclared(string scheme)
		{
			var docPath = FileSystemUtils.PlatformGetFullAppPackageFilePath(PlatformUtils.AppManifestFilename);
			var doc = XDocument.Load(docPath, LoadOptions.None);
			var reader = doc.CreateReader();
			var namespaceManager = new XmlNamespaceManager(reader.NameTable);
			namespaceManager.AddNamespace("x", PlatformUtils.AppManifestXmlns);
			namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

			var root = doc.Root ?? throw new InvalidOperationException("The app manifest could not be loaded.");
			var decl = root.XPathSelectElements($"//uap:Extension[@Category='windows.protocol']/uap:Protocol[@Name='{scheme}']", namespaceManager);

			return decl?.Any() == true;
		}

		static bool IsRegistryDeclared(string scheme)
		{
			var value = Win32.Registry.ClassesRoot.OpenSubKey(scheme);

			return value?.GetValue("URL Protocol") is not null;
		}
	}
}
