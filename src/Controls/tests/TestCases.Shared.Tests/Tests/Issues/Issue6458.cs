using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6458 : _IssuesUITest
	{
		public Issue6458(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Fix load TitleIcon on non app compact";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue6458Test()
		{
			App.WaitForElement("IssuePageLabel");
			VerifyScreenshot();
		}
	}
}
