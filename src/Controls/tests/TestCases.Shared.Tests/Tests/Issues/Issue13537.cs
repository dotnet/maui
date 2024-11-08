#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue13537 : _IssuesUITest
		{
		public Issue13537(TestDevice testDevice) : base(testDevice)
		{
		}

			public override string Issue => "ApplyQueryAttributes should Trigger for all navigations";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforPushAndPopButton()
		{
			App.WaitForElement("TestLabel");
			App.Tap("PushAsyncButton");
			App.WaitForElement("TestLabel3");
			App.Tap("PopAsyncButton");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforPushAndBackButton()
		{
			App.WaitForElement("TestLabel");
			App.Tap("PushAsyncButton");
			App.WaitForElement("TestLabel3");
#if IOS
			App.Back();
#elif ANDROID
          App.Tap("Navigate up");
#else
			App.Tap("NavigationViewBackButton");
#endif
			VerifyScreenshot();
		}

#if !WINDOWS // Currently TabBar AutomationId is not works in Windows
		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributeShouldTriggerforTab()
		{
			App.Tap("favorite");
			App.WaitForElement("TestLabel1");
			App.Tap("GoToAsyncButton");
			App.WaitForElement("TestLabel3");
			VerifyScreenshot();
		}

#endif
	}

}
#endif