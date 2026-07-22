using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
		static readonly object s_essentialsBridgeLock = new();
#if WINDOWS || TIZEN
		static readonly List<MapTokenAssignment> s_mapTokenAssignments = new();
#endif
#if WINDOWS
		internal static Func<string?> WindowsMapServiceTokenGetter { get; set; } =
			static () => global::Windows.Services.Maps.MapService.ServiceToken;

		internal static Action<string?> WindowsMapServiceTokenSetter { get; set; } =
			static token => global::Windows.Services.Maps.MapService.ServiceToken = token;
#endif

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
				lock (s_essentialsBridgeLock)
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
					BridgeLazyVersionTrackingFromDI(versionTrackingDependencies, facadeCleanups);

					// Resolve cleanup after every bridged service so DI disposes it first. This
					// restores static facades and removes AppActions handlers while the provider-owned
					// implementations are still alive.
					cleanup = services.GetRequiredService<EssentialsCleanup>();
					cleanup.SetFacadeCleanups(facadeCleanups);

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
						if (versionTrackingBeforeTrack is null && VersionTracking.GetDefault() is { } initializedVersionTracking)
						{
							TrackInitialized(
								initializedVersionTracking,
								versionTrackingBeforeTrack,
								VersionTracking.GetDefault,
								VersionTracking.SetDefault,
								facadeCleanups);
						}
					}
				}
				catch (Exception initializationException)
				{
					try
					{
						if (cleanup is not null)
							cleanup.Cleanup();
						else
							EssentialsCleanup.RestoreFacades(facadeCleanups);
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
#if ANDROID
				BridgeIfRegistered<IActivityStateManager>(services, () => GetFacadeBackingField<IActivityStateManager>(typeof(ActivityStateManager), "defaultImplementation"), ActivityStateManager.SetDefault, facadeCleanups);
#endif
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
				var implementation = new LazyVersionTracking(
					dependencies.Preferences ?? Preferences.Default,
					dependencies.AppInfo ?? AppInfo.Current);
				TrackAndSet(
					implementation,
					VersionTracking.GetDefault,
					VersionTracking.SetDefault,
					facadeCleanups);
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
				List<Action> facadeCleanups)
				where T : class
			{
				FacadeAssignment<T> assignment;

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					assignment = new FacadeAssignment<T>(impl, currentGetter());
					FacadeBridgeState<T>.Assignments.Add(assignment);
					setter(impl);
				}

				AddFacadeCleanup(assignment, currentGetter, setter, facadeCleanups);
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
				var assignment = new MapTokenAssignment(
					implementation,
					mapServiceToken,
					implementation.MapServiceToken
#if WINDOWS
					, WindowsMapServiceTokenGetter()
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

			static void CleanupMapServiceToken(MapTokenAssignment assignment)
			{
				var index = s_mapTokenAssignments.IndexOf(assignment);
				if (index < 0)
					return;

				var platformSuccessor = index + 1 < s_mapTokenAssignments.Count
					? s_mapTokenAssignments[index + 1]
					: null;
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
				if (platformSuccessor is not null &&
					string.Equals(platformSuccessor.PreviousPlatformToken, assignment.AppliedToken, StringComparison.Ordinal))
				{
					platformSuccessor.PreviousPlatformToken = assignment.PreviousPlatformToken;
				}
#endif

				s_mapTokenAssignments.RemoveAt(index);

				if (implementationSuccessor is null &&
					string.Equals(assignment.Implementation.MapServiceToken, assignment.AppliedToken, StringComparison.Ordinal))
				{
					assignment.Implementation.MapServiceToken = assignment.PreviousToken;
				}
#if WINDOWS
				if (platformSuccessor is null &&
					string.Equals(WindowsMapServiceTokenGetter(), assignment.AppliedToken, StringComparison.Ordinal))
				{
					WindowsMapServiceTokenSetter(assignment.PreviousPlatformToken);
				}
#endif
			}
#endif

			static void TrackInitialized<T>(
				T impl,
				T? original,
				Func<T?> getter,
				Action<T?> setter,
				List<Action> facadeCleanups)
				where T : class
			{
				var assignment = new FacadeAssignment<T>(impl, original);

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					FacadeBridgeState<T>.Assignments.Add(assignment);
				}

				AddFacadeCleanup(assignment, getter, setter, facadeCleanups);
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
						var index = FacadeBridgeState<T>.Assignments.IndexOf(assignment);
						if (index < 0)
							return;

						var wasCurrent = index == FacadeBridgeState<T>.Assignments.Count - 1;
						if (!wasCurrent)
						{
							var successor = FacadeBridgeState<T>.Assignments[index + 1];
							if (ReferenceEquals(successor.Previous, assignment.Implementation))
								successor.Previous = assignment.Previous;
						}

						FacadeBridgeState<T>.Assignments.RemoveAt(index);
						if (!wasCurrent)
							return;

						if (ReferenceEquals(getter(), assignment.Implementation))
							setter(assignment.Previous);
					}
				});
			}

			sealed class FacadeAssignment<T> where T : class
			{
				public FacadeAssignment(T implementation, T? previous)
				{
					Implementation = implementation;
					Previous = previous;
				}

				public T Implementation { get; }

				public T? Previous { get; set; }
			}

			static class FacadeBridgeState<T> where T : class
			{
				internal static readonly object SyncRoot = new();
				internal static readonly List<FacadeAssignment<T>> Assignments = new();
			}

			sealed class LazyVersionTracking : IVersionTracking
			{
				readonly Lazy<IVersionTracking> _implementation;

				public LazyVersionTracking(IPreferences preferences, IAppInfo appInfo)
				{
					_implementation = new(() => new VersionTrackingImplementation(preferences, appInfo));
				}

				IVersionTracking Implementation => _implementation.Value;

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
				// App initialization is synchronous, but a DI-owned implementation must finish
				// before Build returns so provider disposal cannot race its asynchronous work.
				// Pump captured continuations on this thread to preserve platform thread affinity.
				var previousContext = SynchronizationContext.Current;
				using var context = new AppActionsSynchronizationContext();

				try
				{
					SynchronizationContext.SetSynchronizationContext(context);
					var task = SetAppActionsAsync(appActions, logger, actions);
					Task? completion = null;

					if (!task.IsCompleted)
					{
						completion = task.ContinueWith(
							static (_, state) => ((AppActionsSynchronizationContext)state!).Complete(),
							context,
							CancellationToken.None,
							TaskContinuationOptions.ExecuteSynchronously,
							TaskScheduler.Default);
						context.RunOnCurrentThread();
					}

					task.GetAwaiter().GetResult();
					completion?.GetAwaiter().GetResult();
				}
				finally
				{
					SynchronizationContext.SetSynchronizationContext(previousContext);
				}
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

			sealed class AppActionsSynchronizationContext : SynchronizationContext, IDisposable
			{
				readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object?>> _queue = new();
				int _acceptingPosts = 1;

				public override void Post(SendOrPostCallback d, object? state)
				{
					if (d is null)
						throw new ArgumentNullException(nameof(d));

					if (Volatile.Read(ref _acceptingPosts) == 0)
						return;

					try
					{
						_queue.Add(new KeyValuePair<SendOrPostCallback, object?>(d, state));
					}
					catch (InvalidOperationException) when (Volatile.Read(ref _acceptingPosts) == 0)
					{
					}
					catch (ObjectDisposedException) when (Volatile.Read(ref _acceptingPosts) == 0)
					{
					}
				}

				public void RunOnCurrentThread()
				{
					foreach (var workItem in _queue.GetConsumingEnumerable())
						workItem.Key(workItem.Value);
				}

				public void Complete()
				{
					if (Interlocked.Exchange(ref _acceptingPosts, 0) != 0)
						_queue.CompleteAdding();
				}

				public void Dispose()
				{
					Interlocked.Exchange(ref _acceptingPosts, 0);
					_queue.Dispose();
				}
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
#endif
			)
			{
				Implementation = implementation;
				AppliedToken = appliedToken;
				PreviousToken = previousToken;
#if WINDOWS
				PreviousPlatformToken = previousPlatformToken;
#endif
			}

			public IPlatformGeocoding Implementation { get; }

			public string AppliedToken { get; }

			public string? PreviousToken { get; set; }

#if WINDOWS
			public string? PreviousPlatformToken { get; set; }
#endif
		}
