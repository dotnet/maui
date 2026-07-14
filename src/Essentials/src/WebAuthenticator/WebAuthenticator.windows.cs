#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Microsoft.UI.Windowing;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
		const string CallbackRouteKeyPrefix = "Microsoft.Maui.WebAuthenticator:";

		readonly object locker = new();

		TaskCompletionSource<WebAuthenticatorResult>? tcsResponse;
		Uri? currentRedirectUri;
		WebAuthenticatorOptions? currentOptions;
		AppWindow? currentAppWindow;

		public bool OnAppInstanceActivatedCallback(AppActivationArguments args)
		{
			if (args.Kind != ExtendedActivationKind.Protocol ||
				args.Data is not IProtocolActivatedEventArgs protocolArgs)
			{
				return false;
			}

			var callbackUri = protocolArgs.Uri;

			TaskCompletionSource<WebAuthenticatorResult>? response;
			Uri? redirectUri;
			WebAuthenticatorOptions? options;
			AppWindow? appWindow;

			bool isLocalCallbackRoute;

			lock (locker)
			{
				response = tcsResponse;
				redirectUri = currentRedirectUri;
				options = currentOptions;
				appWindow = currentAppWindow;

				isLocalCallbackRoute = redirectUri is not null && IsSameCallbackRoute(redirectUri, callbackUri);
			}

			if (response?.Task.IsCompleted == false &&
				redirectUri is not null &&
				WebUtils.CanHandleCallback(redirectUri, callbackUri))
			{
				try
				{
					var result = new WebAuthenticatorResult(callbackUri, options?.ResponseDecoder);

					// Only the callback that completes the request may restore its window.
					if (response.TrySetResult(result))
						TryBringToForeground(appWindow);
				}
				catch (Exception ex)
				{
					response.TrySetException(ex);
				}

				return true;
			}

			// A callback can have the expected scheme and still fail host/path validation.
			// Leave the active route registered so a later valid callback can complete it.
			if (isLocalCallbackRoute)
				return false;

			var routeOwner = FindCallbackRouteOwner(callbackUri);
			if (routeOwner is null)
				return false;

			return RedirectActivationAndExit(routeOwner, args);
		}

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> await AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(webAuthenticatorOptions);

			var url = webAuthenticatorOptions.Url ??
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
			var callbackUrl = webAuthenticatorOptions.CallbackUrl ??
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));

			ValidateCallbackUrl(callbackUrl);

			var response = new TaskCompletionSource<WebAuthenticatorResult>();
			TaskCompletionSource<WebAuthenticatorResult>? previousResponse;

			// Capture the request window so a multi-window app restores the same window
			// after authentication completes.
			var appWindow = TryGetActiveAppWindow();

			lock (locker)
			{
				RegisterCallbackRoute(callbackUrl);

				previousResponse = tcsResponse;
				tcsResponse = response;
				currentRedirectUri = callbackUrl;
				currentOptions = webAuthenticatorOptions;
				currentAppWindow = appWindow;
			}

			previousResponse?.TrySetCanceled();

			using (cancellationToken.Register(() => response.TrySetCanceled()))
			{
				try
				{
					var launched = await global::Windows.System.Launcher.LaunchUriAsync(url);
					if (!launched)
						throw new InvalidOperationException("Failed to launch the browser for authentication.");

					return await response.Task;
				}
				finally
				{
					lock (locker)
					{
						if (ReferenceEquals(tcsResponse, response))
							ClearCurrentAuthentication();
					}
				}
			}
		}

		void ClearCurrentAuthentication()
		{
			// Keep the route for the process lifetime. Windows App SDK cannot re-register an
			// AppInstance after UnregisterKey (microsoft/WindowsAppSDK#4420).
			tcsResponse = null;
			currentRedirectUri = null;
			currentOptions = null;
			currentAppWindow = null;
		}

		static AppWindow? TryGetActiveAppWindow()
		{
			try
			{
				return WindowStateManager.Default.GetActiveAppWindow(false);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to identify the WebAuthenticator window: {ex}");
				return null;
			}
		}

		static void TryBringToForeground(AppWindow? appWindow)
		{
			if (appWindow is null)
				return;

			try
			{
				var windowHandle = UI.Win32Interop.GetWindowFromWindowId(appWindow.Id);
				if (windowHandle == IntPtr.Zero)
				{
					Debug.WriteLine("Unable to retrieve the WebAuthenticator window handle.");
					return;
				}

				// AppWindow APIs are agile. Restore or show without activation so the native
				// call below makes the only best-effort foreground request.
				if (appWindow.Presenter is OverlappedPresenter presenter &&
					presenter.State == OverlappedPresenterState.Minimized)
				{
					presenter.Restore(false);
				}

				if (!appWindow.IsVisible)
					appWindow.Show(false);

				// RedirectActivationToAsync attempts to transfer foreground permission to the
				// route owner, but Windows can still deny this request.
				if (!PlatformMethods.SetForegroundWindow(windowHandle))
					Debug.WriteLine("Windows denied the WebAuthenticator window foreground activation.");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to bring the WebAuthenticator window to the foreground: {ex}");
			}
		}

		static void ValidateCallbackUrl(Uri callbackUrl)
		{
			if (!callbackUrl.IsAbsoluteUri)
				throw new InvalidOperationException("The callback URI must be absolute.");

			if (callbackUrl.Scheme is "http" or "https")
			{
				throw new InvalidOperationException(
					$"{callbackUrl.Scheme}:// schemes are not supported for the callback URI on Windows. " +
					"Use a custom URI scheme instead.");
			}

			if (AppInfoUtils.IsPackagedApp)
			{
				if (!IsUriProtocolDeclared(callbackUrl.Scheme))
				{
					throw new InvalidOperationException(
						$"You need to declare the windows.protocol usage of the " +
						$"protocol/scheme `{callbackUrl.Scheme}` in your AppxManifest.xml file.");
				}
			}
			else if (!IsRegistryDeclared(callbackUrl.Scheme))
			{
				throw new InvalidOperationException(
					$"The URI scheme '{callbackUrl.Scheme}' is not registered. " +
					"Call ActivationRegistrationManager.RegisterForProtocolActivation when running unpackaged.");
			}
		}

		static void RegisterCallbackRoute(Uri callbackUrl)
		{
			var currentInstance = AppInstance.GetCurrent();
			var routeKey = CreateCallbackRouteKey(callbackUrl);
			var currentKey = currentInstance.Key;

			if (string.Equals(currentKey, routeKey, StringComparison.Ordinal))
				return;

			// Preserve application-owned AppInstance routing.
			if (!CanRegisterCallbackRoute(currentKey))
				return;

			var routeOwner = AppInstance.FindOrRegisterForKey(routeKey);
			if (routeOwner is null)
			{
				throw new InvalidOperationException(
					$"Unable to register the callback route for scheme '{callbackUrl.Scheme}'.");
			}

			if (!routeOwner.IsCurrent)
			{
				throw new InvalidOperationException(
					$"Another app instance is already waiting for the callback scheme '{callbackUrl.Scheme}'.");
			}

			if (!string.Equals(routeOwner.Key, routeKey, StringComparison.Ordinal))
			{
				throw new InvalidOperationException(
					$"Unable to register the callback route for scheme '{callbackUrl.Scheme}'.");
			}
		}

		static AppInstance? FindCallbackRouteOwner(Uri callbackUri)
		{
			var routeKey = CreateCallbackRouteKey(callbackUri);

			return AppInstance.GetInstances().FirstOrDefault(instance =>
				!instance.IsCurrent &&
				string.Equals(instance.Key, routeKey, StringComparison.Ordinal));
		}

		static bool RedirectActivationAndExit(AppInstance routeOwner, AppActivationArguments args)
		{
			try
			{
				// Complete redirection before terminating this transient callback process.
				routeOwner.RedirectActivationToAsync(args).AsTask().GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to redirect WebAuthenticator callback activation: {ex}");
				return false;
			}

			Process.GetCurrentProcess().Kill();
			return true;
		}

		// AppInstance keys are app-defined, so keep WebAuthenticator routes separate from application-owned keys.
		internal static string CreateCallbackRouteKey(Uri callbackUrl) =>
			$"{CallbackRouteKeyPrefix}{callbackUrl.Scheme}";

		internal static bool CanRegisterCallbackRoute(string? currentKey) =>
			string.IsNullOrEmpty(currentKey) ||
			currentKey.StartsWith(CallbackRouteKeyPrefix, StringComparison.Ordinal);

		internal static bool IsSameCallbackRoute(Uri expectedCallbackUrl, Uri callbackUrl) =>
			string.Equals(
				CreateCallbackRouteKey(expectedCallbackUrl),
				CreateCallbackRouteKey(callbackUrl),
				StringComparison.Ordinal);

		static bool IsUriProtocolDeclared(string scheme)
		{
			var docPath = FileSystemUtils.PlatformGetFullAppPackageFilePath(PlatformUtils.AppManifestFilename);
			var doc = XDocument.Load(docPath, LoadOptions.None);

			using var reader = doc.CreateReader();
			var namespaceManager = new XmlNamespaceManager(reader.NameTable);
			namespaceManager.AddNamespace("uap", PlatformUtils.AppManifestUapXmlns);

			var root = doc.Root ?? throw new InvalidOperationException("The app manifest could not be loaded.");
			var declarations = root.XPathSelectElements(
				$"//uap:Extension[@Category='windows.protocol']/uap:Protocol[@Name='{scheme}']",
				namespaceManager);

			return declarations.Any();
		}

		static bool IsRegistryDeclared(string scheme)
		{
			using var key = Win32.Registry.ClassesRoot.OpenSubKey(scheme);
			return key?.GetValue("URL Protocol") is not null;
		}
	}
}
