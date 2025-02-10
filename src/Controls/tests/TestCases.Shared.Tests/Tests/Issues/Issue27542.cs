#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25742 : _IssuesUITest
	{
		public override string Issue => "iOS BackButton title does not update when set to an empty string or whitespace";

		public Issue25742(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void UpdatedBackButtonTitleForEmptyString()
		{
			App.WaitForElement("GotoPage2");
			App.Tap("GotoPage2");
			VerifyScreenshot();
		}
	}
}
#endif