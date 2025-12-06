using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28722 : _IssuesUITest
	{
		public Issue28722(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "IsEnabled does not work in BackButtonBehavior";

		[Test]
		[Category(UITestCategories.Shell)]
		public void IsEnabledShouldWorkInBackButtonBehavior()
		{
			App.WaitForElement("Click");
			App.Click("Click");
			App.WaitForElement("HelloLabel");
			App.Click("Click");
			App.WaitForElement("HelloLabel");
		}
	}
}