using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25887 : _IssuesUITest
	{
		public Issue25887(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ContentPresenter just rendering component string in .Net9";

		[Test]
		[Category(UITestCategories.RadioButton)]
		public void RadioButtonContentNotRendering()
		{
			App.WaitForElement("RadioButtonTemplate1");
			VerifyScreenshot();
		}
	}
}