using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21980 : _IssuesUITest
	{
		public override string Issue => "IndicatorView with DataTemplate does not render correctly";
		
		public Issue21980(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.IndicatorView)]
		public void IndicatorViewShouldRenderCorrectly()
		{
			App.WaitForElement("button");
			App.Click("button");

			VerifyScreenshot();
		}
	}
}