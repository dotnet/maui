#nullable enable
using System;
using System.Collections.Generic;
using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using UIKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public abstract class MauiTestApplicationDelegate<TStartup> : MauiTestApplicationDelegate
		where TStartup : IStartup, new()
	{
		protected override IAppHost OnBuildAppHost()
		{
			var startup = new TStartup();

			return startup
				.CreateAppHostBuilder()
				.ConfigureUsing(startup)
				.Build();
		}
	}

	public abstract class MauiTestApplicationDelegate : UIApplicationDelegate
	{
		public static bool IsHeadlessRunner(string[] args)
		{
			// usually means this is from xharness
			return args?.Length > 0 || Environment.GetEnvironmentVariable("NUNIT_AUTOEXIT")?.Length > 0;
		}

		protected MauiTestApplicationDelegate()
		{
			Current = this;
		}

		public static MauiTestApplicationDelegate Current { get; private set; } = null!;

		public IServiceProvider Services { get; private set; } = null!;

		public TestOptions Options { get; private set; } = null!;

		public HeadlessRunnerOptions RunnerOptions { get; private set; } = null!;

		public override UIWindow? Window { get; set; }

		protected abstract IAppHost OnBuildAppHost();

		public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var host = OnBuildAppHost();

			Services = host.Services;
			Options = Services.GetRequiredService<TestOptions>();
			RunnerOptions = Services.GetRequiredService<HeadlessRunnerOptions>();

			return true;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Window = new UIWindow(UIScreen.MainScreen.Bounds)
			{
				RootViewController = new MauiTestViewController()
			};

			Window.MakeKeyAndVisible();

			return true;
		}
		
	}
}