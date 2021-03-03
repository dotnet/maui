using System;
using System.Collections.Generic;
using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using UIKit;

namespace Microsoft.Maui.TestUtils
{
	public abstract class BaseTestApplicationDelegate : UIApplicationDelegate, ITestEntryPoint
	{
		public static bool IsXHarnessRun(string[] args)
		{
			// usually means this is from xharness
			return args?.Length > 0 || Environment.GetEnvironmentVariable("NUNIT_AUTOEXIT")?.Length > 0;
		}

		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			Window = new UIWindow(UIScreen.MainScreen.Bounds)
			{
				RootViewController = new BaseTestViewController(this)
			};
			Window.MakeKeyAndVisible();

			return true;
		}

		// ITestEntryPoint

		public abstract IEnumerable<TestAssemblyInfo> GetTestAssemblies();

		public virtual void TerminateWithSuccess()
		{
			Console.WriteLine("Exiting test run with success...");

			var s = new ObjCRuntime.Selector("terminateWithSuccess");
			UIApplication.SharedApplication.PerformSelector(s, UIApplication.SharedApplication, 0);
		}

		public virtual TestRunner GetTestRunner(TestRunner testRunner, LogWriter logWriter)
		{
			return testRunner;
		}
	}
}