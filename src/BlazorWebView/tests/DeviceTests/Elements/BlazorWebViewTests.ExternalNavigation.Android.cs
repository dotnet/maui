using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	static readonly TimeSpan ActivityLaunchTimeout = TimeSpan.FromSeconds(10);
	static readonly TimeSpan ActivityNotLaunchedTimeout = TimeSpan.FromSeconds(2);

	[Theory]
	[InlineData("https://example.com/path")]
	[InlineData("tel:+15551234")]
	[InlineData("mailto:user@example.com")]
	[InlineData("geo:29.7604,-95.3698")]
	public void OrdinaryExternalUriCreatesViewIntent(string uri)
	{
		using var intent = WebKitWebViewClient.CreateIntentForExternalUri(new Uri(uri));

		Assert.Equal(Intent.ActionView, intent.Action);
		Assert.Equal(uri, intent.DataString);
		Assert.Null(intent.Package);
		Assert.Null(intent.Component);
		Assert.Null(intent.Selector);
		Assert.Null(intent.Categories);
	}

	[Fact]
	public void AndroidAppUriPreservesImplicitRoutingData()
	{
		const string androidAppUri = "android-app://com.example.app/https/example.com/path";

		using var intent = WebKitWebViewClient.CreateIntentForExternalUri(new Uri(androidAppUri));

		Assert.Equal(Intent.ActionView, intent.Action);
		Assert.Equal("https://example.com/path", intent.DataString);
		Assert.Equal("com.example.app", intent.Package);
		Assert.Contains(Intent.CategoryBrowsable, intent.Categories);
		Assert.Null(intent.Component);
		Assert.Null(intent.Selector);
	}

	[Fact]
	public void IntentUriPreservesImplicitRoutingData()
	{
		var intentUri =
			$"intent://open/item/42#Intent;scheme={ExternalNavigationTestData.BrowsableScheme};package={ExternalNavigationTestData.ApplicationId};action={ExternalNavigationTestData.BrowsableAction};S.source=blazor;i.highlight=7;B.preview=true;end";

		using var intent = WebKitWebViewClient.CreateIntentForExternalUri(new Uri(intentUri));

		Assert.Equal(ExternalNavigationTestData.BrowsableAction, intent.Action);
		Assert.Equal($"{ExternalNavigationTestData.BrowsableScheme}://open/item/42", intent.DataString);
		Assert.Equal(ExternalNavigationTestData.ApplicationId, intent.Package);
		Assert.Contains(Intent.CategoryBrowsable, intent.Categories);
		Assert.Null(intent.Component);
		Assert.Null(intent.Selector);
		Assert.Equal("blazor", intent.GetStringExtra("source"));
		Assert.Equal(7, intent.GetIntExtra("highlight", -1));
		Assert.True(intent.GetBooleanExtra("preview", false));
	}

	[Theory]
	[InlineData(
		"intent:#Intent;component=com.microsoft.maui.mauiblazorwebview.devicetests/com.microsoft.maui.mauiblazorwebview.devicetests.ExplicitIntentTestActivity;end")]
	[InlineData(
		"intent:#Intent;SEL;component=com.microsoft.maui.mauiblazorwebview.devicetests/com.microsoft.maui.mauiblazorwebview.devicetests.ExplicitIntentTestActivity;end")]
	[InlineData(
		"android-app://com.microsoft.maui.mauiblazorwebview.devicetests/#Intent;component=com.microsoft.maui.mauiblazorwebview.devicetests/com.microsoft.maui.mauiblazorwebview.devicetests.ExplicitIntentTestActivity;end")]
	public void StructuredIntentUriRemovesExplicitTargets(string uri)
	{
		using var intent = WebKitWebViewClient.CreateIntentForExternalUri(new Uri(uri));

		Assert.Contains(Intent.CategoryBrowsable, intent.Categories);
		Assert.Null(intent.Component);
		Assert.Null(intent.Selector);
	}

	[Fact]
	public void StructuredIntentUriRemovesUnsupportedActivityFlags()
	{
		const string intentUri = "intent:#Intent;launchFlags=0x10008000;end";

		using var intent = WebKitWebViewClient.CreateIntentForExternalUri(new Uri(intentUri));

		Assert.True(intent.Flags.HasFlag(ActivityFlags.NewTask));
		Assert.False(intent.Flags.HasFlag(ActivityFlags.ClearTask));
	}

	[Theory]
	[InlineData("intent")]
	[InlineData("android-app")]
	public async Task StructuredIntentUriWithExplicitComponentDoesNotLaunchActivity(string uriScheme)
	{
		var activityLaunch = ActivityLaunchMonitor<ExplicitIntentTestActivity>.PrepareForLaunch();
		var intentUri = uriScheme == "intent"
			? $"intent:#Intent;component={ExternalNavigationTestData.ApplicationId}/{ExternalNavigationTestData.ExplicitActivityName};S.test=value;end"
			: $"android-app://{ExternalNavigationTestData.ApplicationId}/#Intent;component={ExternalNavigationTestData.ApplicationId}/{ExternalNavigationTestData.ExplicitActivityName};S.test=value;end";

		try
		{
			await NavigateToExternalUriAsync(intentUri, uriScheme);

			var completedTask = await Task.WhenAny(activityLaunch, Task.Delay(ActivityNotLaunchedTimeout));
			Assert.NotSame(activityLaunch, completedTask);
		}
		finally
		{
			ActivityLaunchMonitor<ExplicitIntentTestActivity>.Reset();
		}
	}

	[Fact]
	public async Task IntentUriWithoutBrowsableHandlerDoesNotLaunchActivity()
	{
		var activityLaunch = ActivityLaunchMonitor<NonBrowsableIntentTestActivity>.PrepareForLaunch();
		var intentUri =
			$"intent://open/item#Intent;scheme={ExternalNavigationTestData.NonBrowsableScheme};package={ExternalNavigationTestData.ApplicationId};action={ExternalNavigationTestData.NonBrowsableAction};end";

		try
		{
			await NavigateToExternalUriAsync(intentUri);

			var completedTask = await Task.WhenAny(activityLaunch, Task.Delay(ActivityNotLaunchedTimeout));
			Assert.NotSame(activityLaunch, completedTask);
		}
		finally
		{
			ActivityLaunchMonitor<NonBrowsableIntentTestActivity>.Reset();
		}
	}

	[Fact]
	public async Task IntentUriWithBrowsableHandlerLaunchesActivity()
	{
		var activityLaunch = ActivityLaunchMonitor<BrowsableIntentTestActivity>.PrepareForLaunch();
		var intentUri =
			$"intent://open/item/42#Intent;scheme={ExternalNavigationTestData.BrowsableScheme};package={ExternalNavigationTestData.ApplicationId};action={ExternalNavigationTestData.BrowsableAction};S.source=blazor;i.highlight=7;B.preview=true;end";

		try
		{
			await NavigateToExternalUriAsync(intentUri);

			var activity = await activityLaunch.WaitAsync(ActivityLaunchTimeout);
			Assert.Equal(ExternalNavigationTestData.BrowsableAction, activity.Intent?.Action);
			Assert.Equal($"{ExternalNavigationTestData.BrowsableScheme}://open/item/42", activity.Intent?.DataString);
			Assert.Equal(ExternalNavigationTestData.ApplicationId, activity.Intent?.Package);
			Assert.Equal("blazor", activity.Intent?.GetStringExtra("source"));
			Assert.Equal(7, activity.Intent?.GetIntExtra("highlight", -1));
			Assert.True(activity.Intent?.GetBooleanExtra("preview", false));
		}
		finally
		{
			ActivityLaunchMonitor<BrowsableIntentTestActivity>.Reset();
		}
	}

	async Task NavigateToExternalUriAsync(string uri, string uriScheme = "intent")
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var navigationObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });
		bwv.UrlLoading += (_, args) =>
		{
			if (args.Url.Scheme.Equals(uriScheme, StringComparison.OrdinalIgnoreCase))
			{
				navigationObserved.TrySetResult();
			}
		};

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForDocumentReady(platformWebView);
			await WebViewHelpers.ExecuteScriptAsync(
				platformWebView,
				$"window.location.href = {JsonSerializer.Serialize(uri)};");
		});

		await navigationObserved.Task.WaitAsync(ActivityLaunchTimeout);
	}
}
