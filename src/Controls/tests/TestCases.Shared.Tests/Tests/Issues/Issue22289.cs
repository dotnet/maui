using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22289 : _IssuesUITest
	{
		public override string Issue => "InputTransparent=\"true\" on a Layout breaks child controls on Android";
		public Issue22289(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.InputTransparent)]
		public void ButtonsShouldBeVisible()
		{
			App.WaitForElement("changeVisibilityButton");
			App.Click("changeVisibilityButton");

			App.WaitForElement("button1");
			App.WaitForElement("button2");
		}
	}
}