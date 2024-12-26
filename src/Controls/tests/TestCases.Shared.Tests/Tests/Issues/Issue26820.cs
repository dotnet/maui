using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26820 : _IssuesUITest
	{

		public Issue26820(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView items height are not proper in initial loading with HasUnevenRows set as True";

		[Test]
		[Category(UITestCategories.ListView)]
		public void AnimationCancel()
		{
			App.WaitForElement("listView");
			VerifyScreenshot();

		}
	}
}
