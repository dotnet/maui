using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issue
{
	public class Issue22075 : _IssuesUITest
	{
		public Issue22075(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Button rendering overflow issue when padding is set in StackLayout";

		[Test]
		[Category(UITestCategories.Layout)]
		public void CreateStackWithPadding()
		{
			App.WaitForElement("Button");
			VerifyScreenshot();
		}
	}
}
