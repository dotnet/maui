using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33382 : _IssuesUITest
	{
		public override string Issue => "[iOS] AppInfo.ShowSettingsUI only opens settings app";

		public Issue33382(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.AppInfo)]
		public void ShowSettingsUIShouldNotCrash()
		{
			// This test verifies that ShowSettingsUI doesn't crash
			// Manual verification is required to confirm it navigates to app settings
			// on iOS 26+
			
			App.WaitForElement("TestButton");
			App.Tap("TestButton");
			
			// Wait a moment for the API to execute
			Task.Delay(1000).Wait();
			
			// If we get here without crashing, the API worked
			// Note: We cannot automatically verify Settings app opened
			// or that it navigated to the correct page - that requires manual testing
		}
	}
}
