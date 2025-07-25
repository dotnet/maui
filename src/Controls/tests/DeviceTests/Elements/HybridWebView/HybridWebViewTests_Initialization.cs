#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
public partial class HybridWebViewTests_Initialization : HybridWebViewTestsBase
{
	const string UserAgent = "HybridWebViewTests User Agent";
	const string ProfileName = "HybridWebViewTests Test Profile";

	protected async Task RunTest(Action<HybridWebView> setup, Action<HybridWebViewHandler, HybridWebView>? test = null)
	{
		var hybridWebView = new HybridWebView
		{
			WidthRequest = 100,
			HeightRequest = 100,

			HybridRoot = "HybridTestRoot",
			DefaultFile = "index.html",
		};

		setup(hybridWebView);

		await RunTest(hybridWebView, async (handler, view) =>
		{
			// await just so the HybridWebView can be created and the initialization events can be fired
			await Task.Delay(1);

			// Run the actual test
			test?.Invoke(handler, hybridWebView);
		});
	}

	[Fact]
	public async Task InitializingEventIsRaised()
	{
		var calledCount = 0;

		await RunTest(
			hybridWebView =>
			{
				hybridWebView.WebViewInitializing += (s, e) =>
				{
					calledCount++;

					Assert.NotNull(e.PlatformArgs);

#if IOS || MACCATALYST
					Assert.NotNull(e.PlatformArgs.Configuration);
#elif ANDROID
					Assert.NotNull(e.PlatformArgs.Settings);
#elif WINDOWS
#endif
				};
			});

		Assert.Equal(1, calledCount);
	}

	[Fact]
	public async Task InitializedEventIsRaised()
	{
		var calledCount = 0;

		await RunTest(
			hybridWebView =>
			{
				hybridWebView.WebViewInitialized += (s, e) =>
				{
					calledCount++;

					Assert.NotNull(e.PlatformArgs);
					Assert.NotNull(e.PlatformArgs.Sender);
#if IOS || MACCATALYST
					Assert.NotNull(e.PlatformArgs.Configuration);
#elif ANDROID || WINDOWS
					Assert.NotNull(e.PlatformArgs.Settings);
#endif
				};
			});

		Assert.Equal(1, calledCount);
	}

	[Fact]
	public async Task InitializingEventIsRaisedAndPropertiesSetAreApplied()
	{
		var calledCount = 0;

		await RunTest(
			hybridWebView =>
			{
				hybridWebView.WebViewInitializing += (s, e) =>
				{
					calledCount++;

					Assert.NotNull(e.PlatformArgs);

#if IOS || MACCATALYST
					Assert.NotNull(e.PlatformArgs.Configuration);
					e.PlatformArgs.Configuration.ApplicationNameForUserAgent = UserAgent;
#elif ANDROID
					Assert.NotNull(e.PlatformArgs.Settings);
					e.PlatformArgs.Settings.UserAgentString = UserAgent;
#elif WINDOWS
					e.PlatformArgs.ProfileName = ProfileName;
#endif
				};
			},
			(handler, view) =>
			{
#if IOS || MACCATALYST
				var actual = handler.PlatformView.Configuration.ApplicationNameForUserAgent;
				Assert.Equal(UserAgent, actual);
#elif ANDROID
				var actual = handler.PlatformView.Settings.UserAgentString;
				Assert.Equal(UserAgent, actual);
#elif WINDOWS
				var actual = handler.PlatformView.CoreWebView2.Profile.ProfileName;
				Assert.Equal(ProfileName, actual);
#endif
			});

		Assert.Equal(1, calledCount);
	}

	[Fact]
	public Task CanSetUserAgentUsingProperties() =>
		RunTest(
			hybridWebView =>
			{
				hybridWebView.WebViewInitializing += (s, e) =>
				{
					Assert.NotNull(e.PlatformArgs);

#if IOS || MACCATALYST
					e.PlatformArgs.Configuration.ApplicationNameForUserAgent = UserAgent;
#elif ANDROID
					e.PlatformArgs.Settings.UserAgentString = UserAgent;
#endif
				};
				hybridWebView.WebViewInitialized += (s, e) =>
				{
					Assert.NotNull(e.PlatformArgs);

#if WINDOWS
					e.PlatformArgs.Settings.UserAgent = UserAgent;
#endif
				};
			},
			(handler, view) =>
			{
#if IOS || MACCATALYST
				var actual = handler.PlatformView.Configuration.ApplicationNameForUserAgent;
#elif ANDROID
				var actual = handler.PlatformView.Settings.UserAgentString;
#elif WINDOWS
				var actual = handler.PlatformView.CoreWebView2.Settings.UserAgent;
#endif
				Assert.Equal(UserAgent, actual);
			});

	[Fact]
	public Task CanSetUserAgentUsingInitializingEvent() =>
		RunTest(
			hybridWebView =>
			{
				hybridWebView.WebViewInitializing += (s, e) =>
				{
					Assert.NotNull(e.PlatformArgs);

#if IOS || MACCATALYST
					e.PlatformArgs.Configuration.ApplicationNameForUserAgent = UserAgent;
#elif ANDROID
					e.PlatformArgs.Settings.UserAgentString = UserAgent;
#elif WINDOWS
					e.PlatformArgs.EnvironmentOptions.AdditionalBrowserArguments = $"--user-agent=\"{UserAgent}\"";
#endif
				};
			},
			(handler, view) =>
			{
#if IOS || MACCATALYST
				var actual = handler.PlatformView.Configuration.ApplicationNameForUserAgent;
#elif ANDROID
				var actual = handler.PlatformView.Settings.UserAgentString;
#elif WINDOWS
				var actual = handler.PlatformView.CoreWebView2.Settings.UserAgent;
#endif
				Assert.Equal(UserAgent, actual);
			});
}
