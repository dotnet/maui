#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Services;

[Category(TestCategory.Application)]
public class EssentialsDIBridgeTests : TestBase
{
	[Fact]
	public void DIRegisteredActivityStateManagerIsInitialized()
	{
		var original = ActivityStateManager.Default;
		var replacement = new StubActivityStateManager();

		try
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IActivityStateManager>(replacement);

			using var app = builder.Build();
			_ = app.Services.GetRequiredService<ILifecycleEventService>();

			Assert.Same(replacement, ActivityStateManager.Default);
			Assert.Equal(1, replacement.ApplicationInitializationCount);
		}
		finally
		{
			var builder = MauiApp.CreateBuilder();
			builder.Services.AddSingleton<IActivityStateManager>(original);

			using var app = builder.Build();
		}

		Assert.Same(original, ActivityStateManager.Default);
	}

	[Fact]
	public void AppActionsBridgeRequiresAndroidLifecycleContract()
	{
		var original = AppActions.Current;

		try
		{
			using (var app = BuildApp<IAppActions>(new StubAppActions()))
				Assert.Same(original, AppActions.Current);

			var replacement = new StubPlatformAppActions();
			using (var app = BuildApp<IAppActions>(replacement))
				Assert.Same(replacement, AppActions.Current);
		}
		finally
		{
			using var app = BuildApp<IAppActions>(original);
		}

		Assert.Same(original, AppActions.Current);
	}

	[Fact]
	public void WebAuthenticatorBridgeRequiresAndroidLifecycleContract()
	{
		var original = WebAuthenticator.Default;

		try
		{
			using (var app = BuildApp<IWebAuthenticator>(new StubWebAuthenticator()))
				Assert.Same(original, WebAuthenticator.Default);

			var replacement = new StubPlatformWebAuthenticator();
			using (var app = BuildApp<IWebAuthenticator>(replacement))
				Assert.Same(replacement, WebAuthenticator.Default);
		}
		finally
		{
			using var app = BuildApp<IWebAuthenticator>(original);
		}

		Assert.Same(original, WebAuthenticator.Default);
	}

	static MauiApp BuildApp<TService>(TService service)
		where TService : class
	{
		var builder = MauiApp.CreateBuilder();
		builder.Services.AddSingleton(service);
		return builder.Build();
	}

	sealed class StubActivityStateManager : IActivityStateManager
	{
		public int ApplicationInitializationCount { get; private set; }

		public event EventHandler<ActivityStateChangedEventArgs>? ActivityStateChanged { add { } remove { } }

		public void Init(global::Android.App.Application application) =>
			ApplicationInitializationCount++;

		public void Init(global::Android.App.Activity activity, global::Android.OS.Bundle? bundle) { }

		public global::Android.App.Activity? GetCurrentActivity() => null;

		public Task<global::Android.App.Activity> WaitForActivityAsync(CancellationToken cancelToken = default) =>
			Task.FromResult<global::Android.App.Activity>(null!);
	}

	class StubAppActions : IAppActions
	{
		public bool IsSupported => true;

		public event EventHandler<AppActionEventArgs>? AppActionActivated { add { } remove { } }

		public Task<IEnumerable<AppAction>> GetAsync() =>
			Task.FromResult<IEnumerable<AppAction>>(Array.Empty<AppAction>());

		public Task SetAsync(IEnumerable<AppAction> actions) => Task.CompletedTask;
	}

	sealed class StubPlatformAppActions : StubAppActions, IPlatformAppActions
	{
		public void OnNewIntent(global::Android.Content.Intent? intent) { }

		public void OnResume(global::Android.Content.Intent? intent) { }
	}

	class StubWebAuthenticator : IWebAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
			Task.FromException<WebAuthenticatorResult>(new NotSupportedException());

		public Task<WebAuthenticatorResult> AuthenticateAsync(
			WebAuthenticatorOptions webAuthenticatorOptions,
			CancellationToken cancellationToken) =>
			Task.FromException<WebAuthenticatorResult>(new NotSupportedException());
	}

	sealed class StubPlatformWebAuthenticator : StubWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
		public bool OnResumeCallback(global::Android.Content.Intent intent) => true;
	}
}
