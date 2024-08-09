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
		[FailsOnMac]
		public void Issue10222Test()
		{
			try
			{
				App.WaitForElement("PushModal");
				App.Tap("PushModal");
				App.WaitForElement("Back");
				App.Tap("Back");
				App.Screenshot("Navigation bar should be red");
			}
			finally
			{
				App.Back();
			}
		}
	}
}