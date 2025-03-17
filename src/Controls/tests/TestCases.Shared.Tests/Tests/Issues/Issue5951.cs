using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5951 : _IssuesUITest
	{
		public Issue5951(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App Crashes On Shadow Effect's OnDetached On Button That's Never Visible";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void Issue5951Test()
		{
			App.Tap("Push page");
			App.WaitForElement("Push page");
			App.WaitForNoElement("Success");
		}
	}
}