using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using MauiContacts = Microsoft.Maui.ApplicationModel.Communication.Contacts;
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
		static readonly object s_appActionsSetLock = new();
		static readonly List<AppActionsSetAssignment> s_appActionsSetAssignments = new();
		static Task s_appActionsSetTail = Task.CompletedTask;

		// Serializes the shared map-token assignment graph and per-implementation state.
		// It is intentionally NOT held across user code, facade reads, or user-implementable
		// IPlatformGeocoding token accessors. Facade ownership bookkeeping is serialized per
		// type by FacadeBridgeState<T>.SyncRoot instead. See EssentialsInitializer.InitializeCore.
#if WINDOWS || TIZEN
		static readonly object s_mapTokenLock = new();
#endif
#if WINDOWS || TIZEN
		static readonly List<MapTokenAssignment> s_mapTokenAssignments = new();
		static readonly List<MapTokenImplementationState> s_mapTokenImplementationStates = new();
		static int s_mapTokenEpoch;
#endif
#if WINDOWS
		internal static Func<string?> WindowsMapServiceTokenGetter { get; set; } =
			static () => global::Windows.Services.Maps.MapService.ServiceToken;

		internal static Action<string?> WindowsMapServiceTokenSetter { get; set; } =
			static token => global::Windows.Services.Maps.MapService.ServiceToken = token;
#endif

		internal static void RestoreFacadeCleanups(List<Action> facadeCleanups)
		{
			// No global lock: each cleanup action acquires the per-type FacadeBridgeState<T>.SyncRoot
			// (map-token cleanups use s_mapTokenLock) so restoration stays serialized with concurrent
			// bridging without holding a process-global lock across the whole batch.
			List<Exception>? exceptions = null;
			try
			{
				for (int i = facadeCleanups.Count - 1; i >= 0; i--)
				{
					try
					{
						facadeCleanups[i]();
					}
					catch (Exception ex)
					{
						(exceptions ??= new()).Add(ex);
					}
				}
			}
			finally
			{
				facadeCleanups.Clear();
			}

			if (exceptions is null)
				return;

			if (exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

			throw new AggregateException("One or more Essentials facade cleanup actions failed.", exceptions);
		}

		internal static MauiAppBuilder UseEssentials(this MauiAppBuilder builder)
		{
			AddEssentialsInitializer(builder);

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
					.OnAppInstanceActivated((application, args) =>
					{
						return ApplicationModel.Platform.OnAppInstanceActivated(application, args);
					})
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

			AddEssentialsInitializer(builder);

			return builder;
		}

		static void AddEssentialsInitializer(MauiAppBuilder builder)
		{
			builder.ConfigureDispatching();

			builder.Services.TryAddSingleton<EssentialsCleanup>();
			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<IMauiAppCleanupService, EssentialsCleanup>(
					services => services.GetRequiredService<EssentialsCleanup>()));
			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<IMauiAppPostProviderCleanupService, EssentialsCleanup>(
					services => services.GetRequiredService<EssentialsCleanup>()));
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, EssentialsInitializer>());
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
				// No process-global lock is held across InitializeCore. It runs user code
				// (ConfigureEssentials delegates and DI factory resolution) that could reentrantly
				// build/dispose another MauiApp or read a facade on another thread; holding a global
				// lock here previously risked deadlock. Facade ownership is serialized per type by
				// FacadeBridgeState<T>.SyncRoot; map-token bookkeeping uses s_mapTokenLock.
				InitializeCore(services);
			}

			void InitializeCore(IServiceProvider services)
			{
				var preProviderCleanups = new List<Action>();
				var facadeCleanups = new List<Action>();
				EssentialsCleanup? cleanup = null;
				try
				{
#if !(ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS || TIZEN)
					BridgeMainThreadFromDispatcher(services, facadeCleanups);
#endif

					_essentialsBuilder = new EssentialsBuilder();
					if (_essentialsRegistrations != null)
					{
						foreach (var essentialsRegistration in _essentialsRegistrations)
						{
							essentialsRegistration.RegisterEssentialsOptions(_essentialsBuilder);
						}
					}

#if WINDOWS || TIZEN
					var hasExplicitMapServiceToken = _essentialsBuilder.HasMapServiceToken;
					var explicitMapServiceToken = _essentialsBuilder.MapServiceToken;
					var preResolvedGeocoding = services.GetService<IGeocoding>();
					IGeocoding? fallbackGeocodingForMapToken = null;
					if (hasExplicitMapServiceToken ? preResolvedGeocoding is null : preResolvedGeocoding is not null)
					{
						fallbackGeocodingForMapToken =
							CaptureGeocodingDefaultForMapServiceToken(createIfMissing: hasExplicitMapServiceToken);
					}

					string? inheritedMapServiceToken = null;
					if (!hasExplicitMapServiceToken &&
						preResolvedGeocoding is not null &&
						fallbackGeocodingForMapToken is IPlatformGeocoding existingPlatformGeocoding)
					{
						inheritedMapServiceToken = CaptureDirectUnownedMapServiceToken(existingPlatformGeocoding);
					}
#endif

					var versionTrackingDependencies = BridgeEssentialsFromDI(
						services,
						facadeCleanups
#if WINDOWS || TIZEN
						, preResolvedGeocoding
#endif
					);
					var versionTrackingOwnedByApp =
						versionTrackingDependencies.Preferences is not null ||
						versionTrackingDependencies.AppInfo is not null ||
						versionTrackingDependencies.VersionTracking is not null;
					BridgeLazyVersionTrackingFromDI(versionTrackingDependencies, facadeCleanups);
					BridgeAppInfoSecureStorageFromDI(
						versionTrackingDependencies.AppInfo,
						versionTrackingDependencies.SecureStorage,
						facadeCleanups);

					// Resolve app-owned cleanup before registering AppActions handlers. Facade
					// actions are appended after initialization has accumulated the complete batch.
					cleanup = services.GetRequiredService<EssentialsCleanup>();

#if WINDOWS || TIZEN
					if (hasExplicitMapServiceToken)
					{
						var geocoding = versionTrackingDependencies.Geocoding ?? fallbackGeocodingForMapToken;
						if (explicitMapServiceToken is not null &&
							geocoding is IPlatformGeocoding platformGeocoding)
						{
							TrackAndSetMapServiceToken(platformGeocoding, explicitMapServiceToken, preProviderCleanups);
						}
						else if (geocoding is not null)
						{
							services.GetService<ILoggerFactory>()?
								.CreateLogger<EssentialsInitializer>()
								.LogWarning(
									"Configured map service token was not applied because {ImplementationType} does not implement {RequiredInterface}.",
									geocoding.GetType().FullName,
									nameof(IPlatformGeocoding));
						}
					}
					else if (inheritedMapServiceToken is not null &&
						versionTrackingDependencies.Geocoding is IPlatformGeocoding platformGeocoding)
					{
						// Preserve a direct pre-existing platform token only when this app replaces
						// the fallback geocoder with a DI implementation.
						TrackAndSetMapServiceToken(platformGeocoding, inheritedMapServiceToken, preProviderCleanups);
					}
#endif

#if !TIZEN
					// Only subscribe to the current AppActions implementation when at least one
					// handler was actually registered via IEssentialsBuilder.OnAppAction. The
					// subscription would otherwise pin this initializer instance for the app's
					// lifetime (and across repeated MauiApp.Build() calls in tests / hosting scenarios)
					// even when the handler is a no-op.
					if (_essentialsBuilder.AppActionHandlers is not null || _essentialsBuilder.AppActions is not null)
					{
						var appActions = AppActions.Current;

						if (_essentialsBuilder.AppActionHandlers is not null)
							Subscribe(appActions, HandleOnAppAction, preProviderCleanups);

						if (_essentialsBuilder.AppActions is not null)
						{
							var logger = services.GetService<ILoggerFactory>()?.CreateLogger<IEssentialsBuilder>();
							SetAppActions(appActions, logger, _essentialsBuilder.AppActions, preProviderCleanups);
						}
					}
#endif

					if (_essentialsBuilder.TrackVersions)
					{
						var versionTrackingBeforeTrack = VersionTracking.GetDefault();
						VersionTracking.Track();
						if (!versionTrackingOwnedByApp && VersionTracking.GetDefault() is { } initializedVersionTracking)
						{
							TrackInitializedOrOwned(
								initializedVersionTracking,
								versionTrackingBeforeTrack,
								VersionTracking.GetDefault,
								VersionTracking.SetDefault,
								facadeCleanups);
						}
					}

					cleanup.SetCleanups(preProviderCleanups, facadeCleanups);
				}
				catch (Exception initializationException)
				{
					try
					{
						RollbackInitialization(preProviderCleanups, facadeCleanups);
					}
					catch (Exception cleanupException)
					{
						throw new AggregateException(
							"Essentials initialization and cleanup both failed.",
							initializationException,
							cleanupException);
					}

					throw;
				}
			}

			static void RollbackInitialization(
				List<Action> preProviderCleanups,
				List<Action> facadeCleanups)
			{
				List<Exception>? exceptions = null;
				try
				{
					RestoreFacadeCleanups(preProviderCleanups);
				}
				catch (Exception ex)
				{
					(exceptions ??= new()).Add(ex);
				}

				try
				{
					RestoreFacadeCleanups(facadeCleanups);
				}
				catch (Exception ex)
				{
					(exceptions ??= new()).Add(ex);
				}

				if (exceptions is null)
					return;

				if (exceptions.Count == 1)
					ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

				throw new AggregateException("Essentials initialization rollback failed.", exceptions);
			}

