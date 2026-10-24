using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class _GalleryUITest : UITest
	{
		public _GalleryUITest(TestDevice device) : base(device) { }

		public override IConfig GetTestConfig()
		{
			var config = base.GetTestConfig();

			if (UseDirectLaunch)
				config.SetTestConfigurationArg("test", GalleryPageName);

			return config;
		}

		public override void LaunchAppWithTest()
		{
			const int maxRetries = 3;
			const int delayBetweenRetries = 2000; // 2 seconds

			for (int attempt = 1; attempt <= maxRetries; attempt++)
			{
				try
				{
					if (UseDirectLaunch)
						App.LaunchApp(GalleryPageName, ResetAfterEachTest);
					else
						App.LaunchApp();
					return; // Success, exit retry loop
				}
				catch (Exception)
				{
					if (attempt == maxRetries)
					{
						TestContext.WriteLine($"Failed to launch app after {maxRetries} attempts.");
						throw; // Final attempt failed, rethrow the original exception
					}

					// Wait before retrying
					System.Threading.Thread.Sleep(delayBetweenRetries);
				}
			}
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			if (!UseDirectLaunch)
				App.NavigateToGallery(GalleryPageName);

			if (GallerySubPageButton is string subPageButton)
			{
				App.WaitForElementTillPageNavigationSettled(subPageButton);
				App.Tap(subPageButton);
			}
		}

		protected override void TryToResetTestState()
		{
			if (!UseDirectLaunch)
				base.TryToResetTestState();
		}

		public abstract string GalleryPageName { get; }

		protected virtual string? GallerySubPageButton => null;
	}
}