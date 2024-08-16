using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21643 : _IssuesUITest
	{
		public Issue21643(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Border has an unexpected background animation";

		[Test]
		[Category(UITestCategories.Border)]
		public void Issue21643Test()
		{
			App.WaitForElement("border");
			App.Tap("button");
			VerifyScreenshot();
		}
	}
}
