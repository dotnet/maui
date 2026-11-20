using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Webkit;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
using Xunit;
using AView = Android.Views.View;
using AWindowInsets = Android.Views.WindowInsets;
using WindowCompat = AndroidX.Core.View.WindowCompat;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

// https://github.com/dotnet/maui/issues/22049
public partial class BlazorWebViewTests
{
	[Fact]
	public async Task WebContentReportsFullScreenAsEnabled()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = CreateFullScreenTestBlazorWebView();

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var isFullScreenEnabled = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.fullscreenEnabled");

			Assert.Equal("true", isFullScreenEnabled);
		});
	}

	[Fact]
	public async Task ShowCustomViewCoversActivityUntilHidden()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = CreateFullScreenTestBlazorWebView();

		await InvokeOnMainThreadAsync(async () =>
		{
			var activity = GetTestRunnerActivity();
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv, CreateMauiContextFor(activity));
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var activityState = new ActivityStateBeforeFullScreen(activity);
			var chromeClient = platformWebView.WebChromeClient;
			Assert.NotNull(chromeClient);
			var customView = new AView(activity);
			var customViewCallback = new RecordingCustomViewCallback();

			try
			{
				chromeClient.OnShowCustomView(customView, customViewCallback);

				Assert.Equal<object>(activity.Window!.DecorView, customView.Parent);
				Assert.Equal(ScreenOrientation.Landscape, activity.RequestedOrientation);

				chromeClient.OnHideCustomView();

				Assert.Null(customView.Parent);
				Assert.Equal(activityState.Orientation, activity.RequestedOrientation);
				Assert.Equal(1, customViewCallback.HiddenCount);
			}
			finally
			{
				await activityState.Restore(chromeClient, customView);
			}
		});
	}

	[Fact]
	public async Task DisconnectingHandlerLeavesFullScreen()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = CreateFullScreenTestBlazorWebView();

		await InvokeOnMainThreadAsync(async () =>
		{
			var activity = GetTestRunnerActivity();
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv, CreateMauiContextFor(activity));
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var activityState = new ActivityStateBeforeFullScreen(activity);
			var chromeClient = platformWebView.WebChromeClient;
			Assert.NotNull(chromeClient);
			var customView = new AView(activity);
			var customViewCallback = new RecordingCustomViewCallback();

			try
			{
				chromeClient.OnShowCustomView(customView, customViewCallback);
				Assert.Equal<object>(activity.Window!.DecorView, customView.Parent);

				((IElementHandler)bwvHandler).DisconnectHandler();

				Assert.Null(customView.Parent);
				Assert.Equal(activityState.Orientation, activity.RequestedOrientation);
				Assert.Equal(1, customViewCallback.HiddenCount);
			}
			finally
			{
				await activityState.Restore(chromeClient, customView);
			}
		});
	}

	[Fact]
	public async Task ShowCustomViewAfterDisconnectIsRejected()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = CreateFullScreenTestBlazorWebView();

		await InvokeOnMainThreadAsync(async () =>
		{
			var activity = GetTestRunnerActivity();
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv, CreateMauiContextFor(activity));
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var activityState = new ActivityStateBeforeFullScreen(activity);
			var chromeClient = platformWebView.WebChromeClient;
			Assert.NotNull(chromeClient);
			var customView = new AView(activity);
			var customViewCallback = new RecordingCustomViewCallback();

			try
			{
				((IElementHandler)bwvHandler).DisconnectHandler();

				// The WebView keeps this client until its components are disposed, so the page can still ask for fullscreen.
				chromeClient.OnShowCustomView(customView, customViewCallback);

				Assert.Null(customView.Parent);
				Assert.Equal(activityState.Orientation, activity.RequestedOrientation);
				Assert.Equal(1, customViewCallback.HiddenCount);
			}
			finally
			{
				await activityState.Restore(chromeClient, customView);
			}
		});
	}

	[Fact]
	public async Task ShowCustomViewWithoutActivityReportsTheViewHidden()
	{
		await InvokeOnMainThreadAsync(() =>
		{
			var chromeClient = new BlazorWebChromeClient(null);
			var customView = new AView(global::Android.App.Application.Context);
			var customViewCallback = new RecordingCustomViewCallback();

			chromeClient.OnShowCustomView(customView, customViewCallback);
			// A client left in the showing state would take this second request as a request to hide.
			chromeClient.OnShowCustomView(customView, customViewCallback);

			Assert.Null(customView.Parent);
			Assert.Equal(2, customViewCallback.HiddenCount);
		});
	}

	[Fact]
	public async Task ShowCustomViewWithoutAWindowOnScreenReportsTheViewHidden()
	{
		await InvokeOnMainThreadAsync(() =>
		{
			// Never started, so it has no window: the state of an activity the system has not attached or already let go.
			var activityWithoutWindow = new Activity();
			var chromeClient = new BlazorWebChromeClient(activityWithoutWindow);
			var customView = new AView(global::Android.App.Application.Context);
			var customViewCallback = new RecordingCustomViewCallback();

			chromeClient.OnShowCustomView(customView, customViewCallback);

			Assert.Null(customView.Parent);
			Assert.Equal(1, customViewCallback.HiddenCount);
		});
	}

	[Fact]
	public async Task HidingAgainAfterLeavingFullScreenChangesNothing()
	{
		await InvokeOnMainThreadAsync(async () =>
		{
			var activity = GetTestRunnerActivity();
			var activityState = new ActivityStateBeforeFullScreen(activity);
			var chromeClient = new BlazorWebChromeClient(activity);
			var customView = new AView(activity);
			var customViewCallback = new RecordingCustomViewCallback();

			try
			{
				chromeClient.OnShowCustomView(customView, customViewCallback);
				chromeClient.OnHideCustomView();
				activity.RequestedOrientation = ScreenOrientation.Locked;

				chromeClient.OnHideCustomView();

				Assert.Equal(ScreenOrientation.Locked, activity.RequestedOrientation);
				Assert.Equal(1, customViewCallback.HiddenCount);
			}
			finally
			{
				await activityState.Restore(chromeClient, customView);
			}
		});
	}

	[Fact]
	public async Task LeavingFullScreenKeepsTheActivityEdgeToEdge()
	{
		// Leaving fullscreen used to call SetDecorFitsSystemWindows(true). That moved the content on API 34 and did not on
		// API 36, which keeps apps edge to edge, so this test only tells the two apart on the older versions.
		await InvokeOnMainThreadAsync(async () =>
		{
			var activity = GetTestRunnerActivity();
			var activityState = new ActivityStateBeforeFullScreen(activity);
			var chromeClient = new BlazorWebChromeClient(activity);
			var customView = new AView(activity);
			var customViewCallback = new RecordingCustomViewCallback();

			try
			{
				// MauiAppCompatActivity draws edge to edge; the test runner's AppCompatActivity does not, so it gets the same policy here.
				WindowCompat.SetDecorFitsSystemWindows(activity.Window!, false);
				await Task.Delay(SystemBarsSettleMilliseconds);
				var contentTopBeforeFullScreen = GetContentTopInWindow(activity);

				chromeClient.OnShowCustomView(customView, customViewCallback);
				chromeClient.OnHideCustomView();
				await Task.Delay(SystemBarsSettleMilliseconds);

				Assert.Equal(contentTopBeforeFullScreen, GetContentTopInWindow(activity));
			}
			finally
			{
				// The AppCompatActivity default.
				WindowCompat.SetDecorFitsSystemWindows(activity.Window!, true);
				await activityState.Restore(chromeClient, customView);
			}
		});
	}

	[Fact]
	public async Task LeavingFullScreenKeepsAHiddenStatusBarHidden()
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			// Below API 30 the presenter does not read the visibility of each bar and shows all of them again.
			return;
		}

		await InvokeOnMainThreadAsync(async () =>
		{
			var activity = GetTestRunnerActivity();
			var activityState = new ActivityStateBeforeFullScreen(activity);
			var insetsController = activity.Window!.InsetsController;
			Assert.NotNull(insetsController);
			var chromeClient = new BlazorWebChromeClient(activity);
			var customView = new AView(activity);
			var customViewCallback = new RecordingCustomViewCallback();

			try
			{
				insetsController.Hide(AWindowInsets.Type.StatusBars());
				var isStatusBarHidden = await AssertHelpers.Wait(() => !IsSystemBarVisible(activity, AWindowInsets.Type.StatusBars()), SystemBarsSettleMilliseconds);
				Assert.True(isStatusBarHidden, "The status bar did not hide before the test entered fullscreen.");
				var isNavigationBarVisibleBeforeFullScreen = IsSystemBarVisible(activity, AWindowInsets.Type.NavigationBars());

				chromeClient.OnShowCustomView(customView, customViewCallback);
				chromeClient.OnHideCustomView();
				await Task.Delay(SystemBarsSettleMilliseconds);

				Assert.False(IsSystemBarVisible(activity, AWindowInsets.Type.StatusBars()));
				Assert.Equal(isNavigationBarVisibleBeforeFullScreen, IsSystemBarVisible(activity, AWindowInsets.Type.NavigationBars()));
			}
			finally
			{
				insetsController.Show(AWindowInsets.Type.StatusBars());
				await activityState.Restore(chromeClient, customView);
			}
		});
	}

	/// <summary>
	/// How long the tests give the system bars, the orientation and the layout to settle after they change: they animate.
	/// </summary>
	const int SystemBarsSettleMilliseconds = 1500;

	static BlazorWebViewWithCustomFiles CreateFullScreenTestBlazorWebView()
	{
		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });
		return bwv;
	}

	/// <summary>
	/// The activity the test runner shows. Platform.CurrentActivity and the tests' default context are not reliable here:
	/// the external navigation tests start activities that finish themselves, and both can still point at one of those.
	/// </summary>
	static Activity GetTestRunnerActivity()
	{
		var activity = MauiTestInstrumentation.Current?.CurrentExecutionContext as Activity ?? Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
		Assert.NotNull(activity);
		return activity;
	}

	/// <summary>
	/// A context whose activity is <paramref name="activity"/>, so the handler gives its chrome client that activity.
	/// </summary>
	IMauiContext CreateMauiContextFor(Activity activity)
	{
		return new MauiContext(MauiContext.Services, activity);
	}

	static int GetContentTopInWindow(Activity activity)
	{
		var content = activity.FindViewById(global::Android.Resource.Id.Content);
		Assert.NotNull(content);
		var location = new int[2];
		content.GetLocationInWindow(location);
		return location[1];
	}

	static bool IsSystemBarVisible(Activity activity, int systemBarType)
	{
		var windowInsets = activity.Window!.DecorView.RootWindowInsets;
		Assert.NotNull(windowInsets);
		return windowInsets.IsVisible(systemBarType);
	}

	/// <summary>
	/// What fullscreen changes on the activity, captured before a test enters it, so the test can leave the activity as
	/// the other tests expect it even when an assertion failed while the custom view was shown.
	/// </summary>
	sealed class ActivityStateBeforeFullScreen
	{
		readonly Activity _activity;
		readonly int? _systemBarsBehavior;

		public ActivityStateBeforeFullScreen(Activity activity)
		{
			_activity = activity;
			Orientation = activity.RequestedOrientation;
			if (OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				_systemBarsBehavior = (int?)activity.Window?.InsetsController?.SystemBarsBehavior;
			}
		}

		public ScreenOrientation Orientation { get; }

		/// <summary>
		/// Hides the custom view, removes it from the decor view, puts back the orientation and the system bar behaviour, and
		/// waits until the rotation and the bar animations have finished, so they cannot reach into the next test.
		/// </summary>
		public async Task Restore(WebChromeClient chromeClient, AView customView)
		{
			chromeClient.OnHideCustomView();
			(customView.Parent as global::Android.Views.ViewGroup)?.RemoveView(customView);
			_activity.RequestedOrientation = Orientation;
			if (OperatingSystem.IsAndroidVersionAtLeast(30) && _systemBarsBehavior is not null && _activity.Window?.InsetsController is not null)
			{
				_activity.Window.InsetsController.SystemBarsBehavior = _systemBarsBehavior.Value;
			}

			await Task.Delay(SystemBarsSettleMilliseconds);
		}
	}

	sealed class RecordingCustomViewCallback : Java.Lang.Object, WebChromeClient.ICustomViewCallback
	{
		public int HiddenCount { get; private set; }

		public void OnCustomViewHidden()
		{
			HiddenCount++;
		}
	}
}
