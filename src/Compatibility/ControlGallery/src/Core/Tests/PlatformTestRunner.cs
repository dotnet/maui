using System;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microsoft.Maui.Controls.ControlGallery.Tests
{
	public class PlatformTestRunner
	{
		readonly ITestListener _testListener = new ControlGalleryTestListener();

		public void Run(ITestFilter testFilter = null)
		{
			testFilter = testFilter ?? TestFilter.Empty;

			// "controls" is the cross-platform test assembly
			var controls = Assembly.GetExecutingAssembly();

			var platformTestSettings = DependencyService.Resolve<IPlatformTestSettings>();

			// "platform" is the native assembly (ControGallery.iOS, ControlGallery.Android, etc.)
			Assembly platform = platformTestSettings.Assembly;

			// The TestRunSettings gives us a way to pass other parameters to the runner.
			// We're not actually using them at the moment, but we might as well leave this here
			// in case we need it in the future.
			var testRunSettings = platformTestSettings.TestRunSettings;

			var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

			try
			{
				runner.Load(controls, testRunSettings);
				runner.Run(_testListener, testFilter);

				runner.Load(platform, testRunSettings);
				runner.Run(_testListener, testFilter);
			}
			catch (Exception ex)
			{
				MessagingCenter.Send(ex, "TestRunnerError");
			}
		}
	}
}