#if !(ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS || TIZEN)
			static void BridgeMainThreadFromDispatcher(IServiceProvider services, List<Action> facadeCleanups)
			{
				var dispatcher = services.GetOptionalApplicationDispatcher();
				if (dispatcher is null)
					return;

				var implementation = MainThread.CreateCustomImplementation(
					isMainThread: () => !dispatcher.IsDispatchRequired,
					beginInvokeOnMainThread: action => dispatcher.Dispatch(action));

				TrackAndSet(
					implementation,
					MainThread.GetCustomImplementation,
					MainThread.SetCustomImplementation,
					facadeCleanups);
			}
#endif

			/// <summary>
			/// Bridges DI-registered Essentials implementations to the static facades.
			/// If a service is registered in DI, it becomes the backing implementation for
			/// the corresponding static API. If not registered, the existing lazy platform
			/// default behavior is preserved.
			/// </summary>
			static (
				IPreferences? Preferences,
				IAppInfo? AppInfo,
				IVersionTracking? VersionTracking,
				IGeocoding? Geocoding,
				ISecureStorage? SecureStorage) BridgeEssentialsFromDI(
				IServiceProvider services,
				List<Action> facadeCleanups,
				IGeocoding? preResolvedGeocoding = null)
			{
				IPreferences? preferences = null;
				IAppInfo? appInfo = null;
				IVersionTracking? versionTracking = null;
				IGeocoding? geocoding = null;
				ISecureStorage? secureStorage = null;

				// SetDefault pattern types
				BridgeIfRegistered<IAccelerometer>(services, () => GetFacadeBackingField<IAccelerometer>(typeof(Accelerometer), "defaultImplementation"), Accelerometer.SetDefault, facadeCleanups);
				// IActivityStateManager is intentionally not bridged. Init(Application) registers
				// Android lifecycle callbacks, and the interface has no way to unregister them.
				BridgeIfRegistered<IBarometer>(services, () => GetFacadeBackingField<IBarometer>(typeof(Barometer), "defaultImplementation"), Barometer.SetDefault, facadeCleanups);
				BridgeIfRegistered<IBattery>(services, () => GetFacadeBackingField<IBattery>(typeof(Battery), "defaultImplementation"), Battery.SetDefault, facadeCleanups);
				BridgeIfRegistered<IBrowser>(services, () => GetFacadeBackingField<IBrowser>(typeof(Browser), "defaultImplementation"), Browser.SetDefault, facadeCleanups);
				BridgeIfRegistered<IClipboard>(services, () => GetFacadeBackingField<IClipboard>(typeof(Clipboard), "defaultImplementation"), Clipboard.SetDefault, facadeCleanups);
				BridgeIfRegistered<ICompass>(services, () => GetFacadeBackingField<ICompass>(typeof(Compass), "defaultImplementation"), Compass.SetDefault, facadeCleanups);
				BridgeIfRegistered<IContacts>(services, () => GetFacadeBackingField<IContacts>(typeof(MauiContacts), "defaultImplementation"), MauiContacts.SetDefault, facadeCleanups);
				BridgeIfRegistered<IEmail>(services, () => GetFacadeBackingField<IEmail>(typeof(Email), "defaultImplementation"), Email.SetDefault, facadeCleanups);
				BridgeIfRegistered<IFilePicker>(services, () => GetFacadeBackingField<IFilePicker>(typeof(FilePicker), "defaultImplementation"), FilePicker.SetDefault, facadeCleanups);
				BridgeIfRegistered<IFlashlight>(services, () => GetFacadeBackingField<IFlashlight>(typeof(Flashlight), "defaultImplementation"), Flashlight.SetDefault, facadeCleanups);
				BridgeIfRegistered<IGeolocation>(services, () => GetFacadeBackingField<IGeolocation>(typeof(Geolocation), "defaultImplementation"), Geolocation.SetDefault, facadeCleanups);
				BridgeIfRegistered<IGyroscope>(services, () => GetFacadeBackingField<IGyroscope>(typeof(Gyroscope), "defaultImplementation"), Gyroscope.SetDefault, facadeCleanups);
				BridgeIfRegistered<IHapticFeedback>(services, () => GetFacadeBackingField<IHapticFeedback>(typeof(HapticFeedback), "defaultImplementation"), HapticFeedback.SetDefault, facadeCleanups);
				BridgeIfRegistered<ILauncher>(services, () => GetFacadeBackingField<ILauncher>(typeof(Launcher), "defaultImplementation"), Launcher.SetDefault, facadeCleanups);
				BridgeIfRegistered<IMagnetometer>(services, () => GetFacadeBackingField<IMagnetometer>(typeof(Magnetometer), "defaultImplementation"), Magnetometer.SetDefault, facadeCleanups);
				BridgeIfRegistered<IMap>(services, () => GetFacadeBackingField<IMap>(typeof(Map), "defaultImplementation"), Map.SetDefault, facadeCleanups);
				BridgeIfRegistered<IMediaPicker>(services, () => GetFacadeBackingField<IMediaPicker>(typeof(MediaPicker), "defaultImplementation"), MediaPicker.SetDefault, facadeCleanups);
				BridgeIfRegistered<IOrientationSensor>(services, () => GetFacadeBackingField<IOrientationSensor>(typeof(OrientationSensor), "defaultImplementation"), OrientationSensor.SetDefault, facadeCleanups);
				BridgeIfRegistered<IPhoneDialer>(services, () => GetFacadeBackingField<IPhoneDialer>(typeof(PhoneDialer), "defaultImplementation"), PhoneDialer.SetDefault, facadeCleanups);
				preferences = services.GetService<IPreferences>();
				if (preferences is not null)
					TrackAndSet(preferences, () => GetFacadeBackingField<IPreferences>(typeof(Preferences), "defaultImplementation"), Preferences.SetDefault, facadeCleanups);
				BridgeIfRegistered<IScreenshot>(services, () => GetFacadeBackingField<IScreenshot>(typeof(Screenshot), "defaultImplementation"), Screenshot.SetDefault, facadeCleanups);
				secureStorage = services.GetService<ISecureStorage>();
				if (secureStorage is not null)
					TrackAndSet(secureStorage, () => GetFacadeBackingField<ISecureStorage>(typeof(SecureStorage), "defaultImplementation"), SecureStorage.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISemanticScreenReader>(services, () => GetFacadeBackingField<ISemanticScreenReader>(typeof(SemanticScreenReader), "defaultImplementation"), SemanticScreenReader.SetDefault, facadeCleanups);
				BridgeIfRegistered<IShare>(services, () => GetFacadeBackingField<IShare>(typeof(Share), "defaultImplementation"), Share.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISms>(services, () => GetFacadeBackingField<ISms>(typeof(Sms), "defaultImplementation"), Sms.SetDefault, facadeCleanups);
				BridgeIfRegistered<ITextToSpeech>(services, () => GetFacadeBackingField<ITextToSpeech>(typeof(TextToSpeech), "defaultImplementation"), TextToSpeech.SetDefault, facadeCleanups);
				versionTracking = services.GetService<IVersionTracking>();
				if (versionTracking is not null)
					TrackAndSet(versionTracking, VersionTracking.GetDefault, VersionTracking.SetDefault, facadeCleanups);
				BridgeIfRegistered<IVibration>(services, () => GetFacadeBackingField<IVibration>(typeof(Vibration), "defaultImplementation"), Vibration.SetDefault, facadeCleanups);
				// IWebAuthenticator: native callback activities and lifecycle hooks
				// (WebAuthenticatorCallbackActivity.OnResume, Platform.OpenUrl,
				// ContinueUserActivity, Platform.OnAppInstanceActivated) cast
				// WebAuthenticator.Default to IPlatformWebAuthenticatorCallback via
				// AsPlatformCallback(). Only bridge a DI implementation that supports that contract
				// on those platforms, mirroring the IAppActions guard below, to avoid a
				// PlatformNotSupportedException at runtime.
				var webAuthenticator = services.GetService<IWebAuthenticator>();
				if (webAuthenticator is not null)
				{
#if ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS
					if (webAuthenticator is IPlatformWebAuthenticatorCallback)
						TrackAndSet(webAuthenticator, () => GetFacadeBackingField<IWebAuthenticator>(typeof(WebAuthenticator), "defaultImplementation"), WebAuthenticator.SetDefault, facadeCleanups);
					else
						LogMissingNativeLifecycleInterface<IWebAuthenticator>(services, nameof(IPlatformWebAuthenticatorCallback));
#else
					TrackAndSet(webAuthenticator, () => GetFacadeBackingField<IWebAuthenticator>(typeof(WebAuthenticator), "defaultImplementation"), WebAuthenticator.SetDefault, facadeCleanups);
#endif
				}
#if WINDOWS || __IOS__ || __MACCATALYST__
				BridgeIfRegistered<IWindowStateManager>(services, () => GetFacadeBackingField<IWindowStateManager>(typeof(WindowStateManager), "defaultImplementation"), WindowStateManager.SetDefault, facadeCleanups);
#endif
				BridgeIfRegistered<IAppleSignInAuthenticator>(services, () => GetFacadeBackingField<IAppleSignInAuthenticator>(typeof(AppleSignInAuthenticator), "defaultImplementation"), AppleSignInAuthenticator.SetDefault, facadeCleanups);

				// SetCurrent pattern types
				// IAppActions: On native platforms, lifecycle hooks cast AppActions.Current to
				// IPlatformAppActions via AsPlatform(). Only bridge if the DI implementation
				// supports it, to prevent PlatformNotSupportedException at runtime.
				var appActions = services.GetService<IAppActions>();
				if (appActions is not null)
				{
#if WINDOWS || __IOS__ || __MACCATALYST__ || ANDROID
					if (appActions is IPlatformAppActions)
						TrackAndSet(appActions, () => GetFacadeBackingField<IAppActions>(typeof(AppActions), "currentImplementation"), AppActions.SetCurrent, facadeCleanups);
					else
						LogMissingNativeLifecycleInterface<IAppActions>(services, nameof(IPlatformAppActions));
#else
					TrackAndSet(appActions, () => GetFacadeBackingField<IAppActions>(typeof(AppActions), "currentImplementation"), AppActions.SetCurrent, facadeCleanups);
#endif
				}
				appInfo = services.GetService<IAppInfo>();
				if (appInfo is not null)
					TrackAndSet(appInfo, () => GetFacadeBackingField<IAppInfo>(typeof(AppInfo), "currentImplementation"), AppInfo.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IConnectivity>(services, () => GetFacadeBackingField<IConnectivity>(typeof(Connectivity), "currentImplementation"), Connectivity.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IDeviceDisplay>(services, () => GetFacadeBackingField<IDeviceDisplay>(typeof(DeviceDisplay), "currentImplementation"), DeviceDisplay.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IDeviceInfo>(services, () => GetFacadeBackingField<IDeviceInfo>(typeof(DeviceInfo), "currentImplementation"), DeviceInfo.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IFileSystem>(services, () => GetFacadeBackingField<IFileSystem>(typeof(FileSystem), "currentImplementation"), FileSystem.SetCurrent, facadeCleanups);
				geocoding = preResolvedGeocoding ?? services.GetService<IGeocoding>();
				if (geocoding is not null)
					TrackAndSet(geocoding, () => GetFacadeBackingField<IGeocoding>(typeof(Geocoding), "defaultImplementation"), Geocoding.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IPermissions>(services, () => GetFacadeBackingField<IPermissions>(typeof(Permissions), "currentImplementation"), Permissions.SetCurrent, facadeCleanups);

				return (preferences, appInfo, versionTracking, geocoding, secureStorage);
			}

			static void BridgeLazyVersionTrackingFromDI(
				(
					IPreferences? Preferences,
					IAppInfo? AppInfo,
					IVersionTracking? VersionTracking,
					IGeocoding? Geocoding,
					ISecureStorage? SecureStorage) dependencies,
				List<Action> facadeCleanups)
			{
				if (dependencies.VersionTracking is not null)
					return;

				if (dependencies.Preferences is null && dependencies.AppInfo is null)
					return;

				// VersionTracking captures Preferences and AppInfo when its lazy default is created.
				// Install an app-owned lazy wrapper so a later static call cannot retain provider-owned
				// services after this MauiApp is disposed.
				Func<VersionTrackingDependency<IPreferences>> getPreferences = dependencies.Preferences is { } preferences
					? CreateOwnedVersionTrackingDependency(preferences, static () => Preferences.Default)
					: static () => CaptureVersionTrackingDependency(static () => Preferences.Default);
				Func<VersionTrackingDependency<IAppInfo>> getAppInfo = dependencies.AppInfo is { } appInfo
					? CreateOwnedVersionTrackingDependency(appInfo, static () => AppInfo.Current)
					: static () => CaptureVersionTrackingDependency(static () => AppInfo.Current);
				var implementation = new LazyVersionTracking(getPreferences, getAppInfo);
				TrackAndSet(
					implementation,
					VersionTracking.GetDefault,
					VersionTracking.SetDefault,
					facadeCleanups,
					allowsSharedOwnership: true);
			}

			static void BridgeAppInfoSecureStorageFromDI(
				IAppInfo? appInfo,
				ISecureStorage? secureStorage,
				List<Action> facadeCleanups)
			{
				if (appInfo is null || secureStorage is not null)
					return;

				// Install VersionTracking ownership before invoking the app-provided PackageName
				// getter, which may reentrantly access other static Essentials facades.
				// SecureStorage namespaces follow the bridged package name. Apps that change
				// PackageName must migrate existing secrets or register ISecureStorage.
				var secureStoragePackageName = appInfo.PackageName;
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
				var defaultAccessiblePredecessor = CaptureUnownedCustomPlatformSecureStoragePredecessor();
				global::Security.SecAccessible? inheritedDefaultAccessible = null;
				if (defaultAccessiblePredecessor is IPlatformSecureStorage platformSecureStorage)
					inheritedDefaultAccessible = platformSecureStorage.DefaultAccessible;

				TrackAndSet<ISecureStorage>(
					previous => new AppInfoSecureStorage(
						secureStoragePackageName,
						previous,
						ReferenceEquals(previous, defaultAccessiblePredecessor) &&
							IsUnownedCustomPlatformSecureStorage(previous)
								? inheritedDefaultAccessible
								: null),
					() => GetFacadeBackingField<ISecureStorage>(typeof(SecureStorage), "defaultImplementation"),
					SecureStorage.SetDefault,
					facadeCleanups);
#else
				TrackAndSet<ISecureStorage>(
					previous => new AppInfoSecureStorage(secureStoragePackageName, previous),
					() => GetFacadeBackingField<ISecureStorage>(typeof(SecureStorage), "defaultImplementation"),
					SecureStorage.SetDefault,
					facadeCleanups);
#endif
			}

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
			static ISecureStorage? CaptureUnownedCustomPlatformSecureStoragePredecessor()
			{
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<ISecureStorage>();
				lock (FacadeBridgeState<ISecureStorage>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						var current = GetFacadeBackingField<ISecureStorage>(
							typeof(SecureStorage),
							"defaultImplementation");

						return IsUnownedCustomPlatformSecureStorage(current)
							? current
							: null;
					}
				}
			}

			static bool IsUnownedCustomPlatformSecureStorage(ISecureStorage? implementation) =>
				implementation is IPlatformSecureStorage &&
				implementation is not AppInfoSecureStorage &&
				implementation is not SecureStorageImplementation &&
				FacadeBridgeState<ISecureStorage>.FindOwner(implementation) is null;
#endif

			/// <summary>
			/// Resolves a DI-registered implementation and assigns it to the corresponding static facade.
			/// The prior facade value is restored when the owning app is disposed, unless another app or
			/// internal caller replaced the facade in the meantime.
			/// </summary>
			static void BridgeIfRegistered<T>(
				IServiceProvider services,
				Func<T?> currentGetter,
				Action<T?> setter,
				List<Action> facadeCleanups)
				where T : class
			{
				var impl = services.GetService<T>();
				if (impl is not null)
					TrackAndSet(impl, currentGetter, setter, facadeCleanups);
			}

			static void TrackAndSet<T>(
				T impl,
				Func<T?> currentGetter,
				Action<T?> setter,
				List<Action> facadeCleanups,
				bool allowsSharedOwnership = false)
				where T : class
			{
				TrackAndSet(_ => impl, currentGetter, setter, facadeCleanups, allowsSharedOwnership);
			}

			static void TrackAndSet<T>(
				Func<T?, T> implementationFactory,
				Func<T?> currentGetter,
				Action<T?> setter,
				List<Action> facadeCleanups,
				bool allowsSharedOwnership = false)
				where T : class
			{
				FacadeAssignment<T> assignment;
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<T>();

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						var previous = currentGetter();
						var previousOwner = FacadeBridgeState<T>.FindOwner(previous);
						var impl = implementationFactory(previous);
						assignment = new FacadeAssignment<T>(
							impl,
							previous,
							previousOwner,
							allowsSharedOwnership);
						FacadeBridgeState<T>.Assignments.Add(assignment);
						SetFacade(setter, impl);
					}
				}

				AddFacadeCleanup(assignment, currentGetter, setter, facadeCleanups, facadeSyncRoot);
			}

			// Invariant: every bridge-owned get-current-owner -> set/restore sequence MUST hold
			// FacadeBridgeState<T>.SyncRoot. Facade setters are independently atomic so lazy fallback
			// initialization cannot overwrite a concurrent DI assignment, while this lock serializes
			// the ownership graph and predecessor restoration across overlapping MauiApp instances.
			static void SetFacade<T>(Action<T?> setter, T? value)
				where T : class
			{
				Debug.Assert(
					Monitor.IsEntered(FacadeBridgeState<T>.SyncRoot),
					"Bridged facade writes (SetDefault/SetCurrent) must hold FacadeBridgeState<T>.SyncRoot.");
				setter(value);
			}

			static T? GetFacadeBackingField<T>(
				[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)] Type facadeType,
				string fieldName)
				where T : class
			{
				var field = facadeType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
					?? throw new InvalidOperationException($"Field '{fieldName}' not found on '{facadeType.Name}'.");

				return (T?)field.GetValue(null);
			}

#if WINDOWS || TIZEN
			static IGeocoding? CaptureGeocodingDefaultForMapServiceToken(bool createIfMissing)
			{
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<IGeocoding>();
				lock (FacadeBridgeState<IGeocoding>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						return createIfMissing
							? Geocoding.Default
							: GetFacadeBackingField<IGeocoding>(typeof(Geocoding), "defaultImplementation");
					}
				}
			}

			static string? CaptureDirectUnownedMapServiceToken(IPlatformGeocoding implementation)
			{
				while (true)
				{
					if (IsFacadeBridgeOwnedGeocoding(implementation))
						return null;

					int epoch;
					lock (s_mapTokenLock)
					{
						var state = FindMapTokenImplementationState(implementation);
						if (state is not null)
						{
							if (state.BaseTokenInitialized)
								return IsFacadeBridgeOwnedGeocoding(implementation) ? null : state.BaseToken;

							Monitor.Wait(s_mapTokenLock);
							continue;
						}

						epoch = s_mapTokenEpoch;
					}

					var token = implementation.MapServiceToken;

					lock (s_mapTokenLock)
					{
						var state = FindMapTokenImplementationState(implementation);
						if (state is not null)
						{
							if (state.BaseTokenInitialized)
								return IsFacadeBridgeOwnedGeocoding(implementation) ? null : state.BaseToken;

							Monitor.Wait(s_mapTokenLock);
							continue;
						}

						if (epoch == s_mapTokenEpoch && !IsFacadeBridgeOwnedGeocoding(implementation))
							return token;
					}
				}
			}

			static bool IsFacadeBridgeOwnedGeocoding(IPlatformGeocoding implementation)
			{
				if (implementation is not IGeocoding geocoding)
					return false;

				lock (FacadeBridgeState<IGeocoding>.SyncRoot)
					return FacadeBridgeState<IGeocoding>.FindOwner(geocoding) is not null;
			}

			static void TrackAndSetMapServiceToken(
				IPlatformGeocoding implementation,
				string mapServiceToken,
				List<Action> facadeCleanups)
			{
				MapTokenImplementationState implementationState;
				lock (s_mapTokenLock)
				{
					implementationState = FindMapTokenImplementationState(implementation)
						?? new MapTokenImplementationState(implementation);
					if (!s_mapTokenImplementationStates.Contains(implementationState))
						s_mapTokenImplementationStates.Add(implementationState);
					implementationState.ActiveOperations++;
				}

				MapTokenAssignment assignment;
				try
				{
					var previousToken = implementation.MapServiceToken;
					lock (s_mapTokenLock)
					{
						if (!implementationState.BaseTokenInitialized)
						{
							implementationState.BaseToken = previousToken;
							implementationState.BaseTokenInitialized = true;
							Monitor.PulseAll(s_mapTokenLock);
						}
#if WINDOWS
						var previousPlatformToken = WindowsMapServiceTokenGetter();
						var previousPlatformOwner = FindMapTokenOwner(previousPlatformToken);
#endif
						assignment = new MapTokenAssignment(
							implementationState,
							mapServiceToken
#if WINDOWS
							, previousPlatformToken
							, previousPlatformOwner
#endif
						);

						s_mapTokenAssignments.Add(assignment);
						implementationState.Version++;
						s_mapTokenEpoch++;
					}
				}
				finally
				{
					ReleaseMapTokenImplementationState(implementationState);
				}

				try
				{
					ReconcileMapServiceToken(implementationState);
				}
				catch (Exception applyException)
				{
					List<Exception>? rollbackExceptions = null;
#if WINDOWS
					Exception? platformRollbackException = null;
#endif
					lock (s_mapTokenLock)
					{
						implementationState.ActiveOperations++;
#if WINDOWS
						var assignmentRemoved = RemoveMapTokenAssignmentLocked(
							assignment,
							out var platformSuccessorToken,
							out var platformPredecessorToken);
						if (assignmentRemoved)
						{
							platformRollbackException = RestoreWindowsMapServiceTokenLocked(
								assignment,
								platformSuccessorToken,
								platformPredecessorToken);
						}
#else
						RemoveMapTokenAssignmentLocked(assignment);
#endif
					}

#if WINDOWS
					if (platformRollbackException is not null)
						(rollbackExceptions ??= new()).Add(platformRollbackException);
#endif
					try
					{
						ReconcileMapServiceToken(implementationState);
					}
					catch (Exception ex)
					{
						(rollbackExceptions ??= new()).Add(ex);
					}
					finally
					{
						ReleaseMapTokenImplementationState(implementationState);
					}

					if (rollbackExceptions is not null)
					{
						rollbackExceptions.Insert(0, applyException);
						throw new AggregateException(
							"Map service token assignment and rollback both failed.",
							rollbackExceptions);
					}

					throw;
				}

				facadeCleanups.Add(() => CleanupMapServiceToken(assignment));
			}

			// Caller must hold s_mapTokenLock.
			static MapTokenImplementationState? FindMapTokenImplementationState(IPlatformGeocoding implementation)
			{
				for (int i = s_mapTokenImplementationStates.Count - 1; i >= 0; i--)
				{
					if (ReferenceEquals(s_mapTokenImplementationStates[i].Implementation, implementation))
						return s_mapTokenImplementationStates[i];
				}

				return null;
			}

			static void ReconcileMapServiceToken(MapTokenImplementationState implementationState)
			{
				lock (s_mapTokenLock)
					implementationState.ActiveOperations++;

				try
				{
					while (true)
					{
						int version;
						string? desiredToken;
						lock (s_mapTokenLock)
						{
							version = implementationState.Version;
							desiredToken = implementationState.BaseToken;
							for (int i = s_mapTokenAssignments.Count - 1; i >= 0; i--)
							{
								if (ReferenceEquals(
									s_mapTokenAssignments[i].ImplementationState,
									implementationState))
								{
									desiredToken = s_mapTokenAssignments[i].AppliedToken;
									break;
								}
							}
						}

						// IPlatformGeocoding is user-implementable. Never invoke its accessors
						// while holding the process-static bookkeeping lock.
						if (!string.Equals(
							implementationState.Implementation.MapServiceToken,
							desiredToken,
							StringComparison.Ordinal))
						{
							implementationState.Implementation.MapServiceToken = desiredToken;
						}

						lock (s_mapTokenLock)
						{
							// A concurrent mutation increments Version, so retries converge once the latest token is stable.
							if (implementationState.Version == version)
								return;
						}
					}
				}
				finally
				{
					ReleaseMapTokenImplementationState(implementationState);
				}
			}

			static void ReleaseMapTokenImplementationState(MapTokenImplementationState implementationState)
			{
				lock (s_mapTokenLock)
				{
					implementationState.ActiveOperations--;
					if (implementationState.ActiveOperations != 0)
						return;

					foreach (var assignment in s_mapTokenAssignments)
					{
						if (ReferenceEquals(assignment.ImplementationState, implementationState))
							return;
					}

					s_mapTokenImplementationStates.Remove(implementationState);
					Monitor.PulseAll(s_mapTokenLock);
				}
			}

#if WINDOWS
			// Caller must hold s_mapTokenLock.
			static MapTokenAssignment? FindMapTokenOwner(string? token)
			{
				for (int i = s_mapTokenAssignments.Count - 1; i >= 0; i--)
				{
					if (string.Equals(s_mapTokenAssignments[i].AppliedToken, token, StringComparison.Ordinal))
						return s_mapTokenAssignments[i];
				}

				return null;
			}

			// Caller must hold s_mapTokenLock so the Windows token and owner graph
			// cannot diverge while an assignment is removed or rebased.
			static Exception? RestoreWindowsMapServiceTokenLocked(
				MapTokenAssignment assignment,
				string? platformSuccessorToken,
				string? platformPredecessorToken)
			{
				Debug.Assert(Monitor.IsEntered(s_mapTokenLock));

				try
				{
					if (string.Equals(WindowsMapServiceTokenGetter(), assignment.AppliedToken, StringComparison.Ordinal))
					{
						WindowsMapServiceTokenSetter(
							platformSuccessorToken ??
							platformPredecessorToken ??
							assignment.PreviousPlatformToken);
					}
				}
				catch (Exception ex)
				{
					return ex;
				}

				return null;
			}
#endif

			static void CleanupMapServiceToken(MapTokenAssignment assignment)
			{
#if WINDOWS
				string? platformSuccessorToken;
				string? platformPredecessorToken;
				Exception? platformException = null;
#endif

				lock (s_mapTokenLock)
				{
					assignment.ImplementationState.ActiveOperations++;
					if (!RemoveMapTokenAssignmentLocked(
						assignment
#if WINDOWS
						, out platformSuccessorToken,
						out platformPredecessorToken
#endif
					))
					{
						ReleaseMapTokenImplementationState(assignment.ImplementationState);
						return;
					}

#if WINDOWS
					// The Windows token and its assignment-owner graph are both process-global.
					// Remove/rebase the graph and restore the platform token atomically before
					// a concurrent app can capture a stale token with no previous owner.
					platformException = RestoreWindowsMapServiceTokenLocked(
						assignment,
						platformSuccessorToken,
						platformPredecessorToken);
#endif
				}

				List<Exception>? exceptions = null;
				try
				{
					ReconcileMapServiceToken(assignment.ImplementationState);
				}
				catch (Exception ex)
				{
					(exceptions ??= new()).Add(ex);
				}
				finally
				{
					ReleaseMapTokenImplementationState(assignment.ImplementationState);
				}
#if WINDOWS
				if (platformException is not null)
					(exceptions ??= new()).Add(platformException);
#endif

				if (exceptions is null)
					return;

				if (exceptions.Count == 1)
					ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

				throw new AggregateException("Map service token restoration failed.", exceptions);
			}

			// Caller must hold s_mapTokenLock. This mutates bookkeeping only and never
			// invokes user/platform token accessors.
			static bool RemoveMapTokenAssignmentLocked(
				MapTokenAssignment assignment
#if WINDOWS
				, out string? platformSuccessorToken,
				out string? platformPredecessorToken
#endif
			)
			{
				var index = s_mapTokenAssignments.IndexOf(assignment);
				if (index < 0)
				{
#if WINDOWS
					platformSuccessorToken = null;
					platformPredecessorToken = null;
#endif
					return false;
				}

#if WINDOWS
				MapTokenAssignment? platformSuccessor = null;
				var platformPredecessor = index > 0
					? s_mapTokenAssignments[index - 1]
					: null;
				MapTokenAssignment? previousDependent = null;
				for (int i = index + 1; i < s_mapTokenAssignments.Count; i++)
				{
					var candidate = s_mapTokenAssignments[i];
					platformSuccessor = candidate;
					if (!ReferenceEquals(candidate.PreviousPlatformOwner, assignment))
						continue;

					if (previousDependent is null)
					{
						candidate.PreviousPlatformToken = assignment.PreviousPlatformToken;
						candidate.PreviousPlatformOwner = assignment.PreviousPlatformOwner;
					}
					else
					{
						candidate.PreviousPlatformToken = previousDependent.AppliedToken;
						candidate.PreviousPlatformOwner = previousDependent;
					}

					previousDependent = candidate;
				}
				platformSuccessorToken = platformSuccessor?.AppliedToken;
				platformPredecessorToken = platformPredecessor?.AppliedToken;
#endif

				s_mapTokenAssignments.RemoveAt(index);
				assignment.ImplementationState.Version++;
				s_mapTokenEpoch++;
				return true;
			}
#endif

			static void TrackInitializedOrOwned<T>(
				T impl,
				T? original,
				Func<T?> getter,
				Action<T?> setter,
				List<Action> facadeCleanups)
				where T : class
			{
				FacadeAssignment<T> assignment;
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<T>();

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						if (!ReferenceEquals(getter(), impl))
							return;

						var currentOwner = FacadeBridgeState<T>.FindOwner(impl);
						T? previous;
						FacadeAssignment<T>? previousOwner;
						if (currentOwner is null)
						{
							if (original is not null)
								return;

							previous = original;
							previousOwner = null;
						}
						else
						{
							// Direct DI implementations belong to the app that resolved them.
							// Only framework-owned defaults and owner-aware wrappers can be shared.
							if (!currentOwner.AllowsSharedOwnership)
								return;

							previous = impl;
							previousOwner = currentOwner;
						}

						assignment = new FacadeAssignment<T>(
							impl,
							previous,
							previousOwner,
							allowsSharedOwnership: true);
						FacadeBridgeState<T>.Assignments.Add(assignment);
					}
				}

				AddFacadeCleanup(assignment, getter, setter, facadeCleanups, facadeSyncRoot);
			}

			static Func<VersionTrackingDependency<T>> CreateOwnedVersionTrackingDependency<T>(
				T implementation,
				Func<T> fallbackGetter)
				where T : class
			{
				var reference = new WeakReference<T>(implementation);
				return () =>
				{
					lock (FacadeBridgeState<T>.SyncRoot)
					{
						if (reference.TryGetTarget(out var target) &&
							FacadeBridgeState<T>.FindOwner(target) is { } owner)
						{
							return new(target, owner);
						}

						var fallback = fallbackGetter();
						return new(fallback, FacadeBridgeState<T>.FindOwner(fallback));
					}
				};
			}

			static VersionTrackingDependency<T> CaptureVersionTrackingDependency<T>(Func<T> getter)
				where T : class
			{
				lock (FacadeBridgeState<T>.SyncRoot)
				{
					var implementation = getter();
					return new(implementation, FacadeBridgeState<T>.FindOwner(implementation));
				}
			}

			static void AddFacadeCleanup<T>(
				FacadeAssignment<T> assignment,
				Func<T?> getter,
				Action<T?> setter,
				List<Action> facadeCleanups,
				object facadeSyncRoot)
				where T : class
			{
				facadeCleanups.Add(() =>
				{
					lock (FacadeBridgeState<T>.SyncRoot)
					{
						lock (facadeSyncRoot)
						{
							// TrackAndSet holds the facade's lazy/setter lock across predecessor
							// capture and installation. Hold it again across current-owner verification
							// and restoration so neither a lazy fallback nor an external setter can be
							// overwritten by a stale predecessor.
							var index = FacadeBridgeState<T>.Assignments.IndexOf(assignment);
							if (index < 0)
								return;

							var current = getter();
							var ownsCurrent = ReferenceEquals(
								FacadeBridgeState<T>.FindOwner(current),
								assignment);

							foreach (var dependent in FacadeBridgeState<T>.Assignments)
							{
								if (ReferenceEquals(dependent.PreviousOwner, assignment))
									dependent.RebasePreviousOwner(assignment);
							}

							FacadeBridgeState<T>.Assignments.RemoveAt(index);
							if (ownsCurrent)
								SetFacade(setter, assignment.Previous);
						}
					}

					if (typeof(T) == typeof(IPreferences) ||
						typeof(T) == typeof(IAppInfo))
					{
						InvalidateLazyVersionTrackingStates();
					}
				});
			}

			static void InvalidateLazyVersionTrackingStates()
			{
				lock (FacadeBridgeState<IVersionTracking>.SyncRoot)
				{
					foreach (var assignment in FacadeBridgeState<IVersionTracking>.Assignments)
					{
						if (assignment.Implementation is LazyVersionTracking lazyVersionTracking)
							lazyVersionTracking.Invalidate();
					}
				}
			}

			sealed class FacadeAssignment<T> where T : class
			{
				public FacadeAssignment(
					T implementation,
					T? previous,
					FacadeAssignment<T>? previousOwner,
					bool allowsSharedOwnership)
				{
					Implementation = implementation;
					AllowsSharedOwnership = allowsSharedOwnership;
					SetPrevious(previous, previousOwner);
				}

				public T Implementation { get; }

				public bool AllowsSharedOwnership { get; }

				public T? Previous { get; private set; }

				public FacadeAssignment<T>? PreviousOwner { get; private set; }

				public void RebasePreviousOwner(FacadeAssignment<T> previousOwner)
				{
					Debug.Assert(ReferenceEquals(PreviousOwner, previousOwner));
					SetPrevious(previousOwner.Previous, previousOwner.PreviousOwner);
				}

				void SetPrevious(T? previous, FacadeAssignment<T>? previousOwner)
				{
					Debug.Assert(previousOwner is null || ReferenceEquals(previous, previousOwner.Implementation));
					Previous = previous;
					PreviousOwner = previousOwner;
				}
			}

			static class FacadeBridgeState<T> where T : class
			{
				// SyncRoot serializes the ownership bookkeeping and every bridge-owned facade
				// replacement/restoration. Facade lazy initialization and setters are independently
				// atomic, so external/internal writers cannot clobber a DI assignment with a stale
				// fallback while this lock preserves the overlapping-app predecessor graph.
				internal static readonly object SyncRoot = new();
				internal static readonly List<FacadeAssignment<T>> Assignments = new();

				internal static FacadeAssignment<T>? FindOwner(T? implementation)
				{
					for (int i = Assignments.Count - 1; i >= 0; i--)
					{
						if (ReferenceEquals(Assignments[i].Implementation, implementation))
							return Assignments[i];
					}

					return null;
				}
			}

			sealed class LazyVersionTracking : IVersionTracking
			{
				readonly object _sync = new();
				readonly Func<VersionTrackingDependency<IPreferences>> _getPreferences;
				readonly Func<VersionTrackingDependency<IAppInfo>> _getAppInfo;
				VersionTrackingState? _state;

				public LazyVersionTracking(
					Func<VersionTrackingDependency<IPreferences>> getPreferences,
					Func<VersionTrackingDependency<IAppInfo>> getAppInfo)
				{
					_getPreferences = getPreferences;
					_getAppInfo = getAppInfo;
				}

				IVersionTracking Implementation
				{
					get
					{
						// Intentionally does NOT take a process-global lock: doing so previously
						// deadlocked when a facade read raced an in-flight EssentialsInitializer that
						// held the global lock across user code. _sync guards this instance's cached
						// state; the dependency getters take the per-type FacadeBridgeState<T>.SyncRoot.
						lock (_sync)
						{
							var preferences = _getPreferences();
							var appInfo = _getAppInfo();

							if (_state is null ||
								!_state.Preferences.Matches(preferences) ||
								!_state.AppInfo.Matches(appInfo))
							{
								_state = new VersionTrackingState(
									new VersionTrackingImplementation(
										preferences.Implementation,
										appInfo.Implementation),
									preferences,
									appInfo);
							}

							return _state.Implementation;
						}
					}
				}

				public void Invalidate()
				{
					lock (_sync)
						_state = null;
				}

				public bool IsFirstLaunchEver => Implementation.IsFirstLaunchEver;

				public bool IsFirstLaunchForCurrentVersion => Implementation.IsFirstLaunchForCurrentVersion;

				public bool IsFirstLaunchForCurrentBuild => Implementation.IsFirstLaunchForCurrentBuild;

				public string CurrentVersion => Implementation.CurrentVersion;

				public string CurrentBuild => Implementation.CurrentBuild;

				public string? PreviousVersion => Implementation.PreviousVersion;

				public string? PreviousBuild => Implementation.PreviousBuild;

				public string? FirstInstalledVersion => Implementation.FirstInstalledVersion;

				public string? FirstInstalledBuild => Implementation.FirstInstalledBuild;

				public IReadOnlyList<string> VersionHistory => Implementation.VersionHistory;

				public IReadOnlyList<string> BuildHistory => Implementation.BuildHistory;

				public void Track() => Implementation.Track();

				public bool IsFirstLaunchForVersion(string version) =>
					Implementation.IsFirstLaunchForVersion(version);

				public bool IsFirstLaunchForBuild(string build) =>
					Implementation.IsFirstLaunchForBuild(build);
			}

			sealed class VersionTrackingState
			{
				public VersionTrackingState(
					IVersionTracking implementation,
					VersionTrackingDependency<IPreferences> preferences,
					VersionTrackingDependency<IAppInfo> appInfo)
				{
					Implementation = implementation;
					Preferences = preferences;
					AppInfo = appInfo;
				}

				public IVersionTracking Implementation { get; }

				public VersionTrackingDependency<IPreferences> Preferences { get; }

				public VersionTrackingDependency<IAppInfo> AppInfo { get; }
			}

			readonly struct VersionTrackingDependency<T> where T : class
			{
				public VersionTrackingDependency(T implementation, object? owner)
				{
					Implementation = implementation;
					Owner = owner;
				}

				public T Implementation { get; }

				public object? Owner { get; }

				public bool Matches(VersionTrackingDependency<T> other) =>
					ReferenceEquals(Implementation, other.Implementation) &&
					ReferenceEquals(Owner, other.Owner);
			}

			static void LogMissingNativeLifecycleInterface<T>(IServiceProvider services, string requiredInterface)
				where T : class =>
				services.GetService<ILoggerFactory>()?
					.CreateLogger<EssentialsInitializer>()
					.LogWarning(
						"DI-registered {ServiceType} was not bridged to its static facade because native lifecycle callbacks require {RequiredInterface}.",
						typeof(T).Name,
						requiredInterface);

			static void SetAppActions(
				IAppActions appActions,
				ILogger? logger,
				List<AppAction> actions,
				List<Action> preProviderCleanups)
			{
				// Build is synchronous and normally runs on the UI thread. Do not block here:
				// a custom implementation may need the native dispatcher to complete SetAsync.
				var assignment = new AppActionsSetAssignment(appActions, logger, new List<AppAction>(actions));
				lock (s_appActionsSetLock)
				{
					s_appActionsSetAssignments.Add(assignment);
					QueueAppActionsSetUnderLock(assignment);
				}

				preProviderCleanups.Add(() => RemoveAppActionsSetAssignment(assignment));
			}

			internal static async Task SetAppActionsAsync(IAppActions appActions, ILogger? logger, List<AppAction> actions)
			{
				try
				{
					await appActions.SetAsync(actions).ConfigureAwait(false);
				}
				catch (FeatureNotSupportedException ex)
				{
					logger?.LogError(ex, "App Actions are not supported on this platform.");
				}
				catch (Exception ex)
				{
					logger?.LogError(ex, "An error occurred while setting app actions.");
				}
			}

			static void RemoveAppActionsSetAssignment(AppActionsSetAssignment assignment)
			{
				lock (s_appActionsSetLock)
				{
					var index = s_appActionsSetAssignments.IndexOf(assignment);
					if (index < 0)
						return;

					var wasCurrent = index == s_appActionsSetAssignments.Count - 1;
					s_appActionsSetAssignments.RemoveAt(index);
					if (wasCurrent && s_appActionsSetAssignments.Count > 0)
						QueueAppActionsSetUnderLock(s_appActionsSetAssignments[s_appActionsSetAssignments.Count - 1]);
				}
			}

			static void QueueAppActionsSetUnderLock(AppActionsSetAssignment assignment)
			{
				Debug.Assert(Monitor.IsEntered(s_appActionsSetLock));
				s_appActionsSetTail = ApplyAppActionsSetAsync(s_appActionsSetTail, assignment);
			}

			static async Task ApplyAppActionsSetAsync(Task predecessor, AppActionsSetAssignment assignment)
			{
				await predecessor;
				// Never invoke an app-provided implementation while holding the assignment lock.
				await Task.Yield();

				lock (s_appActionsSetLock)
				{
					if (s_appActionsSetAssignments.Count == 0 ||
						!ReferenceEquals(s_appActionsSetAssignments[s_appActionsSetAssignments.Count - 1], assignment))
					{
						return;
					}
				}

				await SetAppActionsAsync(assignment.Implementation, assignment.Logger, assignment.Actions).ConfigureAwait(false);
			}

			static void Subscribe(
				IAppActions appActions,
				EventHandler<AppActionEventArgs> handler,
				List<Action> preProviderCleanups)
			{
				appActions.AppActionActivated += handler;
				preProviderCleanups.Add(() => appActions.AppActionActivated -= handler);
			}

			void HandleOnAppAction(object? sender, AppActionEventArgs e)
			{
				_essentialsBuilder?.AppActionHandlers?.Invoke(e.AppAction);
			}
		}

		sealed class AppActionsSetAssignment
		{
			public AppActionsSetAssignment(
				IAppActions implementation,
				ILogger? logger,
				List<AppAction> actions)
			{
				Implementation = implementation;
				Logger = logger;
				Actions = actions;
			}

			public IAppActions Implementation { get; }

			public ILogger? Logger { get; }

			public List<AppAction> Actions { get; }
		}

