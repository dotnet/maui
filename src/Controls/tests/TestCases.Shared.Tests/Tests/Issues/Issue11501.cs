#if ANDROID || IOS//The test fails on Windows and MacCatalyst because the BackgroundApp and ForegroundApp method, which is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
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

		[TestCase("SwapMainPage", Category = UITestCategories.Navigation)]
		[TestCase("SwapFlyoutPage", Category = UITestCategories.FlyoutPage)]
		[TestCase("SwapTabbedPage", Category = UITestCategories.TabbedPage)]
		[TestCase("RemoveAddTabs", Category = UITestCategories.TabbedPage)]
		public void MakingFragmentRelatedChangesWhileAppIsBackgroundedFails(string scenario)
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
	}
}
#endif
