using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18235 : _IssuesUITest
	{
		public Issue18235(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[7.096] Setting .NET MAUI Button.Text to String.Empty inside a Clicked event handler causes previously set buttons to revert to previous values";

		[Test]
		[Category(UITestCategories.Button)]
		public void VerifyButtonText()
		{
			App.WaitForElement("SecondaryActionButton");
			App.Tap("PrimaryActionButton");
			App.Tap("SecondaryActionButton");
			VerifyScreenshot();
		}
	}
}