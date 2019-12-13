using System;
using System.Reflection;
using NUnit;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Xamarin.Forms.Controls.Tests
{
	public class PlatformTestRunner
	{
		readonly ITestListener _testListener = new ControlGalleryTestListener();

		public void Run(ITestFilter testFilter = null)
		{
			testFilter = testFilter ?? TestFilter.Empty;

			// "controls" is the cross-platform test assembly
#if NETSTANDARD2_0
			var controls = Assembly.GetExecutingAssembly();
#else
			var controls = typeof(PlatformTestRunner).GetTypeInfo().Assembly;
#endif

			var platformTestSettings = DependencyService.Resolve<IPlatformTestSettings>();
			
			// "platform" is the native assembly (ControGallery.iOS, ControlGallery.Android, etc.)
			Assembly platform = platformTestSettings.Assembly;
			var testRunSettings = platformTestSettings.TestRunSettings;

			var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

			bool runOnMainThread = false;
			if (testRunSettings.TryGetValue(FrameworkPackageSettings.RunOnMainThread, out object runOnMainSetting))
			{
				runOnMainThread = (bool)runOnMainSetting;
			}

			try
			{
				if (runOnMainThread)
				{
					// FrameworkPackageSettings.RunOnMainThread will force all the tests to run sequentially on the thread
					// they are started on; we have to do this for iOS to avoid cross-thread exceptions when updating
					// renderer properties. It's a less nice runner experience, because we don't get progress updates
					// while it runs, but that's life. Anyway, we push the test runs onto the main thread and wait.

					Device.BeginInvokeOnMainThread(() =>
					{
						runner.Load(controls, testRunSettings);
						runner.Run(_testListener, testFilter);

						runner.Load(platform, testRunSettings);
						runner.Run(_testListener, testFilter);
					});
				}
				else
				{
					// So far, Android lets us get away with running tests asynchronously, so we get
					// progress updates as they run. This should be our default until we run into cross-thread
					// or "not on the UI thread" issues with a platform, at which point we need to set RunOnMainThread
					// like we do for iOS

					runner.Load(controls, testRunSettings);
					runner.Run(_testListener, testFilter);

					runner.Load(platform, testRunSettings);
					runner.Run(_testListener, testFilter);
				}
			}
			catch (Exception ex) 
			{
				MessagingCenter.Send(ex, "TestRunnerError");
			}
		}
	}
}
