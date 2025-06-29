using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21488 : _IssuesUITest
	{
		public Issue21488(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Button text doesn't update when CharacterSpacing is applied";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonTextShouldUpdate()
		{
			App.WaitForElement("Entry");
			App.EnterText("Entry", "Hello, Maui!");
			VerifyScreenshot();
		}
	}
}