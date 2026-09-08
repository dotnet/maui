using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using MauiContacts = Microsoft.Maui.ApplicationModel.Communication.Contacts;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
#if ANDROID
using Android.App;
#endif

namespace Microsoft.Maui.Hosting
{
	public interface IEssentialsBuilder
	{
		IEssentialsBuilder UseMapServiceToken(string token);

		IEssentialsBuilder AddAppAction(AppAction appAction);

		IEssentialsBuilder OnAppAction(Action<AppAction> action);

		IEssentialsBuilder UseVersionTracking();
	}

	public static class EssentialsExtensions
	{
		internal static MauiAppBuilder UseEssentials(this MauiAppBuilder builder)
		{
#if !(ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS || TIZEN)
			// Register MainThreadBridgeInitializer FIRST so MainThread.SetCustomImplementation
			// runs before EssentialsInitializer resolves DI-registered services. Order matters:
			// IMauiInitializeService instances are executed in DI registration order
			// (MauiContextExtensions.InitializeAppServices iterates GetServices<IMauiInitializeService>()),
			// and on netstandard / external TFMs MainThread throws NotImplementedInReferenceAssemblyException
			// until the bridge is installed. If EssentialsInitializer ran first, any DI-registered
			// Essentials implementation whose constructor touched MainThread would fail during
			// the very bridge call meant to enable it.
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MainThreadBridgeInitializer>());
#endif

			// Register the EssentialsInitializer unconditionally so DI-registered Essentials
			// implementations are bridged to the static facades during app startup, even when
			// ConfigureEssentials() is not called. The initializer's AppActions event handler
			// is only attached when at least one AppAction handler is configured, to avoid
			// retaining the initializer instance via the static AppActions.OnAppAction event
			// for apps that never opt into AppActions.
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, EssentialsInitializer>());

			builder.ConfigureLifecycleEvents(life =>
			{
#if ANDROID
				ApplicationModel.Platform.Init((Application)Application.Context);

				life.AddAndroid(android => android
					.OnCreate((activity, savedInstanceState) =>
					{
						ApplicationModel.Platform.Init(activity, savedInstanceState);
					})
					.OnRequestPermissionsResult((activity, requestCode, permissions, grantResults) =>
					{
						ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
					})
					.OnNewIntent((activity, intent) =>
					{
						ApplicationModel.Platform.OnNewIntent(intent);
					})
					.OnResume((activity) =>
					{
						ApplicationModel.Platform.OnResume();
					}));
#elif __IOS__
				life.AddiOS(ios => ios
					.ContinueUserActivity((application, userActivity, completionHandler) =>
					{
						return ApplicationModel.Platform.ContinueUserActivity(application, userActivity, completionHandler);
					})
					.OpenUrl((application, url, options) =>
					{
						return ApplicationModel.Platform.OpenUrl(application, url, options);
					})
					.PerformActionForShortcutItem((application, shortcutItem, completionHandler) =>
					{
						ApplicationModel.Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler);
					}));
#elif WINDOWS
				life.AddWindows(windows => windows
					.OnActivated((window, args) =>
					{
						ApplicationModel.Platform.OnActivated(window, args);
					})
					.OnLaunched((application, args) =>
					{
						ApplicationModel.Platform.OnLaunched(args);
					})
					.OnPlatformWindowSubclassed((window, context) =>
					{
						ApplicationModel.Platform.OnPlatformWindowInitialized(window);
					}));
#elif TIZEN

#endif
			});

			return builder;
		}

		public static MauiAppBuilder ConfigureEssentials(this MauiAppBuilder builder, Action<IEssentialsBuilder>? configureDelegate = null)
		{
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<EssentialsRegistration>(new EssentialsRegistration(configureDelegate));
			}

			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, EssentialsInitializer>());

			return builder;
		}

		public static IEssentialsBuilder AddAppAction(this IEssentialsBuilder essentials, string id, string title, string? subtitle = null, string? icon = null) =>
			essentials.AddAppAction(new AppAction(id, title, subtitle, icon));

		internal class EssentialsRegistration
		{
			private readonly Action<IEssentialsBuilder> _registerEssentials;

			public EssentialsRegistration(Action<IEssentialsBuilder> registerEssentials)
			{
				_registerEssentials = registerEssentials;
			}

			internal void RegisterEssentialsOptions(IEssentialsBuilder essentials)
			{
				_registerEssentials(essentials);
			}
		}

		/// <summary>
		/// Lightweight initializer that bridges the MAUI application dispatcher to MainThread
		/// so that MainThread.BeginInvokeOnMainThread and MainThread.IsMainThread work
		/// on custom platform backends / external TFMs where no native
		/// MainThread implementation exists.
		/// </summary>
#if !(ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS || TIZEN)
		class MainThreadBridgeInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				var dispatcher = services.GetOptionalApplicationDispatcher();
				if (dispatcher is null)
					return;

				MainThread.SetCustomImplementation(
					isMainThread: () => !dispatcher.IsDispatchRequired,
					beginInvokeOnMainThread: action => dispatcher.Dispatch(action));
			}
		}
