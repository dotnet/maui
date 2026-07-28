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
		// Serializes access to the shared map-token bookkeeping (s_mapTokenAssignments) only.
		// It is intentionally NOT held across user code (ConfigureEssentials delegates, DI factory
		// resolution) or facade reads. Facade ownership bookkeeping is serialized per type by
		// FacadeBridgeState<T>.SyncRoot instead. See EssentialsInitializer.InitializeCore.
#if WINDOWS || TIZEN
		static readonly object s_mapTokenLock = new();
#endif
#if WINDOWS || TIZEN
		static readonly List<MapTokenAssignment> s_mapTokenAssignments = new();
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
					var mapServiceToken = _essentialsBuilder.MapServiceToken;
					if (mapServiceToken is null && GetFacadeBackingField<IGeocoding>(typeof(Geocoding), "defaultImplementation") is IPlatformGeocoding existingPlatformGeocoding)
						mapServiceToken = existingPlatformGeocoding.MapServiceToken;
#endif

					var versionTrackingDependencies = BridgeEssentialsFromDI(services, facadeCleanups);
					var versionTrackingOwnedByApp =
						versionTrackingDependencies.Preferences is not null ||
						versionTrackingDependencies.AppInfo is not null ||
						versionTrackingDependencies.VersionTracking is not null;
					BridgeLazyVersionTrackingFromDI(versionTrackingDependencies, facadeCleanups);

					// Resolve app-owned cleanup before registering AppActions handlers. Facade
					// actions are appended after initialization has accumulated the complete batch.
					cleanup = services.GetRequiredService<EssentialsCleanup>();

