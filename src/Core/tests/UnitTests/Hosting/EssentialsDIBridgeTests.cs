#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	[Collection(MainThreadStaticStateCollection.Name)]
	public class EssentialsDIBridgeTests : IDisposable
	{
		public void Dispose()
		{
			// Clear backing fields directly to avoid initializing platform defaults during cleanup
			// and to cover platform-specific types without a common setter contract.
			ResetStaticField(typeof(Accelerometer), "defaultImplementation");
			ResetStaticField(typeof(Barometer), "defaultImplementation");
			ResetStaticField(typeof(Battery), "defaultImplementation");
			ResetStaticField(typeof(Browser), "defaultImplementation");
			ResetStaticField(typeof(Clipboard), "defaultImplementation");
			ResetStaticField(typeof(Compass), "defaultImplementation");
			ResetStaticField(typeof(Contacts), "defaultImplementation");
			ResetStaticField(typeof(Email), "defaultImplementation");
			ResetStaticField(typeof(FilePicker), "defaultImplementation");
			ResetStaticField(typeof(Flashlight), "defaultImplementation");
			ResetStaticField(typeof(Geolocation), "defaultImplementation");
			ResetStaticField(typeof(Gyroscope), "defaultImplementation");
			ResetStaticField(typeof(HapticFeedback), "defaultImplementation");
			ResetStaticField(typeof(Launcher), "defaultImplementation");
			ResetStaticField(typeof(Magnetometer), "defaultImplementation");
			ResetStaticField(typeof(Map), "defaultImplementation");
			ResetStaticField(typeof(MediaPicker), "defaultImplementation");
			ResetStaticField(typeof(OrientationSensor), "defaultImplementation");
			ResetStaticField(typeof(Passkeys), "defaultImplementation");
			ResetStaticField(typeof(PhoneDialer), "defaultImplementation");
			ResetStaticField(typeof(Preferences), "defaultImplementation");
			ResetStaticField(typeof(Screenshot), "defaultImplementation");
			ResetStaticField(typeof(SecureStorage), "defaultImplementation");
			ResetStaticField(typeof(SemanticScreenReader), "defaultImplementation");
			ResetStaticField(typeof(Share), "defaultImplementation");
			ResetStaticField(typeof(Sms), "defaultImplementation");
			ResetStaticField(typeof(TextToSpeech), "defaultImplementation");
			ResetStaticField(typeof(VersionTracking), "defaultImplementation");
			ResetStaticField(typeof(Vibration), "defaultImplementation");
			ResetStaticField(typeof(WebAuthenticator), "defaultImplementation");
			ResetStaticField(typeof(AppleSignInAuthenticator), "defaultImplementation");

			ResetStaticField(typeof(AppActions), "currentImplementation");
			ResetStaticField(typeof(AppInfo), "currentImplementation");
			ResetStaticField(typeof(Connectivity), "currentImplementation");
			ResetStaticField(typeof(DeviceDisplay), "currentImplementation");
			ResetStaticField(typeof(DeviceInfo), "currentImplementation");
			ResetStaticField(typeof(FileSystem), "currentImplementation");
			ResetStaticField(typeof(Permissions), "currentImplementation");
			ResetStaticField("Microsoft.Maui.ApplicationModel.ActivityStateManager", "defaultImplementation");
			ResetStaticField("Microsoft.Maui.ApplicationModel.WindowStateManager", "defaultImplementation");
			// Geocoding is a special case: uses SetCurrent but the backing field is defaultImplementation
			ResetStaticField(typeof(Geocoding), "defaultImplementation");
		}

		static void ResetStaticField(Type type, string fieldName)
		{
			var field = type.GetField(fieldName,
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
				?? throw new InvalidOperationException(
					$"Field '{fieldName}' not found on '{type.Name}'. Was it renamed?");
			field.SetValue(null, null);
		}

		static void InvokeTrackInitializedOrOwned<T>(
			T implementation,
			T? original,
			Func<T?> getter,
			Action<T?> setter,
			List<Action> facadeCleanups)
			where T : class
		{
			var initializerType = typeof(EssentialsExtensions).GetNestedType(
				"EssentialsInitializer",
				System.Reflection.BindingFlags.NonPublic)
				?? throw new InvalidOperationException("EssentialsInitializer type was not found.");
			var method = initializerType.GetMethod(
				"TrackInitializedOrOwned",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
				?? throw new InvalidOperationException("TrackInitializedOrOwned method was not found.");

			method.MakeGenericMethod(typeof(T)).Invoke(
				null,
				new object?[] { implementation, original, getter, setter, facadeCleanups });
		}

		static void ResetStaticField(string typeName, string fieldName)
		{
			var assemblyName = typeof(AppInfo).Assembly.GetName().Name;
			var type = Type.GetType($"{typeName}, {assemblyName}", throwOnError: false);
			// Platform-specific types may not exist in the current TFM (e.g. ActivityStateManager on non-Android)
			if (type is not null)
				ResetStaticField(type, fieldName);
		}

		[Fact]
		public void DIRegisteredPreferences_BridgedToStaticFacade()
		{
			var mock = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Preferences.Default);
		}

		[Fact]
		public async Task ConcurrentPreferencesSetterWinsOverFallbackFactory()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var factoryEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var setterStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var fallback = new StubPreferences();
			var registered = new StubPreferences();
			Preferences.SetDefault(null);

			var fallbackRead = Task.Run(() =>
				Preferences.GetDefault(() =>
				{
					factoryEntered.TrySetResult(true);
					Assert.True(
						setterStarted.Task.Wait(timeout),
						"Timed out waiting for the concurrent Preferences setter.");
					return fallback;
				}));

			await factoryEntered.Task.WaitAsync(timeout);

			var setter = Task.Run(() =>
			{
				setterStarted.TrySetResult(true);
				Preferences.SetDefault(registered);
			});

			Assert.Same(fallback, await fallbackRead.WaitAsync(timeout));
			await setter.WaitAsync(timeout);
			Assert.Same(registered, Preferences.Default);
		}

		[Fact]
		public async Task ConcurrentPreferencesFallbackDoesNotOverwriteDIRegisteredFacade()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var factoryEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseFactory = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var preferencesResolved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var fallback = new StubPreferences();
			var registered = new StubPreferences();
			MauiApp? app = null;
			Task<MauiApp>? build = null;
			Task<IPreferences>? fallbackRead = null;

			try
			{
				Preferences.SetDefault(null);
				fallbackRead = Task.Run(() =>
					Preferences.GetDefault(() =>
					{
						factoryEntered.TrySetResult(true);
						Assert.True(
							releaseFactory.Task.Wait(timeout),
							"Timed out waiting to release the Preferences fallback factory.");
						return fallback;
					}));

				await factoryEntered.Task.WaitAsync(timeout);

				var builder = MauiApp.CreateBuilder();
				builder.Services.AddSingleton<IPreferences>(_ =>
				{
					preferencesResolved.TrySetResult(true);
					return registered;
				});
				build = Task.Run(builder.Build);

				await preferencesResolved.Task.WaitAsync(timeout);
				Assert.NotSame(build, await Task.WhenAny(build, Task.Delay(TimeSpan.FromMilliseconds(250))));

				releaseFactory.TrySetResult(true);

				Assert.Same(fallback, await fallbackRead.WaitAsync(timeout));
				app = await build.WaitAsync(timeout);
				Assert.Same(registered, Preferences.Default);

				app.Dispose();
				app = null;

				Assert.Same(fallback, Preferences.Default);
			}
			finally
			{
				releaseFactory.TrySetResult(true);

				if (fallbackRead is not null)
					await Record.ExceptionAsync(() => fallbackRead.WaitAsync(timeout));

				if (build is not null)
					await Record.ExceptionAsync(() => build.WaitAsync(timeout));

				app?.Dispose();
			}
		}

		[Fact]
		public void DIRegisteredBattery_BridgedToStaticFacade()
		{
			var mock = new StubBattery();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IBattery>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Battery.Default);
		}

		[Fact]
		public void DIRegisteredAppInfo_BridgedToStaticFacade()
		{
			var mock = new StubAppInfo();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppInfo>(mock);

			using var app = builder.Build();

			Assert.Same(mock, AppInfo.Current);
		}

		[Fact]
		public void DIRegisteredConnectivity_BridgedToStaticFacade()
		{
			var mock = new StubConnectivity();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IConnectivity>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Connectivity.Current);
		}

		[Fact]
		public void DIRegisteredPermissions_BridgedToStaticFacade()
		{
			var mock = new StubPermissions();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPermissions>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Permissions.Current);
		}

		[Fact]
		public void DIRegisteredPasskeys_BridgedToStaticFacade()
		{
			var mock = new StubPasskeys();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPasskeys>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Passkeys.Default);
		}

		[Fact]
		public void NoDIRegistration_StaticFacadeUnchanged()
		{
			// When nothing is registered in DI, the backing field should remain null
			// (the lazy getter will create the platform default on first access).
			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			// Verify the setter was NOT called — field is still null.
			// Accessing .Default would trigger lazy init, so we use reflection.
			var field = typeof(Preferences).GetField("defaultImplementation",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			Assert.NotNull(field);
			Assert.Null(field!.GetValue(null));
		}

		[Fact]
		public void MultipleDIRegistrations_AllBridged()
		{
			var mockPrefs = new StubPreferences();
			var mockBattery = new StubBattery();
			var mockAppInfo = new StubAppInfo();

			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mockPrefs);
			builder.Services.AddSingleton<IBattery>(mockBattery);
			builder.Services.AddSingleton<IAppInfo>(mockAppInfo);

			using var app = builder.Build();

			Assert.Same(mockPrefs, Preferences.Default);
			Assert.Same(mockBattery, Battery.Default);
			Assert.Same(mockAppInfo, AppInfo.Current);
		}

		[Fact]
		public void DIBridge_WorksWithConfigureEssentials()
		{
			// Ensure bridge works even when ConfigureEssentials is also called
			var mock = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);
			builder.ConfigureEssentials();

			using var app = builder.Build();

			Assert.Same(mock, Preferences.Default);
		}

		[Fact]
		public void ConfigureEssentialsDoesNotDuplicateCleanupRegistration()
		{
			var builder = MauiApp.CreateBuilder();
			builder.ConfigureEssentials();
			using var app = builder.Build();
			var cleanupType = typeof(EssentialsExtensions).GetNestedType(
				"EssentialsCleanup",
				System.Reflection.BindingFlags.NonPublic)
				?? throw new InvalidOperationException("EssentialsCleanup type not found.");
			var preProviderRegistrationCount = 0;
			var postProviderRegistrationCount = 0;

			foreach (var cleanupService in app.Services.GetServices<IMauiAppCleanupService>())
			{
				if (cleanupService.GetType() == cleanupType)
					preProviderRegistrationCount++;
			}

			foreach (var cleanupService in app.Services.GetServices<IMauiAppPostProviderCleanupService>())
			{
				if (cleanupService.GetType() == cleanupType)
					postProviderRegistrationCount++;
			}

			Assert.Equal(1, preProviderRegistrationCount);
			Assert.Equal(1, postProviderRegistrationCount);
		}

		[Fact]
		public void DIBridge_DoesNotInitializePlatformDefaultForOriginalCapture()
		{
			ResetStaticField(typeof(Preferences), "defaultImplementation");

			var mock = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);

			using (var app = builder.Build())
				Assert.Same(mock, Preferences.Default);

			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
		}

		[Fact]
		public void CleanupDoesNotInitializePlatformDefaultAfterExternalClear()
		{
			ResetStaticField(typeof(Preferences), "defaultImplementation");

			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(new StubPreferences());
			var app = builder.Build();

			ResetStaticField(typeof(Preferences), "defaultImplementation");
			app.Dispose();

			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
		}

		[Fact]
		public void DIRegisteredGeocoding_BridgedToDefaultProperty()
		{
			// Geocoding is unique: uses SetCurrent internally but exposes Default property,
			// and its backing field is named defaultImplementation (not currentImplementation).
			var mock = new StubGeocoding();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IGeocoding>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Geocoding.Default);
		}

		[Fact]
		public void NoDIRegistration_SetCurrentTypeUnchanged()
		{
			// Verify SetCurrent types are also unset when nothing is registered in DI.
			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			var field = typeof(AppInfo).GetField("currentImplementation",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			Assert.NotNull(field);
			Assert.Null(field!.GetValue(null));
		}

		[Fact]
		public void TransientDIRegistration_StillBridged()
		{
			// Even with transient lifetime, the bridge resolves one instance and stores it
			// in the static facade for the lifetime of the owning MauiApp.
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IPreferences, StubPreferences>();

			using var app = builder.Build();

			Assert.IsType<StubPreferences>(Preferences.Default);
		}

		[Fact]
		public void DIRegisteredContacts_BridgedToStaticFacade()
		{
			// Validates the Contacts bridge specifically — this type has an Apple framework
			// namespace collision on iOS/macCatalyst that caused CS0234 build failures.
			var mock = new StubContacts();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IContacts>(mock);

			using var app = builder.Build();

			Assert.Same(mock, Contacts.Default);
		}

		[Fact]
		public void DIRegisteredWebAuthenticator_BridgedOnlyWhenNativeLifecycleContractIsSupported()
		{
			var mock = new StubWebAuthenticator();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IWebAuthenticator>(mock);

			using var app = builder.Build();

#if ANDROID || __IOS__ || __MACCATALYST__ || WINDOWS
			Assert.Null(GetStaticField(typeof(WebAuthenticator), "defaultImplementation"));
#else
			Assert.Same(mock, WebAuthenticator.Default);
#endif
		}

		[Fact]
		public void DIRegisteredAppActions_BridgedOnlyWhenNativeLifecycleContractIsSupported()
		{
			var mock = new StubAppActions();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions>(mock);

			using var app = builder.Build();

#if WINDOWS || __IOS__ || __MACCATALYST__ || ANDROID
			Assert.Null(GetStaticField(typeof(AppActions), "currentImplementation"));
#else
			Assert.Same(mock, AppActions.Current);
#endif
		}

		[Fact]
		public void ConfiguredAppActionHandlerUnsubscribesWhenMauiAppIsDisposed()
		{
			var appActions = new StubAppActions();
			var handlerCalls = 0;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions>(appActions);
			builder.ConfigureEssentials(essentials =>
				essentials.OnAppAction(_ => handlerCalls++));

			using (var app = builder.Build())
			{
				appActions.Raise(new AppAction("test", "Test"));
				Assert.Equal(1, handlerCalls);
			}

			appActions.Raise(new AppAction("test", "Test"));
			Assert.Equal(1, handlerCalls);
		}

		[Fact]
		public void StaticFacadeRestoresPreviousImplementationWhenOwningMauiAppIsDisposed()
		{
			var original = Preferences.Default;
			var mock = new StubPreferences();
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IPreferences>(mock);

			using (var firstApp = firstBuilder.Build())
			{
				Assert.Same(mock, Preferences.Default);
			}

			Assert.Same(original, Preferences.Default);
		}

		[Fact]
		public void NonDisposableServiceProviderRestoresStaticFacadeWhenMauiAppIsDisposed()
		{
			var original = Preferences.Default;
			var mock = new StubPreferences();
			var factory = new NonDisposableServiceProviderFactory();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);
			builder.ConfigureContainer(factory);

			var app = builder.Build();
			try
			{
				Assert.Same(mock, Preferences.Default);
				Assert.False(app.Services is IDisposable);

				app.Dispose();

				Assert.Same(original, Preferences.Default);
			}
			finally
			{
				factory.DisposeInnerProvider();
				Preferences.SetDefault(original);
			}
		}

		[Fact]
		public void UseVersionTrackingDoesNotAdoptConcurrentExclusiveFacade()
		{
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IVersionTracking, DisposableStubVersionTracking>();
			var app = builder.Build();
			var exclusiveVersionTracking = Assert.IsType<DisposableStubVersionTracking>(
				app.Services.GetRequiredService<IVersionTracking>());
			var simulatedConcurrentCleanups = new List<Action>();

			try
			{
				// Simulate another app having captured original == null immediately before
				// this app installed its exclusive DI-owned facade.
				InvokeTrackInitializedOrOwned(
					exclusiveVersionTracking,
					original: null,
					VersionTracking.GetDefault,
					VersionTracking.SetDefault,
					simulatedConcurrentCleanups);

				Assert.Empty(simulatedConcurrentCleanups);
				Assert.Same(
					exclusiveVersionTracking,
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			}
			finally
			{
				EssentialsExtensions.RestoreFacadeCleanups(simulatedConcurrentCleanups);
				app.Dispose();
			}

			Assert.True(exclusiveVersionTracking.IsDisposed);
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
		}

		[Fact]
		public async Task NonDisposableServiceProviderRestoresStaticFacadeWhenMauiAppIsDisposedAsync()
		{
			var original = Preferences.Default;
			var mock = new StubPreferences();
			var factory = new NonDisposableServiceProviderFactory();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(mock);
			builder.ConfigureContainer(factory);

			var app = builder.Build();
			try
			{
				Assert.Same(mock, Preferences.Default);
				Assert.False(app.Services is IAsyncDisposable);

				await app.DisposeAsync();

				Assert.Same(original, Preferences.Default);
			}
			finally
			{
				factory.DisposeInnerProvider();
				Preferences.SetDefault(original);
			}
		}

		[Fact]
		public void ContainerOwnedStaticFacadeRemainsCurrentUntilServiceIsDisposed()
		{
			var original = Preferences.Default;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();

			var app = builder.Build();
			var implementation = Assert.IsType<DisposableStubPreferences>(
				app.Services.GetRequiredService<IPreferences>());

			Assert.Same(implementation, Preferences.Default);

			app.Dispose();

			Assert.True(implementation.IsDisposed);
			Assert.True(implementation.FacadeWasCurrentDuringDispose);
			Assert.Same(original, Preferences.Default);
		}

		[Fact]
		public void RepeatedInitializeAppServicesRestoresAllFacadeAssignmentsAfterProviderDisposal()
		{
			var original = Preferences.Default;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();

			var app = builder.Build();
			var implementation = Assert.IsType<DisposableStubPreferences>(
				app.Services.GetRequiredService<IPreferences>());

			app.InitializeAppServices();
			Assert.Same(implementation, Preferences.Default);

			app.Dispose();

			Assert.True(implementation.IsDisposed);
			Assert.True(implementation.FacadeWasCurrentDuringDispose);
			Assert.Same(original, Preferences.Default);
		}

		[Fact]
		public void FailedRepeatedInitializeAppServicesCanBeRetriedWithoutLeakingFacade()
		{
			var original = Preferences.Default;
			var versionTracking = new FailOnSecondTrackVersionTracking();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
			builder.Services.AddSingleton<IVersionTracking>(versionTracking);
			builder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
			var app = builder.Build();
			var preferences = Assert.IsType<DisposableStubPreferences>(
				app.Services.GetRequiredService<IPreferences>());

			var exception = Assert.Throws<InvalidOperationException>(app.InitializeAppServices);

			Assert.Equal("second track failed", exception.Message);
			Assert.Same(preferences, Preferences.Default);

			app.InitializeAppServices();
			Assert.Same(preferences, Preferences.Default);
			Assert.Equal(3, versionTracking.TrackCount);

			app.Dispose();

			Assert.True(preferences.IsDisposed);
			Assert.True(preferences.FacadeWasCurrentDuringDispose);
			Assert.Same(original, Preferences.Default);
		}

		[Fact]
		public async Task AsyncDisposableServiceCanUseFacadeDuringProviderDisposal()
		{
			var original = Preferences.Default;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
			builder.Services.AddSingleton<AsyncFacadeReadingDisposable>();
			var app = builder.Build();
			var preferences = Assert.IsType<DisposableStubPreferences>(
				app.Services.GetRequiredService<IPreferences>());
			var observer = app.Services.GetRequiredService<AsyncFacadeReadingDisposable>();

			await app.DisposeAsync();

			Assert.Same(preferences, observer.FacadeDuringDispose);
			Assert.True(preferences.FacadeWasCurrentDuringDispose);
			Assert.Same(original, Preferences.Default);
		}

		[Fact]
		public void RepeatedInitializeAppServicesUnsubscribesAllAppActionHandlers()
		{
			var appActions = new StubAppActions();
			var handlerInvocations = 0;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions>(appActions);
			builder.ConfigureEssentials(essentials =>
				essentials.OnAppAction(_ => handlerInvocations++));

			var app = builder.Build();
			app.InitializeAppServices();

			appActions.Raise(new AppAction("test", "Test"));
			Assert.Equal(2, handlerInvocations);

			app.Dispose();
			appActions.Raise(new AppAction("test", "Test"));

			Assert.Equal(2, handlerInvocations);
		}

		[Fact]
		public void OlderMauiAppDisposeDoesNotClobberLaterAppBridge()
		{
			var original = Preferences.Default;
			var firstMock = new StubPreferences();
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IPreferences>(firstMock);
			MauiApp? firstApp = firstBuilder.Build();
			MauiApp? secondApp = null;

			try
			{
				var secondMock = new StubPreferences();
				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(secondMock);
				secondApp = secondBuilder.Build();

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(secondMock, Preferences.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(original, Preferences.Default);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void NewerMauiAppDisposeRestoresOlderAppBridge()
		{
			var original = Preferences.Default;
			var firstMock = new StubPreferences();
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IPreferences>(firstMock);
			using (var firstApp = firstBuilder.Build())
			{
				var secondMock = new StubPreferences();
				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(secondMock);

				MauiApp? secondApp = secondBuilder.Build();
				try
				{
					secondApp.Dispose();
					secondApp = null;
					Assert.Same(firstMock, Preferences.Default);
				}
				finally
				{
					secondApp?.Dispose();
				}
			}

			Assert.Same(original, Preferences.Default);
		}

		[Fact]
		public void MauiAppDisposeDoesNotClobberExternalFacadeReplacement()
		{
			var bridged = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(bridged);
			var app = builder.Build();

			var external = new StubPreferences();
			Preferences.SetDefault(external);

			app.Dispose();

			Assert.Same(external, Preferences.Default);
		}

		[Fact]
		public void ExternalFacadeReplacementSurvivesLaterMauiAppLifetime()
		{
			var firstMock = new StubPreferences();
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IPreferences>(firstMock);
			MauiApp? firstApp = firstBuilder.Build();
			MauiApp? secondApp = null;

			try
			{
				var external = new StubPreferences();
				Preferences.SetDefault(external);

				var secondMock = new StubPreferences();
				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(secondMock);
				secondApp = secondBuilder.Build();
				Assert.Same(secondMock, Preferences.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(external, Preferences.Default);

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(external, Preferences.Default);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void DisposedOlderMauiAppFacadeIsNotRestoredByLaterApp()
		{
			var original = Preferences.Default;
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;
			MauiApp? thirdApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
				firstApp = firstBuilder.Build();
				var firstPreferences = Assert.IsType<DisposableStubPreferences>(
					firstApp.Services.GetRequiredService<IPreferences>());

				var secondPreferences = new StubPreferences();
				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(secondPreferences);
				secondApp = secondBuilder.Build();

				Preferences.SetDefault(firstPreferences);

				var thirdPreferences = new StubPreferences();
				var thirdBuilder = MauiApp.CreateBuilder();
				thirdBuilder.Services.AddSingleton<IPreferences>(thirdPreferences);
				thirdApp = thirdBuilder.Build();

				firstApp.Dispose();
				firstApp = null;
				Assert.True(firstPreferences.IsDisposed);
				Assert.Same(thirdPreferences, Preferences.Default);

				thirdApp.Dispose();
				thirdApp = null;
				Assert.Same(original, Preferences.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(original, Preferences.Default);
			}
			finally
			{
				thirdApp?.Dispose();
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void DisposingExternallySelectedOwnerDoesNotLeaveDisposedFacade()
		{
			var original = Preferences.Default;
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
				firstApp = firstBuilder.Build();
				var firstPreferences = Assert.IsType<DisposableStubPreferences>(
					firstApp.Services.GetRequiredService<IPreferences>());

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(new StubPreferences());
				secondApp = secondBuilder.Build();

				Preferences.SetDefault(firstPreferences);
				firstApp.Dispose();
				firstApp = null;

				Assert.True(firstPreferences.IsDisposed);
				Assert.Same(original, Preferences.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(original, Preferences.Default);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void SharedFacadeImplementationKeepsNewestActiveOwner()
		{
			var original = Preferences.Default;
			var sharedPreferences = new StubPreferences();
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;
			MauiApp? thirdApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IPreferences>(sharedPreferences);
				firstApp = firstBuilder.Build();

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(sharedPreferences);
				secondApp = secondBuilder.Build();

				var thirdPreferences = new StubPreferences();
				var thirdBuilder = MauiApp.CreateBuilder();
				thirdBuilder.Services.AddSingleton<IPreferences>(thirdPreferences);
				thirdApp = thirdBuilder.Build();

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(thirdPreferences, Preferences.Default);

				thirdApp.Dispose();
				thirdApp = null;
				Assert.Same(sharedPreferences, Preferences.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(original, Preferences.Default);
			}
			finally
			{
				thirdApp?.Dispose();
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void ConfiguredAppActionHandlerUnsubscribesBeforeContainerOwnedServiceIsDisposed()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions, DisposableStubAppActions>();
			builder.ConfigureEssentials(essentials => essentials.OnAppAction(_ => { }));
			var app = builder.Build();
			var appActions = Assert.IsType<DisposableStubAppActions>(
				app.Services.GetRequiredService<IAppActions>());

			app.Dispose();

			Assert.True(appActions.WasUnsubscribed);
			Assert.True(appActions.IsDisposed);
		}

		[Fact]
		public void ConfiguredVersionTrackingIsRestoredAfterBridgedServicesAreDisposed()
		{
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));

			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
			builder.Services.AddSingleton<IAppInfo>(new StubAppInfo());
			builder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
			var app = builder.Build();
			var preferences = Assert.IsType<DisposableStubPreferences>(
				app.Services.GetRequiredService<IPreferences>());

			Assert.NotNull(GetStaticField(typeof(VersionTracking), "defaultImplementation"));

			app.Dispose();

			Assert.True(preferences.IsDisposed);
			Assert.True(preferences.FacadeWasCurrentDuringDispose);
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
		}

		[Fact]
		public void OverlappingUseVersionTrackingAppsShareInitializedFacadeOwnership()
		{
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			Preferences.SetDefault(new StubPreferences());
			AppInfo.SetCurrent(new StubAppInfo());

			MauiApp? firstApp = null;
			MauiApp? secondApp = null;
			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
				firstApp = firstBuilder.Build();
				var initializedVersionTracking = Assert.IsAssignableFrom<IVersionTracking>(
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
				secondApp = secondBuilder.Build();

				Assert.Same(
					initializedVersionTracking,
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));

				firstApp.Dispose();
				firstApp = null;

				Assert.Same(
					initializedVersionTracking,
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));

				secondApp.Dispose();
				secondApp = null;
				Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void OverlappingUseVersionTrackingAppDoesNotRetainDisposedRegisteredFacade()
		{
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			Preferences.SetDefault(new StubPreferences());
			AppInfo.SetCurrent(new StubAppInfo());
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IVersionTracking, DisposableStubVersionTracking>();
				firstBuilder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
				firstApp = firstBuilder.Build();
				var registeredVersionTracking = Assert.IsType<DisposableStubVersionTracking>(
					firstApp.Services.GetRequiredService<IVersionTracking>());

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
				secondApp = secondBuilder.Build();

				Assert.Equal(2, registeredVersionTracking.TrackCount);
				Assert.Same(
					registeredVersionTracking,
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));

				firstApp.Dispose();
				firstApp = null;

				Assert.True(registeredVersionTracking.IsDisposed);
				Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));

				VersionTracking.Track();

				Assert.NotSame(
					registeredVersionTracking,
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void OverlappingUseVersionTrackingAppRefreshesDisposedLazyDependencies()
		{
			var fallbackPreferences = new StubPreferences();
			Preferences.SetDefault(fallbackPreferences);
			AppInfo.SetCurrent(new StubAppInfo("3.0.0", "3"));
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
				firstBuilder.Services.AddSingleton<IAppInfo>(
					_ => new DisposableStubAppInfo("1.0.0", "1"));
				firstApp = firstBuilder.Build();
				var providerPreferences = Assert.IsType<DisposableStubPreferences>(
					firstApp.Services.GetRequiredService<IPreferences>());
				var providerAppInfo = Assert.IsType<DisposableStubAppInfo>(
					firstApp.Services.GetRequiredService<IAppInfo>());

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
				secondApp = secondBuilder.Build();

				Assert.Equal("1.0.0", VersionTracking.CurrentVersion);

				firstApp.Dispose();
				firstApp = null;

				Assert.True(providerPreferences.IsDisposed);
				Assert.True(providerAppInfo.IsDisposed);
				Assert.Same(fallbackPreferences, Preferences.Default);
				Assert.Equal("3.0.0", VersionTracking.CurrentVersion);

				secondApp.Dispose();
				secondApp = null;
				Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void UseVersionTrackingDoesNotOwnExternallyInitializedFacade()
		{
			Preferences.SetDefault(new StubPreferences());
			AppInfo.SetCurrent(new StubAppInfo());
			var externalVersionTracking = VersionTracking.Default;

			var builder = MauiApp.CreateBuilder();
			builder.ConfigureEssentials(essentials => essentials.UseVersionTracking());
			using (builder.Build())
			{
				Assert.Same(
					externalVersionTracking,
					GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			}

			Assert.Same(
				externalVersionTracking,
				GetStaticField(typeof(VersionTracking), "defaultImplementation"));
		}

		[Fact]
		public async Task ConcurrentVersionTrackingDefaultReadsShareOneImplementation()
		{
			Preferences.SetDefault(new StubPreferences());
			AppInfo.SetCurrent(new StubAppInfo());
			VersionTracking.SetDefault(null);
			const int readerCount = 12;
			using var start = new Barrier(readerCount);

			var readers = Enumerable.Range(0, readerCount)
				.Select(_ => Task.Factory.StartNew(
					() =>
					{
						Assert.True(start.SignalAndWait(TimeSpan.FromSeconds(30)));
						return VersionTracking.Default;
					},
					CancellationToken.None,
					TaskCreationOptions.LongRunning,
					TaskScheduler.Default))
				.ToArray();

			var implementations = await Task.WhenAll(readers).WaitAsync(TimeSpan.FromSeconds(30));
			Assert.All(implementations, implementation => Assert.Same(implementations[0], implementation));
			Assert.Same(implementations[0], VersionTracking.GetDefault());
		}

		[Fact]
		public void LazyVersionTrackingIsRestoredAfterBridgedServicesAreDisposed()
		{
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));

			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences, DisposableStubPreferences>();
			builder.Services.AddSingleton<IAppInfo>(new StubAppInfo());
			var app = builder.Build();
			var preferences = Assert.IsType<DisposableStubPreferences>(
				app.Services.GetRequiredService<IPreferences>());

			_ = VersionTracking.IsFirstLaunchEver;
			Assert.NotNull(GetStaticField(typeof(VersionTracking), "defaultImplementation"));

			app.Dispose();

			Assert.True(preferences.IsDisposed);
			Assert.True(preferences.FacadeWasCurrentDuringDispose);
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
		}

		[Fact]
		public void AppInfoBridgeDoesNotReadPackageNameForImplicitSecureStorage()
		{
			var appInfo = new ThrowingPackageNameAppInfo();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppInfo>(appInfo);

			using var app = builder.Build();
			Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
		}

		[Fact]
		public void AppInfoBridgePreservesHistoricalSecureStorageAlias()
		{
			var historicalAlias = new SecureStorageImplementation().Alias;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppInfo>(
				new StubAppInfo(packageName: "custom.package"));

			using var app = builder.Build();
			var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
			Assert.Equal(historicalAlias, wrapper.Alias);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void OverlappingAppInfoBridgesPreserveHistoricalSecureStorageAlias(
			bool disposeSecondAppFirst)
		{
			var historicalAlias = new SecureStorageImplementation().Alias;
			var firstAppInfo = new StubAppInfo(packageName: "first.package");
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppInfo>(firstAppInfo);
				firstApp = firstBuilder.Build();
				var firstSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				var secondBuilder = MauiApp.CreateBuilder();
				var secondAppInfo = new StubAppInfo(packageName: "second.package");
				secondBuilder.Services.AddSingleton<IAppInfo>(secondAppInfo);
				secondApp = secondBuilder.Build();
				var secondSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				Assert.Same(secondAppInfo, AppInfo.Current);
				Assert.Same(secondSecureStorage, SecureStorage.Default);
				Assert.Equal(historicalAlias, firstSecureStorage.Alias);
				Assert.Equal(historicalAlias, secondSecureStorage.Alias);

				if (disposeSecondAppFirst)
				{
					secondApp.Dispose();
					secondApp = null;

					Assert.Same(firstAppInfo, AppInfo.Current);
					Assert.Same(firstSecureStorage, SecureStorage.Default);

					firstApp?.Dispose();
					firstApp = null;
				}
				else
				{
					firstApp?.Dispose();
					firstApp = null;

					Assert.Same(secondAppInfo, AppInfo.Current);
					Assert.Same(secondSecureStorage, SecureStorage.Default);

					secondApp.Dispose();
					secondApp = null;
				}

				Assert.Null(GetStaticField(typeof(AppInfo), "currentImplementation"));
				Assert.Null(GetStaticField(typeof(SecureStorage), "defaultImplementation"));
			}
			finally
			{
				firstApp?.Dispose();
				secondApp?.Dispose();
			}
		}

		[Fact]
		public void AppInfoSecureStorageDoesNotShadowNewerExplicitSecureStorage()
		{
			var explicitSecureStorage = new StubSecureStorage();
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;
			MauiApp? secureStorageApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "first.package"));
				firstApp = firstBuilder.Build();
				var firstSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "second.package"));
				secondApp = secondBuilder.Build();

				var secureStorageBuilder = MauiApp.CreateBuilder();
				secureStorageBuilder.Services.AddSingleton<ISecureStorage>(explicitSecureStorage);
				secureStorageApp = secureStorageBuilder.Build();
				Assert.Same(explicitSecureStorage, SecureStorage.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(explicitSecureStorage, SecureStorage.Default);

				secureStorageApp.Dispose();
				secureStorageApp = null;
				Assert.Same(firstSecureStorage, SecureStorage.Default);

				firstApp.Dispose();
				firstApp = null;
				Assert.Null(GetStaticField(typeof(AppInfo), "currentImplementation"));
				Assert.Null(GetStaticField(typeof(SecureStorage), "defaultImplementation"));
			}
			finally
			{
				secureStorageApp?.Dispose();
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void DirectSecureStorageOutlivesOverlappingAppInfoBridges()
		{
			var originalAppInfo = GetStaticField(typeof(AppInfo), "currentImplementation");
			var originalSecureStorage = GetStaticField(typeof(SecureStorage), "defaultImplementation");
			var directSecureStorage = new StubSecureStorage();
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "first.package"));
				firstApp = firstBuilder.Build();

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "second.package"));
				secondApp = secondBuilder.Build();

				SecureStorage.SetDefault(directSecureStorage);
				secondApp.Dispose();
				secondApp = null;
				Assert.Same(directSecureStorage, SecureStorage.Default);

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(directSecureStorage, SecureStorage.Default);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
				SecureStorage.SetDefault((ISecureStorage?)originalSecureStorage);
				AppInfo.SetCurrent((IAppInfo?)originalAppInfo);
			}
		}

		[Fact]
		public void DirectAppInfoWriterDoesNotInvalidateInstalledSecureStorageBridge()
		{
			var originalAppInfo = GetStaticField(typeof(AppInfo), "currentImplementation");
			var originalSecureStorage = GetStaticField(typeof(SecureStorage), "defaultImplementation");
			var appInfo = new StubAppInfo(packageName: "app.package");
			var directAppInfo = new StubAppInfo(packageName: "direct.package");
			MauiApp? app = null;

			try
			{
				var builder = MauiApp.CreateBuilder();
				builder.Services.AddSingleton<IAppInfo>(appInfo);
				app = builder.Build();
				var appSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				AppInfo.SetCurrent(directAppInfo);
				Assert.Same(directAppInfo, AppInfo.Current);
				Assert.Same(appSecureStorage, SecureStorage.Default);

				app.Dispose();
				app = null;
				Assert.Same(directAppInfo, AppInfo.Current);
			}
			finally
			{
				app?.Dispose();
				SecureStorage.SetDefault((ISecureStorage?)originalSecureStorage);
				AppInfo.SetCurrent((IAppInfo?)originalAppInfo);
			}
		}

		[Fact]
		public void DirectAppInfoWriterSurvivesSecureStorageReconciliation()
		{
			var originalAppInfo = GetStaticField(typeof(AppInfo), "currentImplementation");
			var originalSecureStorage = GetStaticField(typeof(SecureStorage), "defaultImplementation");
			var firstAppInfo = new StubAppInfo(packageName: "first.package");
			var directAppInfo = new StubAppInfo(packageName: "direct.package");
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppInfo>(firstAppInfo);
				firstApp = firstBuilder.Build();
				var firstSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "second.package"));
				secondBuilder.Services.AddSingleton<ISecureStorage>(new StubSecureStorage());
				secondApp = secondBuilder.Build();

				AppInfo.SetCurrent(directAppInfo);
				secondApp.Dispose();
				secondApp = null;

				Assert.Same(directAppInfo, AppInfo.Current);
				Assert.Same(firstSecureStorage, SecureStorage.Default);

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(directAppInfo, AppInfo.Current);
				Assert.Same(
					originalSecureStorage,
					GetStaticField(typeof(SecureStorage), "defaultImplementation"));
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
				SecureStorage.SetDefault((ISecureStorage?)originalSecureStorage);
				AppInfo.SetCurrent((IAppInfo?)originalAppInfo);
			}
		}

		[Fact]
		public async Task AppInfoSecureStorageReleasesDisposedPredecessor()
		{
			MauiApp? predecessorApp = null;
			MauiApp? successorApp = null;
			WeakReference? predecessorSecureStorageReference = null;

			try
			{
				(predecessorApp, predecessorSecureStorageReference) =
					BuildDisposableSecureStorageApp();

				var successorBuilder = MauiApp.CreateBuilder();
				successorBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "successor.package"));
				successorApp = successorBuilder.Build();
				Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				predecessorApp.Dispose();
				predecessorApp = null;

				Assert.False(
					await TestHelpers.WaitForCollect(predecessorSecureStorageReference),
					"Installed AppInfo SecureStorage should not retain a disposed predecessor assignment.");
			}
			finally
			{
				successorApp?.Dispose();
				predecessorApp?.Dispose();
			}
		}

		static (MauiApp App, WeakReference SecureStorageReference) BuildDisposableSecureStorageApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<ISecureStorage>(_ => new StubSecureStorage());
			var app = builder.Build();
			return (
				app,
				new WeakReference(app.Services.GetRequiredService<ISecureStorage>()));
		}

		[Fact]
		public void OverlappingMauiAppsOwnIndependentLazyVersionTrackingFacades()
		{
			Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));

			MauiApp? firstApp = null;
			MauiApp? secondApp = null;
			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IPreferences>(new StubPreferences());
				firstBuilder.Services.AddSingleton<IAppInfo>(new StubAppInfo());
				firstApp = firstBuilder.Build();
				var firstVersionTracking = VersionTracking.Default;

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(new StubPreferences());
				secondBuilder.Services.AddSingleton<IAppInfo>(new StubAppInfo());
				secondApp = secondBuilder.Build();
				var secondVersionTracking = VersionTracking.Default;

				Assert.NotSame(firstVersionTracking, secondVersionTracking);

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(secondVersionTracking, VersionTracking.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Null(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public void OverlappingMauiAppsOwnIndependentSecureStorageWrappersWithHistoricalAlias()
		{
			var originalAppInfo = AppInfo.Current;
			AppInfo.SetCurrent(new StubAppInfo(packageName: "platform.package"));
			var originalSecureStorage = SecureStorage.Default;
			var historicalAlias = Assert.IsType<SecureStorageImplementation>(originalSecureStorage).Alias;
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "first.package"));
				firstApp = firstBuilder.Build();
				var firstSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				Assert.False(firstSecureStorage.IsValueCreated);
				Assert.Equal(historicalAlias, firstSecureStorage.Alias);
				Assert.True(firstSecureStorage.IsValueCreated);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "second.package"));
				secondApp = secondBuilder.Build();
				var secondSecureStorage = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				Assert.NotSame(firstSecureStorage, secondSecureStorage);
				Assert.False(secondSecureStorage.IsValueCreated);
				Assert.Equal(historicalAlias, secondSecureStorage.Alias);
				Assert.True(secondSecureStorage.IsValueCreated);

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(secondSecureStorage, SecureStorage.Default);

				secondApp.Dispose();
				secondApp = null;
				Assert.Same(originalSecureStorage, SecureStorage.Default);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
				AppInfo.SetCurrent(originalAppInfo);
			}
		}

		[Fact]
		public void RegisteredSecureStorageWinsOverAppInfoOwnedFallback()
		{
			var registeredSecureStorage = new StubSecureStorage();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppInfo>(
				new StubAppInfo(packageName: "registered.package"));
			builder.Services.AddSingleton<ISecureStorage>(registeredSecureStorage);

			using (builder.Build())
				Assert.Same(registeredSecureStorage, SecureStorage.Default);

			Assert.Null(GetStaticField(typeof(SecureStorage), "defaultImplementation"));
		}

		[Fact]
		public void OverlappingMauiAppsUseVersionTrackingStorageForTheirOwnPackage()
		{
			var preferences = new RecordingStubPreferences();
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IPreferences>(preferences);
				firstBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "first.package"));
				firstApp = firstBuilder.Build();

				_ = VersionTracking.CurrentVersion;
				Assert.Equal(
					new[] { "first.package.microsoft.maui.essentials.versiontracking" },
					preferences.SharedNames);

				preferences.SharedNames.Clear();

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IPreferences>(preferences);
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "second.package"));
				secondApp = secondBuilder.Build();

				_ = VersionTracking.CurrentVersion;
				Assert.Equal(
					new[] { "second.package.microsoft.maui.essentials.versiontracking" },
					preferences.SharedNames);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task OverlappingMauiAppsRefreshLazyVersionTrackingFallbackOwner()
		{
			var originalAppInfo = AppInfo.Current;
			var fallbackAppInfo = new StubAppInfo("3.0.0", "3");
			AppInfo.SetCurrent(fallbackAppInfo);
			MauiApp? appInfoApp = null;
			MauiApp? preferencesApp = null;
			WeakReference? providerAppInfoReference = null;

			try
			{
				(appInfoApp, providerAppInfoReference) = BuildDisposableAppInfoApp();

				var preferencesBuilder = MauiApp.CreateBuilder();
				preferencesBuilder.Services.AddSingleton<IPreferences>(new StubPreferences());
				preferencesApp = preferencesBuilder.Build();

				Assert.Equal("1.0.0", VersionTracking.CurrentVersion);

				appInfoApp.Dispose();
				appInfoApp = null;

				Assert.False(
					await TestHelpers.WaitForCollect(providerAppInfoReference),
					"Disposed AppInfo should not remain rooted by VersionTracking.");
				Assert.Equal("3.0.0", VersionTracking.CurrentVersion);
			}
			finally
			{
				preferencesApp?.Dispose();
				appInfoApp?.Dispose();
				AppInfo.SetCurrent(originalAppInfo);
			}
		}

		static (MauiApp App, WeakReference AppInfoReference) BuildDisposableAppInfoApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppInfo>(
				_ => new DisposableStubAppInfo("1.0.0", "1"));
			var app = builder.Build();
			return (app, new WeakReference(app.Services.GetRequiredService<IAppInfo>()));
		}

		[Fact]
		public void LazyVersionTrackingReusesTransientBridgeDependencies()
		{
			var preferencesResolutions = 0;
			var appInfoResolutions = 0;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddTransient<IPreferences>(_ =>
			{
				preferencesResolutions++;
				return new StubPreferences();
			});
			builder.Services.AddTransient<IAppInfo>(_ =>
			{
				appInfoResolutions++;
				return new StubAppInfo();
			});

			using var app = builder.Build();
			_ = VersionTracking.CurrentVersion;

			Assert.Equal(1, preferencesResolutions);
			Assert.Equal(1, appInfoResolutions);
		}

		[Fact]
		public void LazyVersionTrackingPreservesUnregisteredFacadeLaziness()
		{
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
			Assert.Null(GetStaticField(typeof(AppInfo), "currentImplementation"));

			var appInfoBuilder = MauiApp.CreateBuilder();
			appInfoBuilder.Services.AddSingleton<IAppInfo>(new StubAppInfo());
			using (var app = appInfoBuilder.Build())
			{
				Assert.NotNull(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
				Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
			}

			var preferencesBuilder = MauiApp.CreateBuilder();
			preferencesBuilder.Services.AddSingleton<IPreferences>(new StubPreferences());
			using (var app = preferencesBuilder.Build())
			{
				Assert.NotNull(GetStaticField(typeof(VersionTracking), "defaultImplementation"));
				Assert.Null(GetStaticField(typeof(AppInfo), "currentImplementation"));
			}
		}

		[Fact]
		public void FailedBridgeResolutionRestoresPreviouslyAssignedFacade()
		{
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));

			var bridgedPreferences = new StubPreferences();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(bridgedPreferences);
			builder.Services.AddSingleton<IScreenshot>(_ => throw new InvalidOperationException("boom"));

			var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
			Assert.Equal("boom", ex.Message);
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
		}

		[Fact]
		public async Task FailedBridgeResolutionKeepsFacadeUntilAsyncProviderDisposalCompletes()
		{
			var timeout = TimeSpan.FromSeconds(10);
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));

			var bridgedPreferences = new StubPreferences();
			var factory = new AsyncOnlyServiceProviderFactory();
			PreferencesReadingAsyncDisposable? disposalProbe = null;
			var builder = MauiApp.CreateBuilder();
			builder.ConfigureContainer(factory);
			builder.Services.AddSingleton<IPreferences>(bridgedPreferences);
			builder.Services.AddSingleton<PreferencesReadingAsyncDisposable>();
			builder.Services.AddSingleton<IScreenshot>(services =>
			{
				disposalProbe = services.GetRequiredService<PreferencesReadingAsyncDisposable>();
				throw new InvalidOperationException("bridge failed");
			});

			var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
			Assert.Equal("bridge failed", ex.Message);
			Assert.NotNull(disposalProbe);

			await disposalProbe!.DisposeCompleted.Task.WaitAsync(timeout);

			Assert.Same(bridgedPreferences, disposalProbe.PreferencesDuringDispose);
			Assert.True(factory.Provider.IsDisposed);
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
		}

		[Fact]
		public void LaterInitializerFailureDisposesProviderAndRestoresFacade()
		{
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));

			var preferences = new StubPreferences();
			DisposableProbe? probe = null;
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IPreferences>(preferences);
			builder.Services.AddSingleton(_ => probe = new DisposableProbe());
			builder.Services.AddSingleton<IMauiInitializeService>(services =>
				new ThrowingInitializeService(services.GetRequiredService<DisposableProbe>()));

			var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());

			Assert.Equal("later initializer failed", ex.Message);
			Assert.NotNull(probe);
			Assert.True(probe!.IsDisposed);
			Assert.Null(GetStaticField(typeof(Preferences), "defaultImplementation"));
		}

		[Fact]
		public void InitializationAndDisposalFailuresAreAggregatedInOrder()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<ThrowingDisposable>();
			builder.Services.AddSingleton<IMauiInitializeService>(services =>
				new DoubleFailureInitializeService(services.GetRequiredService<ThrowingDisposable>()));

			var aggregate = Assert.Throws<AggregateException>(() => builder.Build());

			Assert.StartsWith("MauiApp initialization and cleanup both failed.", aggregate.Message, StringComparison.Ordinal);
			Assert.Collection(
				aggregate.InnerExceptions,
				ex =>
				{
					Assert.IsType<InvalidOperationException>(ex);
					Assert.Equal("initialization failed", ex.Message);
				},
				ex =>
				{
					Assert.IsType<InvalidOperationException>(ex);
					Assert.Equal("disposal failed", ex.Message);
				});
		}

		[Fact]
		public void EssentialsInitializationAndCleanupFailuresAreAggregatedInOrder()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions, ThrowingUnsubscribeAppActions>();
			builder.Services.AddSingleton<IVersionTracking, ThrowingVersionTracking>();
			builder.ConfigureEssentials(essentials =>
			{
				essentials.OnAppAction(_ => { });
				essentials.UseVersionTracking();
			});

			var aggregate = Assert.Throws<AggregateException>(() => builder.Build());

			Assert.StartsWith("Essentials initialization and cleanup both failed.", aggregate.Message, StringComparison.Ordinal);
			Assert.Collection(
				aggregate.InnerExceptions,
				ex =>
				{
					Assert.IsType<InvalidOperationException>(ex);
					Assert.Equal("version tracking failed", ex.Message);
				},
				ex =>
				{
					Assert.IsType<InvalidOperationException>(ex);
					Assert.Equal("unsubscribe failed", ex.Message);
				});
		}

		[Fact]
		public void AppActionsUnsubscribeAndFacadeCleanupFailuresAreAggregated()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions, ThrowingUnsubscribeAppActions>();
			builder.ConfigureEssentials(essentials => essentials.OnAppAction(_ => { }));
			var app = builder.Build();

			var cleanupType = typeof(EssentialsExtensions).GetNestedType(
				"EssentialsCleanup",
				System.Reflection.BindingFlags.NonPublic)
				?? throw new InvalidOperationException("EssentialsCleanup type not found.");
			var cleanup = app.Services.GetRequiredService(cleanupType);
			var facadeCleanupsField = cleanupType.GetField(
				"_facadeCleanups",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				?? throw new InvalidOperationException("EssentialsCleanup facade list not found.");
			var facadeCleanups = Assert.IsType<List<Action>>(facadeCleanupsField.GetValue(cleanup));
			facadeCleanups.Add(() => throw new InvalidOperationException("facade cleanup failed"));

			var aggregate = Assert.Throws<AggregateException>(() => app.Dispose());

			Assert.Collection(
				aggregate.InnerExceptions,
				ex => Assert.Equal("unsubscribe failed", ex.Message),
				ex => Assert.Equal("facade cleanup failed", ex.Message));
		}

		[Fact]
		public void FacadeCleanupRunsAllActionsAndClearsAfterFailures()
		{
			var executionOrder = new List<int>();
			var cleanups = new List<Action>
				{
					() =>
					{
						executionOrder.Add(1);
						throw new InvalidOperationException("first cleanup failed");
					},
					() => executionOrder.Add(2),
					() =>
					{
						executionOrder.Add(3);
						throw new InvalidOperationException("third cleanup failed");
					},
				};

			var aggregate = Assert.Throws<AggregateException>(
				() => EssentialsExtensions.RestoreFacadeCleanups(cleanups));

			Assert.Equal(new[] { 3, 2, 1 }, executionOrder);
			Assert.Empty(cleanups);
			Assert.Collection(
				aggregate.InnerExceptions,
				ex => Assert.Equal("third cleanup failed", ex.Message),
				ex => Assert.Equal("first cleanup failed", ex.Message));
		}

		[Fact]
		public async Task ConcurrentMauiAppBuildsDoNotHoldGlobalLockAcrossEssentialsDelegates()
		{
			// Regression test for the process-global s_essentialsBridgeLock being held across user
			// code. The ConfigureEssentials delegates are forced (via a Barrier) to run at the same
			// time; if initialization serialized them under a global lock, the second build could
			// never enter its delegate and this would deadlock. Facade ownership is still serialized
			// per type by FacadeBridgeState<T>.SyncRoot, which the ownership tests cover.
			var timeout = TimeSpan.FromSeconds(30);
			var probe = new InitializationConcurrencyProbe(timeout);
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.ConfigureEssentials(_ => probe.Enter());
			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.ConfigureEssentials(_ => probe.Enter());
			using var start = new Barrier(2);

			var firstBuild = StartBuild(firstBuilder);
			var secondBuild = StartBuild(secondBuilder);

			var apps = await Task.WhenAll(firstBuild, secondBuild).WaitAsync(timeout);
			try
			{
				// Both delegates ran concurrently: user code is no longer serialized by a global lock.
				Assert.Equal(2, probe.MaxConcurrent);
				Assert.Equal(2, probe.EntryCount);
			}
			finally
			{
				for (int i = apps.Length - 1; i >= 0; i--)
					apps[i].Dispose();
			}

			Task<MauiApp> StartBuild(MauiAppBuilder builder) =>
				Task.Factory.StartNew(
					() =>
					{
						Assert.True(
							start.SignalAndWait(timeout),
							"Timed out waiting for both build tasks to start.");
						return builder.Build();
					},
					CancellationToken.None,
					TaskCreationOptions.LongRunning,
					TaskScheduler.Default);
		}

		[Fact]
		public async Task EssentialsInitializationDoesNotDeadlockOnReentrantBuildFromDelegate()
		{
			// A ConfigureEssentials delegate that reentrantly builds and disposes another MauiApp on
			// a different thread must not deadlock. Holding a process-global lock across the delegate
			// (the old behavior) blocked the nested build's own initialization on that lock.
			var timeout = TimeSpan.FromSeconds(30);
			var builder = MauiApp.CreateBuilder();
			builder.ConfigureEssentials(_ =>
			{
				var nested = Task.Run(() =>
				{
					var nestedBuilder = MauiApp.CreateBuilder();
					nestedBuilder.ConfigureEssentials();
					nestedBuilder.Build().Dispose();
				});

				Assert.True(
					nested.Wait(timeout),
					"Nested MauiApp build deadlocked: initialization held a lock across the delegate.");
			});

			var app = await Task.Run(builder.Build).WaitAsync(timeout);
			app.Dispose();
		}

		[Fact]
		public async Task ConfiguredAppActionsDoesNotBlockBuildWhenSetAsyncAwaitsDispatcher()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var dispatchedAction = new TaskCompletionSource<Action>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var dispatcher = new Microsoft.Maui.UnitTests.DispatcherStub(
				isInvokeRequired: () => false,
				invokeOnMainThread: action => dispatchedAction.TrySetResult(action));
			var appActions = new DispatchingStubAppActions(dispatcher);
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions>(appActions);
			builder.ConfigureEssentials(essentials =>
				essentials.AddAppAction(new AppAction("test", "Test")));

			var buildTask = Task.Factory.StartNew(
				builder.Build,
				CancellationToken.None,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default);

			await appActions.SetStarted.Task.WaitAsync(timeout);
			var action = await dispatchedAction.Task.WaitAsync(timeout);
			var completedBeforeDispatch = ReferenceEquals(
				await Task.WhenAny(buildTask, Task.Delay(timeout)),
				buildTask);
			action();

			var app = await buildTask.WaitAsync(timeout);
			try
			{
				await appActions.SetCompleted.Task.WaitAsync(timeout);
				Assert.True(completedBeforeDispatch);
			}
			finally
			{
				app.Dispose();
			}
		}

		[Fact]
		public async Task ConfiguredAppActionsLogsUnexpectedSetFailure()
		{
			var loggerFactory = new RecordingLoggerFactory();
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);
			builder.Services.AddSingleton<IAppActions, FaultingStubAppActions>();
			builder.ConfigureEssentials(essentials =>
				essentials.AddAppAction(new AppAction("test", "Test")));

			using var app = builder.Build();
			var exception = await loggerFactory.Error.Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.IsType<InvalidOperationException>(exception);
			Assert.Equal("app actions failed", exception.Message);
		}

		[Fact]
		public async Task AppActionsWorkerContinuesWhenFailureLoggingThrows()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var loggerFactory = new ThrowingLoggerFactory();
			var second = new ControlledStubAppActions(Task.CompletedTask, _ => { });
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<ILoggerFactory>(loggerFactory);
				firstBuilder.Services.AddSingleton<IAppActions, FaultingStubAppActions>();
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await loggerFactory.LogStarted.Task.WaitAsync(timeout);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppActions>(second);
				secondBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("second", "Second")));
				secondApp = secondBuilder.Build();

				loggerFactory.ReleaseLog.TrySetResult(true);
				await second.FirstCallCompleted.Task.WaitAsync(timeout);
			}
			finally
			{
				loggerFactory.ReleaseLog.TrySetResult(true);
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task LaterAppActionsConfigurationWinsWhenEarlierSetCompletesLast()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			IReadOnlyList<AppAction>? publishedActions = null;
			var first = new ControlledStubAppActions(releaseFirst.Task, actions => publishedActions = actions);
			var second = new ControlledStubAppActions(Task.CompletedTask, actions => publishedActions = actions);
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppActions>(first);
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await first.FirstCallStarted.Task.WaitAsync(timeout);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppActions>(second);
				secondBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("second", "Second")));
				secondApp = secondBuilder.Build();

				releaseFirst.TrySetResult(true);
				await second.FirstCallCompleted.Task.WaitAsync(timeout);

				Assert.Equal("second", Assert.Single(publishedActions!).Id);
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task DisposedQueuedAppActionsConfigurationDoesNotPublish()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			IReadOnlyList<AppAction>? publishedActions = null;
			var first = new ControlledStubAppActions(releaseFirst.Task, actions => publishedActions = actions);
			var second = new ControlledStubAppActions(Task.CompletedTask, actions => publishedActions = actions);
			MauiApp? firstApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppActions>(first);
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await first.FirstCallStarted.Task.WaitAsync(timeout);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppActions>(second);
				secondBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("second", "Second")));
				secondBuilder.Build().Dispose();

				releaseFirst.TrySetResult(true);
				await first.SecondCallCompleted.Task.WaitAsync(timeout);

				Assert.Equal(0, second.CallCount);
				Assert.Equal("first", Assert.Single(publishedActions!).Id);
			}
			finally
			{
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task DisposingLastConfiguredAppActionsPreservesPublishedLaunchActions()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var noClearTimeout = TimeSpan.FromSeconds(1);
			IReadOnlyList<AppAction>? publishedActions = null;
			var appActions = new ControlledStubAppActions(
				Task.CompletedTask,
				actions => Volatile.Write(ref publishedActions, actions));
			MauiApp? app = null;

			try
			{
				var builder = MauiApp.CreateBuilder();
				builder.Services.AddSingleton<IAppActions>(appActions);
				builder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("launch", "Launch")));
				app = builder.Build();

				await appActions.FirstCallCompleted.Task.WaitAsync(timeout);
				app.Dispose();
				app = null;

				var unexpectedClear = await Task.WhenAny(
					appActions.SecondCallCompleted.Task,
					Task.Delay(noClearTimeout));

				Assert.NotSame(appActions.SecondCallCompleted.Task, unexpectedClear);
				Assert.Equal(1, appActions.CallCount);
				Assert.Equal("launch", Assert.Single(Volatile.Read(ref publishedActions)!).Id);
			}
			finally
			{
				app?.Dispose();
			}
		}

		[Fact]
		public async Task DisposedRunningAppActionsCompletesBeforeLaterConfigurationStarts()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var noStartTimeout = TimeSpan.FromSeconds(1);
			var releaseFirst = new TaskCompletionSource<bool>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var publishLock = new object();
			var publishOrder = new List<string>();
			void RecordPublish(IReadOnlyList<AppAction> actions)
			{
				lock (publishLock)
					publishOrder.Add(actions.Single().Id);
			}

			var first = new ControlledStubAppActions(releaseFirst.Task, RecordPublish);
			var second = new ControlledStubAppActions(
				Task.CompletedTask,
				RecordPublish);
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppActions>(first);
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await first.FirstCallStarted.Task.WaitAsync(timeout);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppActions>(second);
				secondBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("second", "Second")));
				secondApp = secondBuilder.Build();

				firstApp.Dispose();
				firstApp = null;

				var unexpectedStart = await Task.WhenAny(
					second.FirstCallStarted.Task,
					Task.Delay(noStartTimeout));
				Assert.NotSame(second.FirstCallStarted.Task, unexpectedStart);

				releaseFirst.TrySetResult(true);
				await first.FirstCallCompleted.Task.WaitAsync(timeout);
				await second.FirstCallCompleted.Task.WaitAsync(timeout);

				lock (publishLock)
					Assert.Equal(new[] { "first", "second" }, publishOrder);
				Assert.Equal(1, second.CallCount);
			}
			finally
			{
				releaseFirst.TrySetResult(true);
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task RemovedAppActionsWriteThatDoesNotCompleteDoesNotBlockSuccessorLifecycle()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var noStartTimeout = TimeSpan.FromSeconds(1);
			var releaseFirst = new TaskCompletionSource<bool>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var first = new ControlledStubAppActions(releaseFirst.Task, _ => { });
			var second = new ControlledStubAppActions(Task.CompletedTask, _ => { });
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppActions>(first);
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await first.FirstCallStarted.Task.WaitAsync(timeout);

				await Task.Run(firstApp.Dispose).WaitAsync(timeout);
				firstApp = null;

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppActions>(second);
				secondBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("second", "Second")));
				secondApp = await Task.Run(secondBuilder.Build).WaitAsync(timeout);

				var unexpectedStart = await Task.WhenAny(
					second.FirstCallStarted.Task,
					Task.Delay(noStartTimeout));
				Assert.NotSame(second.FirstCallStarted.Task, unexpectedStart);

				await Task.Run(secondApp.Dispose).WaitAsync(timeout);
				secondApp = null;
				Assert.Equal(0, second.CallCount);
			}
			finally
			{
				releaseFirst.TrySetResult(true);
				if (first.FirstCallStarted.Task.IsCompleted)
					await first.FirstCallCompleted.Task.WaitAsync(timeout);
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task ActiveRunningAppActionsCompletesBeforeLaterConfigurationStarts()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var noStartTimeout = TimeSpan.FromSeconds(1);
			var releaseFirst = new TaskCompletionSource<bool>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var firstSetAsyncRunning = 0;
			var overlapDetected = 0;
			var first = new OverlapDetectingStubAppActions(
				releaseFirst.Task,
				onStart: () => Volatile.Write(ref firstSetAsyncRunning, 1),
				onEnd: () => Volatile.Write(ref firstSetAsyncRunning, 0));
			var second = new OverlapDetectingStubAppActions(
				Task.CompletedTask,
				onStart: () =>
				{
					if (Volatile.Read(ref firstSetAsyncRunning) != 0)
						Interlocked.Exchange(ref overlapDetected, 1);
				},
				onEnd: () => { });
			MauiApp? firstApp = null;
			MauiApp? secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppActions>(first);
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await first.SetStarted.Task.WaitAsync(timeout);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppActions>(second);
				secondBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("second", "Second")));
				secondApp = secondBuilder.Build();

				var unexpectedStart = await Task.WhenAny(
					second.SetStarted.Task,
					Task.Delay(noStartTimeout));
				Assert.NotSame(second.SetStarted.Task, unexpectedStart);

				releaseFirst.TrySetResult(true);
				await first.SetCompleted.Task.WaitAsync(timeout);
				await second.SetCompleted.Task.WaitAsync(timeout);

				Assert.Equal(0, Volatile.Read(ref overlapDetected));
			}
			finally
			{
				releaseFirst.TrySetResult(true);
				secondApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		[Fact]
		public async Task DisposedQueuedAppActionsCanCollectBehindHungPredecessor()
		{
			var timeout = TimeSpan.FromSeconds(10);
			var releaseFirst = new TaskCompletionSource<bool>(
				TaskCreationOptions.RunContinuationsAsynchronously);
			var first = new ControlledStubAppActions(releaseFirst.Task, _ => { });
			MauiApp? firstApp = null;
			MauiApp? queuedApp = null;
			WeakReference? queuedAppActionsReference = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppActions>(first);
				firstBuilder.ConfigureEssentials(essentials =>
					essentials.AddAppAction(new AppAction("first", "First")));
				firstApp = firstBuilder.Build();
				await first.FirstCallStarted.Task.WaitAsync(timeout);

				(queuedApp, queuedAppActionsReference) = BuildQueuedAppActionsApp();
				queuedApp.Dispose();
				queuedApp = null;

				Assert.False(
					await TestHelpers.WaitForCollect(queuedAppActionsReference),
					"A disposed queued AppActions implementation should not remain rooted behind a hung predecessor.");
			}
			finally
			{
				releaseFirst.TrySetResult(true);
				queuedApp?.Dispose();
				firstApp?.Dispose();
			}
		}

		static (MauiApp App, WeakReference AppActionsReference) BuildQueuedAppActionsApp()
		{
			var appActions = new ControlledStubAppActions(Task.CompletedTask, _ => { });
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IAppActions>(appActions);
			builder.ConfigureEssentials(essentials =>
				essentials.AddAppAction(new AppAction("queued", "Queued")));
			return (builder.Build(), new WeakReference(appActions));
		}

		[Fact]
		public void LaterMauiAppCanReplaceStaticFacade()
		{
			var firstMock = new StubPreferences();
			var firstBuilder = MauiApp.CreateBuilder();
			firstBuilder.Services.AddSingleton<IPreferences>(firstMock);

			using var firstApp = firstBuilder.Build();
			Assert.Same(firstMock, Preferences.Default);

			var secondMock = new StubPreferences();
			var secondBuilder = MauiApp.CreateBuilder();
			secondBuilder.Services.AddSingleton<IPreferences>(secondMock);

			using var secondApp = secondBuilder.Build();

			Assert.Same(secondMock, Preferences.Default);
		}

		static object? GetStaticField(Type type, string fieldName)
		{
			var field = type.GetField(fieldName,
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			Assert.NotNull(field);
			return field!.GetValue(null);
		}

		// Stub implementations for testing
		class StubPreferences : IPreferences
		{
			public bool ContainsKey(string key, string? sharedName = null) => false;
			public void Remove(string key, string? sharedName = null) { }
			public void Clear(string? sharedName = null) { }
			public void Set<T>(string key, T value, string? sharedName = null) { }
			public T Get<T>(string key, T defaultValue, string? sharedName = null) => defaultValue;
		}

		sealed class RecordingStubPreferences : IPreferences
		{
			public HashSet<string> SharedNames { get; } = new(StringComparer.Ordinal);

			public bool ContainsKey(string key, string? sharedName = null)
			{
				Record(sharedName);
				return false;
			}

			public void Remove(string key, string? sharedName = null) =>
				Record(sharedName);

			public void Clear(string? sharedName = null) =>
				Record(sharedName);

			public void Set<T>(string key, T value, string? sharedName = null) =>
				Record(sharedName);

			public T Get<T>(string key, T defaultValue, string? sharedName = null)
			{
				Record(sharedName);
				return defaultValue;
			}

			void Record(string? sharedName) =>
				SharedNames.Add(Assert.IsType<string>(sharedName));
		}

		sealed class StubSecureStorage : ISecureStorage
		{
			public Task<string?> GetAsync(string key) =>
				Task.FromResult<string?>(null);

			public Task SetAsync(string key, string value) =>
				Task.CompletedTask;

			public bool Remove(string key) =>
				false;

			public void RemoveAll()
			{
			}
		}

		sealed class DisposableStubPreferences : StubPreferences, IDisposable
		{
			public bool FacadeWasCurrentDuringDispose { get; private set; }

			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				FacadeWasCurrentDuringDispose = ReferenceEquals(this, Preferences.Default);
				IsDisposed = true;
			}
		}

		sealed class AsyncFacadeReadingDisposable : IAsyncDisposable
		{
			public IPreferences? FacadeDuringDispose { get; private set; }

			public ValueTask DisposeAsync()
			{
				FacadeDuringDispose = Preferences.Default;
				return ValueTask.CompletedTask;
			}
		}

		sealed class DisposableProbe : IDisposable
		{
			public bool IsDisposed { get; private set; }

			public void Dispose()
			{
				IsDisposed = true;
			}
		}

		sealed class NonDisposableServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
		{
			ServiceProvider? _innerProvider;

			public IServiceCollection CreateBuilder(IServiceCollection services) => services;

			public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
			{
				_innerProvider = containerBuilder.BuildServiceProvider();
				return new NonDisposableServiceProvider(_innerProvider);
			}

			public void DisposeInnerProvider()
			{
				_innerProvider?.Dispose();
			}
		}

		sealed class NonDisposableServiceProvider : IServiceProvider
		{
			readonly IServiceProvider _innerProvider;

			public NonDisposableServiceProvider(IServiceProvider innerProvider)
			{
				_innerProvider = innerProvider;
			}

			public object? GetService(Type serviceType) =>
				_innerProvider.GetService(serviceType);
		}

		sealed class ThrowingInitializeService : IMauiInitializeService
		{
			readonly DisposableProbe _probe;

			public ThrowingInitializeService(DisposableProbe probe)
			{
				_probe = probe;
			}

			public void Initialize(IServiceProvider services)
			{
				Assert.False(_probe.IsDisposed);
				throw new InvalidOperationException("later initializer failed");
			}
		}

		sealed class AsyncOnlyServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
		{
			public AsyncOnlyServiceProvider Provider { get; private set; } = null!;

			public IServiceCollection CreateBuilder(IServiceCollection services) => services;

			public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
			{
				Provider = new AsyncOnlyServiceProvider(containerBuilder.BuildServiceProvider());
				return Provider;
			}
		}

		sealed class AsyncOnlyServiceProvider : IServiceProvider, IAsyncDisposable
		{
			readonly ServiceProvider _innerProvider;

			public AsyncOnlyServiceProvider(ServiceProvider innerProvider)
			{
				_innerProvider = innerProvider;
			}

			public bool IsDisposed { get; private set; }

			public object? GetService(Type serviceType) =>
				_innerProvider.GetService(serviceType);

			public async ValueTask DisposeAsync()
			{
				await _innerProvider.DisposeAsync();
				IsDisposed = true;
			}
		}

		sealed class PreferencesReadingAsyncDisposable : IAsyncDisposable
		{
			public TaskCompletionSource<bool> DisposeCompleted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public IPreferences? PreferencesDuringDispose { get; private set; }

			public async ValueTask DisposeAsync()
			{
				try
				{
					await Task.Yield();
					PreferencesDuringDispose = Preferences.Default;
				}
				finally
				{
					DisposeCompleted.TrySetResult(true);
				}
			}
		}

		sealed class ThrowingDisposable : IDisposable
		{
			public void Dispose()
			{
				throw new InvalidOperationException("disposal failed");
			}
		}

		sealed class DoubleFailureInitializeService : IMauiInitializeService
		{
			readonly ThrowingDisposable _dependency;

			public DoubleFailureInitializeService(ThrowingDisposable dependency)
			{
				_dependency = dependency;
			}

			public void Initialize(IServiceProvider services)
			{
				Assert.NotNull(_dependency);
				throw new InvalidOperationException("initialization failed");
			}
		}

		sealed class InitializationConcurrencyProbe
		{
			readonly Barrier _bothEntered = new(2);
			readonly TimeSpan _timeout;
			int _active;
			int _entries;
			int _maxConcurrent;

			public InitializationConcurrencyProbe(TimeSpan timeout)
			{
				_timeout = timeout;
			}

			public int EntryCount => Volatile.Read(ref _entries);

			public int MaxConcurrent => Volatile.Read(ref _maxConcurrent);

			public void Enter()
			{
				var active = Interlocked.Increment(ref _active);
				UpdateMax(active);
				Interlocked.Increment(ref _entries);

				// Block until BOTH initializers are inside their delegate at the same time. If the
				// caller held a process-global lock across this delegate, the second initializer
				// could never reach here and this would time out.
				Assert.True(
					_bothEntered.SignalAndWait(_timeout),
					"The two Essentials initializers did not run their delegates concurrently.");

				Interlocked.Decrement(ref _active);
			}

			void UpdateMax(int value)
			{
				var current = Volatile.Read(ref _maxConcurrent);
				while (value > current)
				{
					var observed = Interlocked.CompareExchange(ref _maxConcurrent, value, current);
					if (observed == current)
						return;

					current = observed;
				}
			}
		}

		class StubBattery : IBattery
		{
			public double ChargeLevel => 1.0;
			public BatteryState State => BatteryState.Full;
			public BatteryPowerSource PowerSource => BatteryPowerSource.AC;
			public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;
			public event EventHandler<BatteryInfoChangedEventArgs>? BatteryInfoChanged { add { } remove { } }
			public event EventHandler<EnergySaverStatusChangedEventArgs>? EnergySaverStatusChanged { add { } remove { } }
		}

		class StubAppInfo : IAppInfo
		{
			readonly string _packageName;
			readonly string _versionString;
			readonly string _buildString;

			public StubAppInfo(
				string versionString = "1.0.0",
				string buildString = "1",
				string packageName = "test.package")
			{
				_versionString = versionString;
				_buildString = buildString;
				_packageName = packageName;
			}

			public virtual string PackageName => _packageName;
			public string Name => "Test";
			public virtual string VersionString => _versionString;
			public Version Version => System.Version.Parse(VersionString);
			public virtual string BuildString => _buildString;
			public AppTheme RequestedTheme => AppTheme.Light;
			public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;
			public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;
			public void ShowSettingsUI() { }
		}

		sealed class ThrowingPackageNameAppInfo : StubAppInfo
		{
			public override string PackageName =>
				throw new InvalidOperationException("Implicit SecureStorage must use the native package identity.");
		}

		sealed class DisposableStubAppInfo : StubAppInfo, IDisposable
		{
			public DisposableStubAppInfo(string versionString, string buildString)
				: base(versionString, buildString)
			{
			}

			public bool IsDisposed { get; private set; }

			public override string VersionString
			{
				get
				{
					ThrowIfDisposed();
					return base.VersionString;
				}
			}

			public override string BuildString
			{
				get
				{
					ThrowIfDisposed();
					return base.BuildString;
				}
			}

			public void Dispose()
			{
				IsDisposed = true;
			}

			void ThrowIfDisposed()
			{
				if (IsDisposed)
					throw new ObjectDisposedException(nameof(DisposableStubAppInfo));
			}
		}

		class StubConnectivity : IConnectivity
		{
			public NetworkAccess NetworkAccess => NetworkAccess.Internet;
			public IEnumerable<ConnectionProfile> ConnectionProfiles => Array.Empty<ConnectionProfile>();
			public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged { add { } remove { } }
		}

		class StubGeocoding : IGeocoding
		{
			public Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
				Task.FromResult<IEnumerable<Placemark>>(Array.Empty<Placemark>());
			public Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
				Task.FromResult<IEnumerable<Location>>(Array.Empty<Location>());
		}

		class StubContacts : IContacts
		{
			public Task<Contact?> PickContactAsync() =>
				Task.FromResult<Contact?>(null);
			public Task<IEnumerable<Contact>> GetAllAsync(System.Threading.CancellationToken cancellationToken = default) =>
				Task.FromResult<IEnumerable<Contact>>(Array.Empty<Contact>());
		}

		class StubPermissions : IPermissions
		{
			public Task<PermissionStatus> CheckStatusAsync<TPermission>()
				where TPermission : Permissions.BasePermission, new() =>
				Task.FromResult(PermissionStatus.Granted);

			public Task<PermissionStatus> RequestAsync<TPermission>()
				where TPermission : Permissions.BasePermission, new() =>
				Task.FromResult(PermissionStatus.Granted);

			public bool ShouldShowRationale<TPermission>()
				where TPermission : Permissions.BasePermission, new() => false;
		}

		sealed class StubPasskeys : IPasskeys
		{
			public bool IsSupported => true;

			public Task<PasskeyCreationResponse> CreateAsync(
				PasskeyCreationOptions options,
				CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();

			public Task<PasskeyAssertionResponse> AssertAsync(
				PasskeyRequestOptions options,
				CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
		}

		class StubWebAuthenticator : IWebAuthenticator
		{
			public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
				Task.FromException<WebAuthenticatorResult>(new NotSupportedException());

			public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, System.Threading.CancellationToken cancellationToken) =>
				Task.FromException<WebAuthenticatorResult>(new NotSupportedException());
		}

		class StubAppActions : IAppActions
		{
			public bool IsSupported => true;

			public event EventHandler<AppActionEventArgs>? AppActionActivated;

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public Task SetAsync(IEnumerable<AppAction> actions) => Task.CompletedTask;

			public void Raise(AppAction appAction) =>
				AppActionActivated?.Invoke(this, new AppActionEventArgs(appAction));
		}

		sealed class FaultingStubAppActions : IAppActions
		{
			public bool IsSupported => true;

			public event EventHandler<AppActionEventArgs>? AppActionActivated { add { } remove { } }

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public async Task SetAsync(IEnumerable<AppAction> actions)
			{
				await Task.Yield();
				throw new InvalidOperationException("app actions failed");
			}
		}

		sealed class DispatchingStubAppActions : IAppActions
		{
			readonly IDispatcher _dispatcher;

			public bool IsSupported => true;

			public TaskCompletionSource<bool> SetCompleted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> SetStarted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public event EventHandler<AppActionEventArgs>? AppActionActivated { add { } remove { } }

			public DispatchingStubAppActions(IDispatcher dispatcher)
			{
				_dispatcher = dispatcher;
			}

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public async Task SetAsync(IEnumerable<AppAction> actions)
			{
				SetStarted.TrySetResult(true);
				await _dispatcher.DispatchAsync(() => { });
				SetCompleted.TrySetResult(true);
			}
		}

		sealed class ControlledStubAppActions : IAppActions
		{
			readonly Task _release;
			readonly Action<IReadOnlyList<AppAction>> _publish;
			int _callCount;

			public ControlledStubAppActions(Task release, Action<IReadOnlyList<AppAction>> publish)
			{
				_release = release;
				_publish = publish;
			}

			public bool IsSupported => true;

			public int CallCount => Volatile.Read(ref _callCount);

			public TaskCompletionSource<bool> FirstCallStarted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> FirstCallCompleted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> SecondCallCompleted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public event EventHandler<AppActionEventArgs>? AppActionActivated { add { } remove { } }

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public async Task SetAsync(IEnumerable<AppAction> actions)
			{
				var call = Interlocked.Increment(ref _callCount);
				if (call == 1)
					FirstCallStarted.TrySetResult(true);

				await _release.ConfigureAwait(false);
				_publish(actions.ToArray());

				if (call == 1)
					FirstCallCompleted.TrySetResult(true);
				else if (call == 2)
					SecondCallCompleted.TrySetResult(true);
			}
		}

		sealed class OverlapDetectingStubAppActions : IAppActions
		{
			readonly Task _release;
			readonly Action _onStart;
			readonly Action _onEnd;

			public OverlapDetectingStubAppActions(
				Task release,
				Action onStart,
				Action onEnd)
			{
				_release = release;
				_onStart = onStart;
				_onEnd = onEnd;
			}

			public bool IsSupported => true;

			public TaskCompletionSource<bool> SetStarted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> SetCompleted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public event EventHandler<AppActionEventArgs>? AppActionActivated { add { } remove { } }

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public async Task SetAsync(IEnumerable<AppAction> actions)
			{
				_onStart();
				SetStarted.TrySetResult(true);
				await _release.ConfigureAwait(false);
				_onEnd();
				SetCompleted.TrySetResult(true);
			}
		}

		sealed class ThrowingUnsubscribeAppActions : IAppActions
		{
			public bool IsSupported => true;

			public event EventHandler<AppActionEventArgs>? AppActionActivated
			{
				add { }
				remove => throw new InvalidOperationException("unsubscribe failed");
			}

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public Task SetAsync(IEnumerable<AppAction> actions) => Task.CompletedTask;
		}

		sealed class DisposableStubVersionTracking : IVersionTracking, IDisposable
		{
			public bool IsDisposed { get; private set; }

			public int TrackCount { get; private set; }

			public bool IsFirstLaunchEver
			{
				get
				{
					ThrowIfDisposed();
					return false;
				}
			}

			public bool IsFirstLaunchForCurrentVersion => IsFirstLaunchEver;

			public bool IsFirstLaunchForCurrentBuild => IsFirstLaunchEver;

			public string CurrentVersion
			{
				get
				{
					ThrowIfDisposed();
					return "1.0";
				}
			}

			public string CurrentBuild => CurrentVersion;

			public string? PreviousVersion => null;

			public string? PreviousBuild => null;

			public string? FirstInstalledVersion => null;

			public string? FirstInstalledBuild => null;

			public IReadOnlyList<string> VersionHistory => Array.Empty<string>();

			public IReadOnlyList<string> BuildHistory => Array.Empty<string>();

			public void Track()
			{
				ThrowIfDisposed();
				TrackCount++;
			}

			public bool IsFirstLaunchForVersion(string version)
			{
				ThrowIfDisposed();
				return false;
			}

			public bool IsFirstLaunchForBuild(string build)
			{
				ThrowIfDisposed();
				return false;
			}

			public void Dispose()
			{
				IsDisposed = true;
			}

			void ThrowIfDisposed()
			{
				if (IsDisposed)
					throw new ObjectDisposedException(nameof(DisposableStubVersionTracking));
			}
		}

		sealed class ThrowingVersionTracking : IVersionTracking
		{
			public bool IsFirstLaunchEver => false;

			public bool IsFirstLaunchForCurrentVersion => false;

			public bool IsFirstLaunchForCurrentBuild => false;

			public string CurrentVersion => "1.0";

			public string CurrentBuild => "1";

			public string? PreviousVersion => null;

			public string? PreviousBuild => null;

			public string? FirstInstalledVersion => null;

			public string? FirstInstalledBuild => null;

			public IReadOnlyList<string> VersionHistory => Array.Empty<string>();

			public IReadOnlyList<string> BuildHistory => Array.Empty<string>();

			public void Track()
			{
				throw new InvalidOperationException("version tracking failed");
			}

			public bool IsFirstLaunchForVersion(string version) => false;

			public bool IsFirstLaunchForBuild(string build) => false;
		}

		sealed class FailOnSecondTrackVersionTracking : IVersionTracking
		{
			public int TrackCount { get; private set; }

			public bool IsFirstLaunchEver => false;

			public bool IsFirstLaunchForCurrentVersion => false;

			public bool IsFirstLaunchForCurrentBuild => false;

			public string CurrentVersion => "1.0";

			public string CurrentBuild => "1";

			public string? PreviousVersion => null;

			public string? PreviousBuild => null;

			public string? FirstInstalledVersion => null;

			public string? FirstInstalledBuild => null;

			public IReadOnlyList<string> VersionHistory => Array.Empty<string>();

			public IReadOnlyList<string> BuildHistory => Array.Empty<string>();

			public void Track()
			{
				TrackCount++;
				if (TrackCount == 2)
					throw new InvalidOperationException("second track failed");
			}

			public bool IsFirstLaunchForVersion(string version) => false;

			public bool IsFirstLaunchForBuild(string build) => false;
		}

		sealed class RecordingLoggerFactory : ILoggerFactory
		{
			public TaskCompletionSource<Exception> Error { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public void AddProvider(ILoggerProvider provider)
			{
			}

			public ILogger CreateLogger(string categoryName) =>
				new RecordingLogger(Error);

			public void Dispose()
			{
			}
		}

		sealed class RecordingLogger : ILogger
		{
			readonly TaskCompletionSource<Exception> _error;

			public RecordingLogger(TaskCompletionSource<Exception> error)
			{
				_error = error;
			}

			public IDisposable? BeginScope<TState>(TState state)
				where TState : notnull =>
				null;

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception? exception,
				Func<TState, Exception?, string> formatter)
			{
				if (logLevel >= LogLevel.Error && exception is not null)
					_error.TrySetResult(exception);
			}
		}

		sealed class ThrowingLoggerFactory : ILoggerFactory
		{
			public TaskCompletionSource<bool> LogStarted { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public TaskCompletionSource<bool> ReleaseLog { get; } =
				new(TaskCreationOptions.RunContinuationsAsynchronously);

			public void AddProvider(ILoggerProvider provider)
			{
			}

			public ILogger CreateLogger(string categoryName) =>
				new ThrowingLogger(LogStarted, ReleaseLog);

			public void Dispose()
			{
			}
		}

		sealed class ThrowingLogger : ILogger
		{
			readonly TaskCompletionSource<bool> _logStarted;
			readonly TaskCompletionSource<bool> _releaseLog;

			public ThrowingLogger(
				TaskCompletionSource<bool> logStarted,
				TaskCompletionSource<bool> releaseLog)
			{
				_logStarted = logStarted;
				_releaseLog = releaseLog;
			}

			public IDisposable? BeginScope<TState>(TState state)
				where TState : notnull =>
				null;

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception? exception,
				Func<TState, Exception?, string> formatter)
			{
				_logStarted.TrySetResult(true);
				_releaseLog.Task.GetAwaiter().GetResult();
				throw new InvalidOperationException("logging failed");
			}
		}

		sealed class DisposableStubAppActions : IAppActions, IDisposable
		{
			EventHandler<AppActionEventArgs>? _appActionActivated;

			public bool IsDisposed { get; private set; }

			public bool IsSupported => true;

			public bool WasUnsubscribed { get; private set; }

			public event EventHandler<AppActionEventArgs>? AppActionActivated
			{
				add
				{
					ThrowIfDisposed();
					_appActionActivated += value;
				}
				remove
				{
					ThrowIfDisposed();
					_appActionActivated -= value;
					WasUnsubscribed = true;
				}
			}

			public Task<IEnumerable<AppAction>> GetAsync() =>
				Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

			public Task SetAsync(IEnumerable<AppAction> actions) => Task.CompletedTask;

			public void Dispose()
			{
				IsDisposed = true;
			}

			void ThrowIfDisposed()
			{
				if (IsDisposed)
					throw new ObjectDisposedException(nameof(DisposableStubAppActions));
			}
		}
	}
}
