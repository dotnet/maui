#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue8788 : _IssuesUITest
	{
		public Issue8788(TestDevice device) : base(device) { }

		public override string Issue => "Shell Tab is still visible after set Tab.IsVisible to false";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellTabItemsShouldUpdateForDynamicChangesInVisibility()
		{
			App.SwipeLeftToRight();
			App.WaitForElement("FirstButton");
			App.Tap("FirstButton");
			VerifyScreenshot();
		}
	}
}
#endif