#if WINDOWS || TIZEN
					// A ConfigureEssentials token takes precedence; otherwise preserve a token set
					// directly through ApplicationModel.Platform.MapServiceToken before MauiApp.Build().
					if (mapServiceToken is not null)
					{
						var geocoding = Geocoding.Default;
						if (geocoding is IPlatformGeocoding platformGeocoding)
						{
							TrackAndSetMapServiceToken(platformGeocoding, mapServiceToken, facadeCleanups);
						}
						else
						{
							services.GetService<ILoggerFactory>()?
								.CreateLogger<EssentialsInitializer>()
								.LogWarning(
									"Configured map service token was not applied because {ImplementationType} does not implement {RequiredInterface}.",
									geocoding.GetType().FullName,
									nameof(IPlatformGeocoding));
						}
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
							cleanup.Subscribe(appActions, HandleOnAppAction);

						if (_essentialsBuilder.AppActions is not null)
						{
							var logger = services.GetService<ILoggerFactory>()?.CreateLogger<IEssentialsBuilder>();
							SetAppActions(appActions, logger, _essentialsBuilder.AppActions);
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

					cleanup.SetFacadeCleanups(facadeCleanups);
				}
				catch (Exception initializationException)
				{
					try
					{
						if (cleanup is not null)
						{
							cleanup.SetFacadeCleanups(facadeCleanups);
							cleanup.Cleanup();
						}
						else
							RestoreFacadeCleanups(facadeCleanups);
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
			static (IPreferences? Preferences, IAppInfo? AppInfo, IVersionTracking? VersionTracking) BridgeEssentialsFromDI(
				IServiceProvider services,
				List<Action> facadeCleanups)
			{
				IPreferences? preferences = null;
				IAppInfo? appInfo = null;
				IVersionTracking? versionTracking = null;

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
				BridgeIfRegistered<ISecureStorage>(services, () => GetFacadeBackingField<ISecureStorage>(typeof(SecureStorage), "defaultImplementation"), SecureStorage.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISemanticScreenReader>(services, () => GetFacadeBackingField<ISemanticScreenReader>(typeof(SemanticScreenReader), "defaultImplementation"), SemanticScreenReader.SetDefault, facadeCleanups);
				BridgeIfRegistered<IShare>(services, () => GetFacadeBackingField<IShare>(typeof(Share), "defaultImplementation"), Share.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISms>(services, () => GetFacadeBackingField<ISms>(typeof(Sms), "defaultImplementation"), Sms.SetDefault, facadeCleanups);
				BridgeIfRegistered<ITextToSpeech>(services, () => GetFacadeBackingField<ITextToSpeech>(typeof(TextToSpeech), "defaultImplementation"), TextToSpeech.SetDefault, facadeCleanups);
				versionTracking = services.GetService<IVersionTracking>();
				if (versionTracking is not null)
					TrackAndSet(versionTracking, VersionTracking.GetDefault, VersionTracking.SetDefault, facadeCleanups);
				BridgeIfRegistered<IVibration>(services, () => GetFacadeBackingField<IVibration>(typeof(Vibration), "defaultImplementation"), Vibration.SetDefault, facadeCleanups);
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
				BridgeIfRegistered<IGeocoding>(services, () => GetFacadeBackingField<IGeocoding>(typeof(Geocoding), "defaultImplementation"), Geocoding.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IPermissions>(services, () => GetFacadeBackingField<IPermissions>(typeof(Permissions), "currentImplementation"), Permissions.SetCurrent, facadeCleanups);

				return (preferences, appInfo, versionTracking);
			}

			static void BridgeLazyVersionTrackingFromDI(
				(IPreferences? Preferences, IAppInfo? AppInfo, IVersionTracking? VersionTracking) dependencies,
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
				FacadeAssignment<T> assignment;

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					var previous = currentGetter();
					var previousOwner = FacadeBridgeState<T>.FindOwner(previous);
					assignment = new FacadeAssignment<T>(
						impl,
						previous,
						previousOwner,
						allowsSharedOwnership);
					FacadeBridgeState<T>.Assignments.Add(assignment);
					SetFacade(setter, impl);
				}

				AddFacadeCleanup(assignment, currentGetter, setter, facadeCleanups);
			}

			// Invariant: every write to a bridged static facade (SetDefault/SetCurrent) MUST hold
			// FacadeBridgeState<T>.SyncRoot. The facade setters are plain unlocked field writes, so the
			// per-type SyncRoot is the only lock serializing the get-current-owner -> set critical
			// sections in TrackAndSet and AddFacadeCleanup. A writer that skips the lock reopens a
			// TOCTOU window that can clobber a concurrent replacement with a stale predecessor. Route
			// all bridged facade writes through here so the invariant is enforced (in Debug) and
			// documented for future callers.
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
			static void TrackAndSetMapServiceToken(
				IPlatformGeocoding implementation,
				string mapServiceToken,
				List<Action> facadeCleanups)
			{
				lock (s_mapTokenLock)
				{
#if WINDOWS
					var previousPlatformToken = WindowsMapServiceTokenGetter();
					var previousPlatformOwner = FindMapTokenOwner(previousPlatformToken);
#endif
					var assignment = new MapTokenAssignment(
						implementation,
						mapServiceToken,
						implementation.MapServiceToken
#if WINDOWS
						, previousPlatformToken
						, previousPlatformOwner
#endif
					);

					s_mapTokenAssignments.Add(assignment);
					try
					{
						implementation.MapServiceToken = mapServiceToken;
					}
					catch
					{
						s_mapTokenAssignments.Remove(assignment);
						throw;
					}
					facadeCleanups.Add(() => CleanupMapServiceToken(assignment));
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
#endif

			static void CleanupMapServiceToken(MapTokenAssignment assignment)
			{
				lock (s_mapTokenLock)
					CleanupMapServiceTokenLocked(assignment);
			}

			static void CleanupMapServiceTokenLocked(MapTokenAssignment assignment)
			{
				var index = s_mapTokenAssignments.IndexOf(assignment);
				if (index < 0)
					return;

				MapTokenAssignment? implementationSuccessor = null;
				for (int i = index + 1; i < s_mapTokenAssignments.Count; i++)
				{
					if (ReferenceEquals(s_mapTokenAssignments[i].Implementation, assignment.Implementation))
					{
						implementationSuccessor = s_mapTokenAssignments[i];
						break;
					}
				}

				if (implementationSuccessor is not null &&
					string.Equals(implementationSuccessor.PreviousToken, assignment.AppliedToken, StringComparison.Ordinal))
				{
					implementationSuccessor.PreviousToken = assignment.PreviousToken;
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
#endif

				s_mapTokenAssignments.RemoveAt(index);

				List<Exception>? exceptions = null;
				if (implementationSuccessor is null)
				{
					try
					{
						if (string.Equals(assignment.Implementation.MapServiceToken, assignment.AppliedToken, StringComparison.Ordinal))
							assignment.Implementation.MapServiceToken = assignment.PreviousToken;
					}
					catch (Exception ex)
					{
						(exceptions ??= new()).Add(ex);
					}
				}
#if WINDOWS
				try
				{
					if (string.Equals(WindowsMapServiceTokenGetter(), assignment.AppliedToken, StringComparison.Ordinal))
					{
						WindowsMapServiceTokenSetter(
							platformSuccessor?.AppliedToken ??
							platformPredecessor?.AppliedToken ??
							assignment.PreviousPlatformToken);
					}
				}
				catch (Exception ex)
				{
					(exceptions ??= new()).Add(ex);
				}
#endif

				if (exceptions is null)
					return;

				if (exceptions.Count == 1)
					ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

				throw new AggregateException("Map service token restoration failed.", exceptions);
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

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					var previousOwner = FacadeBridgeState<T>.FindOwner(original);
					if (original is not null && previousOwner is null)
						return;

					// Direct DI implementations belong to the app that resolved them. Only
					// framework-owned defaults and owner-aware wrappers can outlive that app.
					if (previousOwner is not null && !previousOwner.AllowsSharedOwnership)
						return;

					assignment = new FacadeAssignment<T>(
						impl,
						original,
						previousOwner,
						allowsSharedOwnership: true);
					FacadeBridgeState<T>.Assignments.Add(assignment);
				}

				AddFacadeCleanup(assignment, getter, setter, facadeCleanups);
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
				List<Action> facadeCleanups)
				where T : class
			{
				facadeCleanups.Add(() =>
				{
					lock (FacadeBridgeState<T>.SyncRoot)
					{
						// Invariant: every bridged-facade mutation (SetDefault/SetCurrent, via
						// TrackAndSet) must run under FacadeBridgeState<T>.SyncRoot. The get->check->set
						// restoration below (read current, verify this assignment still owns it, restore
						// Previous) is only atomic against those mutations while that invariant holds.
						// Bridging a facade through a setter without taking this lock would reopen a
						// TOCTOU window that could clobber a concurrent replacement with the predecessor.
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
				// SyncRoot serializes both the ownership bookkeeping (Assignments) and the bridged
				// facade field itself. INVARIANT: all bridged facade writes (SetDefault/SetCurrent for
				// this T) must be performed while holding SyncRoot (see SetFacade). The facade setters
				// are plain unlocked field writes, so this lock is the sole guard against a TOCTOU
				// clobber between reading the current owner and writing the replacement.
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

			static void SetAppActions(IAppActions appActions, ILogger? logger, List<AppAction> actions)
			{
				// Build is synchronous and normally runs on the UI thread. Do not block here:
				// a custom implementation may need the native dispatcher to complete SetAsync.
				_ = SetAppActionsAsync(appActions, logger, actions);
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

			void HandleOnAppAction(object? sender, AppActionEventArgs e)
			{
				_essentialsBuilder?.AppActionHandlers?.Invoke(e.AppAction);
			}
		}

#if WINDOWS || TIZEN
		sealed class MapTokenAssignment
		{
			public MapTokenAssignment(
				IPlatformGeocoding implementation,
				string appliedToken,
				string? previousToken
#if WINDOWS
				, string? previousPlatformToken
				, MapTokenAssignment? previousPlatformOwner
#endif
			)
			{
				Implementation = implementation;
				AppliedToken = appliedToken;
				PreviousToken = previousToken;
#if WINDOWS
				PreviousPlatformToken = previousPlatformToken;
				PreviousPlatformOwner = previousPlatformOwner;
#endif
			}

			public IPlatformGeocoding Implementation { get; }

			public string AppliedToken { get; }

			public string? PreviousToken { get; set; }

#if WINDOWS
			public string? PreviousPlatformToken { get; set; }

			public MapTokenAssignment? PreviousPlatformOwner { get; set; }
#endif
		}
#endif

		sealed class EssentialsCleanup : IMauiAppCleanupService
		{
			// Per-instance lock: each MauiApp owns its own EssentialsCleanup singleton, so cleanup
			// state is guarded per app rather than by a process-global lock. Facade restoration inside
			// DisposeCore still serializes per type via FacadeBridgeState<T>.SyncRoot.
			readonly object _sync = new();
			List<Action> _facadeCleanups = new();
			bool _cleanedUp;
#if !TIZEN
			readonly List<(IAppActions AppActions, EventHandler<AppActionEventArgs> Handler)> _appActionSubscriptions = new();
#endif

			public void SetFacadeCleanups(List<Action> cleanups)
			{
				_facadeCleanups.AddRange(cleanups);
			}

#if !TIZEN
			public void Subscribe(IAppActions appActions, EventHandler<AppActionEventArgs> handler)
			{
				_appActionSubscriptions.Add((appActions, handler));
				appActions.AppActionActivated += handler;
			}
#endif

			public void Cleanup()
			{
				lock (_sync)
				{
					if (_cleanedUp)
						return;

					_cleanedUp = true;
					DisposeCore();
				}
			}

			void DisposeCore()
			{
#if !TIZEN
				List<Exception>? exceptions = null;
				for (int i = _appActionSubscriptions.Count - 1; i >= 0; i--)
				{
					var subscription = _appActionSubscriptions[i];
					try
					{
						subscription.AppActions.AppActionActivated -= subscription.Handler;
					}
					catch (Exception ex)
					{
						(exceptions ??= new()).Add(ex);
					}
				}
				_appActionSubscriptions.Clear();

				try
				{
					RestoreFacadeCleanups(_facadeCleanups);
				}
				catch (Exception facadeException)
				{
					(exceptions ??= new()).Add(facadeException);
				}

				if (exceptions is null)
					return;

				if (exceptions.Count == 1)
					ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

				throw new AggregateException(
					"AppActions unsubscription and facade restoration failed.",
					exceptions);
#else
				RestoreFacadeCleanups(_facadeCleanups);
#endif
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
