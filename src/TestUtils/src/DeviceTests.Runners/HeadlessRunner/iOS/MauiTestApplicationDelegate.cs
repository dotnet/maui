#nullable enable
using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.DotNet.XHarness.iOS.Shared.Execution;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
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

		protected abstract MauiApp CreateMauiApp();

		public override bool WillFinishLaunching(UIApplication application, NSDictionary? launchOptions)
		{
			Runtime.MarshalManagedException += (object sender, MarshalManagedExceptionEventArgs args) =>
			{
				Console.WriteLine("Marshaling managed exception");
				Console.WriteLine("    Exception: {0}", args.Exception);
				Console.WriteLine("    Mode: {0}", args.ExceptionMode);

			};

			Runtime.MarshalObjectiveCException += (object sender, MarshalObjectiveCExceptionEventArgs args) =>
			{
				Console.WriteLine("Marshaling Objective-C exception");
				Console.WriteLine("    Exception: {0}", args.Exception);
				Console.WriteLine("    Mode: {0}", args.ExceptionMode);
			};

			var mauiApp = CreateMauiApp();
			Services = mauiApp.Services;

			Environment.SetEnvironmentVariable("headlessrunner", "true");

			Options = Services.GetRequiredService<TestOptions>();
			RunnerOptions = Services.GetRequiredService<HeadlessRunnerOptions>();

			return true;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
		{
			var tcs = new TaskCompletionSource();

			Window = new UIWindow(UIScreen.MainScreen.Bounds)
			{
				RootViewController = new MauiTestViewController(tcs.Task)
			};

			Window.MakeKeyAndVisible();

			tcs.TrySetResult();

			return true;
		}

	}
}