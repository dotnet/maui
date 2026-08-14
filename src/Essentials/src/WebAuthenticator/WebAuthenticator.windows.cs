#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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
		const int ErrorSuccess = 0;
		const int ErrorInsufficientBuffer = 122;

		public bool OnAppInstanceActivatedCallback(AppActivationArguments args) =>
			TryHandleBuiltInActivation(args);

		internal static bool TryHandleBuiltInActivation(AppActivationArguments args)
		{
			if (args.Kind != ExtendedActivationKind.Protocol ||
				args.Data is not IProtocolActivatedEventArgs protocolArgs)
			{
				return false;
			}

			var callbackUri = protocolArgs.Uri;

			// The built-in process-wide request always gets first refusal for a matching route.
			if (WebAuthenticatorRequestManager.TryHandleCallback(callbackUri))
				return true;

			var routeOwner = FindCallbackRouteOwner(callbackUri);
			if (routeOwner is null)
				return false;

			return RedirectActivationAndExit(routeOwner, args);
		}

		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
			AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(
			WebAuthenticatorOptions webAuthenticatorOptions,
			CancellationToken cancellationToken)
		{
			if (webAuthenticatorOptions is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions));

			var url = webAuthenticatorOptions.Url;
			var callbackUrl = webAuthenticatorOptions.CallbackUrl;
			var responseDecoder = webAuthenticatorOptions.ResponseDecoder;
			var prefersEphemeral = webAuthenticatorOptions.PrefersEphemeralWebBrowserSession;

			if (url is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
			if (callbackUrl is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));
			if (!url.IsAbsoluteUri)
				throw new ArgumentException("The authentication URL must be absolute.", nameof(webAuthenticatorOptions.Url));
			if (!callbackUrl.IsAbsoluteUri)
				throw new ArgumentException("The callback URL must be absolute.", nameof(webAuthenticatorOptions.CallbackUrl));

			ValidateCallbackUrl(callbackUrl);
			cancellationToken.ThrowIfCancellationRequested();

			// AppWindow is agile and remains strongly captured by the framework-owned closure.
			var appWindow = TryGetActiveAppWindow();
			var request = WebAuthenticatorRequestManager.Begin(
				url,
				callbackUrl,
				responseDecoder,
				prefersEphemeral,
				cancellationToken,
				() => TryBringToForeground(appWindow));

			var registration = cancellationToken.Register(
				static state =>
				{
					var request = (WebAuthenticatorRequest)state!;
					WebAuthenticatorRequestManager.TryCancelFromCaller(request);
				},
				request);

			try
			{
				if (!request.Task.IsCompleted)
				{
					try
					{
						RegisterCallbackRoute(callbackUrl);
					}
					catch (Exception)
					{
						WebAuthenticatorRequestManager.TryFail(
							request,
							new InvalidOperationException("Unable to register the WebAuthenticator callback route."));
					}
				}

				if (!request.Task.IsCompleted)
				{
					try
					{
						if (!await global::Windows.System.Launcher.LaunchUriAsync(url))
						{
							WebAuthenticatorRequestManager.TryFail(
								request,
								new InvalidOperationException("Failed to launch the browser for authentication."));
						}
					}
					catch (Exception)
					{
						WebAuthenticatorRequestManager.TryFail(
							request,
							new InvalidOperationException("Failed to launch the browser for authentication."));
					}
				}

				return await request.Task.ConfigureAwait(false);
			}
			finally
			{
				// Windows has no per-request native object to close. Registration disposal precedes End.
				registration.Dispose();
				WebAuthenticatorRequestManager.End(request);
			}
		}

		static AppWindow? TryGetActiveAppWindow()
		{
			try
			{
				return WindowStateManager.Default.GetActiveAppWindow(false);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to identify the WebAuthenticator window ({ex}).");
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

				if (appWindow.Presenter is OverlappedPresenter presenter &&
					presenter.State == OverlappedPresenterState.Minimized)
				{
					presenter.Restore(false);
				}

				if (!appWindow.IsVisible)
					appWindow.Show(false);

				if (!PlatformMethods.SetForegroundWindow(windowHandle))
					Debug.WriteLine("Windows denied the WebAuthenticator window foreground activation.");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to bring the WebAuthenticator window to the foreground ({ex}).");
			}
		}

		static void ValidateCallbackUrl(Uri callbackUrl)
		{
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
						$"You need to declare the windows.protocol usage of scheme '{callbackUrl.Scheme}' " +
						"for the current application in AppxManifest.xml.");
				}
			}
			else if (!IsRegistryDeclared(callbackUrl.Scheme))
			{
				throw new InvalidOperationException(
					$"The URI scheme '{callbackUrl.Scheme}' is not registered for this application. " +
					"Call ActivationRegistrationManager.RegisterForProtocolActivation when running unpackaged.");
			}
		}

		static void RegisterCallbackRoute(Uri callbackUrl)
		{
			AppInstance currentInstance;
			string? currentKey;

			try
			{
				currentInstance = AppInstance.GetCurrent();
				currentKey = currentInstance.Key;
			}
			catch (Exception)
			{
				throw new InvalidOperationException("Unable to inspect the current application instance.");
			}

			var routeKey = CreateCallbackRouteKey(callbackUrl);
			if (string.Equals(currentKey, routeKey, StringComparison.Ordinal))
				return;

			// App-owned keys are never replaced. The app must redirect protocol activations to this instance.
			if (!CanRegisterCallbackRoute(currentKey))
				return;

			// Keep the framework route registered after the request. Explicitly calling UnregisterKey
			// prevents reliable re-registration (https://github.com/microsoft/WindowsAppSDK/issues/4420),
			// while FindOrRegisterForKey replaces a previous framework route when the next request uses
			// a different callback scheme.
			AppInstance? routeOwner;
			try
			{
				routeOwner = AppInstance.FindOrRegisterForKey(routeKey);
			}
			catch (Exception)
			{
				throw new InvalidOperationException("Unable to register the WebAuthenticator callback route.");
			}

			if (routeOwner is null || !routeOwner.IsCurrent ||
				!string.Equals(routeOwner.Key, routeKey, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("Another app instance owns the WebAuthenticator callback route.");
			}
		}

		static AppInstance? FindCallbackRouteOwner(Uri callbackUri)
		{
			try
			{
				var routeKey = CreateCallbackRouteKey(callbackUri);

				return AppInstance.GetInstances().FirstOrDefault(instance =>
					!instance.IsCurrent &&
					string.Equals(instance.Key, routeKey, StringComparison.Ordinal));
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to inspect WebAuthenticator callback route ownership ({ex}).");
				return null;
			}
		}

		static bool RedirectActivationAndExit(AppInstance routeOwner, AppActivationArguments args)
		{
			_ = RedirectActivationAndExitAsync(routeOwner, args);
			return true;
		}

		static async Task RedirectActivationAndExitAsync(AppInstance routeOwner, AppActivationArguments args)
		{
			try
			{
				// Both arguments remain captured until the asynchronous redirection has finished.
				await routeOwner.RedirectActivationToAsync(args).AsTask().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to redirect WebAuthenticator callback activation ({ex}).");
			}
			finally
			{
				TerminateCurrentProcess();
			}
		}

		static void TerminateCurrentProcess()
		{
			try
			{
				Process.GetCurrentProcess().Kill();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to terminate the transient WebAuthenticator callback process ({ex}).");
				Environment.Exit(1);
			}
		}

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
			try
			{
				var docPath = FileSystemUtils.PlatformGetFullAppPackageFilePath(PlatformUtils.AppManifestFilename);
				var document = XDocument.Load(docPath, LoadOptions.None);
				return IsUriProtocolDeclared(document, TryGetCurrentApplicationId(), scheme);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to inspect the current application protocol declaration ({ex}).");
				return false;
			}
		}

		internal static bool IsUriProtocolDeclared(XDocument document, string? applicationId, string scheme)
		{
			if (document?.Root is null || string.IsNullOrEmpty(scheme))
				return false;

			var applications = document.Root
				.Descendants()
				.Where(element => element.Name.LocalName == "Application" && element.Parent?.Name.LocalName == "Applications")
				.ToList();

			XElement? currentApplication;
			if (!string.IsNullOrEmpty(applicationId))
			{
				currentApplication = applications.FirstOrDefault(element =>
					string.Equals((string?)element.Attribute("Id"), applicationId, StringComparison.Ordinal));
			}
			else
			{
				currentApplication = applications.Count == 1 ? applications[0] : null;
			}

			if (currentApplication is null)
				return false;

			return currentApplication
				.Descendants()
				.Where(element =>
					element.Name.LocalName == "Extension" &&
					string.Equals((string?)element.Attribute("Category"), "windows.protocol", StringComparison.Ordinal))
				.SelectMany(extension => extension.Descendants().Where(element => element.Name.LocalName == "Protocol"))
				.Any(protocol => string.Equals(
					(string?)protocol.Attribute("Name"),
					scheme,
					StringComparison.OrdinalIgnoreCase));
		}

		static string? TryGetCurrentApplicationId()
		{
			try
			{
				if (global::Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.ApplicationModel.AppInfo"))
				{
					var applicationId = global::Windows.ApplicationModel.AppInfo.Current.Id;
					if (!string.IsNullOrEmpty(applicationId))
						return applicationId;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to read the current packaged application identity ({ex}).");
			}

			return TryGetApplicationIdFromAppUserModelId();
		}

		static string? TryGetApplicationIdFromAppUserModelId()
		{
			try
			{
				uint length = 0;
				var result = GetCurrentApplicationUserModelId(ref length, null);
				if (result != ErrorInsufficientBuffer || length == 0 || length > 4096)
					return null;

				var builder = new StringBuilder((int)length);
				result = GetCurrentApplicationUserModelId(ref length, builder);
				if (result != ErrorSuccess)
					return null;

				var appUserModelId = builder.ToString();
				var separatorIndex = appUserModelId.LastIndexOf("!", StringComparison.Ordinal);
				return separatorIndex >= 0 && separatorIndex + 1 < appUserModelId.Length
					? appUserModelId.Substring(separatorIndex + 1)
					: null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		static bool IsRegistryDeclared(string scheme)
		{
			try
			{
				using var key = Win32.Registry.ClassesRoot.OpenSubKey(scheme);
				if (key?.GetValue("URL Protocol") is null)
					return false;

				using var commandKey = key.OpenSubKey(@"shell\open\command");
				var command = commandKey?.GetValue(null) as string;
				return IsRegistryCommandOwnedByCurrentExecutable(command, Environment.ProcessPath);
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal static bool IsRegistryCommandOwnedByCurrentExecutable(string? command, string? currentExecutablePath)
		{
			if (string.IsNullOrWhiteSpace(command) || string.IsNullOrWhiteSpace(currentExecutablePath))
				return true;

			if (!TryGetCommandExecutable(command, out var registeredExecutable) ||
				!Path.IsPathFullyQualified(registeredExecutable))
			{
				return true;
			}

			try
			{
				return string.Equals(
					Path.GetFullPath(registeredExecutable),
					Path.GetFullPath(currentExecutablePath),
					StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				// Ambiguous commands must not create a false negative.
				return true;
			}
		}

		static bool TryGetCommandExecutable(string command, out string executable)
		{
			executable = string.Empty;
			var trimmed = command.TrimStart();
			if (trimmed.Length == 0)
				return false;

			if (trimmed[0] == '"')
			{
				var closingQuote = trimmed.IndexOf("\"", 1, StringComparison.Ordinal);
				if (closingQuote <= 1)
					return false;

				executable = trimmed.Substring(1, closingQuote - 1);
			}
			else
			{
				var separator = trimmed.IndexOfAny(new[] { ' ', '\t' });
				executable = separator < 0 ? trimmed : trimmed.Substring(0, separator);
			}

			executable = Environment.ExpandEnvironmentVariables(executable);
			return !string.IsNullOrWhiteSpace(executable) &&
				!executable.Contains("%", StringComparison.Ordinal) &&
				string.Equals(Path.GetExtension(executable), ".exe", StringComparison.OrdinalIgnoreCase);
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		static extern int GetCurrentApplicationUserModelId(
			ref uint applicationUserModelIdLength,
			StringBuilder? applicationUserModelId);
	}
}
