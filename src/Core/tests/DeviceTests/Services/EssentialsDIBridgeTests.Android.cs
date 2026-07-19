#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
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
			builder.Services.AddSingleton(original);

			using var app = builder.Build();
		}
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
}
