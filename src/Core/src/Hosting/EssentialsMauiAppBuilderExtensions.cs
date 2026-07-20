using System;
using System.Collections.Generic;
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

#if !(ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS || TIZEN)
			// Register MainThreadBridgeInitializer first so it runs before EssentialsInitializer
			// resolves DI services whose constructors may use MainThread. This shared path also
			// covers ConfigureEssentials with MauiApp.CreateBuilder(useDefaults: false).
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MainThreadBridgeInitializer>());
#endif
			builder.Services.TryAddSingleton<EssentialsCleanup>();
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

#if WINDOWS || TIZEN
				var mapServiceToken = _essentialsBuilder.MapServiceToken;
				if (mapServiceToken is null && Geocoding.Default is IPlatformGeocoding existingPlatformGeocoding)
					mapServiceToken = existingPlatformGeocoding.MapServiceToken;
#endif

				var facadeCleanups = new List<Action>();
				BridgeEssentialsFromDI(services, facadeCleanups);

				// Resolve cleanup after every bridged service so DI disposes it first. This
				// restores static facades and removes AppActions handlers while the provider-owned
				// implementations are still alive.
				var cleanup = services.GetRequiredService<EssentialsCleanup>();
				cleanup.SetFacadeCleanups(facadeCleanups);

#if WINDOWS || TIZEN
				// A ConfigureEssentials token takes precedence; otherwise preserve a token set
				// directly through ApplicationModel.Platform.MapServiceToken before MauiApp.Build().
				if (mapServiceToken is not null)
				{
					var geocoding = Geocoding.Default;
					if (geocoding is IPlatformGeocoding platformGeocoding)
					{
						platformGeocoding.MapServiceToken = mapServiceToken;
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
				// Only subscribe to the static AppActions.OnAppAction event when at least one
				// handler was actually registered via IEssentialsBuilder.OnAppAction. The static
				// event subscription would otherwise pin this initializer instance for the app's
				// lifetime (and across repeated MauiApp.Build() calls in tests / hosting scenarios)
				// even when the handler is a no-op.
				if (_essentialsBuilder.AppActionHandlers is not null)
				{
					cleanup.Subscribe(AppActions.Current, HandleOnAppAction);
				}

				if (_essentialsBuilder.AppActions is not null)
				{
					SetAppActions(services, _essentialsBuilder.AppActions);
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

			/// <summary>
			/// Bridges DI-registered Essentials implementations to the static facades.
			/// If a service is registered in DI, it becomes the backing implementation for
			/// the corresponding static API. If not registered, the existing lazy platform
			/// default behavior is preserved.
			/// </summary>
			static void BridgeEssentialsFromDI(IServiceProvider services, List<Action> facadeCleanups)
			{
				// SetDefault pattern types
				BridgeIfRegistered<IAccelerometer>(services, () => Accelerometer.Default, Accelerometer.SetDefault, facadeCleanups);
#if ANDROID
				BridgeIfRegistered<IActivityStateManager>(services, () => ActivityStateManager.Default, ActivityStateManager.SetDefault, facadeCleanups);
#endif
				BridgeIfRegistered<IBarometer>(services, () => Barometer.Default, Barometer.SetDefault, facadeCleanups);
				BridgeIfRegistered<IBattery>(services, () => Battery.Default, Battery.SetDefault, facadeCleanups);
				BridgeIfRegistered<IBrowser>(services, () => Browser.Default, Browser.SetDefault, facadeCleanups);
				BridgeIfRegistered<IClipboard>(services, () => Clipboard.Default, Clipboard.SetDefault, facadeCleanups);
				BridgeIfRegistered<ICompass>(services, () => Compass.Default, Compass.SetDefault, facadeCleanups);
				BridgeIfRegistered<IContacts>(services, () => MauiContacts.Default, MauiContacts.SetDefault, facadeCleanups);
				BridgeIfRegistered<IEmail>(services, () => Email.Default, Email.SetDefault, facadeCleanups);
				BridgeIfRegistered<IFilePicker>(services, () => FilePicker.Default, FilePicker.SetDefault, facadeCleanups);
				BridgeIfRegistered<IFlashlight>(services, () => Flashlight.Default, Flashlight.SetDefault, facadeCleanups);
				BridgeIfRegistered<IGeolocation>(services, () => Geolocation.Default, Geolocation.SetDefault, facadeCleanups);
				BridgeIfRegistered<IGyroscope>(services, () => Gyroscope.Default, Gyroscope.SetDefault, facadeCleanups);
				BridgeIfRegistered<IHapticFeedback>(services, () => HapticFeedback.Default, HapticFeedback.SetDefault, facadeCleanups);
				BridgeIfRegistered<ILauncher>(services, () => Launcher.Default, Launcher.SetDefault, facadeCleanups);
				BridgeIfRegistered<IMagnetometer>(services, () => Magnetometer.Default, Magnetometer.SetDefault, facadeCleanups);
				BridgeIfRegistered<IMap>(services, () => Map.Default, Map.SetDefault, facadeCleanups);
				BridgeIfRegistered<IMediaPicker>(services, () => MediaPicker.Default, MediaPicker.SetDefault, facadeCleanups);
				BridgeIfRegistered<IOrientationSensor>(services, () => OrientationSensor.Default, OrientationSensor.SetDefault, facadeCleanups);
				BridgeIfRegistered<IPhoneDialer>(services, () => PhoneDialer.Default, PhoneDialer.SetDefault, facadeCleanups);
				BridgeIfRegistered<IPreferences>(services, () => Preferences.Default, Preferences.SetDefault, facadeCleanups);
				BridgeIfRegistered<IScreenshot>(services, () => Screenshot.Default, Screenshot.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISecureStorage>(services, () => SecureStorage.Default, SecureStorage.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISemanticScreenReader>(services, () => SemanticScreenReader.Default, SemanticScreenReader.SetDefault, facadeCleanups);
				BridgeIfRegistered<IShare>(services, () => Share.Default, Share.SetDefault, facadeCleanups);
				BridgeIfRegistered<ISms>(services, () => Sms.Default, Sms.SetDefault, facadeCleanups);
				BridgeIfRegistered<ITextToSpeech>(services, () => TextToSpeech.Default, TextToSpeech.SetDefault, facadeCleanups);
				BridgeIfRegistered<IVersionTracking>(services, VersionTracking.GetDefault, VersionTracking.SetDefault, facadeCleanups);
				BridgeIfRegistered<IVibration>(services, () => Vibration.Default, Vibration.SetDefault, facadeCleanups);
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
						TrackAndSet(webAuthenticator, () => WebAuthenticator.Default, WebAuthenticator.SetDefault, facadeCleanups);
					else
						LogMissingNativeLifecycleInterface<IWebAuthenticator>(services, nameof(IPlatformWebAuthenticatorCallback));
#else
					TrackAndSet(webAuthenticator, () => WebAuthenticator.Default, WebAuthenticator.SetDefault, facadeCleanups);
#endif
				}
#if WINDOWS || __IOS__ || __MACCATALYST__
				BridgeIfRegistered<IWindowStateManager>(services, () => WindowStateManager.Default, WindowStateManager.SetDefault, facadeCleanups);
#endif
				BridgeIfRegistered<IAppleSignInAuthenticator>(services, () => AppleSignInAuthenticator.Default, AppleSignInAuthenticator.SetDefault, facadeCleanups);

				// SetCurrent pattern types
				// IAppActions: On native platforms, lifecycle hooks cast AppActions.Current to
				// IPlatformAppActions via AsPlatform(). Only bridge if the DI implementation
				// supports it, to prevent PlatformNotSupportedException at runtime.
				var appActions = services.GetService<IAppActions>();
				if (appActions is not null)
				{
#if WINDOWS || __IOS__ || __MACCATALYST__ || ANDROID
					if (appActions is IPlatformAppActions)
						TrackAndSet(appActions, () => AppActions.Current, AppActions.SetCurrent, facadeCleanups);
					else
						LogMissingNativeLifecycleInterface<IAppActions>(services, nameof(IPlatformAppActions));
#else
					TrackAndSet(appActions, () => AppActions.Current, AppActions.SetCurrent, facadeCleanups);
#endif
				}
				BridgeIfRegistered<IAppInfo>(services, () => AppInfo.Current, AppInfo.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IConnectivity>(services, () => Connectivity.Current, Connectivity.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IDeviceDisplay>(services, () => DeviceDisplay.Current, DeviceDisplay.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IDeviceInfo>(services, () => DeviceInfo.Current, DeviceInfo.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IFileSystem>(services, () => FileSystem.Current, FileSystem.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IGeocoding>(services, () => Geocoding.Default, Geocoding.SetCurrent, facadeCleanups);
				BridgeIfRegistered<IPermissions>(services, () => Permissions.Current, Permissions.SetCurrent, facadeCleanups);
			}

			/// <summary>
			/// Resolves a DI-registered implementation and assigns it to the corresponding static facade.
			/// The prior facade value is restored when the owning app is disposed, unless another app or
			/// internal caller replaced the facade in the meantime.
			/// </summary>
			static void BridgeIfRegistered<T>(
				IServiceProvider services,
				Func<T?> getter,
				Action<T?> setter,
				List<Action> facadeCleanups)
				where T : class
			{
				var impl = services.GetService<T>();
				if (impl is not null)
					TrackAndSet(impl, getter, setter, facadeCleanups);
			}

			static void TrackAndSet<T>(
				T impl,
				Func<T?> getter,
				Action<T?> setter,
				List<Action> facadeCleanups)
				where T : class
			{
				var assignment = new FacadeAssignment<T>(impl);

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					if (FacadeBridgeState<T>.Assignments.Count == 0)
						FacadeBridgeState<T>.Original = getter();

					FacadeBridgeState<T>.Assignments.Add(assignment);
					setter(impl);
				}

				AddFacadeCleanup(assignment, getter, setter, facadeCleanups);
			}

			static void TrackInitialized<T>(
				T impl,
				T? original,
				Func<T?> getter,
				Action<T?> setter,
				List<Action> facadeCleanups)
				where T : class
			{
				var assignment = new FacadeAssignment<T>(impl);

				lock (FacadeBridgeState<T>.SyncRoot)
				{
					if (FacadeBridgeState<T>.Assignments.Count == 0)
						FacadeBridgeState<T>.Original = original;

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
						FacadeBridgeState<T>.Assignments.RemoveAt(index);
						if (!wasCurrent)
							return;

						if (ReferenceEquals(getter(), assignment.Implementation))
						{
							var replacement = FacadeBridgeState<T>.Assignments.Count > 0
								? FacadeBridgeState<T>.Assignments[FacadeBridgeState<T>.Assignments.Count - 1].Implementation
								: FacadeBridgeState<T>.Original;
							setter(replacement);
						}

						if (FacadeBridgeState<T>.Assignments.Count == 0)
							FacadeBridgeState<T>.Original = null;
					}
				});
			}

			sealed class FacadeAssignment<T> where T : class
			{
				public FacadeAssignment(T implementation)
				{
					Implementation = implementation;
				}

				public T Implementation { get; }
			}

			static class FacadeBridgeState<T> where T : class
			{
				internal static readonly object SyncRoot = new();
				internal static readonly List<FacadeAssignment<T>> Assignments = new();
				internal static T? Original;
			}

			static void LogMissingNativeLifecycleInterface<T>(IServiceProvider services, string requiredInterface)
				where T : class =>
				services.GetService<ILoggerFactory>()?
					.CreateLogger<EssentialsInitializer>()
					.LogWarning(
						"DI-registered {ServiceType} was not bridged to its static facade because native lifecycle callbacks require {RequiredInterface}.",
						typeof(T).Name,
						requiredInterface);

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

		sealed class EssentialsCleanup : IDisposable
		{
			List<Action> _facadeCleanups = new();
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

			public void Dispose()
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

			void RestoreFacades()
			{
				for (int i = _facadeCleanups.Count - 1; i >= 0; i--)
					_facadeCleanups[i]();

				_facadeCleanups.Clear();
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
