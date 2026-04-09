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
	partial class WebAuthenticatorImplementation : IWebAuthenticator
	{
		internal static bool OnAppInstanceActivated(UI.Xaml.Application application, AppActivationArguments args)
		{
			if (args is null || args.Kind != ExtendedActivationKind.Protocol)
				return false;

			if (args.Data is not IProtocolActivatedEventArgs protocolArgs)
				return false;

			if (!OAuth2Manager.CompleteAuthRequest(protocolArgs.Uri))
				return false;

			// When the protocol callback launches a transient helper instance, complete the auth request
			// and immediately exit before the app finishes booting into a headless background process.
			if (WindowStateManager.Default.GetActiveWindow() is null)
				System.Diagnostics.Process.GetCurrentProcess().Kill();

			return true;
		}

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> await AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None).ConfigureAwait(false);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			ArgumentNullException.ThrowIfNull(webAuthenticatorOptions);

			var url = webAuthenticatorOptions.Url ?? throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
			var callbackUrl = webAuthenticatorOptions.CallbackUrl ?? throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));

			bool isPackaged = AppInfoUtils.IsPackagedApp;

			if (isPackaged)
			{
				if (!IsUriProtocolDeclared(callbackUrl.Scheme))
					throw new InvalidOperationException($"You need to declare the windows.protocol usage of the protocol/scheme `{callbackUrl.Scheme}` in your AppxManifest.xml file");
			}
			else
			{
				if (callbackUrl.Scheme == "http" || callbackUrl.Scheme == "https")
					throw new InvalidOperationException($"{callbackUrl.Scheme}:// schemes are not allowed for callbackUri. Use a custom scheme like 'myapp' instead.");

				var value = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(callbackUrl.Scheme);
				if (value is null || value.GetValue("URL Protocol") is null)
				{
					throw new InvalidOperationException($"The URI Scheme '{callbackUrl.Scheme}' is not registered. Call ActivationRegistrationManager.RegisterForProtocolActivation to register protocol activation.");
				}
			}
			AuthRequestParams authRequestParams = AuthRequestParams.CreateForAuthorizationCodeRequest("", callbackUrl);

			var windowId = WindowStateManager.Default.GetActiveAppWindow(false)?.Id;
			if (!windowId.HasValue)
				throw new InvalidOperationException("No active window found for authentication.");

			AuthRequestResult authRequestResult = await OAuth2Manager
				.RequestAuthWithParamsAsync(windowId.Value, url, authRequestParams)
				.AsTask(cancellationToken)
				.ConfigureAwait(false);
			if (authRequestResult.Failure is not null)
			{
				var message = string.IsNullOrEmpty(authRequestResult.Failure.ErrorDescription)
					? authRequestResult.Failure.Error
					: $"{authRequestResult.Failure.Error}: {authRequestResult.Failure.ErrorDescription}";

				if (IsUserCancellation(authRequestResult.Failure.Error, authRequestResult.Failure.ErrorDescription))
					throw new TaskCanceledException(message);

				throw new InvalidOperationException(message);
			}

			return new WebAuthenticatorResult(authRequestResult.ResponseUri, webAuthenticatorOptions.ResponseDecoder);
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

			// Check if the protocol was declared
			var root = doc.Root ?? throw new InvalidOperationException("The app manifest could not be loaded.");
			var decl = root.XPathSelectElements($"//uap:Extension[@Category='windows.protocol']/uap:Protocol[@Name='{scheme}']", namespaceManager);

			return decl != null && decl.Any();
		}

	}
}
