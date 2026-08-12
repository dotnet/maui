using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue37180 : _IssuesUITest
	{
		public Issue37180(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Label background remains visible after being set to null";

		[Test]
		[Category(UITestCategories.Label)]
		public void LabelBackgroundClearsWhenSetToNull()
		{
			App.WaitForElement("BackgroundLabel");
			VerifyScreenshot("Issue37180_RedBackground");
			App.Tap("SetBackgroundButton");
			VerifyScreenshot("NoBackground");
		}
	}
}
