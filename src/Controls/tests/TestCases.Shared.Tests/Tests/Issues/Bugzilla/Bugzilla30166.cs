using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla30166 : _IssuesUITest
	{
		public Bugzilla30166(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NavigationBar.BarBackgroundColor resets on Lollipop after popping modal page";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla30166Test()
		{
			App.WaitForElement("PushModal");
			App.Tap("PushModal");
			App.WaitForElement("GoBack");
			App.Tap("GoBack");
			VerifyScreenshot();
		}
	}
}