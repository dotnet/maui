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
		static AppActionsSetAssignment? s_pendingAppActionsSetAssignment;
		static object? s_appActionsSetWorkerToken;
		static readonly object s_appInfoSecureStorageBridgeLock = new();

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
			static readonly List<DeferredAppInfoSecureStorageInstallation> s_deferredAppInfoSecureStorageInstallations = new();

			private readonly IEnumerable<EssentialsRegistration> _essentialsRegistrations;
			private readonly MauiAppInitializationState _initializationState;
			private EssentialsBuilder? _essentialsBuilder;

			public EssentialsInitializer(
				IEnumerable<EssentialsRegistration> essentialsRegistrations,
				MauiAppInitializationState initializationState)
			{
				_essentialsRegistrations = essentialsRegistrations;
				_initializationState = initializationState;
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
				var cleanup = services.GetRequiredService<EssentialsCleanup>();
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
						versionTrackingDependencies.SecureStorage,
						versionTrackingDependencies.FileSystem,
						versionTrackingDependencies.AppInfoOwner,
						versionTrackingDependencies.FileSystemOwner,
						versionTrackingDependencies.AppInfoSecureStoragePredecessor,
						facadeCleanups);

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
						if (_initializationState.IsInitialBuild)
						{
							// Failed Build() owns provider teardown. Run the pre-provider phase now
							// so its failures remain paired with this initializer failure, but keep
							// facade restoration in the post-provider phase so provider-owned async
							// disposables can still use their bridged facade. A repeated
							// initialization failure has no teardown owner and must rollback both
							// phases immediately.
							cleanup.SetCleanups(new List<Action>(), facadeCleanups);
							RestoreFacadeCleanups(preProviderCleanups);
						}
						else
						{
							RollbackInitialization(preProviderCleanups, facadeCleanups);
						}
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
				ISecureStorage? SecureStorage,
				IFileSystem? FileSystem,
				FacadeAssignment<IAppInfo>? AppInfoOwner,
				FacadeAssignment<IFileSystem>? FileSystemOwner,
				AppInfoSecureStoragePredecessor AppInfoSecureStoragePredecessor) BridgeEssentialsFromDI(
				IServiceProvider services,
				List<Action> facadeCleanups,
				IGeocoding? preResolvedGeocoding = null)
			{
				IPreferences? preferences = null;
				IAppInfo? appInfo = null;
				IVersionTracking? versionTracking = null;
				IGeocoding? geocoding = null;
				ISecureStorage? secureStorage = null;
				IFileSystem? fileSystem = null;
				FacadeAssignment<IAppInfo>? appInfoOwner = null;
				FacadeAssignment<IFileSystem>? fileSystemOwner = null;
				AppInfoSecureStoragePredecessor appInfoSecureStoragePredecessor = default;

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
				BridgeIfRegistered<IPasskeys>(services, () => GetFacadeBackingField<IPasskeys>(typeof(Passkeys), "defaultImplementation"), Passkeys.SetDefault, facadeCleanups);
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
				fileSystem = services.GetService<IFileSystem>();
				var fileSystemOwnsSecureStorage =
					fileSystem is not null &&
					SecureStorageImplementation.UsesFileSystemAppDataDirectory;
				if (secureStorage is null &&
					(appInfo is not null
#if WINDOWS
					|| fileSystemOwnsSecureStorage
#endif
					))
				{
					(appInfoOwner, fileSystemOwner, appInfoSecureStoragePredecessor) =
						TrackAndSetSecureStorageDependencies(
							appInfo,
							fileSystem,
							fileSystemOwnsSecureStorage,
							facadeCleanups);
				}
				else
				{
					if (appInfo is not null)
					{
						lock (s_appInfoSecureStorageBridgeLock)
						{
							appInfoOwner = TrackAndSet(
								appInfo,
								() => GetFacadeBackingField<IAppInfo>(
									typeof(AppInfo),
									"currentImplementation"),
								AppInfo.SetCurrent,
								facadeCleanups);
						}
					}

					if (fileSystem is not null)
					{
						var trackedFileSystemOwner = TrackAndSet(fileSystem, () => GetFacadeBackingField<IFileSystem>(typeof(FileSystem), "currentImplementation"), FileSystem.SetCurrent, facadeCleanups);
						if (fileSystemOwnsSecureStorage)
							fileSystemOwner = trackedFileSystemOwner;
					}
				}
				BridgeIfRegistered<IConnectivity>(services, () => GetFacadeBackingField<IConnectivity>(typeof(Connectivity), "currentImplementation"), Connectivity.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IDeviceDisplay>(services, () => GetFacadeBackingField<IDeviceDisplay>(typeof(DeviceDisplay), "currentImplementation"), DeviceDisplay.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IDeviceInfo>(services, () => GetFacadeBackingField<IDeviceInfo>(typeof(DeviceInfo), "currentImplementation"), DeviceInfo.SetCurrent, facadeCleanups);
				geocoding = preResolvedGeocoding ?? services.GetService<IGeocoding>();
				if (geocoding is not null)
					TrackAndSet(geocoding, () => GetFacadeBackingField<IGeocoding>(typeof(Geocoding), "defaultImplementation"), Geocoding.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IPermissions>(services, () => GetFacadeBackingField<IPermissions>(typeof(Permissions), "currentImplementation"), Permissions.SetCurrent, facadeCleanups);

				return (
					preferences,
					appInfo,
					versionTracking,
					geocoding,
					secureStorage,
					fileSystem,
					appInfoOwner,
					fileSystemOwner,
					appInfoSecureStoragePredecessor);
			}

			static void BridgeLazyVersionTrackingFromDI(
				(
					IPreferences? Preferences,
					IAppInfo? AppInfo,
					IVersionTracking? VersionTracking,
					IGeocoding? Geocoding,
					ISecureStorage? SecureStorage,
					IFileSystem? FileSystem,
					FacadeAssignment<IAppInfo>? AppInfoOwner,
					FacadeAssignment<IFileSystem>? FileSystemOwner,
					AppInfoSecureStoragePredecessor AppInfoSecureStoragePredecessor) dependencies,
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
				ISecureStorage? secureStorage,
				IFileSystem? fileSystem,
				FacadeAssignment<IAppInfo>? appInfoOwner,
				FacadeAssignment<IFileSystem>? fileSystemOwner,
				AppInfoSecureStoragePredecessor predecessor,
				List<Action> facadeCleanups)
			{
				if (secureStorage is not null ||
					(appInfoOwner is null
#if WINDOWS
					&& fileSystemOwner is null
#endif
					))
					return;

				// Implicit SecureStorage retains the native package identity so bridging a custom
				// IAppInfo cannot silently move existing secrets to a different namespace. Apps
				// that need a custom namespace must register ISecureStorage explicitly.
#if WINDOWS
				var appDataDirectory = SecureStorageImplementation.UsesFileSystemAppDataDirectory
					? fileSystem?.AppDataDirectory ?? FileSystemImplementation.GetDefaultAppDataDirectory()
					: null;
#endif
				var secureStoragePackageName = SecureStorageImplementation.GetDefaultPackageName();

				// Custom IFileSystem getters can yield while a newer app takes facade ownership.
				// Installation revalidates every app-owned dependency and the SecureStorage
				// predecessor under their facade locks before publishing the wrapper.
				var installation = new DeferredAppInfoSecureStorageInstallation(
					appInfoOwner,
					fileSystemOwner,
					secureStoragePackageName,
					predecessor.Facade
#if WINDOWS
					, appDataDirectory
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					,
					predecessor.DefaultAccessiblePredecessor,
					predecessor.InheritedDefaultAccessible
#endif
					);
				facadeCleanups.Add(() => CleanupDeferredAppInfoSecureStorageInstallation(installation));

				lock (s_appInfoSecureStorageBridgeLock)
				{
					if (!TryInstallAppInfoSecureStorage(installation))
						s_deferredAppInfoSecureStorageInstallations.Add(installation);
				}
			}

			static void ReconcileDeferredAppInfoSecureStorageInstallation()
			{
				lock (s_appInfoSecureStorageBridgeLock)
				{
					for (int i = s_deferredAppInfoSecureStorageInstallations.Count - 1; i >= 0; i--)
					{
						var installation = s_deferredAppInfoSecureStorageInstallations[i];
						if (TryInstallAppInfoSecureStorage(installation))
						{
							s_deferredAppInfoSecureStorageInstallations.RemoveAt(i);
							return;
						}
					}
				}
			}

			static bool TryInstallAppInfoSecureStorage(
				DeferredAppInfoSecureStorageInstallation installation)
			{
				Debug.Assert(Monitor.IsEntered(s_appInfoSecureStorageBridgeLock));
				Debug.Assert(installation.SecureStorageCleanup is null);

				if (installation.AppInfoOwner is null)
					return TryInstallAppInfoSecureStorageWithFileSystemOwner(installation);

				var appInfoFacadeSyncRoot = EssentialsImplementation.GetSyncRoot<IAppInfo>();
				lock (FacadeBridgeState<IAppInfo>.SyncRoot)
				{
					lock (appInfoFacadeSyncRoot)
					{
						var current = GetFacadeBackingField<IAppInfo>(
							typeof(AppInfo),
							"currentImplementation");
						if (!ReferenceEquals(
							FacadeBridgeState<IAppInfo>.FindOwner(current),
							installation.AppInfoOwner))
						{
							return false;
						}

						return TryInstallAppInfoSecureStorageWithFileSystemOwner(installation);
					}
				}
			}

			static bool TryInstallAppInfoSecureStorageWithFileSystemOwner(
				DeferredAppInfoSecureStorageInstallation installation)
			{
#if WINDOWS
				if (installation.FileSystemOwner is not null)
				{
					var fileSystemFacadeSyncRoot = EssentialsImplementation.GetSyncRoot<IFileSystem>();
					lock (FacadeBridgeState<IFileSystem>.SyncRoot)
					{
						lock (fileSystemFacadeSyncRoot)
						{
							var current = GetFacadeBackingField<IFileSystem>(
								typeof(FileSystem),
								"currentImplementation");
							if (!ReferenceEquals(
								FacadeBridgeState<IFileSystem>.FindOwner(current),
								installation.FileSystemOwner))
							{
								return false;
							}

							return TryInstallAppInfoSecureStorageCore(installation);
						}
					}
				}
#endif

				return TryInstallAppInfoSecureStorageCore(installation);
			}

			static bool TryInstallAppInfoSecureStorageCore(
				DeferredAppInfoSecureStorageInstallation installation)
			{
				NormalizeDeferredAppInfoSecureStoragePredecessor(installation);
				var secureStorageCleanups = new List<Action>();
				// No user code runs below. Hold every app-owned dependency lock until SecureStorage
				// is published so direct facade setters cannot split the derived assignment.
				// A newer SecureStorage-only app retains precedence; its cleanup retries
				// reconciliation after removing that newer assignment.
				if (!TrackAndSetAppInfoSecureStorage(
					installation.PackageName,
					installation.PredecessorBeforeAppInfo,
					secureStorageCleanups
#if WINDOWS
					, installation.AppDataDirectory
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					, installation.DefaultAccessiblePredecessor,
					installation.InheritedDefaultAccessible
#endif
					, preserveNewerAssignments: true
					))
				{
					return false;
				}

				Debug.Assert(secureStorageCleanups.Count == 1);
				installation.SecureStorageCleanup = secureStorageCleanups[0];
				installation.ReleasePredecessor();
				return true;
			}

			static void NormalizeDeferredAppInfoSecureStoragePredecessor(
				DeferredAppInfoSecureStorageInstallation installation)
			{
				lock (FacadeBridgeState<ISecureStorage>.SyncRoot)
				{
					while (installation.PredecessorBeforeAppInfo.Owner is { } owner &&
						!FacadeBridgeState<ISecureStorage>.Assignments.Contains(owner))
					{
						installation.RebasePredecessor(owner);
					}
				}
			}

			static void CleanupDeferredAppInfoSecureStorageInstallation(
				DeferredAppInfoSecureStorageInstallation deferred)
			{
				Action? secureStorageCleanup;
				lock (s_appInfoSecureStorageBridgeLock)
				{
					s_deferredAppInfoSecureStorageInstallations.Remove(deferred);
					secureStorageCleanup = deferred.SecureStorageCleanup;
					deferred.SecureStorageCleanup = null;
				}

				secureStorageCleanup?.Invoke();
			}

			static bool TrackAndSetAppInfoSecureStorage(
				string packageName,
				FacadePredecessor<ISecureStorage> predecessorBeforeAppInfo,
				List<Action> facadeCleanups
#if WINDOWS
				, string? appDataDirectory
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
				, ISecureStorage? defaultAccessiblePredecessor,
				global::Security.SecAccessible? inheritedDefaultAccessible
#endif
				, bool preserveNewerAssignments = false
				)
			{
				FacadeAssignment<ISecureStorage> assignment;
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<ISecureStorage>();

				lock (FacadeBridgeState<ISecureStorage>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						var current = GetFacadeBackingField<ISecureStorage>(
							typeof(SecureStorage),
							"defaultImplementation");
						var currentOwner = FacadeBridgeState<ISecureStorage>.FindOwner(current);
						var expectedAlias = Preferences.GetPrivatePreferencesSharedName(
							packageName,
							"preferences");
						var initializedDuringHandoff =
							predecessorBeforeAppInfo.Implementation is null &&
							predecessorBeforeAppInfo.Owner is null &&
							currentOwner is null &&
							current is SecureStorageImplementation implementation &&
							string.Equals(implementation.Alias, expectedAlias, StringComparison.Ordinal);
						if (preserveNewerAssignments &&
							!initializedDuringHandoff &&
							!IsAtOrBeforeFacadePredecessor(
								current,
								currentOwner,
								predecessorBeforeAppInfo))
						{
							return false;
						}

						var previous = initializedDuringHandoff
							? null
							: current;
						var previousOwner = initializedDuringHandoff
							? null
							: currentOwner;
						var appInfoSecureStorage = new AppInfoSecureStorage(
							packageName,
							previous
#if WINDOWS
							, appDataDirectory
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
							, ReferenceEquals(current, defaultAccessiblePredecessor) &&
								IsUnownedCustomPlatformSecureStorage(current)
									? inheritedDefaultAccessible
									: null
#endif
							);

						assignment = new FacadeAssignment<ISecureStorage>(
							appInfoSecureStorage,
							previous,
							previousOwner,
							allowsSharedOwnership: false);
						FacadeBridgeState<ISecureStorage>.Assignments.Add(assignment);
						SetFacade<ISecureStorage>(SecureStorage.SetDefault, appInfoSecureStorage);
					}
				}

				AddFacadeCleanup(
					assignment,
					() => GetFacadeBackingField<ISecureStorage>(
						typeof(SecureStorage),
						"defaultImplementation"),
					SecureStorage.SetDefault,
					facadeCleanups,
					facadeSyncRoot);
				return true;
			}

			static bool IsAtOrBeforeFacadePredecessor<T>(
				T? current,
				FacadeAssignment<T>? currentOwner,
				FacadePredecessor<T> predecessor)
				where T : class
			{
				for (var owner = predecessor.Owner; owner is not null; owner = owner.PreviousOwner)
				{
					if (ReferenceEquals(owner, currentOwner))
						return true;
				}

				if (currentOwner is not null)
					return false;

				if (predecessor.Owner is null)
					return ReferenceEquals(current, predecessor.Implementation);

				var rootOwner = predecessor.Owner;
				while (rootOwner.PreviousOwner is not null)
					rootOwner = rootOwner.PreviousOwner;

				return ReferenceEquals(current, rootOwner.Previous);
			}

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
			static AppInfoSecureStoragePredecessor CaptureAppInfoSecureStoragePredecessor()
			{
				while (true)
				{
					var (predecessor, defaultAccessiblePredecessor) =
						CaptureAppInfoSecureStorageFacadeSnapshot();
					if (defaultAccessiblePredecessor is not IPlatformSecureStorage platformSecureStorage)
					{
						return new(predecessor);
					}

					// Custom accessibility is app code. Read it after releasing facade locks,
					// then verify the same unowned predecessor is still current.
					var inheritedDefaultAccessible = platformSecureStorage.DefaultAccessible;
					var (revalidated, revalidatedDefaultAccessiblePredecessor) =
						CaptureAppInfoSecureStorageFacadeSnapshot();
					if (ReferenceEquals(predecessor.Implementation, revalidated.Implementation) &&
						ReferenceEquals(predecessor.Owner, revalidated.Owner) &&
						ReferenceEquals(
							defaultAccessiblePredecessor,
							revalidatedDefaultAccessiblePredecessor))
					{
						return new(
							predecessor,
							defaultAccessiblePredecessor,
							inheritedDefaultAccessible);
					}
				}
			}

			static (
				FacadePredecessor<ISecureStorage> Facade,
				ISecureStorage? DefaultAccessiblePredecessor)
				CaptureAppInfoSecureStorageFacadeSnapshot()
			{
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<ISecureStorage>();
				lock (FacadeBridgeState<ISecureStorage>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						var current = GetFacadeBackingField<ISecureStorage>(
							typeof(SecureStorage),
							"defaultImplementation");
						var facade = new FacadePredecessor<ISecureStorage>(
							current,
							FacadeBridgeState<ISecureStorage>.FindOwner(current));
						return (
							facade,
							FindUnownedCustomPlatformSecureStoragePredecessor(facade));
					}
				}
			}

			static ISecureStorage? FindUnownedCustomPlatformSecureStoragePredecessor(
				FacadePredecessor<ISecureStorage> predecessor)
			{
				Debug.Assert(Monitor.IsEntered(FacadeBridgeState<ISecureStorage>.SyncRoot));

				ISecureStorage? implementation;
				if (predecessor.Owner is null)
				{
					implementation = predecessor.Implementation;
				}
				else
				{
					var rootOwner = predecessor.Owner;
					while (rootOwner.PreviousOwner is not null)
						rootOwner = rootOwner.PreviousOwner;

					implementation = rootOwner.Previous;
				}

				return implementation is IPlatformSecureStorage &&
					implementation is not AppInfoSecureStorage &&
					implementation is not SecureStorageImplementation
						? implementation
						: null;
			}
#else
			static AppInfoSecureStoragePredecessor CaptureAppInfoSecureStoragePredecessor() =>
				new(CaptureFacadePredecessor(
					() => GetFacadeBackingField<ISecureStorage>(
						typeof(SecureStorage),
						"defaultImplementation")));
#endif

			static (
				FacadeAssignment<IAppInfo>? AppInfoOwner,
				FacadeAssignment<IFileSystem>? FileSystemOwner,
				AppInfoSecureStoragePredecessor SecureStoragePredecessor)
				TrackAndSetSecureStorageDependencies(
					IAppInfo? appInfo,
					IFileSystem? fileSystem,
					bool fileSystemOwnsSecureStorage,
					List<Action> facadeCleanups)
			{
				var secureStorageFacadeSyncRoot = EssentialsImplementation.GetSyncRoot<ISecureStorage>();
				while (true)
				{
					// Apple custom accessibility can execute app code, so capture it before
					// entering the bridge gate and revalidate the exact facade predecessor below.
					var predecessor = CaptureAppInfoSecureStoragePredecessor();

					lock (s_appInfoSecureStorageBridgeLock)
					{
						// The bridge gate serializes this SecureStorage -> dependencies acquisition
						// with the dependency -> SecureStorage order used by deferred installation.
						lock (FacadeBridgeState<ISecureStorage>.SyncRoot)
						{
							lock (secureStorageFacadeSyncRoot)
							{
								var current = GetFacadeBackingField<ISecureStorage>(
									typeof(SecureStorage),
									"defaultImplementation");
								var currentOwner = FacadeBridgeState<ISecureStorage>.FindOwner(current);
								if (!predecessor.Matches(current, currentOwner))
									continue;

								FacadeAssignment<IAppInfo>? appInfoOwner = null;
								if (appInfo is not null)
								{
									appInfoOwner = TrackAndSet(
										appInfo,
										() => GetFacadeBackingField<IAppInfo>(
											typeof(AppInfo),
											"currentImplementation"),
										AppInfo.SetCurrent,
										facadeCleanups);
								}

								FacadeAssignment<IFileSystem>? fileSystemOwner = null;
								if (fileSystem is not null)
								{
									var trackedFileSystemOwner = TrackAndSet(
										fileSystem,
										() => GetFacadeBackingField<IFileSystem>(
											typeof(FileSystem),
											"currentImplementation"),
										FileSystem.SetCurrent,
										facadeCleanups);
									if (fileSystemOwnsSecureStorage)
										fileSystemOwner = trackedFileSystemOwner;
								}

								return (appInfoOwner, fileSystemOwner, predecessor);
							}
						}
					}
				}
			}

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
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

			static FacadePredecessor<T> CaptureFacadePredecessor<T>(Func<T?> getter)
				where T : class
			{
				var facadeSyncRoot = EssentialsImplementation.GetSyncRoot<T>();
				lock (FacadeBridgeState<T>.SyncRoot)
				{
					lock (facadeSyncRoot)
					{
						var implementation = getter();
						return new(
							implementation,
							FacadeBridgeState<T>.FindOwner(implementation));
					}
				}
			}

			static FacadeAssignment<T> TrackAndSet<T>(
				T impl,
				Func<T?> currentGetter,
				Action<T?> setter,
				List<Action> facadeCleanups,
				bool allowsSharedOwnership = false)
				where T : class
			{
				return TrackAndSet(_ => impl, currentGetter, setter, facadeCleanups, allowsSharedOwnership);
			}

			static FacadeAssignment<T> TrackAndSet<T>(
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
				return assignment;
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
					// Any dependency or SecureStorage ownership change can make a deferred
					// derived SecureStorage assignment eligible for installation.
					if (typeof(T) == typeof(IAppInfo) ||
						typeof(T) == typeof(IFileSystem) ||
						typeof(T) == typeof(ISecureStorage))
					{
						lock (s_appInfoSecureStorageBridgeLock)
						{
							CleanupFacadeAssignment(
								assignment,
								getter,
								setter,
								facadeSyncRoot);
							ReconcileDeferredAppInfoSecureStorageInstallation();
						}
					}
					else
					{
						CleanupFacadeAssignment(
							assignment,
							getter,
							setter,
							facadeSyncRoot);
					}

					if (typeof(T) == typeof(IPreferences) ||
						typeof(T) == typeof(IAppInfo))
					{
						InvalidateLazyVersionTrackingStates();
					}
				});
			}

			static void CleanupFacadeAssignment<T>(
				FacadeAssignment<T> assignment,
				Func<T?> getter,
				Action<T?> setter,
				object facadeSyncRoot)
				where T : class
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

						if (typeof(T) == typeof(ISecureStorage))
						{
							RebaseDeferredAppInfoSecureStoragePredecessors(
								(FacadeAssignment<ISecureStorage>)(object)assignment);
						}

						FacadeBridgeState<T>.Assignments.RemoveAt(index);
						if (ownsCurrent)
							SetFacade(setter, assignment.Previous);
					}
				}

				static void RebaseDeferredAppInfoSecureStoragePredecessors(
					FacadeAssignment<ISecureStorage> previousOwner)
				{
					Debug.Assert(Monitor.IsEntered(s_appInfoSecureStorageBridgeLock));

					foreach (var installation in s_deferredAppInfoSecureStorageInstallations)
						installation.RebasePredecessor(previousOwner);
				}
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

			readonly struct FacadePredecessor<T> where T : class
			{
				public FacadePredecessor(T? implementation, FacadeAssignment<T>? owner)
				{
					Implementation = implementation;
					Owner = owner;
				}

				public T? Implementation { get; }

				public FacadeAssignment<T>? Owner { get; }
			}

			readonly struct AppInfoSecureStoragePredecessor
			{
				public AppInfoSecureStoragePredecessor(
					FacadePredecessor<ISecureStorage> facade
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					, ISecureStorage? defaultAccessiblePredecessor = null,
					global::Security.SecAccessible? inheritedDefaultAccessible = null
#endif
					)
				{
					Facade = facade;
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					DefaultAccessiblePredecessor = defaultAccessiblePredecessor;
					InheritedDefaultAccessible = inheritedDefaultAccessible;
#endif
				}

				public FacadePredecessor<ISecureStorage> Facade { get; }

				public bool Matches(
					ISecureStorage? implementation,
					FacadeAssignment<ISecureStorage>? owner)
				{
					if (!ReferenceEquals(Facade.Implementation, implementation) ||
						!ReferenceEquals(Facade.Owner, owner))
					{
						return false;
					}

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					return ReferenceEquals(
						DefaultAccessiblePredecessor,
						FindUnownedCustomPlatformSecureStoragePredecessor(
							new(implementation, owner)));
#else
					return true;
#endif
				}

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
				public ISecureStorage? DefaultAccessiblePredecessor { get; }

				public global::Security.SecAccessible? InheritedDefaultAccessible { get; }
#endif
			}

			sealed class DeferredAppInfoSecureStorageInstallation
			{
				public DeferredAppInfoSecureStorageInstallation(
					FacadeAssignment<IAppInfo>? appInfoOwner,
					FacadeAssignment<IFileSystem>? fileSystemOwner,
					string packageName,
					FacadePredecessor<ISecureStorage> predecessorBeforeAppInfo
#if WINDOWS
					, string? appDataDirectory
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					, ISecureStorage? defaultAccessiblePredecessor,
					global::Security.SecAccessible? inheritedDefaultAccessible
#endif
					)
				{
					AppInfoOwner = appInfoOwner;
					FileSystemOwner = fileSystemOwner;
					PackageName = packageName;
					PredecessorBeforeAppInfo = predecessorBeforeAppInfo;
#if WINDOWS
					AppDataDirectory = appDataDirectory;
#endif
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					DefaultAccessiblePredecessor = defaultAccessiblePredecessor;
					InheritedDefaultAccessible = inheritedDefaultAccessible;
#endif
				}

				public FacadeAssignment<IAppInfo>? AppInfoOwner { get; }

				public FacadeAssignment<IFileSystem>? FileSystemOwner { get; }

				public string PackageName { get; }

				public FacadePredecessor<ISecureStorage> PredecessorBeforeAppInfo { get; private set; }

#if WINDOWS
				public string? AppDataDirectory { get; }
#endif

#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
				public ISecureStorage? DefaultAccessiblePredecessor { get; private set; }

				public global::Security.SecAccessible? InheritedDefaultAccessible { get; private set; }
#endif

				public Action? SecureStorageCleanup { get; set; }

				public void RebasePredecessor(
					FacadeAssignment<ISecureStorage> previousOwner)
				{
					if (!ReferenceEquals(PredecessorBeforeAppInfo.Owner, previousOwner))
						return;

					PredecessorBeforeAppInfo = new(
						previousOwner.Previous,
						previousOwner.PreviousOwner);
				}

				public void ReleasePredecessor()
				{
					PredecessorBeforeAppInfo = default;
#if IOS || MACCATALYST || MACOS || TVOS || WATCHOS
					DefaultAccessiblePredecessor = null;
					InheritedDefaultAccessible = null;
#endif
				}
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
					assignment.IsRemoved = true;
					s_appActionsSetAssignments.RemoveAt(index);
					if (wasCurrent)
					{
						if (s_appActionsSetAssignments.Count > 0)
							QueueAppActionsSetUnderLock(s_appActionsSetAssignments[s_appActionsSetAssignments.Count - 1]);
						else
							CancelPendingAppActionsSetUnderLock();
					}
				}
			}

			static void CancelPendingAppActionsSetUnderLock()
			{
				Debug.Assert(Monitor.IsEntered(s_appActionsSetLock));

				// App actions are persistent OS launch affordances, not provider-owned state.
				// Keep the last published set across MauiApp disposal so it can launch the next
				// app instance; SetAsync(empty) here would erase Android shortcuts, iOS shortcut
				// items, and the Windows jump list during normal application shutdown.
				s_pendingAppActionsSetAssignment = null;
			}

			static void QueueAppActionsSetUnderLock(AppActionsSetAssignment assignment)
			{
				Debug.Assert(Monitor.IsEntered(s_appActionsSetLock));
				s_pendingAppActionsSetAssignment = assignment;
				StartAppActionsSetWorkerUnderLock();
			}

			static void StartAppActionsSetWorkerUnderLock()
			{
				Debug.Assert(Monitor.IsEntered(s_appActionsSetLock));
				if (s_pendingAppActionsSetAssignment is not null &&
					s_appActionsSetWorkerToken is null)
				{
					var workerToken = new object();
					s_appActionsSetWorkerToken = workerToken;
					_ = RunAppActionsSetWorkerAsync(workerToken);
				}
			}

			static async Task RunAppActionsSetWorkerAsync(object workerToken)
			{
				try
				{
					// Never invoke an app-provided implementation while holding the assignment lock.
					await Task.Yield();

					while (true)
					{
						AppActionsSetAssignment? assignment;
						lock (s_appActionsSetLock)
						{
							if (!ReferenceEquals(s_appActionsSetWorkerToken, workerToken))
								return;

							assignment = s_pendingAppActionsSetAssignment;
							s_pendingAppActionsSetAssignment = null;
							if (assignment is null)
								return;

							if (assignment.IsRemoved ||
								s_appActionsSetAssignments.Count == 0 ||
								!ReferenceEquals(s_appActionsSetAssignments[s_appActionsSetAssignments.Count - 1], assignment))
							{
								continue;
							}

						}

						try
						{
							// IAppActions writes process-global OS state and cannot be cancelled.
							// Keep the sole worker attached until this write finishes so a stale
							// completion can never overwrite a newer owner's publication.
							await SetAppActionsAsync(
								assignment.Implementation,
								assignment.Logger,
								assignment.Actions).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							Trace.TraceError($"AppActions failure logging threw an exception: {ex}");
						}
					}
				}
				finally
				{
					lock (s_appActionsSetLock)
					{
						if (ReferenceEquals(s_appActionsSetWorkerToken, workerToken))
						{
							s_appActionsSetWorkerToken = null;
							StartAppActionsSetWorkerUnderLock();
						}
					}
				}
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

			// Guarded by s_appActionsSetLock.
			public bool IsRemoved { get; set; }
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