#if WINDOWS || TIZEN
		sealed class MapTokenImplementationState
		{
			public MapTokenImplementationState(IPlatformGeocoding implementation)
			{
				Implementation = implementation;
			}

			public IPlatformGeocoding Implementation { get; }

			public string? BaseToken { get; set; }

			public bool BaseTokenInitialized { get; set; }

			public int Version { get; set; }

			public int ActiveOperations { get; set; }
		}

		sealed class MapTokenAssignment
		{
			public MapTokenAssignment(
				MapTokenImplementationState implementationState,
				string appliedToken
#if WINDOWS
				, string? previousPlatformToken
				, MapTokenAssignment? previousPlatformOwner
#endif
			)
			{
				ImplementationState = implementationState;
				AppliedToken = appliedToken;
#if WINDOWS
				PreviousPlatformToken = previousPlatformToken;
				PreviousPlatformOwner = previousPlatformOwner;
#endif
			}

			public MapTokenImplementationState ImplementationState { get; }

			public IPlatformGeocoding Implementation => ImplementationState.Implementation;

			public string AppliedToken { get; }

#if WINDOWS
			public string? PreviousPlatformToken { get; set; }

			public MapTokenAssignment? PreviousPlatformOwner { get; set; }
#endif
		}
#endif

		sealed class EssentialsCleanup : IMauiAppCleanupService, IMauiAppPostProviderCleanupService
		{
			// Per-instance lock: each MauiApp owns its own EssentialsCleanup singleton, so cleanup
			// state is guarded per app rather than by a process-global lock.
			readonly object _sync = new();
			List<Action> _preProviderCleanups = new();
			List<Action> _facadeCleanups = new();
			bool _preProviderCleanedUp;
			bool _postProviderCleanedUp;

			public void SetCleanups(
				List<Action> preProviderCleanups,
				List<Action> facadeCleanups)
			{
				lock (_sync)
				{
					if (_preProviderCleanedUp || _postProviderCleanedUp)
						throw new ObjectDisposedException(nameof(EssentialsCleanup));

					_preProviderCleanups.AddRange(preProviderCleanups);
					_facadeCleanups.AddRange(facadeCleanups);
				}
			}

			void IMauiAppCleanupService.Cleanup()
			{
				List<Action> cleanups;
				lock (_sync)
				{
					if (_preProviderCleanedUp)
						return;

					_preProviderCleanedUp = true;
					cleanups = _preProviderCleanups;
					_preProviderCleanups = new();
				}

				RestoreFacadeCleanups(cleanups);
			}

			void IMauiAppPostProviderCleanupService.Cleanup()
			{
				List<Action> cleanups;
				lock (_sync)
				{
					if (_postProviderCleanedUp)
						return;

					_postProviderCleanedUp = true;
					cleanups = _facadeCleanups;
					_facadeCleanups = new();
				}

				RestoreFacadeCleanups(cleanups);
			}
		}

		class EssentialsBuilder : IEssentialsBuilder
		{
			List<AppAction>? _appActions;
			internal Action<AppAction>? AppActionHandlers;
			internal bool TrackVersions;

			internal List<AppAction>? AppActions => _appActions;

#pragma warning disable CS0414 // Remove unread private members
			internal bool HasMapServiceToken;
			internal string? MapServiceToken;
#pragma warning restore CS0414 // Remove unread private members

			public IEssentialsBuilder UseMapServiceToken(string token)
			{
				HasMapServiceToken = token is not null;
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
