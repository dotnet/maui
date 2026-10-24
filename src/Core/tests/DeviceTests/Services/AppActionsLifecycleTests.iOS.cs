#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[CollectionDefinition("AppActionsLifecycle", DisableParallelization = true)]
	public class AppActionsLifecycleCollection
	{
	}

	[Collection("AppActionsLifecycle")]
	[Category("AppActions")]
	public class AppActionsLifecycleTests : CoreHandlerTestBase
	{
		const string WarmSceneSelector = "windowScene:performActionForShortcutItem:completionHandler:";

		[Fact]
		public async Task ColdSceneActionIsDeliveredAfterWindowActivationOnce()
		{
			var events = new List<string>();
			await using var fixture = await CreateFixtureAsync(events);
			var shortcutItem = await fixture.CreateShortcutItemAsync("cold-action");
			EventHandler<AppActionEventArgs> appActionHandler = (_, args) => events.Add($"AppAction:{args.AppAction.Id}");
			AppActions.OnAppAction += appActionHandler;

			try
			{
				await fixture.ConnectAsync(shortcutItem);

				Assert.Equal(new[] { "Created" }, events);

				await fixture.ActivateAsync();

				Assert.Equal(new[] { "Created", "Activated", "AppAction:cold-action" }, events);

				await fixture.ActivateAsync();

				Assert.Equal(1, events.Count(value => value == "AppAction:cold-action"));
			}
			finally
			{
				AppActions.OnAppAction -= appActionHandler;
			}
		}

		[Fact]
		public async Task ColdSceneActionIsClearedOnDisconnectAndFreshConnectionCanReuseId()
		{
			var events = new List<string>();
			await using var fixture = await CreateFixtureAsync(events);
			var shortcutItem = await fixture.CreateShortcutItemAsync("reused-action");
			EventHandler<AppActionEventArgs> appActionHandler = (_, args) => events.Add($"AppAction:{args.AppAction.Id}");
			AppActions.OnAppAction += appActionHandler;

			try
			{
				await fixture.ConnectAsync(shortcutItem);
				await fixture.DisconnectAsync();
				Assert.True(fixture.Window.IsDestroyed);
				fixture.CloseWindow();

				await fixture.InvokeActivationCallbackAsync();

				Assert.False(fixture.Window.IsActivated);
				Assert.DoesNotContain("AppAction:reused-action", events);

				events.Clear();
				fixture.UseNewWindow(events);

				await fixture.ConnectAsync(shortcutItem);
				await fixture.ActivateAsync();

				Assert.Equal(new[] { "Created", "Activated", "AppAction:reused-action" }, events);
			}
			finally
			{
				AppActions.OnAppAction -= appActionHandler;
			}
		}

		[Fact]
		public async Task ColdSceneActionIsNotDeliveredThroughWindowClosedDuringActivation()
		{
			var events = new List<string>();
			await using var fixture = await CreateFixtureAsync(events);
			var shortcutItem = await fixture.CreateShortcutItemAsync("closed-window-action");
			var received = 0;
			EventHandler<AppActionEventArgs> appActionHandler = (_, _) => received++;
			AppActions.OnAppAction += appActionHandler;

			try
			{
				await fixture.ConnectAsync(shortcutItem);
				fixture.Window.ActivatedCallback = fixture.CloseWindow;

				await fixture.ActivateAsync();

				Assert.Equal(0, received);

				await fixture.ActivateAsync();

				Assert.Equal(0, received);
			}
			finally
			{
				AppActions.OnAppAction -= appActionHandler;
			}
		}

		[Fact]
		public async Task WarmSceneActionUsesNativeSelectorAndCompletesOncePerInvocation()
		{
			var events = new List<string>();
			await using var fixture = await CreateFixtureAsync(events);
			var shortcutItem = await fixture.CreateShortcutItemAsync("warm-action");
			var received = 0;
			EventHandler<AppActionEventArgs> appActionHandler = (_, args) =>
			{
				received++;
				Assert.Equal("warm-action", args.AppAction.Id);
			};
			AppActions.OnAppAction += appActionHandler;

			try
			{
				await fixture.ConnectAsync(null);
				await fixture.ActivateAsync();

				var first = new CompletionRecorder();
				await fixture.PerformWarmActionAsync(shortcutItem, first);
				first.AssertCompletedOnce(true);

				var second = new CompletionRecorder();
				await fixture.PerformWarmActionAsync(shortcutItem, second);
				second.AssertCompletedOnce(true);

				Assert.Equal(2, received);
			}
			finally
			{
				AppActions.OnAppAction -= appActionHandler;
			}
		}

		[Fact]
		public async Task WarmSceneUnhandledActionCompletesFalseOnce()
		{
			var events = new List<string>();
			await using var fixture = await CreateFixtureAsync(events);
			using var shortcutItem = new UIApplicationShortcutItem(
				"foreign-action",
				"Foreign Action",
				null,
				null,
				null);
			var received = 0;
			EventHandler<AppActionEventArgs> appActionHandler = (_, _) => received++;
			AppActions.OnAppAction += appActionHandler;

			try
			{
				await fixture.ConnectAsync(null);
				await fixture.ActivateAsync();

				var completion = new CompletionRecorder();
				await fixture.PerformWarmActionAsync(shortcutItem, completion);

				Assert.False(await completion.WaitAsync());
				completion.AssertCompletedOnce(false);
				Assert.True(completion.WasOnMainThread);
				Assert.Equal(0, received);
			}
			finally
			{
				AppActions.OnAppAction -= appActionHandler;
			}
		}

		[Fact]
		public async Task WarmSceneActionWaitsForAsyncCustomHandlerAfterEssentialsDeclines()
		{
			var events = new List<string>();
			await using var fixture = await CreateFixtureAsync(events);
			using var shortcutItem = new UIApplicationShortcutItem("custom-action", "Custom Action", null, null, null);
			var customInvoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseCustomHandler = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var lifecycleService = Assert.IsType<LifecycleEventService>(
				fixture.Services.GetRequiredService<ILifecycleEventService>());
			iOSLifecycle.PerformActionForShortcutItem customHandler = (_, _, completion) =>
			{
				customInvoked.TrySetResult();
				_ = Task.Run(async () =>
				{
					await releaseCustomHandler.Task;
					completion(true);
				});
			};
			((ILifecycleBuilder)lifecycleService).AddEvent(
				nameof(iOSLifecycle.PerformActionForShortcutItem),
				customHandler);

			try
			{
				await fixture.ConnectAsync(null);
				await fixture.ActivateAsync();

				var completion = new CompletionRecorder();
				var dispatch = fixture.PerformWarmActionAsync(shortcutItem, completion);

				await dispatch;
				await customInvoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
				Assert.Equal(0, completion.Count);

				releaseCustomHandler.TrySetResult();

				Assert.True(await completion.WaitAsync());
				completion.AssertCompletedOnce(true);
				Assert.True(completion.WasOnMainThread);
			}
			finally
			{
				lifecycleService.RemoveEvent(
					nameof(iOSLifecycle.PerformActionForShortcutItem),
					customHandler);
			}
		}

		[Fact]
		public async Task LegacyApplicationActionUsesProductionEntryPointAndCompletesOnce()
		{
			var originalApplicationDelegate = MauiUIApplicationDelegate.Current;
			var originalPlatformApplication = IPlatformApplication.Current;
			TestLegacyApplicationDelegate? applicationDelegate = null;
			var received = 0;

			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					using var shortcutItem = new UIApplicationShortcutItem(
						"legacy-custom-action",
						"Legacy Custom Action",
						null,
						null,
						null);
					applicationDelegate = new TestLegacyApplicationDelegate((receivedItem, completion) =>
					{
						received++;
						Assert.Same(shortcutItem, receivedItem);
						completion(true);
					});
					applicationDelegate.WillFinishLaunching(UIApplication.SharedApplication, null);
					var completion = new CompletionRecorder();

					applicationDelegate.PerformActionForShortcutItem(
						UIApplication.SharedApplication,
						shortcutItem,
						completion.Handler);

					Assert.Equal(1, received);
					completion.AssertCompletedOnce(true);
					Assert.True(completion.WasOnMainThread);
				});
			}
			finally
			{
				await InvokeOnMainThreadAsync(() =>
				{
					MauiUIApplicationDelegate.Current = originalApplicationDelegate;
					IPlatformApplication.Current = originalPlatformApplication;
				});

				if (applicationDelegate is not null)
					await applicationDelegate.DisposeAsync();
			}
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task SceneFixtureConnectionPreservesKeyboardAutoManagerState(
			bool connectObservers,
			bool initialDisconnect)
		{
			var originalShouldDisconnectLifecycle = false;
			var shouldRestoreDisconnectLifecycle = false;
			NSObject?[]? originalObserverTokens = null;
			NSObject?[]? fixtureObserverTokensBefore = null;
			SceneFixture? fixture = null;
			LifecycleEventService? lifecycleService = null;
			var platformWindowCreatedCallbackRegistered = false;
			var platformWindowCreatedCount = 0;
			bool? disconnectLifecycleDuringWindowCreated = null;
			iOSLifecycle.OnPlatformWindowCreated onPlatformWindowCreated = _ =>
			{
				platformWindowCreatedCount++;
				disconnectLifecycleDuringWindowCreated =
					KeyboardAutoManagerScroll.ShouldDisconnectLifecycle;
			};

			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					originalShouldDisconnectLifecycle = KeyboardAutoManagerScroll.ShouldDisconnectLifecycle;
					shouldRestoreDisconnectLifecycle = true;
					originalObserverTokens = GetKeyboardObserverTokens();

					if (connectObservers)
						KeyboardAutoManagerScroll.Connect();
				});

				fixture = await CreateFixtureAsync(new List<string>());
				var lifecycle = Assert.IsType<LifecycleEventService>(
					fixture.Services.GetRequiredService<ILifecycleEventService>());
				lifecycleService = lifecycle;

				await InvokeOnMainThreadAsync(() =>
				{
					((ILifecycleBuilder)lifecycle).AddEvent(
						nameof(iOSLifecycle.OnPlatformWindowCreated),
						onPlatformWindowCreated);
					platformWindowCreatedCallbackRegistered = true;
					fixtureObserverTokensBefore = GetKeyboardObserverTokens();
					if (connectObservers)
						Assert.All(fixtureObserverTokensBefore!, token => Assert.NotNull(token));

					KeyboardAutoManagerScroll.ShouldDisconnectLifecycle = initialDisconnect;
				});

				await fixture.ConnectAsync(null);

				await InvokeOnMainThreadAsync(() =>
				{
					Assert.Equal(1, platformWindowCreatedCount);
					Assert.True(disconnectLifecycleDuringWindowCreated == true);
					Assert.Equal(initialDisconnect, KeyboardAutoManagerScroll.ShouldDisconnectLifecycle);
					AssertKeyboardObserverTokensSame(fixtureObserverTokensBefore!);
				});

				await InvokeOnMainThreadAsync(() =>
					lifecycle.RemoveEvent(
						nameof(iOSLifecycle.OnPlatformWindowCreated),
						onPlatformWindowCreated));
				platformWindowCreatedCallbackRegistered = false;

				await fixture.DisposeAsync();
				fixture = null;

				await InvokeOnMainThreadAsync(() =>
				{
					Assert.Equal(initialDisconnect, KeyboardAutoManagerScroll.ShouldDisconnectLifecycle);
					AssertKeyboardObserverTokensSame(fixtureObserverTokensBefore!);
				});
			}
			finally
			{
				try
				{
					if (lifecycleService is not null && platformWindowCreatedCallbackRegistered)
					{
						await InvokeOnMainThreadAsync(() =>
							lifecycleService.RemoveEvent(
								nameof(iOSLifecycle.OnPlatformWindowCreated),
								onPlatformWindowCreated));
					}

					if (fixture is not null)
						await fixture.DisposeAsync();
				}
				finally
				{
					if (shouldRestoreDisconnectLifecycle)
					{
						await InvokeOnMainThreadAsync(() =>
						{
							try
							{
								if (originalObserverTokens is not null &&
									originalObserverTokens.All(token => token is null) &&
									GetKeyboardObserverTokens().Any(token => token is not null))
								{
									KeyboardAutoManagerScroll.Disconnect();
								}
							}
							finally
							{
								KeyboardAutoManagerScroll.ShouldDisconnectLifecycle =
									originalShouldDisconnectLifecycle;
							}
						});
					}
				}
			}
		}

		static NSObject?[] GetKeyboardObserverTokens()
		{
			const BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic;
			var observerFields = new[]
			{
				"TextFieldToken",
				"TextViewToken",
				"WillShowToken",
				"WillHideToken",
				"DidHideToken",
			};
			var tokens = new NSObject?[observerFields.Length];

			for (var i = 0; i < observerFields.Length; i++)
			{
				var field = typeof(KeyboardAutoManagerScroll).GetField(observerFields[i], Flags);
				Assert.NotNull(field);

				var value = field!.GetValue(null);
				Assert.True(
					value is null || value is NSObject,
					$"{observerFields[i]} must be an NSObject observer token.");
				tokens[i] = value as NSObject;
			}

			return tokens;
		}

		static void AssertKeyboardObserverTokensSame(NSObject?[] expected)
		{
			var actual = GetKeyboardObserverTokens();
			Assert.Equal(expected.Length, actual.Length);

			for (var i = 0; i < expected.Length; i++)
				Assert.Same(expected[i], actual[i]);
		}

		async Task<SceneFixture> CreateFixtureAsync(List<string> events)
		{
			EnsureHandlerCreated(builder =>
				builder.ConfigureMauiHandlers(handlers =>
					handlers.AddHandler<TrackingWindowStub, WindowHandler>()));

			var application = Assert.IsType<CoreApplicationStub>(
				ApplicationServices.GetRequiredService<IApplication>());
			var originalPlatformApplication = IPlatformApplication.Current;
			var originalShortcutItems = await InvokeOnMainThreadAsync(
				() => UIApplication.SharedApplication.ShortcutItems);
			var window = new TrackingWindowStub(events);
			var hostScene = await InvokeOnMainThreadAsync(() =>
			{
				using var connectedScenes = UIApplication.SharedApplication.ConnectedScenes;
				var windowScene = connectedScenes.OfType<UIWindowScene>().FirstOrDefault();
				Assert.NotNull(windowScene);
				return windowScene;
			});

			await InvokeOnMainThreadAsync(() =>
			{
				var applicationHandler = new ApplicationHandler();
				applicationHandler.SetMauiContext(MauiContext);
				application.Handler = applicationHandler;
				application.SetSingleWindow(window);
			});

			IPlatformApplication.Current = new TestPlatformApplication(ApplicationServices, application);

			try
			{
				return await InvokeOnMainThreadAsync(() =>
					new SceneFixture(
						ApplicationServices,
						application,
						window,
						originalPlatformApplication,
						originalShortcutItems,
						hostScene));
			}
			catch
			{
				IPlatformApplication.Current = originalPlatformApplication;
				throw;
			}
		}

		sealed class SceneFixture : IAsyncDisposable
		{
			readonly CoreApplicationStub _application;
			readonly IPlatformApplication? _originalPlatformApplication;
			readonly UIApplicationShortcutItem[]? _originalShortcutItems;
			readonly IUISceneDelegate? _originalSceneDelegate;
			readonly UIWindow? _originalKeyWindow;
			readonly List<TrackingWindowStub> _windows = new();
			readonly Dictionary<TrackingWindowStub, WindowMapping> _mappedWindows = new();

			public SceneFixture(
				IServiceProvider services,
				CoreApplicationStub application,
				TrackingWindowStub window,
				IPlatformApplication? originalPlatformApplication,
				UIApplicationShortcutItem[]? originalShortcutItems,
				UIWindowScene scene)
			{
				Services = services;
				_application = application;
				_originalPlatformApplication = originalPlatformApplication;
				_originalShortcutItems = originalShortcutItems;
				_originalSceneDelegate = scene.Delegate;
				_originalKeyWindow = scene.Windows.FirstOrDefault(candidate => candidate.IsKeyWindow);
				SceneDelegate = new MauiUISceneDelegate();
				Scene = scene;
				Scene.Delegate = SceneDelegate;
				Session = new TestSceneSession();
				UseWindow(window);
			}

			public IServiceProvider Services { get; }

			public MauiUISceneDelegate SceneDelegate { get; }

			public UIWindowScene Scene { get; }

			public TestSceneSession Session { get; }

			public TrackingWindowStub Window { get; private set; } = null!;

			public async Task<UIApplicationShortcutItem> CreateShortcutItemAsync(string id)
			{
				await InvokeOnMainThreadAsync(() =>
					AppActions.SetAsync(new AppAction(id, id)));

				return await InvokeOnMainThreadAsync(() =>
				{
					var shortcutItems = UIApplication.SharedApplication.ShortcutItems;
					Assert.NotNull(shortcutItems);
					return Assert.Single(shortcutItems);
				});
			}

			public Task ConnectAsync(UIApplicationShortcutItem? shortcutItem) =>
				InvokeOnMainThreadAsync(() =>
				{
					AssertWillConnectPreconditions();
					using var options = new TestSceneConnectionOptions(shortcutItem);
					var originalShouldDisconnectLifecycle = KeyboardAutoManagerScroll.ShouldDisconnectLifecycle;
					// The fixture's non-input window must not change process-wide keyboard observer state.
					KeyboardAutoManagerScroll.ShouldDisconnectLifecycle = true;
					try
					{
						SceneDelegate.WillConnect(Scene, Session, options);
					}
					finally
					{
						KeyboardAutoManagerScroll.ShouldDisconnectLifecycle = originalShouldDisconnectLifecycle;
					}
					AssertMappedWindow();
				});

			public async Task ActivateAsync()
			{
				await InvokeActivationCallbackAsync();
				Assert.True(Window.IsActivated, "The production SceneOnActivated adapter did not activate the mapped MAUI window.");
			}

			public Task InvokeActivationCallbackAsync() =>
				InvokeOnMainThreadAsync(() => SceneDelegate.OnActivated(Scene));

			public Task DisconnectAsync() =>
				InvokeOnMainThreadAsync(() => SceneDelegate.DidDisconnect(Scene));

			public async Task PerformWarmActionAsync(
				UIApplicationShortcutItem shortcutItem,
				CompletionRecorder completion)
			{
				await InvokeOnMainThreadAsync(() =>
				{
					var selector = new Selector(WarmSceneSelector);
					var respondsToSelector = SceneDelegate.RespondsToSelector(selector);
					Assert.True(
						respondsToSelector,
						$"{nameof(MauiUISceneDelegate)} must export '{WarmSceneSelector}'.");

					if (!respondsToSelector)
						return;

					UIWindowSceneDelegate_Extensions.PerformAction(
						SceneDelegate,
						Scene,
						shortcutItem,
						handled => completion.Complete(handled));
				});
			}

			public void UseNewWindow(List<string> events) =>
				UseWindow(new TrackingWindowStub(events));

			public void CloseWindow()
			{
				Assert.True(
					_mappedWindows.Remove(Window, out var mapping),
					"The fixture window must have a verified native mapping before it can be closed.");

				mapping.PlatformWindow.Hidden = true;
				mapping.Handler.DisconnectHandler();
				mapping.MauiContext.DisposeWindowScope();
				mapping.PlatformWindow.Dispose();
				_application.CloseWindow(Window);
				SceneDelegate.Window = null;
			}

			void AssertMappedWindow()
			{
				Assert.NotNull(SceneDelegate.Window);
				var handler = Assert.IsAssignableFrom<IWindowHandler>(Window.Handler);
				var platformWindow = Assert.IsType<UIWindow>(handler.PlatformView);
				var mauiContext = Assert.IsType<MauiContext>(handler.MauiContext);
				_mappedWindows.Add(Window, new WindowMapping(handler, platformWindow, mauiContext));
				var windowScene = platformWindow.WindowScene;
				var mappedWindow = SceneDelegate.Window.GetWindow();
				var applicationLifecycle = Services.GetRequiredService<ILifecycleEventService>();
				var windowLifecycle = mauiContext.Services.GetRequiredService<ILifecycleEventService>();

				Assert.NotEqual(NativeHandle.Zero, Scene.Handle);
				Assert.NotEqual(NativeHandle.Zero, platformWindow.Handle);
				Assert.NotNull(windowScene);
				Assert.Equal(Scene.Handle, windowScene.Handle);
				Assert.Same(Window, handler.VirtualView);
				Assert.Equal(platformWindow.Handle, SceneDelegate.Window.Handle);
				Assert.Same(Window, mappedWindow);
				Assert.Contains(_application.Windows, candidate => ReferenceEquals(candidate, Window));
				Assert.Same(applicationLifecycle, windowLifecycle);
				Assert.True(Window.IsCreated, "The production OnPlatformWindowCreated adapter did not create the mapped MAUI window.");
			}

			void AssertWillConnectPreconditions()
			{
				Assert.Equal(MauiUIApplicationDelegate.MauiSceneConfigurationKey, Session.Configuration.Name);
				Assert.Same(_application, IPlatformApplication.Current?.Application);
				Assert.Null(Window.Handler);

				var applicationContext = _application.Handler?.MauiContext;
				Assert.NotNull(applicationContext);

				var applicationLifecycle = Services.GetRequiredService<ILifecycleEventService>();
				var contextLifecycle = applicationContext.Services.GetRequiredService<ILifecycleEventService>();
				Assert.Same(applicationLifecycle, contextLifecycle);
				Assert.NotEmpty(
					applicationLifecycle.GetEventDelegates<iOSLifecycle.OnPlatformWindowCreated>(
						nameof(iOSLifecycle.OnPlatformWindowCreated)));
				Assert.NotEmpty(
					applicationLifecycle.GetEventDelegates<iOSLifecycle.SceneOnActivated>(
						nameof(iOSLifecycle.SceneOnActivated)));
			}

			void UseWindow(TrackingWindowStub window)
			{
				Window = window;
				_windows.Add(window);
				_application.SetSingleWindow(window);
				_application.OpenWindow(window);
			}

			public async ValueTask DisposeAsync()
			{
				IPlatformApplication.Current = _originalPlatformApplication;

				await InvokeOnMainThreadAsync(() =>
				{
					try
					{
						SceneDelegate.Window = null;
						foreach (var pair in _mappedWindows)
						{
							var mapping = pair.Value;

							mapping.PlatformWindow.Hidden = true;
							mapping.Handler.DisconnectHandler();
							mapping.MauiContext.DisposeWindowScope();
							mapping.PlatformWindow.Dispose();
						}

						foreach (var window in _windows)
						{
							_application.CloseWindow(window);
						}
					}
					finally
					{
						_mappedWindows.Clear();
						_application.Handler = null;
						UIApplication.SharedApplication.ShortcutItems = _originalShortcutItems;
						Scene.Delegate = _originalSceneDelegate;
						_originalKeyWindow?.MakeKeyAndVisible();
						Session.Dispose();
						SceneDelegate.Dispose();
					}
				});
			}

			readonly record struct WindowMapping(
				IWindowHandler Handler,
				UIWindow PlatformWindow,
				MauiContext MauiContext);
		}

		sealed class TrackingWindowStub : WindowStub
		{
			readonly List<string> _events;

			public TrackingWindowStub(List<string> events)
			{
				_events = events;
				Content = new ButtonStub();
				X = 0;
				Y = 0;
				Width = 100;
				Height = 100;
			}

			public Action? ActivatedCallback { get; set; }

			public override void Created()
			{
				base.Created();
				_events.Add("Created");
			}

			public override void Activated()
			{
				base.Activated();
				_events.Add("Activated");
				ActivatedCallback?.Invoke();
			}
		}

		sealed class TestPlatformApplication : IPlatformApplication
		{
			public TestPlatformApplication(IServiceProvider services, IApplication application)
			{
				Services = services;
				Application = application;
			}

			public IServiceProvider Services { get; }

			public IApplication Application { get; }
		}

		sealed class TestLegacyApplicationDelegate : MauiUIApplicationDelegate, IAsyncDisposable
		{
			readonly Action<UIApplicationShortcutItem, UIOperationHandler> _handler;
			MauiApp? _mauiApp;

			public TestLegacyApplicationDelegate(Action<UIApplicationShortcutItem, UIOperationHandler> handler)
			{
				_handler = handler;
			}

			protected override MauiApp CreateMauiApp() =>
				_mauiApp = MauiApp.CreateBuilder()
					.ConfigureLifecycleEvents(lifecycle =>
						lifecycle.AddiOS(ios =>
							ios.PerformActionForShortcutItem((_, shortcutItem, completion) =>
								_handler(shortcutItem, completion))))
					.Build();

			public async ValueTask DisposeAsync()
			{
				if (_mauiApp is IAsyncDisposable disposable)
					await disposable.DisposeAsync();

				await InvokeOnMainThreadAsync(Dispose);
			}
		}

		sealed class TestSceneSession : UISceneSession
		{
			readonly UISceneConfiguration _configuration =
				new TestSceneConfiguration();

			public TestSceneSession()
				: base(NSObjectFlag.Empty)
			{
			}

			public override UISceneConfiguration Configuration => _configuration;

			public override NSUserActivity? StateRestorationActivity { get; set; }

			public override NSDictionary<NSString, NSObject>? UserInfo { get; set; }

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_configuration.Dispose();

				base.Dispose(disposing);
			}
		}

		sealed class TestSceneConfiguration : UISceneConfiguration
		{
			public TestSceneConfiguration()
				: base(MauiUIApplicationDelegate.MauiSceneConfigurationKey, UIWindowSceneSessionRole.Application)
			{
			}

			public override string? Name => MauiUIApplicationDelegate.MauiSceneConfigurationKey;

			public override UIWindowSceneSessionRole Role => UIWindowSceneSessionRole.Application;
		}

		sealed class TestSceneConnectionOptions : UISceneConnectionOptions
		{
			readonly UIApplicationShortcutItem? _shortcutItem;

			public TestSceneConnectionOptions(UIApplicationShortcutItem? shortcutItem)
				: base(NSObjectFlag.Empty)
			{
				_shortcutItem = shortcutItem;
			}

			public override UIApplicationShortcutItem? ShortcutItem => _shortcutItem;

			public override NSSet<NSUserActivity> UserActivities => new();
		}

		sealed class CompletionRecorder
		{
			readonly TaskCompletionSource<bool> _completion =
				new(TaskCreationOptions.RunContinuationsAsynchronously);
			int _count;
			int _wasOnMainThread;
			bool? _result;

			public int Count => Volatile.Read(ref _count);

			public bool WasOnMainThread => Volatile.Read(ref _wasOnMainThread) != 0;

			public UIOperationHandler Handler => Complete;

			public void Complete(bool handled)
			{
				Volatile.Write(ref _wasOnMainThread, MainThread.IsMainThread ? 1 : 0);
				_result = handled;
				Interlocked.Increment(ref _count);
				_completion.TrySetResult(handled);
			}

			public async Task<bool> WaitAsync() =>
				await _completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

			public void AssertCompletedOnce(bool expected)
			{
				Assert.Equal(1, Count);
				Assert.Equal(expected, _result);
			}
		}
	}
}
