#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14801 : _IssuesUITest
	{
		public override string Issue => "NullReferenceException in UpdateLeftBarButtonItem";

		public Issue14801(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public async Task PopShellPageBeforeBackImageIsReady()
		{
			App.WaitForElement("goToChildPage");
			App.Tap("goToChildPage");

			// Look for `Shell.SetBackButtonBehavior` in
			// src/Controls/tests/TestCases.HostApp/Issues/Issue14801.cs
			App.WaitForElement("goBack");
			App.Tap("goBack");

			// Wait for main page to be visible
			App.WaitForElement("goToChildPage");

			// A crash may happen briefly, await for a while to make sure it doesn't happen
			await Task.Delay(1000);
		}
	}
}
#endif