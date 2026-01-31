using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18668 : _IssuesUITest
	{
		public override string Issue => "Visual state change for disabled RadioButton";

		public Issue18668(TestDevice device) : base(device){ }

		[Test]
		[Category(UITestCategories.RadioButton)]
		public void TestIssue18668()
		{
			App.WaitForElement("button");
			App.Click("button");

			// The test passes if the radio button is visually disabled
			VerifyScreenshot();
		}
	}
}