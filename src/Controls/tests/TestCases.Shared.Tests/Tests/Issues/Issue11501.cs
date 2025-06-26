﻿#if ANDROID || IOS//The test fails on Windows and MacCatalyst because the BackgroundApp and ForegroundApp method, which is only supported on mobile platforms iOS and Android.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11501 : _IssuesUITest
	{
		public Issue11501(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Making Fragment Changes While App is Backgrounded Fails";

		[Theory]
		[InlineData("SwapMainPage", Category = UITestCategories.Navigation)]
		[Theory]
		[InlineData("SwapFlyoutPage", Category = UITestCategories.FlyoutPage)]
		[Theory]
		[InlineData("SwapTabbedPage", Category = UITestCategories.TabbedPage)]
		[Theory]
		[InlineData("RemoveAddTabs", Category = UITestCategories.TabbedPage)]
		public void MakingFragmentRelatedChangesWhileAppIsBackgroundedFails(string scenario)
		{
			try
			{
				App.WaitForElement(scenario);
				App.Tap(scenario);
				App.WaitForElement("BackgroundMe");
				App.BackgroundApp();
				App.WaitForNoElement("BackgroundMe");
				App.ForegroundApp();
				App.WaitForElement("Restore");
				App.Tap("Restore");
			}
			catch
			{
				SaveUIDiagnosticInfo();

				// Just in case these tests leave the app in an unreliable state
				App.ResetApp();
				FixtureSetup();
				throw;
			}
		}
	}
}
#endif