#endif

		class EssentialsInitializer : IMauiInitializeService
		{
			private readonly IEnumerable<EssentialsRegistration> _essentialsRegistrations;
			private EssentialsBuilder? _essentialsBuilder;

			public EssentialsInitializer(IEnumerable<EssentialsRegistration> essentialsRegistrations)
			{
				_essentialsRegistrations = essentialsRegistrations;
			}

			public void Initialize(IServiceProvider services)
			{
				_essentialsBuilder = new EssentialsBuilder();
				if (_essentialsRegistrations != null)
				{
					foreach (var essentialsRegistration in _essentialsRegistrations)
					{
						essentialsRegistration.RegisterEssentialsOptions(_essentialsBuilder);
					}
				}

				BridgeEssentialsFromDI(services);

#if WINDOWS
				// Only forward MapServiceToken when ConfigureEssentials(e => e.UseMapServiceToken(...))
				// supplied a value. Without this null guard, EssentialsInitializer (now registered
				// unconditionally) would overwrite any token a caller had set directly via
				// ApplicationModel.Platform.MapServiceToken before MauiApp.Build().
				if (_essentialsBuilder.MapServiceToken is not null)
					ApplicationModel.Platform.MapServiceToken = _essentialsBuilder.MapServiceToken;
#endif

#if !TIZEN
				// Only subscribe to the static AppActions.OnAppAction event when at least one
				// handler was actually registered via IEssentialsBuilder.OnAppAction. The static
				// event subscription would otherwise pin this initializer instance for the app's
				// lifetime (and across repeated MauiApp.Build() calls in tests / hosting scenarios)
				// even when the handler is a no-op.
				if (_essentialsBuilder.AppActionHandlers is not null)
					AppActions.OnAppAction += HandleOnAppAction;

				if (_essentialsBuilder.AppActions is not null)
				{
					SetAppActions(services, _essentialsBuilder.AppActions);
				}
#endif

				if (_essentialsBuilder.TrackVersions)
					VersionTracking.Track();
			}

			/// <summary>
			/// Bridges DI-registered Essentials implementations to the static facades.
			/// If a service is registered in DI, it becomes the backing implementation for
			/// the corresponding static API. If not registered, the existing lazy platform
			/// default behavior is preserved.
			/// </summary>
			static void BridgeEssentialsFromDI(IServiceProvider services)
			{
				// SetDefault pattern types
				BridgeIfRegistered<IAccelerometer>(services, Accelerometer.SetDefault);
				// IActivityStateManager is intentionally NOT bridged. It is Android-only, and its
				// platform default is already initialized — with its ActivityLifecycleCallbacks
				// registered — by ApplicationModel.Platform.Init() during UseEssentials(), before this
				// bridge runs at MauiApp.Build() time. Replacing ActivityStateManager.Default after that
				// point would leave the original lifecycle listener registered while the replacement
				// missed the initial Init(Application) call. Custom (non-Android) backends never reach
				// the Android-only code path, so bridging it here serves no purpose.
				BridgeIfRegistered<IBarometer>(services, Barometer.SetDefault);
				BridgeIfRegistered<IBattery>(services, Battery.SetDefault);
				BridgeIfRegistered<IBrowser>(services, Browser.SetDefault);
				BridgeIfRegistered<IClipboard>(services, Clipboard.SetDefault);
				BridgeIfRegistered<ICompass>(services, Compass.SetDefault);
				BridgeIfRegistered<IContacts>(services, MauiContacts.SetDefault);
				BridgeIfRegistered<IEmail>(services, Email.SetDefault);
				BridgeIfRegistered<IFilePicker>(services, FilePicker.SetDefault);
				BridgeIfRegistered<IFlashlight>(services, Flashlight.SetDefault);
				BridgeIfRegistered<IGeolocation>(services, Geolocation.SetDefault);
				BridgeIfRegistered<IGyroscope>(services, Gyroscope.SetDefault);
				BridgeIfRegistered<IHapticFeedback>(services, HapticFeedback.SetDefault);
				BridgeIfRegistered<ILauncher>(services, Launcher.SetDefault);
				BridgeIfRegistered<IMagnetometer>(services, Magnetometer.SetDefault);
				BridgeIfRegistered<IMap>(services, Map.SetDefault);
				BridgeIfRegistered<IMediaPicker>(services, MediaPicker.SetDefault);
				BridgeIfRegistered<IOrientationSensor>(services, OrientationSensor.SetDefault);
				BridgeIfRegistered<IPhoneDialer>(services, PhoneDialer.SetDefault);
				BridgeIfRegistered<IPreferences>(services, Preferences.SetDefault);
				BridgeIfRegistered<IScreenshot>(services, Screenshot.SetDefault);
				BridgeIfRegistered<ISecureStorage>(services, SecureStorage.SetDefault);
				BridgeIfRegistered<ISemanticScreenReader>(services, SemanticScreenReader.SetDefault);
				BridgeIfRegistered<IShare>(services, Share.SetDefault);
				BridgeIfRegistered<ISms>(services, Sms.SetDefault);
				BridgeIfRegistered<ITextToSpeech>(services, TextToSpeech.SetDefault);
				BridgeIfRegistered<IVersionTracking>(services, VersionTracking.SetDefault);
				BridgeIfRegistered<IVibration>(services, Vibration.SetDefault);
				// IWebAuthenticator: on Android/iOS/MacCatalyst the platform callback activities
				// and lifecycle hooks (WebAuthenticatorCallbackActivity.OnResume, Platform.OpenUrl,
				// ContinueUserActivity) cast WebAuthenticator.Default to IPlatformWebAuthenticatorCallback
				// via AsPlatformCallback(). Only bridge a DI implementation that supports that contract
				// on those platforms, mirroring the IAppActions guard below, to avoid a
				// PlatformNotSupportedException at runtime.
				var webAuthenticator = services.GetService<IWebAuthenticator>();
				if (webAuthenticator is not null)
				{
#if ANDROID || __IOS__ || __MACCATALYST__
					if (webAuthenticator is IPlatformWebAuthenticatorCallback)
						WebAuthenticator.SetDefault(webAuthenticator);
#else
					WebAuthenticator.SetDefault(webAuthenticator);
#endif
				}
#if WINDOWS || __IOS__ || __MACCATALYST__
				BridgeIfRegistered<IWindowStateManager>(services, WindowStateManager.SetDefault);
#endif
				BridgeIfRegistered<IAppleSignInAuthenticator>(services, AppleSignInAuthenticator.SetDefault);

				// SetCurrent pattern types
				// IAppActions: On native platforms, lifecycle hooks cast AppActions.Current to
				// IPlatformAppActions via AsPlatform(). Only bridge if the DI implementation
				// supports it, to prevent PlatformNotSupportedException at runtime.
				var appActions = services.GetService<IAppActions>();
				if (appActions is not null)
				{
#if WINDOWS || __IOS__ || __MACCATALYST__ || ANDROID
					if (appActions is IPlatformAppActions)
						AppActions.SetCurrent(appActions);
#else
					AppActions.SetCurrent(appActions);
#endif
				}
				BridgeIfRegistered<IAppInfo>(services, AppInfo.SetCurrent);
				BridgeIfRegistered<IConnectivity>(services, Connectivity.SetCurrent);
				BridgeIfRegistered<IDeviceDisplay>(services, DeviceDisplay.SetCurrent);
				BridgeIfRegistered<IDeviceInfo>(services, DeviceInfo.SetCurrent);
				BridgeIfRegistered<IFileSystem>(services, FileSystem.SetCurrent);
				BridgeIfRegistered<IGeocoding>(services, Geocoding.SetCurrent);
			}

			/// <summary>
			/// Resolves a DI-registered implementation and assigns it to the corresponding static facade.
			/// Note: The resolved instance is stored in a static field for the app lifetime, effectively
			/// promoting it to singleton scope regardless of its DI registration lifetime. Services bridged
			/// here should be registered as Singleton for correct behavior.
			/// </summary>
			static void BridgeIfRegistered<T>(IServiceProvider services, Action<T?> setter) where T : class
			{
				var impl = services.GetService<T>();
				if (impl is not null)
					setter(impl);
			}

			private static async void SetAppActions(IServiceProvider services, List<AppAction> appActions)
			{
				try
				{
					await AppActions.SetAsync(appActions);
				}
				catch (FeatureNotSupportedException ex)
				{
					services.GetService<ILoggerFactory>()?
						.CreateLogger<IEssentialsBuilder>()?
						.LogError(ex, "App Actions are not supported on this platform.");
				}
			}

			void HandleOnAppAction(object? sender, AppActionEventArgs e)
			{
				_essentialsBuilder?.AppActionHandlers?.Invoke(e.AppAction);
			}
		}

		class EssentialsBuilder : IEssentialsBuilder
		{
			List<AppAction>? _appActions;
			internal Action<AppAction>? AppActionHandlers;
			internal bool TrackVersions;

			internal List<AppAction>? AppActions => _appActions;

#pragma warning disable CS0414 // Remove unread private members
			internal string? MapServiceToken;
#pragma warning restore CS0414 // Remove unread private members

			public IEssentialsBuilder UseMapServiceToken(string token)
			{
				MapServiceToken = token;
				return this;
			}

			public IEssentialsBuilder AddAppAction(AppAction appAction)
			{
				_appActions ??= new List<AppAction>();
				_appActions.Add(appAction);
				return this;
			}

			public IEssentialsBuilder OnAppAction(Action<AppAction> action)
			{
				AppActionHandlers += action;
				return this;
			}

			public IEssentialsBuilder UseVersionTracking()
			{
				TrackVersions = true;
				return this;
			}
		}
	}
}