#endif

		sealed class EssentialsCleanup : IMauiAppCleanupService
		{
			List<Action> _facadeCleanups = new();
			bool _cleanedUp;
#if !TIZEN
			IAppActions? _subscribedAppActions;
			EventHandler<AppActionEventArgs>? _appActionHandler;
#endif

			public void SetFacadeCleanups(List<Action> cleanups)
			{
				_facadeCleanups = cleanups;
			}

#if !TIZEN
			public void Subscribe(IAppActions appActions, EventHandler<AppActionEventArgs> handler)
			{
				_subscribedAppActions = appActions;
				_appActionHandler = handler;
				appActions.AppActionActivated += handler;
			}
#endif

			public void Cleanup()
			{
				lock (s_essentialsBridgeLock)
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
				try
				{
					if (_subscribedAppActions is not null && _appActionHandler is not null)
						_subscribedAppActions.AppActionActivated -= _appActionHandler;
				}
				finally
				{
					_subscribedAppActions = null;
					_appActionHandler = null;
					RestoreFacades();
				}
#else
				RestoreFacades();
#endif
			}

			internal static void RestoreFacades(List<Action> facadeCleanups)
			{
				lock (s_essentialsBridgeLock)
				{
					for (int i = facadeCleanups.Count - 1; i >= 0; i--)
						facadeCleanups[i]();

					facadeCleanups.Clear();
				}
			}

			void RestoreFacades()
			{
				RestoreFacades(_facadeCleanups);
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
